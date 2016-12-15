using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using System.Linq;
using System;

namespace Kooboo.CMS.Content.Persistence.QcloudCOS
{
    public class ObjectMetadata
    {
        public string ContentType { get; set; }

        public Dictionary<string, string> UserMetadata { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    }

    [DataContract]
    public class OssObject
    {
        [DataMember(Name = "code")]
        public int Code { get; set; }
        [DataMember(Name = "message")]
        public string Message { get; set; }
        [DataMember(Name = "data")]
        public Metadata Data { get; set; } = new Metadata();
    }

    [DataContract]
    public class Metadata
    {
        [DataMember(Name = "access_url")]
        public string AccessUrl { get; set; }
        [DataMember(Name = "authority")]
        public string Authority { get; set; }
        [DataMember(Name = "biz_attr")]
        public string BizAttr { get; set; }
        [DataMember(Name = "ctime")]
        public string Ctime { get; set; }
        [DataMember(Name = "custom_headers")]
        public Dictionary<string, string> CustomHeaders { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        [DataMember(Name = "filelen")]
        public string Filelen { get; set; }
        [DataMember(Name = "filesize")]
        public string Filesize { get; set; }
        [DataMember(Name = "mtime")]
        public string Mtime { get; set; }
        [DataMember(Name = "name")]
        public string Name { get; set; }
        [DataMember(Name = "sha")]
        public string Sha { get; set; }
        [DataMember(Name = "source_url")]
        public string SourceUrl { get; set; }

        const string CustomKeyPrefix = "x-cos-meta-";
        private string[] SystemKeys = new[] {
            SystemHeaderKey.CacheControl,
            SystemHeaderKey.ContentType,
            SystemHeaderKey.ContentDisposition,
            SystemHeaderKey.ContentLanguage,
            SystemHeaderKey.ContentEncoding
        };

        public class SystemHeaderKey
        {
            public const string CacheControl = "Cache-Control";
            public const string ContentType = "Content-Type";
            public const string ContentDisposition = "Content-Disposition";
            public const string ContentLanguage = "Content-Language";
            public const string ContentEncoding = "Content-Encoding";
        }

        public void SetMetadata(string key, string value)
        {
            if (SystemKeys.Contains(key, StringComparer.OrdinalIgnoreCase))
            {
                CustomHeaders[key] = value;
            }
            CustomHeaders[$"{CustomKeyPrefix}{key}"] = value;
        }

        public string GetMetadata(string key)
        {
            string response;
            if (!CustomHeaders.TryGetValue(key, out response))
            {
                CustomHeaders.TryGetValue($"{CustomKeyPrefix}{key}", out response);
            }
            return response;
        }

        public void RemoveMetadata(string key)
        {
            if (!CustomHeaders.Remove(key))
            {
                CustomHeaders.Remove($"{CustomKeyPrefix}{key}");
            }
        }
    }
}