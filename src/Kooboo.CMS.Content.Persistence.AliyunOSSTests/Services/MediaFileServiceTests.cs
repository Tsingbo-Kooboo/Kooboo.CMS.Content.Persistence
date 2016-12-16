using Microsoft.VisualStudio.TestTools.UnitTesting;
using Kooboo.CMS.Content.Persistence.AliyunOSS.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kooboo.CMS.Common.Runtime;
using System.IO;
using Kooboo.CMS.Content.Persistence.AliyunOSS.Models;

namespace Kooboo.CMS.Content.Persistence.AliyunOSS.Services.Tests
{
    [TestClass()]
    public class MediaFileServiceTests
    {
        private readonly IMediaFileService _fileService;
        public MediaFileServiceTests()
        {
            _fileService = EngineContext.Current.Resolve<IMediaFileService>();
        }

        const string RepositoryName = "SampleSite";

        [TestMethod()]
        public void CrudTest()
        {
            // list
            var listResponse = _fileService.List("home", RepositoryName);
            var listCount = listResponse.Count();
            // create
            var description = "Test Description";
            var fileName = $"home/{DateTime.Now.ToString("yyyyMMdd-hhmmss")}.jpg";
            using (var stream = TestHelper.GetStream("sago.jpg"))
            {
                var mediaFile = _fileService.Create(fileName,
                    RepositoryName,
                    stream,
                    new Dictionary<string, string>
                    {
                        [ConstValues.Metadata.Description] = description
                    });
            }
            // update
            //_fileService.Update(fileName, RepositoryName, new Dictionary<string, string>
            //{
            //    [ConstValues.Metadata.UserId] = "Admin",
            //    [ConstValues.Metadata.Description] = description
            //});
            // get
            var getResponse = _fileService.Get(fileName, RepositoryName);
            var meta = getResponse.Metadata;
            Assert.AreEqual(description, meta.Description);
            // delete
            _fileService.Delete(fileName, RepositoryName);
        }
    }
}