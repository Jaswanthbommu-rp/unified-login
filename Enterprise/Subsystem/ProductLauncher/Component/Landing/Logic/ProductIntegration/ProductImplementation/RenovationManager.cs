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

		public RenovationManager(ProductEnum productType, long editorPersonaId, long subjectPersonaId, DefaultUserClaim userClaims) : base(productType, editorPersonaId, subjectPersonaId, userClaims)
		{ }

		public RenovationManager(ProductEnum productType, long editorPersonaId, long subjectPersonaId, DefaultUserClaim userClaims, IDataCollector injectedDataCollector, IManagePersona injectedManagePersona, IProductInternalSettingRepository productInternalSettingRepository) :
			base(productType, editorPersonaId, subjectPersonaId, userClaims, injectedDataCollector, injectedManagePersona, productInternalSettingRepository)
		{ }

		#endregion

		#region Methods
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
