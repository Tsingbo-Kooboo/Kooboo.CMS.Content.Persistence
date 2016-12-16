using Microsoft.VisualStudio.TestTools.UnitTesting;
using Kooboo.CMS.Content.Persistence.AliyunOSS.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kooboo.CMS.Common.Runtime;

namespace Kooboo.CMS.Content.Persistence.AliyunOSS.Services.Tests
{
    [TestClass()]
    public class MediaFolderServiceTests
    {
        private readonly IAccountService _accountService;
        private readonly IMediaFolderService _folderService;
        public MediaFolderServiceTests()
        {
            _accountService = EngineContext.Current.Resolve<IAccountService>();
            _folderService = EngineContext.Current.Resolve<IMediaFolderService>();
        }
        const string RepositoryName = "SampleSite";

        [TestMethod()]
        public void CrudTest()
        {
            // create
            for (int i = 1; i < 4; i++)
            {
                var folder = "root/sub" + i;
                _folderService.Create(folder, RepositoryName);
            }
            // list
            var listResponse = _folderService.List("root", RepositoryName);

        }
    }
}