using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kooboo.CMS.Content.Persistence.QcloudCOS.Extensions;

namespace Kooboo.CMS.Content.Persistence.QcloudCOS.Models
{
    public class CosAccount
    {
        public int AppId { get; set; }
        public string AccessKeyId { get; set; }
        public string AccessKeySecret { get; set; }
        public string BucketName { get; set; }
        public string CustomDomain { get; set; }

        /// <summary>
        /// 超时时间(当前系统时间+300秒)
        /// </summary>
        public long ExpiredTime => DateTime.Now.ToUnixTime() / 1000 + SIGN_EXPIRED_TIME;

        //用户计算用户签名超时时间
        const int SIGN_EXPIRED_TIME = 180;
    }
}
