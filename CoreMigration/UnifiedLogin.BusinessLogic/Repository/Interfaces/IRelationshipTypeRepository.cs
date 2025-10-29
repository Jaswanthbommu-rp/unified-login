using System.Collections.Generic;
using UnifiedLogin.SharedObjects.IdentityConfig;

namespace UnifiedLogin.BusinessLogic.Repository.Interfaces
{
	/// <summary>
	/// Interface for Relationship Type Repository
	/// </summary>
	public interface IRelationshipTypeRepository
	{
		/// <summary>
		/// Get RelationshipType
		/// </summary>
		/// <param name="relationshipTypeName">Relationship Type Name</param>
		/// <returns>List RelationshipType object</returns>
		IList<RelationshipType> GetRelationshipType(string relationshipTypeName);
        IList<UserRelationShipType> GetUserRelationShipTypes(long partyId);
    }
}