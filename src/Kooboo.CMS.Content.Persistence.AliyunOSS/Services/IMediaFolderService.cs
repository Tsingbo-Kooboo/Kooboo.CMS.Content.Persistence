using Aliyun.OSS;
using Kooboo.CMS.Caching;
using Kooboo.CMS.Common.Persistence.Non_Relational;
using Kooboo.CMS.Common.Runtime.Dependency;
using Kooboo.CMS.Content.Caching;
using Kooboo.CMS.Content.Models;
using Kooboo.CMS.Content.Persistence.AliyunOSS.Extensions;
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

        void Update(MediaFolder old, MediaFolder @new);

        void Move(MediaFolder old, MediaFolder @new);
    }

    [Dependency(typeof(IMediaFolderService))]
    public class MediaFolderService : IMediaFolderService
    {
        private readonly IAccountService _accountService;
        private readonly IMediaFileService _fileService;
        public MediaFolderService(
            IAccountService accountService,
            IMediaFileService fileService)
        {
            _accountService = accountService;
            _fileService = fileService;
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
                CacheUtility.RemoveCache(path, repository);
                return new MediaFolder(new Repository(repository), info.Folder);
            }
        }

        public void Delete(string path, string repository)
        {
            string bucketName;
            var client = _accountService.GetClient(repository, out bucketName);
            var key = MediaPathUtility.FolderPath(path, repository);
            client.DeleteObject(bucketName, key);
            CacheUtility.RemoveCache(path, repository);
        }

        public MediaFolder Get(string path, string repository)
        {
            return CacheUtility.GetOrAdd(path, repository, () =>
            {
                string bucketName;
                var client = _accountService.GetClient(repository, out bucketName);
                var key = MediaPathUtility.FolderPath(path, repository);
                try
                {
                    var target = client.GetObject(bucketName, key);
                    return new MediaFolder(new Repository(repository), path)
                    {
                        UtcCreationDate = target.Metadata.LastModified.ToUniversalTime()
                    };
                }
                catch (Exception)
                {

                }
                return null;
            });
        }

        public IEnumerable<MediaFolder> List(string parentFolder, string repository)
        {
            parentFolder = parentFolder.Replace('~', '/');
            return CacheUtility.GetOrAdd(parentFolder, repository, () =>
            {
                string bucketName;
                var client = _accountService.GetClient(repository, out bucketName);
                var key = MediaPathUtility.FolderPath(parentFolder, repository);
                var objects = client.ListObjects(new ListObjectsRequest(bucketName)
                {
                    Delimiter = "/",
                    Prefix = key
                });
                var summaries = objects.ObjectSummaries;
                var res = objects
                    .CommonPrefixes
                    .Select(it =>
                    {
                        var info = new KoobooMediaFolderInfo(it);
                        var m = summaries.FirstOrDefault(o => it.StartsWith(o.Key));
                        return new MediaFolder(new Repository(repository), info.Folder)
                        {
                            UtcCreationDate = m == null ? DateTime.UtcNow : m.LastModified.ToUniversalTime()
                        }.AsActual();
                    }).Where(it => it != null);

                return res;
            });
        }

        public void Move(MediaFolder old, MediaFolder @new)
        {
            throw new NotImplementedException();
        }

        public void Update(MediaFolder old, MediaFolder @new)
        {
            string bucket;
            var client = _accountService.GetClient(old.Repository.Name, out bucket);
            var key = old.GetOssKey();
            var metaData = new ObjectMetadata();
            metaData.AddHeader(ConstValues.Metadata.UserId, @new.UserId);
            metaData.AddHeader(ConstValues.Metadata.AllowedExtensions, string.Join(",", @new.AllowedExtensions));
            client.ModifyObjectMeta(bucket, key, metaData);
        }
    }
}
