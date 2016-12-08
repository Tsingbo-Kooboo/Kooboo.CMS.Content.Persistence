#region License
// 
// Copyright (c) 2013, Kooboo team
// 
// Licensed under the BSD License
// See the file LICENSE.txt for details.
// 
#endregion
using Aliyun.OSS;
using Kooboo.CMS.Content.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
namespace Kooboo.CMS.Content.Persistence.AliyunOSS
{
    public static class OssAccountHelper
    {
        /// <summary>
        /// item1: OssClient
        /// item2: BucketName
        /// item3: Domain
        /// </summary>
        /// <param name="repository"></param>
        /// <returns></returns>
        public static Tuple<OssClient, string, string> GetOssClientBucket(Repository repository)
        {
            var account = AliyunAccountSettings.Instance;
            OssClient client = new OssClient(account.Endpoint, account.AccessKeyId, account.AccessKeySecret);
            string bucket = account.BucketName;
            string domain = account.CustomDomain;
            if (repository != null)
            {
                var config = account.RepositoryBuckets
                    .FirstOrDefault(it => it.RepositoryName.Equals(repository.Name, StringComparison.OrdinalIgnoreCase));
                if (config != null)
                {
                    client = new OssClient(config.Endpoint, account.AccessKeyId, account.AccessKeySecret);
                    bucket = config.BucketName;
                    if (!string.IsNullOrEmpty(config.CustomDomain))
                    {
                        domain = config.CustomDomain;
                    }
                }
            }
            bucket = StorageNamesEncoder.EncodeContainerName(bucket);
            return new Tuple<OssClient, string, string>(client, bucket, domain);
        }

        public static string GetUrl(Repository repository, OssObject blob)
        {
            return GetUrl(repository, blob.Key);
        }

        public static string GetUrl(Repository repository, string key)
        {
            var account = GetOssClientBucket(repository);
            if (string.IsNullOrEmpty(account.Item3))
            {
                return account.Item1.GeneratePresignedUri(account.Item2, key)?.AbsoluteUri?.Split('?')[0];
            }
            return Kooboo.Web.Url.UrlUtility.ToHttpAbsolute(account.Item3, key);
        }
    }
}
