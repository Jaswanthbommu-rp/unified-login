using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Base;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using System.Collections.Generic;
using System.Linq;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;


namespace RP.Enterprise.Subsystem.ProductLauncher.Web.Landing.Helper
{
	public static class ProductHelper
    {
        public static GbProductMap GetBooksMasterProductDetail(DefaultUserClaim userClaim, int gbProductId)
        {
            var gbProductMap = GetGbProductMap(userClaim).FirstOrDefault(x => x.ProductId == gbProductId);
            return gbProductMap;
        }

        private static IList<GbProductMap> GetGbProductMap(DefaultUserClaim userClaim)
        {
			// Get products
			RPObjectCache rpcache = new RPObjectCache();
			var cacheKey = "GB-BB-ProductMap";
			var products = rpcache.GetFromCache<IList<GbProductMap>>(cacheKey, 3600, () =>
			{
				IManageProduct manageProduct = new ManageProduct(userClaim);
				return manageProduct.ListProducts();
			});

            return products;
        }
    }
}