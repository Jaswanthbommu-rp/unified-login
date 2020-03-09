using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum
{
	/// <summary>
	/// Product Enum Helper
	/// </summary>
	public static class ProductEnumHelper
	{
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
				case ProductEnum.UnifiedLogin: return "unified-platform";
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
				case ProductEnum.HelpCenter: return "help-center";
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
					ProductEnum.AoBenchmarking
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

				case ProductEnum.AoPerformanceAnalytics: return "YIELDSTAR";
				case ProductEnum.AoRevenueManagement: return "YIELDSTAR";
				case ProductEnum.AoBenchmarking: return "YIELDSTAR";
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

			}

			throw new Exception($"AO product with Id - {productCode} is not supported in green book.");
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
		UnifiedUI = 2,

		/// <summary>
		/// The Greenbook landing website
		/// </summary>
		[Description("UL")]
		UnifiedLogin = 3,

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
		/// Help Center
		/// </summary>
		[Description("HC")]
		HelpCenter = 49,

		/// <summary>
		/// RPDocumentManagement
		/// </summary>
		RPDocumentManagement = 20,

		/// <summary>
		/// OneSiteConversions
		/// </summary>
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
		ManageSettingsTemplates = 50,
    }
}
