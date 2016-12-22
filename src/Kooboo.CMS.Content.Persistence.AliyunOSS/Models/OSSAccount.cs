using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kooboo.CMS.Content.Persistence.AliyunOSS.Models
{
    public class OSSAccount
    {
        public string AccessKeyId { get; set; }

        public string AccessKeySecret { get; set; }

        public string Endpoint { get; set; }

        public string RepositoryName { get; set; }

        public string BucketName { get; set; }

        /// <summary>
        /// 自定义域名
        /// </summary>
        public string CustomDomain { get; set; }
    }
}
