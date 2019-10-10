using RP.Enterprise.Subsystem.ProductLauncher.WinService.UserNotification.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RP.Enterprise.Subsystem.ProductLauncher.WinService.UserNotification.Repository
{
    public interface IUserApiCaller
    {
        Task<string> ProcessUserLogins(List<ProcessUserLogin> userLogin);
        Task<string> DisableExpiredUsers(List<ProcessUserLogin> userLogin);
    }
}
