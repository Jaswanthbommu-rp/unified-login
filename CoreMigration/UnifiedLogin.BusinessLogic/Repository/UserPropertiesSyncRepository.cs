using UnifiedLogin.DataAccess;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Landing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnifiedLogin.BusinessLogic.Repository
{
    public class UserPropertiesSyncRepository : BaseRepository, IUserPropertiesSyncRepository
    {
        #region Ctor
        /// <summary>
        /// Persona base Constructor
        /// </summary>
        public UserPropertiesSyncRepository() : base(DbConnectionEnum.IdpConfigurationDb)
        {           
        }

        /// <summary>
        /// Shared repository constructor
        /// </summary>
        /// <param name="repository"></param>
        public UserPropertiesSyncRepository(IRepository repository) : base(repository)
        {
          
        }


        #endregion
        /// <summary>
        /// Get User Sync Object
        /// </summary>
        /// <returns>Persona Environment Type Object</returns>
        public UserSyncData GetUserSyncData(long syncJobTaskId)
        {
            using (var repository = GetRepository())
            {
                dynamic param = new
                {
                    taskId = syncJobTaskId
                };
               return repository.GetOne<UserSyncData>(StoredProcNameConstants.SP_GetUserSyncJobTaskDetails, param);

               // return userSyncData;
            }
        }
    }
}
