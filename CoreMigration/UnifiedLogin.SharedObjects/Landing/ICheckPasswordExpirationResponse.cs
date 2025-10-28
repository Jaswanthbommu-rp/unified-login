using UnifiedLogin.SharedObjects.Enum;

namespace UnifiedLogin.SharedObjects.Landing
{
	/// <summary>
	/// Password Expiration Response Object
	/// </summary>
	public interface ICheckPasswordExpirationResponse
	{
		/// <summary>
		/// Days to expire password
		/// </summary>
		int DaysToExpire { get; set; }

		/// <summary>
		/// IsPasswordExpired (true if daysToExpire less or equal to 0; otherwise false)  
		/// </summary>
		bool IsPasswordExpired { get; set; }

		/// <summary>
		/// Severity Level for message
		/// </summary>
		SeverityLevelType SeverityLevel { get; set; }
	}
}