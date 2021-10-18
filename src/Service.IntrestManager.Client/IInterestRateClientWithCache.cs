using System.Threading.Tasks;
using Service.IntrestManager.Domain.Models;

namespace Service.IntrestManager.Client
{
    public interface IInterestRateClientWithCache
    {
        Task<InterestRateByWallet> GetInterestRatesByWalletAsync(string walletId);
    }
}