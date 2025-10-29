using UnifiedLogin.DataAccess;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.IdentityConfig;
using System.Collections.Generic;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.Repository
{
	/// <summary>
	/// UserLoginPersona Repository
	/// </summary>
	public class UserLoginPersonaRepository : BaseRepository, IUserLoginPersonaRepository
	{
		#region Constructor
		/// <summary>
		/// UserRepository Constructor
		/// </summary>
		public UserLoginPersonaRepository() : base(DbConnectionEnum.IdpConfigurationDb)
		{
		}

        public UserLoginPersonaRepository(IRepository repository) : base(repository)
        {
        }
        #endregion

        #region Publuc UserLoginPersona Methods
        /// <summary>
        /// Create UserLoginPersona
        /// </summary>
        /// <param name="userLoginPersona">userLoginPersona object</param>
        /// <returns>Repository response object</returns>
        public RepositoryResponse CreateUserLoginPersona(UserLoginPersona userLoginPersona)
		{
			dynamic param = new
			{
				UserLoginId = userLoginPersona.UserLoginId,
				StatusTypeId = userLoginPersona.StatusTypeId,
				PrimaryOrganization = userLoginPersona.PrimaryOrganization,
				FromDate = userLoginPersona.FromDate,
				ThruDate = userLoginPersona.ThruDate
			};

			using (var repository = GetRepository())
			{
				RepositoryResponse repositoryResponse = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_CreateUserLoginPersona, param);
				return repositoryResponse;
			}
		}

		/// <summary>
		/// List one or more UserLogin Persona records
		/// </summary>
		/// <param name="userLoginPersonaId">Optional userLoginPersonaId</param>
		/// <param name="userLoginId">Optional UserLoginId</param>
		/// <param name="organizationPartyId">Optional Organization PartyId</param>
		/// <returns>List of UserLoginPersona</returns>
		public IList<UserLoginPersona> ListUserLoginPersona(long? userLoginPersonaId, long? userLoginId, long? organizationPartyId)
		{
			dynamic param = new
			{
				UserLoginPersonaId = userLoginPersonaId,
				UserLoginId = userLoginId,
				OrganizationPartyId = organizationPartyId
			};

			using (var repository = GetRepository())
			{
				return repository.GetMany<UserLoginPersona>(StoredProcNameConstants.SP_GetUserLoginPersona, param);
			}
		}
		#endregion
	}
}
