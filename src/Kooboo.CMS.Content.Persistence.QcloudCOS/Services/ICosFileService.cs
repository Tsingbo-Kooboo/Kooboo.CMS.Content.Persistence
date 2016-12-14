using Kooboo.CMS.Common.Runtime.Dependency;
using Kooboo.CMS.Content.Persistence.QcloudCOS.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kooboo.CMS.Content.Models;
using Kooboo.CMS.Content.Persistence.QcloudCOS.Utilities;
using Kooboo.IO;
using Kooboo.Web.Script.Serialization;

namespace Kooboo.CMS.Content.Persistence.QcloudCOS.Services
{
    public interface ICosFileService
    {
        CreateFile Create(string path, string repository, Stream stream);

        FileDetail Get(string path, string repository);

        ListFile List(string path, string repository);

        MoveFile Move(string oldPath, string oldRepository, string newPath, string newRepository = null);

        DeleteFile Delete(string path, string repository);

        UpdateFile Update(string path, string repository, Dictionary<string, string> headers);
    }

    [Dependency(typeof(ICosFileService))]
    public class CosFileService : ICosFileService
    {
        private readonly IRequest _request;
        private readonly ICosAccountService _accountService;
        public CosFileService(IRequest request, ICosAccountService accountService)
        {
            _request = request;
            _accountService = accountService;
        }

        public FileDetail Get(string path, string repository)
        {
            var request = new FileDetailRequest();
            var context = new RequestContext
            {
                remotePath = MediaPathUtility.FilePath(path, repository),
                repository = repository
            };
            var account = _accountService.Get(repository);
            context.Sign(account);
            return _request.Get<FileDetail, FileDetailRequest, FileDetailData>(request, context);
        }

        public ListFile List(string path, string repository)
        {
            var request = new ListFileRequest();
            var context = new RequestContext
            {
                remotePath = MediaPathUtility.FolderPath(path, repository),
                repository = repository
            };
            var account = _accountService.Get(repository);
            context.Sign(account);
            return _request.Get<ListFile, ListFileRequest, ListFileData>(request, context);
        }

        public MoveFile Move(string oldPath, string oldRepository, string newPath, string newRepository = null)
        {
            if (string.IsNullOrEmpty(newRepository))
            {
                newRepository = oldRepository;
            }
            var request = new MoveFileRequest
            {
                dest_fileid = "/" + MediaPathUtility.FilePath(newPath, newRepository).TrimStart('/'),
            };
            var context = new RequestContext
            {
                remotePath = MediaPathUtility.FilePath(oldPath, oldRepository)
            };
            var account = _accountService.Get(oldRepository);
            context.SignOnce(account);
            return _request.Post<MoveFile, MoveFileRequest, string>(request, context);
        }

        public DeleteFile Delete(string path, string repository)
        {
            var request = new DeleteRequest();
            var context = new RequestContext
            {
                remotePath = MediaPathUtility.FilePath(path, repository),
                repository = repository
            };
            var account = _accountService.Get(repository);
            context.SignOnce(account);
            return _request.Post<DeleteFile, DeleteRequest, string>(request, context);
        }

        public UpdateFile Update(string path, string repository, Dictionary<string, string> headers)
        {
            var dict = new Dictionary<string, string>();
            foreach (var header in headers)
            {
                if (ConstValues.SystemHeaders.Contains(header.Key))
                {
                    dict[header.Key] = header.Value;
                }
                else
                {
                    dict[ConstValues.CustomHeaders.XCosMeta + header.Key] = header.Value;
                }
            }
            var request = new UpdateFileRequest
            {
                custom_headers = dict,
            };
            var context = new RequestContext
            {
                remotePath = MediaPathUtility.FilePath(path, repository),
                repository = repository
            };
            var account = _accountService.Get(repository);
            context.SignOnce(account);
            return _request.Post<UpdateFile, UpdateFileRequest, string>(request, context);
        }

        public CreateFile Create(string path, string repository, Stream stream)
        {
            var context = new RequestContext
            {
                remotePath = MediaPathUtility.FilePath(path, repository),
                repository = repository,
                contentType = ConstValues.MultipartFormData
            };
            var customHeaders = new Dictionary<string, string>
            {
                [ConstValues.ContentType] = IOUtility.MimeType(context.remotePath)
            };
            var request = new CreateFileRequest
            {
                biz_attr = customHeaders.ToJSON(),
                filecontent = stream.ReadData(),
                sha = SHA1Utility.GetFileSHA1(stream)
            };
            var account = _accountService.Get(repository);
            context.Sign(account);
            return _request.Post<CreateFile, CreateFileRequest, CreateFileData>(request, context);
        }
    }
}
