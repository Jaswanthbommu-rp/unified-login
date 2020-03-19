using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Attribute;
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
        [AuditLog("First Name")]
        public string FirstName { get; set; }

        /// <summary>
        /// User Middle Initial
        /// </summary>
        [AuditLog("Middle Initial")]
        public string MiddleInitial { get; set; }

        /// <summary>
        /// User Last Name
        /// </summary>
        [AuditLog("Last Name")]
        public string LastName { get; set; }

        /// <summary>
        /// User Type
        /// </summary>
        [AuditLog("User Type")]
        public string UserType { get; set; }

        /// <summary>
        /// User Name
        /// </summary>
        [AuditLog("Email (Username)")]
        public string UserName { get; set; }

        /// <summary>
        /// Notification Email
        /// </summary>
        [AuditLog("Notification Email")]
        public string NotificationEmail { get; set; }

        /// <summary>
        /// User Expire Date
        /// </summary>
        [AuditLog("User Expires")]
        public DateTime? UserExpire { get; set; }

        /// <summary>
        /// User Effective Date
        /// </summary>
        [AuditLog("User Effective")]
        public DateTime? UserEffective { get; set; }

        /// <summary>
        /// User Enable Access
        /// </summary>
        ///public bool? Access { get; set; }
    }
}
