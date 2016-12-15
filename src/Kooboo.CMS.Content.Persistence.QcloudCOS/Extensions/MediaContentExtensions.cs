using Kooboo.CMS.Common.Runtime;
using Kooboo.CMS.Content.Models;
using Kooboo.CMS.Content.Persistence.QcloudCOS.Models;
using Kooboo.CMS.Content.Persistence.QcloudCOS.Services;
using Kooboo.CMS.Content.Persistence.QcloudCOS.Utilities;
using Kooboo.Web.Url;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kooboo.CMS.Content.Persistence.QcloudCOS.Extensions
{
    public static class MediaContentExtensions
    {
        public static string GetMediaBlobPath(this MediaContent content)
        {
            return UrlUtility.Combine(content.FolderName, content.FileName);
        }

        public static MediaContent BlobToMediaContent(this FileDetail blob,
            MediaContent source,
            ICosAccountService accountService)
        {
            var data = blob.data;
            var pathRepository = MediaPathUtility.GetPathRepository(data.source_url);
            source.VirtualPath = accountService.ResourceUrl(pathRepository.Value, pathRepository.Key);
            source.Size = data.filesize;
            source.UtcCreationDate = data.ctime.ToUtcTime();
            source.UtcLastModificationDate = data.mtime.ToUtcTime();
            source.UUID = data.access_url;
            source.UserKey = pathRepository.Key;
            source.UserId = data.custom_headers.GetString("UserId");
            return source;
        }

        public static MediaContent BlobToMediaContent(this CosFileData blob,
          MediaContent source,
          ICosAccountService accountService)
        {
            var data = blob;
            var pathRepository = MediaPathUtility.GetPathRepository(data.source_url);
            source.VirtualPath = accountService.ResourceUrl(pathRepository.Value, pathRepository.Key);
            source.Size = data.filesize.GetValueOrDefault();
            source.UtcCreationDate = data.ctime.ToUtcTime();
            source.UtcLastModificationDate = data.mtime.ToUtcTime();
            source.UUID = data.access_url;
            source.UserKey = pathRepository.Key;
            //source.UserId = data.custom_headers.GetString("UserId");
            return source;
        }
    }
}
