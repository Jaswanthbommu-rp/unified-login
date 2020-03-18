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
        public string FirstName { get; set; }

        /// <summary>
        /// User Middle Initial
        /// </summary>
        public string MiddleInitial { get; set; }

        /// <summary>
        /// User Last Name
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// User Type
        /// </summary>
        public string UserType { get; set; }

        /// <summary>
        /// User Name
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Notification Email
        /// </summary>
        public string NotificationEmail { get; set; }

        /// <summary>
        /// User Expire Date
        /// </summary>
        public DateTime? UserExpire { get; set; }

        /// <summary>
        /// User Effective Date
        /// </summary>
        public DateTime? UserEffective { get; set; }

        /// <summary>
        /// User Enable Access
        /// </summary>
        ///public bool? Access { get; set; }
    }
}
