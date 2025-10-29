using System;
using System.Collections.Generic;
using UnifiedLogin.DataAccess;
using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.BusinessLogic.Repository;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.Logic
{
	/// <summary>
	/// Manage PartyRole repository calls
	/// </summary>
	public class ManagePartyRole : IManagePartyRole
	{
		#region Private Variables
		IPartyRoleRepository _partyRoleRepository;
		#endregion

		#region Constructors
		/// <summary>
        /// Unit test constructor
		/// </summary>
		/// <param name="partyRoleRepository">PartyRole Repository</param>
		public ManagePartyRole(IPartyRoleRepository partyRoleRepository)
		{
			_partyRoleRepository = partyRoleRepository;
		}

        /// <summary>
        /// Unit test constructor
        /// </summary>
        /// <param name="repository">Repository</param>
        public ManagePartyRole(IRepository repository)
        {
            _partyRoleRepository = new PartyRoleRepository(repository);
        }

		/// <summary>
		/// Create a basic instance of the ManagePartyRole Controller class
		/// </summary>
		/// 
		public ManagePartyRole()
		{
			_partyRoleRepository = new PartyRoleRepository();
		}
		#endregion

		#region Public ManagePartyRole methods
		/// <summary>
		/// Create a PartyRole
		/// </summary>
		/// <param name="realPageId">User unique identifier</param>
		/// <param name="partyRole">PartyRole data object</param>
		/// <returns>RepositoryResponse object</returns>
		public RepositoryResponse CreatePartyRoleEnterpriseUserID(Guid realPageId, IPartyRole partyRole)
		{
			if (realPageId == Guid.Empty)
			{
				throw new Exception("Invalid parameter realPageId.");
			}

			if (partyRole == null)
			{
				throw new ArgumentNullException(nameof(partyRole), "Null PartyRole.");
			}

			return _partyRoleRepository.CreatePartyRoleEnterpriseUserID(realPageId, partyRole);
		}

		/// <summary>
		/// Get PartyRole
		/// </summary>
		/// <param name="realPageId">User unique identifier</param>
		/// <returns>PartyRole object</returns>
		public PartyRole GetPartyRole(Guid realPageId)
		{
			if (realPageId == Guid.Empty)
			{
				throw new Exception("Invalid parameter realPageId.");
			}

			return _partyRoleRepository.GetPartyRoleByEnterpriseUserID(realPageId);
		}

        /// <summary>
		/// Get PartyRoles
		/// </summary>
		/// <param name="partyId"></param>
		/// <returns>PartyRole object</returns>
		public IList<PartyRole> GetPartyRoles(long? partyId)
        {
            if (partyId == null)
            {
                throw new Exception("Invalid parameter partyId.");
            }

            return _partyRoleRepository.GetPartyRoles(partyId.Value);
        }

        /// <summary>
        /// Update an existing PartyRole
        /// </summary>
        /// <param name="partyRole">PartyRole object of the parameter values</param>
        /// <returns>RepositoryResponse object</returns>
        public RepositoryResponse UpdatePartyRole(IPartyRole partyRole)
		{
			if (partyRole == null)
			{
				throw new ArgumentNullException(nameof(partyRole), "Null PartyRole.");
			}

			return _partyRoleRepository.UpdatePartyRoleEnterpriseUserID(partyRole);
		}
		#endregion
	}
}