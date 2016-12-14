using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kooboo.CMS.Content.Persistence.QcloudCOS.Models
{
    public class UpdateFileRequest : RequestBase
    {
        public override string op { get; } = "update";

        /// <summary>
        /// 需要执行的修改操作，有效值：
        /// 0x40 修改 customheader 属性
        /// 0x80 修改 authority 属性
        /// 0x40 0x80 同时修改两个属性
        /// </summary>
        public int flag { get; set; } = 0x40 | 0x80;
        /// <summary>
        /// Object 的权限，默认与 Bucket 权限一致，此时不会返回该字段。
        /// 如果设置了独立权限，则会返回该字段。
        /// 有效值：
        /// eInvalid 空权限，此时系统会默认调取 Bucket 权限
        /// eWRPrivate 私有读写
        /// eWPrivateRPublic 公有读私有写
        /// </summary>
        public FileAuthority? authority { get; set; } = FileAuthority.eInvalid;
        /// <summary>
        /// 用户自定义 header，在执行更新操作时，只需填写需要增加或修改的项
        /// </summary>
        public Dictionary<string, string> custom_headers { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    }

    public class UpdateFile : EmptyResponse
    {
    }
}
