using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum
{
	/// <summary>
	/// Product Enum Helper
	/// </summary>
	public static class ProductEnumHelper
	{
		public static string StringValueOf(ProductEnum value)
		{
			FieldInfo fi = value.GetType().GetField(value.ToString());
			DescriptionAttribute[] attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);
			if (attributes.Length > 0)
			{
				return attributes[0].Description;
			}
			else
			{
				return value.ToString();
			}
		}

		/// <summary>
		/// Get ProductName by Product Id
		/// </summary>
		/// <param name="productEnum">Product Id identifier</param>
		/// <returns>Name of the Product</returns>
		public static string GetProductRaulLabel(ProductEnum productEnum)
		{

			// When adding a product, please verify the return value by navigating to the page listed below and mousing over the icon.
			// The resulting tooltip will show 'icon="[value]"' where [value] is the value to return here.
			// https://cdn.realpage.com/raul/v2.85.0/styleguide/components/icons.html
			switch (productEnum)
			{
				case ProductEnum.OneSite: return "onesite";
				case ProductEnum.UnifiedUI: return "unified-ui";
				case ProductEnum.UnifiedPlatform: return "unified-platform";
				case ProductEnum.AssetOptimizer: return "asset-optimization";
				case ProductEnum.Propertyware: return "propertyware";
				case ProductEnum.Lead2Lease: return "lead2lease";
				case ProductEnum.Yieldstar: return "yieldstar";
				case ProductEnum.FinancialSuite: return "realpage-accounting";
				case ProductEnum.MarketingCenter: return "marketing-center";
				case ProductEnum.ProspectContactCenter: return "prospect-contact-center";
				case ProductEnum.Social: return "social";
				case ProductEnum.OpsBid: return "opsbid";
				case ProductEnum.OpsBuyer: return "spend-management";
				case ProductEnum.ClientPortal: return "client-portal";
				case ProductEnum.Insurance: return "renters-insurance";
				case ProductEnum.VendorServices: return "vendor-services";
				case ProductEnum.ResidentPortal: return "resident-portals";
				case ProductEnum.UtilityManagement: return "utility-management";
				case ProductEnum.ProductLearningPortal: return "learning-portal";
				case ProductEnum.RPDocumentManagement: return "realpage-document-management";
				case ProductEnum.OneSiteConversions: return "leasing-and-rent-conversion-tool";
				case ProductEnum.OmniChannel: return "rentjoy";
				case ProductEnum.OnSite: return "on-site";
				case ProductEnum.ResearchApplication: return "research-application";
				case ProductEnum.SelfProvisioningPortal: return "self-provisioning-portal";
				case ProductEnum.UnifiedAmenities: return "unified-amenities";
				case ProductEnum.MigrationTool: return "migration-tool";
				case ProductEnum.ProductUpdates: return "product-updates";
				case ProductEnum.AoBusinessIntelligence: return "business-intelligence";
				case ProductEnum.AoPerformanceAnalytics: return "performance-analytics";
				case ProductEnum.AoInvestmentAnalytics: return "investment-analytics";
				case ProductEnum.AoRevenueManagement: return "revenue-management";
				case ProductEnum.AoAxiometrics: return "axiometrics";
				case ProductEnum.AoBenchmarking: return "benchmarking";
				case ProductEnum.SupportTool: return "support-tool";
				case ProductEnum.EasyLMS: return "easy-lms";
				case ProductEnum.PropertyPhotos: return "property-photos";
				case ProductEnum.VendorMarketplace: return "vendor-marketplace";
				case ProductEnum.IntegrationMarketplace: return "integration-marketplace";
				case ProductEnum.CIMPL: return "cimpl";
				case ProductEnum.PortfolioManagement: return "portfolio-asset-management";
				case ProductEnum.LeadManagement: return "intelligent-lead-management";
				case ProductEnum.LeadAnalytics: return "ilm-leasing-analytics";
				case ProductEnum.DepositAlternative: return "deposit-iq";
				case ProductEnum.ClickPay: return "payments";
                case ProductEnum.HelpCenter: return "help-center";
                case ProductEnum.SeniorLeadManagement: return "senior-lead-management"; // Need this value replaced with actual based on the CDN value
				case ProductEnum.AoLeaseRentOption: return "lro";
				case ProductEnum.AoAmenityOptimization: return "amenity-analytics";
				case ProductEnum.AoAIRevenueManagement: return "ai-revenue-management";
				case ProductEnum.AoRentControl: return "rent-control";
				case ProductEnum.RenovationManager: return "renovation-manager";
				case ProductEnum.IntelligentBuildingTrash: return "intelligent-building-trash";
				case ProductEnum.IntelligentBuildingEnergy: return "intelligent-building-energy";
				case ProductEnum.IntelligentBuildingWater: return "intelligent-building-water";
				case ProductEnum.HospitalityService: return "resident-services"; // Temp image until product has defined their final production
				case ProductEnum.PMEDasboard: return "product-updates";
				case ProductEnum.HandsOnTrainingSystem: return "hots";
				case ProductEnum.P2EngagementQueue: return "video-call-laptop";
				case ProductEnum.AoMarketAnalytics: return "marketing-analytics";
				case ProductEnum.LeaseLabs: return "lease-labs";
			}
			return null;
		}

		/// <summary>
		/// Get enum list of AO products
		/// </summary>
		/// <returns></returns>
		public static IList<ProductEnum> GetAoProductList()
		{
			return new List<ProductEnum>
				{
					ProductEnum.AoBusinessIntelligence,
					ProductEnum.AoInvestmentAnalytics,
					ProductEnum.AoPerformanceAnalytics,
					ProductEnum.AoRevenueManagement,
					ProductEnum.AoAxiometrics,
					ProductEnum.AoBenchmarking,
					ProductEnum.AoLeaseRentOption,
					ProductEnum.AoAIRevenueManagement,
					ProductEnum.AoAmenityOptimization,
					ProductEnum.AoRentControl,
					ProductEnum.AoMarketAnalytics,
			};
		}

		/// <summary>
		/// Check AO product is supported in GB
		/// </summary> 
		public static bool CheckAoProductSupportedByGreenBook(string productName)
		{
			try
			{
				GetAoProductEnum(productName);
				return true;
			}
			catch
			{
				return false;
			}
		}

		/// <summary>
		/// Get Ao Product Division Name by Ao product Enum
		/// </summary>
		public static string GetAoDivisionName(ProductEnum aoProductProductEnum)
		{
			switch (aoProductProductEnum)
			{
				case ProductEnum.AoBusinessIntelligence: return "BI";

				case ProductEnum.AoInvestmentAnalytics: return "MPF";
				case ProductEnum.AoAxiometrics: return "MPF";
				case ProductEnum.AoMarketAnalytics: return "MPF";

				case ProductEnum.AoPerformanceAnalytics: return "YIELDSTAR";
				case ProductEnum.AoRevenueManagement: return "YIELDSTAR";
				case ProductEnum.AoBenchmarking: return "YIELDSTAR";
				case ProductEnum.AoLeaseRentOption: return "YIELDSTAR";
				case ProductEnum.AoAmenityOptimization: return "YIELDSTAR";
				case ProductEnum.AoAIRevenueManagement: return "YIELDSTAR";
				case ProductEnum.AoRentControl: return "YIELDSTAR";
			}

			return null;
		}

		/// <summary>
		/// Get Ao Product ID by product enum (replace with BB call)
		/// </summary>
		public static string GetAoProductId(ProductEnum aoProductProductEnum)
		{
			if (aoProductProductEnum != null)
			{
				switch (aoProductProductEnum)
				{
					case ProductEnum.AoBusinessIntelligence: return "BI";

					case ProductEnum.AoInvestmentAnalytics: return "MA";
					case ProductEnum.AoAxiometrics: return "AX";

					case ProductEnum.AoPerformanceAnalytics: return "PA";
					case ProductEnum.AoRevenueManagement: return "PO";
					case ProductEnum.AoBenchmarking: return "BM";
					case ProductEnum.AoLeaseRentOption: return "LRO";
					case ProductEnum.AoAmenityOptimization: return "AA";
					case ProductEnum.AoAIRevenueManagement: return "AIRM";
					case ProductEnum.AoRentControl: return "RC";
					case ProductEnum.AoMarketAnalytics: return "RMA";
				}
			}

			return null;
		}

		public static ProductEnum GetAoProductEnum(string productCode)
		{
			switch (productCode)
			{
				case "BI": return ProductEnum.AoBusinessIntelligence;

				case "MA": return ProductEnum.AoInvestmentAnalytics;
				case "AX": return ProductEnum.AoAxiometrics;

				case "PA": return ProductEnum.AoPerformanceAnalytics;
				case "PO": return ProductEnum.AoRevenueManagement;
				case "BM": return ProductEnum.AoBenchmarking;
				case "LRO": return ProductEnum.AoLeaseRentOption;
				case "AA": return ProductEnum.AoAmenityOptimization;
				case "AIRM": return ProductEnum.AoAIRevenueManagement;
				case "RC": return ProductEnum.AoRentControl;
				case "RMA": return ProductEnum.AoMarketAnalytics;

			}

			throw new Exception($"AO product with Id - {productCode} is not supported in green book.");
		}

		public static ProductEnum GetUPFMProductEnum(int productID)
		{
			switch (productID)
			{
				case 57 : return ProductEnum.IntelligentBuildingTrash;
				case 58 : return ProductEnum.IntelligentBuildingEnergy;
				case 59 : return ProductEnum.IntelligentBuildingWater;
				case 60 : return ProductEnum.HospitalityService;
				case 63: return ProductEnum.HandsOnTrainingSystem;
				case 68: return ProductEnum.LeaseLabs;
			}

			throw new Exception($"UPFM product with Id - {productID} is not supported in green book.");
		}
		/// <summary>
		/// GetProductEnumByProductCode
		/// </summary>
		/// <param name="productCode">Product Code</param>
		/// <returns>ProductEnum</returns>
		public static ProductEnum GetProductEnumByProductCode(string productCode)
		{
			var ProductEnumsList = System.Enum.GetValues(typeof(ProductEnum));

			foreach (object pEnum in ProductEnumsList)
			{
				string result;
				FieldInfo fi = typeof(ProductEnum).GetField(pEnum.ToString());
				if (fi != null)
				{
					try
					{
						object[] descriptionAttrs = fi.GetCustomAttributes(typeof(DescriptionAttribute), false);
						DescriptionAttribute description = (DescriptionAttribute)descriptionAttrs[0];
						result = (description.Description);
						if (result.Equals(productCode, StringComparison.OrdinalIgnoreCase))
							return (ProductEnum)pEnum;
					}
					catch
					{
						result = null;
					}
				}
			}

			//If Code reach here that means product code did not match with any Product Enum Description value. So raise an exception
			throw new Exception("Invalid product code");
		}
	}

	/// <summary>
	/// Used to identify products by id.
	/// </summary>
	public enum ProductEnum
	{
		/// <summary>
		/// OneSite
		/// </summary>
		[Description("OS")]
		OneSite = 1,

		/// <summary>
		/// Unified Ui
		/// </summary>
		[Description("UI")]
		UnifiedUI = 2,

		/// <summary>
		/// The Greenbook landing website
		/// </summary>
		[Description("UPFM")]
		UnifiedPlatform = 3,

		/// <summary>
		/// Asset Optimizer - Umbrella for all AO products
		/// </summary>
		[Description("AO")]
		AssetOptimizer = 4,

		/// <summary>
		/// Propertyware
		/// </summary>
		[Description("PW")]
		Propertyware = 5,

		/// <summary>
		/// Lead2Lease
		/// </summary>
		[Description("L2L")]
		Lead2Lease = 6,

		/// <summary>
		/// Yieldstar - THIS IS NOT REQUIRED
		/// </summary>
		[Description("YS")]
		Yieldstar = 7, //TODO: This can be replaced with some other products

		/// <summary>
		/// Accounting - Financial Suite
		/// </summary>
		[Description("ACCT")]
		FinancialSuite = 8,

		/// <summary>
		/// Marketing Center
		/// </summary>
		[Description("LS")]
		MarketingCenter = 9,

		/// <summary>
		/// Prospect Contact Center
		/// </summary>
		[Description("LVL1")]
		ProspectContactCenter = 10,

		/// <summary>
		/// Social
		/// </summary>
		[Description("??")]
		Social = 11,

		/// <summary>
		/// OpsBid
		/// </summary>
		[Description("OPSB")]
		OpsBid = 12,

		/// <summary>
		/// OpsBuyer
		/// </summary>
		[Description("OPS")]
		OpsBuyer = 13,

		/// <summary>
		/// SalesForce ClientPortal
		/// </summary>
		[Description("OMS")]
		ClientPortal = 14,

		/// <summary>
		/// Insurance
		/// </summary>
		[Description("LD")]
		Insurance = 15,

		/// <summary>
		/// Vendor Credentialing
		/// </summary>
		[Description("CD")]
		VendorServices = 16,

		/// <summary>
		/// Resident Portal (Active Building)
		/// </summary>
		[Description("AB")]
		ResidentPortal = 17,

		/// <summary>
		/// Utility Management
		/// </summary>
		[Description("NWP")]
		UtilityManagement = 18,

		/// <summary>
		/// Product Learning Portal
		/// </summary>
		[Description("LP")]
		ProductLearningPortal = 19,
        		
		/// <summary>
		/// RPDocumentManagement
		/// </summary>
        [Description("DOC")]		
        RPDocumentManagement = 20,

		/// <summary>
		/// OneSiteConversions
		/// </summary>
		[Description("OSC")]
		OneSiteConversions = 21,

		/// <summary>
		/// OmniChannel
		/// </summary>
		[Description("OC")]
		OmniChannel = 22,

		/// <summary>
		/// OnSite
		/// </summary>
		[Description("ONST")]
		OnSite = 23,

		/// <summary>
		/// ResearchApplication - BlackBook
		/// </summary>
		[Description("RA")]
		ResearchApplication = 24,

		/// <summary>
		/// SelfProvisioningPortal
		/// </summary>
		[Description("SP")]
		SelfProvisioningPortal = 25,

		/// <summary>
		/// UnifiedAmenities
		/// </summary>
		[Description("UA")]
		UnifiedAmenities = 26,

		/// <summary>
		/// Migration Tool
		/// </summary>
		[Description("MT")]
		MigrationTool = 27,

		/// <summary>
		/// Product Updates
		/// </summary>
		[Description("PUPDATE")]
		ProductUpdates = 28,

		/// <summary>
		/// AO Business Intelligence
		/// </summary>
		[Description("BI")]
		AoBusinessIntelligence = 29,

		/// <summary>
		/// Ao Performance Analytics
		/// </summary>
		[Description("PA")]
		AoPerformanceAnalytics = 30,

		/// <summary>
		/// AO Investment Analytic
		/// </summary>
		[Description("MA")]
		AoInvestmentAnalytics = 31,

		/// <summary>
		/// Ao Revenue Management
		/// </summary>
		[Description("PO")]
		AoRevenueManagement = 32,

		/// <summary>
		/// Ao Axiometrics
		/// </summary>
		[Description("AX")]
		AoAxiometrics = 33,

		/// <summary>
		/// Ao Benchmarking
		/// </summary>
		[Description("BM")]
		AoBenchmarking = 34,

		/// <summary>
		/// Search Tool
		/// </summary>
		[Description("ST")]
		SupportTool = 35,

		/// <summary>
		/// EasyLMS
		/// </summary>
		[Description("ELMS")]
		EasyLMS = 36,

		/// <summary>
		/// Property Photos
		/// </summary>
		[Description("PHOTO")]
		PropertyPhotos = 37,

		/// <summary>
		/// VendorMarketplace
		/// </summary>
		[Description("VMP")]
		VendorMarketplace = 38,

		/// <summary>
		/// Integration Marketplace
		/// </summary>
		[Description("IMP")]
		IntegrationMarketplace = 39,

		/// <summary>
		/// VendorMarketplace
		/// </summary>
		[Description("ILMLM")]
		LeadManagement = 40,

		/// <summary>
		/// VendorMarketplace
		/// </summary>
		[Description("ILMLA")]
		LeadAnalytics = 41,

		/// <summary>
		/// SalesForce
		/// </summary>
		[Description("SF")]
		SalesForce = 42,

		/// <summary>
		/// Settings Management
		/// </summary>
		[Description("SM")]
		SettingsManagement = 43,

		/// <summary>
		/// Portfolio Management
		/// </summary>
		[Description("RPM")]
		PortfolioManagement = 44,

		/// <summary>
		/// CIMPL
		/// </summary>
		[Description("CIMPL")]
		CIMPL = 45,

		/// <summary>
		/// Financial Suite - Site Spend Management
		/// </summary>
		[Description("SSM")]
		SiteSpendManagement = 46,

		/// <summary>
		/// Deposit IQ aka Deposit Alternative
		/// </summary>
		[Description("DIQ")]
		DepositAlternative = 47,

		/// <summary>
		/// Click Pay
		/// </summary>
		[Description("CPAY")]
		ClickPay = 48,

        /// <summary>
		/// Help Center
		/// </summary>
		[Description("HLP")]
        HelpCenter = 49,

        /// <summary>
		/// Senior Lead Management
		/// </summary>
		[Description("SLM")]
        SeniorLeadManagement = 50,

		/// <summary>
		/// AO LeaseRentOption
		/// </summary>
		[Description("LRO")]
		AoLeaseRentOption = 51,

		/// <summary>
		/// AO Amenity Optimization
		/// </summary>
		[Description("AA")]
		AoAmenityOptimization = 52,

		/// <summary>
		/// AO AI Revenue Management
		/// </summary>
		[Description("AIRM")]
		AoAIRevenueManagement = 53,

		/// <summary>
		/// AO Rent Control
		/// </summary>
		[Description("RC")]
		AoRentControl = 54,
		/// <summary>
		///Renovation Manager
		/// </summary>
		[Description("RENO")]
		RenovationManager = 55,
		/// <summary>
		/// UnifiedSettings
		/// </summary>
		[Description("SET")]
		UnifiedSettings = 56,
		/// <summary>
		/// IB Trash
		/// </summary>
		[Description("SMS-T")]
		IntelligentBuildingTrash = 57,
		/// <summary>
		/// IB energy
		/// </summary>
		[Description("SMS-E")]
		IntelligentBuildingEnergy = 58,
		/// <summary>
		/// IB water
		/// </summary>
		[Description("SMS-W")]
		IntelligentBuildingWater = 59,
		/// <summary>
		/// Hospitality as a Service
		/// </summary>
		[Description("HAAS")]
		HospitalityService = 60,
		/// <summary>
		/// PME Dashboard
		/// </summary>
		[Description("PME")]
		PMEDasboard = 62,
		/// <summary>
		/// HOTS
		/// </summary>
		[Description("HOTS")]
		HandsOnTrainingSystem = 63,
		/// <summary>
		/// P2EngagementQueue
		/// </summary>
		[Description("PEQ")]
		P2EngagementQueue = 64,
		/// <summary>
		/// AO Market Analytics
		/// </summary>
		[Description("RMA")]
		AoMarketAnalytics = 66,
		/// <summary>
		/// LeaseLabs
		/// </summary>
		[Description("LeaseLabs")]
		LeaseLabs = 68,
		/// <summary>
		/// Reporting
		/// </summary>
		[Description("RPT")]
		Reporting = 67
	}

	/// <summary>
	/// Used to identify products by id.
	/// </summary>
	public enum ProductRightEnum
	{
		/// <summary>
		/// OneSite
		/// </summary>
		[Description("OS")]
		ManageOneSiteProductAccess = 1,

		/// <summary>
		/// Asset Optimizer - Umbrella for all AO products
		/// </summary>
		[Description("AO")]
		ManageAssetOptimizationProductAccess = 4,

		/// <summary>
		/// Lead2Lease
		/// </summary>
		[Description("L2L")]
		ManageLead2LeaseProductAccess = 6,

		/// <summary>
		/// Accounting
		/// </summary>
		[Description("ACCT")]
		ManageAccountingProductAccess = 8,

		/// <summary>
		/// Marketing Center
		/// </summary>
		[Description("LS")]
		ManageMarketingCenterProductAccess = 9,

		/// <summary>
		/// Prospect Contact Center
		/// </summary>
		[Description("LVL1")]
		ProspectContactCenterProductAccess = 10,

		/// <summary>
		/// OpsBuyer
		/// </summary>
		[Description("OPS")]
		ManageSpendManagementProductAccess = 13,

		/// <summary>
		/// SalesForce ClientPortal
		/// </summary>
		[Description("OMS")]
		ManageClientPortalProductAccess = 14,

		/// <summary>
		/// Insurance
		/// </summary>
		[Description("LD")]
		ManageRentersInsuranceProductAccess = 15,

		/// <summary>
		/// Vendor Credentialing
		/// </summary>
		[Description("CD")]
		ManageVendorComplianceProductAccess = 16,

		/// <summary>
		/// Resident Portal (Active Building)
		/// </summary>
		[Description("AB")]
		AddEditResidentPortalUser = 17,

		/// <summary>
		/// Utility Management
		/// </summary>
		[Description("NWP")]
		ManageUtilityManagementProductAccess = 18,

		/// <summary>
		/// RPDocumentManagement
		/// </summary>
		ManageDocumentManagementProductAccess = 20,

		/// <summary>
		/// OnSite
		/// </summary>
		[Description("ONST")]
		ManageOnSiteProductAccess = 23,

		/// <summary>
		/// UnifiedAmenities
		/// </summary>
		[Description("UA")]
		ManageUnifiedAmenitiesProductAccess = 26,

		/// <summary>
		/// AO Business Intelligence
		/// </summary>
		[Description("BI")]
		AoBusinessIntelligence = 29,

		/// <summary>
		/// Ao Performance Analytics
		/// </summary>
		[Description("PA")]
		AoPerformanceAnalytics = 30,

		/// <summary>
		/// AO Investment Analytic
		/// </summary>
		[Description("MA")]
		AoInvestmentAnalytics = 31,

		/// <summary>
		/// Ao Revenue Management
		/// </summary>
		[Description("PO")]
		AoRevenueManagement = 32,

		/// <summary>
		/// Ao Axiometrics
		/// </summary>
		[Description("AX")]
		AoAxiometrics = 33,

		/// <summary>
		/// Ao Benchmarking
		/// </summary>
		[Description("BM")]
		AoBenchmarking = 34,

		/// <summary>
		/// Ao LRO
		/// </summary>
		[Description("LRO")]
		AoLeaseRentOption = 51,

		/// <summary>
		/// AO Amenity Optimization
		/// </summary>
		[Description("AA")]
		AoAmenityOptimization = 52,

		/// <summary>
		/// AO AI Revenue Management
		/// </summary>
		[Description("AIRM")]
		AoAIRevenueManagement = 53,

		/// <summary>
		/// AO Rent Control
		/// </summary>
		[Description("RC")]
		AoRentControl = 54,

		/// <summary>
		/// Integration Marketplace
		/// </summary>
		[Description("IMP")]
		AccessIntegrationMarketplace = 39,

		/// <summary>
		/// VendorMarketplace
		/// </summary>
		[Description("ILMLM")]
		ManageILMLeadManagemementProductAccess = 40,

		/// <summary>
		/// VendorMarketplace
		/// </summary>
		[Description("ILMLA")]
		ManageILMLeasingAnalyticsProductAccess = 41,

		/// <summary>
		/// Portfolio Management
		/// </summary>
		[Description("RPM")]
		ManagePortfolioManagementProductAccess = 44,

		/// <summary>
		/// Manage Unified Platform Security Settings
		/// </summary>
		ManagePlatFormSecurity = 45,

		/// <summary>
		/// Manage Custom User fields settings
		/// </summary>
		ManageCustomFields = 46,

		/// <summary>
		/// Deposit Alternative
		/// </summary>
		[Description("DIQ")]
		ManageDepositAlternativeProductAccess = 47,

		/// <summary>
		/// Click Pay
		/// </summary>
		[Description("CPAY")]
		ManageClickPayProductAccess = 48,

		/// <summary>
		/// Manage Unified settings
		/// </summary>
		ManageUnifiedSettings = 49,

		/// <summary>
		/// Manage Settings Template
		/// </summary>
		ManageSettingsTemplates = 56,

		/// <summary>
		/// Renovation Manager
		/// </summary>
		[Description("RENO")]
		ManageRenovationManager = 55,

        /// <summary>
		/// Senior Lead Management
		/// </summary>
		[Description("SLM")]
        ManageSeniorLeadManagement = 50,

		/// <summary>
		/// Intelligent Building Trash
		/// </summary>
		[Description("SMS-T")]
		ManageIntelligentBuildingTrashProductAccess = 57,
		/// <summary>
		/// Intelligent Building energy
		/// </summary>
		[Description("SMS-E")]
		ManageIntelligentBuildingEnergyProductAccess = 58,
		/// <summary>
		/// Intelligent Building water
		/// </summary>
		[Description("SMS-W")]
		ManageIntelligentBuildingWaterProductAccess = 59,
		/// <summary>
		/// Hospitality As A Service
		/// </summary>
		[Description("HAAS")]
		ManageHomeSharingProductAccess = 60,
		/// <summary>
		/// HOTS
		/// </summary>
		[Description("HOTS")]
		ManageHandsOnTrainingSystemProductAccess = 63,		

		/// <summary>
		/// AO Market Analytic
		/// </summary>
		[Description("RMA")]
		AoMarketAnalytics = 66,
		/// <summary>
		/// HOTS
		/// </summary>
		[Description("LeaseLabs")]
		ManageLeaseLabsProductAccess = 68,


		/// <summary>
		/// Reporting
		/// </summary>
		[Description("RPT")]
		Reporting = 67,

	}
	public enum ProductProcVersion
	{
		/// <summary>
		/// Version1
		/// </summary>
		[Description("Ver1")]
		Version1 = 1,

		/// <summary>
		/// Version2
		/// </summary>
		[Description("Ver2")]
		Version2 = 2,

		/// <summary>
		/// Version
		/// </summary>
		[Description("Ver 3")]
		Version3 = 3
	}
}

	

//public static class EnumHelper
//{
//	public static string GetDescription(this T enumerationValue) where T : struct
//	{
//		var type = enumerationValue.GetType();
//		if (!type.IsEnum)
//		{
//			throw new ArgumentException($"{nameof(enumerationValue)} must be of Enum type", nameof(enumerationValue));
//		}
//		var memberInfo = type.GetMember(enumerationValue.ToString());
//		if (memberInfo.Length > 0)
//		{
//			var attrs = memberInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);

//			if (attrs.Length > 0)
//			{
//				return ((DescriptionAttribute)attrs[0]).Description;
//			}
//		}
//		return enumerationValue.ToString();
//	}
//}