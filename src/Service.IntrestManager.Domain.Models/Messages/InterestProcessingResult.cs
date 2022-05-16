using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Service.IntrestManager.Domain.Models
{
    [DataContract]
    public class InterestProcessingResult
    {
        public const string TopicName = "interest-processing-result";
        [DataMember(Order = 1)] public double TotalPaidAmountInUsd { get; set; }
        [DataMember(Order = 2)] public int FailedCount { get; set; }
        [DataMember(Order = 3)] public int CompletedCount { get; set; }

        [DataMember(Order = 4)]
        public Dictionary<string, decimal> FailedToPayAmountByAsset { get; set; } = new Dictionary<string, decimal>();

        [DataMember(Order = 5)] public decimal FailedToPayAmountInUsd { get; set; }
        [DataMember(Order = 6)] public decimal CompletedToPayAmountInUsd { get; set; }

        [DataMember(Order = 7)]
        public Dictionary<string, decimal> CompletedToPayAmountByAsset { get; set; } =
            new Dictionary<string, decimal>();

        public void AddFailed(IEnumerable<FailedInterestRateMessage> messages)
        {
            if (messages == null)
            {
                return;
            }
            
            FailedToPayAmountByAsset ??= new Dictionary<string, decimal>();

            foreach (var interestRate in messages)
            {
                if (FailedToPayAmountByAsset.ContainsKey(interestRate.Symbol))
                {
                    FailedToPayAmountByAsset[interestRate.Symbol] += interestRate.Amount;
                }
                else
                {
                    FailedToPayAmountByAsset.Add(interestRate.Symbol, interestRate.Amount);
                }

                FailedToPayAmountInUsd +=
                    interestRate.Amount * interestRate.IndexPrice;
                FailedCount += 1;
            }
        }

        public void AddCompleted(IEnumerable<PaidInterestRateMessage> messages)
        {
            if (messages == null)
            {
                return;
            }

            CompletedToPayAmountByAsset ??= new Dictionary<string, decimal>();

            foreach (var interestRate in messages)
            {
                if (CompletedToPayAmountByAsset.ContainsKey(interestRate.Symbol))
                {
                    CompletedToPayAmountByAsset[interestRate.Symbol] += interestRate.Amount;
                }
                else
                {
                    CompletedToPayAmountByAsset.Add(interestRate.Symbol, interestRate.Amount);
                }

                CompletedToPayAmountInUsd += interestRate.Amount * interestRate.IndexPrice;
                CompletedCount += 1;
            }
        }
    }
}