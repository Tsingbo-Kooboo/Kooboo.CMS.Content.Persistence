using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kooboo.CMS.Content.Persistence.AliyunOSS.Models
{
    public class ConstValues
    {
        public const string OssMetaPrefix = "x-oss-meta-";
        public const string TextContentFileDirectoryName = "Folders";
        public class Metadata
        {
            public const string AllowedExtensions = "AllowedExtensions";
            public const string Title = "Title";
            public const string AlternateText = "AlternateText";
            public const string Description = "Description";
            public const string UserId = "UserId";
        }
    }
}
