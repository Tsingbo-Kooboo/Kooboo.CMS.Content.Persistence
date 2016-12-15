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
using Kooboo.CMS.Content.Models;
using Ionic.Zip;
using Kooboo.CMS.Content.Services;
using System.IO;
using Kooboo.CMS.Common.Persistence.Non_Relational;
using Kooboo.Web.Url;
using Kooboo.CMS.Content.Caching;
using Kooboo.CMS.Caching;
using Kooboo.IO;
using Kooboo.CMS.Common.Runtime.Dependency;
using Kooboo.CMS.Content.Persistence.QcloudCOS.Services;
using System.Threading;
using Kooboo.HealthMonitoring;
using Kooboo.CMS.Content.Persistence.QcloudCOS.Models;
using Kooboo.Web.Script.Serialization;
using Kooboo.CMS.Content.Persistence.QcloudCOS.Extensions;

namespace Kooboo.CMS.Content.Persistence.QcloudCOS
{
    public partial class MediaFolderProvider
    {
        private IEnumerable<MediaFolder> ToMediaFolders(Repository repository, Dictionary<string, CosFolderData> folders)
        {
            return folders.Select(it => ToMediaFolder(repository, it.Key, it.Value)).ToArray();
        }

        private MediaFolder ToMediaFolder(Repository repository, string fullName, CosFolderData folderProperties)
        {
            var json = JsonHelper.Deserialize<Dictionary<string, string>>(folderProperties.biz_attr);

            return new MediaFolder(repository, fullName)
            {
                DisplayName = folderProperties.name,
                UserId = json.GetString("UserId"),
                UtcCreationDate = json.GetDateTime("UtcCreationDate", DateTime.UtcNow),
                AllowedExtensions = new string[] { }
            };
        }

        private Dictionary<string, CosFolderData> GetList(Repository repository)
        {
            var mediaFolders = repository
                .ObjectCache()
                .GetCache($"Qcloud-COS-MediaFolders-Cachings-{repository.Name}", () =>
                {
                    Dictionary<string, CosFolderData> folders = null;
                    try
                    {
                        var folderList = _folderService.List("/", repository.Name);
                        folders = folderList.data.infos.ToDictionary(it => it.name, it => it);
                    }
                    catch (Exception ex)
                    {
                        Log.LogException(ex);
                    }
                    if (folders == null)
                    {
                        folders = new Dictionary<string, CosFolderData>();
                    }
                    return new Dictionary<string, CosFolderData>(folders, StringComparer.OrdinalIgnoreCase);
                });
            return mediaFolders;
        }
    }

    [Dependency(typeof(IMediaFolderProvider), Order = 2)]
    [Dependency(typeof(IProvider<MediaFolder>), Order = 2)]
    public partial class MediaFolderProvider : IMediaFolderProvider
    {
        static ReaderWriterLockSlim locker = new ReaderWriterLockSlim();

        private readonly ICosAccountService _accountService;
        private readonly ICosFolderService _folderService;
        private readonly ICosFileService _fileService;
        public MediaFolderProvider(ICosAccountService accountService,
            ICosFolderService folderService,
            ICosFileService fileService)
        {
            _accountService = accountService;
            _folderService = folderService;
            _fileService = fileService;
        }

        public IQueryable<MediaFolder> ChildFolders(MediaFolder parent)
        {
            locker.EnterReadLock();
            try
            {
                var query = ToMediaFolders(parent.Repository, GetList(parent.Repository));
                //loop bug in azure
                query = query.Where(it => (parent == null && it.Parent == null) || (it.Parent != null && it.Parent.UUID == parent.UUID));
                return query.AsQueryable();
            }
            finally
            {
                locker.ExitReadLock();
            }
        }

        public IEnumerable<MediaFolder> All(Repository repository)
        {
            return ToMediaFolders(repository, GetList(repository));
        }

        public MediaFolder Get(MediaFolder dummy)
        {
            locker.EnterReadLock();
            try
            {
                var folders = GetList(dummy.Repository);
                if (folders.ContainsKey(dummy.FullName))
                {
                    return ToMediaFolder(dummy.Repository, dummy.FullName, folders[dummy.FullName]);
                }
                return null;
            }
            finally
            {
                locker.ExitReadLock();
            }
        }

        public void Add(MediaFolder folder)
        {
            locker.EnterWriteLock();
            try
            {
                var folders = GetList(folder.Repository);
                var name = folder.FullName.Trim('~');
                if (!folders.ContainsKey(name))
                {
                    var newFolder = _folderService.Create(name, folder.Repository.Name);
                }
            }
            finally
            {
                locker.ExitWriteLock();
            }
        }

