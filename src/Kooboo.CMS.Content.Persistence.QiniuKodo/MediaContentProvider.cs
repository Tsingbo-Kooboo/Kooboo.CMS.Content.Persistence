using Kooboo.CMS.Content.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kooboo.CMS.Content.Query;
using System.IO;
using Kooboo.CMS.Common.Runtime.Dependency;
using Kooboo.CMS.Content.Persistence.QiniuKodo.Services;
using Qiniu.Util;
using Qiniu.Storage;
using Kooboo.CMS.Content.Persistence.QiniuKodo.Extensions;
using Kooboo.IO;
using Kooboo.CMS.Content.Query.Expressions;
using System.Net;
using Kooboo.Web.Url;

namespace Kooboo.CMS.Content.Persistence.QiniuKodo
{
    [Dependency(typeof(IMediaContentProvider), Order = 10)]
    [Dependency(typeof(IContentProvider<MediaContent>), Order = 10)]
    public class MediaContentProvider : IMediaContentProvider
    {
        private readonly IAccountService _accountService;
        public MediaContentProvider(IAccountService accountService)
        {
            _accountService = accountService;
        }

        public void Add(MediaContent content)
        {
            Add(content, true);
        }

        public void Add(MediaContent content, bool overrided)
        {
            string token;
            var um = _accountService.GetUploadManager(content.Repository, out token);
            var headers = new Dictionary<string, string>();
            foreach (var item in content)
            {
                headers[item.Key] = item.Value?.ToString();
            }
            var options = new UploadOptions(headers,
                IOUtility.MimeType(content.FileName),
                false,
                (key, progress) =>
                {

                },
                null);
            um.uploadStream(content.ContentFile.Stream, content.GetMediaKey(), token, options, null);
            var path = content.GetMediaPath();
            content.VirtualPath = _accountService.ResolveUrl(path, content.Repository);
        }

        public void Delete(MediaContent content)
        {
            string bucket;
            var mac = _accountService.GetMac(content.Repository, out bucket);
            BucketManager bm = new BucketManager(mac);
            bm.delete(bucket, content.GetMediaKey());
        }

        public object Execute(IContentQuery<MediaContent> query)
        {
            var mediaQuery = (MediaContentQuery)query;

            QueryExpressionTranslator translator = new QueryExpressionTranslator();

            var blobs = translator
                .Translate(query.Expression, mediaQuery.MediaFolder, _accountService)
                .Where(it => it != null);

            foreach (var item in translator.OrderFields)
            {
                if (item.Descending)
                {
                    blobs = blobs.OrderByDescending(it => it.GetType().GetProperty(item.FieldName).GetValue(it, null));
                }
                else
                {
                    blobs = blobs.OrderBy(it => it.GetType().GetProperty(item.FieldName).GetValue(it, null));
                }
            }

            switch (translator.CallType)
            {
                case CallType.Count:
                    return blobs.Count();
                case CallType.First:
                    return blobs.First();
                case CallType.Last:
                    return blobs.Last();
                case CallType.LastOrDefault:
                    return blobs.LastOrDefault();
                case CallType.FirstOrDefault:
                    return blobs.FirstOrDefault();
                case CallType.Unspecified:
                default:
                    return blobs;
            }
        }

        public byte[] GetContentStream(MediaContent content)
        {
            var client = new WebClient();
            return client.DownloadData(content.Url);
        }

        public void InitializeMediaContents(Repository repository)
        {
            //_folderService.Create("/", repository.Name);

            //Default.MediaFolderProvider fileMediaFolderProvider = new Default.MediaFolderProvider();
            //foreach (var item in fileMediaFolderProvider.All(repository))
            //{
            //    ImportMediaFolderDataCascading(fileMediaFolderProvider.Get(item));
            //}
        }

        public void Move(MediaFolder sourceFolder, string oldFileName, MediaFolder targetFolder, string newFileName)
        {
            string bucket;
            var mac = _accountService.GetMac(sourceFolder.Repository.Name, out bucket);
            BucketManager bm = new BucketManager(mac);
            var oldKey = UrlUtility.Combine(sourceFolder.GetMediaKey(), oldFileName);
            var newKey = UrlUtility.Combine(targetFolder.GetMediaKey(), newFileName);
            bm.move(bucket, oldKey, bucket, newKey, true);
        }

        public void SaveContentStream(MediaContent content, Stream stream)
        {
            if (stream.Length == 0)
            {
                return;
            }
            stream.Position = 0;
            content.ContentFile = new ContentFile
            {
                Name = content.FileName,
                FileName = content.FileName,
                Stream = stream
            };
        }

        public void Update(MediaContent @new, MediaContent old)
        {
            throw new NotImplementedException();
        }
    }
}
