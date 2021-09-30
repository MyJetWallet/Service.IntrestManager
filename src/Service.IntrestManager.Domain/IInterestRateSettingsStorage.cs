using System.Collections.Generic;
using System.Threading.Tasks;
using Service.IntrestManager.Domain.Models;

namespace Service.IntrestManager.Domain
{
    public interface IInterestRateSettingsStorage
    {
        Task<List<InterestRateSettings>> GetSettings();
        Task UpsertSettings(InterestRateSettings settings);
        Task RemoveSettings(InterestRateSettings settings);
    }
}