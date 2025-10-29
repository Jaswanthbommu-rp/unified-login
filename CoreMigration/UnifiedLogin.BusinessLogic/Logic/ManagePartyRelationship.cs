using System;
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
	/// Manage Party Relationship repository calls
	/// </summary>
	public class ManagePartyRelationship : IManagePartyRelationship
	{
		#region Private Variables
		IPartyRelationshipRepository _partyRelationshipRepository;
		#endregion

		#region Constructors
		/// <summary>
		/// Party Relationship Repository Constructor
		/// </summary>
		/// <param name="partyRelationshipRepository">PartyRelationship Repository</param>
		public ManagePartyRelationship(IPartyRelationshipRepository partyRelationshipRepository)
		{
			_partyRelationshipRepository = partyRelationshipRepository;
		}

		/// <summary>
		/// Create a basic instance of the ManageOrganization Controller class
		/// </summary>
		public ManagePartyRelationship()
		{
			_partyRelationshipRepository = new PartyRelationshipRepository();
		}

        /// <summary>
        /// Create a basic instance of the ManageOrganization Controller class
        /// </summary>
        public ManagePartyRelationship(IRepository repository)
        {
            _partyRelationshipRepository = new PartyRelationshipRepository(repository);
        }

		#endregion

		/// <summary>
		/// Get a Party (Person) Relationship within a Linked Organization
		/// </summary>
		/// <param name="realPageIdFrom">Person unique identifier</param>
		/// <param name="realPageIdTo">Organization unique identifier</param>
		/// <param name="roleTypeNameFrom">Person Role Type name in the Relationship (Optional)</param>
		/// <param name="roleTypeNameTo">Organization Role Type name in the Relationship (Optional)</param>
		/// <param name="relationshipTypeName">Parties Relationhip type name (Optional)</param>
		/// <returns>PartyRelationship object</returns>
		public PartyRelationship GetPartyRelationship(Guid realPageIdFrom, Guid realPageIdTo, string roleTypeNameFrom, string roleTypeNameTo, string relationshipTypeName)
		{
			if (realPageIdFrom == Guid.Empty)
			{
				throw new Exception("Invalid parameter realPageIdFrom.");
			}

			if (realPageIdTo == Guid.Empty)
			{
				throw new Exception("Invalid parameter realPageIdTo.");
			}

			return _partyRelationshipRepository.GetPartyRelationship(realPageIdFrom, realPageIdTo, roleTypeNameFrom, roleTypeNameTo, relationshipTypeName);
		}

		/// <summary>
		/// Link an Organization to an Organization
		/// </summary>
		/// <param name="RealPageIdFrom">From Organization unique identifier</param>
		/// <param name="partyRelationship">To Party Unique Identifier and From/To Relationship RoleType</param>
		/// <returns>Repository response object</returns>
		public RepositoryResponse LinkOrganizationToOrganization(Guid RealPageIdFrom, PartyRelationship partyRelationship)
		{
			if (RealPageIdFrom == Guid.Empty)
			{
				throw new Exception("Invalid parameter RealPageIdFrom.");
			}

			if (partyRelationship == null)
			{
				throw new ArgumentNullException(nameof(partyRelationship), "Null PartyRelationship.");
			}

			if (partyRelationship.RealPageIdTo == Guid.Empty)
			{
				throw new Exception("Invalid parameter partyRelationship.RealPageIdTo.");
			}

			if (partyRelationship.RoleTypeIdFrom <= 0)
			{
				throw new Exception("Invalid parameter partyRelationship.RoleTypeIdFrom.");
			}

			if (partyRelationship.RoleTypeIdTo <= 0)
			{
				throw new Exception("Invalid parameter partyRelationship.RoleTypeIdTo.");
			}

			return _partyRelationshipRepository.LinkOrganizationToOrganization(RealPageIdFrom, partyRelationship);
		}

		/// <summary>
		/// Link a Person to an Organization
		/// </summary>
		/// <param name="RealPageIdFrom">Person unique identifier</param>
		/// <param name="partyRelationship">To Party Unique Identifier and From/To Relationship RoleType</param>
		/// <returns>Repository response object</returns>
		public RepositoryResponse LinkPersonToOrganization(Guid RealPageIdFrom, PartyRelationship partyRelationship)
		{
			if (RealPageIdFrom == Guid.Empty)
			{
				throw new Exception("Invalid parameter RealPageIdFrom.");
			}

			if (partyRelationship == null)
			{
				throw new ArgumentNullException(nameof(partyRelationship), "Null PartyRelationship.");
			}

			if (partyRelationship.RealPageIdTo == Guid.Empty)
			{
				throw new Exception("Invalid parameter partyRelationship.RealPageIdTo.");
			}

			if (partyRelationship.RoleTypeIdFrom <= 0)
			{
				throw new Exception("Invalid parameter partyRelationship.RoleTypeIdFrom.");
			}

			if (partyRelationship.RoleTypeIdTo <= 0)
			{
				throw new Exception("Invalid parameter partyRelationship.RoleTypeIdTo.");
			}

			return _partyRelationshipRepository.LinkPersonToOrganization(RealPageIdFrom, partyRelationship);
		}
	}
}