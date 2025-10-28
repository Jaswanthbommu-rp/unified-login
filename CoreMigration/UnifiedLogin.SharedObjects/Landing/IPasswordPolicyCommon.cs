namespace UnifiedLogin.SharedObjects.Landing
{
	/// <summary>
	/// Interface for Common Password Policy properties - Unified Security Settings
	/// </summary>
	public interface IPasswordPolicyCommon
	{
		/// <summary>
		/// Specify the minimum number of characters allowed in a user password
		/// </summary>
		byte MinimumLength { get; set; }

		/// <summary>
		/// Number of previous passwords a user is not allowed to reuse
		/// </summary>
		byte NumberOfPasswordsToRemember { get; set; }

		/// <summary>
		/// Number of days a password is valid for
		/// </summary>
		short PasswordExpirationPeriodInDays { get; set; }

		/// <summary>
		/// Unique Password Policy ID
		/// </summary>
		int PasswordPolicyId { get; set; }

		/// <summary>
		/// References the User Unique ID
		/// </summary>
		long UserId { get; set; }
	}
}