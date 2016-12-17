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
using Aliyun.OSS;
using Kooboo.CMS.Content.Query.Expressions;
using Kooboo.CMS.Content.Query;
using System.IO;
using Kooboo.CMS.Content.Query.Translator;
using Kooboo.CMS.Content.Persistence.AliyunOSS.Services;
using Kooboo.CMS.Common.Runtime;
using Kooboo.CMS.Content.Persistence.AliyunOSS.Models;
using Kooboo.CMS.Content.Persistence.AliyunOSS.Utilities;
using Kooboo.CMS.Content.Persistence.AliyunOSS.Extensions;
using Kooboo.HealthMonitoring;
using Kooboo.Web.Url;
using Kooboo.IO;

namespace Kooboo.CMS.Content.Persistence.AliyunOSS
{
    #region Translator
    public class QueryExpressionTranslator : ExpressionVisitor
    {
        public CallType CallType { get; set; }
        private int? Skip { get; set; }
        private int? Take { get; set; }
        string fileName = null;
        string prefix = null;

        public IEnumerable<MediaContent> Translate(
            IExpression expression,
            MediaFolder mediaFolder,
            IAccountService accountService)
        {
            string bucket;
            var client = accountService.GetClient(mediaFolder.Name, out bucket);
            this.Visite(expression);
            var key = mediaFolder.GetOssKey();
            var repository = mediaFolder.Repository.Name;
            if (!string.IsNullOrEmpty(fileName))
            {
                var fileKey = UrlUtility.Combine(mediaFolder.FullName, fileName);
                fileKey = MediaPathUtility.FilePath(fileKey, repository);
                if (!client.DoesObjectExist(bucket, fileKey))
                {
                    return Enumerable.Empty<MediaContent>();
                }
                var blob = client.GetObject(bucket, fileKey);
                return new[] {
                    BlobToMediaContent(blob, accountService)
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

                return client.ListBlobsInFolder(bucket, mediaFolder)
                      .Skip(skip)
                      .Take(take)
                      .Select(it => BlobToMediaContent(it, accountService));
            }
        }
        private MediaContent BlobToMediaContent(OssObject metaData, IAccountService accountService)
        {
            var info = new KoobooMediaInfo(metaData.Key);
            var modifiedDate = metaData.Metadata.LastModified.ToUniversalTime();
            var url = accountService.ResolveUrl(info.FilePath, info.Repository);
            var fileName = info.FileName;

            return new MediaContent(info.Repository, info.Folder)
            {
                VirtualPath = url,
                UtcLastModificationDate = modifiedDate,
                UtcCreationDate = modifiedDate,
                FileName = fileName,
                UserKey = fileName,
                UUID = fileName,
                Size = metaData.Content.Length,
                ContentFile = new ContentFile
                {
                    FileName = fileName,
                    Name = fileName,
                    Stream = metaData.Content
                }
            };
        }

        private MediaContent BlobToMediaContent(OssObjectSummary metaData, IAccountService accountService)
        {
            var info = new KoobooMediaInfo(metaData.Key);
            var url = accountService.ResolveUrl(info.FilePath, info.Repository);
            var modifiedDate = metaData.LastModified.ToUniversalTime();
            var fileName = info.FileName;
            return new MediaContent(info.Repository, info.Folder)
            {
                VirtualPath = url,
                UtcLastModificationDate = modifiedDate,
                UtcCreationDate = modifiedDate,
                FileName = fileName,
                UserKey = fileName,
                UUID = fileName,
                Size = metaData.Size,
                ContentFile = new ContentFile
                {
                    FileName = fileName,
                    Name = fileName
                }
            };
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
        private readonly IAccountService _accountService;
        private readonly IMediaFolderService _folderService;
        private readonly IMediaFileService _fileService;
        public MediaContentProvider(IAccountService accountService,
            IMediaFolderService folderService,
            IMediaFileService fileService)
        {
            _accountService = accountService;
            _folderService = folderService;
            _fileService = fileService;
        }

        #region IMediaContentProvider
        public void Add(MediaContent content, bool overrided)
        {
            var key = content.GetMediaPath();
            var metaData = new Dictionary<string, string>();
            foreach (var item in content)
            {
                metaData[item.Key] = item.Value?.ToString();
            }
            _fileService.Create(key, content.Repository, content.ContentFile.Stream, metaData, overrided);
        }

        public void Move(MediaFolder sourceFolder, string oldFileName, MediaFolder targetFolder, string newFileName)
        {
            var oldMediaContent = new MediaContent() { Repository = sourceFolder.Repository.Name, FolderName = sourceFolder.FullName, UUID = oldFileName, FileName = oldFileName };
            var newMediaContent = new MediaContent() { Repository = targetFolder.Repository.Name, FolderName = targetFolder.FullName, UUID = newFileName, FileName = newFileName };

            MoveContent(oldMediaContent, newMediaContent);
        }

        private void MoveContent(MediaContent oldMediaContent, MediaContent newMediaContent)
        {
            string bucket;
            var client = _accountService.GetClient(oldMediaContent.Repository, out bucket);
            var oldKey = oldMediaContent.GetMediaPath();
            var newKey = newMediaContent.GetMediaPath();

            try
            {
                var request = new CopyObjectRequest(bucket, oldKey, bucket, newKey);
                var result = client.CopyObject(request);
                client.DeleteObject(bucket, oldKey);
            }
            catch (Exception e)
            {
                Add(newMediaContent, true);
                Log.LogException(e);
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
            string bucket;
            var client = _accountService.GetClient(content.Repository, out bucket);
            var key = content.GetOssKey();
            client.DeleteObject(bucket, key);
        }

        public void Delete(MediaFolder mediaFolder)
        {
            var key = mediaFolder.GetOssKey();
            string bucket;
            var client = _accountService.GetClient(mediaFolder.Repository.Name, out bucket);
            var keys = client.ListBlobsInFolder(bucket, mediaFolder)
                .Select(it => it.Key);
            if (keys.Any())
            {
                client.DeleteObjects(new DeleteObjectsRequest(bucket, keys.ToList(), true));
            }
        }

        public object Execute(IContentQuery<MediaContent> query)
        {
            var mediaQuery = (MediaContentQuery)query;

            QueryExpressionTranslator translator = new QueryExpressionTranslator();

            var blobs = translator
                .Translate(query.Expression, mediaQuery.MediaFolder, _accountService)
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
            _folderService.Create("/", repository.Name);

            Default.MediaFolderProvider fileMediaFolderProvider = new Default.MediaFolderProvider();
            foreach (var item in fileMediaFolderProvider.All(repository))
            {
                ImportMediaFolderDataCascading(fileMediaFolderProvider.Get(item));
            }
        }

        private void ImportMediaFolderDataCascading(MediaFolder mediaFolder)
        {
            var repository = mediaFolder.Repository.Name;
            Default.MediaContentProvider fileProvider = EngineContext.Current.Resolve<Default.MediaContentProvider>();
            string bucket;
            var client = _accountService.GetClient(repository, out bucket);

            //add media folder
            _folderService.Create(mediaFolder.FullName, repository);
            foreach (var item in fileProvider.All(mediaFolder))
            {
                item.ContentFile = new ContentFile
                {
                    FileName = item.FileName
                };
                var koobooMeta = item.Metadata;
                var meta = new ObjectMetadata();
                foreach (var field in item)
                {
                    meta.AddHeader(field.Key, field.Value?.ToString());
                }
                client.PutObject(bucket, item.FileName, item.PhysicalPath, meta);
            }
            Default.MediaFolderProvider fileMediaFolderProvider = new Default.MediaFolderProvider();
            foreach (var item in fileMediaFolderProvider.ChildFolders(mediaFolder))
            {
                ImportMediaFolderDataCascading(item);
            }
        }

        public byte[] GetContentStream(MediaContent content)
        {
            string bucket;
            var client = _accountService.GetClient(content.Repository, out bucket);
            var path = content.GetOssKey();
            var stream = new MemoryStream();
            client.GetObject(new GetObjectRequest(bucket, path), stream);
            return stream.ReadData();
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
