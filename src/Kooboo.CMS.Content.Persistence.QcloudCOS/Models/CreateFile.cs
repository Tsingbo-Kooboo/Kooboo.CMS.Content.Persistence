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
        public InsertOnlyOption insertOnly { get; set; }
    }

    public class CreateFile:ResponseBase<CreateFileData>
    {
    }

    public class CreateFileData
    {
        /// <summary>
        /// 通过 CDN 访问该文件的资源链接（访问速度更快）
        /// </summary>
        public string access_url { get; set; }
        /// <summary>
        /// 该文件在 COS 中的相对路径名，可作为其 ID 标识。 
        /// 格式 /appid/bucket/filename。
        /// 推荐业务端存储 resource_path，
        /// 然后根据业务需求灵活拼接资源 url
        /// （通过 CDN 访问 COS 资源的 url 和直接访问 COS 资源的 url 不同）。
        /// </summary>
        public string resource_path { get; set; }
        /// <summary>
        /// （不通过 CDN）直接访问 COS 的资源链接
        /// </summary>
        public string source_url { get; set; }
        /// <summary>
        /// 操作文件的 url 。
        /// 业务端可以将该 url 作为请求地址来进一步操作文件，
        /// 对应 API ：文件属性、更新文件、删除文件、移动文件中的请求地址。
        /// </summary>
        public string url { get; set; }
    }
}