        public void Update(MediaFolder @new, MediaFolder old)
        {
            throw new NotImplementedException();
        }

        public void Remove(MediaFolder item)
        {
            locker.EnterWriteLock();
            try
            {
                _folderService.Delete(item.FullName.Trim('~'), item.Repository.Name);
            }
            finally
            {
                locker.ExitWriteLock();
            }
        }

        public void Export(Repository repository, IEnumerable<MediaFolder> models, Stream outputStream)
        {
            throw new NotImplementedException();
        }

        public void Import(Repository repository, MediaFolder folder, Stream zipStream, bool @override)
        {
            using (ZipFile zipFile = ZipFile.Read(zipStream))
            {
                foreach (ZipEntry item in zipFile)
                {
                    if (item.IsDirectory)
                    {

                    }
                    else
                    {
                        var path = Path.GetDirectoryName(item.FileName);
                        if (!string.IsNullOrEmpty(path))
                        {
                            path = string.Join("~", path.Split('\\').ToArray());
                        }
                        var fileName = Path.GetFileName(item.FileName);
                        if (fileName.ToLower() != "setting.config")
                        {
                            var currentFolder = CreateMediaFolderByPath(folder, path);
                            Add(currentFolder);
                            var stream = new MemoryStream();
                            item.Extract(stream);
                            stream.Position = 0;
                            ServiceFactory.MediaContentManager.Add(repository, currentFolder,
                                fileName, stream, true);
                        }
                    }
                }
            }
        }
        private MediaFolder CreateMediaFolderByPath(MediaFolder folder, string pathName)
        {
            return new MediaFolder(folder.Repository, pathName, folder);
        }

        public IEnumerable<MediaFolder> All()
        {
            throw new NotImplementedException();
        }

        public void Rename(MediaFolder @new, MediaFolder old)
        {
            throw new NotImplementedException();
        }

        public void Export(Repository repository, string baseFolder, string[] folders, string[] docs, Stream outputStream)
        {
            throw new NotImplementedException();

            //ZipFile zipFile = new ZipFile();
            //var basePrefix = StorageNamesEncoder.EncodeContainerName(repository.Name) + "/" + MediaBlobHelper.MediaDirectoryName + "/";
            //if (!string.IsNullOrEmpty(baseFolder))
            //{
            //    var baseMediaFolder = ServiceFactory.MediaFolderManager.Get(repository, baseFolder);
            //    basePrefix = baseMediaFolder.GetMediaFolderItemPath(null) + "/";
            //}

            ////add file
            //if (docs != null)
            //{
            //    foreach (var doc in docs)
            //    {
            //        var key = basePrefix + StorageNamesEncoder.EncodeBlobName(doc);
            //        var bytes = ossClient.GetObjectData(bucket, key);
            //        zipFile.AddEntry(doc, bytes);
            //    }
            //}
            ////add folders
            //if (folders != null)
            //{
            //    foreach (var folder in folders)
            //    {
            //        var folderName = folder.Split('~').LastOrDefault();
            //        zipFolder(ossClient, basePrefix, folderName, "", ref zipFile);
            //    }
            //}
            //zipFile.Save(outputStream);
        }

        //private void zipFolder(CosClient ossClient, string basePrefix, string folderName, string zipDir, ref ZipFile zipFile)
        //{
        //    zipDir = string.IsNullOrEmpty(zipDir) ? folderName : zipDir + "/" + folderName;
        //    zipFile.AddDirectoryByName(zipDir);
        //    var folderPrefix = basePrefix + StorageNamesEncoder.EncodeBlobName(folderName) + "/";

        //    var blobs = ossClient.ListBlobsWithPrefix(bucket, folderPrefix);
        //    foreach (var blob in blobs.ObjectSummaries)
        //    {
        //        if (blob.Key.EndsWith("/"))
        //        {
        //            continue;
        //        }
        //        var bytes = ossClient.GetObjectData(bucket, blob.Key);
        //        if (bytes.Length > 0)
        //        {
        //            zipFile.AddEntry(zipDir + "/" + blob.Key, bytes);
        //        }
        //    }
        //}

        private void MoveContent(string oldKey, string newKey)
        {

        }

        private void MoveDirectory(string repository, string newPrefix, string oldPrefix)
        {
            var files = _fileService.List(oldPrefix, repository);
            foreach (var item in files.data.infos)
            {
            }
        }
    }
}
