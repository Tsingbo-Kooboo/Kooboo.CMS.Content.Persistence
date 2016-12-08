using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Kooboo.CMS.Content.Models;
using Kooboo.Runtime.Serialization;
using System.IO;
using Kooboo.CMS.Content.Caching;
using Kooboo.CMS.Caching;
using Aliyun.OSS;
using Kooboo.Web.Script.Serialization;
using Kooboo.HealthMonitoring;
using Kooboo.IO;

namespace Kooboo.CMS.Content.Persistence.AliyunOSS
{

    public static class MediaFolders
    {

        static System.Threading.ReaderWriterLockSlim locker = new System.Threading.ReaderWriterLockSlim();
        public static void AddFolder(MediaFolder folder)
        {
            locker.EnterWriteLock();
            try
            {
                var folders = GetList(folder.Repository);
                var name = folder.FullName.Trim('~');
                if (!folders.ContainsKey(name))
                {
                    folder.UtcCreationDate = DateTime.UtcNow;
                    folders[name] = folder;
                    SaveList(folder.Repository, folders);
                }
            }
            finally
            {
                locker.ExitWriteLock();
            }
        }
        public static MediaFolder GetFolder(MediaFolder dummy)
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
        public static void RemoveFolder(MediaFolder folder)
        {
            locker.EnterWriteLock();
            try
            {
                var storeList = GetList(folder.Repository);
                var mediaFolders = ToMediaFolders(folder.Repository, storeList);
                if (storeList.ContainsKey(folder.FullName))
                {
                    storeList.Remove(folder.FullName);

                    foreach (var item in mediaFolders)
                    {
                        if (item.Parent == folder)
                        {
                            if (storeList.ContainsKey(item.FullName))
                            {
                                storeList.Remove(item.FullName);
                            }
                        }
                    }
                }
                SaveList(folder.Repository, storeList);
            }
            finally
            {
                locker.ExitWriteLock();
            }
        }

        public static void UpdateFolder(MediaFolder folder)
        {
            locker.EnterWriteLock();
            try
            {
                var folders = GetList(folder.Repository);
                folders[folder.FullName] = folder;
                SaveList(folder.Repository, folders);
            }
            finally
            {
                locker.ExitWriteLock();
            }

        }
        public static void RenameFolder(MediaFolder @new, MediaFolder old)
        {
            locker.EnterWriteLock();
            try
            {
                var folders = GetList(old.Repository);
                var keys = folders.Keys.ToList();
                foreach (var key in keys)
                {
                    if (key.StartsWith(old.FullName + "~"))
                    {
                        var newKey = @new.FullName + key.Substring(old.FullName.Length);
                        folders.Add(newKey, folders[key]);
                        folders.Remove(key);
                    }
                }
                if (folders.ContainsKey(old.FullName) && !folders.ContainsKey(@new.FullName))
                {
                    folders.Add(@new.FullName, @new);
                    folders.Remove(@old.FullName);
                    SaveList(@new.Repository, folders);
                }
            }
            finally
            {
                locker.ExitWriteLock();
            }
        }

        public static IEnumerable<MediaFolder> RootFolders(Repository repository)
        {
            locker.EnterReadLock();
            try
            {
                return ToMediaFolders(repository, GetList(repository)).Where(it => it.Parent == null);
            }
            finally
            {
                locker.ExitReadLock();
            }

        }
        public static IEnumerable<MediaFolder> ChildFolders(MediaFolder parent)
        {
            locker.EnterReadLock();
            try
            {
                var query = ToMediaFolders(parent.Repository, GetList(parent.Repository));
                //loop bug in azure
                query = query.Where(it => (parent == null && it.Parent == null) || (it.Parent != null && it.Parent.UUID == parent.UUID));
                return query;
            }
            finally
            {
                locker.ExitReadLock();
            }

        }
        private static IEnumerable<MediaFolder> ToMediaFolders(Repository repository, Dictionary<string, MediaFolder> folders)
        {
            return folders.Select(it => ToMediaFolder(repository, it.Key, it.Value)).ToArray();
        }
        private static MediaFolder ToMediaFolder(Repository repository, string fullName, MediaFolder folderProperties)
        {
            return new MediaFolder(repository, fullName)
            {
                DisplayName = folderProperties.DisplayName,
                UserId = folderProperties.UserId,
                UtcCreationDate = folderProperties.UtcCreationDate,
                AllowedExtensions = folderProperties.AllowedExtensions
            };
        }

        public static Dictionary<string, MediaFolder> GetList(Repository repository)
        {
            var account = OssAccountHelper.GetOssClientBucket(repository);
            var mediaFolders = repository
                .ObjectCache()
                .GetCache($"Aliyun-OSS-MediaFolders-Cachings-{repository.Name}", () =>
            {
                Dictionary<string, MediaFolder> folders = null;
                try
                {
                    byte[] data = null;
                    var jsonConfigName = $"{MediaBlobHelper.MediaDirectoryName}.{repository.Name}.json".ToLower();
                    var xmlConfigName = $"{MediaBlobHelper.MediaDirectoryName}.{repository.Name}.xml".ToLower();
                    if (account.Item1.DoesObjectExist(account.Item2, jsonConfigName))
                    {
                        data = account.Item1.GetObjectData(account.Item2, jsonConfigName);
                        if (data != null && data.Length > 0)
                        {
                            var json = Encoding.UTF8.GetString(data);
                            folders = JsonHelper.Deserialize<Dictionary<string, MediaFolder>>(json);
                        }
                    }
                    else if (account.Item1.DoesObjectExist(account.Item2, xmlConfigName))
                    {
                        data = account.Item1.GetObjectData(account.Item2, xmlConfigName);
                        if (data != null && data.Length > 0)
                        {
                            var xml = Encoding.UTF8.GetString(data);
                            folders = DataContractSerializationHelper.DeserializeFromXml<Dictionary<string, MediaFolder>>(xml);
                            SaveList(repository, folders);
                            account.Item1.DeleteObject(account.Item2, xmlConfigName);
                        }
                    }
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
            return mediaFolders;
        }

        private static void SaveList(Repository repository, Dictionary<string, MediaFolder> folders)
        {
            var account = OssAccountHelper.GetOssClientBucket(repository);
            var container = MediaBlobHelper.InitializeRepositoryContainer(repository);
            var jsonConfigName = $"{MediaBlobHelper.MediaDirectoryName}.{repository.Name}.json".ToLower();

            if (folders != null && folders.Count > 0)
            {
                var json = JsonHelper.ToJSON(folders);
                var bytes = Encoding.UTF8.GetBytes(json);
                using (var stream = new MemoryStream(bytes))
                {
                    stream.Position = 0;
                    var meta = new ObjectMetadata
                    {
                        ContentType = IOUtility.MimeType(jsonConfigName)
                    };
                    account.Item1.PutObject(account.Item2, jsonConfigName, stream, meta);
                }
            }
            else
            {
                account.Item1.DeleteObject(account.Item2, jsonConfigName);
            }
        }

    }

}