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

    public class FileDetailData : FolderDetailData
    {
        public int filesize { get; set; }

        public string sha { get; set; }

        public string access_url { get; set; }
        public string source_url { get; set; }

        public FileAuthority? authority { get; set; }
    }
}
