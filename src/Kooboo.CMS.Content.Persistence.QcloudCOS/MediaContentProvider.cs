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
using Kooboo.CMS.Content.Query.Expressions;
using Kooboo.CMS.Content.Query;
using System.IO;
using Kooboo.CMS.Content.Query.Translator;

namespace Kooboo.CMS.Content.Persistence.QcloudCOS
{
    #region Translator
    public class QueryExpressionTranslator : ExpressionVisitor
    {
        public CallType CallType { get; set; }
        private int? Skip { get; set; }
        private int? Take { get; set; }
        string fileName = null;
        string prefix = null;

        public IEnumerable<MediaContent> Translate(IExpression expression, OssClient ossClient, MediaFolder mediaFolder)
        {
            var account = OssAccountHelper.GetOssClientBucket(mediaFolder.Repository);
            this.Visite(expression);
            var key = mediaFolder.GetMediaFolderItemPath(fileName);
            if (!string.IsNullOrEmpty(fileName))
            {
                if (!ossClient.DoesObjectExist(account.Item2, key))
                {
                    return Enumerable.Empty<MediaContent>();
                }
                var blob = ossClient.GetObject(account.Item2, key);
                return new[] { blob.BlobToMediaContent(
                    new MediaContent(mediaFolder.Repository.Name, mediaFolder.FullName),
                    ossClient)
                };
            }
            else
            {
                var maxResult = 1000;
                if (Take.HasValue)
                {
                    maxResult = Take.Value;
                }
                var take = maxResult;

                var skip = 0;
                if (Skip.HasValue)
                {
                    skip = Skip.Value;
                    maxResult += skip;
                }
                var blobPrefix = mediaFolder.GetMediaFolderItemPath(prefix);

                if (string.IsNullOrEmpty(prefix))
                {
                    blobPrefix += "/";
                }

                return ossClient
                    .ListObjects(account.Item2, blobPrefix)
                    .ObjectSummaries
                    .Where(it => !it.Key.Substring(blobPrefix.Length).Contains("/"))
                    .Skip(skip)
                    .Take(take)
                    .Select(it => it.BlobToMediaContent(
                        new MediaContent(mediaFolder.Repository.Name, mediaFolder.FullName),
                        ossClient)
                        );
            }
        }

        protected override void VisitSkip(SkipExpression expression)
        {
            Skip = expression.Count;
        }

        protected override void VisitTake(TakeExpression expression)
        {
            Take = expression.Count;
        }

        protected override void VisitWhereEquals(WhereEqualsExpression expression)
        {
            if (expression.Value != null)
            {
                ValidExpression(expression.FieldName);
                fileName = expression.Value.ToString();
            }

        }
        private void ValidExpression(string fieldName)
        {
            //if (!fieldName.EqualsOrNullEmpty("FileName", StringComparison.OrdinalIgnoreCase)
            //    && !fieldName.EqualsOrNullEmpty("UUID", StringComparison.OrdinalIgnoreCase)
            //    && !fieldName.EqualsOrNullEmpty("UserKey", StringComparison.OrdinalIgnoreCase))
            //{      
            //    //throw new NotSupportedException("The azure storage provider only support query by FileName,UUID,UserKey.");
            //}      
            //if (!string.IsNullOrEmpty(fileName) || !string.IsNullOrEmpty(prefix))
            //{
            //    throw new NotSupportedException("The azure storage provider only support query by one condition.");
            //}
        }
        protected override void VisitWhereStartsWith(WhereStartsWithExpression expression)
        {
            WhereStartWith(expression.FieldName, expression.Value);
        }

        private void WhereStartWith(string fieldName, object value)
        {
            if (value != null)
            {
                ValidExpression(fieldName);
                prefix = value.ToString();
            }
        }

        protected override void VisitWhereContains(WhereContainsExpression expression)
        {
            WhereStartWith(expression.FieldName, expression.Value);
        }

        private void VisitInner(IExpression expression)
        {
            this.Visite(expression);
        }

        protected override void VisitNot(NotExpression expression)
        {
            throw new NotSupportedException();
        }
        protected override void VisitAndAlso(AndAlsoExpression expression)
        {
            if (!(expression.Left is TrueExpression))
            {
                VisitInner(expression.Left);
            }

            if (!(expression.Right is TrueExpression))
            {
                VisitInner(expression.Right);
            }
        }

