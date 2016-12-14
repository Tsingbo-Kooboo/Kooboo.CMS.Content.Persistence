using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kooboo.CMS.Content.Persistence.QcloudCOS.Models;
using Kooboo.CMS.Common.Runtime.Dependency;
using Kooboo.CMS.Content.Persistence.QcloudCOS.Utilities;

namespace Kooboo.CMS.Content.Persistence.QcloudCOS.Services
{
    public interface ICosFolderService
    {
        CreateFolder Create(string path, string repository);

        DeleteFolder Delete(DeleteRequest request);

        ListFolder List(ListFolderRequest request);
    }

    [Dependency(typeof(ICosFolderService))]
    public class CosFolderService : ICosFolderService
    {
        private readonly IRequest _request;
        private readonly ICosAccountService _accountService;
        public CosFolderService(IRequest request, ICosAccountService accountService)
        {
            _request = request;
            _accountService = accountService;
        }
        public CreateFolder Create(string path, string repository)
        {
            var request = new CreateFolderRequest
            {
                
            };
            var context = new RequestContext
            {
                remotePath = MediaPathUtility.FolderPath(path, repository)
            };
            var account = _accountService.Get(repository);
            context.Sign(account);
            return _request.Post<CreateFolder, CreateFolderRequest, CreateFolderData>(request, context);
        }

        public DeleteFolder Delete(DeleteRequest request)
        {
            throw new NotImplementedException();
        }

        public ListFolder List(ListFolderRequest request)
        {
            throw new NotImplementedException();
        }
    }

}
