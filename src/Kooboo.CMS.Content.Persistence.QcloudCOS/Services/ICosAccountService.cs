using Kooboo.CMS.Common.Runtime.Dependency;
using Kooboo.CMS.Content.Persistence.QcloudCOS.Models;
using Kooboo.Web.Url;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kooboo.CMS.Content.Persistence.QcloudCOS.Services
{
    public interface ICosAccountService
    {
        CosAccount Get(string repository);
        string ResourceUrl(string repository, string url);
    }

    [Dependency(typeof(ICosAccountService))]
    public class CosAccountService : ICosAccountService
    {
        public CosAccount Get(string repository)
        {
            var config = QcloudCOSAccountSettings.Instance;
            var bucket = config.BucketName;
            var customDomain = config.CustomDomain;
            var appId = config.AppId;
            var sub = config
                .RepositoryBuckets
                .FirstOrDefault(it => it.RepositoryName.Equals(repository, StringComparison.OrdinalIgnoreCase));
            if (sub != null)
            {
                bucket = sub.BucketName;
                customDomain = sub.CustomDomain;
                appId = sub.AppId;
            }
            return new CosAccount
            {
                AccessKeyId = config.AccessKeyId,
                AccessKeySecret = config.AccessKeySecret,
                BucketName = bucket,
                CustomDomain = customDomain,
                AppId = appId
            };
        }

        /// <summary>
        /// 外网可访问的地址
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        public string ResourceUrl(string repository, string url)
        {
            var account = Get(repository);
            var customUri = new Uri(account.CustomDomain, UriKind.Absolute);
            var path = url;
            if (UrlUtility.IsAbsoluteUrl(url))
            {
                path = new Uri(url, UriKind.Absolute).LocalPath;
            }
            var newUri = new UriBuilder(customUri.Scheme, customUri.Host, customUri.Port, path);
            return newUri.Uri.AbsoluteUri;
        }
    }
}
