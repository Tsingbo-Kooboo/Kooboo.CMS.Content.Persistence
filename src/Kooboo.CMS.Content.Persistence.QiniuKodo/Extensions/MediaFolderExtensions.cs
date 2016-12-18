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
    public static class MediaFolderExtensions
    {
        public static string GetMediaKey(this MediaFolder folder)
        {
            var path = UrlUtility.Combine(folder.NamePaths);
            return MediaPathUtility.FolderPath(path, folder.Repository.Name);
        }
    }
}
