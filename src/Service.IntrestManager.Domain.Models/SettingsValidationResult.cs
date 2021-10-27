using System.Runtime.Serialization;

namespace Service.IntrestManager.Domain.Models
{
    [DataContract]
    public class SettingsValidationResult
    {
        [DataMember(Order = 1)] public InterestRateSettings InterestRateSettings { get; set; }
        [DataMember(Order = 2)] public SettingsValidationResultEnum ValidationResult { get; set; }
    }
}