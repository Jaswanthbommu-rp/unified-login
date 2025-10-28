namespace UnifiedLogin.SharedObjects.IdentityConfig
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
		/// UserId
		/// </summary>
		long UserId { get; set; }
	}
}