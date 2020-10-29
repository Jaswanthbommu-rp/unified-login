using Newtonsoft.Json;
using System;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing
{
	/// <summary>
	/// Does LoginName/UserName exists in anywhere in Unified Login
	/// </summary>
	public class UserOrganization : IUserOrganization
	{
		/// <summary>
		/// Role name
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// The name of the Organization
		/// </summary>
		[JsonProperty(PropertyName = "OrganizationPartyId")]
		public long OrganizationPartyId { get; set; }

		/// <summary>
		/// The unique id for the Organization
		/// </summary>
		[JsonProperty(PropertyName = "OrganizationRealPageId")]
		public Guid OrganizationRealPageId { get; set; }

		/// <summary>
		/// Party RoleTypeId
		/// </summary>
		[JsonProperty(PropertyName = "PartyRoleTypeId")]
		public int PartyRoleTypeId { get; set; } = new int();

        /// <summary>
        /// Is the org the primary login org for the user
        /// </summary>
        public bool PrimaryOrganization { get; set; }

        /// <summary>
        /// Used to store the BlueBook master id for the organization
        /// </summary>
        public long BooksMasterId { get; set; }

        /// <summary>
        /// Used to store the BlackBook Company master id for the organization RPUP id
        /// </summary>
        public long BooksCustomerMasterId { get; set; }

    }
}
