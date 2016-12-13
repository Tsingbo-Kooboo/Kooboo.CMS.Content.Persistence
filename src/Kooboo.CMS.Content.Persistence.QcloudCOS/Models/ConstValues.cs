using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kooboo.CMS.Content.Persistence.QcloudCOS.Models
{
    public class ConstValues
    {
        public const string Authorization = "Authorization";
        public const string ContentType = "Content-Type";
        public const string MultipartFormData = "multipart/form-data";
        public const string ApplicationJson = "application/json";

        public static string[] SystemHeaders = new[] {
            CustomHeaders.CacheControl,
            CustomHeaders.ContentType,
            CustomHeaders.ContentDisposition,
            CustomHeaders.ContentLanguage,
            CustomHeaders.ContentEncoding
        };

        public class CustomHeaders
        {
            public const string CacheControl = "Cache-Control";
            public const string ContentType = "Content-Type";
            public const string ContentDisposition = "Content-Disposition";
            public const string ContentLanguage = "Content-Language";
            public const string ContentEncoding = "Content-Encoding";
            public const string XCosMeta = "x-cos-meta-";
        }
    }
}
