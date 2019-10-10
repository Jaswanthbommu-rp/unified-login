using System.Collections.Generic;
using RP.Enterprise.Subsystem.ProductLauncher.Web.Landing.SAML;
using RP.Enterprise.Subsystem.ProductLauncher.Web.Landing.Models;

namespace RP.Enterprise.Subsystem.ProductLauncher.Web.Landing.Repository
{
    public interface ISamlRepository
    {
        IList<SAMLAttributes> GetProductSamlDetailsByPortfolioProductUserId(int portfolioProductUserId);

        IList<PortfolioProductUserDetails> GetAllPortfolioProductUser(int portfolioId, int userId, int productId);
    }
}