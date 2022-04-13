using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using DotNetCoreDecorators;
using Microsoft.Extensions.Logging;
using MyJetWallet.Sdk.Service;
using MyJetWallet.Sdk.ServiceBus;
using MyNoSqlServer.Abstractions;
using Newtonsoft.Json;
using Service.AssetsDictionary.Client;
using Service.ChangeBalanceGateway.Grpc;
using Service.ChangeBalanceGateway.Grpc.Models;
using Service.IndexPrices.Client;
using Service.InterestManager.Postrges;
using Service.IntrestManager.Domain.Models;
using Service.IntrestManager.Domain.Models.NoSql;

namespace Service.IntrestManager.Engines
{
    public class InterestProcessingEngine
    {
        private readonly ILogger<InterestProcessingEngine> _logger;
        private readonly DatabaseContextFactory _databaseContextFactory;
        private readonly ISpotChangeBalanceService _spotChangeBalanceService;
        private readonly IServiceBusPublisher<PaidInterestRateMessage> _paidInterestPublisher;
        private readonly IServiceBusPublisher<FailedInterestRateMessage> _failedInterestPublisher;
        private readonly IServiceBusPublisher<InterestProcessingResult> _processingResultPublisher;
        private readonly IAssetsDictionaryClient _assetsClient;
        private readonly IMyNoSqlServerDataReader<InterestManagerConfigNoSql> _myNoSqlServerDataReader;
        private readonly IIndexPricesClient _indexPrices;
        public InterestProcessingEngine(ILogger<InterestProcessingEngine> logger,
            DatabaseContextFactory databaseContextFactory,
            ISpotChangeBalanceService spotChangeBalanceService,
            IServiceBusPublisher<PaidInterestRateMessage> paidInterestPublisher,
            IServiceBusPublisher<FailedInterestRateMessage> failedInterestPublisher,
            IServiceBusPublisher<InterestProcessingResult> processingResultPublisher,
            IAssetsDictionaryClient assetsClient, 
            IMyNoSqlServerDataReader<InterestManagerConfigNoSql> myNoSqlServerDataReader, 
            IIndexPricesClient indexPrices)
        {
            _logger = logger;
            _databaseContextFactory = databaseContextFactory;
            _spotChangeBalanceService = spotChangeBalanceService;
            _paidInterestPublisher = paidInterestPublisher;
            _failedInterestPublisher = failedInterestPublisher;
            _processingResultPublisher = processingResultPublisher;
            _assetsClient = assetsClient;
            _myNoSqlServerDataReader = myNoSqlServerDataReader;
            _indexPrices = indexPrices;
        }

        public async Task Execute()
        {
            using var activity = MyTelemetry.StartActivity(nameof(InterestProcessingEngine));
            await ProcessInterest();
        }

