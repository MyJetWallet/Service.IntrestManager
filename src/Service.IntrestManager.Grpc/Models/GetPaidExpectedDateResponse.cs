using System;
using System.Runtime.Serialization;
using Service.IntrestManager.Domain.Models;

namespace Service.IntrestManager.Grpc.Models;

[DataContract]
public class GetPaidExpectedDateResponse
{
    [DataMember(Order = 1)] public bool IsError { get; set; }
    [DataMember(Order = 2)] public string ErrorMessage { get; set; }
    [DataMember(Order = 3)] public DateTime ExpectedDate { get; set; }
    [DataMember(Order = 4)] public PaidPeriod Period { get; set; }
}