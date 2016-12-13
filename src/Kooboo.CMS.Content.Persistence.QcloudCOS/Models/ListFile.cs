using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kooboo.CMS.Content.Persistence.QcloudCOS.Models
{
    public class ListFileRequest : ListCosObjectRequest
    {
        public override ListObjectPattern pattern { get; } = ListObjectPattern.eListFileOnly;
    }

    public class ListFile : ResponseBase<ListFileData>
    {
        public ListFile()
        {
            data = new ListFileData();
        }
    }

    public class ListFileData : ListCosObjectData
    {
        public IEnumerable<CosFileData> infos { get; set; } = Enumerable.Empty<CosFileData>();
    }

    public class CosFileData : CosFolderData
    {
        #region --- File ---
        public int? filelen { get; set; }

        public int? filesize { get; set; }

        public string sha { get; set; }

        public string source_url { get; set; }

        public string access_url { get; set; }
        #endregion
    }
}
