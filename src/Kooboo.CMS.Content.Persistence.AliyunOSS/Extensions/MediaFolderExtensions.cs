using Kooboo.CMS.Content.Models;
using Kooboo.CMS.Content.Persistence.AliyunOSS.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kooboo.CMS.Content.Persistence.AliyunOSS.Extensions
{
    public static class MediaFolderExtensions
    {
        public static string GetOssKey(this MediaFolder folder)
        {
            return MediaPathUtility.FolderPath(folder.FullName.Replace('~','/'), folder.Repository.Name);
        }
    }
}
