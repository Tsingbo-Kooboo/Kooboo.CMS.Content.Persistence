using QCloud.CosApi.Api;

namespace Kooboo.CMS.Content.Persistence.QcloudCOS
{
    public class CosClient : CosCloud
    {
        public CosClient(int appId, string secretId, string secretKey, int timeOut = 60) 
            : base(appId, secretId, secretKey, timeOut)
        {
        }
    }
}