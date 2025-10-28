using System.ComponentModel;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.SharedObjects.Enum
{
	/// <summary>
	/// User type
	/// </summary>
	public enum UserRoleType : int
    {
		/// <summary>
		/// Regular User with email
		/// </summary>
		[Description("Regular User")]
		User = 401,

		/// <summary>
		/// Real Page Admistrator(Super User)
		/// </summary>
		[Description("RealPage System Administrator")]
		SuperUser = 402,

		/// <summary>
		/// Real Page Employee
		/// </summary>
		[Description("RealPage Employee")]
		RealPageEmployee = 403,

		/// <summary>
		/// Regaular user with no email
		/// </summary>
		[Description("User (No Email)")]
		UserNoEmail = 404,

		/// <summary>
		/// External user
		/// </summary>
		[Description("External User")]
		ExternalUser = 405,

		/// <summary>
		/// SDE user
		/// </summary>
		[Description("SDE")]
		SDE = 406
    }
}
