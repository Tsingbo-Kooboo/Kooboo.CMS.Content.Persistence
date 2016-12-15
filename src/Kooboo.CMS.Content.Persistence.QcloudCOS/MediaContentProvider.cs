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
using Kooboo.CMS.Common.Runtime.Dependency;
using Kooboo.CMS.Common.Runtime;
using Kooboo.CMS.Content.Persistence.QcloudCOS.Services;
using Kooboo.CMS.Content.Persistence.QcloudCOS.Extensions;
using Kooboo.CMS.Content.Persistence.QcloudCOS.Utilities;
using System.Net;

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

        public IEnumerable<MediaContent> Translate(IExpression expression,
             MediaFolder mediaFolder,
             ICosFolderService folderService,
             ICosFileService fileService,
             ICosAccountService accountService)
        {
            this.Visite(expression);
            if (!string.IsNullOrEmpty(fileName))
            {
                var blob = fileService.Get(fileName, mediaFolder.Repository.Name);
                if (blob.code != 0)
                {
                    return Enumerable.Empty<MediaContent>();
                }
                return new[] { blob.BlobToMediaContent(
                    new MediaContent(mediaFolder.Repository.Name, mediaFolder.FullName),
                    accountService)
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

                return fileService.List(mediaFolder.FullName, mediaFolder.Repository.Name, take)
                    .data.infos.Select(it => it.BlobToMediaContent(
                    new MediaContent(mediaFolder.Repository.Name, mediaFolder.FullName),
                    accountService));
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
            if (!fieldName.EqualsOrNullEmpty("FileName", StringComparison.OrdinalIgnoreCase)
                && !fieldName.EqualsOrNullEmpty("UUID", StringComparison.OrdinalIgnoreCase)
                && !fieldName.EqualsOrNullEmpty("UserKey", StringComparison.OrdinalIgnoreCase))
            {
                //throw new NotSupportedException("The azure storage provider only support query by FileName,UUID,UserKey.");
            }
            if (!string.IsNullOrEmpty(fileName) || !string.IsNullOrEmpty(prefix))
            {
                throw new NotSupportedException("The azure storage provider only support query by one condition.");
            }
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
    [Dependency(typeof(IMediaContentProvider), Order = 2)]
    [Dependency(typeof(IContentProvider<MediaContent>), Order = 2)]
    public class MediaContentProvider : IMediaContentProvider
    {
        private ICosFileService _fileService;
        private ICosFolderService _folderService;
        private ICosAccountService _accountService;
        public MediaContentProvider(
            ICosFileService fileService,
            ICosFolderService folderService,
            ICosAccountService accountService)
        {
            _fileService = fileService;
            _folderService = folderService;
            _accountService = accountService;
        }

        #region IMediaContentProvider
        public void Add(MediaContent content, bool overrided)
        {
            var repository = content.GetRepository();
            if (content.ContentFile != null)
            {
                content.FileName = content.ContentFile.FileName;
                content.UserKey = content.FileName;
                content.UUID = content.FileName;
                var result = _fileService.Create(content.FileName, repository.Name, content.ContentFile.Stream, overrided);
                content.VirtualPath = _accountService.ResourceUrl(repository.Name, result.data.source_url);
            }
        }

        public void Move(MediaFolder sourceFolder, string oldFileName, MediaFolder targetFolder, string newFileName)
        {
            var oldMediaContent = new MediaContent()
            {
                Repository = sourceFolder.Repository.Name,
                FolderName = sourceFolder.FullName,
                UUID = oldFileName,
                FileName = oldFileName
            };
            var newMediaContent = new MediaContent()
            {
                Repository = targetFolder.Repository.Name,
                FolderName = targetFolder.FullName,
                UUID = newFileName,
                FileName = newFileName
            };

            MoveContent(oldMediaContent, newMediaContent);
        }

        private void MoveContent(MediaContent oldMediaContent, MediaContent newMediaContent)
        {
            var oldKey = oldMediaContent.GetMediaBlobPath();
            var newKey = newMediaContent.GetMediaBlobPath();

            _fileService.Move(oldKey, oldMediaContent.Repository, newKey, newMediaContent.Repository);
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
            var key = content.GetMediaBlobPath();
            _fileService.Delete(key, content.Repository);
        }

        public void Delete(MediaFolder mediaFolder)
        {
            _folderService.Delete(mediaFolder.FullName, mediaFolder.Repository.Name);
        }

        public object Execute(IContentQuery<MediaContent> query)
        {
            var mediaQuery = (MediaContentQuery)query;

            QueryExpressionTranslator translator = new QueryExpressionTranslator();

            var blobs = translator.Translate(
                query.Expression,
                mediaQuery.MediaFolder,
                _folderService,
                _fileService,
                _accountService)
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
            Default.MediaContentProvider fileProvider = EngineContext.Current.Resolve<Default.MediaContentProvider>();

            //add media folder
            MediaFolderProvider folderProvider = new MediaFolderProvider(_accountService,
                _folderService,
                _fileService);
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
            var pathRepository = MediaPathUtility.GetPathRepository(content.Url);
            var detail = _fileService.Get(pathRepository.Key, pathRepository.Value);
            using (var webclient = new WebClient())
            {
                return webclient.DownloadData(detail.data.source_url);
            }
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
