using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Base;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using RP.Enterprise.Subsystem.ProductLauncher.Web.IdentityHelper.Repository;
using System.Collections.Generic;
using System.Linq;

namespace RP.Enterprise.Subsystem.ProductLauncher.Web.IdentityHelper.Logic
{
    public class ManageSameSite
    {
        #region Private Variables

        private IIdentityServerRepository _identityServerRepository;
        private IProductInternalSettingRepository _productInternalSettingRepository;

        #endregion

        public ManageSameSite()
        {
            _identityServerRepository = new IdentityServerRepository();
            _productInternalSettingRepository = new Component.Landing.Repository.ProductInternalSettingRepository();
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

        public IList<ProductInternalSetting> GetProductInternalSettings(int productId)
        {
            IList<ProductInternalSetting> productInternalSettingList = new List<ProductInternalSetting>();

            RPObjectCache rpcache = new RPObjectCache();
            var cacheKey = "productInternalSetting_" + productId;
            productInternalSettingList = rpcache.GetFromCache<IList<ProductInternalSetting>>(cacheKey, 60, () =>
            {
                // load from database
                return _productInternalSettingRepository.GetProductInternalSettings(productId);
            });
            return productInternalSettingList;
        }
    }
}
