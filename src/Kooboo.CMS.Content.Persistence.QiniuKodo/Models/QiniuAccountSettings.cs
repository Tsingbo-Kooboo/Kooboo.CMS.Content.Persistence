#region License
// 
// Copyright (c) 2013, Kooboo team
// 
// Licensed under the BSD License
// See the file LICENSE.txt for details.
// 
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization;
using Kooboo.Web.Script.Serialization;
using System.Xml.Serialization;

namespace Kooboo.CMS.Content.Persistence.QiniuKodo
{
    [DataContract]
    public class QiniuAccountSettings
    {
        [XmlIgnore]
        private static QiniuAccountSettings instance = null;
        static QiniuAccountSettings()
        {
            string settingFile = GetSettingFile();
            if (File.Exists(settingFile))
            {
                var text = File.ReadAllText(settingFile);
                instance = JsonHelper.Deserialize<QiniuAccountSettings>(text);
            }
            else
            {
                instance = new QiniuAccountSettings()
                {
                    Endpoint = "http://oss-cn-shanghai.aliyuncs.com",
                    AccessKeyId = "Your Access Key Id Here",
                    AccessKeySecret = "Your Access Key Secrect Here",
                    BucketName = "Your Bucket Name Here",
                    CustomDomain = "http://cdn.kooboo.com",
                    RepositoryBuckets = new[]
                    {
                        new RepositoryBucket
                        {
                            RepositoryName = "sample",
                            BucketName="Sample",
                            Endpoint = "http://oss-cn-shanghai.aliyuncs.com",
                            CustomDomain = "http://sample.kooboo.com"
                        }
                    }
                };
                Save(instance);
            }
        }

        public static void Save(QiniuAccountSettings instance)
        {
            string settingFile = GetSettingFile();
            var json = JsonHelper.ToJSON(instance);
            File.WriteAllText(settingFile, json, Encoding.UTF8);
        }
        private static string GetSettingFile()
        {
            return Path.Combine(Settings.BinDirectory, "QiniuSettings.json");
        }
        public static QiniuAccountSettings Instance
        {
            get
            {
                return instance;
            }
            set
            {
                instance = value;
                Save(instance);
            }
        }
        [DataMember]
        public string Endpoint { get; set; }
        [DataMember]
        public string AccessKeyId { get; set; }
        [DataMember]
        public string AccessKeySecret { get; set; }
        [DataMember]
        public string BucketName { get; set; }

        private string domain;
        /// <summary>
        /// 自定义域名
        /// </summary>
        [DataMember]
        public string CustomDomain
        {
            get
            {
                if (string.IsNullOrEmpty(domain))
                {
                    var uri = new Uri(Endpoint);
                    return $"{uri.Scheme}://{BucketName}.{uri.Host}";
                }
                return domain;
            }
            set
            {
                domain = value;
            }
        }

        /// <summary>
        /// RepositoryName~BucketName
        /// </summary>
        [DataMember]
        public IEnumerable<RepositoryBucket> RepositoryBuckets { get; set; } = Enumerable.Empty<RepositoryBucket>();
    }

    [DataContract]
    public class RepositoryBucket
    {
        [DataMember]
        public string Endpoint { get; set; }

        [DataMember]
        public string RepositoryName { get; set; }

        [DataMember]
        public string BucketName { get; set; }

        /// <summary>
        /// 自定义域名
        /// </summary>
        [DataMember]
        public string CustomDomain { get; set; }
    }
}
