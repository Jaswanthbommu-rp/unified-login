using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using System.Collections.Generic;
using System.Linq;
using RP.Enterprise.Foundation.DataAccess.Component;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic
{
    /// <summary>
    /// Manage Role Type repository calls
    /// </summary>
    public class ManageRoleType : IManageRoleType
    {
        #region Private Variables

        readonly IRoleTypeRepository _roleTypeRepository;
        IList<RoleType> _roleTypeList = new List<RoleType>();

        #endregion

        #region Constructors

        /// <summary>
        /// Role Type Repository Constructor
        /// </summary>
        /// <param name="roleTypeRepository">Role Type Repository</param>
        public ManageRoleType(IRoleTypeRepository roleTypeRepository)
        {
            _roleTypeRepository = roleTypeRepository;
        }

        /// <summary>
        /// Create a basic instance of the ManageOrganization Controller class
        /// </summary>
        public ManageRoleType()
        {
            _roleTypeRepository = new RoleTypeRepository();
        }

        /// <summary>
        /// Unit test constructor
        /// </summary>
        /// <param name="repository"></param>
        public ManageRoleType(IRepository repository)
        {
            _roleTypeRepository = new RoleTypeRepository(repository);
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
            _roleTypeList = _roleTypeRepository.GetRoleType(roleTypeName, partyId).ToList();
            _roleTypeList = FilterRoleType(_roleTypeList, loginName, partyId, orgMasterId);
            return _roleTypeList;
        }

        /// <summary>
        /// Get RoleType Dependency
        /// </summary>
        /// <param name="roleTypeId">Role Type Name</param>
        /// <param name="partyId">The organization to filter the role type if used</param>
        /// <param name="orgMasterId">The books master id of the organization if used</param>
        /// <param name="loginName">Optional User loginName</param>
        /// <returns>List of RoleType object</returns>
        public IList<RoleType> GetRoleTypeDependency(long? roleTypeId, long? partyId, long? orgMasterId, string loginName = null)
        {
            _roleTypeList = _roleTypeRepository.GetRoleTypeDependency(roleTypeId, partyId);
            _roleTypeList = FilterRoleType(_roleTypeList, loginName, partyId, orgMasterId);

            return _roleTypeList;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Filter RoleType list
        /// </summary>
        /// <param name="roleTypeList">List of RoleType object</param>
        /// <param name="loginName">User loginname</param>
        /// <param name="partyId">org party id</param>
        /// <param name="orgMasterId">The books master id of the organization if used</param>
        /// <returns>List of RoleType object</returns>
        private IList<RoleType> FilterRoleType(IList<RoleType> roleTypeList, string loginName, long? partyId, long? orgMasterId)
        {
            if (roleTypeList != null && !string.IsNullOrWhiteSpace(loginName))
            {
                IManageUserLogin manageUserLogin = new ManageUserLogin();               
                IList<UserOrganization> userPersonaOrganizationList = manageUserLogin.GetUserPersonaOrganization(loginName);

                if (userPersonaOrganizationList != null && userPersonaOrganizationList.Count > 0)
                {
                        if (userPersonaOrganizationList.ToList().Any(i => !i.OrganizationPartyId.Equals(partyId) && !i.PartyRoleTypeId.Equals((int)UserRoleType.ExternalUser)))
                        {
                            roleTypeList = roleTypeList.ToList().Where(r => r.PartyRoleTypeId.Equals((int)UserRoleType.ExternalUser)).ToList();
                        }

                        if (userPersonaOrganizationList.ToList().Any(i => i.PartyRoleTypeId.Equals((int)UserRoleType.ExternalUser)))
                        {
                            roleTypeList = roleTypeList.ToList().Where(r => !r.PartyRoleTypeId.Equals((int)UserRoleType.UserNoEmail)).ToList();
                        }                   
                   
                }
            }

            return roleTypeList;
        }

        #endregion
    }
}