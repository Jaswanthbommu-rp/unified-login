namespace UnifiedLogin.BusinessLogic.Logic.Enterprise.User.Models
{
	/// <summary>
	/// Used only for Enterprise API to indicate user status
	/// </summary>
	public enum EntApiUserStatus
	{
		/// <summary>
		/// Activate external API user status
		/// </summary>
		Activate = 1,

		/// <summary>
		/// Deactivate external API user status
		/// </summary>
		Deactivate = 2
	}
}
