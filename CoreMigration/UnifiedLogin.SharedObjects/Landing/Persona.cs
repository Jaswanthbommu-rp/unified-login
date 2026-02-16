using System;
using System.Diagnostics.CodeAnalysis;

namespace UnifiedLogin.SharedObjects.Landing
{
    /// <summary>
    /// Interface for Persona
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class Persona : PersonaCommon, IPersona
	{
        /// <summary>
        /// Organization Detail
        /// </summary>
        public Organization Organization { get; set; }
        /// <summary>
        /// Persona Name
        /// </summary>
        public string PersonaName { get; set; } = "Primary";

        /// <summary>
        /// Persona Type
        /// </summary>
        public int PersonaTypeId { get; set; }

        /// <summary>
        /// Persona Environment Type
        /// </summary>
        public int PersonaEnvironmentTypeId { get; set; }

        /// <summary>
        /// Persona From Date
        /// </summary>
        public DateTime? FromDate { get; set; }

        /// <summary>
        /// Persona thru Date
        /// </summary>
        public DateTime? ThruDate { get; set; }

        /// <summary>
        /// IsDefault Persona
        /// </summary>
        public bool? IsDefault { get; set; }

        /// <summary>
        /// Used to determine if a user exists in multiple companies
        /// </summary>
        public bool hasMultiCompany { get; set; }

        /// <summary>
        /// User to determine if a user has multo
        /// </summary>
        public bool hasMultiPersona { get; set; }

        /// <summary>
        /// Persona Has Resident Portal User Access
        /// </summary>
        public bool hasResidentPortalUserAccess { get; set; } = true;

        /// <summary>
        /// Persona Has Accounting User Access
        /// </summary>
        public bool hasManageAccountingProductAccess { get; set; } = true;
        /// <summary>
        /// Persona Has Asset Optimization User Access
        /// </summary>
        public bool hasManageAssetOptimizationProductAccess { get; set; } = true;
        /// <summary>
        /// Persona Has Real Connect User Access
        /// </summary>
        public bool hasManageRealConnectProductAccess { get; set; } = true;
        /// <summary>
        /// Persona Has Client Portal User Access
        /// </summary>
        public bool hasManageClientPortalProductAccess { get; set; } = true;
        /// <summary>
        /// Persona Has Document Management User Access
        /// </summary>
        public bool hasManageDocumentManagementProductAccess { get; set; } = true;
        /// <summary>
        /// Persona Has ILM  User Access
        /// </summary>
        public bool hasManageILMLeadManagemementProductAccess { get; set; } = true;
        /// <summary>
        /// Persona Has ILM Analytics User Access
        /// </summary>
        public bool hasManageILMLeasingAnalyticsProductAccess { get; set; } = true;
        /// <summary>
        /// Persona Has Lead2Lease User Access
        /// </summary>
        public bool hasManageLead2LeaseProductAccess { get; set; } = true;
        /// <summary>
        /// Persona Has Marketing Center User Access
        /// </summary>
        public bool hasManageMarketingCenterProductAccess { get; set; } = true;
        /// <summary>
        /// Persona Has Onesite User Access
        /// </summary>
        public bool hasManageOneSiteProductAccess { get; set; } = true;
        /// <summary>
        /// Persona Has On Site User Access
        /// </summary>
        public bool hasManageOnSiteProductAccess { get; set; } = true;
        /// <summary>
        /// Persona Has Prospect Contact Center User Access
        /// </summary>
        public bool hasProspectContactCenterProductAccess { get; set; } = true;
        /// <summary>
        /// Persona Has Renters Insurance User Access
        /// </summary>
        public bool hasManageRentersInsuranceProductAccess { get; set; } = true;
        /// <summary>
        /// Persona Has Spend Management User Access
        /// </summary>
        public bool hasManageSpendManagementProductAccess { get; set; } = true;
        /// <summary>
        /// Persona Has Unified Amenities User Access
        /// </summary>
        public bool hasManageUnifiedAmenitiesProductAccess { get; set; } = true;
        /// <summary>
        /// Persona Has Utility Mgmt User Access
        /// </summary>
        public bool hasManageUtilityManagementProductAccess { get; set; } = true;

        /// <summary>
        /// Persona Has Vendor Compliance User Access
        /// </summary>
        public bool hasManageVendorComplianceProductAccess { get; set; } = true;

		/// <summary>
		/// Persona Has PAM User Access
		/// </summary>
		public bool hasManagePortfolioManagementProductAccess { get; set; } = true;

        /// <summary>
		/// Persona Has Integration Marketplace User Access
		/// </summary>
		public bool hasManageIntegrationMarketplaceProductAccess { get; set; } = true;

        /// <summary>
		/// Manage Unified Platform Security Settings
		/// </summary>
		public bool hasManagePlatFormSecurity { get; set; } = false;

        /// <summary>
		/// Manage Custom User fields settings
		/// </summary>
		public bool hasManageCustomFields { get; set; } = false;

        ///<summary>
        /// Persona User Type
        ///</summary>
        public int? UserTypeId { get; set; }

		/// <summary>
		/// Persona Has View Only Support Tool Access
		/// </summary>
		public bool hasViewOnlySupportToolAccess { get; set; }

        /// <summary>
		/// Persona Has Settings Access
		/// </summary>
		public bool hasViewOnlySettingsAccess { get; set; } = false ;

        /// <summary>
        /// Persona Has Settings Access
        /// </summary>
        public bool hasManageUnifiedSettings { get; set; } = false;

        /// <summary>
        /// Persona Has Import Users Access
        /// </summary>
        public bool hasImportUsersAccess { get; set; } = false;
		
		/// <summary>
		/// Persona Has Deposit Alternative User Access
		/// </summary>
		public bool hasManageDepositAlternativeProductAccess { get; set; } = true;

		/// <summary>
		/// Persona Has Click Pay User Access
		/// </summary>
		public bool hasManageClickPayProductAccess { get; set; } = true;

        /// <summary>
        /// Persona Has Manage Template
        /// </summary>

        public bool hasManageSettingsTemplates { get; set; }

        /// <summary>
        /// Persona Has acess Settings Admin right
        /// </summary>

        public bool hasAccessSettingsAdmin { get; set; }

        /// <summary>
        /// Persona Has acess Notifications right
        /// </summary>

        public bool hasnotificationsAccess { get; set; }

        
        // <summary>
        /// Persona Has Intelligen tBuilding User Access
        /// </summary>
        public bool hasManageIntelligentBuildingProductAccess { get; set; } = true;
        // <summary>
        /// Persona Has Intelligen tBuilding trash User Access
        /// </summary>
        public bool hasManageIntelligentBuildingTrashProductAccess { get; set; } = true;
        // <summary>
        /// Persona Has Intelligen tBuilding energy User Access
        /// </summary>
        public bool hasManageIntelligentBuildingEnergyProductAccess { get; set; } = true;
        // <summary>
        /// Persona Has Intelligen tBuilding water User Access
        /// </summary>
        public bool hasManageIntelligentBuildingWaterProductAccess { get; set; } = true;
        // <summary>
        /// Persona Has HospitalityService User Access
        /// </summary>
        public bool hasManageHospitalityServiceAccess { get; set; } = true;
        // <summary>
        /// Persona Has LeadScoring User Access
        /// </summary>
        public bool hasManageLeadScoringAccess { get; set; } = true;
        /// Persona Has Smart Commerical Trash User Access
        /// </summary>
        public bool hasManageSmartWasteCommercialProductAccess { get; set; } = true;
        // <summary>
        // <summary>
        /// Persona Has SelfGuidedTour User Access
        /// </summary>
        public bool hasSelfGuidedTourAccess { get; set; } = true;
        // <summary>
        /// Persona Has Hands-On Training System User Access
        /// </summary>
        public bool hasManageHandsOnTrainingSystemAccess { get; set; } = true;

        /// <summary>
        /// Persona Has Platform Alerts Access
        /// </summary>
        public bool hasPlatformAlertsAccess { get; set; } = false;

        // <summary>
        /// Persona Has Hands-On Training System User Access
        /// </summary>
        public bool hasManageLeaseLabsAccess { get; set; } = true;

        /// <summary>
        /// Persona Has Admin & Support Portal User Access
        /// </summary>
        public bool hasManageAdminSupportPortalProductAccess { get; set; } = true;

        #region Examples
        /// <summary>
        /// Example for New Persona method
        /// </summary>
        /// <returns>Newly Created Party Id</returns>
        public static PersonaOutputResult GetNewPersonaExample()
		{
			PersonaOutputResult result = new PersonaOutputResult();
			result.PersonaId = 1;
			return result;
		}

		/// <summary>
		/// Output result for New Persona
		/// </summary>
		public class PersonaOutputResult
		{
			/// <summary>
			/// Represents the newly created Persona Id
			/// </summary>
			public long PersonaId { get; set; }
		}
		#endregion
	}
}