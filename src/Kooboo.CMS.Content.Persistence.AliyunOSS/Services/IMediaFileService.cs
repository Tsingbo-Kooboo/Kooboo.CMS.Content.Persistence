using Aliyun.OSS;
using Kooboo.CMS.Common.Runtime.Dependency;
using Kooboo.CMS.Content.Models;
using Kooboo.CMS.Content.Persistence.AliyunOSS.Utilities;
using Kooboo.CMS.Content.Persistence.AliyunOSS.Extensions;
using Kooboo.Web.Url;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kooboo.IO;
using Kooboo.CMS.Content.Persistence.AliyunOSS.Models;

namespace Kooboo.CMS.Content.Persistence.AliyunOSS.Services
{
    public interface IMediaFileService
    {
        void Create(string path, string repository, Stream stream, Dictionary<string, string> metadata, bool overwrite = true);

        MediaContent Get(string path, string repository);

        IEnumerable<MediaContent> List(string folder, string repository, int count = 100);

        void Move(string oldPath, string oldRepository, string newPath, string newRepository = null);

        void Delete(string path, string repository);

        void Update(string path, string repository, Dictionary<string, string> headers);
    }

    [Dependency(typeof(IMediaFileService))]
    public class MediaFileService : IMediaFileService
    {
        private readonly IAccountService _accountService;
        public MediaFileService(IAccountService accountService)
        {
            _accountService = accountService;
        }

        public void Create(string path,
            string repository,
            Stream stream,
            Dictionary<string, string> metadata,
            bool overwrite = true)
        {
            var folder = Path.GetDirectoryName(path);
            var fileName = Path.GetFileName(path);
            string bucketName;
            var client = _accountService.GetClient(repository, out bucketName);
            var key = MediaPathUtility.FilePath(path, repository);
            var meta = new ObjectMetadata();
            if (metadata != null)
            {
                foreach (var item in metadata)
                {
                    meta.UserMetadata[item.Key] = item.Value;
                }
            }
            var response = client.PutObject(bucketName, key, stream, meta);
            CacheUtility.RemoveCache("Get" + path, repository);
        }

        public void Delete(string path, string repository)
        {
            string bucketName;
            var client = _accountService.GetClient(repository, out bucketName);
            var key = MediaPathUtility.FilePath(path, repository);
            client.DeleteObject(bucketName, key);
            CacheUtility.RemoveCache("Get" + path, repository);
        }

        public MediaContent Get(string path, string repository)
        {
            return CacheUtility.GetOrAdd("Get" + path, repository, () =>
            {
                string bucketName;
                var fileName = Path.GetFileName(path);
                var folderName = Path.GetDirectoryName(path);
                var client = _accountService.GetClient(repository, out bucketName);
                var key = MediaPathUtility.FilePath(path, repository);
                var stream = new MemoryStream();
                var metaData = client.GetObject(new GetObjectRequest(bucketName, key), stream);
                var userMetaData = metaData.UserMetadata;
                return new MediaContent(repository, folderName)
                {
                    UtcLastModificationDate = metaData.LastModified.ToUniversalTime(),
                    UtcCreationDate = metaData.LastModified.ToUniversalTime(),
                    Metadata = new MediaContentMetadata
                    {
                        Title = userMetaData.GetValueOrDefault(ConstValues.Metadata.Title),
                        AlternateText = userMetaData.GetValueOrDefault(ConstValues.Metadata.AlternateText),
                        Description = userMetaData.GetValueOrDefault(ConstValues.Metadata.Description)
                    },
                    UserId = userMetaData.GetValueOrDefault(ConstValues.Metadata.UserId),
                    UserKey = fileName,
                    UUID = fileName,
                    Size = stream.Length,
                    VirtualPath = _accountService.ResolveUrl(path, repository),
                    ContentFile = new ContentFile
                    {
                        FileName = fileName,
                        Name = fileName,
                        Stream = stream
                    }
                };
            });
        }

        public IEnumerable<MediaContent> List(string folder, string repository, int count = 100)
        {
            return CacheUtility.GetOrAdd("List" + folder, repository, () =>
            {
                string bucketName;
                var client = _accountService.GetClient(repository, out bucketName);
                var mediaFolder = new MediaFolder(new Repository(repository), folder);
                return client.ListBlobsInFolder(bucketName, mediaFolder)
                    .Select(it => it.ToMediaContent(_accountService));
            });
        }

        public void Move(string oldPath, string oldRepository, string newPath, string newRepository = null)
        {
            throw new NotImplementedException();
        }

        public void Update(string path, string repository, Dictionary<string, string> headers)
        {
            string bucketName;
            var key = MediaPathUtility.FilePath(path, repository);
            var client = _accountService.GetClient(repository, out bucketName);
            var meta = new ObjectMetadata();
            foreach (var item in headers)
            {
                meta.AddHeader(item.Key, item.Value);
            }
            client.ModifyObjectMeta(bucketName, key, meta);
        }
    }
}
