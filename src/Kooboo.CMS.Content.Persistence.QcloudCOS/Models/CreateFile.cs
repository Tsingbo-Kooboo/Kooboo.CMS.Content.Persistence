using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kooboo.CMS.Content.Persistence.QcloudCOS.Models
{
    public class CreateFileRequest : RequestBase
    {
        public override string op { get; } = "upload";

        public byte[] filecontent { get; set; }

        public string sha { get; set; }

        public string biz_attr { get; set; }

        /// <summary>
        /// 同名文件覆盖选项，有效值：
        /// 0 覆盖（删除已有的重名文件，存储新上传的文件）
        /// 1 不覆盖（若已存在重名文件，则不做覆盖，返回“上传失败”）。
        /// 默认为 1 不覆盖。
        /// </summary>
        public int insertOnly { get; set; }
    }

    public class CreateFile : ResponseBase<CreateFileData>
    {
        public CreateFile()
        {
            data = new CreateFileData();
        }
    }

    public class CreateFileData : CosFileObject
    {
        public string resource_path { get; set; }

        /// <summary>
        /// 操作文件的 url 。
        /// 业务端可以将该 url 作为请求地址来进一步操作文件，
        /// 对应 API ：文件属性、更新文件、删除文件、移动文件中的请求地址。
        /// </summary>
        public string url { get; set; }
    }
}
