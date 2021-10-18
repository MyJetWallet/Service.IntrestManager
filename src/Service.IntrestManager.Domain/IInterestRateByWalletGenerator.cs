using System.Threading.Tasks;
using Service.IntrestManager.Domain.Models;

namespace Service.IntrestManager.Domain
{
    public interface IInterestRateByWalletGenerator
    {
        Task<InterestRateByWallet> GenerateRatesByWallet(string walletId);
        Task ClearRates();
    }
}