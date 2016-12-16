using Kooboo.CMS.Common.Runtime;
using Kooboo.CMS.Content.Models;
using Kooboo.CMS.Content.Persistence.QcloudCOS.Models;
using Kooboo.CMS.Content.Persistence.QcloudCOS.Services;
using Kooboo.CMS.Content.Persistence.QcloudCOS.Utilities;
using Kooboo.Web.Url;
using System;
using System.Collections.Generic;
using System.IO;
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
            var filePath = MediaPathUtility.FilePath(pathRepository.Key, pathRepository.Value);
            source.VirtualPath = accountService.ResourceUrl(pathRepository.Value, filePath);
            source.FileName = Path.GetFileName(filePath);
            source.UUID = source.FileName;
            source.UserKey = source.FileName;
            source.Size = data.filesize;
            source.UtcCreationDate = data.ctime.ToUtcTime();
            source.UtcLastModificationDate = data.mtime.ToUtcTime();
            source.UserId = data.custom_headers.GetValueOrDefault("UserId");
            return source;
        }

        public static MediaContent BlobToMediaContent(this CosFileData blob,
          MediaContent source,
          ICosAccountService accountService)
        {
            var data = blob;
            var pathRepository = MediaPathUtility.GetPathRepository(data.source_url);
            var filePath = MediaPathUtility.FilePath(pathRepository.Key, pathRepository.Value);
            source.VirtualPath = accountService.ResourceUrl(pathRepository.Value, filePath);
            source.FileName = Path.GetFileName(filePath);
            source.UUID = source.FileName;
            source.UserKey = source.FileName;
            source.Size = data.filesize.GetValueOrDefault();
            source.UtcCreationDate = data.ctime.ToUtcTime();
            source.UtcLastModificationDate = data.mtime.ToUtcTime();
            return source;
        }

        public static MediaContent BlobToMediaContent(this CosFileObject blob,
         MediaContent source,
         ICosAccountService accountService)
        {
            var data = blob;
            var pathRepository = MediaPathUtility.GetPathRepository(data.source_url);
            var filePath = MediaPathUtility.FilePath(pathRepository.Key, pathRepository.Value);
            source.VirtualPath = accountService.ResourceUrl(pathRepository.Value, filePath);
            source.FileName = Path.GetFileName(filePath);
            source.UUID = source.FileName;
            source.UserKey = source.FileName;
            source.Size = data.filesize;
            source.UtcCreationDate = data.ctime.ToUtcTime();
            source.UtcLastModificationDate = data.mtime.ToUtcTime();
            return source;
        }
    }
}
