using System;

namespace UnifiedLogin.SharedObjects.Landing
{
	/// <summary>
	/// Organization Product User Details
	/// </summary>
	public class PersonaProductUserDetails : ProductUserDetails
	{
        /// <summary>
        /// The unique id User Persona
        /// </summary>
        public long PersonaId { get; set; } = 0;

		/// <summary>
		/// The Organization PartyId
		/// </summary>
		public long OrganizationPartyId { get; set; } = 0;

        /// <summary>
        /// The name of the Organization
        /// </summary>
        public string OrganizationName { get; set; } = "";     
           
    }
}