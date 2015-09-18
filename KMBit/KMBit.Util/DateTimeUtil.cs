using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KMBit.Util
{
    public class DateTimeUtil
    {
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
