using System.Runtime.Serialization;

namespace Service.IntrestManager.Domain.Models
{
    [DataContract]
    public class InterestProcessingResult
    {
        public const string TopicName = "interest-processing-result";
        [DataMember(Order = 1)] public double TotalPaidAmountInUsd { get; set; }
        [DataMember(Order = 2)] public int FailedCount { get; set; }
        [DataMember(Order = 3)] public int PaidCount { get; set; }
    }
}