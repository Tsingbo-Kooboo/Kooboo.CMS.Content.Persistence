using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kooboo.CMS.Content.Persistence.QcloudCOS.Models
{
    public class ListFolderRequest : ListCosObjectRequest
    {
        public override ListObjectPattern pattern { get; } = ListObjectPattern.eListDirOnly;
    }

    public class ListFolder : ResponseBase<ListCosFolderData>
    {
        public ListFolder()
        {
            data = new ListCosFolderData();
        }
    }

    public class ListCosFolderData : ListCosObjectData
    {
        public IEnumerable<CosFolderData> infos { get; set; } = Enumerable.Empty<CosFolderData>();
    }

    public class CosFolderData
    {
        public string biz_attr { get; set; }

        public string ctime { get; set; }

        public string mtime { get; set; }

        public string name { get; set; }
    }
}
