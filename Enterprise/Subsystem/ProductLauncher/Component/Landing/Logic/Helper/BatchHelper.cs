using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.ProductIntegration.Model;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.Accounting;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.ResidentPortal;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.Rum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Saml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Helper
{
	public static class BatchHelper
	{
		public static ProductBatch CreateProductBatchRecord(ListResponse propertiesResponse,
															ListResponse rolesResponse,
															int productID,
															bool usePrimaryProperties,
															ProductIntegrationTypeEnum integrationType)
		{
			List<string> PropertyList = new List<string>();
			List<string> RoleList = new List<string>();
			List<string> MessageGroups = new List<string>();
			List<string> propertyGroupList = new List<string>();
			List<string> regionsList = new List<string>();
			bool allProperties = false;
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
				allProperties = CheckForAllProperties(propertiesResponse.Additional);
				isAssignNewPropertyByDefault = CheckForIsAssignedNewPropertyFlag(propertiesResponse.Additional);
			}

			if (productID != (int)ProductEnum.ProspectContactCenter)
			{
				if (rolesResponse.Records != null)
				{
					IEnumerable<object> roleCollection = (IEnumerable<object>)rolesResponse.Records;
					foreach (object item in roleCollection)
					{
						if (((SharedObjects.Product.ProductRole)item).IsAssigned)
						{
							RoleList.Add(((SharedObjects.Product.ProductRole)item).ID);
						}
					}
				}
			}

			if (allProperties)
			{
				if (productID == (int)ProductEnum.ClientPortal ||
					integrationType == ProductIntegrationTypeEnum.UPFM)
				{
					PropertyList.Add("-1");
				}
				else if (productID == (int)ProductEnum.OneSite ||
						 productID == (int)ProductEnum.FinancialSuite ||
						 productID == (int)ProductEnum.ProspectContactCenter ||
						 productID == (int)ProductEnum.MarketingCenter ||
						 productID == (int)ProductEnum.Insurance ||
						 productID == (int)ProductEnum.ResidentPortal)
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
						if (integrationType == ProductIntegrationTypeEnum.UPFM)
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
				InputJson = new RolePropertyList() { PropertyList = PropertyList, RoleList = RoleList, IsAssignedNewPropertyByDefault = isAssignNewPropertyByDefault, UsePrimaryProperties = usePrimaryProperties }
			};

			return pb;
		}
		public static ProductBatch CreateOnSiteBatchRecord(ListResponse propertiesResponse, ListResponse rolesResponse, ListResponse regionResponse, int productId, bool usePrimaryProperties)
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
			inputJson.UsePrimaryProperties = usePrimaryProperties;
			ProductBatch productBatch = new ProductBatch()
			{
				ProductId = productId,
				StatusTypeId = 5,
				RetryCount = 0,
				InputJson = inputJson
			};

			return productBatch;
		}

		public static ProductBatch CreateProductBatchRecordForClickPay(List<OrganizationRole> userOrganizationRole, bool usePrimaryProperties)
		{
			var pb = new ProductBatch()
			{
				ProductId = (int)ProductEnum.ClickPay,
				StatusTypeId = 5,
				RetryCount = 0,
				InputJson = new RolePropertyList()
				{
					OrganizationRoleList = userOrganizationRole,
					UsePrimaryProperties = usePrimaryProperties
				}
			};

			return pb;
		}

		public static ProductBatch CreateProductBatchRecordForDepositIQ(IntegrationProductUser productUser, bool usePrimaryProperties)
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
					PropertyList = productUser.Properties,
					UsePrimaryProperties = usePrimaryProperties
				}
			};

			return productBatch;
		}

		public static ProductBatch CreateIntegrationMarketplaceBatchRecord(int existingRoleId, int productProductId, bool usePrimaryProperties)
		{
			var roleList = new List<string> { existingRoleId.ToString() };
			ProductBatch productBatch = new ProductBatch()
			{
				ProductId = productProductId,
				StatusTypeId = 5,
				RetryCount = 0,
				InputJson = new RolePropertyList() { RoleList = roleList, UsePrimaryProperties = usePrimaryProperties }
			};

			return productBatch;
		}

		public static ProductBatch CreateILMProductBatchRecord(ProductEnum ilmProduct, List<string> productUserProperties,
			List<string> productUserRoles, List<string> productUserGroups, bool usePrimaryProperties)
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
					PropertyGroupList = productUserGroups,
					UsePrimaryProperties = usePrimaryProperties
				}
			};

			return pb;
		}

		public static ProductBatch CreateProductBatchRecordForPortfolioManagement(List<PAMRolePropertyList> rolePropertyList, List<string> roleList, bool usePrimaryProperties)
		{
			var pb = new ProductBatch()
			{
				ProductId = (int)ProductEnum.PortfolioManagement,
				StatusTypeId = 5,
				RetryCount = 0,
				InputJson = new RolePropertyList() { RolePropertiesList = rolePropertyList, RoleList = roleList, UsePrimaryProperties = usePrimaryProperties }
			};

			return pb;
		}

		public static ProductBatch CreateMarketingCenterProductBatchRecord(ListResponse propertiesResponse, ListResponse rolesResponse, int productID, bool usePrimaryProperties)
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
						if (((SharedObjects.Product.ProductRole)item).IsAssigned)
						{
							RoleList.Add(((SharedObjects.Product.ProductRole)item).ID);
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
				InputJson = new RolePropertyList() { PropertyList = PropertyList, RoleList = RoleList, IsAssignedNewPropertyByDefault = isAssignNewPropertyByDefault, UsePrimaryProperties = usePrimaryProperties }
			};

			return pb;
		}

		public static ProductBatch CreateFinancialSuiteProductBatchRecord(ListResponse propertiesResponse, ListResponse rolesResponse, int productID, ListResponse companiesResponse, ListResponse propertyGroupResponse, bool usePrimaryProperties)
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
					if (((SharedObjects.Product.ProductRole)item).IsAssigned)
					{
						RoleList.Add(((SharedObjects.Product.ProductRole)item).ID);
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

			foreach (object item in propertyGroupsCollection)
			{
				if (((ProductPropertyGroup)item).IsAssigned.Value)
				{
					PropertyList.Add(((ProductPropertyGroup)item).ID);
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
					CompaniesList = companiesList,
					UsePrimaryProperties = usePrimaryProperties
				}
			};

			return pb;
		}

		public static ProductBatch CreateVendorServiceProductBatchRecord(ListResponse propertiesResponse, ListResponse rolesResponse, ListResponse propertyGroup, Component.SharedObjects.Product.VendorServices.Notification notification, int productID, bool usePrimaryProperties)
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
				if (((SharedObjects.Product.ProductRole)item).IsAssigned)
				{
					RoleList.Add(((SharedObjects.Product.ProductRole)item).ID);
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
			inputJson.UsePrimaryProperties = usePrimaryProperties;

			ProductBatch pb = new ProductBatch()
			{
				ProductId = productID,
				StatusTypeId = 5,
				RetryCount = 0,
				InputJson = inputJson
			};

			return pb;
		}

		public static IList<ProductBatch> CreateAoBatchRecords(DefaultUserClaim userClaim, long editorPersonaId, long newUserPersonaId, bool externalUser = false, bool usePrimaryProperties = false)
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
							PropertyGroupList = (from i in aoUserCompanyPropertyRoleDetail.PropertyGroups select i.ToString()).ToList(),
							UsePrimaryProperties = usePrimaryProperties
						}
				};

				productBatchList.Add(productBatch);
			}

			return productBatchList;
		}

		public static ProductBatch CreateRumProductBatchRecord(ListResponse propertiesResponse, ListResponse groupResponse, ListResponse regionResponse, ListResponse rolesResponse, bool usePrimaryProperties)
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
				InputJson = new RolePropertyList() { PropertyList = propertyList, PropertyGroupList = propertyGroupList, RegionList = regionsList, RoleList = roleList, UsePrimaryProperties = usePrimaryProperties }
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
		public static ProductBatch CreateResidentPortalProductBatchRecord(ListResponse propertiesResponse, List<ILevel> rolesResponse, Notifications notifications, List<IMessagingGroups> messagingGroups, int productID, bool usePrimaryProperties)
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
			inputJson.UsePrimaryProperties = usePrimaryProperties;

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
		public static ProductBatch CreateRentersInsuranceProductBatchRecord(ListResponse propertiesResponse, IList<SharedObjects.Product.ProductRole> rolesResponse, int productID, bool usePrimaryProperties)
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
			inputJson.UsePrimaryProperties = usePrimaryProperties;
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
		public static ProductBatch CreateSelfProvisioningPortalProductBatchRecord(int productID)
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
		public static ProductBatch CreateDocManagementBatchRecords(DefaultUserClaim userClaim, long createUserPersonaId, long personaId, bool usePrimaryProperties)
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
				IList<SharedObjects.Product.ProductRole> roleList = result.Records.Cast<SharedObjects.Product.ProductRole>().ToList().FindAll(p => p.IsAssigned);
				foreach (SharedObjects.Product.ProductRole role in roleList)
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
			inputJson.UsePrimaryProperties = usePrimaryProperties;

			ProductBatch productBatch = new ProductBatch()
			{
				ProductId = (int)ProductEnum.RPDocumentManagement,
				StatusTypeId = 5,
				RetryCount = 0,
				InputJson = inputJson
			};

			return productBatch;
		}

		private static bool CheckForAllRegions(object additionalInfo)
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

		/// <summary>
		/// Check For All Properties as one of the Keys
		/// </summary>
		/// <param name="additionalInfo">additional Info to process the data</param>
		/// <returns>All Properties = true</returns>
		private static bool CheckForAllProperties(object additionalInfo)
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

		private static bool CheckForIsAssignedNewPropertyFlag(object additionalInfo)
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
	}
}
