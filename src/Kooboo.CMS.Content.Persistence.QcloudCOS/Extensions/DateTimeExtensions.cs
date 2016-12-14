using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kooboo.CMS.Content.Persistence.QcloudCOS.Extensions
{
    public static class DateTimeExtensions
    {
        private static DateTime MinTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);
        public static long ToUnixTime(this DateTime nowTime)
        {
            DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(MinTime);
            return (long)Math.Round((nowTime - startTime).TotalMilliseconds, MidpointRounding.AwayFromZero);
        }

        public static DateTime ToUtcTime(this string time)
        {
            long t;
            if (long.TryParse(time, out t))
            {
                return MinTime.AddMilliseconds(t);
            }
            return MinTime;
        }
    }
}
