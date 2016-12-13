using Kooboo.IO;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kooboo.CMS.Common.Runtime;
using Kooboo.CMS.Content.Persistence.QcloudCOS.Services;
using Kooboo.CMS.Content.Persistence.QcloudCOS.Utilities;

namespace Kooboo.CMS.Content.Persistence.QcloudCOS.Models
{
    public abstract class RequestBase
    {
        public abstract string op { get; }
    }

    public class RequestContext
    {
        [Required]
        public string remotePath { get; set; }

        [Required]
        public string repository { get; set; }

        public Dictionary<string, string> headers { get; set; } =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                //{"Host", "web.file.myqcloud.com"},
                //{"Content-Type", "application/json"}
            };

        public string contentType { get; set; } = "application/json";

        public int offset { get; set; } = -1;

        public void Sign(CosAccount account)
        {
            var auth = SignUtility.Signature(account.AppId, account.AccessKeyId, account.AccessKeySecret, account.ExpiredTime, account.BucketName);
            headers["Authorization"] = auth;
        }

        public void SignOnce(CosAccount account)
        {
            var path = "/" + remotePath.TrimStart('/');
            var auth = SignUtility.SignatureOnce(account.AppId, account.AccessKeyId, account.AccessKeySecret, path, account.BucketName);
            headers["Authorization"] = auth;
        }

    }
}
