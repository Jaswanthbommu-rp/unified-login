using RP.Enterprise.Subsystem.ProductLauncher.WinService.UserNotification.Model;
using System.Collections.Generic;

namespace RP.Enterprise.Subsystem.ProductLauncher.WinService.UserNotification.Repository
{
    public interface IUserListRepository
    {
        List<ProcessUserLogin> GetFutureUsersToProcess(int batchSize);
		List<ProcessUserLogin> GetPendingUsersToProcess();
        List<ProcessUserLogin> GetExpiredUsersToProcess();
    }
}
