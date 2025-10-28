namespace UnifiedLogin.SharedObjects.IdentityConfig
{
	/// <summary>
	/// Interface for RoleType
	/// </summary>
	public interface IRoleType
	{
		/// <summary>
		/// RoleType Name
		/// </summary>
		string Name { get; set; }

		/// <summary>
		/// Parent Party RoleTypeId
		/// </summary>
		int ParentPartyRoleTypeId { get; set; }

		/// <summary>
		/// Party RoleTypeId
		/// </summary>
		int PartyRoleTypeId { get; set; }
	}
}