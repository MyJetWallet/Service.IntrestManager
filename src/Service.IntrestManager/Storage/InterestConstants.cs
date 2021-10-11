using System;

namespace Service.IntrestManager.Storage
{
    public class InterestConstants
    {
        public static DateTime CalculationPeriodDate => DateTime.UtcNow.Date.AddSeconds(-1);
        public static DateTime CalculationExecutedDate => DateTime.UtcNow;
        
        public static DateTime PaidPeriodToDate => DateTime.UtcNow.Date.AddSeconds(-1);
        public static DateTime PaidCreatedDate => DateTime.UtcNow;
    }
}