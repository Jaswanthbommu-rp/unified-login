using System;
using System.Threading.Tasks;
using UnifiedLogin.Core.Interfaces.Providers;
namespace RP.Enterprise.Subsystem.ProductLauncher.Web.Landing.Providers
{
    public interface IDistributedCacheProvider : ICacheProvider
    {
        Task<T> ReadThroughCacheAsync<T>(string key, int expirationSeconds, Func<Task<T>> handler);
    }
}
