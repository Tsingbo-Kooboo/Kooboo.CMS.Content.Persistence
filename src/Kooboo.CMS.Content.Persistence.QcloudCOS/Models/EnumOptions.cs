using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kooboo.CMS.Content.Persistence.QcloudCOS.Models
{
    /// <summary>
    /// 有效值：
    /// eInvalid 空权限，此时系统会默认调取 Bucket 权限
    /// eWRPrivate 私有读写
    /// eWPrivateRPublic 公有读私有写
    /// </summary>
    public enum FileAuthority
    {
        eInvalid,
        eWRPrivate,
        eWPrivateRPublic
    }
    /// <summary>
    /// 返回的数据逻辑，有效值：
    /// eListBoth 查询文件和目录
    /// eListDirOnly 仅查询目录
    /// eListFileOnly 仅查询文件
    /// 默认值为 eListBoth 。
    /// </summary>
    public enum ListObjectPattern
    {
        eListBoth,
        eListDirOnly,
        eListFileOnly
    }
}
