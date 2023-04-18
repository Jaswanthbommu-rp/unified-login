using System.Collections.Generic;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces
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