using System.Collections.Generic;
using UnifiedLogin.SharedObjects.IdentityConfig;

namespace UnifiedLogin.BusinessLogic.Logic.Interfaces
{
    /// <summary>
    /// Interface for Manage RoleType repository calls
    /// </summary>
    public interface IManageRoleType
    {
        /// <summary>
        /// Get RoleType
        /// </summary>
        /// <param name="roleTypeName">Role Type Name</param>
        /// <param name="partyId">The organization to filter the role type if used</param>
        /// <param name="orgMasterId">The books master id of the organization if used</param>
        /// <param name="loginName">Optional User loginName</param>
        /// <returns>List of RoleType object</returns>
        IList<RoleType> GetRoleType(string roleTypeName, long? partyId, long? orgMasterId, string loginName = null);

        /// <summary>
        /// Get RoleType Dependency
        /// </summary>
        /// <param name="roleTypeId">Role Type Name</param>
        /// <param name="partyId">The organization to filter the role type if used</param>
        /// <param name="orgMasterId">The books master id of the organization if used</param>
        /// <param name="loginName">Optional User LoginName</param>
        /// <returns>List of RoleType object</returns>
        IList<RoleType> GetRoleTypeDependency(long? roleTypeId, long? partyId, long? orgMasterId, string loginName = null);
    }
}