using System.Runtime.Serialization;

namespace Service.IntrestManager.Domain.Models
{
    [DataContract]
    public class InterestManagerConfig
    {
        [DataMember(Order = 1)] public string ServiceWallet { get; set; }
    }
}