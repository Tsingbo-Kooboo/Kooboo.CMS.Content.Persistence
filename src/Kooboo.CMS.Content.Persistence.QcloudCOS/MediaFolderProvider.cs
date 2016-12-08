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

namespace Kooboo.CMS.Content.Persistence.QcloudCOS
{
    [Kooboo.CMS.Common.Runtime.Dependency.Dependency(typeof(IMediaFolderProvider), Order = 2)]
    [Kooboo.CMS.Common.Runtime.Dependency.Dependency(typeof(IProvider<MediaFolder>), Order = 2)]
    public class MediaFolderProvider : IMediaFolderProvider
    {
        private readonly string bucket;
        private readonly OssClient ossClient;
        public MediaFolderProvider()
        {
            var account = OssAccountHelper.GetOssClientBucket(Repository.Current);
            ossClient = account.Item1;
            bucket = account.Item2;
        }

        public IQueryable<MediaFolder> ChildFolders(MediaFolder parent)
        {
            return MediaFolders.ChildFolders(parent).AsQueryable();
        }

        public IEnumerable<MediaFolder> All(Repository repository)
        {
            return MediaFolders.RootFolders(repository).AsQueryable();
        }

        public MediaFolder Get(MediaFolder dummy)
        {
            return MediaFolders.GetFolder(dummy);
        }

        public void Add(MediaFolder item)
        {
            MediaFolders.AddFolder(item);
        }

        public void Update(MediaFolder @new, MediaFolder old)
        {
            MediaFolders.UpdateFolder(@new);
        }

        public void Remove(MediaFolder item)
        {
            MediaFolders.RemoveFolder(item);
            (new MediaContentProvider()).Delete(item);
        }


        public void Export(Repository repository, IEnumerable<MediaFolder> models, System.IO.Stream outputStream)
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
            MediaFolders.RenameFolder(@new, old);
            var oldPrefix = old.GetMediaFolderItemPath(null) + "/";
            var newPrefix = @new.GetMediaFolderItemPath(null) + "/";
            MoveDirectory(ossClient, bucket, newPrefix, oldPrefix);
        }


        public void Export(Repository repository, string baseFolder, string[] folders, string[] docs, Stream outputStream)
        {
            ZipFile zipFile = new ZipFile();
            var basePrefix = StorageNamesEncoder.EncodeContainerName(repository.Name) + "/" + MediaBlobHelper.MediaDirectoryName + "/";
            if (!string.IsNullOrEmpty(baseFolder))
            {
                var baseMediaFolder = ServiceFactory.MediaFolderManager.Get(repository, baseFolder);
                basePrefix = baseMediaFolder.GetMediaFolderItemPath(null) + "/";
            }

            //add file
            if (docs != null)
            {
                foreach (var doc in docs)
                {
                    var key = basePrefix + StorageNamesEncoder.EncodeBlobName(doc);
                    var bytes = ossClient.GetObjectData(bucket, key);
                    zipFile.AddEntry(doc, bytes);
                }
            }
            //add folders
            if (folders != null)
            {
                foreach (var folder in folders)
                {
                    var folderName = folder.Split('~').LastOrDefault();
                    zipFolder(ossClient, basePrefix, folderName, "", ref zipFile);
                }
            }
            zipFile.Save(outputStream);
        }

        private void zipFolder(OssClient ossClient, string basePrefix, string folderName, string zipDir, ref ZipFile zipFile)
        {
            zipDir = string.IsNullOrEmpty(zipDir) ? folderName : zipDir + "/" + folderName;
            zipFile.AddDirectoryByName(zipDir);
            var folderPrefix = basePrefix + StorageNamesEncoder.EncodeBlobName(folderName) + "/";

            var blobs = ossClient.ListBlobsWithPrefix(bucket, folderPrefix);
            foreach (var blob in blobs.ObjectSummaries)
            {
                if (blob.Key.EndsWith("/"))
                {
                    continue;
                }
                var bytes = ossClient.GetObjectData(bucket, blob.Key);
                if (bytes.Length > 0)
                {
                    zipFile.AddEntry(zipDir + "/" + blob.Key, bytes);
                }
            }
        }
        private void MoveContent(string oldKey, string newKey)
        {
            if (ossClient.DoesObjectExist(bucket, oldKey)
                && !ossClient.DoesObjectExist(bucket, newKey))
            {
                var oldContentBlob = ossClient.GetObject(bucket, oldKey);
                try
                {
                    var result = ossClient.CopyObject(new CopyObjectRequest(bucket, oldKey, bucket, newKey));
                }
                catch (Exception e)
                {
                    ossClient.PutObject(bucket, newKey, oldContentBlob.Content);
                    Kooboo.HealthMonitoring.Log.LogException(e);
                }
                ossClient.DeleteObject(bucket, oldKey);
            }
        }

        private void MoveDirectory(OssClient ossClient, string bucket, string newPrefix, string oldPrefix)
        {
            var blobs = ossClient.ListBlobsWithPrefix(bucket, oldPrefix);
            foreach (var blob in blobs.ObjectSummaries)
            {
                if (blob.Key.EndsWith("/"))
                {
                    var names = blob.Key.Substring(bucket.Length).Split('/');
                    for (var i = names.Length - 1; i >= 0; i--)
                    {
                        if (!string.IsNullOrEmpty(names[i]))
                        {
                            MoveDirectory(ossClient,
                                bucket,
                                $"{newPrefix}{ StorageNamesEncoder.EncodeBlobName(names[i])}/",
                                $"{oldPrefix}{ StorageNamesEncoder.EncodeBlobName(names[i])}/");
                            break;
                        }
                    }
                }
                else
                {
                    if (!ossClient.DoesObjectExist(bucket, blob.Key))
                    {
                        continue;
                    }
                    var newKey = UrlUtility.Combine(newPrefix, blob.Key.Substring(oldPrefix.Length));
                    MoveContent(blob.Key, newKey);
                }
            }
        }
    }
}
