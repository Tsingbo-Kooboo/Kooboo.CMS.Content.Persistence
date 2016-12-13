using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kooboo.CMS.Content.Persistence.QcloudCOS.Models
{
    public abstract class RequestBase
    {
        public abstract string op { get; }
    }

    public class RequestContext
    {
        [Required]
        public string remotePath { get; set; }

        [Required]
        public string repository { get; set; }

        public Dictionary<string, string> headers { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public string contentType { get; set; }

        public int offset { get; set; } = -1;
    }
}
