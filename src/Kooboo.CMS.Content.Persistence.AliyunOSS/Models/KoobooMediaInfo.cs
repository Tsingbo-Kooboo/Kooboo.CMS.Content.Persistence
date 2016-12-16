using Kooboo.Web.Url;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kooboo.CMS.Content.Persistence.AliyunOSS.Models
{
    public class KoobooMediaInfo
    {
        public KoobooMediaInfo(string url)
        {
            if (!UrlUtility.IsAbsoluteUrl(url))
            {
                throw new ArgumentOutOfRangeException(nameof(url), $"{nameof(url)} should be an absolute url");
            }
            var uri = new Uri(url);
            var absolutePath = uri.AbsolutePath;
            Repository = uri.Segments[1].Trim('/');
            var segments = uri.Segments.Skip(3).ToArray();
            var path = UrlUtility.Combine(segments);
            FilePath = path;
            FileName = Path.GetFileName(path);
            Folder = Path.GetDirectoryName(path);
        }

        public string Folder { get; private set; }

        public string FileName { get; private set; }

        public string Repository { get; private set; }

        public string FilePath { get; private set; }
    }
}
