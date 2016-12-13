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
                {"Host", "web.file.myqcloud.com"},
                {"Content-Type", "application/json"}
            };

        private string _contentType;

        public string contentType
        {
            get
            {
                if (string.IsNullOrEmpty(_contentType))
                {
                    return IOUtility.MimeType(remotePath);
                }
                return _contentType;
            }
            set
            {
                _contentType = value;
            }
        }

        public int offset { get; set; } = -1;

        public long ExpiredTime { get; set; }

        public void Sign()
        {
            var accountService = EngineContext.Current.Resolve<ICosAccountService>();
            var account = accountService.Get(repository);
            var auth = SignUtility.Signature(account.AppId, account.AccessKeyId, account.AccessKeySecret, account.ExpiredTime, account.BucketName);
            headers["Authorization"] = auth;
        }
    }
}
