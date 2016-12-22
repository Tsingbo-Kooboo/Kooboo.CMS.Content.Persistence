using Aliyun.OSS;
using Kooboo.CMS.Content.Models;
using Kooboo.CMS.Content.Persistence.AliyunOSS.Utilities;
using Kooboo.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kooboo.CMS.Content.Persistence.AliyunOSS
{
    public static class OssClientExtensions
    {
        public static ObjectListing ListBlobsWithPrefix(this OssClient client, string bucket, string prefix)
        {
            return client.ListObjects(bucket, prefix);
        }

        public static IEnumerable<OssObjectSummary> ListBlobsInFolder(this OssClient client,
            string bucket,
            MediaFolder folder)
        {
            var folderPath = MediaPathUtility.FolderPath(folder.FullName, folder.Repository.Name);
            return client
                .ListObjects(new ListObjectsRequest(bucket)
                {
                    Delimiter = "/",
                    Prefix = folderPath
                })
                .ObjectSummaries
                .Where(it => !it.Key.EndsWith("/"));
        }

        //public static byte[] GetObjectData(this OssClient client, string bucket, string key)
        //{
        //    if (!client.DoesObjectExist(bucket, key))
        //    {
        //        return new byte[] { };
        //    }
        //    var obj = client.GetObject(bucket, key);
        //    using (var requestStream = obj.Content)
        //    {
        //        using (var fs = new MemoryStream())
        //        {
        //            int length = 4 * 1024;
        //            var buf = new byte[length];
        //            do
        //            {
        //                length = requestStream.Read(buf, 0, length);
        //                fs.Write(buf, 0, length);
        //            } while (length != 0);
        //            fs.Flush();
        //            fs.Position = 0;
        //            return fs.ReadData();
        //        }
        //    }
        //}
    }
}
