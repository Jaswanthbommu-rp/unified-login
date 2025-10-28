using UnifiedLogin.SharedObjects.Audit.Dtos;
using UnifiedLogin.SharedObjects.IdentityConfig;

namespace UnifiedLogin.SharedObjects.Mappers
{
    public static class ProfileDetailsMapper
    {
        #region From IProfileDetail to UserAuditDto
        /// <summary>
        /// IProfileDetail becomes a UserAuditDto object
        /// </summary>
        /// <typeparam name="TR">UserAuditDto object</typeparam>
        /// <param name="origin">IProfileDetail object</param>
        /// <returns>A UserAuditDto object</returns>
        public static TR IProfileDetailToUserAuditDto<TR>(this IProfileDetail origin)
        where TR : UserAuditDto, new()
        {
            return origin.IProfileDetailToUserAuditDto(new TR());
        }

        /// <summary>
        /// IProfileDetail becomes a UserAuditDto object
        /// </summary>
        /// <typeparam name="TR">UserAuditDto object</typeparam>
        /// <param name="origin">IProfileDetail object</param>
        /// <param name="result">UserAuditDto object</param>
        /// <returns>A UserAuditDto object</returns>
        public static TR IProfileDetailToUserAuditDto<TR>(this IProfileDetail origin, TR result)
        where TR : UserAuditDto, new()
        {
            if (origin == null)
            {
                result = new TR();
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
                result.UserExpire = origin.userLogin.ThruDate;
                result.UserEffective = origin.userLogin.FromDate;
                result.IsActive = origin.userLogin.IsActive;
                result.EmployeeId = origin.EmployeeId;
            }

            return result;
        }
        #endregion
    }
}
