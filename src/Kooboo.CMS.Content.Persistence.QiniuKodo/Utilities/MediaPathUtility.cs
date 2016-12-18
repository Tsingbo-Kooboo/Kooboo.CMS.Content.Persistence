using Kooboo.CMS.Content.Persistence.QiniuKodo.Models;
using Kooboo.Web.Url;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kooboo.CMS.Content.Persistence.QiniuKodo.Utilities
{
    public static class MediaPathUtility
    {
        public const string MediaPathPrefix = "Media";
        public static string FilePath(string path, string repository)
        {
            return UrlUtility.Combine(repository, MediaPathPrefix, path.Replace('~', '/'));
        }

        public static string FolderPath(string path, string repository)
        {
            return UrlUtility.Combine(repository, MediaPathPrefix, path.Replace('~','/'))
                .TrimEnd('/') + "/";
        }
        /// <summary>
        /// 根据外网可访问的绝对路径返回文件基本信息
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static KoobooMediaInfo GetMediaInfo(string url)
        {
            return new KoobooMediaInfo(url);
        }
    }
}
