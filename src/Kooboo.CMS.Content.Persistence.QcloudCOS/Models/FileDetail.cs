using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kooboo.CMS.Content.Persistence.QcloudCOS.Models
{
    public class FileDetailRequest : RequestBase
    {
        public override string op { get; } = "stat";
    }

    public class FileDetail : ResponseBase<FileDetailData>
    {
    }

    public class FileDetailData : CosFileObject
    {
        /// <summary>
        /// 用户自定义头部
        /// </summary>
        public Dictionary<string, string> custom_headers { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    }
}
