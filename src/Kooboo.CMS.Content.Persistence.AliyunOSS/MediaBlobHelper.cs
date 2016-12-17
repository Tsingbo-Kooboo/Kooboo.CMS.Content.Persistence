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
using Kooboo.Extensions;
using Aliyun.OSS;
using Kooboo.CMS.Content.Models;
using Kooboo.Web.Url;
using System.IO;

namespace Kooboo.CMS.Content.Persistence.AliyunOSS
{
    public static class MediaBlobHelper
    {
        #region const
        public static string MediaDirectoryName = "Media";
        public static string MediaFolderContentType = "MediaFolder";
        #endregion

        #region GetMediaDirectoryPath
        public static string GetMediaDirectoryPath(this MediaFolder mediaFolder)
        {
            return UrlUtility.Combine(new string[] { StorageNamesEncoder.EncodeContainerName(mediaFolder.Repository.Name), MediaDirectoryName }
               .Concat(mediaFolder.NamePaths)
               .ToArray());
        }
        #endregion

        #region GetMediaFolderItemPath
        public static string GetMediaFolderItemPath(this MediaFolder mediaFolder, string itemName)
        {
            return UrlUtility.Combine(mediaFolder.GetMediaDirectoryPath(), itemName);
        }
        #endregion

        #region MediaContentToBlob
        public static ObjectMetadata GetBlobMetadata(this MediaContent mediaContent)
        {
            var metaData = new ObjectMetadata();
            if (mediaContent.Published.HasValue)
            {
                metaData.UserMetadata["Published"] = mediaContent.Published.Value.ToString();
            }
            if (!string.IsNullOrEmpty(mediaContent.UserId))
            {
                metaData.UserMetadata["UserId"] = mediaContent.UserId;
            }
            if (!string.IsNullOrEmpty(mediaContent.FileName))
            {
                metaData.UserMetadata["FileName"] = StorageNamesEncoder.EncodeBlobName(mediaContent.FileName);
            }
            if (mediaContent.ContentFile != null)
            {
                metaData.UserMetadata["Size"] = mediaContent.ContentFile.Stream.Length.ToString();
            }
            if (mediaContent.Metadata != null)
            {
                if (!string.IsNullOrEmpty(mediaContent.Metadata.AlternateText))
                {
                    metaData.UserMetadata["AlternateText"] = StorageNamesEncoder.EncodeBlobName(mediaContent.Metadata.AlternateText);
                }
                else if (metaData.UserMetadata.ContainsKey("AlternateText"))
                {
                    metaData.UserMetadata.Remove("AlternateText");
                }

                if (!string.IsNullOrEmpty(mediaContent.Metadata.Description))
                {
                    metaData.UserMetadata["Description"] = StorageNamesEncoder.EncodeBlobName(mediaContent.Metadata.Description);
                }
                else if (metaData.UserMetadata.ContainsKey("Description"))
                {
                    metaData.UserMetadata.Remove("Description");
                }

                if (!string.IsNullOrEmpty(mediaContent.Metadata.Title))
                {
                    metaData.UserMetadata["Title"] = StorageNamesEncoder.EncodeBlobName(mediaContent.Metadata.Title);
                }
                else if (metaData.UserMetadata.ContainsKey("Title"))
                {
                    metaData.UserMetadata.Remove("Title");
                }
            }
            metaData.ContentType = Kooboo.IO.IOUtility.MimeType(mediaContent.FileName);

            return metaData;
        }

        public static OssObject MediaContentToBlob(this MediaContent mediaContent, OssObject blob)
        {
            if (mediaContent.Published.HasValue)
            {
                blob.Metadata.UserMetadata["Published"] = mediaContent.Published.Value.ToString();
            }
            if (!string.IsNullOrEmpty(mediaContent.UserId))
            {
                blob.Metadata.UserMetadata["UserId"] = mediaContent.UserId;
            }
            if (!string.IsNullOrEmpty(mediaContent.FileName))
            {
                blob.Metadata.UserMetadata["FileName"] = StorageNamesEncoder.EncodeBlobName(mediaContent.FileName);
            }
            if (mediaContent.ContentFile != null)
            {
                blob.Metadata.UserMetadata["Size"] = mediaContent.ContentFile.Stream.Length.ToString();
            }
            if (mediaContent.Metadata != null)
            {
                if (!string.IsNullOrEmpty(mediaContent.Metadata.AlternateText))
                {
                    blob.Metadata.UserMetadata["AlternateText"] = StorageNamesEncoder.EncodeBlobName(mediaContent.Metadata.AlternateText);
                }
                else if (blob.Metadata.UserMetadata.ContainsKey("AlternateText"))
                {
                    blob.Metadata.UserMetadata.Remove("AlternateText");
                }

                if (!string.IsNullOrEmpty(mediaContent.Metadata.Description))
                {
                    blob.Metadata.UserMetadata["Description"] = StorageNamesEncoder.EncodeBlobName(mediaContent.Metadata.Description);
                }
                else if (blob.Metadata.UserMetadata.ContainsKey("Description"))
                {
                    blob.Metadata.UserMetadata.Remove("Description");
                }

                if (!string.IsNullOrEmpty(mediaContent.Metadata.Title))
                {
                    blob.Metadata.UserMetadata["Title"] = StorageNamesEncoder.EncodeBlobName(mediaContent.Metadata.Title);
                }
                else if (blob.Metadata.UserMetadata.ContainsKey("Title"))
                {
                    blob.Metadata.UserMetadata.Remove("Title");
                }
            }
            blob.Metadata.ContentType = Kooboo.IO.IOUtility.MimeType(mediaContent.FileName);
            return blob;
        }
        #endregion

