using UnifiedLogin.SharedObjects.Attribute;
using UnifiedLogin.SharedObjects.Constants;
using System;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.SharedObjects.Audit.Dtos
{
    /// <summary>
    /// DTO class to the User Audit
    /// </summary>
    public class UserAuditDto
    {
        /// <summary>
        /// User Firt Name
        /// </summary>
        [AuditLog("First Name", LogActivityTypeConstants.UPDATE_USER)]
        public string FirstName { get; set; }

        /// <summary>
        /// User Middle Initial
        /// </summary>
        [AuditLog("Middle Initial", LogActivityTypeConstants.UPDATE_USER)]
        public string MiddleInitial { get; set; }

        /// <summary>
        /// User Last Name
        /// </summary>
        [AuditLog("Last Name", LogActivityTypeConstants.UPDATE_USER)]
        public string LastName { get; set; }

        /// <summary>
        /// User Type
        /// </summary>
        [AuditLog("User Type", LogActivityTypeConstants.PRODUCT_ACCESS)]
        public string UserType { get; set; }

        /// <summary>
        /// User Name
        /// </summary>
        [AuditLog("Email (Username)", LogActivityTypeConstants.UPDATE_USER)]
        public string UserName { get; set; }

        /// <summary>
        /// Notification Email
        /// </summary>
        [AuditLog("Notification Email", LogActivityTypeConstants.UPDATE_USER)]
        public string NotificationEmail { get; set; }

        /// <summary>
        /// User Expires
        /// </summary>
        [AuditLog("User Expires", LogActivityTypeConstants.UPDATE_USER, "{0:MM/dd/yyyy}")]
        public DateTime? UserExpire { get; set; }

        /// <summary>
        /// User Effective
        /// </summary>
        [AuditLog("User Effective", LogActivityTypeConstants.UPDATE_USER, "{0:MM/dd/yyyy}")]
        public DateTime? UserEffective { get; set; }

        /// <summary>
        /// User Enable Access
        /// </summary>
        [AuditLog("Active Access", LogActivityTypeConstants.UPDATE_USER, "", "{'True':'{2} {3} Activated user {0} {1}.','False':'{2} {3} Deactivated user {0} {1}.'}")]
        public bool? IsActive { get; set; }

        /// <summary>
        /// User Employee Id
        /// </summary>
        [AuditLog("Employee Id", LogActivityTypeConstants.UPDATE_USER)]
        public string EmployeeId { get; set; }
    }
}
