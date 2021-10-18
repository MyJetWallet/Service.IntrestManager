using System.Collections.Generic;
using System.Threading.Tasks;
using Service.IntrestManager.Domain.Models;

namespace Service.IntrestManager.Domain
{
    public interface IInterestRateByWalletStorage
    {
        Task<InterestRateByWallet> GetRatesByWallet(string walletId);
        Task ClearRates();
    }
}