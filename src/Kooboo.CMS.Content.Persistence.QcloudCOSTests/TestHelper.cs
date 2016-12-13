using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kooboo.CMS.Content.Persistence.QcloudCOSTests
{
    public class TestHelper
    {
        public static Stream GetStream(string name)
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", name);
            var stream = new FileStream(path, FileMode.Open);
            stream.Position = 0;
            return stream;
        }
    }
}
