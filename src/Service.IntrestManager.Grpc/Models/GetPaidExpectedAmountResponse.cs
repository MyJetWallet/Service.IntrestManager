using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Service.IntrestManager.Grpc.Models;

[DataContract]
public class GetPaidExpectedAmountResponse
{
    [DataMember(Order = 1)] public bool IsError { get; set; }
    [DataMember(Order = 2)] public string ErrorMessage { get; set; }
    [DataMember(Order = 3)] public Dictionary<string, decimal> ExpectedAmountByAsset { get; set; }
}