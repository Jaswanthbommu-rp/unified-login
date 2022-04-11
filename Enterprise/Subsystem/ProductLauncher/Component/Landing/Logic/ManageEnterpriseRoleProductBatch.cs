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
using Newtonsoft.Json;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Base;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.Ops;

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
		private IPropertyRepository _propertyRepository;
		/// <summary>
		/// Default Constructor
		/// </summary>
		public ManageEnterpriseRoleProductBatch(DefaultUserClaim userClaim)
		{
			_userClaim = userClaim;

			var manageProduct = new ManageProduct(_userClaim);
			var manageUnifiedLogin = new ManageUnifiedLogin(_userClaim);
			var manageProductOneSite = new ManageProductOneSite(_userClaim);
			var productInternalSettingRepository = new ProductInternalSettingRepository();
			_productRepository = new ProductRepository();
			_propertyRepository = new PropertyRepository();
			_integrationTypeFactory = new IntegrationTypeFactory(manageProduct, manageUnifiedLogin,
				manageProductOneSite, _productRepository, productInternalSettingRepository, _userClaim);
		}
		public ManageEnterpriseRoleProductBatch(IProductRepository productRepository, IPropertyRepository propertyRepository)
		{
			_productRepository = productRepository;
			_propertyRepository = propertyRepository;
		}

		public string GenerateEnterpriseRoleUserProductBatch(EnterpriseRoleBatch batch)
		{
			IList<ProductBatch> productListToCreate = new List<ProductBatch>();
			IManagePersona _managePersona = new ManagePersona();
			ManageProductBatch manageProductBatch = new ManageProductBatch(_userClaim);
			var editorPersona = _managePersona.GetPersona(batch.EditorUserPersonaId);
			var userPersona = _managePersona.GetPersona(batch.SubjectUserPersonaId);
			_userClaim.UserRealPageGuid = editorPersona.RealPageId;
			_userClaim.OrganizationRealPageGuid = editorPersona.Organization.RealPageId;
			_userClaim.Rights = manageProductBatch.GetPersonaRoleRights(batch.EditorUserPersonaId, editorPersona.OrganizationPartyId);

			IPersonaRepository personaRepository = new PersonaRepository();
			IUserLoginRepository userLoginRepository = new UserLoginRepository();
			BatchProductBulkUpdateRepository enterpriseRoleProductRepository = new BatchProductBulkUpdateRepository();

			IList<Organization> organizationList = userLoginRepository.ListOrganizationByEnterpriseUserId(userPersona.RealPageId, null);
			var personaOrganization = organizationList.FirstOrDefault(i => i.PartyId == userPersona.OrganizationPartyId);
			var personaProductSettings = personaRepository.GetPersonaProductSettings(batch.SubjectUserPersonaId);
			var roleTemplateNewProducts = _productRepository.GetEnterpriseRoleNewProductsByRoleTemplateId(batch.EnterpriseRoleTemplateId);
			var roleTemplateUpdatedProducts = _productRepository.GetEnterpriseRoleUpdatedProductsByRoleTemplateId(batch.EnterpriseRoleTemplateId);
			var roleTemplateDeletedProducts = _productRepository.GetEnterpriseRoleDeletedProductsByRoleTemplateId(batch.EnterpriseRoleTemplateId);
			var roleTemplateProductRole = _productRepository.GetRoleTemplateProductRoleMapping(batch.EnterpriseRoleTemplateId, editorPersona.OrganizationPartyId);
			bool isExternalUser = personaOrganization.RelationshipType.Equals("User Type", StringComparison.OrdinalIgnoreCase) && personaOrganization.RoleNameFrom.Equals("External User", StringComparison.OrdinalIgnoreCase);

			string message = $"Enterprise role product update started to user - {batch.SubjectUserPersonaId}";
			Log.Write(LogEventLevel.Debug, message);
			bool personaProductUsePrimaryProperty = false;
			bool usePrimaryProperties = false;

			//remove any new products from updatedproduct list
			if (roleTemplateNewProducts?.Count > 0)
			{
				roleTemplateUpdatedProducts = roleTemplateUpdatedProducts.Except(roleTemplateNewProducts).ToList();
			}
			
			//New products
			foreach (var product in roleTemplateNewProducts)
			{
				ListResponse propertiesResponse = new ListResponse();
				ListResponse rolesResponse = new ListResponse();
				personaProductUsePrimaryProperty = false;
				usePrimaryProperties = false;

				bool productEnabledForPrimaryProperty = manageProductBatch.IsProductEnabledForUsePrimaryProperty(product);
				if (productEnabledForPrimaryProperty){
					var integrationType = _integrationTypeFactory.GetIntegrationTypeForProductId(product);

					var productRoles = GetProductRoleList(roleTemplateProductRole, product);
					rolesResponse = new ListResponse()
					{
						Records = productRoles.Cast<object>().ToList(),
						TotalRows = productRoles.Count,
						RowsPerPage = productRoles.Count,
						TotalPages = 1,
						ErrorReason = ""
					};

					propertiesResponse = manageProductBatch.GetEnterpriseRoleUserPrimaryPropertiesData(batch.EditorUserPersonaId, batch.SubjectUserPersonaId, product);

					if (propertiesResponse.Records?.Count > 0)
					{
						if (ProductEnumHelper.GetAoProductList().Contains((ProductEnum)product))
						{
							var batchRecords = BatchHelper.CreateAoBatchRecords(_userClaim, batch.EditorUserPersonaId, batch.SubjectUserPersonaId, isExternalUser, true);
							foreach (var productBatch in batchRecords)
							{
								productListToCreate.Add(productBatch);
							}
						}
						else{
							var productBatchRecord = manageProductBatch.GetProductBatchRecord(batch.EditorUserPersonaId, batch.SubjectUserPersonaId, productRoles, propertiesResponse, rolesResponse, product, true);
							productListToCreate.Add(productBatchRecord);
						}							
					}
				}
				
			}
			//updated products
			foreach (var product in roleTemplateUpdatedProducts)
			{
				try
				{
					ListResponse propertiesResponse = new ListResponse();
					ListResponse propertyGroupResponse = new ListResponse();
					ListResponse rolesResponse = new ListResponse();
					personaProductUsePrimaryProperty = false;
					usePrimaryProperties = false;

					bool productEnabledForPrimaryProperty = manageProductBatch.IsProductEnabledForUsePrimaryProperty(product);
					var integrationType = _integrationTypeFactory.GetIntegrationTypeForProductId(product);

					var productSetting = personaProductSettings.FirstOrDefault(item => item.Name.Equals("UsePrimaryProperties", StringComparison.OrdinalIgnoreCase) && item.ProductId == product);

					if (productSetting != null)
					{
						personaProductUsePrimaryProperty = productSetting.Value.Trim() == "1" ? true : false;
					}

					usePrimaryProperties = productEnabledForPrimaryProperty && personaProductUsePrimaryProperty;

					var productRoles = GetProductRoleList(roleTemplateProductRole, product);
					
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
						propertiesResponse = manageProductBatch.GetProductProperties(batch.EditorUserPersonaId, batch.SubjectUserPersonaId, product, _userClaim);
						if (propertiesResponse.IsError == true)
						{
							string PropertyErrorMessage = $"Enterprise role product update - There was a problem getting the list of properties for product {product} - user persona {batch.SubjectUserPersonaId}";
							Log.Write(LogEventLevel.Error, PropertyErrorMessage);
							enterpriseRoleProductRepository.UpdateEnterpriseRoleProductBatch(batch.EnterpriseRoleBatchProcessId, (int)ProductBatchStatusType.Error);
							return "Error";
						}
					}


					//Get product specific other info and create product batch
					if (product == (int)ProductEnum.UnifiedPlatform)
					{
						int platformRole = Convert.ToInt32(productRoles.Select(p => p.ID).FirstOrDefault());
						enterpriseRoleProductRepository.UpdateUnifiedPlatFormRole(platformRole, editorPersona.UserId, batch.SubjectUserPersonaId);
					}					
					else if (ProductEnumHelper.GetAoProductList().Contains((ProductEnum)product))
					{
						var batchRecords = BatchHelper.CreateAoBatchRecords(_userClaim, batch.EditorUserPersonaId, batch.SubjectUserPersonaId, isExternalUser, usePrimaryProperties);
						foreach (var productBatch in batchRecords)
						{
							productListToCreate.Add(productBatch);
						}
					}
					else
					{
						var productBatchRecord = manageProductBatch.GetProductBatchRecord(batch.EditorUserPersonaId, batch.SubjectUserPersonaId, productRoles, propertiesResponse, rolesResponse, product, usePrimaryProperties);
						productListToCreate.Add(productBatchRecord);
					}
				}
				catch (Exception ex)
				{
					string exmessage = $"Exception during enterprise role product updates to user - {batch.SubjectUserPersonaId}  for {product}";
					Log.Write(LogEventLevel.Error, ex, exmessage);
					enterpriseRoleProductRepository.UpdateEnterpriseRoleProductBatch(batch.EnterpriseRoleBatchProcessId, (int)ProductBatchStatusType.Error);
					return "Error";
				}

			}
			try
			{
				Dictionary<string, RolePropertyList> oneSiteAndOtherProducts = new Dictionary<string, RolePropertyList>();
				bool isOnesiteMix = false;
				if (productListToCreate?.Count > 0)
				{
					string btmessage = $"Enterprise role product batch update started to user - {batch.SubjectUserPersonaId} - product count {productListToCreate.Count}";
					Log.Write(LogEventLevel.Debug, btmessage);
					
					if (productListToCreate.Any(a => a.ProductId == (int)ProductEnum.OneSite)
						   && (productListToCreate.Any(a => a.ProductId == (int)ProductEnum.Lead2Lease) || productListToCreate.Any(a => a.ProductId == (int)ProductEnum.SeniorLeadManagement)))
					{
						// need to combine the Lead2Lease and OneSite product details so they can run synchronously				
						isOnesiteMix = true;
						ProductBatch pbOneSite = (from a in productListToCreate
												  where a.ProductId == (int)ProductEnum.OneSite
												  select a).FirstOrDefault();

						ProductBatch pbLead2Lease = null;
						ProductBatch pbSeniorLead = null;

						oneSiteAndOtherProducts.Add(ProductEnum.OneSite.ToString(), pbOneSite.InputJson);

						if (productListToCreate.Any(a => a.ProductId == (int)ProductEnum.Lead2Lease))
						{
							pbLead2Lease = (from a in productListToCreate
											where a.ProductId == (int)ProductEnum.Lead2Lease
											select a).FirstOrDefault();

							oneSiteAndOtherProducts.Add(ProductEnum.Lead2Lease.ToString(), pbLead2Lease.InputJson);
							productListToCreate.Remove(pbLead2Lease);
						}

						if (productListToCreate.Any(a => a.ProductId == (int)ProductEnum.SeniorLeadManagement))
						{
							pbSeniorLead = (from a in productListToCreate
											where a.ProductId == (int)ProductEnum.SeniorLeadManagement
											select a).FirstOrDefault();

							oneSiteAndOtherProducts.Add(ProductEnum.Lead2Lease.ToString(), pbSeniorLead.InputJson);
							productListToCreate.Remove(pbSeniorLead);
						}
					}					
				}
				if (roleTemplateDeletedProducts?.Count > 0)
				{
					foreach (var product in roleTemplateDeletedProducts)
					{
						ProductBatch pb = new ProductBatch()
						{
							ProductId = product,
							StatusTypeId = 5,
							RetryCount = 0,
							InputJson = new RolePropertyList() { PropertyList = new List<string>(), RoleList = new List<string>(), IsAssigned = false }
						};
						productListToCreate.Add(pb);
					}
				}
				if (productListToCreate?.Count > 0)
				{
					int statusTypeId = (int)ProductBatchStatusType.Success;
					bool isBatchCompleted = enterpriseRoleProductRepository.SaveProductBatch(batch.EditorUserPersonaId, batch.SubjectUserPersonaId, editorPersona.RealPageId, productListToCreate, JsonConvert.SerializeObject(oneSiteAndOtherProducts), isOnesiteMix, (int)BatchProcessType.EnterpriseRoleCreateUpdateProductUser);
					if (!isBatchCompleted)
					{
						statusTypeId = (int)ProductBatchStatusType.Error;
					}
					bool status = enterpriseRoleProductRepository.UpdateEnterpriseRoleProductBatch(batch.EnterpriseRoleBatchProcessId, statusTypeId);
				}
			}
			catch (Exception ex)
			{
				string exmessage = $"Exception during enterprise role product batch data insert to user - {batch.SubjectUserPersonaId}";
				Log.Write(LogEventLevel.Error, ex, exmessage);
				enterpriseRoleProductRepository.UpdateEnterpriseRoleProductBatch(batch.EnterpriseRoleBatchProcessId, (int)ProductBatchStatusType.Error);
				return "Error";
			}

			return "";
		}

		private List<string> GetSelectedProperties(ListResponse productResult)
		{
			List<string> selectedProperties = new List<string>();
			var productPropertyType = productResult.Records[0].GetType();

			if (productPropertyType == typeof(ProductProperty))
			{
				var productList = productResult.Records.Cast<ProductProperty>();
				foreach (var property in productList)
				{
					if (property.IsAssigned == true)
					{
						selectedProperties.Add(property.ID);
					}
				}
			}
			else if (productPropertyType == typeof(ACProperty))
			{
				foreach (var property in productResult.Records.Cast<ACProperty>())
				{
					if (property.IsAssigned == true)
					{
						selectedProperties.Add(property.Id);
					}
				}
			}
			else if (productPropertyType == typeof(AssetGroup))
			{
				foreach (var property in productResult.Records.Cast<AssetGroup>())
				{
					if (property.IsAssigned == true)
					{
						selectedProperties.Add(property.ID);
					}
				}
			}
			else if (productPropertyType == typeof(OnSiteProperty))
			{
				foreach (var property in productResult.Records.Cast<OnSiteProperty>())
				{
					if (property.IsAssigned == true)
					{
						selectedProperties.Add(property.GetPropertyId.ToString());
					}
				}
			}
			else if (productPropertyType == typeof(RumPropertyGroup))
			{
				foreach (var property in productResult.Records.Cast<RumPropertyGroup>())
				{
					if (property.IsAssigned == true)
					{
						selectedProperties.Add(property.Id.ToString());
					}
				}
			}
			else if (productPropertyType == typeof(ProductProperties))
			{
				foreach (var property in productResult.Records.Cast<ProductProperties>())
				{
					if (property.IsAssigned == true)
					{
						selectedProperties.Add(property.GetPropertyId.ToString());
					}
				}
			}
			else if (productPropertyType == typeof(Portfolio))
			{
				foreach (var property in productResult.Records.Cast<Portfolio>())
				{
					if (property.IsAssigned == true)
					{
						selectedProperties.Add(property.ID);
					}
				}
			}
			return selectedProperties;
		}

		private IList<ProductRole>  GetProductRoleList (List<RoleTemplateProductRole> roleTemplateProductRole, int productId)
		{
			ListResponse rolesResponse = new ListResponse();
			IList<ProductRole> productRoles = new List<ProductRole>();

			var productRoleData = roleTemplateProductRole?.Where(p => p.ProductId == productId);

			var roleTemplateRoles = productRoleData?.Select(p => new
			{
				p.RoleTemplateProductRoleMappingId,
				p.ProductRoleId,
				p.ProductRoleName
			}).Distinct();


			//Roles
			//List<string> productRoles = new List<string>();
			//IList<ProductRole> productRoles = new List<ProductRole>();
			foreach (var role in roleTemplateRoles)
			{
				if (role.RoleTemplateProductRoleMappingId != 0)
				{
					productRoles.Add(new ProductRole
					{
						ID = role.ProductRoleId.ToString(),
						Name = role.ProductRoleName,
						IsAssigned = true
					});
				}
			}

			return productRoles;
		}		
	}
}
