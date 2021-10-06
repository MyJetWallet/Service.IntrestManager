using System;
using System.Collections.Generic;
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
            await using var ctx = _databaseContextFactory.Create();
            var paidToProcess = ctx.GetNewPaidCollection();
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

            if (paidToProcess.Any())
            {
                allClients = (await _clientWalletService.GetAllClientsAsync())?.Clients;
            }
            
            while (paidToProcess.Any())
            {
                _logger.LogInformation($"InterestProcessingJob find {paidToProcess.Count} new records to process at {DateTime.UtcNow}.");
                foreach (var interestRatePaid in paidToProcess)
                {
                    var client = allClients?.FirstOrDefault(e => e.Wallets.Contains(interestRatePaid.WalletId));
                    if (client == null)
                    {
                        _logger.LogError($"Cannot find client for wallet {interestRatePaid.WalletId}");
                        continue;
                    }

                    if (interestRatePaid.Amount == 0)
                    {
                        _logger.LogInformation($"Skipped walletId: {interestRatePaid.WalletId} and asset: {interestRatePaid.Symbol} with amount {interestRatePaid.Amount}");
                        continue;
                    }
                    var transactionId = Guid.NewGuid().ToString();
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
                        await _publisher.PublishAsync(new PaidInterestRateMessage()
                        {
                            TransactionId = transactionId,
                            WalletId = interestRatePaid.WalletId,
                            Symbol = interestRatePaid.Symbol,
                            Date = DateTime.UtcNow,
                            Amount = interestRatePaid.Amount
                        });
                    }
                    else
                    {
                        interestRatePaid.State = PaidState.Failed;
                        interestRatePaid.ErrorMessage = $"{processResponse.ErrorCode}: {processResponse.ErrorMessage}";
                    }
                }
                await ctx.SaveChangesAsync();
                _logger.LogInformation($"InterestProcessingJob finish process {paidToProcess.Count} records.");
                paidToProcess = ctx.GetNewPaidCollection() ?? new List<InterestRatePaid>();
            }
        }
    }
}