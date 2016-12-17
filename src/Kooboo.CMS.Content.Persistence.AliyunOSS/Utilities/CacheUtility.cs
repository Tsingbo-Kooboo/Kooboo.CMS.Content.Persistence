using Kooboo.CMS.Caching;
using Kooboo.CMS.Content.Caching;
using Kooboo.CMS.Content.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kooboo.CMS.Content.Persistence.AliyunOSS.Utilities
{
    public static class CacheUtility
    {
        private static string CacheKey(string path, string repository)
        {
            return $"Kooboo.CMS.Content.Persistence.AliyunOSS.Services.{repository}.{path}";
        }

        public static T GetOrAdd<T>(string key, string repository, Func<T> data)
            where T : class
        {
            var cacheKey = CacheKey(key, repository);
            var rep = new Repository(repository);
            return rep.ObjectCache().GetCache<T>(cacheKey, data);
        }

        public static void RemoveCache(string key, string repository)
        {
            var cacheKey = CacheKey(key, repository);
            var rep = new Repository(repository);
            rep.ObjectCache().Remove(cacheKey);
        }
    }
}
