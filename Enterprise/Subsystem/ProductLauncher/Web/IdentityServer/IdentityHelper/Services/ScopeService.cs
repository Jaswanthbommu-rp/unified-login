using IdentityServer3.Core.Services;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Base;
using RP.Enterprise.Subsystem.ProductLauncher.Web.IdentityHelper.Extensions;
using RP.Enterprise.Subsystem.ProductLauncher.Web.IdentityHelper.Repository;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RP.Enterprise.Subsystem.ProductLauncher.Web.IdentityHelper.Services
{
	class ScopeService : IScopeStore
    {
        private readonly IdentityServerRepository _identityServerRepository;

        public ScopeService(IdentityServerRepository identityServerRepository)
        {
	        _identityServerRepository = identityServerRepository;
        }

        public async Task<IEnumerable<IdentityServer3.Core.Models.Scope>> FindScopesAsync(IEnumerable<string> scopeNames)
        {

            var scopes = await GetAllScopesAsync();

            if (scopeNames != null && scopeNames.Any())
            {
                scopes = from s in scopes
                         where scopeNames.Contains(s.Name)
                         select s;
            }

            return scopes;
        }

        public async Task<IEnumerable<IdentityServer3.Core.Models.Scope>> GetScopesAsync(bool publicOnly = true)
        {
            var scopes = await GetAllScopesAsync();

            if (publicOnly)
            {
                scopes = from s in scopes
                         where s.ShowInDiscoveryDocument == true
                         select s;
            }

            return scopes;
        }

        private async Task<IEnumerable<IdentityServer3.Core.Models.Scope>> GetAllScopesAsync()
        {
            var scopesToReturn = new List<IdentityServer3.Core.Models.Scope>();
			RPObjectCache rpcache = new RPObjectCache();
			var cacheKey = "scopes";

			IEnumerable<Scope> scopes = await rpcache.GetFromCacheAsync<IEnumerable<Scope>>(cacheKey, 300, () => _identityServerRepository.GetAllScopes());

			cacheKey = "allScopeClaims";
			IEnumerable<ScopeClaim> allScopeClaims = await rpcache.GetFromCacheAsync<IEnumerable<ScopeClaim>>(cacheKey, 300, () => _identityServerRepository.GetAllScopeClaims());

			cacheKey = "allScopeSecrets";
			IEnumerable<ScopeSecret> allScopeSecrets = await rpcache.GetFromCacheAsync<IEnumerable<ScopeSecret>>(cacheKey, 300, () => _identityServerRepository.GetAllScopeSecrets());

			scopes.ToList().ForEach(s =>
            {
                var scopeSecrets = allScopeSecrets.Where(c => c.ScopeId == s.ScopeId);
                var scopeClaims = allScopeClaims.Where(c => c.ScopeId == s.ScopeId);
                var identityModel = s.ToIdentityServerModel();
                identityModel.Claims = scopeClaims.ToIdentityServerModels().ToList();
                identityModel.ScopeSecrets = scopeSecrets.ToIdentityServerModels().ToList();
                scopesToReturn.Add(identityModel);
            });
            return scopesToReturn;
        }

    }
}
