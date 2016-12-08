#region License
// 
// Copyright (c) 2013, Kooboo team
// 
// Licensed under the BSD License
// See the file LICENSE.txt for details.
// 
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aliyun.OSS;
using Kooboo.CMS.Content.Models;

namespace Kooboo.CMS.Content.Persistence.AliyunOSS
{
    [Kooboo.CMS.Common.Runtime.Dependency.Dependency(typeof(ITextContentFileProvider), Order = 2)]
    public class TextContentFileProvider : ITextContentFileProvider
    {
        private readonly string bucket;
        private readonly OssClient ossClient;
        public TextContentFileProvider()
        {
            var account = OssAccountHelper.GetOssClientBucket(Repository.Current);
            ossClient = account.Item1;
            bucket = account.Item2;
        }

        public string Save(TextContent content, ContentFile file)
        {
            var key = content.GetTextContentFilePath(file);
            ossClient.PutObject(bucket, key, file.Stream);
            return OssAccountHelper.GetUrl(content.GetRepository(), key);
        }

        public void DeleteFiles(TextContent content)
        {
            var prefix = content.GetTextContentDirectoryPath();
            var keys = ossClient.ListBlobsWithPrefix(bucket, prefix)
                .ObjectSummaries
                .Select(it => it.Key);
            if (keys != null && keys.Any())
            {
                ossClient.DeleteObjects(new DeleteObjectsRequest(bucket, keys.ToList()));
            }
        }
    }
}
