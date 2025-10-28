using System;

namespace UnifiedLogin.SharedObjects.Landing
{
	/// <summary>
	/// Persona Configuration
	/// </summary>
	public class PersonaConfiguration : IPersonaConfiguration
	{
		/// <summary>
		/// PersonaConfiguration Unique Id
		/// </summary>
		public long PersonaConfigurationId { get; set; }

		/// <summary>
		/// PersonaId
		/// </summary>
		public long PersonaId { get; set; }

		/// <summary>
		/// ConfigurationId
		/// </summary>
		public int ConfigurationId { get; set; }

		/// <summary>
		/// ProductId
		/// </summary>
		public int ProductId { get; set; }

		/// <summary>
		/// Persona Configuration From Date
		/// </summary>
		public DateTime FromDate { get; set; }

		/// <summary>
		/// Persona Configuration Thru Date
		/// </summary>
		public DateTime ThruDate { get; set; }
	}
}
