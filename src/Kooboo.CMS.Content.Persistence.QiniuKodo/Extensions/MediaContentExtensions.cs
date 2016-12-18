using Kooboo.CMS.Content.Models;
using Kooboo.CMS.Content.Persistence.QiniuKodo.Utilities;
using Kooboo.Web.Url;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kooboo.CMS.Content.Persistence.QiniuKodo.Extensions
{
    public static class MediaContentExtensions
    {
        public static string GetMediaKey(this MediaContent content)
        {
            var path = UrlUtility.Combine(content.FolderName.Replace('~', '/'), content.FileName);
            return MediaPathUtility.FilePath(path, content.Repository);
        }
    }
}
