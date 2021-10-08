using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using DotNetCoreDecorators;
using Microsoft.Extensions.Logging;
using Service.ChangeBalanceGateway.Grpc;
using Service.ChangeBalanceGateway.Grpc.Models;
using Service.ClientWallets.Grpc;
using Service.ClientWallets.Grpc.Models;
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
        private readonly IClientWalletService _clientWalletService;
        private readonly IInterestManagerConfigService _interestManagerConfigService;
        private readonly IPublisher<PaidInterestRateMessage> _publisher;

        public InterestProcessingEngine(ILogger<InterestProcessingEngine> logger,
            DatabaseContextFactory databaseContextFactory,
            ISpotChangeBalanceService spotChangeBalanceService,
            IClientWalletService clientWalletService, 
            IInterestManagerConfigService interestManagerConfigService, 
            IPublisher<PaidInterestRateMessage> publisher)
        {
            _logger = logger;
            _databaseContextFactory = databaseContextFactory;
            _spotChangeBalanceService = spotChangeBalanceService;
            _clientWalletService = clientWalletService;
            _interestManagerConfigService = interestManagerConfigService;
            _publisher = publisher;
        }

        public async Task Execute()
        {
            await ProcessInterest();
        }

        private async Task ProcessInterest()
        {
            var allClients = new List<ClientGrpc>();
            var fromWalletResponse = await _interestManagerConfigService.GetInterestManagerConfigAsync();

            if (!fromWalletResponse.Success ||
                fromWalletResponse.Config == null ||
                string.IsNullOrWhiteSpace(fromWalletResponse.Config.ServiceWallet))
            {
                _logger.LogError("Cannot process interest. Service wallet IS EMPTY !!!!");
                return;
            }
            var fromWallet = fromWalletResponse.Config.ServiceWallet;

            var iterationCount = 0;
            
            while (true)
            {
                var serviceBusTaskList = new List<Task>();
                var gatewayTaskList = new List<Task>();
                iterationCount++;
                
                var sv = new Stopwatch();
                sv.Start();
                
                await using var ctx = _databaseContextFactory.Create();
                var paidToProcess = ctx.GetNewPaidCollection();
                
                if (!paidToProcess.Any())
                {
                    break;
                };
                
                if (!allClients.Any())
                {
                    allClients = (await _clientWalletService.GetAllClientsAsync())?.Clients ?? new List<ClientGrpc>();
                }
                
                _logger.LogInformation("InterestProcessingJob find {paidCount} new records to process at {dateJson}.",
                    paidToProcess.Count, DateTime.UtcNow.ToString(CultureInfo.InvariantCulture));
                foreach (var interestRatePaid in paidToProcess)
                {
                    var client = allClients?.FirstOrDefault(e => e.Wallets.Contains(interestRatePaid.WalletId));
                    if (client == null)
                    {
                        _logger.LogError("Cannot find client for wallet {walletId}", interestRatePaid.WalletId);
                        
                        interestRatePaid.State = PaidState.Failed;
                        interestRatePaid.ErrorMessage = string.Format("Cannot find client for wallet {walletId}", interestRatePaid.WalletId);
                        
                        continue;
                    }
                    if (interestRatePaid.Amount == 0)
                    {
                        _logger.LogInformation("Skipped walletId: {walletid} and asset: {assetSymbol} with amount {amountJson}",
                            interestRatePaid.WalletId, interestRatePaid.Symbol, interestRatePaid.Amount);
                        
                        interestRatePaid.State = PaidState.Failed;
                        interestRatePaid.ErrorMessage = string.Format("Skipped walletId: {walletid} and asset: {assetSymbol} with amount {amountJson}",
                            interestRatePaid.WalletId, interestRatePaid.Symbol, interestRatePaid.Amount);
                        
                        continue;
                    }
                    var transactionId = Guid.NewGuid().ToString();
                    
                    await Task.Delay(1);
                    
                    gatewayTaskList.Add(PushToGateway(transactionId, client, fromWallet, interestRatePaid, serviceBusTaskList));
                }
                await Task.WhenAll(gatewayTaskList);
                await ctx.SaveChangesAsync();
                await Task.WhenAll(serviceBusTaskList);

                sv.Stop();
                _logger.LogInformation("Iteration number: {iterationNumber}. InterestProcessingJob finish process {paidCount} records. Iteration time: {delay}", 
                    iterationCount, paidToProcess.Count, sv.Elapsed.ToString());
            }
        }

        private async Task PushToGateway(string transactionId, ClientGrpc client, string fromWallet,
            InterestRatePaid interestRatePaid, ICollection<Task> serviceBusTaskList)
        {
            var processResponse = await _spotChangeBalanceService.PayInterestRateAsync(new PayInterestRateRequest()
            {
                TransactionId = transactionId,
                ClientId = client?.ClientId,
                FromWalletId = fromWallet,
                ToWalletId = interestRatePaid.WalletId,
                Amount = (double) interestRatePaid.Amount,
                AssetSymbol = interestRatePaid.Symbol,
                Comment = "Paid interest rate",
                BrokerId = client?.BrokerId,
                RequestSource = nameof(InterestProcessingEngine)
            });

            if (processResponse.Result)
            {
                interestRatePaid.State = PaidState.Completed;
                lock (serviceBusTaskList)
                {
                    serviceBusTaskList.Add(_publisher.PublishAsync(new PaidInterestRateMessage()
                    {
                        TransactionId = transactionId,
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