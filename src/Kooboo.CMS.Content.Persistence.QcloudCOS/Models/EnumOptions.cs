using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kooboo.CMS.Content.Persistence.QcloudCOS.Models
{
    /// <summary>
    /// 覆盖写入目标文件选项，有效值:
    /// 0 不覆盖（若已存在重名文件，则不做覆盖，返回“移动失败”）
    /// 1 覆盖
    /// 默认值为 0 不覆盖。
    /// </summary>
    public enum OverwriteOption
    {
        No = 0,
        Yes = 1
    }
    /// <summary>
    /// 同名文件覆盖选项，有效值：
    /// 0 覆盖（删除已有的重名文件，存储新上传的文件）
    /// 1 不覆盖（若已存在重名文件，则不做覆盖，返回“上传失败”）。
    /// 默认为 1 不覆盖。
    /// </summary>
    public enum InsertOnlyOption
    {
        NoCover = 1,
        Cover = 0
    }

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
    /// <summary>
    /// 列出顺序，有效值：
    /// 0 正序（默认）；
    /// 1 反序
    /// </summary>
    public enum OrderDirection
    {
        Asc = 0,
        Desc = 1
    }
}
