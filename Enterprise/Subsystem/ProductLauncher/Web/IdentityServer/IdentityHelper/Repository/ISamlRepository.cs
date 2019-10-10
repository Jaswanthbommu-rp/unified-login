using System.Collections.Generic;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Saml;

namespace RP.Enterprise.Subsystem.ProductLauncher.Web.IdentityHelper.Repository
{
    public interface ISamlRepository
    {
        /// <summary>
        /// Get the SAML attribute names, types, and values by PersonaId and ProductId
        /// </summary>
        /// <param name="PersonaId">User personaId</param>
        /// <param name="ProductId">ProductId</param>
        /// <returns>List SamlAttributes object</returns>
        IList<SamlAttributes> GetProductSamlDetails(long PersonaId, int ProductId);
    }
}