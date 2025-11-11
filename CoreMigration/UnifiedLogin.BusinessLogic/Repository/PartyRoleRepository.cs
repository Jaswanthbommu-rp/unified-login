using System;
using System.Collections.Generic;
using System.Linq;
using UnifiedLogin.DataAccess;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Landing.Enum;
using UnifiedLogin.SharedObjects.Constants;

namespace UnifiedLogin.BusinessLogic.Repository
{
	/// <summary>
	/// PartyRole Repository
	/// </summary>
	public class PartyRoleRepository : BaseRepository, IPartyRoleRepository
	{
		#region Constructor
		/// <summary>
		/// Person base Constructor
		/// </summary>
		public PartyRoleRepository() : base(DbConnectionEnum.IdpConfigurationDb)
        {
		}

        /// <summary>
        /// Unit test constructor
        /// </summary>
        /// <param name="repository"></param>
        public PartyRoleRepository(IRepository repository) : base(repository)
        {
        }

		#endregion

		#region public PartyRole methods
		/// <summary>
		/// Create a party Role (User Job Title) by Enterprise UserID
		/// </summary>
		/// <param name="realPageId">User unique identifier</param>
		/// <param name="partyRole">PartyRole object of the parameter values</param>
		/// <returns>Repository response object</returns>
		public RepositoryResponse CreatePartyRoleEnterpriseUserID(Guid realPageId, IPartyRole partyRole)
		{
			dynamic param = new
			{
				realPageId,
				partyRole.RoleTypeId
			};

			using (var repository = GetRepository())
			{
				var result = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_CreatePartyRoleByRealPageId, param);
				return result;
			}
		}

		/// <summary>
		/// Get the person party role (Job Title) by unique identifier
		/// </summary>
		/// <param name="realPageId">User unique identifier</param>
		/// <returns>PartyRole object</returns>
		public PartyRole GetPartyRoleByEnterpriseUserID(Guid realPageId)
		{
			using (var repository = GetRepository())
			{
				return repository.GetOne<PartyRole>(StoredProcNameConstants.SP_GetPartyRoleByRealPageId, new { realPageId });
			}
		}

        /// <summary>
		/// Get the person party role (Job Title) by unique identifier
		/// </summary>
		/// <param name="partyId">Person party id</param>
		/// <returns>PartyRole object</returns>
		public IList<PartyRole> GetPartyRoles(long partyId)
        {
            using (var repository = GetRepository())
            {
                return repository.GetMany<PartyRole>(StoredProcNameConstants.SP_GetPartyRole, new { partyId }).ToList();
            }
        }

        /// <summary>
        /// Update a party Role by Enterprise UserID
        /// </summary>
        /// <param name="partyRole">PartyRole object of the parameter values</param>
        /// <returns>Repository response object</returns>
        public RepositoryResponse UpdatePartyRoleEnterpriseUserID(IPartyRole partyRole)
		{
			dynamic param = new
			{
				PartyRoleId = partyRole.PartyRoleId,
				RoleTypeID = partyRole.RoleTypeId
			};

			using (var repository = GetRepository())
			{
				var result = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_UpdatePartyRoleByRealPageId, param);
				return result;
			}
		}
		#endregion
	}
}