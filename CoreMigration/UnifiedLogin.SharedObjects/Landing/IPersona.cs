using System;

namespace UnifiedLogin.SharedObjects.Landing
{
    /// <summary>
    /// Persona Object
    /// </summary>
    public interface IPersona
    {
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
        /// Organization PartyId
        /// </summary>
        long OrganizationPartyId { get; set; }

        /// <summary>
        /// Organization Detail
        /// </summary>
        Organization Organization { get; set; }

        /// <summary>
        /// Persona Type
        /// </summary>
        int PersonaTypeId { get; set; }

        /// <summary>
        /// Persona Environment Type
        /// </summary>
        int PersonaEnvironmentTypeId { get; set; }

        /// <summary>
        /// Persona From Date
        /// </summary>
        DateTime? FromDate { get; set; }

        /// <summary>
        /// Persona thru Date
        /// </summary>
        DateTime? ThruDate { get; set; }

        /// <summary>
        /// IsDefault Persona
        /// </summary>
        bool? IsDefault { get; set; }

        /// <summary>
        /// Persona Name
        /// </summary>
        string Name { get; set; }

		/// <summary>
		/// UserId
		/// </summary>
		long UserId { get; set; }

        /// <summary>
        /// Persona Has Resident Portal User Access
        /// </summary>
        bool hasResidentPortalUserAccess { get; set; }

        ///<summary>
        /// Persona User Type
        ///</summary>
        int? UserTypeId { get; set; }

		///<summary>
		///Persona has VIEWONLYSUPPORTTOOLACCESS
		///</summary>
		bool hasViewOnlySupportToolAccess { get; set; }

        /// <summary>
		/// Persona Has Settings Access
		/// </summary>
		bool hasViewOnlySettingsAccess { get; set;  } 
    }
}
