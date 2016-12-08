using Aliyun.OSS;
using Kooboo.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kooboo.CMS.Content.Persistence.QcloudCOS
{
    public static class OssClientExtensions
    {
        public static ObjectListing ListBlobsWithPrefix(this OssClient client, string bucket, string prefix)
        {
            return client.ListObjects(bucket, prefix);
        }

        public static IEnumerable<OssObjectSummary> ListBlobsInFolder(this OssClient client, string bucket, string folder)
        {
            return client.ListBlobsWithPrefix(bucket, folder)
                .ObjectSummaries
                .Where(it => !it.Key.Substring(folder.Length).Contains("/"));
        }

        public static byte[] GetObjectData(this OssClient client, string bucket, string key)
        {
            if (!client.DoesObjectExist(bucket, key))
            {
                return new byte[] { };
            }
            var obj = client.GetObject(bucket, key);
            using (var requestStream = obj.Content)
            {
                using (var fs = new MemoryStream())
                {
                    int length = 4 * 1024;
                    var buf = new byte[length];
                    do
                    {
                        length = requestStream.Read(buf, 0, length);
                        fs.Write(buf, 0, length);
                    } while (length != 0);
                    fs.Flush();
                    fs.Position = 0;
                    return fs.ReadData();
                }
            }
        }
    }
}
