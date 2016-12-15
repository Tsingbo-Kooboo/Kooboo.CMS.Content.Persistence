using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kooboo.CMS.Content.Persistence.QcloudCOS.Models
{
    public abstract class ListCosObjectRequest : RequestBase
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
        public abstract ListObjectPattern pattern { get; }

        /// <summary>
        /// 0 正序（默认）；1 反序
        /// </summary>
        public int order { get; set; }
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

    public abstract class ListCosObjectData
    {
        public string context { get; set; }

        public bool has_more { get; set; }

        public int dircount { get; set; }

        public int filecount { get; set; }
    }

    public abstract class CosFolderObject
    {
        /// <summary>
        /// 文件属性，业务端维护
        /// </summary>
        public string biz_attr { get; set; }
        /// <summary>
        /// 创建时间，10 位 Unix 时间戳
        /// </summary>
        public string ctime { get; set; }
        /// <summary>
        /// 修改时间，10 位 Unix 时间戳
        /// </summary>
        public string mtime { get; set; }
       
        /// <summary>
        /// 文件（夹）名
        /// </summary>
        public string name { get; set; }
    }

    public abstract class CosFileObject : CosFolderObject
    {
        /// <summary>
        /// 文件大小
        /// </summary>
        public int filesize { get; set; }
        /// <summary>
        /// 文件 SHA-1 校验码
        /// </summary>
        public string sha { get; set; }
        /// <summary>
        /// 通过 CDN 访问该文件的资源链接
        /// </summary>
        public string access_url { get; set; }
        /// <summary>
        /// （不通过 CDN ）直接访问 COS 的资源链接
        /// </summary>
        public string source_url { get; set; }
        /// <summary>
        /// Object 的权限，默认与 Bucket 权限一致，此时不会返回该字段。
        /// 如果设置了独立权限，则会返回该字段。
        /// 有效值：
        /// eInvalid 空权限，此时系统会默认调取 Bucket 权限
        /// eWRPrivate 私有读写
        /// eWPrivateRPublic 公有读私有写
        /// </summary>
        public FileAuthority? authority { get; set; }
    }

}
