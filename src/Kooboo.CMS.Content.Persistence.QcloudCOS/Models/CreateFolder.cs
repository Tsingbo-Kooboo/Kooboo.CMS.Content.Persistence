using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kooboo.CMS.Content.Persistence.QcloudCOS.Models
{
    public class CreateFolderRequest : RequestBase
    {
        public override string op { get; } = "create";
    }

    public class CreateFolder : ResponseBase<CreateFolderData>
    {
        internal CreateFolder()
        {
            data = new CreateFolderData();
        }
    }

    public class CreateFolderData
    {
        public string ctime { get; set; }

        public string resource_path { get; set; }
    }

}
