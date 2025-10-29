using System.Collections.Generic;
using UnifiedLogin.SharedObjects.Audit.Common;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product.Migration;
using UnifiedLogin.SharedObjects.Product.Ops;

namespace UnifiedLogin.BusinessLogic.Logic.Product.Interfaces
{
    /// <summary>
    /// 
    /// </summary>
    public interface IManageProductOps
    {
        ListResponse GetCompanyAssets(long editorPersonaId, long userPersonaId, bool includeDisabled, RequestParameter datafilter);
        ListResponse GetRoles(long editorPersonaId, long userPersonaId, string assetCode, RequestParameter datafilter);
        ListResponse GetRolesCount(long editorPersonaId, string assetCode);
        ListResponse GetRights(long editorPersonaId);
        string ManageOpsUser(long editorPersonaId, long userPersonaId, List<int> RoleList, List<int> PropertyList, out List<AdditionalParameters> additionalParameters);
        string EnableUser(long editorPersonaId, long userPersonaId, bool isActive, bool deleteUser);
        string UnassignUser(long createUserPersonaId, long assignUserPersonaId);

        /// <summary>
        /// List all users
        /// </summary>
        /// <param name="editorPersonaId"></param>
        /// <param name="datafilter"></param>
        /// <returns></returns>
        ListResponse GetUsers(long editorPersonaId, RequestParameter datafilter);
        ListResponse GetRolesForRight(long editorPersonaId, int rightId);
        ListResponse CreateRole(long editorPersonaId, OpsInput rightInput, long roleId);
        ListResponse GetRightsByRole(long editorPersonaId, long roleId);

        /// <summary>
        /// List all users
        /// </summary>
        /// <param name="editorPersonaId"></param>
        /// <param name="datafilter"></param>
        ListResponse GetMigrationUsers(long editorPersonaId, RequestParameter datafilter);

        /// <summary>
        /// Update the users migration status
        /// </summary>
        /// <param name="editorPersonaId"></param>
        /// <param name="migrateUsers"></param>
        /// <returns></returns>
        MigrateResponse UpdateUsersMigrationStatus(long editorPersonaId, IList<MigrateUser> migrateUsers);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="editorPersonaId"></param>
        /// <param name="userName"></param>
        /// <param name="productUserId"></param>
        /// <param name="isActive"></param>
        /// <returns></returns>
        bool ChangeUserStatus(long editorPersonaId, string userName, string productUserId, bool isActive = false);

        /// <summary>
        /// Get Ops AssetGroups
        /// </summary>
		/// <param name="editorPersonaId">Logged-in user PersonaId</param>
		/// <param name="userPersonaId">new user PersonaId</param>
        /// <param name="assetGroupId">Optional assetGroupId</param>
        /// <returns>ListResponse</returns>
        ListResponse GetOpsAssetGroups(long editorPersonaId, long userPersonaId, int assetGroupId = 0);

        /// <summary>
        /// Used to get the list of assets
        /// </summary>
        /// <param name="editorPersonaId"></param>
        /// <param name="userPersonaId"></param>
        /// <param name="status">Used to remove the disabled assets from the result</param>
        /// <returns></returns>
        ListResponse GetOpsAssets(long editorPersonaId, long userPersonaId, string status);

        /// <summary>
        /// Create an Ops AssetGroup
        /// </summary>
        /// <param name="editorPersonaId">Logged-in user PersonaId</param>
        /// <param name="userPersonaId">new user PersonaId</param>
        /// <param name="assetGroup">AssetGroup object (Name, Description, and List of Properties {Ids, OR Codes}</param>
        /// <returns>ListResponse object</returns>
        ListResponse CreateOpsAssetGroup(long editorPersonaId, long userPersonaId, AssetGroupCreate assetGroup);

        /// <summary>
        /// Edit/Update an Ops AssetGroup
        /// </summary>
        /// <param name="editorPersonaId">Logged-in user PersonaId</param>
        /// <param name="userPersonaId">new user PersonaId</param>
        /// <param name="assetGroupId">assetGroupId being updated</param>
        /// <param name="assetGroup">AssetGroup object (Name, Description, and List of Properties {Ids, OR Codes}</param>
        /// <returns>ListResponse object</returns>
        ListResponse UpdateOpsAssetGroup(long editorPersonaId, long userPersonaId, int assetGroupId, AssetGroupCreate assetGroup);

        /// <summary>
        /// Update Asset Group Name/Status
        /// </summary>
        /// <param name="editorPersonaId">Logged-in user PersonaId</param>
        /// <param name="userPersonaId">new user PersonaId</param>
        /// <param name="assetGroupId">assetGroupId being updated</param>
        /// <param name="assetGroup">AssetGroup object (Name, Description, and List of Properties {Ids, OR Codes}</param>
        /// <returns>ListResponse object</returns>
        ListResponse PatchOpsAssetGroup(long editorPersonaId, long userPersonaId, int assetGroupId, AssetGroupPatch assetGroup);
    }
}