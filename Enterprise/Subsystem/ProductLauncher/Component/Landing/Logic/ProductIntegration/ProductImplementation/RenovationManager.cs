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
	public class RenovationManager : ManageProductInvokerBase, IManageProductIntegration
	{
		#region Ctor
		//private readonly IList<ProductInternalSetting> _productIntegrationDetails;
		public RenovationManager(ProductEnum productType, long editorPersonaId, long subjectPersonaId, DefaultUserClaim userClaims) : base(productType, editorPersonaId, subjectPersonaId, userClaims)
		{ }

		public RenovationManager(ProductEnum productType, long editorPersonaId, long subjectPersonaId, DefaultUserClaim userClaims, IDataCollector injectedDataCollector, IManagePersona injectedManagePersona, IProductInternalSettingRepository productInternalSettingRepository) :
			base(productType, editorPersonaId, subjectPersonaId, userClaims, injectedDataCollector, injectedManagePersona, productInternalSettingRepository)
		{ }

		#endregion

		#region Methods
		protected override void ApplySuperUserData(IntegrationProductUser productUser)
		{
			// super user related assignments
			string superUserRoleId = GetProductInternalSettingValue("SuperUserRoleId");
			productUser.Roles = new List<string> { superUserRoleId };
			productUser.Properties = new List<string>();
			productUser.PropertyGroups = new List<string>();
		}
		protected override bool CheckUserExistInProduct(string loginNameToCheck, string baseUrlAndQuery = null)
		{
			if (string.IsNullOrEmpty(baseUrlAndQuery))
				baseUrlAndQuery = GetOperationEndPoint(ProductEntityEndpointKeyEnum.GetUserEndpoint);

			bool isCompanyIdRequiredToQuery = baseUrlAndQuery.Contains("{0}");
			if (isCompanyIdRequiredToQuery)
			{
				baseUrlAndQuery = string.Format(baseUrlAndQuery, CompanyInstanceSourceId, loginNameToCheck);
			}
			else
			{
				baseUrlAndQuery = string.Format(baseUrlAndQuery, loginNameToCheck);
			}
						
			WriteToDiagnosticLog(
				$"ManageProductInvokerBase.GetProductUser - Product {ProductType} editorPersona id - {EditorUserDetails.PersonaId}. At beginning of the method.");

			var productUser =  GetResultFromApi<IntegrationProductUser>(baseUrlAndQuery, false);

			WriteToDiagnosticLog(
				$"ManageProductInvokerBase.GetProductUser - Product {ProductType} editorPersona id - {EditorUserDetails.PersonaId}. Calling API - {baseUrlAndQuery}.");

			if (productUser != null && !string.IsNullOrEmpty(productUser.UserId))
			{
				return true;
			}

			return false;
		}

		/// <summary>
		/// Get Product User API call
		/// </summary> 
		public override IntegrationProductUser GetProductUser(string baseUrlAndQuery = null, bool isThrowOnError = true)
		{
			WriteToDiagnosticLog(
				$"ManageProductInvokerBase.GetProductUser - Product {ProductType} editorPersona id - {EditorUserDetails.PersonaId}. At beginning of the method.");
			if (string.IsNullOrEmpty(baseUrlAndQuery))
				baseUrlAndQuery = GetOperationEndPoint(ProductEntityEndpointKeyEnum.GetUserEndpoint);

			bool isCompanyIdRequiredToQuery = baseUrlAndQuery.Contains("{0}");
			if (isCompanyIdRequiredToQuery)
			{
				baseUrlAndQuery = string.Format(baseUrlAndQuery, CompanyInstanceSourceId, SubjectUserDetails.ProductUserName);
			}
			else
			{
				baseUrlAndQuery = string.Format(baseUrlAndQuery, SubjectUserDetails.ProductUserName);
			}			

			WriteToDiagnosticLog(
				$"ManageProductInvokerBase.GetProductUser - Product {ProductType} editorPersona id - {EditorUserDetails.PersonaId}. Calling API - {baseUrlAndQuery}.");

			return GetResultFromApi<IntegrationProductUser>(baseUrlAndQuery, isThrowOnError);
		}

		/// <summary>
		/// Override this in product implementation if any product requires to create additional saml settings
		/// e.g. used in PAM
		/// </summary>
		protected override void CreateAdditionalSamlUserAttribute(long personaId, int productId, IntegrationProductUser productUser)
		{
			WriteToDiagnosticLog(
				$"RenovationManager.CreateAdditionalSamlUserAttribute - Product {ProductType} userLoginName - {productUser.LoginName} ; PMC {productUser.CompanyId} . At beginning of the method.");

			_dataCollector.CreateSamlUserAttribute(personaId, productId, SamlAttributeEnum.PMCID, productUser.CompanyId);
		}
		#endregion
	}
}
