using Newtonsoft.Json;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig
{
	/// <summary>
	/// PartyRole object
	/// </summary>
	public class PartyRole : IPartyRole
	{
		/// <summary>
		/// PartyRoleId
		/// </summary>
		[JsonProperty(PropertyName = "PartyRoleId")]
		public int PartyRoleId { get; set; } = new int();

		/// <summary>
		/// PartyId
		/// </summary>
		[JsonProperty(PropertyName = "PartyId")]
		public long PartyId { get; set; } = new long();

		/// <summary>
		/// RoleTypeId
		/// </summary>
		[JsonProperty(PropertyName = "RoleTypeId")]
		public int RoleTypeId { get; set; } = new int();

        /// <summary>
		/// Role name
		/// </summary>
        public string Name { get; set; }

    }
}
