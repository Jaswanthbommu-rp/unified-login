using RP.Enterprise.Subsystem.ProductLauncher.Web.Landing.Models;

namespace RP.Enterprise.Subsystem.ProductLauncher.Web.Landing.Repository
{
    public interface IUserRepository
    {
        UserProduct GetUserProductDetailsByUserId(int userId);
    }
}