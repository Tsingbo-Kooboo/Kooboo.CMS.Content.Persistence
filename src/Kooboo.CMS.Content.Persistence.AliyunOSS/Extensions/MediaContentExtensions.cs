using Aliyun.OSS;
using Kooboo.CMS.Content.Models;
using Kooboo.CMS.Content.Persistence.AliyunOSS.Models;
using Kooboo.CMS.Content.Persistence.AliyunOSS.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kooboo.CMS.Content.Persistence.AliyunOSS.Extensions
{
    public static class MediaContentExtensions
    {
        /// <summary>
        /// 摘要信息，不含内容Stream
        /// </summary>
        /// <param name="blob"></param>
        /// <param name="accountService"></param>
        /// <returns></returns>
        public static MediaContent ToMediaContent(this OssObjectSummary blob, IAccountService accountService)
        {
            var mediaContent = new MediaContent();
            var info = new KoobooMediaInfo(blob.Key);
            mediaContent.Published = true;
            mediaContent.Size = blob.Size;
            mediaContent.FileName = info.FileName;
            mediaContent.UserKey = mediaContent.FileName;
            mediaContent.UUID = mediaContent.FileName;
            mediaContent.VirtualPath = accountService.ResolveUrl(info.FilePath, info.Repository);
            if (mediaContent.Metadata == null)
            {
                mediaContent.Metadata = new MediaContentMetadata();
            }
            return mediaContent;
        }
    }
}
