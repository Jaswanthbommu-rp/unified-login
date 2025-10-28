using UnifiedLogin.SharedObjects.Landing;
using System;

namespace UnifiedLogin.SharedObjects.Landing
{
	/// <summary>
	/// Interface for Password Policy
	/// </summary>
	public interface IPasswordPolicy : IPasswordPolicyCommon
	{
		/// <summary>
		/// Unique Portfolio ID
		/// </summary>
		long PartyId { get; set; }

		/// <summary>
		/// Organization Name
		/// </summary>
		string Name { get; set; }

		/// <summary>
		/// Specify the maximum number of characters allowed in a user password
		/// </summary>
		Byte MaximumLength { get; set; }

		/// <summary>
		/// Minimum lowercase characters required in a user password
		/// </summary>
		Byte MinimumLowercase { get; set; }

		/// <summary>
		/// Minimum uppercase characters required in a user password
		/// </summary>
		Byte MinimumUppercase { get; set; }

		/// <summary>
		/// Minimum numbers required in a user password
		/// </summary>
		Byte MinimumNumeric { get; set; }

		/// <summary>
		/// Minimum special characters required in a user password
		/// </summary>
		Byte MinimumSpecialCharacter { get; set; }

		/// <summary>
		/// Permit Users to Change Their Own Password
		/// </summary>
		bool AllowUsersToChangeOwnPassword { get; set; }

		/// <summary>
		/// Enable user passwords to be valid for only the specified number of days
		/// </summary>
		bool EnablePasswordExpiration { get; set; }

		/// <summary>
		/// Prevent users from reusing a specified number of previous passwords
		/// </summary>
		bool PreventPasswordReuse { get; set; }

		/// <summary>
		/// System_DateTime period end datetime
		/// </summary>
		string SysEndDateTime { get; set; }

		/// <summary>
		/// System_DateTime period start datetime
		/// </summary>
		string SysStartDateTime { get; set; }
	}
}