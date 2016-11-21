using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KMBit.Util
{
    public class DateTimeUtil
    {
        public static DateTime GetLastDayOfMonth(DateTime date)
        {
            DateTime lastDay = DateTime.MinValue;
            int month = date.Month;
            int year = date.Year;
            int last = 0;

            if (month >= 1 && month <= 7 && month!=2)
            {
                if (month % 2 == 0)
                {
                    month = 30;
                }
                else
                {
                    month = 31;
                }
            }
            if(month>=8 && month<=12)
            {
                if (month % 2 == 0)
                {
                    month = 31;
                }
                else
                {
                    month = 30;
                }
            }

            if (month == 2)
            {
                if (year % 4 == 0)
                {
                    last = 29;
                }
                else
                {
                    last = 28;
                }                
            }

            lastDay = new DateTime(year, month, last, 23, 59, 59);
            return lastDay;
        }
        public static DateTime ConvertToDateTime(long time, string timeZone = "China Standard Time")
        {
            TimeZoneInfo zoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timeZone);
            DateTime minTime = DateTime.MinValue;
            DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            startTime = TimeZoneInfo.ConvertTimeToUtc(startTime);
            minTime = startTime.AddSeconds(time);
            minTime = TimeZoneInfo.ConvertTimeFromUtc(minTime, zoneInfo);
            return minTime;
        }

        public static long ConvertDateTimeToInt(DateTime time)
        {
            double ret = 0;
            DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            ret = (time - startTime).TotalSeconds;
            return (long)Math.Round(ret, 0);
        }
    }
}
