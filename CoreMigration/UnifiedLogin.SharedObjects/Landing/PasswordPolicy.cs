using Newtonsoft.Json;
using UnifiedLogin.SharedObjects.Landing;
using System;

namespace UnifiedLogin.SharedObjects.Landing
{
	/// <summary>
	/// Password Policy
	/// </summary>
	public class PasswordPolicy : PasswordPolicyCommon, IPasswordPolicy
	{
		/// <summary>
		/// Unique Portfolio ID
		/// </summary>
		[JsonProperty(PropertyName = "PartyId")]
		public long PartyId { get; set; }

		/// <summary>
		/// Portfolio Name
		/// </summary>
		[JsonProperty(PropertyName = "Name")]
		public string Name { get; set; } = "RealPage";

		/// <summary>
		/// Specify the minimum number of characters allowed in a user password
		/// </summary>
		[JsonProperty(PropertyName = "MinimumLength")]
		public Byte MinimumLength { get; set; }

		/// <summary>
		/// Specify the maximum number of characters allowed in a user password
		/// </summary>
		[JsonProperty(PropertyName = "MaximumLength")]
		public Byte MaximumLength { get; set; }

		/// <summary>
		/// Minimum lowercase characters required in a user password
		/// </summary>
		[JsonProperty(PropertyName = "MinimumLowercase")]
		public Byte MinimumLowercase { get; set; }

		/// <summary>
		/// Minimum uppercase characters required in a user password
		/// </summary>
		[JsonProperty(PropertyName = "MinimumUppercase")]
		public Byte MinimumUppercase { get; set; }

		/// <summary>
		/// Minimum numbers required in a user password
		/// </summary>
		[JsonProperty(PropertyName = "MinimumNumeric")]
		public Byte MinimumNumeric { get; set; }

		/// <summary>
		/// Minimum special characters required in a user password
		/// </summary>
		[JsonProperty(PropertyName = "MinimumSpecialCharacter")]
		public Byte MinimumSpecialCharacter { get; set; }

		/// <summary>
		/// Permit Users to Change Their Own Password
		/// </summary>
		[JsonProperty(PropertyName = "AllowUsersToChangeOwnPassword")]
		public bool AllowUsersToChangeOwnPassword { get; set; }

		/// <summary>
		/// Enable user passwords to be valid for only the specified number of days
		/// </summary>
		[JsonProperty(PropertyName = "EnablePasswordExpiration")]
		public bool EnablePasswordExpiration { get; set; }

		/// <summary>
		/// Prevent users from reusing a specified number of previous passwords
		/// </summary>
		[JsonProperty(PropertyName = "PreventPasswordReuse")]
		public bool PreventPasswordReuse { get; set; }

		/// <summary>
		/// System_DateTime period start datetime
		/// </summary>
		[JsonProperty(PropertyName = "SysStartDateTime")]
		public string SysStartDateTime { get; set; }

		/// <summary>
		/// System_DateTime period end datetime
		/// </summary>
		[JsonProperty(PropertyName = "SysEndDateTime")]
		public string SysEndDateTime { get; set; }

		#region Examples
		/// <summary>
		/// Example for New Password Policy method
		/// </summary>
		/// <returns>Newly Created PasswordPolicy Id</returns>
		public static PasswordPolicyOutputResult GetNewPasswordPolicyExample()
		{
			PasswordPolicyOutputResult result = new PasswordPolicyOutputResult();
			result.NewPasswordPolicyId = 1;
			return result;
		}

		/// <summary>
		/// Output result for New PasswordPolicy
		/// </summary>
		public class PasswordPolicyOutputResult
		{
			/// <summary>
			/// Represents the newly created PasswordPolicy Id
			/// </summary>
			public long NewPasswordPolicyId { get; set; }
		}

		/// <summary>
		/// Example for New Security settings method
		/// </summary>
		/// <returns>Newly Created PasswordPolicy</returns>
		public static SecuritySettingsOutputResult GetNewSecuritySettingsExample()
		{
			SecuritySettingsOutputResult result = new SecuritySettingsOutputResult();
			IPasswordPolicy example = new PasswordPolicy()
			{
				PasswordPolicyId = 1,
				PartyId = 1,
				Name = "RealPage",
				MinimumLength = 8,
				MaximumLength = 128,
				MinimumLowercase = 0,
				MinimumUppercase = 0,
				MinimumNumeric = 0,
				MinimumSpecialCharacter = 0,
				AllowUsersToChangeOwnPassword = true,
				EnablePasswordExpiration = false,
				PasswordExpirationPeriodInDays = 0,
				PreventPasswordReuse = false,
				NumberOfPasswordsToRemember = 0
			};
			result.passwordPolicy = example;
			return result;
		}

		/// <summary>
		/// Example for New Security Settings method
		/// </summary>
		/// <returns>Newly Created PasswordPolicy Id</returns>
		public class SecuritySettingsOutputResult
		{
			/// <summary>
			/// Represents the newly created PasswordPolicy Id
			/// </summary>
			public IPasswordPolicy passwordPolicy { get; set; }
		}
		#endregion
	}
}
