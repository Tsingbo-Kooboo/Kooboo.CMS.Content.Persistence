using Kooboo.CMS.Common.Runtime.Dependency;
using Kooboo.Web.Url;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Kooboo.CMS.Sites.Controllers
{
    [Dependency(typeof(ResourceController), Order = 50)]
    public class QcloudResourceController : ResourceController
    {
        public override ActionResult ResizeImage(string url, int width, int height, bool? preserverAspectRatio, int? quality)
        {
            if (UrlUtility.IsAbsoluteUrl(url))
            {
                var value = $"image/resize,m_fill,w_{width},h_{height},limit_0/auto-orient,0/quality,q_{quality ?? 90}";
                //?x-oss-process=image/resize,m_fill,w_440,h_250,limit_0/auto-orient,0/quality,q_90
                var u = UrlUtility.AddQueryParam(url, "x-oss-process", value);
                return Redirect(u);
            }
            return base.ResizeImage(url, width, height, preserverAspectRatio, quality);
        }
    }
}
