using System;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.Repository.Interfaces
{
	/// <summary>
	/// Interface for Party Relationship Repository
	/// </summary>
	public interface IPartyRelationshipRepository
	{
		/// <summary>
		/// Get a Party (Person) Relationship within a Linked Organization
		/// </summary>
		/// <param name="realPageIdFrom">Person unique identifier</param>
		/// <param name="realPageIdTo">Organization unique identifier</param>
		/// <param name="roleTypeNameFrom">Person Role Type name in the Relationship (Optional)</param>
		/// <param name="roleTypeNameTo">Organization Role Type name in the Relationship (Optional)</param>
		/// <param name="relationshipTypeName">Parties Relationhip type name (Optional)</param>
		/// <returns>PartyRelationship object</returns>
		PartyRelationship GetPartyRelationship(Guid realPageIdFrom, Guid realPageIdTo, string roleTypeNameFrom, string roleTypeNameTo, string relationshipTypeName);

		/// <summary>
		/// Link an Organization to an Organization
		/// </summary>
		/// <param name="RealPageIdFrom">From Organization unique identifier</param>
		/// <param name="partyRelationship">To Party Unique Identifier and From/To Relationship RoleType</param>
		/// <returns>Repository response object</returns>
		RepositoryResponse LinkOrganizationToOrganization(Guid RealPageIdFrom, PartyRelationship partyRelationship);

		/// <summary>
		/// Link a Person to an Organization
		/// </summary>
		/// <param name="RealPageIdFrom">Person unique identifier</param>
		/// <param name="partyRelationship">To Party Unique Identifier and From/To Relationship RoleType</param>
		/// <returns>Repository response object</returns>
		RepositoryResponse LinkPersonToOrganization(Guid RealPageIdFrom, PartyRelationship partyRelationship);
	}
}