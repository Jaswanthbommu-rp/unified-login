using System.Collections.Generic;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces
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