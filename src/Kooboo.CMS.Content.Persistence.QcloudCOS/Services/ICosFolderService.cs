using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kooboo.CMS.Content.Persistence.QcloudCOS.Models;
using Kooboo.CMS.Common.Runtime.Dependency;

namespace Kooboo.CMS.Content.Persistence.QcloudCOS.Services
{
    public interface ICosFolderService
    {
        CreateFolder Create(CreateFolderRequest request);

        DeleteFolder Delete(DeleteFolderRequest request);

        ListFolder List(ListFolderRequest request);
    }

    [Dependency(typeof(ICosFolderService))]
    public class CosFolderService : ICosFolderService
    {
        public CreateFolder Create(CreateFolderRequest request)
        {
            throw new NotImplementedException();
        }

        public DeleteFolder Delete(DeleteFolderRequest request)
        {
            throw new NotImplementedException();
        }

        public ListFolder List(ListFolderRequest request)
        {
            throw new NotImplementedException();
        }
    }

}
