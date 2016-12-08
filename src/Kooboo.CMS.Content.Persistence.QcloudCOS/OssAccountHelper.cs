#region License
// 
// Copyright (c) 2013, Kooboo team
// 
// Licensed under the BSD License
// See the file LICENSE.txt for details.
// 
#endregion
using Kooboo.CMS.Content.Models;
using QCloud.CosApi.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
namespace Kooboo.CMS.Content.Persistence.QcloudCOS
{
    internal static class OssAccountHelper
    {
        /// <summary>
        /// item1: OssClient
        /// item2: BucketName
        /// item3: Domain
        /// </summary>
        /// <param name="repository"></param>
        /// <returns></returns>
        public static Tuple<CosCloud, string, string> GetOssClientBucket(Repository repository)
        {
            var account = QcloudCOSAccountSettings.Instance;
            var client = new CosCloud(account.AppId, account.AccessKeyId, account.AccessKeySecret);
            string bucket = account.BucketName;
            string domain = account.CustomDomain;
            if (repository != null)
            {
                var config = account.RepositoryBuckets
                    .FirstOrDefault(it => it.RepositoryName.Equals(repository.Name, StringComparison.OrdinalIgnoreCase));
                if (config != null)
                {
                    client = new CosCloud(config.AppId, account.AccessKeyId, account.AccessKeySecret);
                    bucket = config.BucketName;
                    if (!string.IsNullOrEmpty(config.CustomDomain))
                    {
                        domain = config.CustomDomain;
                    }
                }
            }
            bucket = StorageNamesEncoder.EncodeContainerName(bucket);
            return new Tuple<CosCloud, string, string>(client, bucket, domain);
        }

        public static string GetUrl(Repository repository, OssObject blob)
        {
            return blob.Data.SourceUrl;
        }
    }
}
