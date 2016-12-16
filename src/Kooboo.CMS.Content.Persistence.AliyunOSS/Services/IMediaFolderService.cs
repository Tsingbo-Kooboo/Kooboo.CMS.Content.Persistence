using Aliyun.OSS;
using Kooboo.CMS.Common.Runtime.Dependency;
using Kooboo.CMS.Content.Models;
using Kooboo.CMS.Content.Persistence.AliyunOSS.Models;
using Kooboo.CMS.Content.Persistence.AliyunOSS.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kooboo.CMS.Content.Persistence.AliyunOSS.Services
{
    public interface IMediaFolderService
    {
        MediaFolder Create(string path, string repository);

        IEnumerable<MediaFolder> List(string parentFolder, string repository);

        MediaFolder Get(string path, string repository);

        void Delete(string path, string repository);
    }
    [Dependency(typeof(IMediaFolderService))]
    public class MediaFolderService : IMediaFolderService
    {
        private readonly IAccountService _accountService;
        public MediaFolderService(IAccountService accountService)
        {
            _accountService = accountService;
        }

        public MediaFolder Create(string path, string repository)
        {
            string bucketName;
            var client = _accountService.GetClient(repository, out bucketName);
            using (var stream = new MemoryStream())
            {
                var key = MediaPathUtility.FolderPath(path, repository);
                var folder = client.PutObject(bucketName, key, stream);
                var info = new KoobooMediaFolderInfo(key);
                return new MediaFolder(new Repository(repository), info.Folder);
            }
        }

        public void Delete(string path, string repository)
        {
            string bucketName;
            var client = _accountService.GetClient(repository, out bucketName);
            var key = MediaPathUtility.FolderPath(path, repository);
            client.DeleteObject(bucketName, key);
        }

        public MediaFolder Get(string path, string repository)
        {
            return new MediaFolder(new Repository(repository), path);
        }

        public IEnumerable<MediaFolder> List(string parentFolder, string repository)
        {
            string bucketName;
            var client = _accountService.GetClient(repository, out bucketName);
            var key = MediaPathUtility.FolderPath(parentFolder, repository);
            var objects = client.ListObjects(new ListObjectsRequest(bucketName)
            {
                Delimiter = "/",
                Prefix = key
            });
            return objects
                .CommonPrefixes
                .Select(it =>
                {
                    var info = new KoobooMediaFolderInfo(it);
                    return new MediaFolder(new Repository(repository), info.Folder);
                });
        }
    }
}
