using System.Threading.Tasks;
using RP.Enterprise.Subsystem.ProductLauncher.WinService.ProductAssigner.Helper;
using RP.Enterprise.Subsystem.ProductLauncher.WinService.ProductAssigner.Model;

namespace RP.Enterprise.Subsystem.ProductLauncher.WinService.ProductAssigner.Repository
{
    public class LandingApiCaller : ILandingApiCaller
    {
        public async Task<string> CreateProductUser(ProductUserProperitiesRoles productUserProperitiesRoles)
        {
            var result = await ApiCaller.PostApi<string, ProductUserProperitiesRoles>(productUserProperitiesRoles, $"api/productuser/user");
            return result;
        }
    }
}
