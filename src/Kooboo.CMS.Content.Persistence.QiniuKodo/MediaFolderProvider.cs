using Kooboo.CMS.Common.Persistence.Non_Relational;
using Kooboo.CMS.Common.Runtime.Dependency;
using Kooboo.CMS.Content.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Kooboo.CMS.Content.Persistence.QiniuKodo.Utilities;
using Kooboo.CMS.Content.Persistence.QiniuKodo.Services;
using Kooboo.CMS.Content.Persistence.QiniuKodo.Models;
using Kooboo.CMS.Content.Caching;
using Kooboo.CMS.Caching;
using Kooboo.HealthMonitoring;
using Kooboo.Web.Script.Serialization;
using Qiniu.Storage;
using System.Net;

namespace Kooboo.CMS.Content.Persistence.QiniuKodo
{
    [Dependency(typeof(IMediaFolderProvider), Order = 10)]
    [Dependency(typeof(IProvider<MediaFolder>), Order = 10)]
    public class MediaFolderProvider : IMediaFolderProvider
    {
        private readonly IAccountService _accountService;
        public MediaFolderProvider(IAccountService accountService)
        {
            _accountService = accountService;
        }

        public void Add(MediaFolder item)
        {
            var repository = item.Repository;
            item.UtcCreationDate = DateTime.UtcNow;
            var list = GetList(repository);
            list[item.FullName] = item;
            Save(item.Repository, list);
            var cacheKey = string.Format(CacheKeyFormat, repository.Name);
            repository.ObjectCache().Remove(cacheKey);
        }

        public IEnumerable<MediaFolder> All()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<MediaFolder> All(Repository repository)
        {
            return GetList(repository)
                .Values
                .Select(it => it.AsActual())
                .Where(it => it != null);
        }

        public IQueryable<MediaFolder> ChildFolders(MediaFolder parent)
        {
            return GetList(parent.Repository)
                .Values
                .Where(it => it.Parent != null && it.Parent == parent)
                //.Select(it => it.AsActual())
                //.Where(it => it != null)
                .AsQueryable();
        }

        public void Export(
            Repository repository,
            IEnumerable<MediaFolder> models,
            Stream outputStream)
        {
            throw new NotImplementedException();
        }

        public void Export(
            Repository repository,
            string baseFolder,
            string[] folders,
            string[] docs,
            Stream outputStream)
        {
            throw new NotImplementedException();
        }

        public MediaFolder Get(MediaFolder dummy)
        {
            return GetList(dummy.Repository)
                .Values
                .FirstOrDefault(it => it.FullName == dummy.FullName);
        }

        public void Import(Repository repository, MediaFolder folder, Stream zipStream, bool @override)
        {
            throw new NotImplementedException();
        }

        public void Remove(MediaFolder item)
        {
            throw new NotImplementedException();
        }

        public void Rename(MediaFolder @new, MediaFolder old)
        {
            throw new NotImplementedException();
        }

        public void Update(MediaFolder @new, MediaFolder old)
        {
            throw new NotImplementedException();
        }

        private void Save(Repository repository, Dictionary<string, MediaFolder> folders)
        {
            var repositoryName = repository.Name;
            var configKey = $"Media.{repositoryName}.json".ToLower();
            string token;
            var uploader = _accountService.GetUploadManager(repositoryName, out token);
            var json = folders.ToJSON();
            var bytes = Encoding.UTF8.GetBytes(json);
            uploader.uploadData(bytes, configKey, token, null, null);
        }
        const string CacheKeyFormat = "Qiniu-Kodo-MediaFolders-Cachings-{0}";
        private Dictionary<string, MediaFolder> GetList(Repository repository)
        {
            var repositoryName = repository.Name;
            return repository
                .ObjectCache()
                .GetCache(string.Format(CacheKeyFormat, repositoryName), () =>
                 {
                     Dictionary<string, MediaFolder> folders = new Dictionary<string, MediaFolder>(StringComparer.OrdinalIgnoreCase);
                     try
                     {
                         var configKey = $"Media.{repositoryName}.json".ToLower();
                         string bucket;
                         var client = _accountService.GetBucketManager(repositoryName, out bucket);
                         var res = client.stat(bucket, configKey);
                         if (!res.ResponseInfo.isOk())
                         {
                             var key = MediaPathUtility.FolderPath("/", repositoryName);
                             var result = client.listFiles(bucket, key, "", 100, "/");
                             var folderKeys = result.CommonPrefixes;
                             if (folderKeys != null)
                             {
                                 foreach (var it in folderKeys)
                                 {
                                     var info = new KoobooMediaFolderInfo(it);
                                     folders[info.Folder] = new MediaFolder(repository, info.Folder)
                                     {
                                         UtcCreationDate = DateTime.UtcNow
                                     };
                                 }
                             }
                         }
                         else
                         {
                             var url = _accountService.AbsoluteUrl(configKey, repositoryName);
                             using (var wc = new WebClient())
                             {
                                 return JsonHelper.Deserialize<Dictionary<string, MediaFolder>>(wc.DownloadString(url));
                             }
                         }
                         return folders;
                     }
                     catch (Exception ex)
                     {
                         Log.LogException(ex);
                     }
                     return new Dictionary<string, MediaFolder>(folders, StringComparer.OrdinalIgnoreCase);
                 });
        }
    }
}
