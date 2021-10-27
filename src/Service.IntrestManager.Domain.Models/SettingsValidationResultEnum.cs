using System.Runtime.Serialization;

namespace Service.IntrestManager.Domain.Models
{
    [DataContract]
    public enum SettingsValidationResultEnum
    {
        [DataMember(Order = 1)] Ok,
        [DataMember(Order = 2)] WalletAndAssetCannotBeEmpty,
        [DataMember(Order = 3)] DoubleWalletSettingsError,
        [DataMember(Order = 4)] CrossedRangeError
    }
}