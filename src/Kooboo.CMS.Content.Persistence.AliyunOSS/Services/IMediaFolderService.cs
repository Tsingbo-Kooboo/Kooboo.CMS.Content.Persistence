using Kooboo.CMS.Content.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kooboo.CMS.Content.Persistence.AliyunOSS.Services
{
    public interface IMediaFolderService
    {
        MediaFolder Create(string path, string repository);

        IEnumerable<MediaFolder> List(string parentFolder, string repository);

        MediaFolder Get(string path, string repository);

        MediaFolder Delete(string path, string repository);
    }
}
