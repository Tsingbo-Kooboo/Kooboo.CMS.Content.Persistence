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
using Kooboo.CMS.Common.Runtime.Dependency;
using Kooboo.CMS.Content.Persistence.AliyunOSS.Services;

namespace Kooboo.CMS.Content.Persistence.AliyunOSS
{
    [Dependency(typeof(ITextContentFileProvider), Order = 2)]
    public class TextContentFileProvider : ITextContentFileProvider
    {
        private readonly IAccountService _accountService;
        public TextContentFileProvider(IAccountService accountService)
        {
            _accountService = accountService;
        }

        public string Save(TextContent content, ContentFile file)
        {
            var key = content.GetTextContentFilePath(file);
            string bucket;
            var client = _accountService.GetClient(content.Repository, out bucket);
            client.PutObject(bucket, key, file.Stream);
            return _accountService.AbsoluteUrl(key, content.Repository);
        }

        public void DeleteFiles(TextContent content)
        {
            string bucket;
            var client = _accountService.GetClient(content.Repository, out bucket);

            var prefix = content.GetTextContentDirectoryPath();
            var keys = client.ListBlobsWithPrefix(bucket, prefix)
                .ObjectSummaries
                .Select(it => it.Key);
            if (keys != null && keys.Any())
            {
                client.DeleteObjects(new DeleteObjectsRequest(bucket, keys.ToList()));
            }
        }
    }
}
