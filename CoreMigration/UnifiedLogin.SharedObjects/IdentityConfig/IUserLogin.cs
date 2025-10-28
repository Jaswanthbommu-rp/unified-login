using UnifiedLogin.SharedObjects.Enum;
using System;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.SharedObjects.IdentityConfig
{
    /// <summary>
    /// Interface for User Object
    /// </summary>
    public interface IUserLogin
    {
        /// <summary>
        /// UserId
        /// </summary>
        long UserId { get; set; }

        /// <summary>
        /// PartyId
        /// </summary>
        long PartyId { get; set; }

        /// <summary>
        /// RealPageId
        /// </summary>
        Guid RealPageId { get; set; }

        /// <summary>
        /// LoginName
        /// </summary>
        string LoginName { get; set; }

        /// <summary>
		/// Not an input. Used to determine if the loginname is an email. Used only for the UI.
		/// </summary>
		string LoginNameType { get; set; }

        /// <summary>
        /// IsActive
        /// </summary>
        bool? IsActive { get; set; }

        /// <summary>
        /// When the account can be used
        /// </summary>
        DateTime? FromDate { get; set; }

        /// <summary>
        /// When the account can no longer be used
        /// </summary>
        DateTime? ThruDate { get; set; }

        ///// <summary>
        ///// User Status As Of Date time
        ///// </summary>
        //DateTime StatusSetDate { get; set; }

        /// <summary>
        /// PasswordHash
        /// </summary>
        string PasswordHash { get; set; }

        /// <summary>
        /// Password Salt
        /// </summary>
        string PasswordSalt { get; set; }

        /// <summary>
        /// Is the user account locked
        /// </summary>
        bool? IsLocked { get; set; }

        ///// <summary>
        ///// Force users to change password.
        ///// </summary>
        //bool? IsTainted { get; set; }

        /// <summary>
        /// Indicates if user account is pending
        /// </summary>
        bool? IsPending { get; set; }

        /// <summary>
        /// Is invitation to create user Expired
        /// </summary>
        bool? IsExpired { get; set; }

        /// <summary>
        /// Is the user a super user
        /// </summary>
        bool IsSuperUser { get; set; }

        /// <summary>
        /// Sortable user statuses (Active|Disabled|Pending|Expired).
        /// </summary>
        UserUiStatusType Status { get; set; }

        /// <summary>
        /// LastLogin date
        /// </summary>
        DateTime? LastLogin { get; set; }

        /// <summary>
        /// UserRoleType
        /// </summary>
        UserRoleType? UserRoleType { get; set; }

        /// <summary>
        /// Clear text password used to create new or update user 
        /// </summary>
        string Password { get; set; }

        /// <summary>
        /// Get or Set Password modified date
        /// </summary>
        DateTime? PasswordModifiedDate { get; set; }

        int OffsetMinutes { get; set; }

		/// <summary>
		/// Use third party identity service provider.  Default to true so that the toggle switch is on on the Add new User.
		/// </summary>
		bool Is3rdPartyIDP { get; set; }

		/// <summary>
		/// Get or Set status thru date
		/// </summary>
		DateTime? StatusThruDate { get; set; }

		/// <summary>
		/// Get or Set status Id
		/// </summary>
		int StatusId { get; set; }

		/// <summary>
		/// Is LoginName Null or WhiteSpace
		/// </summary>
		bool IsLoginNameNullOrWhiteSpace { get; }

		/// <summary>
		/// Force Temporary Password Set
		/// </summary>
		bool? IsForceReSetPassword { get; set; }

        /// <summary>
        /// doNotForceChangePassword
        /// </summary>
        bool doNotForceChangePassword { get; set; }
    }
}