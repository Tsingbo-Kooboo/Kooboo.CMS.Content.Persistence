using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kooboo.CMS.Content.Persistence.QcloudCOS.Models
{


    public class ListCosObjectRequest : RequestBase
    {
        public override string op { get; } = "list";

        public int num { get; set; } = 100;
        /// <summary>
        /// 返回的数据逻辑，有效值：
        /// eListBoth 查询文件和目录
        /// eListDirOnly 仅查询目录
        /// eListFileOnly 仅查询文件
        /// 默认值为 eListBoth 。
        /// </summary>
        public ListObjectPattern pattern { get; set; }

        /// <summary>
        /// 0 正序（默认）；1 反序
        /// </summary>
        public OrderDirection order { get; set; }
        /// <summary>
        /// 透传字段，从响应的返回内容中得到。
        /// 若查看第一页，则将空字符串作为 context 传入。
        /// 若需要翻页，需要将前一页返回内容中的 context 透传到参数中。
        /// order 用于指定翻页顺序。
        /// 若 order 填 0，则从当前页正序/往下翻页；
        /// 若 order 填 1，则从当前页倒序/往上翻页。
        /// </summary>
        public string context { get; set; }
    }

    public class ListCosObject : ResponseBase<ListCosObjectData>
    {
        public ListCosObject()
        {
            data = new ListCosObjectData();
        }
    }

    public class ListCosObjectData
    {
        public string context { get; set; }

        public bool has_more { get; set; }

        public int dircount { get; set; }

        public int filecount { get; set; }

        public CosObjectData infos { get; set; } = new CosObjectData();
    }

    public class CosObjectData
    {
        public string biz_attr { get; set; }

        public string ctime { get; set; }

        public string mtime { get; set; }

        public string name { get; set; }

        #region --- File ---
        public int? filelen { get; set; }

        public int? filesize { get; set; }

        public string sha { get; set; }

        public string source_url { get; set; }

        public string access_url { get; set; }
        #endregion
    }
}
