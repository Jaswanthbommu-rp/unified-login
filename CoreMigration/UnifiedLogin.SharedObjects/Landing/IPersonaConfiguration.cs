using System;

namespace UnifiedLogin.SharedObjects.Landing
{
	/// <summary>
	/// Interface for PersonaConfiguration
	/// </summary>
	public interface IPersonaConfiguration
	{
		/// <summary>
		/// ConfigurationId
		/// </summary>
		int ConfigurationId { get; set; }

		/// <summary>
		/// Persona Configuration From Date
		/// </summary>
		DateTime FromDate { get; set; }

		/// <summary>
		/// PersonaConfiguration Unique Id
		/// </summary>
		long PersonaConfigurationId { get; set; }

		/// <summary>
		/// PersonaId
		/// </summary>
		long PersonaId { get; set; }

		/// <summary>
		/// ProductId
		/// </summary>
		int ProductId { get; set; }

		/// <summary>
		/// Persona Configuration Thru Date
		/// </summary>
		DateTime ThruDate { get; set; }
	}
}