using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Microsoft.Extensions.Logging;
using MyJetWallet.Sdk.Service.Tools;
using Service.ChangeBalanceGateway.Grpc;
using Service.ChangeBalanceGateway.Grpc.Models;
using Service.ClientWallets.Grpc;
using Service.ClientWallets.Grpc.Models;
using Service.InterestManager.Postrges;
using Service.IntrestManager.Domain.Models;
using Service.IntrestManager.Grpc;

namespace Service.IntrestManager.Jobs
{
    public class InterestProcessingJob : IStartable
    {
        private readonly ILogger<InterestProcessingJob> _logger;
        private readonly MyTaskTimer _timer;
        private readonly DatabaseContextFactory _databaseContextFactory;
        private readonly ISpotChangeBalanceService _spotChangeBalanceService;
        private readonly IClientWalletService _clientWalletService;
        private readonly IInterestManagerConfigService _interestManagerConfigService;

        public InterestProcessingJob(ILogger<InterestProcessingJob> logger,
            DatabaseContextFactory databaseContextFactory,
            ISpotChangeBalanceService spotChangeBalanceService,
            IClientWalletService clientWalletService, 
            IInterestManagerConfigService interestManagerConfigService)
        {
            _logger = logger;
            _databaseContextFactory = databaseContextFactory;
            _spotChangeBalanceService = spotChangeBalanceService;
            _clientWalletService = clientWalletService;
            _interestManagerConfigService = interestManagerConfigService;

            _timer = new MyTaskTimer(nameof(InterestProcessingJob), 
                TimeSpan.FromSeconds(Program.Settings.InterestCalculationTimerInSeconds), _logger, DoTime);
            _logger.LogInformation($"InterestProcessingJob timer: {TimeSpan.FromSeconds(Program.Settings.InterestProcessingTimerInSeconds)}");
        }

        private async Task DoTime()
        {
            await ProcessInterest();
        }

        private async Task ProcessInterest()
        {
            return;
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
                allClients = (await _clientWalletService.GetAllClientsAsync()).Clients;
            }
            
            while (paidToProcess.Any())
            {
                _logger.LogInformation($"InterestProcessingJob find {paidToProcess.Count} new records to process at {DateTime.UtcNow}.");
                foreach (var interestRatePaid in paidToProcess)
                {
                    var client = allClients.FirstOrDefault(e => e.Wallets.Contains(interestRatePaid.WalletId));
                    var processResponse = await _spotChangeBalanceService.PayInterestRateAsync(new PayInterestRateRequest()
                    {
                        TransactionId = Guid.NewGuid().ToString(),
                        ClientId = client?.ClientId,
                        FromWalletId = fromWallet,
                        ToWalletId = interestRatePaid.WalletId,
                        Amount = (double) interestRatePaid.Amount,
                        AssetSymbol = interestRatePaid.Symbol,
                        Comment = "Paid interest rate",
                        BrokerId = client?.BrokerId,
                        RequestSource = nameof(InterestProcessingJob)
                    });

                    if (processResponse.Result)
                    {
                        interestRatePaid.State = PaidState.Completed;
                        // todo: push to service bus
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

        public void Start()
        {
            _timer.Start();
        }
    }
}