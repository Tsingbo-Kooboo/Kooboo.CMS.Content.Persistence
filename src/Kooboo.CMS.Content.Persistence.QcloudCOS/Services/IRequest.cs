using Kooboo.CMS.Common.Runtime.Dependency;
using Kooboo.CMS.Content.Persistence.QcloudCOS.Models;
using Kooboo.Web.Script.Serialization;
using Kooboo.Web.Url;
using QCloud.CosApi.Common;
using QCloud.CosApi.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Kooboo.CMS.Content.Persistence.QcloudCOS.Services
{
    public interface IRequest
    {
        T Get<T, R>(RequestBase request, RequestContext context) where T : ResponseBase<R> where R : class;

        T Post<T, R>(RequestBase request, RequestContext context) where T : ResponseBase<R> where R : class;

    }

    [Dependency(typeof(IRequest))]
    public class DefaultRequest : IRequest
    {
        private readonly ICosAccountService _accountService;
        public DefaultRequest(ICosAccountService accountService)
        {
            _accountService = accountService;
        }

        const string COSAPI_CGI_URL = "http://sh.file.myqcloud.com/files/v2/";
        private string generateURL(RequestContext context)
        {
            var account = _accountService.Get(context.repository);
            return UrlUtility.Combine(
                COSAPI_CGI_URL,
                account.AppId.ToString(),
                account.BucketName,
                HttpUtils.EncodeRemotePath(context.remotePath));
        }

        public T Get<T, R>(RequestBase request, RequestContext context)
            where T : ResponseBase<R>
            where R : class
        {
            try
            {
                var url = generateURL(context);
                var fields = request
                    .GetType()
                    .GetProperties()
                    .Where(it => it.PropertyType != typeof(byte[]));
                foreach (var item in fields)
                {
                    url = url.AddQueryParam(item.Name, HttpUtility.UrlEncode(item.GetValue(request)?.ToString()));
                }
                HttpWebRequest httpRequest = (HttpWebRequest)WebRequest.Create(url);
                httpRequest.Accept = CosDefaultValue.ACCEPT;
                httpRequest.KeepAlive = true;
                httpRequest.UserAgent = CosDefaultValue.USER_AGENT_VERSION;
                httpRequest.Timeout = 60 * 1000;
                httpRequest.ContentType = context.contentType;
                foreach (var item in context.headers)
                {
                    httpRequest.Headers.Add(item.Key, item.Value);
                }
                var response = httpRequest.GetResponse();
                using (var s = response.GetResponseStream())
                {
                    var reader = new StreamReader(s, Encoding.UTF8);
                    return JsonHelper.Deserialize<T>(reader.ReadToEnd());
                }
            }
            catch (WebException we)
            {
                if (we.Status == WebExceptionStatus.ProtocolError)
                {
                    using (var s = we.Response.GetResponseStream())
                    {
                        var reader = new StreamReader(s, Encoding.UTF8);
                        return JsonHelper.Deserialize<T>(reader.ReadToEnd());
                    }
                }
                else
                {
                    throw we;
                }
            }
            catch (Exception e)
            {
                Kooboo.HealthMonitoring.Log.LogException(e);
                throw;
            }
        }

        public T Post<T, R>(RequestBase request, RequestContext context)
            where T : ResponseBase<R>
            where R : class
        {
            try
            {
                var url = generateURL(context);
                var props = request
                    .GetType()
                    .GetProperties();
                var fields = props.Where(it => it.PropertyType != typeof(byte[]));
                var bytes = props.Where(it => it.PropertyType == typeof(byte[]));

                HttpWebRequest httpRequest = (HttpWebRequest)WebRequest.Create(url);
                httpRequest.Accept = CosDefaultValue.ACCEPT;
                httpRequest.KeepAlive = true;
                httpRequest.UserAgent = CosDefaultValue.USER_AGENT_VERSION;
                httpRequest.Timeout = 60 * 1000;
                httpRequest.Method = "POST";
                foreach (var item in context.headers)
                {
                    httpRequest.Headers.Add(item.Key, item.Value);
                }
                var memStream = new MemoryStream();
                if (context.contentType == "application/json")
                {
                    var dict = new Dictionary<string, string>();
                    foreach (var item in fields)
                    {
                        dict[item.Name] = item.GetValue(request)?.ToString();
                    }
                    var json = JsonHelper.ToJSON(dict);
                    var jsonByte = Encoding.GetEncoding("utf-8").GetBytes(json.ToString());
                    memStream.Write(jsonByte, 0, jsonByte.Length);
                }
                else
                {
                    var boundary = "---------------" + DateTime.Now.Ticks.ToString("x");
                    var beginBoundary = Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");
                    var endBoundary = Encoding.ASCII.GetBytes("\r\n--" + boundary + "--\r\n");
                    httpRequest.ContentType = "multipart/form-data; boundary=" + boundary;

                    var strBuf = new StringBuilder();
                    foreach (var item in fields)
                    {
                        strBuf.Append("\r\n--" + boundary + "\r\n");
                        strBuf.Append("Content-Disposition: form-data; name=\"" + item.Name + "\"\r\n\r\n");
                        strBuf.Append(item.GetValue(request)?.ToString());
                    }

                    var paramsByte = Encoding.GetEncoding("utf-8").GetBytes(strBuf.ToString());
                    memStream.Write(paramsByte, 0, paramsByte.Length);

                    if (bytes.Any())
                    {
                        foreach (var item in bytes)
                        {
                            using (var fileStream = new MemoryStream((byte[])item.GetValue(request)))
                            {
                                memStream.Write(beginBoundary, 0, beginBoundary.Length);
                                const string filePartHeader =
                                    "Content-Disposition: form-data; name=\"fileContent\"; filename=\"{0}\"\r\n" +
                                    "Content-Type: application/octet-stream\r\n\r\n";
                                var headerText = string.Format(filePartHeader, item.Name);
                                var headerbytes = Encoding.UTF8.GetBytes(headerText);
                                memStream.Write(headerbytes, 0, headerbytes.Length);

                                if (context.offset == -1)
                                {
                                    var buffer = new byte[1024];
                                    int bytesRead;
                                    while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
                                    {
                                        memStream.Write(buffer, 0, bytesRead);
                                    }
                                }
                                else
                                {
                                    var buffer = new byte[SLICE_SIZE.SLIZE_SIZE_1M];
                                    int bytesRead;
                                    fileStream.Seek(context.offset, SeekOrigin.Begin);
                                    bytesRead = fileStream.Read(buffer, 0, buffer.Length);
                                    memStream.Write(buffer, 0, bytesRead);
                                }
                            }
                        }
                    }
                    memStream.Write(endBoundary, 0, endBoundary.Length);
                }

                httpRequest.ContentLength = memStream.Length;
                var requestStream = httpRequest.GetRequestStream();
                memStream.Position = 0;
                var tempBuffer = new byte[memStream.Length];
                memStream.Read(tempBuffer, 0, tempBuffer.Length);
                memStream.Close();

                requestStream.Write(tempBuffer, 0, tempBuffer.Length);
                requestStream.Close();

                var response = httpRequest.GetResponse();
                using (var s = response.GetResponseStream())
                {
                    var reader = new StreamReader(s, Encoding.UTF8);
                    return JsonHelper.Deserialize<T>(reader.ReadToEnd());
                }
            }
            catch (WebException we)
            {
                if (we.Status == WebExceptionStatus.ProtocolError)
                {
                    using (var s = we.Response.GetResponseStream())
                    {
                        var reader = new StreamReader(s, Encoding.UTF8);
                        return JsonHelper.Deserialize<T>(reader.ReadToEnd());
                    }
                }
                else
                {
                    throw we;
                }
            }
            catch (Exception e)
            {
                Kooboo.HealthMonitoring.Log.LogException(e);
                throw;
            }
        }
    }
}
