namespace UnifiedLogin.SharedObjects.IdentityConfig
{
	/// <summary>
	/// Interface for TelecommunicationNumber
	/// </summary>
	public interface ITelecommunicationNumber
	{
		/// <summary>
		/// Area Code
		/// </summary>
		string AreaCode { get; set; }

		/// <summary>
		/// Contact Contact Mechanism unique Id
		/// </summary>
		int ContactMechanismId { get; set; }

		/// <summary>
		/// Country Code
		/// </summary>
		string CountryCode { get; set; }

		/// <summary>
		/// Party Contact Mechanism unique Id
		/// </summary>
		long PartyContactMechanismId { get; set; }

		/// <summary>
		/// PhoneNumber
		/// </summary>
		string PhoneNumber { get; set; }

        /// <summary>
        /// IsDeleted
        /// </summary>        
        bool IsDeleted { get; set; }

		/// <summary>
		/// IsPreferred
		/// </summary>
		bool IsPreferred { get; set; }

		/// <summary>
		/// Contact Mechanism usage type Id
		/// </summary>
		int ContactMechanismUsageTypeId { get; set; }

		/// <summary>
		/// ISOCode
		/// </summary>		
		string ISOCode { get; set; }

        /// <summary>
        /// IsDefault
        /// </summary>
        bool IsDefault { get; set; }

        /// <summary>
        /// Contact Mechanism usage type detail
        /// </summary>
        ContactMechanismUsageType contactMechanismUsageType { get; set; }
	}
}