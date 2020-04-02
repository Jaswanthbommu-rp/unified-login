using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Attribute;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Constants;
using System;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Audit.Dtos
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

        //[AuditLog("User Expires", LogActivityTypeConstants.UPDATE_USER, "{0:MM/dd/yyyy}")]
        //public DateTime? UserExpire { get; set; }

        //[AuditLog("User Effective", LogActivityTypeConstants.UPDATE_USER, "{0:MM/dd/yyyy}")]
        //public DateTime? UserEffective { get; set; }

        /// <summary>
        /// User Enable Access
        /// </summary>
        [AuditLog("Active Access", LogActivityTypeConstants.UPDATE_USER)]
        public bool? IsActive { get; set; }
    }
}
