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
    public class ManagePrimaryPropertiesBatch
    {
		private readonly DefaultUserClaim _userClaim;
		private IProductRepository _productRepository;
		private readonly IntegrationTypeFactory _integrationTypeFactory;
		private IPropertyRepository _propertyRepository;
		/// <summary>
		/// Default Constructor
		/// </summary>
		public ManagePrimaryPropertiesBatch(DefaultUserClaim userClaim)
		{
			_userClaim = userClaim;
			//var manageProductBatch = new ManageProductBatch(_userClaim);
			var manageProduct = new ManageProduct(_userClaim);
			var manageUnifiedLogin = new ManageUnifiedLogin(_userClaim);
			var manageProductOneSite = new ManageProductOneSite(_userClaim);
			var productInternalSettingRepository = new ProductInternalSettingRepository();
			_productRepository = new ProductRepository();
			_propertyRepository = new PropertyRepository();
			_integrationTypeFactory = new IntegrationTypeFactory(manageProduct, manageUnifiedLogin,
				manageProductOneSite, _productRepository, productInternalSettingRepository, _userClaim);
		}
		public ManagePrimaryPropertiesBatch(IProductRepository productRepository, IPropertyRepository propertyRepository)
		{
			_productRepository = productRepository;
			_propertyRepository = propertyRepository;
		}

		public string GeneratePrimaryPropertiesUserProductBatch(PrimaryPropertyBatch batch)
        {
			IList<ProductBatch> productListToCreate = new List<ProductBatch>();
			IManagePersona _managePersona = new ManagePersona();
			ManageProductBatch manageProductBatch = new ManageProductBatch(_userClaim);
			var editorPersona = _managePersona.GetPersona(batch.EditorUserPersonaId);
			var userPersona = _managePersona.GetPersona(batch.SubjectUserPersonaId);
			_userClaim.UserRealPageGuid = editorPersona.RealPageId;
			_userClaim.OrganizationRealPageGuid = editorPersona.Organization.RealPageId;
			_userClaim.Rights = manageProductBatch.GetPersonaRoleRights(batch.EditorUserPersonaId, editorPersona.OrganizationPartyId);
			_userClaim.OrganizationPartyId = editorPersona.OrganizationPartyId;

			IPersonaRepository personaRepository = new PersonaRepository();
			IUserLoginRepository userLoginRepository = new UserLoginRepository();
			BatchProductBulkUpdateRepository productBulkUpdateRepository = new BatchProductBulkUpdateRepository();

			IList<Organization> organizationList = userLoginRepository.ListOrganizationByEnterpriseUserId(userPersona.RealPageId, null);
			var personaOrganization = organizationList.FirstOrDefault(i => i.PartyId == userPersona.OrganizationPartyId);
			var personaProductSettings = personaRepository.GetPersonaProductSettings(batch.SubjectUserPersonaId);
			var personaProducts = _productRepository.ListProductsByPersonaId(userPersona.PersonaId, (Int32)UserUiStatusType.AccountCreationSuccessful).ToList();
			bool isExternalUser = personaOrganization.RelationshipType.Equals("User Type", StringComparison.OrdinalIgnoreCase) && personaOrganization.RoleNameFrom.Equals("External User", StringComparison.OrdinalIgnoreCase);

			string message = $"Primary Properties product update started to user - {batch.SubjectUserPersonaId}";
			Log.Write(LogEventLevel.Debug, message);
			bool personaProductUsePrimaryProperty = false;
			bool usePrimaryProperties = false;

			foreach (var product in personaProducts)
            {
				ListResponse propertiesResponse = new ListResponse();
				ListResponse rolesResponse = new ListResponse();
				personaProductUsePrimaryProperty = false;
				usePrimaryProperties = false;

				string prodmessage = $"Primary Properties product update batch record generation started for product - {product.ProductName}";
				Log.Write(LogEventLevel.Debug, prodmessage);

				var integrationType = _integrationTypeFactory.GetIntegrationTypeForProductId(product.ProductId);
				bool productEnabledForPrimaryProperty = manageProductBatch.IsProductEnabledForUsePrimaryProperty(product.ProductId);
				var productSetting = personaProductSettings.FirstOrDefault(item => item.Name.Equals("UsePrimaryProperties", StringComparison.OrdinalIgnoreCase) && item.ProductId == product.ProductId);

				if (productSetting != null)
				{
					personaProductUsePrimaryProperty = productSetting.Value.Trim() == "1" ? true : false;
				}

				usePrimaryProperties = productEnabledForPrimaryProperty && personaProductUsePrimaryProperty;

				if (productEnabledForPrimaryProperty)
				{
					rolesResponse = manageProductBatch.GetProductRoles(editorPersona.PersonaId, userPersona.PersonaId, product.ProductId, userPersona.OrganizationPartyId, _userClaim);				
					
					propertiesResponse = manageProductBatch.GetEnterpriseRoleUserPrimaryPropertiesData(batch.EditorUserPersonaId, batch.SubjectUserPersonaId, product.ProductId);

					if (propertiesResponse.Records?.Count > 0 && rolesResponse.Records?.Count > 0)
					{
						if (ProductEnumHelper.GetAoProductList().Contains((ProductEnum)product.ProductId))
						{
							var batchRecords = BatchHelper.CreateAoBatchRecords(_userClaim, batch.EditorUserPersonaId, batch.SubjectUserPersonaId, isExternalUser, true);
							foreach (var productBatch in batchRecords)
							{
								productListToCreate.Add(productBatch);
							}
						}
						else
						{
							var productRoles = rolesResponse.Records?.Cast<ProductRole>().ToList();
							var productBatchRecord = manageProductBatch.GetProductBatchRecord(batch.EditorUserPersonaId, batch.SubjectUserPersonaId, productRoles, propertiesResponse, rolesResponse, product.ProductId, true);
							productListToCreate.Add(productBatchRecord);
						}
					}
				}				
			}
			try
			{
				Dictionary<string, RolePropertyList> oneSiteAndOtherProducts = new Dictionary<string, RolePropertyList>();
				bool isOnesiteMix = false;
				if (productListToCreate?.Count > 0)
				{
					string btmessage = $"Primary Properties product batch update started to user - {batch.SubjectUserPersonaId} - product count {productListToCreate.Count}";
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
				if (productListToCreate?.Count > 0)
				{
					int statusTypeId = (int)ProductBatchStatusType.Success;
					bool isBatchCompleted = productBulkUpdateRepository.SaveProductBatch(batch.EditorUserPersonaId, batch.SubjectUserPersonaId, editorPersona.RealPageId, productListToCreate, JsonConvert.SerializeObject(oneSiteAndOtherProducts), isOnesiteMix, (int)BatchProcessType.PrimaryPropertiesUpdateProductUser);
					if (!isBatchCompleted)
					{
						statusTypeId = (int)ProductBatchStatusType.Error;
					}
					bool status = productBulkUpdateRepository.UpdatePrimaryPropertyProductBatch(batch.PrimaryPropertyBatchProcessId, statusTypeId);
				}
			}
			catch (Exception ex)
			{
				string exmessage = $"Exception during product primary properties updates to user - {batch.SubjectUserPersonaId} ";
				Log.Write(LogEventLevel.Error, ex, exmessage);
				productBulkUpdateRepository.UpdatePrimaryPropertyProductBatch(batch.PrimaryPropertyBatchProcessId, (int)ProductBatchStatusType.Error);
				return "Error";
			}
			return "";
		}

	}
}
