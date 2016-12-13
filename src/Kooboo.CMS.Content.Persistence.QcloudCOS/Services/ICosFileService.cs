using Kooboo.CMS.Common.Runtime.Dependency;
using Kooboo.CMS.Content.Persistence.QcloudCOS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kooboo.CMS.Content.Persistence.QcloudCOS.Services
{
    public interface ICosFileService
    {
        CreateFile Create(CreateFileRequest request);

        FileDetail Get(FileDetailRequest request);

        ListFile List(ListFileRequest request);

        MoveFile Move(MoveFileRequest request);

        UpdateFile Update(UpdateFileRequest request);
    }

    [Dependency(typeof(ICosFileService))]
    public class CosFileService : ICosFileService
    {
        private readonly IRequest _request;
        public CosFileService(IRequest request)
        {
            _request = request;
        }

        public FileDetail Get(FileDetailRequest request)
        {
            var x = _request.Get<FileDetail, FileDetailRequest, FileDetailData>(request, new RequestContext { });
            throw new NotImplementedException();
        }

        public ListFile List(ListFileRequest request)
        {
            throw new NotImplementedException();
        }

        public MoveFile Move(MoveFileRequest request)
        {
            throw new NotImplementedException();
        }
        /*
        public string UpdateFile(string bucketName, string remotePath, Dictionary<string, string> parameterDic = null)
        {
            var url = generateURL(bucketName, remotePath);
            var data = new Dictionary<string, object>();
            var customerHeaders = new Dictionary<string, object>();

            data.Add("op", "update");

            //接口中的flag统一cgi设置

            //将forbid设置到data中，这个不用设置flag
            addParameter(CosParameters.PARA_FORBID, ref data, ref parameterDic);

            //将biz_attr设置到data中
            addParameter(CosParameters.PARA_BIZ_ATTR, ref data, ref parameterDic);

            //将authority设置到data中
            addAuthority(ref data, ref parameterDic);

            //将customer_headers设置到data["custom_headers"]中
            if (parameterDic != null && setCustomerHeaders(ref customerHeaders, ref parameterDic))
            {
                data.Add(CosParameters.PARA_CUSTOM_HEADERS, customerHeaders);
            }

            var sign = Sign.SignatureOnce(appId, secretId, secretKey, (remotePath.StartsWith("/") ? "" : "/") + remotePath, bucketName);
            var header = new Dictionary<string, string>();
            header.Add(CosParameters.Authorization, sign);
            header.Add(CosParameters.PARA_CONTENT_TYPE, "application/json");
            return httpRequest.SendRequest(url, ref data, HttpMethod.Post, ref header, timeOut);
        }
        */
        public UpdateFile Update(UpdateFileRequest request)
        {

            throw new NotImplementedException();
        }

        public CreateFile Create(CreateFileRequest request)
        {
            throw new NotImplementedException();
        }
    }
}
