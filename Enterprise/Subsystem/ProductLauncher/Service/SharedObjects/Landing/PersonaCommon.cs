using Newtonsoft.Json;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.UserManagement;
using System;
using System.Collections.Generic;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing
{
	/// <summary>
	/// Persona properties common to Persona and ProductUsers classes
	/// </summary>
	public class PersonaCommon : IPersonaCommon
	{
		/// <summary>
		/// Persona Unique Id
		/// </summary>
		public long PersonaId { get; set; }

		/// <summary>
		/// Person PartyId
		/// </summary>
		public long PersonPartyId { get; set; }

		/// <summary>
		/// Unique Identifier - EnterpriseUserId
		/// </summary>
		public Guid RealPageId { get; set; }

		/// <summary>
		/// Organization PartyId
		/// </summary>
		public long OrganizationPartyId { get; set; }

		/// <summary>
		/// Persona Name
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// UserId
		/// </summary>
		public long UserId { get; set; }

		/// <summary>
		/// List of User Personas
		/// </summary>
		[JsonProperty("Role", NullValueHandling = NullValueHandling.Ignore)]
		public IList<Role> Role { get; set; } = new List<Role>();

        /// <summary>
        /// Organization Domain
        /// </summary>
        public string OrganizationDomain { get; set; }
	}
}
