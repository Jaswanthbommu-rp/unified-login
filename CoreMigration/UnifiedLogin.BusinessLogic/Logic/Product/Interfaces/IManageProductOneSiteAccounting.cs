using System.Collections.Generic;
using UnifiedLogin.SharedObjects.Audit.Common;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product;
using UnifiedLogin.SharedObjects.Product.Migration;

namespace UnifiedLogin.BusinessLogic.Logic.Product.Interfaces
{
    /// <summary>
    /// Used to add/edit users in the Accounting product
    /// </summary>
    public interface IManageProductOneSiteAccounting
    {
        string DeleteAccountingUser(long editorPersonaId, long deletedPersona);
        ListResponse GetUserProperties(long editorPersonaId, long userPersonaId, RequestParameter datafilter);
        ListResponse GetUserPropertyGroups(long editorPersonaId, long userPersonaId, RequestParameter datafilter);
        ListResponse GetUserPropertiesNew(long editorPersonaId, long userPersonaId, RequestParameter datafilter);
        ListResponse GetUserCompanies(long editorPersonaId, long userPersonaId, RequestParameter datafilter);
        ListResponse GetPropertyGroupEntities(long editorPersonaId, long userPersonaId, string locationGrpId, RequestParameter datafilter);
        ListResponse GetUserRoles(long editorPersonaId, long userPersonaId, RequestParameter datafilter);
        string ChangeAccountingServiceUserType(long createUserPersonaId, long assignUserPersonaId, List<string> rpList, List<string> PropertyList, List<string> CompanyList, bool isAccountingAdmin, bool isSiteSpendManagementUser, bool isUnRestrictedAccessToProp, BatchProcessType batchProcessType);
        string ManageAccountingUser(long editorPersonaId, long userPersonaId, List<string> RoleList, List<string> PropertyList, List<string> CompanyList, bool isAccountingAdmin, bool isSiteSpendManagementUser, bool isUnRestrictedAccessToProp, out List<AdditionalParameters> additionalParameters, BatchProcessType batchProcessType = BatchProcessType.CreateUpdateProductUser);
        string UpdatePropertiesToUser(long editorPersonaId, long userPersonaId, List<string> propertiesToAssign, bool isAccountingAdmin, out List<AdditionalParameters> additionalParametersProperties, BatchProcessType batchProcessType = BatchProcessType.CreateUpdateProductUser);
        string UpdateRolesToUser(long editorPersonaId, long userPersonaId, List<string> rolesToAssign, bool isAccountingAdmin, out List<AdditionalParameters> additionalParametersRoles, BatchProcessType batchProcessType = BatchProcessType.CreateUpdateProductUser);
        string UnassignUser(long createUserPersonaId, long assignUserPersonaId);
        ListResponse GetApplications(long editorPersonaId);
        ListResponse GetRolesCount(long editorPersonaId, RequestParameter datafilter);
        ListResponse GetAllRoles(long editorPersonaId, RequestParameter datafilter);
        ListResponse GetRights(long editorPersonaId);
        ListResponse GetRolesForRight(long editorPersonaId, RequestParameter datafilter, int rightId, bool assignedOnly,ProductRightAcct right);
        ListResponse GetRightsForRole(long editorPersonaId, RequestParameter datafilter, string roleName, int roleId );
        ListResponse UpdateRightsForRole(long editorPersonaId, int roleId, string roleName, List<ProductRightAcct> rightsToAdd, List<ProductRightAcct> rightsToRemove);
        ListResponse UpdateRolesForRight(long editorPersonaId, int rightId, List<ProductRoleAcct> rolesToAdd, List<ProductRoleAcct> rolesToRemove, ProductRightAcct right);
        ListResponse CreateRole(long editorPersonaId, string roleName);
        ListResponse DeleteRole(long editorPersonaId, long roleId, string roleName);
        ListResponse CloneRole(long editorPersonaId, string roleName, string inheritedRoleName);
        
        /// <summary>
        /// Used to enable/disable an Accounting user
        /// </summary>
        /// <param name="editorPersonaId"></param>
        /// <param name="userPersonaId"></param>
        /// <param name="isLinked"></param>
        /// <returns></returns>
        bool ChangeAccountingUserClaimStatus(long editorPersonaId, long userPersonaId, bool isLinked);

        /// <summary>
        /// List all users
        /// </summary>
        /// <param name="editorPersonaId"></param>
        /// <param name="datafilter"></param>
        /// <returns></returns>
        ListResponse GetMigrationUsers(long editorPersonaId, RequestParameter datafilter);

        /// <summary>
        /// Update the users migration status
        /// </summary>
        /// <param name="editorPersonaId"></param>
        /// <param name="migrateUsers"></param>
        /// <returns></returns>
        MigrateResponse UpdateUsersMigrationStatus(long editorPersonaId, IList<MigrateUser> migrateUsers);

        /// <summary>
        /// Disable OneSite Accounting product user.
        /// </summary>
        /// <param name="editorPersonaId">The editor persona identifier.</param>
        /// <param name="username">The username.</param>
        /// <param name="isActive">if set to <c>true</c> [is active].</param>
        /// <returns></returns>
        bool ChangeUserStatus(long editorPersonaId, string username, bool isActive = false);
    }
}