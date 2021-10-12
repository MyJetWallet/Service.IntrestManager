using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using DotNetCoreDecorators;
using Microsoft.Extensions.Logging;
using MyJetWallet.Sdk.Service;
using Service.ChangeBalanceGateway.Grpc;
using Service.ChangeBalanceGateway.Grpc.Models;
using Service.InterestManager.Postrges;
using Service.IntrestManager.Domain.Models;
using Service.IntrestManager.Grpc;

namespace Service.IntrestManager.Engines
{
    public class InterestProcessingEngine
    {
        private readonly ILogger<InterestProcessingEngine> _logger;
        private readonly DatabaseContextFactory _databaseContextFactory;
        private readonly ISpotChangeBalanceService _spotChangeBalanceService;
        private readonly IInterestManagerConfigService _interestManagerConfigService;
        private readonly IPublisher<PaidInterestRateMessage> _publisher;

        public InterestProcessingEngine(ILogger<InterestProcessingEngine> logger,
            DatabaseContextFactory databaseContextFactory,
            ISpotChangeBalanceService spotChangeBalanceService,
            IInterestManagerConfigService interestManagerConfigService, 
            IPublisher<PaidInterestRateMessage> publisher)
        {
            _logger = logger;
            _databaseContextFactory = databaseContextFactory;
            _spotChangeBalanceService = spotChangeBalanceService;
            _interestManagerConfigService = interestManagerConfigService;
            _publisher = publisher;
        }

        public async Task Execute()
        {
            using var activity = MyTelemetry.StartActivity(nameof(InterestProcessingEngine));
            await ProcessInterest();
        }

        private async Task ProcessInterest()
        {
            var serviceConfig = await _interestManagerConfigService.GetInterestManagerConfigAsync();

            if (!serviceConfig.Success ||
                serviceConfig.Config == null ||
                string.IsNullOrWhiteSpace(serviceConfig.Config.ServiceBroker) ||
                string.IsNullOrWhiteSpace(serviceConfig.Config.ServiceClient) ||
                string.IsNullOrWhiteSpace(serviceConfig.Config.ServiceWallet))
            {
                _logger.LogError("Cannot process interest. Service wallet IS EMPTY !!!!");
                return;
            }

            var iterationCount = 0;

            while (true)
            {
                await Task.Delay(1);
                
                var serviceBusTaskList = new List<Task>();
                var gatewayTaskList = new List<Task>();
                iterationCount++;
                
                using var activity = MyTelemetry.StartActivity($"ProcessInterest iteration {iterationCount}");
                
                var sv = new Stopwatch();
                sv.Start();
                
                await using var ctx = _databaseContextFactory.Create();
                var paidToProcess = ctx.GetNewPaidCollection();
                
                if (!paidToProcess.Any())
                {
                    break;
                };

                _logger.LogInformation("InterestProcessingJob find {paidCount} new records to process at {dateJson}.",
                    paidToProcess.Count, DateTime.UtcNow.ToString(CultureInfo.InvariantCulture));

                foreach (var interestRatePaid in paidToProcess)
                {
                    if (interestRatePaid.Amount == 0)
                    {
                        _logger.LogInformation("Skipped walletId: {walletid} and asset: {assetSymbol} with amount {amountJson}",
                            interestRatePaid.WalletId, interestRatePaid.Symbol, interestRatePaid.Amount);
                        
                        interestRatePaid.State = PaidState.Failed;
                        interestRatePaid.ErrorMessage =
                            $"Skipped walletId: {interestRatePaid.WalletId} and asset: {interestRatePaid.Symbol} with amount {interestRatePaid.Amount}";
                        
                        continue;
                    }
                    gatewayTaskList.Add(PushToGateway(serviceConfig.Config, interestRatePaid, serviceBusTaskList));
                }
                await Task.WhenAll(gatewayTaskList);
                await ctx.SaveChangesAsync();
                await Task.WhenAll(serviceBusTaskList);

                sv.Stop();
                _logger.LogInformation("Iteration number: {iterationNumber}. InterestProcessingJob finish process {paidCount} records. Iteration time: {delay}", 
                    iterationCount, paidToProcess.Count, sv.Elapsed.ToString());
            }
        }

        private async Task PushToGateway(InterestManagerConfig serviceConfig,
            InterestRatePaid interestRatePaid, ICollection<Task> serviceBusTaskList)
        {
            var processResponse = await _spotChangeBalanceService.PayInterestRateAsync(new PayInterestRateRequest()
            {
                TransactionId = interestRatePaid.TransactionId,
                ClientId = serviceConfig.ServiceClient,
                FromWalletId = serviceConfig.ServiceWallet,
                ToWalletId = interestRatePaid.WalletId,
                Amount = (double) interestRatePaid.Amount,
                AssetSymbol = interestRatePaid.Symbol,
                Comment = "Paid interest rate",
                BrokerId = serviceConfig.ServiceBroker,
                RequestSource = nameof(InterestProcessingEngine)
            });

            if (processResponse.Result)
            {
                interestRatePaid.State = PaidState.Completed;
                lock (serviceBusTaskList)
                {
                    serviceBusTaskList.Add(_publisher.PublishAsync(new PaidInterestRateMessage()
                    {
                        BrokerId = serviceConfig.ServiceBroker,
                        ClientId = serviceConfig.ServiceClient,
                        TransactionId = interestRatePaid.TransactionId,
                        WalletId = interestRatePaid.WalletId,
                        Symbol = interestRatePaid.Symbol,
                        Date = DateTime.UtcNow,
                        Amount = interestRatePaid.Amount
                    }).AsTask());
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