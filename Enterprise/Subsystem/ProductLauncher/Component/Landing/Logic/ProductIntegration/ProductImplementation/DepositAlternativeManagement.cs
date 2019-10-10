using System;
using System.Collections.Generic;
using System.Linq;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.ProductIntegration.Factory;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.ProductIntegration.Helpers;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.ProductIntegration.Model;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Base;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.ProductIntegration.ProductImplementation
{
	public sealed class DepositAlternativeManagement : ManageProductInvokerBase, IManageProductIntegration
	{
		public DepositAlternativeManagement(ProductEnum productType, long editorPersonaId, long subjectPersonaId, DefaultUserClaim userClaims) : base(productType, editorPersonaId, subjectPersonaId, userClaims)
		{ }

		public DepositAlternativeManagement(ProductEnum productType, long editorPersonaId, long subjectPersonaId, DefaultUserClaim userClaims, IDataCollector injectedDataCollector, IManagePersona injectedManagePersona, IProductInternalSettingRepository productInternalSettingRepository) :
			base(productType, editorPersonaId, subjectPersonaId, userClaims, injectedDataCollector, injectedManagePersona, productInternalSettingRepository)
		{ }

		public override ListResponse GetProductRoles(RequestParameter dataFilter, string baseUrlAndQuery = null)
		{
			var listResponse = base.GetProductRoles(dataFilter);
			Dictionary<string, bool> additionalInfo;
			if (!string.IsNullOrEmpty(SubjectUserDetails?.ProductUserName))
			{
				var user = GetProductUser();
				additionalInfo = new Dictionary<string, bool>
				{
					{"CanReceiveMonthlyReport", user.CanReceiveMonthlyReport }
				};
			}
			else
			{
				additionalInfo = new Dictionary<string, bool>
				{
					{"CanReceiveMonthlyReport", false }
				};
			}

			listResponse.Additional = additionalInfo;
			return listResponse;
		}

		protected override void MergeUserPropertyGroups(IList<ProductPropertyGroups> groupList, IntegrationProductUser user)
		{
			List<string> userPropertyGroups = user.PropertyGroups;
			var role = user.Roles[0];

			if (userPropertyGroups != null && userPropertyGroups.Any())
			{
				foreach (var group in groupList)
				{
					if (group.GroupType == role && userPropertyGroups.Contains(group.GetGroupId))
					{
						group.IsAssigned = true;
					}
				}
			}
		}

		protected override void UpdateSamlUserAttribute(long personaId, int productId,
			string productUserId, string productUserLoginName, string productUserEmail)
		{
			WriteToDiagnosticLog(
				$"DepositAlternativeManagement.UpdateSamlUserAttribute - Product {ProductType} productUserLoginName - {productUserLoginName}. At beginning of the method.");

			// if userName not matches with email then update user login with email
			if (!productUserLoginName.Equals(productUserEmail, StringComparison.OrdinalIgnoreCase))
			{
				_dataCollector.UpdateSamlUserAttribute(personaId, productId, SamlAttributeEnum.productUsername, productUserEmail);
			}
		}
	}
}
