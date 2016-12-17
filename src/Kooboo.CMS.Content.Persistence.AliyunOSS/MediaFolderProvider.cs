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
using Kooboo.CMS.Content.Models.Paths;
using Kooboo.CMS.Content.Models;
using System.Runtime.Serialization;
using Kooboo.Runtime.Serialization;
using Ionic.Zip;
using Kooboo.CMS.Content.Services;
using System.IO;
using Kooboo.CMS.Common.Persistence.Non_Relational;
using Kooboo.Web.Url;
using Kooboo.CMS.Content.Caching;
using Kooboo.CMS.Caching;
using Kooboo.IO;
using Aliyun.OSS;
using Kooboo.CMS.Content.Persistence.AliyunOSS.Services;
using Kooboo.CMS.Content.Persistence.AliyunOSS.Utilities;
using Kooboo.CMS.Content.Persistence.AliyunOSS.Models;
using Kooboo.CMS.Common.Runtime.Dependency;

namespace Kooboo.CMS.Content.Persistence.AliyunOSS
{
    [Dependency(typeof(IMediaFolderProvider), Order = 10)]
    [Dependency(typeof(IProvider<MediaFolder>), Order = 10)]
    public class MediaFolderProvider : IMediaFolderProvider
    {
        private readonly IAccountService _accountService;
        private readonly IMediaFileService _fileService;
        private readonly IMediaFolderService _folderService;
        public MediaFolderProvider(IAccountService accountService,
            IMediaFileService fileService,
            IMediaFolderService folderService)
        {
            _accountService = accountService;
            _fileService = fileService;
            _folderService = folderService;
        }

        public IQueryable<MediaFolder> ChildFolders(MediaFolder parent)
        {
            return _folderService
                .List(parent.FullName, parent.Repository.Name)
                .AsQueryable();
        }

        public IEnumerable<MediaFolder> All(Repository repository)
        {
            return _folderService
                .List("/", repository.Name)
                .Where(it => it != null);
        }

        public MediaFolder Get(MediaFolder dummy)
        {
            return _folderService
                .Get(dummy.FullName, dummy.Repository.Name);
        }

        public void Add(MediaFolder item)
        {
            _folderService.Create(item.FullName, item.Repository.Name);
        }

        public void Update(MediaFolder @new, MediaFolder old)
        {
            throw new NotImplementedException();
        }

        public void Remove(MediaFolder item)
        {
            _folderService.Delete(item.FullName, item.Repository.Name);
        }


        public void Export(Repository repository,
            IEnumerable<MediaFolder> models,
            Stream outputStream)
        {
            throw new NotImplementedException();
        }

        public void Import(Repository repository,
            MediaFolder folder,
            Stream zipStream,
            bool @override)
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
            _folderService.Move(old, @new);
        }


        public void Export(Repository repository,
            string baseFolder,
            string[] folders,
            string[] docs,
            Stream outputStream)
        {
            string bucket;
            var ossClient = _accountService.GetClient(repository.Name, out bucket);
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
                    using (var stream = new MemoryStream())
                    {
                        ossClient.GetObject(new GetObjectRequest(bucket, path), stream);
                        stream.Position = 0;
                        zipFile.AddEntry(doc, stream.ReadData());
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
            var ossClient = _accountService.GetClient(repository.Name, out bucket);
            var blobs = ossClient.ListBlobsWithPrefix(bucket, folderPrefix);
            var len = folderPrefix.Length;
            foreach (var blob in blobs.ObjectSummaries)
            {
                if (blob.Key.EndsWith("/"))
                {
                    continue;
                }
                using (var stream = new MemoryStream())
                {
                    ossClient.GetObject(new GetObjectRequest(bucket, blob.Key), stream);
                    stream.Position = 0;
                    var bytes = stream.ReadData();
                    if (bytes.Length > 0)
                    {
                        var key = UrlUtility.Combine(zipDir, blob.Key.Substring(len));
                        zipFile.AddEntry(key, bytes);
                    }
                }
            }
        }


        private void MoveDirectory(OssClient ossClient, string bucket, string newPrefix, string oldPrefix)
        {
            //var blobs = ossClient.ListBlobsWithPrefix(bucket, oldPrefix);
            //foreach (var blob in blobs.ObjectSummaries)
            //{
            //    if (blob.Key.EndsWith("/"))
            //    {
            //        var names = blob.Key.Substring(bucket.Length).Split('/');
            //        for (var i = names.Length - 1; i >= 0; i--)
            //        {
            //            if (!string.IsNullOrEmpty(names[i]))
            //            {
            //                MoveDirectory(ossClient,
            //                    bucket,
            //                    $"{newPrefix}{ StorageNamesEncoder.EncodeBlobName(names[i])}/",
            //                    $"{oldPrefix}{ StorageNamesEncoder.EncodeBlobName(names[i])}/");
            //                break;
            //            }
            //        }
            //    }
            //    else
            //    {
            //        if (!ossClient.DoesObjectExist(bucket, blob.Key))
            //        {
            //            continue;
            //        }
            //        var newKey = UrlUtility.Combine(newPrefix, blob.Key.Substring(oldPrefix.Length));
            //        MoveContent(blob.Key, newKey);
            //    }
            //}
        }
    }
}
