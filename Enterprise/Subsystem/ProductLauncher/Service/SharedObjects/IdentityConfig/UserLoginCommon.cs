using Newtonsoft.Json;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig
{
	/// <summary>
	/// UserLogin properties common to UserLogin and ProductUsers classes
	/// </summary>
	public class UserLoginCommon : IUserLoginCommon
	{
		/// <summary>
		/// UserId
		/// </summary>
		[JsonProperty(PropertyName = "UserId")]
		public long UserId { get; set; }

		/// <summary>
		/// LoginName
		/// </summary>
		[JsonProperty(PropertyName = "LoginName")]
		public string LoginName { get; set; }
	}
}
