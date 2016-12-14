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
        /// <summary>
        /// key:path
        /// value:repository
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static KeyValuePair<string, string> GetPathRepository(string url)
        {
            var uri = new Uri(url);
            var absolutePath = uri.AbsolutePath;
            var repository = uri.Segments[1].Trim('/');
            var segments = uri.Segments.Skip(3).ToArray();
            var path = UrlUtility.Combine(segments);
            return new KeyValuePair<string, string>(path, repository);
        }
    }
}
