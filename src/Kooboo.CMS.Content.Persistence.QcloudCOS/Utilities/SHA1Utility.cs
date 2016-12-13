using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Kooboo.CMS.Content.Persistence.QcloudCOS.Utilities
{
    public static class SHA1Utility
    {
        public static string GetFileSHA1(Stream oFileStream)
        {
            var strResult = "";
            var osha1 = new SHA1CryptoServiceProvider();
            try
            {
                var arrbytHashValue = osha1.ComputeHash(oFileStream);
                oFileStream.Close();
                //由以连字符分隔的十六进制对构成的String，其中每一对表示value 中对应的元素；例如“F-2C-4A”
                var strHashData = BitConverter.ToString(arrbytHashValue);
                //替换-
                strHashData = strHashData.Replace("-", "");
                strResult = strHashData.ToLower();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return strResult;
        }
    }
}
