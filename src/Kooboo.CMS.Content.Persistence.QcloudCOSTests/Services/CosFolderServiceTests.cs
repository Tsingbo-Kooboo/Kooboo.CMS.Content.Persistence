using Microsoft.VisualStudio.TestTools.UnitTesting;
using Kooboo.CMS.Content.Persistence.QcloudCOS.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kooboo.CMS.Common.Runtime;
using Kooboo.CMS.Content.Persistence.QcloudCOS.Utilities;

namespace Kooboo.CMS.Content.Persistence.QcloudCOS.Services.Tests
{
    [TestClass()]
    public class CosFolderServiceTests
    {
        private readonly IRequest request;
        private readonly ICosFolderService cosFolderService;
        public CosFolderServiceTests()
        {
            request = EngineContext.Current.Resolve<IRequest>();
            cosFolderService = EngineContext.Current.Resolve<ICosFolderService>();
        }
        const string RepositoryName = "SampleSite";

        [TestMethod()]
        public void CrudTest()
        {
            var folderName = Guid.NewGuid().ToString();
            // create
            var createResponse = cosFolderService.Create(folderName, RepositoryName);
            Assert.AreEqual(0, createResponse.code);
            Assert.IsTrue(createResponse.data.resource_path.Contains(folderName));

            // get
            var getResponse = cosFolderService.Get(folderName, RepositoryName);
            Assert.AreEqual(0, getResponse.code);

            // delete
            var deleteResponse = cosFolderService.Delete(folderName, RepositoryName);
            Assert.AreEqual(0, deleteResponse.code);

            // list
            var listResponse = cosFolderService.List("/", RepositoryName);
            Assert.AreEqual(0, listResponse.code);
            foreach (var item in listResponse.data.infos)
            {
                cosFolderService.Delete(item.name, RepositoryName);
            }
        }
    }
}