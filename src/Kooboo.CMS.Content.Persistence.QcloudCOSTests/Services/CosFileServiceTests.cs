using Microsoft.VisualStudio.TestTools.UnitTesting;
using Kooboo.CMS.Content.Persistence.QcloudCOS.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kooboo.CMS.Content.Persistence.QcloudCOS.Models;
using Kooboo.CMS.Common.Runtime;
using Kooboo.CMS.Content.Persistence.QcloudCOSTests;
using Kooboo.CMS.Content.Persistence.QcloudCOS.Utilities;

namespace Kooboo.CMS.Content.Persistence.QcloudCOS.Services.Tests
{
    [TestClass()]
    public class CosFileServiceTests
    {
        private readonly IRequest request;
        private readonly ICosFileService cosFileService;

        public CosFileServiceTests()
        {
            cosFileService = EngineContext.Current.Resolve<ICosFileService>();
            request = EngineContext.Current.Resolve<IRequest>();
        }

        [TestInitialize]
        public void Init()
        {
        }

        const string RepositoryName = "SampleSite";
        [TestMethod()]
        public void CrudTest()
        {
            var remotePath = $"home/{DateTime.Now.ToString("yyyyMMdd-hhmmss")}.jpg";

            //create
            using (var stream = TestHelper.GetStream("98.jpg"))
            {
                var createResponse = cosFileService.Create(remotePath, RepositoryName, stream);
                Assert.AreEqual(0, createResponse.code);
            }

            //get
            var getResponse = cosFileService.Get(remotePath, RepositoryName);
            Assert.AreEqual(0, getResponse.code);

            //update
            var customerHeaders = new Dictionary<string, string>
            {
                ["Alt"] = "test alt"
            };
            var updateResponse = cosFileService.Update(remotePath, RepositoryName, customerHeaders);
            Assert.AreEqual(0, updateResponse.code);

            //move
            var newPath = "move-" + remotePath;
            var moveResponse = cosFileService.Move(remotePath, RepositoryName, newPath, RepositoryName);
            Assert.AreEqual(0, moveResponse.code);

            //delete
            var deleteResponse = cosFileService.Delete(newPath, RepositoryName);
            Assert.AreEqual(0, deleteResponse.code);

            //list
            var listResponse = cosFileService.List("move-home", RepositoryName);
            Assert.AreEqual(0, listResponse.code);
            foreach (var info in listResponse.data.infos)
            {
                var o = MediaPathUtility.GetPathRepository(info.source_url);
                cosFileService.Delete(o.Key, o.Value);
            }
        }
    }
}