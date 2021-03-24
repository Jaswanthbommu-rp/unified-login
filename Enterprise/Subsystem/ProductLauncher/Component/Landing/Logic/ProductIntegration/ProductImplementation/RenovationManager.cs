using System;
using System.Collections.Generic;
using System.Linq;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.ProductIntegration.Factory;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.ProductIntegration.Helpers;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.ProductIntegration.Model;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Base;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.ProductIntegration.ProductImplementation
{
	public class RenovationManager : ManageProductInvokerBase, IManageProductIntegration
	{
		private IManagePersona _managePersona = new ManagePersona();
		private const string PRODUCT_SETTINGTYPE_STATUS = "ProductStatus";

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
			List<string> roleNames = new List<string> { "SuperUserRoleId1", "SuperUserRoleId2" };
			productUser.Roles = GetProductInternalSettingValue(roleNames);
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

		/// <summary>
		/// Create or update product user
		/// Gets called from Product-Batch
		/// </summary> 
		public override string CreateUpdateProductUser(ProductUserRolePropertiesGroups userRolePropertiesRegion, BatchProcessType batchProcessType = BatchProcessType.CreateUpdateProductUser)
		{
			string result;

			WriteToDiagnosticLog($"RenovationManager.CreateUpdateProductUser - Product {ProductType} editorPersona id - {EditorUserDetails.PersonaId}. At beginning of method.");

			// Get product user object 
			var newProductUser = GenerateProductUserObject(userRolePropertiesRegion);

			if (SubjectUserDetails.UserRoleTypeId == (int)UserRoleType.UserNoEmail)
			{
				newProductUser.LoginName = newProductUser.Email;
			}

			var productUser = getBaseUserDataFromProduct(newProductUser.LoginName);
			//For Multi company user creation first check user data from product,if user data exists then do put else post

			if (string.IsNullOrEmpty(SubjectUserDetails.ProductUserName) && productUser == null)
			{
				WriteToDiagnosticLog($"RenovationManager.CreateUpdateProductUser - Product {ProductType} editorPersona id - {EditorUserDetails.PersonaId}. Calling CreateUser.");
				// Create User
				result = CreateUser(newProductUser);
			}
			//Create Multi company with put
			else if(string.IsNullOrEmpty(SubjectUserDetails.ProductUserName) && productUser != null)
			{
				result = CreateMultiCompanyUser(newProductUser);
			}
			else
			{
				WriteToDiagnosticLog($"RenovationManager.CreateUpdateProductUser - Product {ProductType} editorPersona id - {EditorUserDetails.PersonaId}. Calling UpdateUser.");
				// Update user with Id/Login from product
				if (productUser != null)
				{
					newProductUser.UserId = productUser.UserId;
					newProductUser.LoginName = productUser.LoginName;
				}
				else 
				{
					newProductUser.UserId = SubjectUserDetails.ProductUserId;
					newProductUser.LoginName = SubjectUserDetails.ProductUserName;
				}
				
				result = UpdateUser(newProductUser, batchProcessType);
			}
			return result;
		}

		/// <summary>
		/// Unassign user form the product 
		/// </summary>
		/// <returns></returns>
		public override string UnassignUser()
		{
			WriteToDiagnosticLog(
				$"RenovationManager.UnassignUser - Product {ProductType} editorPersona id - {EditorUserDetails.PersonaId}. At beginning of the method, calling DeleteUser().");
			var productUserProfile = new IntegrationProductUser
			{
				UserId = SubjectUserDetails.ProductUserId,
				IsActive = false,
				CompanyId = CompanyInstanceSourceId,
				LoginName = SubjectUserDetails.ProductUserName,
				Email = SubjectUserDetails.Email,
				FirstName = SubjectUserDetails.FirstName,
				LastName = SubjectUserDetails.LastName,
				Properties = new List<string>(),
				Roles = new List<string>(),
				IsMigratedUser = false,
				IsAdminUser = false
			};
			// Delete / deactivate uer in the product
			var result = DeleteUser(productUserProfile);
			if (result.IsSuccessStatusCode)
			{
				WriteToDiagnosticLog(
					$"RenovationManager.UnassignUser - Product {ProductType} editorPersona id - {EditorUserDetails.PersonaId}. DeleteUser() returns success, updating Greenboook status.");
				IManageUserLogin manageUserLogin = new ManageUserLogin();
				IUserLoginRepository userLoginRepository = new UserLoginRepository();
				var userLogin = manageUserLogin.GetUserLoginOnly(SubjectUserDetails.UserRealPageId);
				Persona persona = _managePersona.GetPersona(SubjectUserDetails.PersonaId);
				OrganizationStatus orgStatus = userLoginRepository.GetUserOrganizationWithStatus(userLogin.UserId, userLogin.LastLogin, persona.OrganizationPartyId, false);
				//var organizationList = userLoginRepository.ListOrganizationWithoutStatusByUserId(userLogin.UserId);
				//OrganizationStatus orgStatus = organizationList.FirstOrDefault(p => p.PartyId == persona.OrganizationPartyId);
				int statusValue = (int)UserUiStatusType.AccountHidden;
				//if user is disabled then set status to deactivated instead hidden
				if (orgStatus.Status.ToString().Equals(UserUiStatusType.Disabled.ToString(), StringComparison.OrdinalIgnoreCase))
				{
					statusValue = (int)UserUiStatusType.Deactivated;
				}
				// Update product status in green book
				_dataCollector.UpdateProductSettingProductStatus(SubjectUserDetails.PersonaId, PRODUCT_SETTINGTYPE_STATUS, ProductId, statusValue);
				// Activity Logging
				ProductActivityLogger.WriteUnassignUserActivityLog(EditorUserDetails, SubjectUserDetails, BlueBookGbProductMap.Name, BlueBookGbProductMap.BooksProductCode, CorrelationId);
				return string.Empty;
			}
			WriteToErrorLog($"RenovationManager.UnassignUser - Product {ProductType} editorPersona id - {EditorUserDetails.PersonaId}. DeleteUser() returns fail - error - {result}");
			return result.Content;
		}

		/// <summary>
		/// Get Product Roles
		/// </summary>
		/// <param name="dataFilter"></param>
		/// <param name="baseUrlAndQuery"></param>
		/// <returns></returns>
		public override ListResponse GetProductRoles(RequestParameter dataFilter, string baseUrlAndQuery = null)
		{
			try
			{
				WriteToDiagnosticLog(
					$"ManageProductInvokerBase.GetProductRoles - Product {ProductType} editorPersona id - {EditorUserDetails.PersonaId}. At beginning of the method.");
				if (string.IsNullOrEmpty(baseUrlAndQuery))
					baseUrlAndQuery = GetOperationEndPoint(ProductEntityEndpointKeyEnum.GetRoleEndpoint);
				WriteToDiagnosticLog(
					$"ManageProductInvokerBase.GetProductRoles - Product {ProductType} editorPersona id - {EditorUserDetails.PersonaId}. At API calling - {baseUrlAndQuery}");
				bool isCompanyIdRequiredToQuery = baseUrlAndQuery.Contains("{0}");
				if (isCompanyIdRequiredToQuery)
					baseUrlAndQuery = string.Format(baseUrlAndQuery, CompanyInstanceSourceId, "false");
				var roleList = GetResultFromApi<IList<ProductRole>>(baseUrlAndQuery);
				WriteToDiagnosticLog(
					$"ManageProductInvokerBase.GetProductRoles - Product {ProductType} editorPersona id - {EditorUserDetails.PersonaId}. Received roleList with count = {roleList?.Count}");
				if (!string.IsNullOrEmpty(SubjectUserDetails?.ProductUserName))
				{
					WriteToDiagnosticLog(
						$"ManageProductInvokerBase.GetProductRoles - Product {ProductType} editorPersona id - {EditorUserDetails.PersonaId}. Calling GetUser for subject persona Id -{SubjectUserDetails.PersonaId}");
					var user = GetProductUser(null, false);
					// map user roles
					if (user != null)
					{
						WriteToDiagnosticLog(
							$"ManageProductInvokerBase.GetProductRoles - Product {ProductType} editorPersona id - {EditorUserDetails.PersonaId}. Calling Merge for subject persona Id -{SubjectUserDetails.PersonaId}");
						var userRoles = user.Roles;
						MergeUserRoles(roleList, userRoles);
					}
				}
				if (roleList == null)
					throw new Exception("Null Role List.");
				return new ListResponse
				{
					Records = roleList.Cast<object>().ToList(),
					TotalRows = roleList.Count,
					RowsPerPage = 9999,
					ErrorReason = string.Empty,
					TotalPages = 1
				};
			}
			catch (Exception ex)
			{
				WriteToErrorLog($"ManageProductInvokerBase.GetProductRoles - Product {ProductType} editorPersona id - {EditorUserDetails.PersonaId}. Error - {ex.Message}", null, ex);
				return new ListResponse()
				{
					ErrorReason = ex.Message,
					IsError = true
				};
			}
		}
		/// <summary>
		/// Returns Product Properties
		/// </summary> 
		public override ListResponse GetProductProperties(RequestParameter dataFilter, string baseUrlAndQuery = null)
		{
			try
			{
				WriteToDiagnosticLog(
					$"ManageProductInvokerBase.GetProductProperties - Product {ProductType} editorPersona id - {EditorUserDetails.PersonaId}. At beginning of the method.");
				if (string.IsNullOrEmpty(baseUrlAndQuery))
				{
					baseUrlAndQuery = GetOperationEndPoint(ProductEntityEndpointKeyEnum.GetPropertyEndpoint);
					baseUrlAndQuery = string.Format(baseUrlAndQuery, CompanyInstanceSourceId);
				}
				WriteToDiagnosticLog(
					$"ManageProductInvokerBase.GetProductProperties - Product {ProductType} editorPersona id - {EditorUserDetails.PersonaId}. At API calling - {baseUrlAndQuery}");
				var propertyList = GetResultFromApi<IList<ProductProperties>>(baseUrlAndQuery);
				WriteToDiagnosticLog(
					$"ManageProductInvokerBase.GetProductProperties - Product {ProductType} editorPersona id - {EditorUserDetails.PersonaId}. Received propertyList with count = {propertyList?.Count}");
				if (!string.IsNullOrEmpty(SubjectUserDetails?.ProductUserName))
				{
					WriteToDiagnosticLog(
						$"ManageProductInvokerBase.GetProductProperties - Product {ProductType} editorPersona id - {EditorUserDetails.PersonaId}. Calling GetUser for subject persona Id -{SubjectUserDetails.PersonaId}");
					var user = GetProductUser(null, false);
					// map user properties
					if (user != null && user.Properties != null)
					{
						WriteToDiagnosticLog(
							$"ManageProductInvokerBase.GetProductProperties - Product {ProductType} editorPersona id - {EditorUserDetails.PersonaId}. Calling Merge for subject persona Id -{SubjectUserDetails.PersonaId}");
						if (user != null && user.Properties != null)
						{
							var userProperties = user.Properties.ConvertAll(p => p.ToUpper());
							MergeUserProperties(propertyList, userProperties);
						}
					}
				}
				if (propertyList == null)
					throw new Exception("Null Property List.");
				return new ListResponse
				{
					Records = propertyList.Cast<object>().ToList(),
					TotalRows = propertyList.Count,
					RowsPerPage = 9999,
					ErrorReason = string.Empty,
					TotalPages = 1
				};
			}
			catch (Exception ex)
			{
				WriteToErrorLog($"ManageProductInvokerBase.GetProductProperties - Product {ProductType} editorPersona id - {EditorUserDetails.PersonaId}. Error - {ex.Message}", null, ex);
				return new ListResponse()
				{
					ErrorReason = ex.Message,
					IsError = true
				};
			}
		}

		/// <summary>
		/// Delete User - patch with isActive = false
		/// </summary> 
		protected virtual ApiResponse DeleteUser(IntegrationProductUser profile = null)
		{
			WriteToDiagnosticLog(
				$"RenovationManager.DeleteUser - Product {ProductType} editorPersona id - {EditorUserDetails.PersonaId}. At beginning of the method.");
			// patch to se isActive flag to false
			var baseUrlAndQuery = GetOperationEndPoint(ProductEntityEndpointKeyEnum.PutUserEndpoint);
			WriteToDiagnosticLog(
				$"RenovationManager.DeleteUser - Product {ProductType} editorPersona id - {EditorUserDetails.PersonaId}. Calling API - {baseUrlAndQuery}.");
			DumpApiCallInfoToDiagnosticLog(baseUrlAndQuery, profile);
			var integration = new ApiIntegration(_httpClient, baseUrlAndQuery);
			return integration.PutEntity<string>(profile);
		}

		protected override IntegrationProductUser GenerateProductUserObject(ProductUserRolePropertiesGroups userRolePropertiesRegion)
		{
			// Map user info
			var productUser = new IntegrationProductUser
			{
				LoginName = string.IsNullOrEmpty(SubjectUserDetails.LoginName) ? SubjectUserDetails.LoginName : GetUniqueProductLogin(SubjectUserDetails.LoginName),
				CompanyId = CompanyInstanceSourceId,
				FirstName = SubjectUserDetails.FirstName,
				LastName = SubjectUserDetails.LastName,
				Email = SubjectUserDetails.Email,
				Phone = SubjectUserDetails.PhoneNumber,
				IsActive = true,
				PropertyGroups = userRolePropertiesRegion.PropertyGroupList,
				Properties = userRolePropertiesRegion.PropertyList,
				Roles = userRolePropertiesRegion.RoleList?.ConvertAll<string>(x => x.ToString()),
				PropertyRoles = userRolePropertiesRegion.PropertyRoleList,
				OrganizationRoles = userRolePropertiesRegion.OrganizationRoleList,
				CanReceiveMonthlyReport = userRolePropertiesRegion.CanReceiveMonthlyReport,
				PropertyRoleList = userRolePropertiesRegion.RolePropertiesList,
				RoleList = userRolePropertiesRegion.RoleList?.ConvertAll<string>(x => x.ToString()),
				IsMigratedUser = true
			};

			if (SubjectUserDetails.UserRoleTypeId == (int)UserRoleType.SuperUser)
			{
				ApplySuperUserData(productUser);
			}

			return productUser;
		}

		protected List<string> GetProductInternalSettingValue(List<string> settingName)
		{
			// Get product setting value
			var settingValue = ProductInternalSettingList.Where(a => settingName.Contains(a.Name)).Select(b => b.Value).ToList(); 
			return settingValue;
		}

		/// <summary>
		/// Create a user in the product
		/// </summary>
		protected override string CreateMultiCompanyUser(IntegrationProductUser productUser)
		{
			WriteToDiagnosticLog(
				$"ManageProductInvokerBase.CreateUser - Product {ProductType} editorPersona id - {EditorUserDetails.PersonaId}. At beginning of the method.");

			var baseUrlAndQuery = GetOperationEndPoint(ProductEntityEndpointKeyEnum.PostUserEndpoint);

			WriteToDiagnosticLog(
				$"ManageProductInvokerBase.CreateUser - Product {ProductType} editorPersona id - {EditorUserDetails.PersonaId}. Calling API - {baseUrlAndQuery}.");

			// dump api info
			DumpApiCallInfoToDiagnosticLog(baseUrlAndQuery, productUser);

			var integration = new ApiIntegration(_httpClient, baseUrlAndQuery);
			var result = integration.PutEntity<IntegrationProductUser>(productUser);

			if (result.IsSuccessStatusCode)
			{
				WriteToDiagnosticLog(
					$"ManageProductInvokerBase.CreateUser - Product {ProductType} editorPersona id - {EditorUserDetails.PersonaId}. Received success. Updating Geenbook mapping.");

				// map product user in green book
				_dataCollector.CreateProductUserInGreenBook(SubjectUserDetails.PersonaId, result.Content, ProductId, productUser.LoginName);

				// OPTIONAL - If product needs more attributes than userid or loginName then override in the product (e.g. PAM uses)
				CreateAdditionalSamlUserAttribute(SubjectUserDetails.PersonaId, ProductId, productUser);

				// activity logging
				ProductActivityLogger.WriteCreateUserActivityLog(EditorUserDetails, SubjectUserDetails, BlueBookGbProductMap.Name, BlueBookGbProductMap.BooksProductCode,
					CorrelationId);

				return string.Empty;
			}

			WriteToErrorLog(
				$"ManageProductInvokerBase.CreateUser - Product {ProductType} editorPersona id - {EditorUserDetails.PersonaId}. result received - {result}.");

			return result.Content;
		}
		#endregion
		#region private
		public  override IntegrationProductUser getBaseUserDataFromProduct(string loginNameToCheck, string baseUrlAndQuery = null)
		{
			if (string.IsNullOrEmpty(baseUrlAndQuery))
				baseUrlAndQuery = GetOperationEndPoint(ProductEntityEndpointKeyEnum.GetUserExistEndpoint);

			baseUrlAndQuery = string.Format(baseUrlAndQuery, loginNameToCheck);			

			WriteToDiagnosticLog(
				$"ManageProductInvokerBase.GetProductUser - Product {ProductType} editorPersona id - {EditorUserDetails.PersonaId}. At beginning of the method.");

			var productUser = GetResultFromApi<IntegrationProductUser>(baseUrlAndQuery, false);

			WriteToDiagnosticLog(
				$"ManageProductInvokerBase.GetProductUser - Product {ProductType} editorPersona id - {EditorUserDetails.PersonaId}. Calling API - {baseUrlAndQuery}.");

			return productUser;
		}
		#endregion
	}
}
