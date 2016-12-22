using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kooboo.CMS.Content.Persistence.QiniuKodo.Extensions
{
    public static class DateTimeExtensions
    {
        private static DateTime MinTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);
        public static long ToUnixTime(this DateTime nowTime)
        {
            DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(MinTime);
            return (nowTime - startTime).Ticks;
        }

        public static DateTime ToUtcTime(this long time)
        {
            return MinTime.AddTicks(time).ToUniversalTime();
        }
    }
}
