using Kooboo.CMS.Content.Models;
using Kooboo.CMS.Content.Persistence.QiniuKodo.Extensions;
using Kooboo.CMS.Content.Persistence.QiniuKodo.Models;
using Kooboo.CMS.Content.Persistence.QiniuKodo.Services;
using Kooboo.CMS.Content.Query.Expressions;
using Kooboo.CMS.Content.Query.Translator;
using Kooboo.Web.Url;
using Qiniu.Storage.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kooboo.CMS.Content.Persistence.QiniuKodo
{
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
            var repository = mediaFolder.Repository.Name;
            string bucket;
            var client = accountService.GetBucketManager(repository, out bucket);
            this.Visite(expression);
            var key = mediaFolder.GetMediaKey();
            if (!string.IsNullOrEmpty(fileName))
            {
                var fileKey = UrlUtility.Combine(key, fileName);
                var status = client.stat(bucket, fileKey);
                if (!status.ResponseInfo.isOk())
                {
                    return Enumerable.Empty<MediaContent>();
                }
                var info = new KoobooMediaInfo(fileKey);

                return new[] {
                    BlobToMediaContent(info,status, accountService)
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
                return client.listFiles(bucket, key, "", take, "")
                    .Items
                    .Skip(skip)
                    .Take(take)
                    .Select(it => BlobToMediaContent(it, accountService));
            }
        }
        private MediaContent BlobToMediaContent(KoobooMediaInfo info, StatResult metaData, IAccountService accountService)
        {
            var modifiedDate = metaData.PutTime.ToUtcTime();
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
                Size = metaData.Fsize,
                ContentFile = new ContentFile
                {
                    FileName = fileName,
                    Name = fileName,
                    //Stream = metaData.
                }
            };
        }

        private MediaContent BlobToMediaContent(FileDesc metaData, IAccountService accountService)
        {
            var info = new KoobooMediaInfo(metaData.Key);
            var url = accountService.ResolveUrl(info.FilePath, info.Repository);
            var modifiedDate = metaData.PutTime.ToUtcTime();
            var fileName = info.FileName;
            return new MediaContent(info.Repository, info.Folder)
            {
                VirtualPath = url,
                UtcLastModificationDate = modifiedDate,
                UtcCreationDate = modifiedDate,
                FileName = fileName,
                UserKey = fileName,
                UUID = fileName,
                Size = metaData.Fsize,
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
}
