using System.Collections.Generic;
using UnifiedLogin.SharedObjects.IdentityConfig;

namespace UnifiedLogin.BusinessLogic.Repository.Interfaces
{
	/// <summary>
	/// Interface for Relationship Type Repository
	/// </summary>
	public interface IRelationshipTypeRepositoryAsync
	{
        /// <summary>
        /// Get RelationshipType
        /// </summary>
        /// <param name="relationshipTypeName">Relationship Type Name</param>
        /// <returns>List RelationshipType object</returns>
        Task<IList<RelationshipType>> GetRelationshipTypeAsync(string relationshipTypeName, CancellationToken cancellationToken = default);
        Task<IList<UserRelationShipType>> GetUserRelationShipTypesAsync(long partyId, CancellationToken cancellationToken = default);
    }
}