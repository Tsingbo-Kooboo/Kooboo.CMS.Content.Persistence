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
using Ionic.Zip;
using Kooboo.Web.Url;
using Kooboo.CMS.Content.Services;

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
            SaveList(item.Repository, list);
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
                .Where(it => it != null && it.Parent == null);
        }

        public IQueryable<MediaFolder> ChildFolders(MediaFolder parent)
        {
            return GetList(parent.Repository)
                .Values
                .Where(it => it.Parent != null && it.Parent == parent)
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
            string bucket;
            var manager = _accountService.GetBucketManager(repository.Name, out bucket);
            ZipFile zipFile = new ZipFile();
            var repositoryName = repository.Name;
            var basePrefix = MediaPathUtility.FolderPath("/", repositoryName);
            if (!string.IsNullOrEmpty(baseFolder))
            {
                var baseMediaFolder = ServiceFactory.MediaFolderManager.Get(repository, baseFolder);
                basePrefix = MediaPathUtility.FolderPath(baseMediaFolder.FullName, repositoryName);
            }

            //add file
            if (docs != null)
            {
                foreach (var doc in docs)
                {
                    var path = UrlUtility.Combine(basePrefix, doc);
                    var url = _accountService.AbsoluteUrl(path, repository.Name);
                    using (var wc = new WebClient())
                    {
                        var bytes = wc.DownloadData(url);
                        zipFile.AddEntry(doc, bytes);
                    }
                }
            }
            //add folders
            if (folders != null)
            {
                foreach (var folder in folders)
                {
                    var folderName = folder.Split('~').LastOrDefault();
                    zipFolder(repository, basePrefix, folderName, "", ref zipFile);
                }
            }
            zipFile.Save(outputStream);
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
            var repository = item.Repository;
            var list = GetList(repository);
            if (list.Remove(item.FullName))
            {
                SaveList(repository, list);
            }
        }

        public void Rename(MediaFolder @new, MediaFolder old)
        {
            var repository = old.Repository;
            var list = GetList(repository);
            if (list.Remove(old.FullName))
            {
                list[@new.FullName] = @new;
                SaveList(repository, list);
                //TODO: 移动文件
            }
        }

        public void Update(MediaFolder @new, MediaFolder old)
        {
            var repository = old.Repository;
            var list = GetList(repository);
            list[old.FullName] = @new;
            SaveList(repository, list);
        }

        private void SaveList(Repository repository, Dictionary<string, MediaFolder> folders)
        {
            var repositoryName = repository.Name;
            var configKey = $"Media.{repositoryName}.json".ToLower();
            var json = folders.ToJSON();
            var bytes = Encoding.UTF8.GetBytes(json);
            string token;
            var uploader = _accountService.GetUploadManager(repositoryName, out token, configKey);
            uploader.uploadData(bytes, configKey, token, null, null);
            var cacheKey = string.Format(CacheKeyFormat, repositoryName);
            repository.ObjectCache().Remove(cacheKey);
        }

        private void zipFolder(
           Repository repository,
           string basePrefix,
           string folderName,
           string zipDir,
           ref ZipFile zipFile)
        {
            zipDir = string.IsNullOrEmpty(zipDir) ? folderName : zipDir + "/" + folderName;
            zipFile.AddDirectoryByName(zipDir);
            var folderPrefix = UrlUtility.Combine(basePrefix, folderName).Trim('/') + "/";
            string bucket;
            var client = _accountService.GetBucketManager(repository.Name, out bucket);
            var blobs = client.listFiles(bucket, folderPrefix, "", 100, "/");
            var len = folderPrefix.Length;
            foreach (var blob in blobs.Items)
            {
                if (blob.Key.EndsWith("/"))
                {
                    continue;
                }
                using (var stream = new MemoryStream())
                {
                    var k = blob.Key;
                    //client.GetObject(new GetObjectRequest(bucket, blob.Key), stream);
                    //stream.Position = 0;
                    //var bytes = stream.ReadData();
                    //if (bytes.Length > 0)
                    //{
                    //    var key = UrlUtility.Combine(zipDir, blob.Key.Substring(len));
                    //    zipFile.AddEntry(key, bytes);
                    //}
                }
            }
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
                             var url = _accountService.AbsoluteUrl(configKey, repositoryName).AddQueryParam("t", DateTime.Now.Ticks.ToString());
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
