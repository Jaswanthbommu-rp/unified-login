using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Audit.Dtos;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Mappers
{
    public static class UserDetailsMapper
    {
        #region From UserDetails to UserAuditDto
        /// <summary>
        /// UserDetails becomes a UserAuditDto object
        /// </summary>
        /// <typeparam name="TR">UserAuditDto object</typeparam>
        /// <param name="origin">UserDetails object</param>
        /// <returns>A UserAuditDto object</returns>
        public static TR UserDetailsToUserAuditDto<TR>(this UserDetails origin)
        where TR : UserAuditDto, new()
        {
            return origin.UserDetailsToUserAuditDto(new TR());
        }

        /// <summary>
        /// UserDetails becomes a UserAuditDto object
        /// </summary>
        /// <typeparam name="TR">UserAuditDto object</typeparam>
        /// <param name="origin">UserDetails object</param>
        /// <param name="result">UserAuditDto object</param>
        /// <returns>A UserAuditDto object</returns>
        public static TR UserDetailsToUserAuditDto<TR>(this UserDetails origin, TR result)
        where TR : UserAuditDto, new()
        {
            if (origin == null)
            {
                result = new TR(); ;
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
                result.NotificationEmail = origin.Email;
                result.UserName = origin.LoginName;
                result.UserExpire = origin.ThruDate;
                result.UserEffective = origin.FromDate;
                result.IsActive = origin.IsActive;
            }

            return result;
        }
        #endregion
    }
}
