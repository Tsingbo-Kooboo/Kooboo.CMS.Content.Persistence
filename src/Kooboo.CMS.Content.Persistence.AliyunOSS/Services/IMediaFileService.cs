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

namespace Kooboo.CMS.Content.Persistence.AliyunOSS.Services
{
    public interface IMediaFileService
    {
        MediaContent Create(string path, string repository, Stream stream, bool overwrite = true);

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

        public MediaContent Create(string path, string repository, Stream stream, bool overwrite = true)
        {
            var folder = Path.GetDirectoryName(path);
            var fileName = Path.GetFileName(path);
            string bucketName;
            var client = _accountService.GetClient(repository, out bucketName);
            var key = MediaPathUtility.FilePath(path, repository);
            var response = client.PutObject(bucketName, key, stream);
            return new MediaContent(repository, folder)
            {
                VirtualPath = _accountService.ResolveUrl(path, repository),
                ContentFile = new ContentFile
                {
                    FileName = fileName,
                    Name = fileName,
                    Stream = stream
                }
            };
        }

        public void Delete(string path, string repository)
        {
            string bucketName;
            var client = _accountService.GetClient(repository, out bucketName);
            var key = MediaPathUtility.FilePath(path, repository);
            client.DeleteObject(bucketName, key);
        }

        public MediaContent Get(string path, string repository)
        {
            string bucketName;
            var fileName = Path.GetFileName(path);
            var folderName = Path.GetDirectoryName(path);
            var client = _accountService.GetClient(repository, out bucketName);
            var key = MediaPathUtility.FilePath(path, repository);
            var stream = new MemoryStream();
            var metaData = client.GetObject(new GetObjectRequest(bucketName, key), stream);
            return new MediaContent(repository, folderName)
            {
                VirtualPath = _accountService.ResolveUrl(path, repository),
                ContentFile = new ContentFile
                {
                    FileName = fileName,
                    Name = fileName,
                    Stream = stream
                }
            };
        }

        public IEnumerable<MediaContent> List(string folder, string repository, int count = 100)
        {
            string bucketName;
            var client = _accountService.GetClient(repository, out bucketName);
            return client.ListObjects(new ListObjectsRequest(bucketName)
            {
                Delimiter = "/",
                Prefix = folder,
                MaxKeys = count
            }).ObjectSummaries
            .Select(it => it.ToMediaContent(_accountService));
        }

        public void Move(string oldPath, string oldRepository, string newPath, string newRepository = null)
        {
            throw new NotImplementedException();
        }

        public void Update(string path, string repository, Dictionary<string, string> headers)
        {
            throw new NotImplementedException();
        }
    }
}
