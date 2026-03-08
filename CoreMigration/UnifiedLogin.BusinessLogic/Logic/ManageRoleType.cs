using System;
using System.Collections.Generic;
using System.Linq;
using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.BusinessLogic.Repository;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.DataAccess;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.Logic
{
    /// <summary>
    /// Manage Role Type repository calls
    /// </summary>
    public class ManageRoleType : IManageRoleType
    {
        #region Constants
        /// <summary>
        /// External User role type ID
        /// </summary>
        private const int ExternalUserRoleTypeId = 405;

        /// <summary>
        /// User No Email role type ID
        /// </summary>
        private const int UserNoEmailRoleTypeId = 404;
        #endregion

        #region Private Variables

        private readonly IRoleTypeRepository _roleTypeRepository;
        private readonly IManageUserLogin _manageUserLogin;
        private IList<RoleType> _roleTypeList = new List<RoleType>();

        #endregion

        #region Constructors

        /// <summary>
        /// Primary constructor with full dependency injection (recommended)
        /// </summary>
        /// <param name="roleTypeRepository">Role Type Repository</param>
        /// <param name="manageUserLogin">User Login management service</param>
        public ManageRoleType(IRoleTypeRepository roleTypeRepository, IManageUserLogin manageUserLogin)
        {
            _roleTypeRepository = roleTypeRepository ?? throw new ArgumentNullException(nameof(roleTypeRepository));
            _manageUserLogin = manageUserLogin ?? throw new ArgumentNullException(nameof(manageUserLogin));
        }

        /// <summary>
        /// Role Type Repository Constructor (legacy support)
        /// </summary>
        /// <param name="roleTypeRepository">Role Type Repository</param>
        public ManageRoleType(IRoleTypeRepository roleTypeRepository)
        {
            _roleTypeRepository = roleTypeRepository ?? throw new ArgumentNullException(nameof(roleTypeRepository));
            _manageUserLogin = new ManageUserLogin();
        }

        /// <summary>
        /// Create a basic instance of the ManageRoleType class (legacy support)
        /// </summary>
        public ManageRoleType()
        {
            _roleTypeRepository = new RoleTypeRepository();
            _manageUserLogin = new ManageUserLogin();
        }

        /// <summary>
        /// Unit test constructor (legacy support)
        /// </summary>
        /// <param name="repository">Repository</param>
        public ManageRoleType(IRepository repository)
        {
            if (repository == null) throw new ArgumentNullException(nameof(repository));
            
            _roleTypeRepository = new RoleTypeRepository(repository);
            _manageUserLogin = new ManageUserLogin();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Get RoleType
        /// </summary>
        /// <param name="roleTypeName">Role Type Name</param>
        /// <param name="partyId">The organization to filter the role type if used</param>
        /// <param name="orgMasterId">The books master id of the organization if used</param>
        /// <param name="loginName">Optional User loginName</param>
        /// <returns>List of RoleType object</returns>
        public IList<RoleType> GetRoleType(string roleTypeName, long? partyId, long? orgMasterId, string loginName = null)
        {
            _roleTypeList = GetRoleTypesFromRepository(roleTypeName, partyId);
            _roleTypeList = FilterRoleType(_roleTypeList, loginName, partyId, orgMasterId);
            return _roleTypeList;
        }

        /// <summary>
        /// Get RoleType Dependency
        /// </summary>
        /// <param name="roleTypeId">Role Type ID</param>
        /// <param name="partyId">The organization to filter the role type if used</param>
        /// <param name="orgMasterId">The books master id of the organization if used</param>
        /// <param name="loginName">Optional User loginName</param>
        /// <returns>List of RoleType object</returns>
        public IList<RoleType> GetRoleTypeDependency(long? roleTypeId, long? partyId, long? orgMasterId, string loginName = null)
        {
            _roleTypeList = GetRoleTypeDependenciesFromRepository(roleTypeId, partyId);
            _roleTypeList = FilterRoleType(_roleTypeList, loginName, partyId, orgMasterId);

            return _roleTypeList;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Get role types from repository
        /// </summary>
        /// <param name="roleTypeName">Role type name</param>
        /// <param name="partyId">Party ID</param>
        /// <returns>List of RoleType</returns>
        private IList<RoleType> GetRoleTypesFromRepository(string roleTypeName, long? partyId)
        {
            return _roleTypeRepository.GetRoleType(roleTypeName, partyId).ToList();
        }

        /// <summary>
        /// Get role type dependencies from repository
        /// </summary>
        /// <param name="roleTypeId">Role type ID</param>
        /// <param name="partyId">Party ID</param>
        /// <returns>List of RoleType</returns>
        private IList<RoleType> GetRoleTypeDependenciesFromRepository(long? roleTypeId, long? partyId)
        {
            return _roleTypeRepository.GetRoleTypeDependency(roleTypeId, partyId);
        }

        /// <summary>
        /// Filter RoleType list based on user organization and role type
        /// </summary>
        /// <param name="roleTypeList">List of RoleType object</param>
        /// <param name="loginName">User loginname</param>
        /// <param name="partyId">org party id</param>
        /// <param name="orgMasterId">The books master id of the organization if used</param>
        /// <returns>Filtered list of RoleType object</returns>
        private IList<RoleType> FilterRoleType(IList<RoleType> roleTypeList, string loginName, long? partyId, long? orgMasterId)
        {
            if (!ShouldApplyFiltering(roleTypeList, loginName))
            {
                return roleTypeList ?? new List<RoleType>();
            }

            var userPersonaOrganizationList = GetUserPersonaOrganizations(loginName);

            if (!HasUserPersonaOrganizations(userPersonaOrganizationList))
            {
                return roleTypeList;
            }

            return ApplyRoleTypeFilters(roleTypeList, userPersonaOrganizationList, partyId);
        }

        /// <summary>
        /// Determine if filtering should be applied
        /// </summary>
        /// <param name="roleTypeList">Role type list</param>
        /// <param name="loginName">Login name</param>
        /// <returns>True if filtering should be applied</returns>
        private bool ShouldApplyFiltering(IList<RoleType> roleTypeList, string loginName)
        {
            return roleTypeList != null && !string.IsNullOrWhiteSpace(loginName);
        }

        /// <summary>
        /// Get user persona organizations
        /// </summary>
        /// <param name="loginName">Login name</param>
        /// <returns>List of UserOrganization</returns>
        private IList<UserOrganization> GetUserPersonaOrganizations(string loginName)
        {
            return _manageUserLogin.GetUserPersonaOrganization(loginName);
        }

        /// <summary>
        /// Check if user has persona organizations
        /// </summary>
        /// <param name="userPersonaOrganizationList">User persona organization list</param>
        /// <returns>True if list is not null and has items</returns>
        private bool HasUserPersonaOrganizations(IList<UserOrganization> userPersonaOrganizationList)
        {
            return userPersonaOrganizationList != null && userPersonaOrganizationList.Count > 0;
        }

        /// <summary>
        /// Apply role type filters based on user organizations
        /// </summary>
        /// <param name="roleTypeList">Role type list</param>
        /// <param name="userPersonaOrganizationList">User persona organizations</param>
        /// <param name="partyId">Party ID</param>
        /// <returns>Filtered role type list</returns>
        private IList<RoleType> ApplyRoleTypeFilters(
            IList<RoleType> roleTypeList,
            IList<UserOrganization> userPersonaOrganizationList,
            long? partyId)
        {
            // Filter Rule 1: If user has persona in different organization (not current) 
            // AND that persona is NOT an external user, show only external user roles
            if (HasNonExternalUserInDifferentOrganization(userPersonaOrganizationList, partyId))
            {
                roleTypeList = FilterToExternalUserRolesOnly(roleTypeList);
            }

            // Filter Rule 2: If user has any external user persona, 
            // hide UserNoEmail role type
            if (HasExternalUserPersona(userPersonaOrganizationList))
            {
                roleTypeList = FilterOutUserNoEmailRole(roleTypeList);
            }

            return roleTypeList;
        }

        /// <summary>
        /// Check if user has a non-external user persona in a different organization
        /// </summary>
        /// <param name="userPersonaOrganizationList">User persona organizations</param>
        /// <param name="partyId">Current party ID</param>
        /// <returns>True if condition met</returns>
        private bool HasNonExternalUserInDifferentOrganization(
            IList<UserOrganization> userPersonaOrganizationList,
            long? partyId)
        {
            return userPersonaOrganizationList.ToList().Any(i =>
                !i.OrganizationPartyId.Equals(partyId) &&
                !i.PartyRoleTypeId.Equals(ExternalUserRoleTypeId));
        }

        /// <summary>
        /// Check if user has any external user persona
        /// </summary>
        /// <param name="userPersonaOrganizationList">User persona organizations</param>
        /// <returns>True if user has external user persona</returns>
        private bool HasExternalUserPersona(IList<UserOrganization> userPersonaOrganizationList)
        {
            return userPersonaOrganizationList.ToList().Any(i =>
                i.PartyRoleTypeId.Equals(ExternalUserRoleTypeId));
        }

        /// <summary>
        /// Filter role type list to only external user roles
        /// </summary>
        /// <param name="roleTypeList">Role type list</param>
        /// <returns>Filtered list with only external user roles</returns>
        private IList<RoleType> FilterToExternalUserRolesOnly(IList<RoleType> roleTypeList)
        {
            return roleTypeList.ToList()
                .Where(r => r.PartyRoleTypeId.Equals(ExternalUserRoleTypeId))
                .ToList();
        }

        /// <summary>
        /// Filter out UserNoEmail role type
        /// </summary>
        /// <param name="roleTypeList">Role type list</param>
        /// <returns>Filtered list without UserNoEmail role</returns>
        private IList<RoleType> FilterOutUserNoEmailRole(IList<RoleType> roleTypeList)
        {
            return roleTypeList.ToList()
                .Where(r => !r.PartyRoleTypeId.Equals(UserNoEmailRoleTypeId))
                .ToList();
        }

        #endregion
    }
}