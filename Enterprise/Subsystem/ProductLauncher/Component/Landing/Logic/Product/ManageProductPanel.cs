using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using RP.Enterprise.Foundation.Audit.Core.Component;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.ProductIntegration.Factory;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.ProductIntegration.Model;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Base;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Constants;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.Accounting;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.ClientPortal;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.IntegrationMarketplace;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.Lead2Lease;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.MarketingCenter;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.OneSite;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.Ops;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.ProspectContactCenter;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.RentersInsurance;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.ResearchApplication;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.ResidentPortal;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.Rum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.SelfProvisioningPortal;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.UnifiedAmenities;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.VendorServices;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Saml;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product
{
	public class ManageProductPanel : IManageProductPanel
	{
		#region Private Variables		
		private DefaultUserClaim _userClaims;

		#endregion

		#region Constructors
		/// <summary>
		/// Manages Product panel constructor
		/// </summary>
		public ManageProductPanel(DefaultUserClaim userClaims)
		{
			_userClaims = userClaims;
		}

		public ManageProductOneSite IManageProductOneSite_manageProductOneSite { get; private set; }
		#endregion

		#region public methods
		public ListResponse GetProductProperties(long editorPersonaId, long userPersonaId, int productId, RequestParameter datafilter, bool assignedOnly = false, string userLoginName = "")
		{
			ListResponse result = new ListResponse();
			IProduct product;
			string productName = Enum.GetName(typeof(ProductEnum), productId);
			string productcode = ProductEnumHelper.StringValueOf((ProductEnum)productId);
			switch (productId)
			{
				case (int)ProductEnum.OneSite:
					IManageProductOneSite _manageProductOneSite = new ManageProductOneSite(_userClaims);
					result = _manageProductOneSite.GetOneSitePropertyList(editorPersonaId, userPersonaId, assignedOnly, datafilter);
					break;
				case (int)ProductEnum.MarketingCenter:
					ManageProductMarketingCenter mg = new ManageProductMarketingCenter(_userClaims);
					result = mg.GetProperties(editorPersonaId, userPersonaId, datafilter);
					break;
				case (int)ProductEnum.FinancialSuite:
					IManageProductOneSiteAccounting mangeProductOneSiteAccounting = new ManageProductOneSiteAccounting(_userClaims);
					result = mangeProductOneSiteAccounting.GetUserPropertiesNew(editorPersonaId, userPersonaId, datafilter);
					break;
				case (int)ProductEnum.OpsBuyer:
					IManageProductOps manageProductOps = new ManageProductOps(_userClaims);
					result = manageProductOps.GetCompanyAssets(editorPersonaId, userPersonaId, assignedOnly, datafilter);
					break;
				case (int)ProductEnum.VendorServices:
					IManageProductVendorServices manageProductVendorServices = new ManageProductVendorServices(_userClaims);
					result = manageProductVendorServices.GetProperties(editorPersonaId, userPersonaId, datafilter);
					break;
				case (int)ProductEnum.ClientPortal:
					IManageProductClientPortal _manageProductClientPortal = new ManageProductClientPortal(_userClaims);
					result = _manageProductClientPortal.GetProperties(editorPersonaId, userPersonaId, datafilter);
					break;
				case (int)ProductEnum.ProspectContactCenter:
					IManageProductProspectContact manageProductProspectContact = new ManageProductProspectContact(_userClaims.UserRealPageGuid);
					result = manageProductProspectContact.GetProperties(editorPersonaId, userPersonaId, datafilter);
					break;
				case (int)ProductEnum.Lead2Lease:
					IManageProductLead2Lease manageProductLead2Lease = new ManageProductLead2Lease(_userClaims);
					result = manageProductLead2Lease.GetProperties(editorPersonaId, userPersonaId, datafilter);
					break;
				case (int)ProductEnum.ResidentPortal:
					ManageProductResidentPortal manageProductResidentPortal = new ManageProductResidentPortal(_userClaims);
					result = manageProductResidentPortal.ListProperties(editorPersonaId, userPersonaId, datafilter);
					break;
				case (int)ProductEnum.OnSite:
					var manageProductOnSite = new ManageProductOnSite(_userClaims.UserRealPageGuid);
					result = manageProductOnSite.GetProperties(editorPersonaId, userPersonaId, datafilter);
					break;
				case (int)ProductEnum.Insurance:
					ManageProductRentersInsurance manageProductRentersInsurance = new ManageProductRentersInsurance(_userClaims);
					result = manageProductRentersInsurance.ListProperties(editorPersonaId, userPersonaId, datafilter);
					break;
				case (int)ProductEnum.UtilityManagement:
					var manageProductRum = new ManageProductRum(_userClaims);
					result = manageProductRum.GetProperties(editorPersonaId, userPersonaId, datafilter);
					break;
				case (int)ProductEnum.UnifiedAmenities:
					IManageUnifiedAmenities manageUnifiedAmenities = new ManageUnifiedAmenities(_userClaims);
					result = manageUnifiedAmenities.GetProperties(editorPersonaId, userPersonaId, assignedOnly, datafilter);
					break;
				case (int)ProductEnum.AoBusinessIntelligence:
				case (int)ProductEnum.AoInvestmentAnalytics:
				case (int)ProductEnum.AoPerformanceAnalytics:
				case (int)ProductEnum.AoRevenueManagement:
				case (int)ProductEnum.AoBenchmarking:
				case (int)ProductEnum.AoLeaseRentOption:
				case (int)ProductEnum.AoAmenityOptimization:
				case (int)ProductEnum.AoAIRevenueManagement:
				case (int)ProductEnum.AoRentControl:
					var manageProductAo = new ManageProductAssetOptimization(_userClaims);
					result = manageProductAo.GetProductProperties(editorPersonaId, userPersonaId, productcode, datafilter, userLoginName);
					break;
				case (int)ProductEnum.LeadManagement:
					var productLMLogic = ManageProductFactory.GetProductLogic(ProductEnum.LeadManagement, editorPersonaId, userPersonaId, _userClaims);
					result = productLMLogic.GetProductProperties(datafilter);
					break;
				case (int)ProductEnum.LeadAnalytics:
					var productLALogic = ManageProductFactory.GetProductLogic(ProductEnum.LeadAnalytics, editorPersonaId, userPersonaId, _userClaims);
					result = productLALogic.GetProductProperties(datafilter);
					break;
				//case (int)ProductEnum.RPDocumentManagement:

				//	break;
				case (int)ProductEnum.PortfolioManagement:
					var productPMLogic = ManageProductFactory.GetProductLogic(ProductEnum.PortfolioManagement, editorPersonaId, userPersonaId, _userClaims);
					result = productPMLogic.GetProductProperties(datafilter);
					break;
				case (int)ProductEnum.DepositAlternative:
					var productDALogic = ManageProductFactory.GetProductLogic(ProductEnum.DepositAlternative, editorPersonaId, userPersonaId, _userClaims);
					result = productDALogic.GetProductProperties(datafilter);
					break;
				case (int)ProductEnum.UnifiedPlatform:
					IManageUnifiedLogin manageUnifiedLogin = new ManageUnifiedLogin(_userClaims);
					result = manageUnifiedLogin.GetProperties(editorPersonaId, userPersonaId, false, datafilter);
					break;
				default:
					break;
			}
			return result;
		}

		public ListResponse GetProductRoles(long editorPersonaId, long userPersonaId, long partyId, int productId, RequestParameter datafilter, bool assignedOnly = false, string userLoginName = "")
		{
			ListResponse result = new ListResponse();
			string productName = Enum.GetName(typeof(ProductEnum), productId);
			string productcode = ProductEnumHelper.StringValueOf((ProductEnum)productId);
			switch (productId)
			{
				case (int)ProductEnum.OneSite:
					IManageProductOneSite _manageProductOneSite = new ManageProductOneSite(_userClaims);
					if (userPersonaId > 0)
					{
						result = _manageProductOneSite.GetOneSiteRoleList(editorPersonaId, userPersonaId, assignedOnly, datafilter);
					}
					else
					{
						result = _manageProductOneSite.GetOneSiteRoleListAll(editorPersonaId, datafilter);
					}
					break;
				case (int)ProductEnum.MarketingCenter:
					ManageProductMarketingCenter mg = new ManageProductMarketingCenter(_userClaims);
					result = mg.GetRoles(editorPersonaId, userPersonaId, datafilter);
					break;
				case (int)ProductEnum.FinancialSuite:
					IManageProductOneSiteAccounting mangeProductOneSiteAccounting = new ManageProductOneSiteAccounting(_userClaims);
					result = mangeProductOneSiteAccounting.GetUserRoles(editorPersonaId, userPersonaId, datafilter);
					break;
				case (int)ProductEnum.OpsBuyer:
					IManageProductOps manageProductOps = new ManageProductOps(_userClaims);
					result = manageProductOps.GetRoles(editorPersonaId, userPersonaId, "", datafilter);
					break;
				case (int)ProductEnum.VendorServices:
					IManageProductVendorServices manageProductVendorServices = new ManageProductVendorServices(_userClaims);
					result = manageProductVendorServices.GetRoles(editorPersonaId, userPersonaId, AccessType.Property, datafilter);
					break;
				case (int)ProductEnum.ClientPortal:
					IManageProductClientPortal _manageProductClientPortal = new ManageProductClientPortal(_userClaims);
					result = _manageProductClientPortal.GetRoles(editorPersonaId, userPersonaId, datafilter);
					break;
				case (int)ProductEnum.Lead2Lease:
					IManageProductLead2Lease manageProductLead2Lease = new ManageProductLead2Lease(_userClaims);
					result = manageProductLead2Lease.GetRoles(editorPersonaId, userPersonaId, datafilter);
					break;
				case (int)ProductEnum.ResidentPortal:
					ManageProductResidentPortal manageProductResidentPortal = new ManageProductResidentPortal(_userClaims);
					result = manageProductResidentPortal.ListLevelsResponse(editorPersonaId, userPersonaId);
					break;
				case (int)ProductEnum.OnSite:
					var manageProductOnSite = new ManageProductOnSite(_userClaims.UserRealPageGuid);
					result = manageProductOnSite.GetRoles(editorPersonaId, userPersonaId, datafilter);
					break;
				case (int)ProductEnum.Insurance:
					ManageProductRentersInsurance manageProductRentersInsurance = new ManageProductRentersInsurance(_userClaims);
					result = manageProductRentersInsurance.ListRolesResponse(editorPersonaId, userPersonaId);
					break;
				case (int)ProductEnum.UtilityManagement:
					var manageProductRum = new ManageProductRum(_userClaims);
					result = manageProductRum.GetUMGlobalRoles(editorPersonaId, userPersonaId, datafilter);								
					break;
				case (int)ProductEnum.UnifiedAmenities:
					IManageUnifiedAmenities manageUnifiedAmenities = new ManageUnifiedAmenities(_userClaims);
					result = manageUnifiedAmenities.GetRoles(editorPersonaId, userPersonaId, partyId);
					break;
				case (int)ProductEnum.AoBusinessIntelligence:
				case (int)ProductEnum.AoInvestmentAnalytics:
				case (int)ProductEnum.AoPerformanceAnalytics:
				case (int)ProductEnum.AoRevenueManagement:
				case (int)ProductEnum.AoBenchmarking:
				case (int)ProductEnum.AoLeaseRentOption:
				case (int)ProductEnum.AoAmenityOptimization:
				case (int)ProductEnum.AoAIRevenueManagement:
				case (int)ProductEnum.AoRentControl:
					var manageProductAo = new ManageProductAssetOptimization(_userClaims);
					result = manageProductAo.GetProductRoles(editorPersonaId, userPersonaId, productcode, datafilter, userLoginName);
					break;
				case (int)ProductEnum.LeadManagement:
					var productLMLogic = ManageProductFactory.GetProductLogic(ProductEnum.LeadManagement, editorPersonaId, userPersonaId, _userClaims);
					result = productLMLogic.GetProductRoles(datafilter);
					break;
				case (int)ProductEnum.LeadAnalytics:
					var productLALogic = ManageProductFactory.GetProductLogic(ProductEnum.LeadAnalytics, editorPersonaId, userPersonaId, _userClaims);
					result = productLALogic.GetProductRoles(datafilter);
					break;
				case (int)ProductEnum.IntegrationMarketplace:
					var manageProductIntegartionMarketplace = new ManageProductIntegrationMarketplace(_userClaims);
					result = manageProductIntegartionMarketplace.GetRoles(editorPersonaId, userPersonaId, partyId);
					break;
				//case (int)ProductEnum.RPDocumentManagement:

				//	break;
				case (int)ProductEnum.PortfolioManagement:
					var productPMLogic = ManageProductFactory.GetProductLogic(ProductEnum.PortfolioManagement, editorPersonaId, userPersonaId, _userClaims);
					result = productPMLogic.GetProductRoles(datafilter);
					break;
				case (int)ProductEnum.DepositAlternative:
					var productDALogic = ManageProductFactory.GetProductLogic(ProductEnum.DepositAlternative, editorPersonaId, userPersonaId, _userClaims);
					result = productDALogic.GetProductRoles(datafilter);
					break;
				case (int)ProductEnum.UnifiedPlatform:
					IManageUnifiedLogin manageUnifiedLogin = new ManageUnifiedLogin(_userClaims);
					result = manageUnifiedLogin.GetUserRolesWithRights(editorPersonaId, userPersonaId, partyId);
					break;
				default:
					break;
			}

			return result;
		}

		public ListResponse GetProductRightsForRole(long editorPersonaId, int roleId, long partyId, int productId, RequestParameter datafilter, bool assignedToRoleOnly = false)
		{
			ListResponse result = new ListResponse();

			switch (productId)
			{
				case (int)ProductEnum.OneSite:
					IManageProductOneSite manageProductOneSite = new ManageProductOneSite(_userClaims);
					result = manageProductOneSite.GetOneSiteRights(editorPersonaId, datafilter, roleId, assignedToRoleOnly);
					break;
				case (int)ProductEnum.UnifiedPlatform:
					IManageUnifiedLogin manageUnifiedLogin = new ManageUnifiedLogin(_userClaims);
					result = manageUnifiedLogin.GetRightsByRole(editorPersonaId, partyId, roleId);
					break;
				case (int)ProductEnum.UnifiedAmenities:
					IManageUnifiedAmenities manageUnifiedAmenities = new ManageUnifiedAmenities(_userClaims);
					result = manageUnifiedAmenities.GetRightsByRole(editorPersonaId, partyId, roleId);
					break;
				default:
					break;
			}
			return result;
		}

		public ListResponse GetProductPropertyGroups(long editorPersonaId, long userPersonaId, int productId, RequestParameter datafilter, bool assignedOnly = false, string userLoginName = "")
		{
			ListResponse result = new ListResponse();
			IProduct product;
			string productName = Enum.GetName(typeof(ProductEnum), productId);
			string productcode = ProductEnumHelper.StringValueOf((ProductEnum)productId);
			switch (productId)
			{

				case (int)ProductEnum.OnSite:
					var manageProductOnSite = new ManageProductOnSite(_userClaims.UserRealPageGuid);
					result = manageProductOnSite.GetRegions(editorPersonaId, userPersonaId, datafilter);
					break;
				case (int)ProductEnum.ResidentPortal:
					List<IMessagingGroups> messageGroupsList = new List<IMessagingGroups>();
					ManageProductResidentPortal manageProductResidentPortal = new ManageProductResidentPortal(_userClaims);
					messageGroupsList = manageProductResidentPortal.ListMessageGroups(editorPersonaId, userPersonaId);
					if (messageGroupsList?.Count > 0){
						result.Records = messageGroupsList.Cast<object>().ToList();
						result.TotalRows = messageGroupsList.Count;
						result.RowsPerPage = messageGroupsList.Count;
						result.TotalPages = 1;
						result.ErrorReason = string.Empty;
						result.Additional = null;
					}
					break;
				case (int)ProductEnum.AoBusinessIntelligence:
				case (int)ProductEnum.AoInvestmentAnalytics:
				case (int)ProductEnum.AoPerformanceAnalytics:
				case (int)ProductEnum.AoRevenueManagement:
				case (int)ProductEnum.AoBenchmarking:
				case (int)ProductEnum.AoLeaseRentOption:
				case (int)ProductEnum.AoAmenityOptimization:
				case (int)ProductEnum.AoAIRevenueManagement:
				case (int)ProductEnum.AoRentControl:
					var manageProductAo = new ManageProductAssetOptimization(_userClaims);
					result = manageProductAo.GetProductPropertyGroups(editorPersonaId, userPersonaId, productcode, userLoginName);
					break;
				case (int)ProductEnum.UtilityManagement:
					var manageProductRum = new ManageProductRum(_userClaims);
					result = manageProductRum.GetPropertyGroups(editorPersonaId, userPersonaId, datafilter);
					break;
				default:
					break;
			}
			return result;
		}

		public ListResponse GetProductGroupProperties(long editorPersonaId, long userPersonaId, int productId,int propertyGroupId, RequestParameter datafilter)
		{
			ListResponse result = new ListResponse();
			IProduct product;
			string productName = Enum.GetName(typeof(ProductEnum), productId);
			string productcode = ProductEnumHelper.StringValueOf((ProductEnum)productId);
			switch (productId)
			{
				case (int)ProductEnum.AoBusinessIntelligence:
				case (int)ProductEnum.AoInvestmentAnalytics:
				case (int)ProductEnum.AoPerformanceAnalytics:
				case (int)ProductEnum.AoRevenueManagement:
				case (int)ProductEnum.AoBenchmarking:
				case (int)ProductEnum.AoLeaseRentOption:
				case (int)ProductEnum.AoAmenityOptimization:
				case (int)ProductEnum.AoAIRevenueManagement:
				case (int)ProductEnum.AoRentControl:
					var manageProductAo = new ManageProductAssetOptimization(_userClaims);
					result = manageProductAo.GetGroupProperties(editorPersonaId, userPersonaId, propertyGroupId);
					break;
				
				default:
					break;
			}
			return result;
		}

		public ListResponse GetProductRights(long editorPersonaId, long userPersonaId, long partyId, int productId, RequestParameter datafilter)
		{
			ListResponse result = new ListResponse();
			string productName = Enum.GetName(typeof(ProductEnum), productId);
			string productcode = ProductEnumHelper.StringValueOf((ProductEnum)productId);
			switch (productId)
			{
				case (int)ProductEnum.UtilityManagement:
					var manageProductRum = new ManageProductRum(_userClaims);
					result = manageProductRum.GetRoles(editorPersonaId, userPersonaId, datafilter);								
					break;

				default:
					break;
			}
			return result;
		}
		#endregion
	}
}
