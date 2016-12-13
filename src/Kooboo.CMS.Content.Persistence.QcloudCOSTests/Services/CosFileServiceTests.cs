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

namespace Kooboo.CMS.Content.Persistence.QcloudCOS.Services.Tests
{
    [TestClass()]
    public class CosFileServiceTests
    {
        private IRequest request;
        private RequestContext context;

        private ICosFileService cosFileService;

        [TestInitialize]
        public void Init()
        {
            var accountService = new CosAccountService();
            request = new DefaultRequest(accountService);
            context = new RequestContext
            {
                repository = "test"
            };

            cosFileService = EngineContext.Current.Resolve<ICosFileService>();
        }

        [TestMethod()]
        public void GetTest()
        {
            //http://cdn-10046248.cos.myqcloud.com/SampleSite/Media/home/sago.jpg
            var response = cosFileService.Get("home/sago.jpg", "SampleSite");
            Assert.AreEqual(0, response.code);
        }

        [TestMethod()]
        public void ListTest()
        {
            var response = cosFileService.List("home", "SampleSite");
            Assert.AreEqual(0, response.code);
        }

        [TestMethod()]
        public void DeleteTest()
        {
            var response = cosFileService.Delete("home/sago.jpg", "SampleSite");
            Assert.AreEqual(0, response.code);
        }

        [TestMethod()]
        public void CreateTest()
        {
            using (var stream = TestHelper.GetStream("98.jpg"))
            {
                var response = cosFileService.Create("home/98.jpg", "SampleSite", stream);

            }
        }
    }
}