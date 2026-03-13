using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum
{
    /// <summary>
    /// Product Enum Helper
    /// </summary>
    public static class ProductEnumHelper
    {
        [Obsolete("Replaced by GetProductCodeByProductId - Do not use")]
        public static string StringValueOf(ProductEnum value)
        {
            if(value != ProductEnum.UnifiedPlatform
                && value != ProductEnum.OneSite)
            {
                throw new Exception($"This function is obsolute, use {nameof(GetProductCodeByProductId)} instead");
            }

            FieldInfo fi = value.GetType().GetField(value.ToString());
            DescriptionAttribute[] attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);
            if (attributes.Length > 0)
            {
                return attributes[0].Description;
            }

            return value.ToString();
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
                    ProductEnum.AoBIX,
                    ProductEnum.AoLuminaAscent
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

        public static string GetAoProductDescription(ProductEnum productCode)
        {
            switch (productCode)
            {
                case ProductEnum.AoBusinessIntelligence: return "Business Intelligence";
                case ProductEnum.AoBIX: return "BIX";
                case ProductEnum.AoInvestmentAnalytics: return "Investment Analytics";
                case ProductEnum.AoAxiometrics: return "Axiometrics";               
                case ProductEnum.AoRevenueManagement: return "YieldStar";
                case ProductEnum.AoPerformanceAnalytics: return "Benchmarking";
                case ProductEnum.AoLeaseRentOption: return "LRO";
                case ProductEnum.AoAmenityOptimization: return "Amenity Optimization";
                case ProductEnum.AoAIRevenueManagement: return "AI Revenue Management";
                case ProductEnum.AoRentControl: return "Rent Control";
                case ProductEnum.AoMarketAnalytics: return "Market Analytics";
                case ProductEnum.AoLuminaAscent: return "Lumina Ascent";
                default : return "Asset Optimization";
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
                case ProductEnum.AoBIX: return "BI";

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
                case ProductEnum.AoLuminaAscent: return "YIELDSTAR";
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
                    case ProductEnum.AoBIX: return "BIX";

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
                    case ProductEnum.AoLuminaAscent: return "LA";
                }
            }

            return null;
        }

        public static ProductEnum GetAoProductEnum(string productCode)
        {
            switch (productCode)
            {
                case "BI": return ProductEnum.AoBusinessIntelligence;
                case "BIX": return ProductEnum.AoBIX;
                
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
                case "LA": return ProductEnum.AoLuminaAscent;

            }

            throw new Exception($"AO product with Id - {productCode} is not supported in green book.");
        }

        /// <summary>
        /// GetProductEnumByProductCode
        /// </summary>
        /// <param name="productCode">Product Code</param>
        /// <param name="products"></param>
        /// <returns>ProductEnum</returns>
        public static int GetProductIdByProductCode(string productCode, IList<GbProductMap> products)
        {
            var lookupValue = products.FirstOrDefault(a => a.BooksProductCode?.Equals(productCode, StringComparison.OrdinalIgnoreCase) == true);

            if(lookupValue == null)
            {
                //If Code reach here that means product code did not match with any Product Enum Description value. So raise an exception
                throw new Exception("Invalid product code " + productCode);
            }

            return lookupValue.ProductId;
        }

        /// <summary>
        /// Do NOT override the BooksProductCode with the UDMSourceCode in this call, it will break other areas!
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="products"></param>
        /// <returns></returns>
        public static string GetProductCodeByProductId(int productId, IList<GbProductMap> products) =>
            products.FirstOrDefault(a => a.ProductId == productId)?.BooksProductCode;

        public static string GetUDMSourceCodeByProductId(int productId, IList<GbProductMap> products) =>
            products.FirstOrDefault(a => a.ProductId == productId)?.UDMSourceCode;

        public static string GetBooksSourceCodeByProductId(int productId, IList<GbProductMap> products) =>
            products.FirstOrDefault(a => a.ProductId == productId)?.BooksProductCode;

    }

    /// <summary>
    /// Used to identify products by id.
    /// </summary>
    //[Obsolete("Use an int instead")]
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
        /// SalesForce ClientPortal
        /// </summary>
        [Description("OMS-P")]
        AdminSupportPortal = 89,

        /// <summary>
        /// SalesForce AdminSupportPortalStandard
        /// </summary>
        [Description("RPISF")]
        AdminSupportPortalStandard = 104,

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
        /// Product Updates Dashboard
        /// </summary>
        [Description("PUDASH")]
        ProductUpdatesDashboard = 98,

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
        /// AO BIX
        /// </summary>
        [Description("BIX")]
        AoBIX = 95,

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
        /// PME Dashboard
        /// </summary>
        [Description("PME")]
        PMEDasboard = 62,

        /// <summary>
        /// Esupply
        /// </summary>
        [Description("ESUPPLY")]
        ESupply = 96,
        
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
        Reporting = 67,
        /// <summary>
        /// SelfGuidedTour
        /// </summary>
        [Description("6247")]
        SelfGuidedTour = 65,
        /// <summary>
        /// Lead Scoring
        /// </summary>
        [Description("LST")]
        LeadScoring = 69,
        /// <summary>
        /// Smart Waste Commercial
        /// </summary>
        [Description("SMS-TC")]
        SmartWasteCommercial = 70,
        /// <summary>
        /// Facilities
        /// </summary>
        [Description("OS")]
        Facilities = 75,
        /// <summary>
        /// L&R Conversion Portal
        /// </summary>
        [Description("OSCE")]
        LRConversionPortal = 85,

        /// <summary>
        /// Sustainability Services
        /// </summary>
        [Description("SMS-S")]
        SustainabilityServices = 84,

        /// <summary>
        /// Web2Print Social
        /// </summary>
        [Description("W2PS")]
        Web2PrintSocial = 87,

        /// <summary>
        /// G5+LL Marketing Solutions
        /// </summary>
        [Description("G5")]
        G5LLMarketing = 86,

        /// <summary>
        /// DataHub
        /// </summary>
        [Description("DHB")]
        DataHub = 90,

        /// <summary>
        /// Knock CRM
        /// </summary>
        [Description("KNCK")]
        KnockCRM = 91,

        /// <summary>
        /// Real Connect
        /// </summary>
        [Description("RCLMS")]
        RealConnect = 94,

        /// <summary>
        /// Managed Services
        /// </summary>
        [Description("MS")]
        ManagedServices = 93,
        
        /// <summary>
        /// Esupply
        /// </summary>
        [Description("TD")]
        TrustDashboard = 97,

        /// <summary>
        /// Ao Lumina Ascent
        /// </summary>
        [Description("LA")]
        AoLuminaAscent = 103
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
        /// SalesForce AdminSupportPortal
        /// </summary>
        [Description("OMS")]
        ManageAdminSupportPortalProductAccess = 89,

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
        /// BIX
        /// </summary>
        [Description("BIX")]
        AoBIX = 95,

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
        /// AO Lumina Ascent
        /// </summary>
        [Description("LA")]
        AoLuminaAscent = 103,

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

        /// <summary>
        /// SelfGuidedTour
        /// </summary>
        [Description("6247")]
        ManageSGTourProductAccess = 65,

        /// <summary>
        /// Hospitality As A Service
        /// </summary>
        [Description("LST")]
        ManageLeadScoringProductAccess = 69,

        /// <summary>
        /// Smart  Commercial Waste
        /// </summary>
        [Description("SMS-TC")]
        ManageSmartWasteCommercialProductAccess = 70,

        /// <summary>
        /// Real Connect
        /// </summary>
        [Description("RCLMS")]
        ManageRealConnectProductAccess = 94
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