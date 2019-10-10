using IdentityServer3.Core.Models;
using IdentityServer3.Core.Services.Default;
using IdentityServer3.Core.ViewModels;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Helper;
using System.IO;
using System.Threading.Tasks;

namespace RP.Enterprise.Subsystem.ProductLauncher.Web.Identity
{
    public class CustomLoginPageUserService : DefaultViewService
    {
        public CustomLoginPageUserService(DefaultViewServiceOptions config, IViewLoader viewLoader)
            : base(config, viewLoader)
        {
        }

        public override Task<Stream> Login(LoginViewModel model, SignInMessage message)
        {
            // remove the external providers so we aren't exposing it to customers
            model.ExternalProviders = null;
            model.Custom = message.AcrValues; 
            return base.Render(model, "login");
        }
    }

}