using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Audit.Dto;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Mappers
{
    public static class ProfileDetailMapper
    {
        #region From ProfileDetail to UserAuditDto
        /// <summary>
        /// ProfileDetail becomes a UserAuditDto object
        /// </summary>
        /// <typeparam name="TR">UserAuditDto object</typeparam>
        /// <param name="origin">ProfileDetail object</param>
        /// <returns>A UserAuditDto object</returns>
        public static TR UserDetailToUserAuditDto<TR>(this ProfileDetail origin)
        where TR : UserAuditDto, new()
        {
            return origin.UserDetailToUserAuditDto(new TR());
        }

        /// <summary>
        /// ProfileDetail becomes a UserAuditDto object
        /// </summary>
        /// <typeparam name="TR">UserAuditDto object</typeparam>
        /// <param name="origin">ProfileDetail object</param>
        /// <param name="result">UserAuditDto object</param>
        /// <returns>A UserAuditDto object</returns>
        public static TR UserDetailToUserAuditDto<TR>(this ProfileDetail origin, TR result)
        where TR : UserAuditDto, new()
        {
            if (origin == null)
            {
                result = default;
            }
            else
            {
                if (result == null)
                {
                    result = new TR();
                }

                result.FirstName = origin.FirstName;
                result.LastName = origin.LastName;
                result.MiddleInitial = origin.MiddleName;
                result.NotificationEmail = origin.NotificationEmail;
                result.UserName = origin.userLogin.LoginName;
            }

            return result;
        }
        #endregion
    }
}
