using Newtonsoft.Json;
using System;

namespace UnifiedLogin.SharedObjects.Landing
{
	/// <summary>
	/// Common Password Policy properties - Unified Security Settings
	/// </summary>
	public class PasswordPolicyCommon : IPasswordPolicyCommon
	{
		/// <summary>
		/// Unique Password Policy ID
		/// </summary>
		[JsonProperty(PropertyName = "PasswordPolicyId")]
		public int PasswordPolicyId { get; set; }

		/// <summary>
		/// Specify the minimum number of characters allowed in a user password
		/// </summary>
		[JsonProperty(PropertyName = "MinimumLength")]
		public Byte MinimumLength { get; set; }

		/// <summary>
		/// Number of days a password is valid for
		/// </summary>
		[JsonProperty(PropertyName = "PasswordExpirationPeriodInDays")]
		public Int16 PasswordExpirationPeriodInDays { get; set; }

		/// <summary>
		/// Number of previous passwords a user is not allowed to reuse
		/// </summary>
		[JsonProperty(PropertyName = "NumberOfPasswordsToRemember")]
		public Byte NumberOfPasswordsToRemember { get; set; }

		/// <summary>
		/// References the User Unique ID
		/// </summary>
		[JsonProperty(PropertyName = "UserId")]
		public long UserId { get; set; }
	}
}
