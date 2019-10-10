using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Extensions
{
    /// <summary>
    /// 
    /// </summary>
    public static class ProfileExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="organizations"></param>
        /// <param name="userTypes"></param>
        /// <returns></returns>
        public static bool HasAnyUserRole(this IList<Organization> organizations, IList<UserRoleType> userTypes)
        {
            return organizations.Any((organization) =>
            {
                return userTypes.Any(userType => organization.partyRelationship?.RoleTypeIdFrom == (int)userType);
            });
        }
    }
}
