using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kooboo.CMS.Content.Persistence.QcloudCOS
{
    internal static class LibObjectExtensions
    {
        private static readonly string[] Booleans = new string[] { "true", "yes", "on", "1" };

        public static string AsString(this object value)
        {
            return AsString(value, String.Empty);
        }

        public static string AsString(this object value, string defaultValue)
        {
            return As<string>(value, defaultValue);
        }

        public static bool AsBool(this object value)
        {
            return AsBool(value, false);
        }

        public static bool AsBool(this object value, bool defaultValue)
        {
            if (value != null && value != DBNull.Value)
            {
                return Booleans.Contains(value.ToString().ToLower());
            }

            return defaultValue;
        }

        public static T As<T>(this object value)
        {
            return As<T>(value, default(T));
        }

        public static T As<T>(this object value, T defaultValue)
        {
            T convertedValue = defaultValue;
            if (value != null && value != DBNull.Value && value is IConvertible)
            {
                try
                {
                    convertedValue = (T)value;
                }
                catch
                {
                    try
                    {
                        convertedValue = (T)Convert.ChangeType(value, typeof(T));
                    }
                    catch
                    { }
                }
            }

            return convertedValue;
        }
    }
}
