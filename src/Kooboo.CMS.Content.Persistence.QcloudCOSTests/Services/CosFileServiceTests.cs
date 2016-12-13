using Microsoft.VisualStudio.TestTools.UnitTesting;
using Kooboo.CMS.Content.Persistence.QcloudCOS.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kooboo.CMS.Content.Persistence.QcloudCOS.Models;
using Kooboo.CMS.Common.Runtime;

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
            //http://cdn-10046248.cos.myqcloud.com/sago.jpg
            context.remotePath = "sago.jpg";
            context.repository = "";
            var response = request.Get<FileDetail, FileDetailRequest, FileDetailData>(new FileDetailRequest(), context);
            Assert.AreEqual(0, response.code);

        }
    }
}