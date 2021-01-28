using System;
using System.Collections.Generic;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.ProductIntegration.Factory;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.ProductIntegration.Helpers;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.ProductIntegration.Model;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.ProductIntegration.ProductImplementation
{
	/// <summary>
	/// Product specific implementation 
	/// Override methods from base class if custom implementation
	/// </summary>
	public sealed class LeadManagement : ManageProductInvokerBase, IManageProductIntegration
	{
		#region Ctor

		public LeadManagement(ProductEnum productType, long editorPersonaId, long subjectPersonaId, DefaultUserClaim userClaims) : base(productType, editorPersonaId, subjectPersonaId, userClaims)
		{
			_userClaims = userClaims;
			ProductId = (int)ProductEnum.LeadManagement;
			ProductType = productType;
			_productDetails = _productRepository.GetBooksMasterProductDetail(ProductId);
			_udmSourceCode = _productDetails.UDMSourceCode?.Length > 0 ? _productDetails.UDMSourceCode : _productDetails.BooksProductCode;
		}

		public LeadManagement(ProductEnum productType, long editorPersonaId, long subjectPersonaId, DefaultUserClaim userClaims, IDataCollector injectedDataCollector, IManagePersona injectedManagePersona, IProductInternalSettingRepository productInternalSettingRepository) :
			base(productType, editorPersonaId, subjectPersonaId, userClaims, injectedDataCollector, injectedManagePersona, productInternalSettingRepository)
		{ }

		#endregion

		protected override void ApplySuperUserData(IntegrationProductUser productUser)
		{
			// super user related assignments
			if (ProductType == ProductEnum.LeadAnalytics)
			{
				productUser.Roles = new List<string> { "18" };
			}
			else
			{
				productUser.Roles = new List<string> { "17" };
			}

			productUser.Properties = new List<string>() { "all" };
			productUser.PropertyGroups = new List<string>();
		}

		protected override void UpdateSamlUserAttribute(long personaId, int productId,
			string productUserId, string productUserLoginName, string productUserEmail)
		{
			WriteToDiagnosticLog(
				$"LeadManagement.UpdateSamlUserAttribute - Product {ProductType} productUserLoginName - {productUserLoginName}. At beginning of the method.");

			// Issue - GB-4715
			// if userName not matches with email then update user login with email
			if (!productUserLoginName.Equals(productUserEmail, StringComparison.OrdinalIgnoreCase))
			{
				_dataCollector.UpdateSamlUserAttribute(personaId, productId, SamlAttributeEnum.productUsername, productUserEmail);
			}
		}
	}
}