        protected override void VisitOrElse(OrElseExpression expression)
        {
            if (!(expression.Left is FalseExpression))
            {
                VisitInner(expression.Left);
            }

            if (!(expression.Right is FalseExpression))
            {
                VisitInner(expression.Right);
            }
        }


        protected override void VisitCall(CallExpression expression)
        {
            this.CallType = expression.CallType;
        }

        #region NotSupported

        protected override void VisitWhereCategory(WhereCategoryExpression expression)
        {
            ThrowNotSupported();
        }

        protected override void VisitFalse(FalseExpression expression)
        {
            ThrowNotSupported();
        }

        protected override void VisitTrue(TrueExpression expression)
        {
            ThrowNotSupported();
        }

        protected override void VisitWhereIn(WhereInExpression expression)
        {
            ThrowNotSupported();
        }

        private void ThrowNotSupported()
        {
            throw new NotSupportedException("Not supported for aliyun oss storage.");
        }
        protected override void VisitSelect(SelectExpression expression)
        {
            ThrowNotSupported();
        }

        protected override void VisitOrder(OrderExpression expression)
        {
            OrderFields.Add(new OrderField() { FieldName = expression.FieldName, Descending = expression.Descending });
            //ThrowNotSupported();
        }

        protected override void VisitWhereBetweenOrEqual(WhereBetweenOrEqualExpression expression)
        {
            ThrowNotSupported();
        }

        protected override void VisitWhereBetween(WhereBetweenExpression expression)
        {
            ThrowNotSupported();
        }


        protected override void VisitWhereEndsWith(WhereEndsWithExpression expression)
        {
            ThrowNotSupported();
        }

        protected override void VisitWhereClause(WhereClauseExpression expression)
        {
            ThrowNotSupported();
        }

        protected override void VisitWhereGreaterThan(WhereGreaterThanExpression expression)
        {
            ThrowNotSupported();
        }

        protected override void VisitWhereGreaterThanOrEqual(WhereGreaterThanOrEqualExpression expression)
        {
            ThrowNotSupported();
        }


        protected override void VisitWhereLessThan(WhereLessThanExpression expression)
        {
            ThrowNotSupported();
        }

        protected override void VisitWhereLessThanOrEqual(WhereLessThanOrEqualExpression expression)
        {
            ThrowNotSupported();
        }

        protected override void VisitWhereNotEquals(WhereNotEqualsExpression expression)
        {
            ThrowNotSupported();
        }
        #endregion
    }
    #endregion
    [Kooboo.CMS.Common.Runtime.Dependency.Dependency(typeof(IMediaContentProvider), Order = 2)]
    [Kooboo.CMS.Common.Runtime.Dependency.Dependency(typeof(IContentProvider<MediaContent>), Order = 2)]
    public class MediaContentProvider : IMediaContentProvider
    {
        private readonly OssClient ossClient;
        private readonly string bucket;

        public MediaContentProvider()
        {
            var account = OssAccountHelper.GetOssClientBucket(Repository.Current);
            ossClient = account.Item1;
            bucket = account.Item2;
        }

        #region IMediaContentProvider
        public void Add(MediaContent content, bool overrided)
        {
            var repository = content.GetRepository();
            var account = OssAccountHelper.GetOssClientBucket(repository);

            if (content.ContentFile != null)
            {
                content.FileName = content.ContentFile.FileName;
                content.UserKey = content.FileName;
                content.UUID = content.FileName;

                var key = content.GetMediaBlobPath();
                if (!overrided)
                {
                    if (account.Item1.DoesObjectExist(bucket, key))
                    {
                        return;
                    }
                }
                var metaData = content.GetBlobMetadata();
                var result = account.Item1.PutObject(bucket, key, content.ContentFile.Stream, metaData);
                content.VirtualPath = OssAccountHelper.GetUrl(repository, key);
            }
        }

        public void Move(MediaFolder sourceFolder, string oldFileName, MediaFolder targetFolder, string newFileName)
        {
            var oldMediaContent = new MediaContent() { Repository = sourceFolder.Repository.Name, FolderName = sourceFolder.FullName, UUID = oldFileName, FileName = oldFileName };
            var newMediaContent = new MediaContent() { Repository = targetFolder.Repository.Name, FolderName = targetFolder.FullName, UUID = newFileName, FileName = newFileName };

            MoveContent(oldMediaContent, newMediaContent);
        }

