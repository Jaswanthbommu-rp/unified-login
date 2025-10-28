using UnifiedLogin.SharedObjects.Product.UnifiedLogin;

namespace UnifiedLogin.SharedObjects.Landing
{
	/// <summary>
	/// Interface for PersonaCommon: Persona properties common to Persona and ProductUsers classes
	/// </summary>
	public interface IPersonaCommon
	{
		/// <summary>
		/// Persona Name
		/// </summary>
		string Name { get; set; }

		/// <summary>
		/// Organization PartyId
		/// </summary>
		long OrganizationPartyId { get; set; }

		/// <summary>
		/// Persona Unique Id
		/// </summary>
		long PersonaId { get; set; }

		/// <summary>
		/// Person PartyId
		/// </summary>
		long PersonPartyId { get; set; }

		/// <summary>
		/// Unique Identifier - EnterpriseUserId
		/// </summary>
		Guid RealPageId { get; set; }

		/// <summary>
		/// UserId
		/// </summary>
		long UserId { get; set; }

		/// <summary>
		/// List of User Personas
		/// </summary>
		IList<Role> Role { get; set; }
	}
}