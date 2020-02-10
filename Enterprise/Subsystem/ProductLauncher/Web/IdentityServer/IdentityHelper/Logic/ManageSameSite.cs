using System.Collections.Generic;
using System.Linq;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Base;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using RP.Enterprise.Subsystem.ProductLauncher.Web.IdentityHelper.Repository;

namespace RP.Enterprise.Subsystem.ProductLauncher.Web.IdentityHelper.Logic
{
    public class ManageSameSite
    {
        #region Private Variables

        private IIdentityServerRepository _identityServerRepository;

        #endregion

        public ManageSameSite()
        {
            _identityServerRepository = new IdentityServerRepository();
        }

        public ManageSameSite(IIdentityServerRepository identityServerRepository)
        {
            _identityServerRepository = identityServerRepository;
        }

        public List<SameSiteExclusion> GetSameSiteExclusionList()
        {
            RPObjectCache rpcache = new RPObjectCache();
            var cacheKey = "getSameSiteExclusionList";

            List<SameSiteExclusion> scopes = rpcache.GetFromCache<List<SameSiteExclusion>>(cacheKey, 60, () => _identityServerRepository.GetSameSiteExclusionList().ToList());

            return scopes;
        }
    }
}
