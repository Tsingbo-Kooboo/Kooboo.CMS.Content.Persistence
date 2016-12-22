using Aliyun.OSS;
using Kooboo.CMS.Caching;
using Kooboo.CMS.Common.Persistence.Non_Relational;
using Kooboo.CMS.Common.Runtime.Dependency;
using Kooboo.CMS.Content.Caching;
using Kooboo.CMS.Content.Models;
using Kooboo.CMS.Content.Persistence.AliyunOSS.Extensions;
using Kooboo.CMS.Content.Persistence.AliyunOSS.Models;
using Kooboo.CMS.Content.Persistence.AliyunOSS.Utilities;
using Kooboo.HealthMonitoring;
using Kooboo.Web.Script.Serialization;
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
                CacheUtility.RemoveCache("Folder" + path, repository);
                return new MediaFolder(new Repository(repository), info.Folder);
            }
        }

        public void Delete(string path, string repository)
        {
            string bucketName;
            var client = _accountService.GetClient(repository, out bucketName);
            var key = MediaPathUtility.FolderPath(path, repository);
            client.DeleteObject(bucketName, key);
            CacheUtility.RemoveCache("Folder" + path, repository);
        }

        public MediaFolder Get(string path, string repository)
        {
            return CacheUtility.GetOrAdd("Folder" + path, repository, () =>
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
                    return null;
                }
            });
        }

        public IEnumerable<MediaFolder> List(string parentFolder, string repository)
        {
            if (string.IsNullOrEmpty(parentFolder) || parentFolder == "/")
            {
                return GetList(new Repository(repository))
                    .Values
                    .Where(it => it.Parent == null)
                    .Select(it => it.AsActual())
                    .Where(it => it != null);
            }
            else
            {
                return GetList(new Repository(repository))
                    .Values
                    .Where(it => it.Parent != null && it.Parent.FullName == parentFolder)
                    .Select(it => it.AsActual())
                    .Where(it => it != null);
            }
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

        private Dictionary<string, MediaFolder> GetList(Repository repository)
        {
            var repositoryName = repository.Name;
            return repository
                .ObjectCache()
                .GetCache($"Aliyun-OSS-MediaFolders-Cachings-{repositoryName}", () =>
                {
                    string bucket;
                    var client = _accountService.GetClient(repositoryName, out bucket);
                    Dictionary<string, MediaFolder> folders = new Dictionary<string, MediaFolder>(StringComparer.OrdinalIgnoreCase);
                    try
                    {
                        var rootKey = MediaPathUtility.FolderPath("/", repositoryName);
                        //var key = $"Media.{repositoryName}.json".ToLower();
                        //if (!client.DoesObjectExist(bucket, key))
                        //{
                        var existFolders = client.ListObjects(new ListObjectsRequest(bucket)
                        {
                            Delimiter = "/",
                            Prefix = rootKey
                        }).CommonPrefixes;
                        foreach (var item in existFolders)
                        {
                            var info = new KoobooMediaFolderInfo(item);
                            folders[info.Folder] = new MediaFolder(repository, info.Folder)
                            {
                                UtcCreationDate = DateTime.UtcNow
                            };
                            using (var stream = new MemoryStream())
                            {
                                client.PutObject(bucket, item, stream);
                            }
                        }
                        //var json = JsonHelper.ToJSON(folders);
                        //var bytes = Encoding.UTF8.GetBytes(json);
                        //using (var stream = new MemoryStream(bytes))
                        //{
                        //    stream.Position = 0;
                        //    client.PutObject(bucket, key, stream);
                        //}
                        return folders;
                        //}
                        //using (var stream = new MemoryStream())
                        //{
                        //    var meta = client.GetObject(new GetObjectRequest(bucket, key), stream);
                        //    stream.Position = 0;
                        //    using (var reader = new StreamReader(stream))
                        //    {
                        //        var json = reader.ReadToEnd();
                        //        folders = JsonHelper.Deserialize<Dictionary<string, MediaFolder>>(json);
                        //    }
                        //}
                    }
                    catch (Exception ex)
                    {
                        Log.LogException(ex);
                    }
                    if (folders == null)
                    {
                        folders = new Dictionary<string, MediaFolder>();
                    }
                    return new Dictionary<string, MediaFolder>(folders, StringComparer.OrdinalIgnoreCase);
                });
        }
    }
}
