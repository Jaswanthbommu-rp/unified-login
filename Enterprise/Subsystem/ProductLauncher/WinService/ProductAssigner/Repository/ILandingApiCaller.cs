using System.Threading.Tasks;
using RP.Enterprise.Subsystem.ProductLauncher.WinService.ProductAssigner.Model;

namespace RP.Enterprise.Subsystem.ProductLauncher.WinService.ProductAssigner.Repository
{
    public interface ILandingApiCaller
    {
        Task<string> CreateProductUser(ProductUserProperitiesRoles productUserProperitiesRoles);
    }
}