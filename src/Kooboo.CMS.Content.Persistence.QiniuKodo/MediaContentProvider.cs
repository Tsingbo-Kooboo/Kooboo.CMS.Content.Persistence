using Kooboo.CMS.Content.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kooboo.CMS.Content.Query;
using System.IO;
using Kooboo.CMS.Common.Runtime.Dependency;

namespace Kooboo.CMS.Content.Persistence.QiniuKodo
{
    [Dependency(typeof(IMediaContentProvider), Order = 2)]
    [Dependency(typeof(IContentProvider<MediaContent>), Order = 2)]
    public class MediaContentProvider : IMediaContentProvider
    {
        public void Add(MediaContent content)
        {
            throw new NotImplementedException();
        }

        public void Add(MediaContent content, bool overrided)
        {
        } 

        public void Delete(MediaContent content)
        {
            throw new NotImplementedException();
        }

        public object Execute(IContentQuery<MediaContent> query)
        {
            throw new NotImplementedException();
        }

        public byte[] GetContentStream(MediaContent content)
        {
            throw new NotImplementedException();
        }

        public void InitializeMediaContents(Repository repository)
        {
            throw new NotImplementedException();
        }

        public void Move(MediaFolder sourceFolder, string oldFileName, MediaFolder targetFolder, string newFileName)
        {
            throw new NotImplementedException();
        }

        public void SaveContentStream(MediaContent content, Stream stream)
        {
            throw new NotImplementedException();
        }

        public void Update(MediaContent @new, MediaContent old)
        {
            throw new NotImplementedException();
        }
    }
}
