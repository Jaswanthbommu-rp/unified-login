using RP.Enterprise.Subsystem.ProductLauncher.WinService.UnityGBDisableUser.Model;
using System.Collections.Generic;

namespace RP.Enterprise.Subsystem.ProductLauncher.WinService.UnityGBDisableUser.Repository
{
    public interface IUserListRepository
	{
		List<ProcessUserLogin> GetExpiredUsersToProcess();
	}
}
