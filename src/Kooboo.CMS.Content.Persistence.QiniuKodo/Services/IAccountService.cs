using Qiniu.Common;
using Qiniu.Util;
using Kooboo.CMS.Common.Runtime.Dependency;
using Kooboo.CMS.Content.Models;
using Kooboo.CMS.Content.Persistence.QiniuKodo.Models;
using Kooboo.CMS.Content.Persistence.QiniuKodo.Utilities;
using Kooboo.Web.Url;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Qiniu.Storage;
using Kooboo.CMS.Content.Persistence.QiniuKodo;

namespace Kooboo.CMS.Content.Persistence.QiniuKodo.Services
{

    public interface IAccountService
    {
        KodoAccount Get(string repository);

        Mac GetMac(string repository, out string bucket);

        BucketManager GetBucketManager(string repository, out string bucket);

        UploadManager GetUploadManager(string repository, out string token);
        /// <summary>
        /// 外网可访问的地址
        /// </summary>
        /// <param name="path">Kooboo管理的路径，比如 home/abc.jpg</param>
        /// <param name="repository"></param>
        /// <returns></returns>
        string ResolveUrl(string path, string repository);

        string AbsoluteUrl(string key, string repository);
    }
    [Dependency(typeof(IAccountService))]
    public class AccountService : IAccountService
    {
        public string AbsoluteUrl(string key, string repository)
        {
            var account = Get(repository);
            return UrlUtility.ToHttpAbsolute(account.CustomDomain, key);
        }

        public KodoAccount Get(string repository)
        {
            var account = QiniuAccountSettings.Instance;
            var result = new KodoAccount
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

        public BucketManager GetBucketManager(string repository, out string bucket)
        {
            var mac = GetMac(repository, out bucket);
            return new BucketManager(mac);
        }

        public Mac GetMac(string repository, out string bucket)
        {
            var account = Get(repository);
            bucket = account.BucketName;
            return new Mac(
                account.AccessKeyId,
                account.AccessKeySecret
                );
        }

        public UploadManager GetUploadManager(string repository, out string token)
        {
            var account = Get(repository);
            string bucket;
            var mac = GetMac(repository, out bucket);
            // 上传策略
            PutPolicy putPolicy = new PutPolicy
            {
                Scope = bucket
            };
            // 上传策略的过期时间(单位:秒)
            putPolicy.SetExpires(3600);
            // 生成上传凭证
            token = Auth.createUploadToken(putPolicy, mac);
            return new UploadManager();
        }

        public string ResolveUrl(string path, string repository)
        {
            var account = Get(repository);
            var key = MediaPathUtility.FilePath(path, repository);
            return UrlUtility.ToHttpAbsolute(account.CustomDomain, key);
        }
    }
}
