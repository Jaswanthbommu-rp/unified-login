namespace UnifiedLogin.SharedObjects.IdentityConfig
{
	/// <summary>
	/// Interface for PartyRole
	/// </summary>
	public interface IPartyRole
	{
		/// <summary>
		/// PartyId
		/// </summary>
		long PartyId { get; set; }

		/// <summary>
		/// PartyRoleId
		/// </summary>
		int PartyRoleId { get; set; }

		/// <summary>
		/// RoleTypeId
		/// </summary>
		int RoleTypeId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        string Name { get; set; }
    }
}