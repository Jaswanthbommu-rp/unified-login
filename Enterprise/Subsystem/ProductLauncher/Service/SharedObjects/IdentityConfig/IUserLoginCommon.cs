namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig
{
	/// <summary>
	/// Interface for UserLoginCommon - UserLogin properties common to UserLogin and ProductUsers classes
	/// </summary>
	public interface IUserLoginCommon
	{
		/// <summary>
		/// LoginName
		/// </summary>
		string LoginName { get; set; }

		/// <summary>
		/// User Time Zone Offset Value
		/// </summary>
		string TimeZoneOffset { get; set; }

		/// <summary>
		/// UserId
		/// </summary>
		long UserId { get; set; }
	}
}