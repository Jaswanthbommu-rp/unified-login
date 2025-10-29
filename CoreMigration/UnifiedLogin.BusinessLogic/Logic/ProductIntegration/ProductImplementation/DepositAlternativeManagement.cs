using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.BusinessLogic.Logic.ProductIntegration.Factory;
using UnifiedLogin.BusinessLogic.Logic.ProductIntegration.Helpers;
using UnifiedLogin.BusinessLogic.Logic.ProductIntegration.Model;
using UnifiedLogin.BusinessLogic.Repository;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Landing;
using System;
using System.Collections.Generic;
using System.Linq;

namespace UnifiedLogin.BusinessLogic.Logic.ProductIntegration.ProductImplementation
{
    public sealed class DepositAlternativeManagement : StandardV1ProductIntegration, IManageProductIntegration
	{
		public DepositAlternativeManagement(ProductEnum productType, long editorPersonaId, long subjectPersonaId, DefaultUserClaim userClaims) : base((int)productType, editorPersonaId, subjectPersonaId, userClaims)
		{ }

		public DepositAlternativeManagement(ProductEnum productType, long editorPersonaId, long subjectPersonaId, DefaultUserClaim userClaims, IDataCollector injectedDataCollector, IManagePersona injectedManagePersona, IProductInternalSettingRepository productInternalSettingRepository) :
			base((int)productType, editorPersonaId, subjectPersonaId, userClaims, injectedDataCollector, injectedManagePersona, productInternalSettingRepository)
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
			WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateSamlUserAttribute", $"productUserLoginName - {productUserLoginName}. At beginning of the method." });

			// if userName not matches with email then update user login with email
			if (!productUserLoginName.Equals(productUserEmail, StringComparison.OrdinalIgnoreCase))
			{
				_dataCollector.UpdateSamlUserAttribute(personaId, productId, SamlAttributeEnum.productUsername, productUserEmail);
			}
		}

		public override ListResponse GetProductPropertyGroups(RequestParameter dataFilter, string baseUrlAndQuery = null)
		{
			try
			{
				WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetProductPropertyGroups", $"editorPersona id - {EditorUserDetails.PersonaId}. At beginning of the method." });

				if (string.IsNullOrEmpty(baseUrlAndQuery))
				{
					baseUrlAndQuery = GetOperationEndPoint(ProductEntityEndpointKeyEnum.GetPropertyGroupsEndpoint);
					baseUrlAndQuery = string.Format(baseUrlAndQuery, CompanyInstanceSourceId);
				}

				WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetProductPropertyGroups", $"editorPersona id - {EditorUserDetails.PersonaId}. At API calling - {baseUrlAndQuery}" });

				var groupList = GetResultFromApi<IList<ProductPropertyGroups>>(baseUrlAndQuery);

				WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetProductPropertyGroups", $"editorPersona id - {EditorUserDetails.PersonaId}. Received regionList with count = {groupList?.Count}" });

				if (!string.IsNullOrEmpty(SubjectUserDetails?.ProductUserName))
				{
					WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetProductPropertyGroups", $"editorPersona id - {EditorUserDetails.PersonaId}. Calling GetUser for subject persona Id -{SubjectUserDetails.PersonaId}" });
					var user = GetProductUser();

					// map user regions
					if (user != null)
					{
						WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetProductPropertyGroups", $"editorPersona id - {EditorUserDetails.PersonaId}. Calling Merge for subject persona Id -{SubjectUserDetails.PersonaId}" });

						MergeUserPropertyGroups(groupList, user);
					}
				}

				if (groupList == null)
					throw new Exception("Null Property Group List.");

				return new ListResponse()
				{
					Records = groupList.Cast<object>().ToList(),
					TotalRows = groupList.Count(),
					RowsPerPage = 9999,
					ErrorReason = string.Empty,
					TotalPages = 1
				};
			}
			catch (Exception ex)
			{
				WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetProductPropertyGroups", $"editorPersona id - {EditorUserDetails.PersonaId}. Error - {ex.Message}" }, exception: ex);
				ListResponse response = new ListResponse
				{
					IsError = true
				};

				response.ErrorReason = ex.Message;
				return response;
			}
		}

		/// <summary>
		/// Unassign User
		/// </summary> 
		public override string UnassignUser()
		{
			WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UnassignUser", $"editorPersona id - {EditorUserDetails.PersonaId}. At beginning of the method, calling DeleteUser." });

			var productUserProfile = new ProductUserProfile
			{
				UserId = SubjectUserDetails.ProductUserId,
				IsActive = false,
				CompanyId = CompanyInstanceSourceId,
				LoginName = SubjectUserDetails.ProductUserName,
				Email = SubjectUserDetails.Email,
				FirstName = SubjectUserDetails.FirstName,
				LastName = SubjectUserDetails.LastName
			};

			// Delete / deactivate uer in the product
			var result = DeleteUser(productUserProfile);

			if (result.IsSuccessStatusCode)
			{
				WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UnassignUser", $"editorPersona id - {EditorUserDetails.PersonaId}. DeleteUser returns success, updating Greenboook status." });

				IManageUserLogin manageUserLogin = new ManageUserLogin();
				IUserLoginRepository userLoginRepository = new UserLoginRepository();
				var _managePersona = new ManagePersona();
				SamlRepository samlRepository = new SamlRepository();

				var userLogin = manageUserLogin.GetUserLoginOnly(SubjectUserDetails.UserRealPageId);
				Persona persona = _managePersona.GetPersona(SubjectUserDetails.PersonaId);

				OrganizationStatus orgStatus = userLoginRepository.GetUserOrganizationWithStatus(userLogin.UserId, userLogin.LastLogin, persona.OrganizationPartyId, false);
				int statusValue = (int)UserUiStatusType.AccountHidden;

				//if user is disabled then set status to deactivated instead hidden
				if (orgStatus.Status.ToString().Equals(UserUiStatusType.Disabled.ToString(), StringComparison.OrdinalIgnoreCase))
				{
					statusValue = (int)UserUiStatusType.Deactivated;
				}

				// Update product status in green book
				
				samlRepository.DeleteSamlUserProductInfoAndStatus(SubjectUserDetails.PersonaId, (int)ProductEnum.DepositAlternative);
				_dataCollector.UpdateProductSettingProductStatus(SubjectUserDetails.PersonaId, "ProductStatus", ProductId, statusValue);

				return string.Empty;
			}

			WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "UnassignUser", $"editorPersona id - {EditorUserDetails.PersonaId}. DeleteUser() returns fail - error - {result}" });

			return result.Content;
		}
	}
}
