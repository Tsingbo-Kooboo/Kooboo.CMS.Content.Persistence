using Aliyun.OSS;
using Aliyun.OSS.Common;
using Kooboo.CMS.Common.Runtime.Dependency;
using Kooboo.CMS.Content.Models;
using Kooboo.CMS.Content.Persistence.AliyunOSS.Models;
using Kooboo.CMS.Content.Persistence.AliyunOSS.Utilities;
using Kooboo.Web.Url;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kooboo.CMS.Content.Persistence.AliyunOSS.Services
{
   
    public interface IAccountService
    {
        OSSAccount Get(string repository);

        OssClient GetClient(string repository, out string bucket);

        /// <summary>
        /// 外网可访问的地址
        /// </summary>
        /// <param name="path">Kooboo管理的路径，比如 home/abc.jpg</param>
        /// <param name="repository"></param>
        /// <returns></returns>
        string ResolveUrl(string path, string repository);
    }
    [Dependency(typeof(IAccountService))]
    public class AccountService : IAccountService
    {
        public OSSAccount Get(string repository)
        {
            var account = AliyunAccountSettings.Instance;
            var result = new OSSAccount
            {
                BucketName = account.BucketName.ToLower(),
                Endpoint = account.Endpoint,
                AccessKeyId = account.AccessKeyId,
                AccessKeySecret = account.AccessKeySecret,
                CustomDomain = account.CustomDomain,
                RepositoryName = repository
            };
            if (repository != null)
            {
                var config = account.RepositoryBuckets
                    .FirstOrDefault(it => it.RepositoryName.Equals(repository, StringComparison.OrdinalIgnoreCase));
                if (config != null)
                {
                    result.BucketName = config.BucketName;
                    if (!string.IsNullOrEmpty(config.CustomDomain))
                    {
                        result.CustomDomain = config.CustomDomain;
                    }
                }
            }
            return result;
        }

        public OssClient GetClient(string repository, out string bucket)
        {
            var account = Get(repository);
            bucket = account.BucketName;
            return new OssClient(account.Endpoint,
                account.AccessKeyId,
                account.AccessKeySecret);
        }

        public string ResolveUrl(string path, string repository)
        {
            var account = Get(repository);
            var key = MediaPathUtility.FilePath(path, repository);
            return UrlUtility.ToHttpAbsolute(account.CustomDomain, key);
        }
    }
}
