using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KM.JXC.Common.Util
{
    public class DateTimeUtil
    {
        public static DateTime ConvertToDateTime(int time)
        {
            DateTime minTime = DateTime.MinValue;
            DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970,1,1));
            minTime = startTime.AddSeconds(time);
            return minTime;
        }

        public static int ConvertDateTimeToInt(DateTime time)
        {
            double ret = 0;
            DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            ret = (time - startTime).TotalSeconds;
            return (Int32)Math.Round(ret,0);
        }
    }
}
