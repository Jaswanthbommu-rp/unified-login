using System;
using UnifiedLogin.DataAccess;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Enum;

namespace UnifiedLogin.BusinessLogic.Repository
{
	/// <summary>
	/// User Activity token Repository
	/// </summary>
	public class UserTokenRepository :BaseRepository, IUserTokenRepository
    {
        #region Ctor

        /// <summary>
        /// User base Constructor
        /// </summary>
        public UserTokenRepository() : base(DbConnectionEnum.IdpConfigurationDb)
        {
        }

        public UserTokenRepository(IRepository repository) : base(repository)
        {
        }

        #endregion

        /// <summary>
        /// Get User Activity token By RealPageId
        /// </summary>
        /// <param name="realPageId">RealPageId</param>
        /// <param name="activityTypeId">ActivityId</param>
        /// <param name="partyId">Org partyId</param>
        /// <returns>UserProduct object</returns>
        public string GetUserActivityToken( Guid realPageId, int activityTypeId, long partyId)
        {
            string userActivityToken = "";
            if (realPageId != Guid.Empty && activityTypeId > 0)
            {
                dynamic paramNewUserToken = new
                {
					partyId,
					realPageId,
                    activityTypeId					
				};

                using (var repository = GetRepository())
                {
                    userActivityToken = repository.GetOne<string>(StoredProcNameConstants.SP_CreateActivityToken, paramNewUserToken);
                }
            }
            return userActivityToken;
        }
    }
}