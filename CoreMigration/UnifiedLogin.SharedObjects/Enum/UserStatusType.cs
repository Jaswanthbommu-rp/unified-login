using System.ComponentModel;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.SharedObjects.Enum
{
    /// <summary>
    /// User DB StatusType
    /// </summary>
    public enum UserDbStatusType
    {
		/// <summary>
		/// Active
		/// </summary>
        Active = 1,

		/// <summary>
		/// Pending
		/// </summary>
        Pending = 2,

		/// <summary>
		/// Locked
		/// </summary>
        Locked = 3,
        
		/// <summary>
		/// Tainted
		/// </summary>
		Tainted = 4 ,// after MVP

        /// <summary>
        /// Forced reset password
        /// </summary>
        ForceResetPassword=12
    }

    /// <summary>
    /// User Ui StatusType
    /// </summary>
    public enum UserUiStatusType
    {
		/// <summary>
		/// UnDefined
		/// </summary>
		[Description("UnDefined")]
		UnDefined = 0,
		/// <summary>
		/// Active
		/// </summary>
		[Description("Active")]
		Active = 1,

		/// <summary>
		/// Pending
		/// </summary>
		[Description("Pending")]
		Pending = 2,

		/// <summary>
		/// Locked
		/// </summary>
		[Description("Locked")]
		Locked = 3,

		/// <summary>
		/// Tainted
		/// </summary>
		[Description("Tainted")]
		Tainted = 4, // after MVP

		/// <summary>
		/// Disabled
		/// </summary>
		[Description("Disabled")]
		Disabled = 24,

		/// <summary>
		/// Unlocked
		/// </summary>
		[Description("Unlocked")]
		Unlocked = 6,

		/// <summary>
		/// Expired
		/// </summary>
		[Description("Expired")]
		Expired = 23,

		/// <summary>
		/// Error
		/// </summary>
		[Description("Error")]
		Error = 7,

		/// <summary>
		/// Account Creation Successful
		/// </summary>
		[Description("Account Creation Successful")]
		AccountCreationSuccessful = 8,

		/// <summary>
		/// Account Hidden/Deleted
		/// </summary>
		[Description("Hidden")]
		AccountHidden = 10,
		
		/// <summary>
		/// Set Temporary Password
		/// </summary>
		[Description("Force Reset Password")]
		ForceResetPassword = 12,

		/// <summary>
		/// Deactivated
		/// </summary>
		[Description("Deactivated")]
		Deactivated = 19
			   
	}
}