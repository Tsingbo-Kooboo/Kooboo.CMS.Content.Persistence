using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kooboo.CMS.Content.Persistence.QcloudCOS.Models
{
    public abstract class ResponseBase<T>
        where T : class
    {
        public int code { get; set; }

        public string message { get; set; }

        public T data { get; set; }
    }
}