        #region BlobToMediaContent
        public static MediaContent BlobToMediaContent(this OssObjectSummary blob, MediaContent mediaContent, OssClient client)
        {
            mediaContent.Published = true;
            mediaContent.Size = blob.Size;
            var fileFullName = StorageNamesEncoder.DecodeBlobName(blob.Key);
            mediaContent.FileName = Path.GetFileName(fileFullName);
            mediaContent.UserKey = mediaContent.FileName;
            mediaContent.UUID = mediaContent.FileName;
            mediaContent.VirtualPath = OssAccountHelper.GetUrl(mediaContent.GetRepository(), blob.Key);
            if (mediaContent.Metadata == null)
            {
                mediaContent.Metadata = new MediaContentMetadata();
            }
            return mediaContent;
        }

        public static MediaContent BlobToMediaContent(this OssObject blob, MediaContent mediaContent, OssClient client)
        {
            mediaContent.Published = blob.Metadata.UserMetadata.GetBool("Published");
            mediaContent.Size = blob.Metadata.UserMetadata.GetInt("Size");

            mediaContent.FileName = StorageNamesEncoder.DecodeBlobName(blob.Metadata.UserMetadata.GetString("FileName"));
            mediaContent.UserKey = mediaContent.FileName;
            mediaContent.UUID = mediaContent.FileName;
            mediaContent.UserId = blob.Metadata.UserMetadata.GetString("UserId");
            mediaContent.VirtualPath = OssAccountHelper.GetUrl(mediaContent.GetRepository(), blob);
            if (mediaContent.Metadata == null)
            {
                mediaContent.Metadata = new MediaContentMetadata();
            }

            mediaContent.Metadata.AlternateText = StorageNamesEncoder.DecodeBlobName(blob.Metadata.UserMetadata.GetString("AlternateText"));
            mediaContent.Metadata.Description = StorageNamesEncoder.DecodeBlobName(blob.Metadata.UserMetadata.GetString("Description"));
            mediaContent.Metadata.Title = StorageNamesEncoder.DecodeBlobName(blob.Metadata.UserMetadata.GetString("Title"));
            return mediaContent;
        }
        #endregion

        #region SetMediaFolderContentType
        public static void SetMediaFolderContentType(this OssObject blob)
        {
            blob.Metadata.ContentType = MediaFolderContentType;
        }
        #endregion

        #region CheckIfMediaFolder
        public static bool CheckIfMediaFolder(this OssObject blob)
        {
            return blob.Metadata.ContentType == MediaFolderContentType;
        }
        #endregion

        #region InitializeRepositoryContainer
        public static Bucket InitializeRepositoryContainer(Repository repository)
        {
            var account = OssAccountHelper.GetOssClientBucket(repository);
            var ossClient = account.Item1;
            var bucketName = account.Item2;
            var container = ossClient.CreateBucket(bucketName);
            ossClient.SetBucketAcl(bucketName, CannedAccessControlList.PublicRead);
            return container;
        }
        #endregion

        #region DeleteRepositoryContainer
        public static void DeleteRepositoryContainer(Repository repository)
        {
            var account = OssAccountHelper.GetOssClientBucket(repository);
            var ossClient = account.Item1;
            var bucket = account.Item2;
            var items = ossClient.ListObjects(bucket, repository.Name)
                .ObjectSummaries
                .Select(it => it.Key)
                .ToList();
            ossClient.DeleteObjects(new DeleteObjectsRequest(bucket, items));
        }
        #endregion
    }
}