        private async Task ProcessInterest()
        {
            var serviceConfig = _myNoSqlServerDataReader.Get().FirstOrDefault();

            if (string.IsNullOrWhiteSpace(serviceConfig?.Config.ServiceBroker) ||
                string.IsNullOrWhiteSpace(serviceConfig?.Config.ServiceClient) ||
                string.IsNullOrWhiteSpace(serviceConfig?.Config.ServiceWallet))
            {
                _logger.LogError("Cannot process interest. Service config IS EMPTY !!!!");
                return;
            }

            var iterationCount = 0;

            var assets = _assetsClient.GetAllAssets();
            if (!assets.Any())
            {
                await Task.Delay(5000);
                assets = _assetsClient.GetAllAssets();
            }
            
            var processingResult = new InterestProcessingResult();

            while (true)
            {
                await Task.Delay(1);

                var complitedInteredRates = new List<PaidInterestRateMessage>();
                var gatewayTaskList = new List<Task>();
                iterationCount++;
                
                using var activity = MyTelemetry.StartActivity($"ProcessInterest iteration {iterationCount}");
                
                var sv = new Stopwatch();
                sv.Start();
                
                await using var loopCtx = _databaseContextFactory.Create();
                var paidToProcess = loopCtx.GetTop100PaidToProcess();
                
                if (!paidToProcess.Any())
                {
                    break;
                };

                _logger.LogInformation("InterestProcessingJob find {paidCount} new records to process at {dateJson}.",
                    paidToProcess.Count, DateTime.UtcNow.ToString(CultureInfo.InvariantCulture));

                foreach (var interestRatePaid in paidToProcess)
                {
                    interestRatePaid.Iteration++;
                    
                    if (interestRatePaid.Amount == 0)
                    {
                        _logger.LogInformation("Skipped walletId: {walletid} and asset: {assetSymbol} with amount {amountJson}",
                            interestRatePaid.WalletId, interestRatePaid.Symbol, interestRatePaid.Amount);
                        
                        interestRatePaid.State = PaidState.Failed;
                        interestRatePaid.ErrorMessage =
                            $"Skipped walletId: {interestRatePaid.WalletId} and asset: {interestRatePaid.Symbol} with amount {interestRatePaid.Amount}";
                        continue;
                    }
                    var asset = assets.FirstOrDefault(e => e.Symbol == interestRatePaid.Symbol);
                    if (asset == null)
                    {
                        _logger.LogInformation("Skipped walletId: {walletId} and asset: {asset}. Cannot find asset in asset dictionary.",
                            interestRatePaid.WalletId, interestRatePaid.Symbol);
                        
                        interestRatePaid.State = PaidState.Failed;
                        interestRatePaid.ErrorMessage =
                            $"Skipped walletId: {interestRatePaid.WalletId} and asset: {interestRatePaid.Symbol}. Cannot find asset in asset dictionary.";
                        continue;
                    }
                    gatewayTaskList.Add(PushToGateway(serviceConfig.Config, interestRatePaid, complitedInteredRates, asset.Accuracy));
                }
                await Task.WhenAll(gatewayTaskList);
                await loopCtx.SaveChangesAsync();
                await _paidInterestPublisher.PublishAsync(complitedInteredRates);
                var failedInterestRates = paidToProcess
                    .Where(p => p.State == PaidState.Failed)
                    .Select(f => new FailedInterestRateMessage
                    {
                        Date = f.Date,
                        ErrorMessage = f.ErrorMessage,
                        WalletId = f.WalletId
                    })
                    .ToList();
                await _failedInterestPublisher.PublishAsync(failedInterestRates);
                processingResult.FailedCount += failedInterestRates.Count;
                processingResult.CompletedCount += complitedInteredRates.Count;

                sv.Stop();
                _logger.LogInformation("Iteration number: {iterationNumber}. InterestProcessingJob finish process {paidCount} records. Iteration time: {delay}", 
                    iterationCount, paidToProcess.Count, sv.Elapsed.ToString());
            }
            
            await using var ctx = _databaseContextFactory.Create();
            var history = ctx.GetLastPaidHistory();
            processingResult.TotalPaidAmountInUsd = history.TotalPaidInUsd;
            await _processingResultPublisher.PublishAsync(processingResult);
        }

        private async Task PushToGateway(InterestManagerConfig serviceConfig,
            InterestRatePaid interestRatePaid, List<PaidInterestRateMessage> messages, int accuracy)
        {
            var roundedAmount = Math.Round(interestRatePaid.Amount, accuracy, MidpointRounding.ToZero);
            if (roundedAmount == 0m)
            {
                _logger.LogInformation("Skipped walletId: {walletId} and asset: {asset}. Amount {amount} is too low.",
                    interestRatePaid.WalletId, interestRatePaid.Symbol, interestRatePaid.Amount);
                
                interestRatePaid.State = PaidState.TooLowAmount;
                interestRatePaid.ErrorMessage = $"Amount {interestRatePaid.Amount} for asset {interestRatePaid.Symbol} and wallet {interestRatePaid.WalletId} is too low.";
                return;
            }
            var request = new PayInterestRateRequest()
            {
                TransactionId = interestRatePaid.TransactionId,
                ClientId = serviceConfig.ServiceClient,
                FromWalletId = serviceConfig.ServiceWallet,
                ToWalletId = interestRatePaid.WalletId,
                Amount = roundedAmount,
                AssetSymbol = interestRatePaid.Symbol,
                Comment = "Paid interest rate",
                BrokerId = serviceConfig.ServiceBroker,
                RequestSource = nameof(InterestProcessingEngine)
            };
            _logger.LogInformation("PushToGateway push message : {messageJson}", JsonConvert.SerializeObject(request));
            
            var processResponse = await _spotChangeBalanceService.PayInterestRateAsync(request);
            if (processResponse.Result)
            {
                interestRatePaid.State = PaidState.Completed;
                interestRatePaid.Amount = roundedAmount;
                interestRatePaid.ErrorMessage = string.Empty;
                interestRatePaid.DatePaid = DateTime.UtcNow;
                interestRatePaid.IndexPrice = _indexPrices.GetIndexPriceByAssetAsync(interestRatePaid.Symbol).UsdPrice;
                
                lock (messages)
                {
                    messages.Add(new PaidInterestRateMessage()
                    {
                        BrokerId = serviceConfig.ServiceBroker,
                        ClientId = serviceConfig.ServiceClient,
                        TransactionId = interestRatePaid.TransactionId,
                        WalletId = interestRatePaid.WalletId,
                        Symbol = interestRatePaid.Symbol,
                        Date = DateTime.UtcNow,
                        Amount = roundedAmount
                    });
                }
            }
            else
            {
                interestRatePaid.State = PaidState.Failed;
                interestRatePaid.ErrorMessage = $"{processResponse.ErrorCode}: {processResponse.ErrorMessage}";
            }
        }
    }
}