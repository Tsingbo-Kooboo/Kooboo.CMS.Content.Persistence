using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kooboo.Web.Url;

namespace Kooboo.CMS.Content.Persistence.QcloudCOS.Utilities
{
    public static class MediaPathUtility
    {
        public const string MediaPathPrefix = "Media";
        public static string FilePath(string path, string repository)
        {
            return UrlUtility.Combine(repository, MediaPathPrefix, path);
        }

        public static string FolderPath(string path, string repository)
        {
            return UrlUtility.Combine(repository, MediaPathPrefix, path).TrimEnd('/') + "/";
        }
    }
}
