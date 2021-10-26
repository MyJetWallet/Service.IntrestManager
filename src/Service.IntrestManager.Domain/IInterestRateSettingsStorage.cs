using System.Collections.Generic;
using System.Threading.Tasks;
using Service.IntrestManager.Domain.Models;

namespace Service.IntrestManager.Domain
{
    public interface IInterestRateSettingsStorage
    {
        Task<List<InterestRateSettings>> GetSettings();
        Task<string> UpsertSettings(InterestRateSettings settings);
        Task UpsertSettingsList(List<InterestRateSettings> settingsList);
        Task RemoveSettings(InterestRateSettings settings);
        Task SyncSettings();
    }
}