using System;

namespace Service.IntrestManager.Domain.Models.Extensions
{
    public static class PaidPeriodExtensions
    {
        public static (DateTime start, DateTime end) ToDateRange(this PaidPeriod src)
        {
            DateTime start;
            DateTime end;
            
            switch (src)
            {
                case PaidPeriod.Day:
                {
                    start = DateTime.UtcNow.Date;
                    end = DateTime.UtcNow.Date.AddDays(1).AddSeconds(-1);
                    break;
                }
                case PaidPeriod.Week:
                {
                    var payDay = DayOfWeek.Monday;
                    start = DateTime.UtcNow.GetStartOfWeek(payDay).Date;
                    end = start.Date.AddDays(8).AddSeconds(-1);
                    break;
                }
                case PaidPeriod.Month:
                {
                    start = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
                    end = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, start.GetLastDayOfMonth())
                        .AddDays(1).AddSeconds(-1);
                    break;
                }
                default: throw new NotSupportedException($"Period {src}");
            }

            return (start, end);
        }
        
        private static DateTime GetStartOfWeek(this DateTime dt, DayOfWeek startOfWeek)
        {
            var diff = (7 + (dt.DayOfWeek - startOfWeek)) % 7;
            
            return dt.AddDays(-1 * diff).Date;
        }
        
        private static int GetLastDayOfMonth(this DateTime dateTime, int? month = null)
        {
            return DateTime.DaysInMonth(dateTime.Year, month ?? dateTime.Month);;
        }
    }
}