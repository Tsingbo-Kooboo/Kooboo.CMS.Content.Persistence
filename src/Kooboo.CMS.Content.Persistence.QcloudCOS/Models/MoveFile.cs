using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kooboo.CMS.Content.Persistence.QcloudCOS.Models
{
    public class MoveFileRequest : RequestBase
    {
        public override string op { get; } = "move";
        /// <summary>
        /// 目标路径（不带路径则为当前路径下，带路径则会移动到携带指定的路径下）
        /// </summary>
        public string dest_fileid { get; set; }
        /// <summary>
        /// 覆盖写入目标文件选项，有效值:
        /// 0 不覆盖（若已存在重名文件，则不做覆盖，返回“移动失败”）
        /// 1 覆盖
        /// 默认值为 0 不覆盖。
        /// </summary>
        public OverwriteOption to_over_write { get; set; }
    }

    public class MoveFile
    {
    }
}
