using System.Threading.Tasks;
using DotNetCoreDecorators;
using Service.ClientWallets.Domain.Models.ServiceBus;
using Service.IntrestManager.Domain;

namespace Service.IntrestManager.Api.Jobs
{
    public class WalletUpdateJob
    {
        private readonly IInterestRateByWalletGenerator _interestRateByWalletGenerator;

        public WalletUpdateJob(IInterestRateByWalletGenerator interestRateByWalletGenerator, ISubscriber<ClientWalletUpdateMessage> subscriber)
        {
            _interestRateByWalletGenerator = interestRateByWalletGenerator;
            subscriber.Subscribe(HandleMessage);
        }

        private async ValueTask HandleMessage(ClientWalletUpdateMessage message)
        {
            if (message.OldWallet.EnableEarnProgram != message.NewWallet.EnableEarnProgram)
                await _interestRateByWalletGenerator.GenerateRatesByWallet(message.NewWallet.WalletId);
        }
    }
}