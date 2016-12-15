using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kooboo.CMS.Content.Persistence.QcloudCOS.Models
{
    public class FolderDetailRequest : RequestBase
    {
        public override string op { get; } = "stat";
    }

    public class FolderDetail : ResponseBase<FolderDetailData>
    {
    }

    public class FolderDetailData : CosFolderObject
    {
        /// <summary>
        /// 用户自定义头部
        /// </summary>
        public Dictionary<string, string> custom_headers { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    }
}
