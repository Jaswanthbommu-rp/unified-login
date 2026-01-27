using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.Logic.EnterpriseRolesProperties
{
    /// <summary>
    /// Factory for creating IManageEnterpriseRolesPrimaryProperties instances
    /// </summary>
    public interface IManageEnterpriseRolesPrimaryPropertiesFactory
    {
        /// <summary>
        /// Creates a new instance of IManageEnterpriseRolesPrimaryProperties
        /// </summary>
        IManageEnterpriseRolesPrimaryProperties Create(DefaultUserClaim userClaim);
    }
}