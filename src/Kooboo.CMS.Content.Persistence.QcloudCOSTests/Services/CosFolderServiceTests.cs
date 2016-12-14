using Microsoft.VisualStudio.TestTools.UnitTesting;
using Kooboo.CMS.Content.Persistence.QcloudCOS.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kooboo.CMS.Common.Runtime;

namespace Kooboo.CMS.Content.Persistence.QcloudCOS.Services.Tests
{
    [TestClass()]
    public class CosFolderServiceTests
    {
        private readonly IRequest request;
        private readonly ICosFileService cosFileService;
        private readonly ICosFolderService cosFolderService;
        public CosFolderServiceTests()
        {
            cosFileService = EngineContext.Current.Resolve<ICosFileService>();
            request = EngineContext.Current.Resolve<IRequest>();
            cosFolderService = EngineContext.Current.Resolve<ICosFolderService>();
        }
        const string RepositoryName = "SampleSite";


        [TestMethod()]
        public void CrudTest()
        {
            var createResponse = cosFolderService.Create("testCreate", RepositoryName);
            Assert.AreEqual(0, createResponse.code);
        }
    }
}