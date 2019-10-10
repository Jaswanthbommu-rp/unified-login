using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Constants;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Saml;
using RP.Enterprise.Subsystem.ProductLauncher.Web.IdentityHelper.Enum;
using System.Collections.Generic;
using System.Linq;

namespace RP.Enterprise.Subsystem.ProductLauncher.Web.IdentityHelper.Repository
{
    public class SamlRepository : BaseRepository, ISamlRepository
    {
        #region Ctor
        /// <summary>
        /// SAML base Constructor
        /// </summary>
        public SamlRepository() : base(DbConnectionEnum.IdpConfigurationDb)
        {
        }
        #endregion

        /// <summary>
        /// Get the SAML attribute names, types, & values by PersonaId and ProductId
        /// </summary>
        /// <param name="PersonaId">User personaId</param>
        /// <param name="ProductId">ProductId</param>
        /// <returns>list SamlAttributes object</returns>
        public IList<SamlAttributes> GetProductSamlDetails(long PersonaId, int ProductId)
        {
            using (var repo = GetRepository())
            {
                return repo.GetMany<SamlAttributes>(StoredProcNameConstants.SP_GetProductSamlDetails, new { PersonaId, ProductId }).ToList();
            }
        }

        /// <summary>
        /// Get a product Saml Settings
        /// </summary>
        /// <param name="productId"></param>
        /// <returns>ProductSamlSettings object</returns>
        public ProductSamlSettings GetProductSamlSettingsByProductId(int productId)
        {
            using (var repo = GetRepository())
            {
                return repo.GetOne<ProductSamlSettings>(StoredProcNameConstants.SP_GetProductSamlSettings, new { productId });
            }
        }
    }
}