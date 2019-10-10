using System.Collections.Generic;
using System.Linq;
using RP.Enterprise.Subsystem.ProductLauncher.Web.Landing.Models.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Web.Landing.SAML;
using RP.Enterprise.Subsystem.ProductLauncher.Web.Landing.Models;

namespace RP.Enterprise.Subsystem.ProductLauncher.Web.Landing.Repository
{

    public class SamlRepository : BaseRepository, ISamlRepository
    {
        #region Ctor

        public SamlRepository() : base(DbConnectionEnum.IdpConfigurationDb)
        {
        }

        #endregion
        //GetAllPortfolioProductUser
        public IList<PortfolioProductUserDetails> GetAllPortfolioProductUser(int portfolioId, int userId, int productId)
        {
            using (var repo = GetRepository())
            {
                return repo.GetMany<PortfolioProductUserDetails>("Auth.GetAllPortfolioProductUser", new { portfolioId, userId, productId }).ToList();
            }
        }

        public IList<SAMLAttributes> GetProductSamlDetailsByPortfolioProductUserId(int portfolioProductUserId)
        {
            using (var repo = GetRepository())
            {
                return repo.GetMany<SAMLAttributes>("Auth.GetProductSamlDetailsByPortfolioProductUserId", new { portfolioProductUserId }).ToList();
            }
        }

    }
}