using RP.Enterprise.Subsystem.ProductLauncher.WinService.UserNotification.Constants;
using RP.Enterprise.Subsystem.ProductLauncher.WinService.UserNotification.Model;
using System.Collections.Generic;
using System.Linq;

namespace RP.Enterprise.Subsystem.ProductLauncher.WinService.UserNotification.Repository
{
    public class UserListRepository : BaseRepository, IUserListRepository
    {
        #region Constructor

        /// <summary>
        /// base Constructor
        /// </summary>
        public UserListRepository() : base(DbConnectionEnum.IdpConfigurationDb)
        {

        }

        #endregion

        #region Public Methods
        public List<ProcessUserLogin> GetFutureUsersToProcess(int batchSize)
        {
            using (var repository = GetRepository())
            {
                var result = repository.GetMany<ProcessUserLogin>(StoredProcNameConstants.SP_FeatureUsersList, new { batchSize }).ToList();
                return result;
            }
        }

		public List<ProcessUserLogin> GetPendingUsersToProcess()
		{
			using (var repository = GetRepository())
			{
				var result = repository.GetMany<ProcessUserLogin>(StoredProcNameConstants.SP_PendingUsersList, new {  }).ToList();
				return result;
			}
		}

        public List<ProcessUserLogin> GetExpiredUsersToProcess()
        {
            using (var repository = GetRepository())
            {
                var result = repository.GetMany<ProcessUserLogin>(StoredProcNameConstants.SP_ListExpiringUsers, null).ToList();
                return result;
            }
        }

        #endregion
    }
}
