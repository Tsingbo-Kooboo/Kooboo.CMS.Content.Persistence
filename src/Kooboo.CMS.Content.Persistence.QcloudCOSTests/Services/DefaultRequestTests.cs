using Microsoft.VisualStudio.TestTools.UnitTesting;
using Kooboo.CMS.Content.Persistence.QcloudCOS.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kooboo.CMS.Content.Persistence.QcloudCOS.Models;

namespace Kooboo.CMS.Content.Persistence.QcloudCOS.Services.Tests
{
    [TestClass()]
    public class DefaultRequestTests
    {
        private IRequest request;
        private RequestContext context;
        [TestInitialize]
        public void Init()
        {
            var accountService = new CosAccountService();
            request = new DefaultRequest(accountService);
            context = new RequestContext
            {
                repository = "test"
            };
        }

        [TestMethod()]
        public void GetTest()
        {
          
            Assert.Fail();
        }
    }
}