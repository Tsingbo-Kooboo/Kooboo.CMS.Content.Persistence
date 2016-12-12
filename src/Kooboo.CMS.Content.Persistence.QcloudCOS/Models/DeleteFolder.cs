using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kooboo.CMS.Content.Persistence.QcloudCOS.Models
{
    public class DeleteFolderRequest : RequestBase
    {
        public override string op { get; } = "delete";
    }

    public class DeleteFolder : EmptyResponse
    {
    }
}
