using Newtonsoft.Json;
using System;

namespace UnifiedLogin.SharedObjects.Landing
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
		/// <summary>
		/// Organization Domain Id
		/// </summary>
		[JsonIgnore]
		public int OrganizationDomainId { get; set; }

		/// <summary>
		/// Organization Domain
		/// </summary>
		[JsonProperty(PropertyName = "OrganizationDomain")]
		public OrganizationDomain OrganizationDomain { get; set; }

		/// <summary>
		/// PersonaId
		/// </summary>
		[JsonProperty("PersonaId")]
		public long PersonaId { get; set; }

		/// <summary>
		/// Organization Name
		/// </summary>
		[JsonProperty("OrganizationName")]
		public string OrganizationName { get; set; }
	}
}
