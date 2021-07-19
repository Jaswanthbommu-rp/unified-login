using System;
using System.Collections.Generic;
using System.Linq;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.ProductIntegration.Factory;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.ProductIntegration.Model;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.ResidentPortal;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.Rum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Saml;
using ProductRole = RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.ProductRole;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.Accounting;
using Serilog;
using Serilog.Events;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Audit.Common;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.BlackBook;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Batch;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.EnterpriseRole;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Helper;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic
{

	/// <summary>
	/// Manage Enterprise Role User Product Batch 
	/// </summary>
	public class ManageEnterpriseRoleProductBatch
	{
		private readonly DefaultUserClaim _userClaim;
		private IProductRepository _productRepository;
		private readonly IntegrationTypeFactory _integrationTypeFactory;

		/// <summary>
		/// Default Constructor
		/// </summary>
		public ManageEnterpriseRoleProductBatch(DefaultUserClaim userClaim)
		{
			_userClaim = userClaim;

			var manageProduct = new ManageProduct(_userClaim);
			var manageUnifiedLogin = new ManageUnifiedLogin(_userClaim);
			var manageProductOneSite = new ManageProductOneSite(_userClaim);
			_productRepository = new ProductRepository();

			_integrationTypeFactory = new IntegrationTypeFactory(manageProduct, manageUnifiedLogin,
				manageProductOneSite, _productRepository, _userClaim);
		}
		public ManageEnterpriseRoleProductBatch(IProductRepository productRepository)
		{
			_productRepository = productRepository;			
		}

		public string GenerateEnterpriseRoleUserProductBatch (EnterpriseRoleBatch batch)
		{
			IList<ProductBatch> productListToCreate = new List<ProductBatch>();
			IManagePersona _managePersona = new ManagePersona();
			var editorPersona = _managePersona.GetPersona(batch.EditorUserPersonaId);
			var userPersona = _managePersona.GetPersona(batch.SubjectUserPersonaId);
			_userClaim.UserRealPageGuid = editorPersona.RealPageId;

			IPersonaRepository personaRepository = new PersonaRepository();
			IUserLoginRepository userLoginRepository = new UserLoginRepository();
			EnterpriseRoleProductRepository enterpriseRoleProductRepository = new EnterpriseRoleProductRepository();

			IList<Organization> organizationList = userLoginRepository.ListOrganizationByEnterpriseUserId(userPersona.RealPageId, null);
			var personaOrganization = organizationList.FirstOrDefault(i => i.PartyId == userPersona.OrganizationPartyId);
			var personaProductSettings = personaRepository.GetPersonaProductSettings(batch.SubjectUserPersonaId);
			var roleTemplateProducts = _productRepository.GetEnterpriseRoleProductsByOrganization(batch.EnterpriseRoleTemplateId, editorPersona.Organization.RealPageId);
			var roleTemplateProductRole = _productRepository.GetRoleTemplateProductRoleMapping(batch.EnterpriseRoleTemplateId, editorPersona.OrganizationPartyId);
			bool isExternalUser = personaOrganization.RelationshipType.Equals("User Type", StringComparison.OrdinalIgnoreCase) && personaOrganization.RoleNameFrom.Equals("External User", StringComparison.OrdinalIgnoreCase);

			foreach (var product in roleTemplateProducts)
			{
				try
				{
					ListResponse propertiesResponse = new ListResponse();
					ListResponse propertyGroupResponse = new ListResponse();
					ListResponse rolesResponse = new ListResponse();

					bool personaProductUsePrimaryProperty = false;
					bool usePrimaryProperties = false;
					bool productEnabledForPrimaryProperty = IsProductEnabledForUsePrimaryProperty(product);
					var integrationType = _integrationTypeFactory.GetIntegrationTypeForProductId(product);

					var productSetting = personaProductSettings.FirstOrDefault(item => item.Name.Equals("UsePrimaryProperties", StringComparison.OrdinalIgnoreCase) && item.ProductId == product);

					if (productSetting != null)
					{
						personaProductUsePrimaryProperty = productSetting.Value.Trim() == "1" ? true : false;
					}

					usePrimaryProperties = productEnabledForPrimaryProperty && personaProductUsePrimaryProperty;

					var productRoleData = roleTemplateProductRole?.Where(p => p.ProductId == product);

					var roleTemplateRoles = productRoleData?.Select(p => new
					{
						p.RoleTemplateProductRoleMappingId,
						p.ProductRoleId,
						p.ProductRoleName
					}).Distinct();


					//Roles
					//List<string> productRoles = new List<string>();
					IList<ProductRole> productRoles = new List<ProductRole>();
					foreach (var role in roleTemplateRoles)
					{
						if (role.RoleTemplateProductRoleMappingId != 0)
						{
							productRoles.Add(new ProductRole
							{
								ID = role.ProductRoleId.ToString(),
								Name = role.ProductRoleName,
							});
						}
					}

					rolesResponse = new ListResponse()
					{
						Records = productRoles.Cast<object>().ToList(),
						TotalRows = productRoles.Count,
						RowsPerPage = productRoles.Count,
						TotalPages = 1,
						ErrorReason = ""
					};

					//Get Properties
					if (product != (int)ProductEnum.DepositAlternative)
					{
						propertiesResponse = GetProductProperties(batch.EditorUserPersonaId, batch.SubjectUserPersonaId, product, _userClaim);
					}


					//Get product specific other info and create product batch
					if (product == (int)ProductEnum.FinancialSuite)
					{
						ManageProductOneSiteAccounting accounting = new ManageProductOneSiteAccounting(_userClaim);
						propertyGroupResponse = accounting.GetUserPropertyGroups(batch.EditorUserPersonaId, batch.SubjectUserPersonaId, null);
						ListResponse companiesResponse = accounting.GetUserCompanies(batch.EditorUserPersonaId, batch.SubjectUserPersonaId, null);
						productListToCreate.Add(BatchHelper.CreateFinancialSuiteProductBatchRecord(propertiesResponse, rolesResponse, product, companiesResponse, propertyGroupResponse, usePrimaryProperties));
					}
					else if (product == (int)ProductEnum.VendorServices)
					{
						ManageProductVendorServices vs = new ManageProductVendorServices(_userClaim);
						var notifications = vs.GetNotificationSettings(batch.EditorUserPersonaId, batch.SubjectUserPersonaId);
						propertyGroupResponse = vs.GetPropertyGroups(batch.EditorUserPersonaId, batch.SubjectUserPersonaId, null);
						productListToCreate.Add(BatchHelper.CreateVendorServiceProductBatchRecord(propertiesResponse, rolesResponse, propertyGroupResponse, notifications, product, usePrimaryProperties));
					}
					else if (product == (int)ProductEnum.ResidentPortal)
					{
						ManageProductResidentPortal manageProductResidentPortal = new ManageProductResidentPortal(_userClaim);
						//propertiesResponse = manageProductResidentPortal.ListProperties(batch.EditorUserPersonaId, batch.SubjectUserPersonaId, null);

						List<ILevel> LevelList = new List<ILevel>();
						foreach (var rRole in productRoles)
						{
							LevelList.Add(new Level { Id = rRole.ID, Name = rRole.Name, IsAssigned = rRole.IsAssigned });
						}

						Notifications notifications = manageProductResidentPortal.GetNotificationSettings(batch.EditorUserPersonaId, batch.SubjectUserPersonaId);
						List<IMessagingGroups> messagingGroups = manageProductResidentPortal.ListMessageGroups(batch.EditorUserPersonaId, batch.SubjectUserPersonaId);
						productListToCreate.Add(BatchHelper.CreateResidentPortalProductBatchRecord(propertiesResponse, LevelList, notifications, messagingGroups, product, usePrimaryProperties));
					}
					else if (product == (int)ProductEnum.OnSite)
					{
						ManageProductOnSite manageProductOnSite = new ManageProductOnSite(_userClaim);
						var regionResponse = manageProductOnSite.GetRegions(batch.EditorUserPersonaId, batch.SubjectUserPersonaId, null);
						productListToCreate.Add(BatchHelper.CreateOnSiteBatchRecord(propertiesResponse, rolesResponse, regionResponse, product, usePrimaryProperties));
					}
					else if (product == (int)ProductEnum.UtilityManagement)
					{
						ManageProductRum manageProductrum = new ManageProductRum(_userClaim);
						propertyGroupResponse = manageProductrum.GetPropertyGroups(batch.EditorUserPersonaId, batch.SubjectUserPersonaId, null);
						var regionResponse = manageProductrum.GetRegions(batch.EditorUserPersonaId, batch.SubjectUserPersonaId, null);
						
						productListToCreate.Add(BatchHelper.CreateRumProductBatchRecord(propertiesResponse, propertyGroupResponse, regionResponse, rolesResponse, usePrimaryProperties));
					}
					else if (product == (int)ProductEnum.ClickPay)
					{
						//Don't know how it works with enterprise role, since it need more information along with the role
						break;
						//var productLogic = ManageProductFactory.GetProductLogic(product, batch.EditorUserPersonaId, batch.SubjectUserPersonaId, _userClaim);
						//var productUser = productLogic.GetProductUser();
						//var organizationRoles = productUser.OrganizationRoles;

						//productListToCreate.Add(CreateProductBatchRecordForClickPay(organizationRoles, usePrimaryProperties));
					}
					else if (product == (int)ProductEnum.DepositAlternative)
					{
						var productLogic = ManageProductFactory.GetProductLogic(product, batch.EditorUserPersonaId, batch.SubjectUserPersonaId, _userClaim);
						var productUser = productLogic.GetProductUser();
						productUser.RoleList = productRoles.Select(p => p.ID).ToList();
						productListToCreate.Add(BatchHelper.CreateProductBatchRecordForDepositIQ(productUser, usePrimaryProperties));
					}
					else if (product == (int)ProductEnum.IntegrationMarketplace)
					{

						var existingRoleId = Convert.ToInt32(productRoles.Select(p => p.ID).FirstOrDefault());
						productListToCreate.Add(BatchHelper.CreateIntegrationMarketplaceBatchRecord(existingRoleId, product, usePrimaryProperties));
					}
					else if (product == (int)ProductEnum.LeadManagement)
					{
						var productLogic = ManageProductFactory.GetProductLogic(product, batch.EditorUserPersonaId, batch.SubjectUserPersonaId, _userClaim);
						var productUser = productLogic.GetProductUser();

						productListToCreate.Add(BatchHelper.CreateILMProductBatchRecord(ProductEnum.LeadManagement, productUser.Properties,
							productRoles.Select(p => p.ID).ToList(), null, usePrimaryProperties));//no groups for LM
					}
					else if (product == (int)ProductEnum.LeadAnalytics)
					{
						var productLogic = ManageProductFactory.GetProductLogic(product, batch.EditorUserPersonaId, batch.SubjectUserPersonaId, _userClaim);
						var productUser = productLogic.GetProductUser();

						productListToCreate.Add(BatchHelper.CreateILMProductBatchRecord(ProductEnum.LeadAnalytics, productUser.Properties,
							productRoles.Select(p => p.ID).ToList(), productUser.PropertyGroups, usePrimaryProperties));
					}
					else if (product == (int)ProductEnum.RPDocumentManagement)
					{
						//Don't know how it works with enterprise role, since it need more information along with the role
						break;
					}
					else if (product == (int)ProductEnum.PortfolioManagement)
					{
						var productLogic = ManageProductFactory.GetProductLogic(product, batch.EditorUserPersonaId, batch.SubjectUserPersonaId, _userClaim);
						var productUser = productLogic.GetProductUser();
						var propertyRoles = productUser.PropertyRoleList;
						var roles = productRoles.Select(p => p.ID).ToList();

						productListToCreate.Add(BatchHelper.CreateProductBatchRecordForPortfolioManagement(propertyRoles, roles, usePrimaryProperties));
					}				
					else if (ProductEnumHelper.GetAoProductList().Contains((ProductEnum)product))
					{
						var batchRecords = BatchHelper.CreateAoBatchRecords(_userClaim, batch.EditorUserPersonaId, batch.SubjectUserPersonaId,isExternalUser );
						foreach (var productBatch in batchRecords)
						{
							productListToCreate.Add(productBatch);
						}							
					}
					else 
					{
						var type = _integrationTypeFactory.GetIntegrationTypeForProductId(product);
						
						var productBatchRecord = BatchHelper.CreateProductBatchRecord(propertiesResponse, rolesResponse, product, usePrimaryProperties, type);
						productListToCreate.Add(productBatchRecord);
					}
				}
				catch (Exception ex)
				{
					return "Error";
					string message = $"Exception during enterprise role product updates to user - {product}";
					Log.Write(LogEventLevel.Error, ex, message);
				}
				
			}
			if (productListToCreate?.Count > 0){
				enterpriseRoleProductRepository.SaveProductBatch(batch.EditorUserPersonaId, batch.SubjectUserPersonaId, editorPersona.RealPageId, productListToCreate);
			}
			return "";
		}

		private ListResponse GetProductProperties(long editorPersonaId, long userPersonaId, int productId, DefaultUserClaim userClaim)
		{
			ManageProductPanel manageProductPanel = new ManageProductPanel(userClaim);
			var productResult = manageProductPanel.GetProductProperties(editorPersonaId, userPersonaId, productId, null);
			return productResult;
		}

		private bool IsProductEnabledForUsePrimaryProperty(int productId)
		{
			ProductInternalSetting productInternalSetting = new ProductInternalSetting();
			IProductInternalSettingRepository productInternalSettingRepository = new ProductInternalSettingRepository();
			IList<ProductInternalSetting> productInternalSettingList = productInternalSettingRepository.GetProductInternalSettings(productId);
			productInternalSetting = productInternalSettingList.FirstOrDefault(item => item.Name.Equals("UsePrimaryProperties", StringComparison.OrdinalIgnoreCase));

			if (productInternalSetting != null)
			{
				return productInternalSetting.Value.Trim() == "1" ? true : false;
			}
			return false;
		}
	}
}
