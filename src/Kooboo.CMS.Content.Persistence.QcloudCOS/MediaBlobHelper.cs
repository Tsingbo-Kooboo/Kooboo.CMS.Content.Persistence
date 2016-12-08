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
using Kooboo.CMS.Content.Models;
using Kooboo.Web.Url;
using System.IO;

namespace Kooboo.CMS.Content.Persistence.QcloudCOS
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

        #region GetMediaBlobPath
        public static string GetMediaBlobPath(this MediaContent mediaContent)
        {
            return GetMediaFolderItemPath(mediaContent.GetFolder(), mediaContent.FileName);
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
                blob.Data.SetMetadata("Published", mediaContent.Published.Value.ToString());
            }
            if (!string.IsNullOrEmpty(mediaContent.UserId))
            {
                blob.Data.SetMetadata("UserId", mediaContent.UserId);
            }
            if (!string.IsNullOrEmpty(mediaContent.FileName))
            {
                blob.Data.SetMetadata("FileName", StorageNamesEncoder.EncodeBlobName(mediaContent.FileName));
            }
            if (mediaContent.ContentFile != null)
            {
                blob.Data.SetMetadata("Size", mediaContent.ContentFile.Stream.Length.ToString());
            }
            if (mediaContent.Metadata != null)
            {
                if (!string.IsNullOrEmpty(mediaContent.Metadata.AlternateText))
                {
                    blob.Data.SetMetadata("AlternateText", StorageNamesEncoder.EncodeBlobName(mediaContent.Metadata.AlternateText));
                }
                else
                {
                    blob.Data.RemoveMetadata("AlternateText");
                }

                if (!string.IsNullOrEmpty(mediaContent.Metadata.Description))
                {
                    blob.Data.SetMetadata("Description", StorageNamesEncoder.EncodeBlobName(mediaContent.Metadata.Description));
                }
                else
                {
                    blob.Data.RemoveMetadata("Description");
                }

                if (!string.IsNullOrEmpty(mediaContent.Metadata.Title))
                {
                    blob.Data.SetMetadata("Title", StorageNamesEncoder.EncodeBlobName(mediaContent.Metadata.Title));
                }
                else
                {
                    blob.Data.RemoveMetadata("Title");
                }
            }
            blob.Data.SetMetadata(Metadata.SystemHeaderKey.ContentType, Kooboo.IO.IOUtility.MimeType(mediaContent.FileName));
            return blob;
        }
        #endregion

        #region BlobToMediaContent
        public static MediaContent BlobToMediaContent(this OssObjectSummary blob, MediaContent mediaContent)
        {
            mediaContent.Published = true;
            mediaContent.Size = blob.Size;
            var fileFullName = blob.Name;
            mediaContent.FileName = Path.GetFileName(fileFullName);
            mediaContent.UserKey = mediaContent.FileName;
            mediaContent.UUID = mediaContent.FileName;
            mediaContent.VirtualPath = blob.AccessUrl;
            if (mediaContent.Metadata == null)
            {
                mediaContent.Metadata = new MediaContentMetadata();
            }
            return mediaContent;
        }

        //public static MediaContent BlobToMediaContent(this OssObject blob, MediaContent mediaContent, OssClient client)
        //{
        //    mediaContent.Published = blob.Metadata.UserMetadata.GetBool("Published");
        //    mediaContent.Size = blob.Metadata.UserMetadata.GetInt("Size");

        //    mediaContent.FileName = StorageNamesEncoder.DecodeBlobName(blob.Metadata.UserMetadata.GetString("FileName"));
        //    mediaContent.UserKey = mediaContent.FileName;
        //    mediaContent.UUID = mediaContent.FileName;
        //    mediaContent.UserId = blob.Metadata.UserMetadata.GetString("UserId");
        //    mediaContent.VirtualPath = OssAccountHelper.GetUrl(mediaContent.GetRepository(), blob);
        //    if (mediaContent.Metadata == null)
        //    {
        //        mediaContent.Metadata = new MediaContentMetadata();
        //    }

        //    mediaContent.Metadata.AlternateText = StorageNamesEncoder.DecodeBlobName(blob.Metadata.UserMetadata.GetString("AlternateText"));
        //    mediaContent.Metadata.Description = StorageNamesEncoder.DecodeBlobName(blob.Metadata.UserMetadata.GetString("Description"));
        //    mediaContent.Metadata.Title = StorageNamesEncoder.DecodeBlobName(blob.Metadata.UserMetadata.GetString("Title"));
        //    return mediaContent;
        //}
        #endregion

        //#region InitializeRepositoryContainer
        //public static Bucket InitializeRepositoryContainer(Repository repository)
        //{
        //    var account = OssAccountHelper.GetOssClientBucket(repository);
        //    var ossClient = account.Item1;
        //    var bucketName = account.Item2;
        //    var container = ossClient.CreateBucket(bucketName);
        //    ossClient.SetBucketAcl(bucketName, CannedAccessControlList.PublicRead);
        //    return container;
        //}
        //#endregion

        #region DeleteRepositoryContainer
        public static void DeleteRepositoryContainer(Repository repository)
        {
            throw new NotImplementedException();
            //var account = OssAccountHelper.GetOssClientBucket(repository);
            //var ossClient = account.Item1;
            //var bucket = account.Item2;
            //var items = ossClient.ListObjects(bucket, repository.Name)
            //    .ObjectSummaries
            //    .Select(it => it.Key)
            //    .ToList();
            //ossClient.DeleteObjects(new DeleteObjectsRequest(bucket, items));
        }
        #endregion
    }
}
