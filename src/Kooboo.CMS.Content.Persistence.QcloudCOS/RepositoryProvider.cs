#region License
// 
// Copyright (c) 2013, Kooboo team
// 
// Licensed under the BSD License
// See the file LICENSE.txt for details.
// 
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kooboo.CMS.Content.Models;
using Kooboo.CMS.Common.Runtime.Dependency;
using Kooboo.CMS.Content.Persistence.QcloudCOS.Services;
using Kooboo.CMS.Common.Runtime;

namespace Kooboo.CMS.Content.Persistence.QcloudCOS
{
    public class RepositoryProvider : IRepositoryProvider
    {
        public void Initialize(Repository repository)
        {
            inner.Initialize(repository);
            _folderService.Create("/", repository.Name);
        }

        public void Remove(Repository item)
        {
            inner.Remove(item);
            _folderService.Delete("/", item.Name);
        }

        public bool TestDbConnection()
        {
            return inner.TestDbConnection();
        }

        private readonly IRepositoryProvider inner;
        private readonly ICosFolderService _folderService;

        public RepositoryProvider(IRepositoryProvider innerProvider)
        {
            inner = innerProvider;
            _folderService = EngineContext.Current.Resolve<ICosFolderService>();
        }
        public IEnumerable<Repository> All()
        {
            return inner.All();
        }

        public void Offline(Repository repository)
        {
            inner.Offline(repository);
        }

        public void Online(Repository repository)
        {
            inner.Online(repository);
        }

        public bool IsOnline(Repository repository)
        {
            return inner.IsOnline(repository);
        }

        public Repository Create(string repositoryName, System.IO.Stream templateStream)
        {
            return inner.Create(repositoryName, templateStream);
        }

        public Repository Copy(Repository sourceRepository, string destRepositoryName)
        {
            return inner.Copy(sourceRepository, destRepositoryName);
        }

        public void Export(Repository repository, System.IO.Stream outputStream)
        {
            inner.Export(repository, outputStream);
        }

        public Repository Get(Repository dummy)
        {
            return inner.Get(dummy);
        }

        public void Add(Repository item)
        {
            inner.Add(item);
        }

        public void Update(Repository @new, Repository old)
        {
            inner.Update(@new, old);
        }
    }
}
