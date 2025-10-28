using Newtonsoft.Json;
using UnifiedLogin.SharedObjects.Product.UnifiedLogin;
using System;
using System.Collections.Generic;

namespace UnifiedLogin.SharedObjects.Landing
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
	}
}
