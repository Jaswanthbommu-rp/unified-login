using System.Collections.Generic;
using UnifiedLogin.SharedObjects.IdentityConfig;

namespace UnifiedLogin.BusinessLogic.Logic.Interfaces
{
	/// <summary>
	/// Interface for Manage RelationshipType repository calls
	/// </summary>
	public interface IManageRelationshipType
	{
		/// <summary>
		/// Get RelationshipType
		/// </summary>
		/// <param name="relationshipTypeName">Relationship Type Name</param>
		/// <returns>List of RelationshipType object</returns>
		IList<RelationshipType> GetRelationshipType(string relationshipTypeName);

        /// <summary>
        /// Get UserRelationShip Types
        /// </summary>
        /// <returns></returns>
        IList<UserRelationShipType> GetUserRelationShipTypes();

    }
}