using RP.Enterprise.Subsystem.ProductLauncher.WinService.UserNotification.Helper;
using RP.Enterprise.Subsystem.ProductLauncher.WinService.UserNotification.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RP.Enterprise.Subsystem.ProductLauncher.WinService.UserNotification.Repository
{
    public class UserApiCaller : IUserApiCaller
    {
        public async Task<string> ProcessUserLogins(List<ProcessUserLogin> userLogin)
        {
            var result = await ApiCaller.PostApi<string, List<ProcessUserLogin>>(userLogin, $"api/userlogins/processfutureuserlogins");
            return result;
        }

        public async Task<string> DisableExpiredUsers(List<ProcessUserLogin> userLogin)
        {
            var result = await ApiCaller.PostApi<string, List<ProcessUserLogin>>(userLogin, $"api/disableexpiredusers");
            return result;
        }

    }
}
