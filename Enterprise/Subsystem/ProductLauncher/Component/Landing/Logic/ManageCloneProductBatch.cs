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

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic
{
	/// <summary>
	/// Manage User Product Batch for Cloning
	/// </summary>
	public class ManageCloneProductBatch
	{
		/// <summary>
		/// Default Constructor
		/// </summary>
		public ManageCloneProductBatch()
		{

		}

		/// <summary>
		/// Gets Product Batch
		/// </summary> 
		public IList<ProductBatch> GetUserProductBatchData(long personaId, DefaultUserClaim userClaim, List<PersonaProductUserDetails> userProducts, long createUserPersonaId, bool externalUser = false, bool usePropertyInstanceUnifiedAmenities = false)
		{
			IList<ProductBatch> productListToCreate = new List<ProductBatch>();

			ListResponse propertiesResponse = new ListResponse();
			ListResponse propertyGroupResponse = new ListResponse();
			ListResponse rolesResponse = new ListResponse();
			foreach (PersonaProductUserDetails product in userProducts)
			{
				try
				{
					if (product.ProductId == (int)ProductEnum.OneSite)
					{
						ManageProductOneSite mg = new ManageProductOneSite(userClaim);
						propertiesResponse = mg.GetOneSitePropertyList(createUserPersonaId, personaId, true, null);
						rolesResponse = mg.GetOneSiteRoleList(createUserPersonaId, personaId, true, null);
						productListToCreate.Add(CreateProductBatchRecord(propertiesResponse, rolesResponse, product.ProductId));
					}
					else if (product.ProductId == (int)ProductEnum.FinancialSuite)
					{
						ManageProductOneSiteAccounting accounting = new ManageProductOneSiteAccounting(userClaim);
						propertiesResponse = accounting.GetUserProperties(createUserPersonaId, personaId, null);
						propertyGroupResponse = accounting.GetUserPropertyGroups(createUserPersonaId, personaId, null);
						rolesResponse = accounting.GetUserRoles(createUserPersonaId, personaId, null);
						ListResponse companiesResponse = accounting.GetUserCompanies(createUserPersonaId, personaId, null);
						productListToCreate.Add(CreateFinancialSuiteProductBatchRecord(propertiesResponse, rolesResponse, product.ProductId, companiesResponse, propertyGroupResponse));
					}
					else if (product.ProductId == (int)ProductEnum.MarketingCenter)
					{
						ManageProductMarketingCenter marketing = new ManageProductMarketingCenter(userClaim);
						propertiesResponse = marketing.GetProperties(createUserPersonaId, personaId, null);
						rolesResponse = marketing.GetRoles(createUserPersonaId, personaId, null);
                        productListToCreate.Add(CreateMarketingCenterProductBatchRecord(propertiesResponse, rolesResponse, product.ProductId));
                    }
					else if (product.ProductId == (int)ProductEnum.OpsBuyer)
					{
						ManageProductOps opsbuyer = new ManageProductOps(userClaim);
						propertiesResponse = opsbuyer.GetCompanyAssets(createUserPersonaId, personaId, false, null);
						rolesResponse = opsbuyer.GetRoles(createUserPersonaId, personaId, "", null);
						productListToCreate.Add(CreateProductBatchRecord(propertiesResponse, rolesResponse, product.ProductId));
					}
					else if (product.ProductId == (int)ProductEnum.VendorServices)
					{
						ManageProductVendorServices vs = new ManageProductVendorServices(userClaim);
						propertiesResponse = vs.GetProperties(createUserPersonaId, personaId, null);                        
						rolesResponse = vs.GetRoles(createUserPersonaId, personaId, AccessType.Property, null);
						var notifications = vs.GetNotificationSettings(createUserPersonaId, personaId);
						propertyGroupResponse = vs.GetPropertyGroups(createUserPersonaId, personaId, null);
						productListToCreate.Add(CreateVendorServiceProductBatchRecord(propertiesResponse, rolesResponse, propertyGroupResponse, notifications, product.ProductId));
					}
					else if (product.ProductId == (int)ProductEnum.ClientPortal)
					{
						ManageProductClientPortal cp = new ManageProductClientPortal(userClaim);
						propertiesResponse = cp.GetProperties(createUserPersonaId, personaId, null);
						rolesResponse = cp.GetRoles(createUserPersonaId, personaId, null);
						productListToCreate.Add(CreateProductBatchRecord(propertiesResponse, rolesResponse, product.ProductId));
					}
					else if (product.ProductId == (int)ProductEnum.ProspectContactCenter)
					{
						ManageProductProspectContact prospContact = new ManageProductProspectContact(userClaim);
						propertiesResponse = prospContact.GetProperties(createUserPersonaId, personaId, null);
						productListToCreate.Add(CreateProductBatchRecord(propertiesResponse, rolesResponse, product.ProductId));
					}
					else if (product.ProductId == (int)ProductEnum.Lead2Lease)
					{
						ManageProductLead2Lease l2l = new ManageProductLead2Lease(userClaim);
						propertiesResponse = l2l.GetProperties(createUserPersonaId, personaId, null);
						rolesResponse = l2l.GetRoles(createUserPersonaId, personaId, null);
						productListToCreate.Add(CreateProductBatchRecord(propertiesResponse, rolesResponse, product.ProductId));
					}
					else if (product.ProductId == (int)ProductEnum.ResidentPortal)
					{
						ManageProductResidentPortal manageProductResidentPortal = new ManageProductResidentPortal(userClaim);
						propertiesResponse = manageProductResidentPortal.ListProperties(createUserPersonaId, personaId, null);
						List<ILevel> LevelList = manageProductResidentPortal.ListLevels(createUserPersonaId, personaId);
						Notifications notifications = manageProductResidentPortal.GetNotificationSettings(createUserPersonaId, personaId);
						List<IMessagingGroups> messagingGroups = manageProductResidentPortal.ListMessageGroups(createUserPersonaId, personaId);
						productListToCreate.Add(CreateResidentPortalProductBatchRecord(propertiesResponse, LevelList, notifications, messagingGroups, product.ProductId));
					}
					else if (product.ProductId == (int)ProductEnum.Insurance)
					{
						ManageProductRentersInsurance manageProductRentersInsurance = new ManageProductRentersInsurance(userClaim);
						propertiesResponse = manageProductRentersInsurance.ListProperties(createUserPersonaId, personaId, null);
						IList<ProductRole> productRoleList = manageProductRentersInsurance.ListRoles(createUserPersonaId, personaId);
						productListToCreate.Add(CreateRentersInsuranceProductBatchRecord(propertiesResponse, productRoleList, product.ProductId));
					}
					else if (product.ProductId == (int)ProductEnum.OnSite)
					{
						ManageProductOnSite manageProductOnSite = new ManageProductOnSite(userClaim);
						propertiesResponse = manageProductOnSite.GetProperties(createUserPersonaId, personaId, null);
						rolesResponse = manageProductOnSite.GetRoles(createUserPersonaId, personaId, null);
						var regionResponse = manageProductOnSite.GetRegions(createUserPersonaId, personaId, null);
						productListToCreate.Add(CreateOnSiteBatchRecord(propertiesResponse, rolesResponse, regionResponse, product.ProductId));
					}
					else if (product.ProductId == (int)ProductEnum.UtilityManagement)
					{
						ManageProductRum manageProductrum = new ManageProductRum(userClaim);
						propertiesResponse = manageProductrum.GetProperties(createUserPersonaId, personaId, null);
						propertyGroupResponse = manageProductrum.GetPropertyGroups(createUserPersonaId, personaId, null);
						var regionResponse = manageProductrum.GetRegions(createUserPersonaId, personaId, null);
						rolesResponse = manageProductrum.GetRoles(createUserPersonaId, personaId, null);
						productListToCreate.Add(CreateRumProductBatchRecord(propertiesResponse, propertyGroupResponse, regionResponse, rolesResponse));
					}
					else if (product.ProductId == (int)ProductEnum.SelfProvisioningPortal)
					{
						ManageProductSelfProvisioningPortal manageProductSelfProvisioningPortal = new ManageProductSelfProvisioningPortal(userClaim);
						productListToCreate.Add(CreateSelfProvisioningPortalProductBatchRecord(product.ProductId));
					}
					else if (product.ProductId == (int)ProductEnum.UnifiedAmenities)
					{
						ManageUnifiedAmenities manageUnifiedAmenities = new ManageUnifiedAmenities(userClaim);
                        if (!usePropertyInstanceUnifiedAmenities)
                        {
                            propertiesResponse = manageUnifiedAmenities.GetProperties(createUserPersonaId, personaId, true, null);
                        }
                        else
                        {
							ManageUnifiedLogin manageUnifiedLogin = new ManageUnifiedLogin(userClaim);
                            propertiesResponse = manageUnifiedLogin.GetUPFMProperties(createUserPersonaId, personaId, true, ProductEnum.UnifiedAmenities, null);
                        }

                        rolesResponse = manageUnifiedAmenities.GetRoles(createUserPersonaId, personaId, product.OrganizationPartyId);

                        productListToCreate.Add(CreateProductBatchRecord(propertiesResponse, rolesResponse, product.ProductId));
					}
					else if (product.ProductId == (int)ProductEnum.LeadManagement)
					{
						var productLogic = ManageProductFactory.GetProductLogic(ProductEnum.LeadManagement, createUserPersonaId, personaId, userClaim);
						var productUser = productLogic.GetProductUser();

						productListToCreate.Add(CreateILMProductBatchRecord(ProductEnum.LeadManagement, productUser.Properties,
							productUser.Roles.ConvertAll<string>(i => i.ToString()), null));//no groups for LM
					}
					else if (product.ProductId == (int)ProductEnum.LeadAnalytics)
					{
						var productLogic = ManageProductFactory.GetProductLogic(ProductEnum.LeadAnalytics, createUserPersonaId, personaId, userClaim);
						var productUser = productLogic.GetProductUser();

						productListToCreate.Add(CreateILMProductBatchRecord(ProductEnum.LeadAnalytics, productUser.Properties,
							productUser.Roles.ConvertAll<string>(i => i.ToString()), productUser.PropertyGroups));
					}
					else if (product.ProductId == (int)ProductEnum.RPDocumentManagement)
					{
						productListToCreate.Add(CreateDocManagementBatchRecords(userClaim, createUserPersonaId, personaId));
					}
					else if (product.ProductId == (int)ProductEnum.PortfolioManagement)
					{
						var productLogic = ManageProductFactory.GetProductLogic(ProductEnum.PortfolioManagement, createUserPersonaId, personaId, userClaim);
						var productUser = productLogic.GetProductUser();
						var propertyRoles = productUser.PropertyRoleList;
						var roles = productUser.RoleList.ConvertAll<string>(i => i.ToString());

						productListToCreate.Add(CreateProductBatchRecordForPortfolioManagement(propertyRoles, roles));
					}
					else if (product.ProductId == (int)ProductEnum.IntegrationMarketplace)
					{
						var existingRoleId = 0;

						ISamlRepository samlRepository = new SamlRepository();
						IList<SamlAttributes> productAttributes = samlRepository.GetProductSamlDetails(personaId, (int)ProductEnum.IntegrationMarketplace);

						if (productAttributes.Any(a => a.Name.ToUpper() == SamlAttributeEnum.RoleCode.ToString().ToUpperInvariant()))
						{
							var imLogic = new ManageProductIntegrationMarketplace(userClaim);
							List<IntegrationMarketplaceRole> allImRoles = imLogic.GetIntegrationMarketplaceRoles();

							var existingRoleCode = (from a in productAttributes where a.Name.ToUpper() == SamlAttributeEnum.RoleCode.ToString().ToUpperInvariant() select a.Value).FirstOrDefault();
							existingRoleId = allImRoles.FirstOrDefault(x =>
								x.ShortName.Equals(existingRoleCode, StringComparison.OrdinalIgnoreCase)).Id;
						}

						productListToCreate.Add(CreateIntegrationMarketplaceBatchRecord(existingRoleId, product.ProductId));
					}
					else if (product.ProductId == (int)ProductEnum.DepositAlternative)
					{
						var productLogic = ManageProductFactory.GetProductLogic(ProductEnum.DepositAlternative, createUserPersonaId, personaId, userClaim);
						var productUser = productLogic.GetProductUser();

						productListToCreate.Add(CreateProductBatchRecordForDepositIQ(productUser));
					}
					else if (product.ProductId == (int)ProductEnum.ClickPay)
					{
						var productLogic = ManageProductFactory.GetProductLogic(ProductEnum.ClickPay, createUserPersonaId, personaId, userClaim);
						var productUser = productLogic.GetProductUser();
						var organizationRoles = productUser.OrganizationRoles;

						productListToCreate.Add(CreateProductBatchRecordForClickPay(organizationRoles));
					}
					else if (product.ProductId == (int)ProductEnum.RenovationManager)
					{
						var productLogic = ManageProductFactory.GetProductLogic(ProductEnum.RenovationManager, createUserPersonaId, personaId, userClaim);
						var productUser = productLogic.GetProductUser();						

						productListToCreate.Add(CreateProductBatchRecordForRenovationManager(productUser));
					}
					//else if (product.ProductId == (int)ProductEnum.IntelligentBuilding)
					//{
					//	ManageIntelligentBuilding ib = new ManageIntelligentBuilding(userClaim);
					//	propertiesResponse = ib.GetUPFMProperties(createUserPersonaId, personaId, false, ProductEnum.IntelligentBuilding,null);
					//	rolesResponse = ib.GetRoles(createUserPersonaId, personaId, userClaim.OrganizationPartyId);
					//	productListToCreate.Add(CreateProductBatchRecord(propertiesResponse, rolesResponse, product.ProductId));
					//}
					else if (product.ProductId == (int)ProductEnum.IntelligentBuildingTrash ||
							 product.ProductId == (int)ProductEnum.IntelligentBuildingEnergy ||
							 product.ProductId == (int)ProductEnum.IntelligentBuildingWater ||
							 product.ProductId == (int)ProductEnum.HospitalityService ||
							 product.ProductId == (int)ProductEnum.HandsOnTrainingSystem)
					{
						ManageUPFMProductsIntegration upfmProductIntegration = new ManageUPFMProductsIntegration(product.ProductId, userClaim);
						var upfmProduct = ProductEnumHelper.GetUPFMProductEnum(product.ProductId);
						propertiesResponse = upfmProductIntegration.GetUPFMProperties(createUserPersonaId, personaId, false, upfmProduct, null);
						rolesResponse = upfmProductIntegration.GetRoles(createUserPersonaId, personaId, userClaim.OrganizationPartyId, upfmProduct);
						productListToCreate.Add(CreateProductBatchRecord(propertiesResponse, rolesResponse, product.ProductId));
					}
				}
				catch (Exception ex)
				{
					string message = $"Exception during clone user for product - {product?.ProductName}";

					Log.Write(LogEventLevel.Error, ex, message);
				}
			}

			// Check if any AO products exists & then add them in batch
			var aoProductList = userProducts.Where(y => ProductEnumHelper.GetAoProductList().Contains((ProductEnum)y.ProductId)).ToList();
			if (aoProductList.Any())
			{
				var batches = CreateAoBatchRecords(userClaim, createUserPersonaId, personaId, externalUser);
				foreach (var productBatch in batches)
				{
					// add only if userProducts has productId else product is modified after clone
					if (userProducts.Any(x => x.ProductId == productBatch.ProductId))
					{
						productListToCreate.Add(productBatch);
					}
				}
			}

			return productListToCreate;
		}

		private ProductBatch CreateProductBatchRecordForClickPay(List<OrganizationRole> userOrganizationRole)
		{
			var pb = new ProductBatch()
			{
				ProductId = (int)ProductEnum.ClickPay,
				StatusTypeId = 5,
				RetryCount = 0,
				InputJson = new RolePropertyList()
				{
					OrganizationRoleList = userOrganizationRole
				}
			};

			return pb;
		}

		private ProductBatch CreateProductBatchRecordForDepositIQ(IntegrationProductUser productUser)
		{
			ProductBatch productBatch = new ProductBatch()
			{
				ProductId = (int)ProductEnum.DepositAlternative,
				StatusTypeId = 5,
				RetryCount = 0,
				InputJson = new RolePropertyList()
				{
					RoleList = productUser.Roles,
					CanReceiveMonthlyReport = productUser.CanReceiveMonthlyReport,
					PropertyGroupList = productUser.PropertyGroups,
					PropertyList = productUser.Properties
				}
			};

			return productBatch;
		}

		private ProductBatch CreateProductBatchRecordForRenovationManager(IntegrationProductUser productUser)
		{
			ProductBatch productBatch = new ProductBatch()
			{
				ProductId = (int)ProductEnum.RenovationManager,
				StatusTypeId = 5,
				RetryCount = 0,
				InputJson = new RolePropertyList()
				{
					RoleList = productUser.Roles,				
					PropertyList = productUser.Properties
				}
			};

			return productBatch;
		}

		private ProductBatch CreateIntegrationMarketplaceBatchRecord(int existingRoleId, int productProductId)
		{
			var roleList = new List<string> { existingRoleId.ToString() };
			ProductBatch productBatch = new ProductBatch()
			{
				ProductId = productProductId,
				StatusTypeId = 5,
				RetryCount = 0,
				InputJson = new RolePropertyList() { RoleList = roleList }
			};

			return productBatch;
		}

		private ProductBatch CreateILMProductBatchRecord(ProductEnum ilmProduct, List<string> productUserProperties,
			List<string> productUserRoles, List<string> productUserGroups)
		{
			var pb = new ProductBatch()
			{
				ProductId = (int)ilmProduct,
				StatusTypeId = 5,
				RetryCount = 0,
				InputJson = new RolePropertyList()
				{
					PropertyList = productUserProperties,
					RoleList = productUserRoles,
					PropertyGroupList = productUserGroups
				}
			};

			return pb;
		}

		private ProductBatch CreateProductBatchRecordForPortfolioManagement(List<PAMRolePropertyList> rolePropertyList, List<string> roleList)
		{
			var pb = new ProductBatch()
			{
				ProductId = (int)ProductEnum.PortfolioManagement,
				StatusTypeId = 5,
				RetryCount = 0,
				InputJson = new RolePropertyList() { RolePropertiesList = rolePropertyList, RoleList = roleList }
			};

			return pb;
		}

		private ProductBatch CreateOnSiteBatchRecord(ListResponse propertiesResponse, ListResponse rolesResponse, ListResponse regionResponse, int productId)
		{
			List<string> propertyList = new List<string>();
			List<string> roleList = new List<string>();
			List<string> regionList = new List<string>();

			bool allProperties = false;
			bool allRegions = false;

			IEnumerable<object> propertiesCollection = propertiesResponse.Records;
			if (propertiesResponse.Additional != null)
			{
				allProperties = CheckForAllProperties(propertiesResponse.Additional);
			}

			if (allProperties)
			{
				propertyList.Add("-1");
			}
			else
			{
				foreach (object item in propertiesCollection)
				{
					if (((OnSiteProperty)item).IsAssigned)
					{
						propertyList.Add(((OnSiteProperty)item).GetPropertyId.ToString());
					}
				}
			}

			RolePropertyList inputJson = new RolePropertyList { PropertyList = propertyList };

			/**/
			IEnumerable<object> regionCollection = regionResponse.Records;
			if (regionResponse.Additional != null)
			{
				allRegions = CheckForAllRegions(regionResponse.Additional);
			}

			if (allRegions)
			{
				regionList.Add("-1");
			}
			else
			{
				foreach (object item in regionCollection)
				{
					if (((OnSiteRegion)item).IsAssigned)
					{
						regionList.Add(((OnSiteRegion)item).GetRegionId.ToString());
					}
				}
			}

			inputJson.RegionList = regionList;

			/**/

			foreach (object item in rolesResponse.Records)
			{
				var isAssigned = ((OnSiteRole)item).IsAssigned;
				bool result = isAssigned != null && isAssigned.Value;

				if (result)
				{
					roleList.Add(((OnSiteRole)item).Level.ToString());
				}
			}

			inputJson.RoleList = roleList;

			ProductBatch productBatch = new ProductBatch()
			{
				ProductId = productId,
				StatusTypeId = 5,
				RetryCount = 0,
				InputJson = inputJson
			};

			return productBatch;
		}

		private bool CheckForAllRegions(object additionalInfo)
		{
			bool allProperties = false;
			if (additionalInfo.GetType().Name.ToUpper() != "STRING")
			{
				Dictionary<string, bool> additionalDataCollection = (Dictionary<string, bool>)additionalInfo;
				foreach (KeyValuePair<string, bool> pair in additionalDataCollection)
				{
					if (pair.Key == "allProperties")
					{
						allProperties = pair.Value;
					}
				}
			}

			return allProperties;
		}

		private ProductBatch CreateProductBatchRecord(ListResponse propertiesResponse, ListResponse rolesResponse, int productID)
		{
			List<string> PropertyList = new List<string>();
			List<string> RoleList = new List<string>();
			bool allProperties = false;
			IEnumerable<object> propertiesCollection;
			if (propertiesResponse.Records != null)
			{
				propertiesCollection = (IEnumerable<object>)propertiesResponse.Records;
			}
			else
			{
				propertiesCollection = new List<object>();
			}

			if (propertiesResponse.Additional != null)
			{
				allProperties = CheckForAllProperties(propertiesResponse.Additional);
			}

			if (productID != (int)ProductEnum.ProspectContactCenter)
			{
				if (rolesResponse.Records != null)
				{
					IEnumerable<object> roleCollection = (IEnumerable<object>)rolesResponse.Records;
					foreach (object item in roleCollection)
					{
						if (((ProductRole)item).IsAssigned)
						{
							RoleList.Add(((ProductRole)item).ID);
						}
					}
				}
			}

			if (allProperties)
			{
			
				if (productID == (int)ProductEnum.ClientPortal ||
					productID == (int)ProductEnum.IntelligentBuildingTrash ||
					productID == (int)ProductEnum.IntelligentBuildingEnergy ||
					productID == (int)ProductEnum.IntelligentBuildingWater ||
					productID == (int)ProductEnum.HospitalityService)
				{
					PropertyList.Add("-1");
				}
				else if (productID == (int)ProductEnum.OneSite || 
                         productID == (int)ProductEnum.FinancialSuite || 
                         productID == (int)ProductEnum.ProspectContactCenter ||
                         productID == (int)ProductEnum.MarketingCenter)
				{
					PropertyList.Add("ALL");
				}
			}
			else
			{
				foreach (object item in propertiesCollection)
				{
					if (productID == (int)ProductEnum.OpsBuyer)
					{
						if (((Component.SharedObjects.Product.Ops.AssetGroup)item).IsAssigned)
						{
							PropertyList.Add(((Component.SharedObjects.Product.Ops.AssetGroup)item).ID);
						}
					}					
					else if (((ProductProperty)item).IsAssigned.Value)
					{
						if (productID == (int)ProductEnum.IntelligentBuildingTrash ||
							productID == (int)ProductEnum.IntelligentBuildingEnergy ||
							productID == (int)ProductEnum.IntelligentBuildingWater ||
							productID == (int)ProductEnum.HospitalityService)
						{
							PropertyList.Add(((ProductProperty)item).Alias);
						}
						else
						{
							PropertyList.Add(((ProductProperty)item).ID);
						}							
					}
				}

			}

			ProductBatch pb = new ProductBatch()
			{
				ProductId = productID,
				StatusTypeId = 5,
				RetryCount = 0,
				InputJson = new RolePropertyList() { PropertyList = PropertyList, RoleList = RoleList }
			};

			return pb;
		}

        private ProductBatch CreateMarketingCenterProductBatchRecord(ListResponse propertiesResponse, ListResponse rolesResponse, int productID)
        {
            List<string> PropertyList = new List<string>();
            List<string> RoleList = new List<string>();
            bool isAssignNewPropertyByDefault = false;
            IEnumerable<object> propertiesCollection;
            if (propertiesResponse.Records != null)
            {
                propertiesCollection = (IEnumerable<object>)propertiesResponse.Records;
            }
            else
            {
                propertiesCollection = new List<object>();
            }

            if (propertiesResponse.Additional != null)
            {
                isAssignNewPropertyByDefault = CheckForIsAssignedNewPropertyFlag(propertiesResponse.Additional);
            }

            if (productID != (int)ProductEnum.ProspectContactCenter)
            {
                if (rolesResponse.Records != null)
                {
                    IEnumerable<object> roleCollection = (IEnumerable<object>)rolesResponse.Records;
                    foreach (object item in roleCollection)
                    {
                        if (((ProductRole)item).IsAssigned)
                        {
                            RoleList.Add(((ProductRole)item).ID);
                        }
                    }
                }
            }

            foreach (object item in propertiesCollection)
            {
                if (productID == (int)ProductEnum.OpsBuyer)
                {
                    if (((Component.SharedObjects.Product.Ops.AssetGroup)item).IsAssigned)
                    {
                        PropertyList.Add(((Component.SharedObjects.Product.Ops.AssetGroup)item).ID);
                    }
                }
                else if (((ProductProperty)item).IsAssigned.Value)
                {
                    PropertyList.Add(((ProductProperty)item).ID);
                }
            }

            ProductBatch pb = new ProductBatch()
            {
                ProductId = productID,
                StatusTypeId = 5,
                RetryCount = 0,
                InputJson = new RolePropertyList() { PropertyList = PropertyList, RoleList = RoleList, IsAssignedNewPropertyByDefault= isAssignNewPropertyByDefault}
            };

            return pb;
        }

		private ProductBatch CreateFinancialSuiteProductBatchRecord(ListResponse propertiesResponse, ListResponse rolesResponse, int productID, ListResponse companiesResponse, ListResponse propertyGroupResponse)
		{
			List<string> PropertyList = new List<string>();
			List<string> PropertyGroupList = new List<string>();
			List<string> RoleList = new List<string>();
			List<string> companiesList = new List<string>();
			bool hasAccessToSiteSpendManagementOnly = false;
			bool isAccountingAdmin = false;
			bool hasAccessToAllCurrentFutureProperties = false;
			IEnumerable<object> propertiesCollection;
			IEnumerable<object> propertyGroupsCollection;

			if (propertiesResponse.Records != null)
			{
				propertiesCollection = (IEnumerable<object>)propertiesResponse.Records;
			}
			else
			{
				propertiesCollection = new List<object>();
			}

			if (propertyGroupResponse.Records != null)
			{
				propertyGroupsCollection = (IEnumerable<object>)propertyGroupResponse.Records;
			}
			else
			{
				propertyGroupsCollection = new List<object>();
			}

			if (companiesResponse.Additional != null)
			{
				AccountingUser accountingUser = (AccountingUser)companiesResponse.Additional;
				hasAccessToSiteSpendManagementOnly = accountingUser.HasAccessToSiteSpendManagementOnly;
				isAccountingAdmin = accountingUser.IsAccountingAdmin;
				hasAccessToAllCurrentFutureProperties = accountingUser.HasAccessToAllCurrentFutureProperties;
			}

			if (companiesResponse?.Records != null)
			{
				IEnumerable<object> companiesCollection = (IEnumerable<object>)companiesResponse.Records;
				foreach (object item in companiesCollection)
				{
					if (!string.IsNullOrEmpty(((ACCompany)item).Id))
					{
						companiesList.Add(((ACCompany)item).Id);
					}
				}
			}

			if (rolesResponse.Records != null)
			{
				IEnumerable<object> roleCollection = (IEnumerable<object>)rolesResponse.Records;
				foreach (object item in roleCollection)
				{
					if (((ProductRole)item).IsAssigned)
					{
						RoleList.Add(((ProductRole)item).ID);
					}
				}
			}

			foreach (object item in propertiesCollection)
			{
				if (((ProductProperty)item).IsAssigned.Value)
				{
					PropertyList.Add(((ProductProperty)item).ID);
				}
			}

			foreach(object item in propertyGroupsCollection)
			{
				if (((ProductProperty)item).IsAssigned.Value)
				{
					PropertyList.Add(((ProductProperty)item).ID);
				}
			}

			ProductBatch pb = new ProductBatch()
			{
				ProductId = productID,
				StatusTypeId = 5,
				RetryCount = 0,
				InputJson = new RolePropertyList()
				{
					PropertyList = PropertyList,
					RoleList = RoleList,
					HasAccessToSiteSpendManagementOnly = hasAccessToSiteSpendManagementOnly,
					IsAccountingAdmin = isAccountingAdmin,
					HasAccessToAllCurrentFutureProperties = hasAccessToAllCurrentFutureProperties,
					CompaniesList = companiesList
				}
			};

			return pb;
		}

		private bool CheckForIsAssignedNewPropertyFlag(object additionalInfo)
        {
            bool isAssignNewPropertyByDefault = false;
            if (additionalInfo.GetType().Name.ToUpper() != "STRING")
            {
                Dictionary<string, bool> additionalDataCollection = (Dictionary<string, bool>)additionalInfo;
                foreach (KeyValuePair<string, bool> pair in additionalDataCollection)
                {
                    if (pair.Key.Equals("IsAssignedNewPropertyByDefault", StringComparison.OrdinalIgnoreCase))
                    {
                        isAssignNewPropertyByDefault = pair.Value;
                    }
                }
            }
            return isAssignNewPropertyByDefault;
        }

        private ProductBatch CreateVendorServiceProductBatchRecord(ListResponse propertiesResponse, ListResponse rolesResponse, ListResponse propertyGroup, Component.SharedObjects.Product.VendorServices.Notification notification, int productID)
		{
			List<string> PropertyList = new List<string>();
			List<string> RoleList = new List<string>();
			List<Component.SharedObjects.Product.VendorServices.PropertyGroup> propertyGroupList = new List<Component.SharedObjects.Product.VendorServices.PropertyGroup>();
			bool allProperties = false;

			IEnumerable<object> roleCollection = (IEnumerable<object>)rolesResponse.Records;
			IEnumerable<object> propertiesCollection = (IEnumerable<object>)propertiesResponse.Records;

			if (propertiesResponse.Additional != null)
			{
				allProperties = CheckForAllProperties(propertiesResponse.Additional);
			}

            // Below logic is applied when a user is being cloned from a user that has access to all properties. 
            if (propertiesResponse != null)
            {
                var unselectedPropertiesCount = propertiesCollection.Where(p => ((ProductProperty)p).IsAssigned == false).Count();
                if (unselectedPropertiesCount == propertiesCollection.Count())
                    allProperties = true;
            }
                        

			foreach (object item in roleCollection)
			{
				if (((ProductRole)item).IsAssigned)
				{
					RoleList.Add(((ProductRole)item).ID);
				}
			}

			if (propertyGroup.TotalRows > 0)
			{
				foreach (object item in propertyGroup.Records)
				{
					if (((VendorServicesPropertyGroup)item).IsAssigned)
					{
						int? value = ((VendorServicesPropertyGroup)item).PropertyGroupId;
						var propertyGroupData = new Component.SharedObjects.Product.VendorServices.PropertyGroup
						{
							Id = value,
							IsAssigned = true,
							Type = (Component.SharedObjects.Product.VendorServices.AccessTypeEnum)Enum.Parse(typeof(Component.SharedObjects.Product.VendorServices.AccessTypeEnum), ((VendorServicesPropertyGroup)item).AccessLevel)
						};

						propertyGroupList.Add(propertyGroupData);
					}
				}
			}

			if (allProperties)
			{
				PropertyList.Add("-1");
			}
			else
			{
				foreach (object item in propertiesCollection)
				{
					if (productID == (int)ProductEnum.OpsBuyer)
					{
						if (((Component.SharedObjects.Product.Ops.AssetGroup)item).IsAssigned)
						{
							PropertyList.Add(((Component.SharedObjects.Product.Ops.AssetGroup)item).ID);
						}
					}
					else if (((ProductProperty)item).IsAssigned.Value)
					{
						PropertyList.Add(((ProductProperty)item).ID);
					}
				}
			}

			var inputJson = new RolePropertyList();
			inputJson.PropertyList = PropertyList;
			inputJson.RoleList = RoleList;            
			if (propertyGroupList.Count > 0)
			{
				inputJson.PropertyGroup = propertyGroupList;
			}
            
			inputJson.IsInsuranceExpired = notification.IsInsuranceExpired;
			inputJson.IsVendorRecommendationChanges = notification.IsVendorRecommendationChanges;
			inputJson.IsVendorNotLinkedToAnyProperty = notification.IsVendorNotLinkedToAnyProperty;

			ProductBatch pb = new ProductBatch()
			{
				ProductId = productID,
				StatusTypeId = 5,
				RetryCount = 0,
				InputJson = inputJson
			};

			return pb;
		}

		private IList<ProductBatch> CreateAoBatchRecords(DefaultUserClaim userClaim, long editorPersonaId, long newUserPersonaId, bool externalUser = false)
		{
			var productBatchList = new List<ProductBatch>();
			IList<AoUserCompanyPropertyRoleDetail> aoBIUserCompanyPropertyRoleDetails = new List<AoUserCompanyPropertyRoleDetail>();
			var manageProductAssetOptimization = new ManageProductAssetOptimization(userClaim);
			//below code block will add external user bi product to clone user batch.
			if (externalUser)
			{
				ISamlRepository samlRepository = new SamlRepository();
				string aoBIUserName = string.Empty;
				IList<SamlAttributes> productAttributes = samlRepository.GetProductSamlDetails(newUserPersonaId, (int)ProductEnum.AoBusinessIntelligence);
				if (productAttributes.Any(a => a.Name.Equals("ProductUserName", StringComparison.OrdinalIgnoreCase)))
				{
					aoBIUserName = (from a in productAttributes where a.Name.Equals("ProductUserName", StringComparison.OrdinalIgnoreCase) select a.Value).FirstOrDefault();
				}
				if (aoBIUserName != null)
				{
					aoBIUserCompanyPropertyRoleDetails = manageProductAssetOptimization.CopyRegularUser(editorPersonaId, newUserPersonaId, aoBIUserName);
				}				
			}

			var aoUserCompanyPropertyRoleDetails = manageProductAssetOptimization.CopyRegularUser(editorPersonaId, newUserPersonaId);

			foreach (var aoBIUserCompanyPropertyRoleDetail in aoBIUserCompanyPropertyRoleDetails)
			{
				aoUserCompanyPropertyRoleDetails.Add(aoBIUserCompanyPropertyRoleDetail);
			}

			foreach (var aoUserCompanyPropertyRoleDetail in aoUserCompanyPropertyRoleDetails)
			{
				if (aoUserCompanyPropertyRoleDetail.SelectedPortfolioValues == null)
				{
					aoUserCompanyPropertyRoleDetail.SelectedPortfolioValues = new List<int>();
				}

				if (aoUserCompanyPropertyRoleDetail.PropertyGroups == null)
				{
					aoUserCompanyPropertyRoleDetail.PropertyGroups = new List<int>();
				}

				var productBatch = new ProductBatch()
				{
					ProductId = (int)ProductEnumHelper.GetAoProductEnum(aoUserCompanyPropertyRoleDetail.ProductName),
					StatusTypeId = 5,
					RetryCount = 0,
					InputJson =
						new RolePropertyList()
						{
							PropertyList = (from i in aoUserCompanyPropertyRoleDetail.SelectedPortfolioValues select i.ToString()).ToList(),
							RoleList = (from i in aoUserCompanyPropertyRoleDetail.SelectedRoleValues select i).ToList(),
							CompanyId = aoUserCompanyPropertyRoleDetail.CompanyId,
							PropertyGroupList = (from i in aoUserCompanyPropertyRoleDetail.PropertyGroups select i.ToString()).ToList()
						}
				};

				productBatchList.Add(productBatch);
			}

			return productBatchList;
		}

		private ProductBatch CreateRumProductBatchRecord(ListResponse propertiesResponse, ListResponse groupResponse, ListResponse regionResponse, ListResponse rolesResponse)
		{
			List<string> propertyList = new List<string>();
			List<string> propertyGroupList = new List<string>();
			List<string> regionsList = new List<string>();
			List<string> roleList = new List<string>();

			IEnumerable<object> roleCollection = (IEnumerable<object>)rolesResponse.Records;
			foreach (object item in roleCollection)
			{
				if (((RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.Rum.Role)item).IsAssigned)
				{
					roleList.Add(((RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.Rum.Role)item).Name);
				}
			}

			IEnumerable<object> regionCollection = (IEnumerable<object>)regionResponse.Records;
			foreach (object item in regionCollection)
			{
				if (((RumPropertyGroup)item).IsAssigned)
				{
					regionsList.Add(((RumPropertyGroup)item).Id.ToString());
				}
			}

			IEnumerable<object> groupCollection = (IEnumerable<object>)groupResponse.Records;
			foreach (object item in groupCollection)
			{
				if (((RumPropertyGroup)item).IsAssigned)
				{
					propertyGroupList.Add(((RumPropertyGroup)item).Id.ToString());
				}
			}

			IEnumerable<object> propertiesCollection = (IEnumerable<object>)propertiesResponse.Records;
			foreach (object item in propertiesCollection)
			{
				if (((RumPropertyGroup)item).IsAssigned)
				{
					propertyList.Add(((RumPropertyGroup)item).Id.ToString());
				}
			}

            // Below logic is applied when a user is being cloned from a user that has access to all properties. 
            if (propertiesCollection != null && propertyGroupList.Count == 0)
            {
                var unselectedPropertiesCount = propertiesCollection.Where(p => ((RumPropertyGroup)p).IsAssigned == false).Count();
                if (unselectedPropertiesCount == propertiesCollection.Count())
                    propertyList.Add("All");
            }

            ProductBatch pb = new ProductBatch()
			{
				ProductId = (int)ProductEnum.UtilityManagement,
				StatusTypeId = 5,
				RetryCount = 0,
				InputJson = new RolePropertyList() { PropertyList = propertyList, PropertyGroupList = propertyGroupList, RegionList = regionsList, RoleList = roleList }
			};

			return pb;
		}

		/// <summary>
		/// Create ResidentPortal ProductBatch Record
		/// </summary>
		/// <param name="propertiesResponse">list of Communities</param>
		/// <param name="rolesResponse"> list of Roles (Level of Access)</param>
		/// <param name="notifications">Notification Settings</param>
		/// <param name="messagingGroups">Message Groups</param>
		/// <param name="productID">Product Id</param>
		/// <returns>ProductBatch object</returns>
		private ProductBatch CreateResidentPortalProductBatchRecord(ListResponse propertiesResponse, List<ILevel> rolesResponse, Notifications notifications, List<IMessagingGroups> messagingGroups, int productID)
		{
			List<string> PropertyList = new List<string>();
			List<string> RoleList = new List<string>();
			List<string> MessageGroups = new List<string>();
			bool allProperties = false;

			IEnumerable<object> propertiesCollection = (IEnumerable<object>)propertiesResponse.Records;
			if (propertiesResponse.Additional != null)
			{
				allProperties = CheckForAllProperties(propertiesResponse.Additional);
			}

			if (allProperties)
			{
				if (productID == (int)ProductEnum.ResidentPortal)
				{
					PropertyList.Add("ALL");
				}
			}
			else
			{
				foreach (object item in propertiesCollection)
				{
					if (((ProductProperty)item).IsAssigned.Value)
					{
						PropertyList.Add(((ProductProperty)item).ID);
					}
				}
			}

			RolePropertyList inputJson = new RolePropertyList();
			inputJson.PropertyList = PropertyList;
			//RoleList - Level of Access
			string accessLevel = rolesResponse.Find(item => item.IsAssigned == true).Id.ToUpper();
			RoleList.Add(accessLevel);

			inputJson.RoleList = RoleList;
			//Notification Settings
			inputJson.Notifications = notifications;
			//Message Group
			foreach (MessagingGroups messageGroup in messagingGroups)
			{
				if (messageGroup.IsAssigned)
				{
					MessageGroups.Add(messageGroup.Id);
				}
			}

			inputJson.MessageGroups = MessageGroups;

			ProductBatch productBatch = new ProductBatch()
			{
				ProductId = productID,
				StatusTypeId = 5,
				RetryCount = 0,
				InputJson = inputJson
			};

			return productBatch;
		}


		/// <summary>
		/// Create Renters Insurance ProductBatch Record
		/// </summary>
		/// <param name="propertiesResponse">list of Properties</param>
		/// <param name="rolesResponse"> list of Roles</param>
		/// <param name="productID">Product Id</param>
		/// <returns>ProductBatch object</returns>
		private ProductBatch CreateRentersInsuranceProductBatchRecord(ListResponse propertiesResponse, IList<ProductRole> rolesResponse, int productID)
		{
			List<string> PropertyList = new List<string>();
			List<string> RoleList = new List<string>();
			bool allProperties = false;

			IEnumerable<object> propertiesCollection = (IEnumerable<object>)propertiesResponse.Records;
			if (propertiesResponse.Additional != null)
			{
				allProperties = CheckForAllProperties(propertiesResponse.Additional);
			}

			if (allProperties)
			{
				if (productID == (int)ProductEnum.Insurance)
				{
					PropertyList.Add("ALL");
				}
			}
			else
			{
				foreach (object item in propertiesCollection)
				{
					if (((ProductProperty)item).IsAssigned.Value)
					{
						PropertyList.Add(((ProductProperty)item).ID);
					}
				}
			}

			RolePropertyList inputJson = new RolePropertyList();
			inputJson.PropertyList = PropertyList;

			//RoleList
			string roleId = rolesResponse.ToList().Find(item => item.IsAssigned == true).ID;
			RoleList.Add(roleId);

			inputJson.RoleList = RoleList;

			ProductBatch productBatch = new ProductBatch()
			{
				ProductId = productID,
				StatusTypeId = 5,
				RetryCount = 0,
				InputJson = inputJson
			};

			return productBatch;
		}

		/// <summary>
		/// Create SelfProvisioningPortal ProductBatch Record
		/// </summary>
		/// <param name="productID">Product Id</param>
		/// <returns>ProductBatch object</returns>
		private ProductBatch CreateSelfProvisioningPortalProductBatchRecord(int productID)
		{
			RolePropertyList inputJson = new RolePropertyList();

			ProductBatch productBatch = new ProductBatch()
			{
				ProductId = productID,
				StatusTypeId = 5,
				RetryCount = 0,
				InputJson = inputJson
			};

			return productBatch;
		}

		/// <summary>
		/// Used to copy DocManagement Users information to another user
		/// </summary>
		/// <param name="userClaim"></param>
		/// <param name="createUserPersonaId"></param>
		/// <param name="personaId"></param>
		/// <returns></returns>
		private ProductBatch CreateDocManagementBatchRecords(DefaultUserClaim userClaim, long createUserPersonaId, long personaId)
		{
			ManageProductRPDocumentManagement manageProductRpDocumentManagement = new ManageProductRPDocumentManagement(userClaim);

			List<string> propertyList = new List<string>();
			List<string> departmentList = new List<string>();
			List<PAMRolePropertyList> lstRoleProperties = new List<PAMRolePropertyList>();
			
			//List<string> roleList = new List<string>();
			RolePropertyList inputJson = new RolePropertyList() { IsAssigned = true };

			ListResponse result = manageProductRpDocumentManagement.GetPropertyRoles(createUserPersonaId, personaId, null);
			if (result != null && result.Records.Count > 0)
			{
				IList<ProductRole> roleList = result.Records.Cast<ProductRole>().ToList().FindAll(p => p.IsAssigned);
				foreach (ProductRole role in roleList)
				{
					PAMRolePropertyList objRole = new PAMRolePropertyList();
					objRole.RoleId = role.ID;
					if (role.Roletype != null)
					{
						// get the additional role info that is assigned to the user
						result = manageProductRpDocumentManagement.GetRoleClassifierDataset(createUserPersonaId, personaId, role.ID, null);
						if (result != null && result.Records.Count > 0)
						{
							IList<ProductProperty> assignedList = result.Records.Cast<ProductProperty>().ToList().FindAll(p => p.IsAssigned.Value);
							List<string> propertyIds = new List<string>();
							foreach (ProductProperty pp in assignedList)
							{
								propertyIds.Add(pp.ID);
							}
							objRole.PropertyIds = propertyIds;
						}
					}
					lstRoleProperties.Add(objRole);
				}
				inputJson.RolePropertiesList = lstRoleProperties;
			}

			ProductBatch productBatch = new ProductBatch()
			{
				ProductId = (int)ProductEnum.RPDocumentManagement,
				StatusTypeId = 5,
				RetryCount = 0,
				InputJson = inputJson
			};

			return productBatch;
		}

		/// <summary>
		/// Check For All Properties as one of the Keys
		/// </summary>
		/// <param name="additionalInfo">additional Info to process the data</param>
		/// <returns>All Properties = true</returns>
		private bool CheckForAllProperties(object additionalInfo)
		{
			bool allProperties = false;
			if (additionalInfo.GetType().Name.ToUpper() != "STRING")
			{
				Dictionary<string, bool> additionalDataCollection = (Dictionary<string, bool>)additionalInfo;
				foreach (KeyValuePair<string, bool> pair in additionalDataCollection)
				{
					if (pair.Key == "allProperties")
					{
						allProperties = pair.Value;
					}
				}
			}

			return allProperties;
		}
	}
}