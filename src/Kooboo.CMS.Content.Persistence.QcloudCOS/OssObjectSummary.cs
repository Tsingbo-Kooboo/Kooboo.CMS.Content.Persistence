using System.Runtime.Serialization;

namespace Kooboo.CMS.Content.Persistence.QcloudCOS
{
    [DataContract]
    public class OssObjectSummary
    {
        [DataMember(Name="size")]
        public int Size { get; set; }
        [DataMember(Name="name")]
        public string Name { get; set; }
        [DataMember(Name = "access_url")]
        public string AccessUrl { get; set; }
    }
}