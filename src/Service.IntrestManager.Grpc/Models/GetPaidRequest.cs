using System;
using System.Runtime.Serialization;
using Service.IntrestManager.Domain.Models;

namespace Service.IntrestManager.Grpc.Models
{
    [DataContract]
    public class GetPaidRequest
    {
        [DataMember(Order = 1)] public long LastId { get; set; }
        [DataMember(Order = 2)] public int BatchSize { get; set; }
        [DataMember(Order = 3)] public string AssetFilter { get; set; }
        [DataMember(Order = 4)] public string WalletFilter { get; set; }
        [DataMember(Order = 5)] public DateTime DateFilter { get; set; }
        [DataMember(Order = 6)] public PaidState StateFilter { get; set; }
    }
}