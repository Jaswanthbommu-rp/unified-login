using RP.Enterprise.Subsystem.ProductLauncher.WinService.UnityGBDisableUser.Model;
using System.Collections.Generic;
using System.Linq;

namespace RP.Enterprise.Subsystem.ProductLauncher.WinService.UnityGBDisableUser.Repository
{
    public class UserListRepository : BaseRepository, IUserListRepository
	{
		/// <summary>
		/// base Constructor
		/// </summary>
		public UserListRepository() : base(DbConnectionEnum.IdpConfigurationDb)
		{

		}

		public List<ProcessUserLogin> GetExpiredUsersToProcess() 
		{
			using (var repository = GetRepository())
			{
				var result = repository.GetMany<ProcessUserLogin>("Ident.ListExpiringUsers",null).ToList();
				return result;
			}
		}
	}
}