        private void MoveContent(MediaContent oldMediaContent, MediaContent newMediaContent)
        {
            var oldKey = oldMediaContent.GetMediaBlobPath();
            var newKey = newMediaContent.GetMediaBlobPath();

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
                    Add(newMediaContent, true);
                    Kooboo.HealthMonitoring.Log.LogException(e);
                }
                ossClient.DeleteObject(bucket, oldKey);
            }
        }

        public void Add(MediaContent content)
        {
            Add(content, true);
        }

        public void Update(MediaContent @new, MediaContent old)
        {
            if (!@new.FileName.EqualsOrNullEmpty(old.FileName, StringComparison.OrdinalIgnoreCase))
            {
                MoveContent(old, @new);
            }
            else
            {
                Add(@new, true);
            }
        }

        public void Delete(MediaContent content)
        {
            ossClient.DeleteObject(content.Repository.ToLower(), content.GetMediaBlobPath());
        }

        public void Delete(MediaFolder mediaFolder)
        {
            var prefix = mediaFolder.GetMediaDirectoryPath();
            var blobs = ossClient.ListObjects(bucket, prefix);
            var keys = blobs.ObjectSummaries.Select(it => it.Key);
            if (keys.Any())
            {
                ossClient.DeleteObjects(new DeleteObjectsRequest(bucket, keys.ToList(), true));
            }
        }

        public object Execute(IContentQuery<MediaContent> query)
        {
            var mediaQuery = (MediaContentQuery)query;

            QueryExpressionTranslator translator = new QueryExpressionTranslator();

            var blobs = translator.Translate(query.Expression, ossClient, mediaQuery.MediaFolder)
                .Where(it => it != null);

            foreach (var item in translator.OrderFields)
            {
                if (item.Descending)
                {
                    blobs = blobs.OrderByDescending(it => it.GetType().GetProperty(item.FieldName).GetValue(it, null));
                }
                else
                {
                    blobs = blobs.OrderBy(it => it.GetType().GetProperty(item.FieldName).GetValue(it, null));
                }
            }
            //translator.Visite(query.Expression);

            switch (translator.CallType)
            {
                case CallType.Count:
                    return blobs.Count();
                case CallType.First:
                    return blobs.First();
                case CallType.Last:
                    return blobs.Last();
                case CallType.LastOrDefault:
                    return blobs.LastOrDefault();
                case CallType.FirstOrDefault:
                    return blobs.FirstOrDefault();
                case CallType.Unspecified:
                default:
                    return blobs;
            }
        }
        #endregion

        public void InitializeMediaContents(Repository repository)
        {
            //TODO: fix this
            //MediaBlobHelper.InitializeRepositoryContainer(repository);

            Default.MediaFolderProvider fileMediaFolderProvider = new Default.MediaFolderProvider();
            foreach (var item in fileMediaFolderProvider.All(repository))
            {
                ImportMediaFolderDataCascading(fileMediaFolderProvider.Get(item));
            }
        }

        private void ImportMediaFolderDataCascading(MediaFolder mediaFolder)
        {
            Default.MediaContentProvider fileProvider = Kooboo.CMS.Common.Runtime.EngineContext.Current.Resolve<Kooboo.CMS.Content.Persistence.Default.MediaContentProvider>();

            //add media folder
            MediaFolderProvider folderProvider = new MediaFolderProvider();
            folderProvider.Add(mediaFolder);

            foreach (var item in fileProvider.All(mediaFolder))
            {
                item.ContentFile = new ContentFile() { FileName = item.FileName };
                using (var fileStream = new FileStream(item.PhysicalPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    item.ContentFile.Stream = fileStream;
                    Add(item);
                }
            }
            Default.MediaFolderProvider fileMediaFolderProvider = new Default.MediaFolderProvider();
            foreach (var item in fileMediaFolderProvider.ChildFolders(mediaFolder))
            {
                ImportMediaFolderDataCascading(item);
            }
        }

        public byte[] GetContentStream(MediaContent content)
        {
            var path = string.Empty;
            if (string.IsNullOrEmpty(content.Repository))
            {
                var uri = new Uri(content.VirtualPath, UriKind.RelativeOrAbsolute);
                path = uri.LocalPath.Trim('/');
            }
            else
            {
                path = content.GetMediaBlobPath();
            }
            return ossClient.GetObjectData(bucket, path);
        }

        public void SaveContentStream(MediaContent content, Stream stream)
        {
            if (stream.Length == 0)
            {
                return;
            }
            stream.Position = 0;
            content.ContentFile = new ContentFile
            {
                Name = content.FileName,
                FileName = content.FileName,
                Stream = stream
            };
        }
    }
}
