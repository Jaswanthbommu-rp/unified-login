using Newtonsoft.Json;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Enterprise;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Audit.Common;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Constants;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enterprise;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Extensions;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Helper;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.Ops;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.ResponseObject;
using RP.Enterprise.Subsystem.ProductLauncher.Service.SharedObjects.Kafka;
using Serilog;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using Role = RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.UserManagement.Role;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Enterprise.User
{
	/// <summary>
	/// User Management related logic
	/// </summary>
	public class UserManagement
	{
		#region Private variables
		private DefaultUserClaim _userClaims;
		#endregion

		#region Ctor
		/// <summary>
		/// UserManagement Constructor
		/// </summary>
		/// <param name="userClaims"></param>
		public UserManagement(DefaultUserClaim userClaims)
		{
			_userClaims = userClaims;
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Create Enterprise user
		/// </summary>
		public ObjectResponse CreateEnterpriseUnityUser(UserProductDetails userProductDetails)
		{
			var logData = new Dictionary<string, object> { { "userProductDetails", userProductDetails } };
			WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", logData, null, new object[] { "CreateEnterpriseUnityUser", "Beginning CreateEnterpriseUnityUser for new user with json" });

			var response = new ObjectResponse();

			// validations
			var validationError = ValidateUserProductDetails(userProductDetails);
			if (!string.IsNullOrEmpty(validationError))
			{
				response.IsError = true;
				response.ErrorReason = validationError;
				return response;
			}

			// Check if user exists
			IManageUserLogin userLoginLogic = new ManageUserLogin(_userClaims);
			var userOrganizationExists = userLoginLogic.IsLoginNameExists(
				userProductDetails.UserProfileDetails.LoginName,
				userProductDetails.UserProfileDetails.OrganizationRealPageId,
                Guid.Empty,
                userProductDetails.UserProfileDetails.FirstName,
                userProductDetails.UserProfileDetails.LastName);
			if (userOrganizationExists.UserExists)
			{
				response.IsError = true;
				response.ErrorReason = "User Login Name already exists.";
				return response;
			}
			if(userOrganizationExists.IsValidDomainUsername)
			{
				response.IsError = true;
				response.ErrorReason = "Email domain is not allowed.";
				return response;
            }

            // Check product roles & properties are valid
            var validProductError = ValidateProductData(userProductDetails.ProductList);
			if (validProductError.Count() > 0)
			{
				response.IsError = true;
				response.ErrorReason = String.Join(",", validProductError);
				return response;
			}

			// get password hash for local IDP
			if (!userProductDetails.UserProfileDetails.IsExternalIdp && !(userProductDetails.UserProfileDetails.SendInvitationEmail ?? false))
			{
				// Get password hash & salt
				var pwd = userProductDetails.UserProfileDetails.Password.PasswordHash();
				userProductDetails.UserProfileDetails.PasswordHash = pwd.PasswordHash;
				userProductDetails.UserProfileDetails.PasswordSalt = pwd.PasswordSalt;
				WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", null, null, new object[] { "CreateEnterpriseUnityUser", $"Received password hash salt info for new user with login name {userProductDetails.UserProfileDetails.LoginName}" });
			}

			WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", null, null, new object[] { "CreateEnterpriseUnityUser", $"Getting custom field info for new user with login name {userProductDetails.UserProfileDetails.LoginName}" });

			// custom fields
			IList<CustomFieldValue> userCustomFields = null;
			var userCustomFieldValueJson = string.Empty;
			var errorReason = ValidateAndAssignCustomFieldValues(null, userProductDetails.UserProfileDetails.CustomFields, out userCustomFields);

			if (!string.IsNullOrEmpty(errorReason))
			{
				response.IsError = true;
				response.ErrorReason = errorReason;
				return response;
			}

			WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", null, null, new object[] { "CreateEnterpriseUnityUser", $"Calling CreateEnterpriseUser for new user with login name {userProductDetails.UserProfileDetails.LoginName}" });

			// create user
			var entUserRepository = new EntUserRepository(_userClaims);
			userProductDetails.UserProfileDetails.FirstName = userProductDetails.UserProfileDetails.FirstName.TrimWhiteSpace();
			userProductDetails.UserProfileDetails.MiddleName = userProductDetails.UserProfileDetails.MiddleName != null ? userProductDetails.UserProfileDetails.MiddleName.TrimWhiteSpace() : "";
			userProductDetails.UserProfileDetails.LastName = userProductDetails.UserProfileDetails.LastName.TrimWhiteSpace();
			var userRealPageId = entUserRepository.CreateEnterpriseUser(userProductDetails);

			WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", null, null, new object[] { "CreateEnterpriseUnityUser", $"Received new user id {userRealPageId} for new user with login name {userProductDetails.UserProfileDetails.LoginName}" });

			bool isMailNotified = false;
			if (userProductDetails.UserProfileDetails.SendInvitationEmail ?? false)
			{
				isMailNotified = SendInvitationEmail(new Guid(userRealPageId));
			}

			WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", null, null, new object[] { "CreateEnterpriseUnityUser", $"Adding activity log for new user with RealPage id {userRealPageId} for new user with login name {userProductDetails.UserProfileDetails.LoginName}" });

			// Get User Details from persona id
			var userRepository = new UserRepository(_userClaims);
			var newUserDetails = userRepository.GetUserDetails(userRealPageId: userRealPageId);
			IUserLoginPersonaRepository userLoginPersonaRepository = new UserLoginPersonaRepository();
			IList<UserLoginPersona> userLoginPersonaList = userLoginPersonaRepository.ListUserLoginPersona(userLoginPersonaId: null, userLoginId: newUserDetails.UserId, organizationPartyId: userProductDetails.UserProfileDetails.OrganizationPartyId);
			
			if (userCustomFields != null)
			{
				userCustomFields.ToList().ForEach(c => c.UserLoginPersonaId = userLoginPersonaList[0].UserLoginPersonaId);
				userCustomFieldValueJson = JsonConvert.SerializeObject(userCustomFields);
				ICustomFieldsRepository customFieldsRepository = new CustomFieldsRepository();
				RepositoryResponse repositoryResponse = customFieldsRepository.AddUpdateFieldValue(customFieldsValuesJson: userCustomFieldValueJson, createdBy: _userClaims.UserId);
				if (repositoryResponse.Id == 0)
				{
					repositoryResponse.ErrorMessage = $"Add/Update custom fields values {userCustomFieldValueJson} Error: {repositoryResponse.ErrorMessage}.";
				}
			}

			WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", null, null, new object[] { "CreateEnterpriseUnityUser", $"Custom fields json - {userCustomFieldValueJson} for new user with login name {userProductDetails.UserProfileDetails.LoginName}" });

			LogAuditActivity(LogActivityTypeConstants.CREATE_USER, LogActivityCategoryType.User, "New User {0} {1} successfully created by RealPage user {2} using enterprise API.", "CreateUser", newUserDetails);
			if (userProductDetails.UserProfileDetails.SendInvitationEmail ?? false)
			{
				if (isMailNotified)
				{
					//Log Activity
					LogAuditActivity(LogActivityTypeConstants.EMAIL_SENT, LogActivityCategoryType.Email, "Welcome Email sent to user {0} {1} by RealPage user {2}.", "CreateUser", newUserDetails);
				}
			}
			response.Data = userRealPageId;

			return response;
		}

		/// <summary>
		/// Update Enterprise Unity User
		/// </summary>
		public ObjectResponse UpdateEnterpriseUnityUser(UserProductDetails userProductDetails)
		{
			WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", null, null, new object[] { "UpdateEnterpriseUnityUser", $"Beginning UpdateEnterpriseUnityUser for update user with json {JsonConvert.SerializeObject(userProductDetails)}" });

			var response = new ObjectResponse();

			// check login name matches with id
			IManageUserLogin manageUserLogin = new ManageUserLogin();
			bool isValidUsername = manageUserLogin.ValidateUsername(
				userProductDetails.UserProfileDetails.UserRealPageId,
				userProductDetails.UserProfileDetails.LoginName);
			if (!isValidUsername)
			{
				response.IsError = true;
				response.ErrorReason = "User login name doesn't match with RealPage Id.";
				return response;
			}

			// common validations
			var validationError = ValidateUserProductDetails(userProductDetails);
			if (!string.IsNullOrEmpty(validationError))
			{
				response.IsError = true;
				response.ErrorReason = validationError;
				return response;
			}

			// Check product roles & properties are valid
			var validProductError = ValidateProductData(userProductDetails.ProductList);
			if (validProductError.Count() > 0)
			{
				response.IsError = true;
				response.ErrorReason = String.Join(",", validProductError);
				return response;
			}

			// Get password hash & salt for non-idp user
			if (!userProductDetails.UserProfileDetails.IsExternalIdp)
			{
				var pwd = userProductDetails.UserProfileDetails.Password.PasswordHash();
				userProductDetails.UserProfileDetails.PasswordHash = pwd.PasswordHash;
				userProductDetails.UserProfileDetails.PasswordSalt = pwd.PasswordSalt;
				WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", null, null, new object[] { "UpdateEnterpriseUnityUser", $"Received password hash salt info for update user with login name {userProductDetails.UserProfileDetails.LoginName}" });
			}

			// Get User Details from persona id
			var userRepository = new UserRepository(_userClaims);
			var updateUserDetails = userRepository.GetUserDetails(userRealPageId: userProductDetails.UserProfileDetails.UserRealPageId.ToString());
			IUserLoginPersonaRepository userLoginPersonaRepository = new UserLoginPersonaRepository();
			IList<UserLoginPersona> userLoginPersonaList = userLoginPersonaRepository.ListUserLoginPersona(userLoginPersonaId: null, userLoginId: updateUserDetails.UserId, organizationPartyId: userProductDetails.UserProfileDetails.OrganizationPartyId);

			IProfileDetail updateObject = new ProfileDetail
			{
				CreateUserSourceType = CreateUserSourceType.RPX
			};

			WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", null, null, new object[] { "UpdateEnterpriseUnityUser", $"Getting custom field info for update user with login name {userProductDetails.UserProfileDetails.LoginName}" });

			// custom fields
			IList<CustomFieldValue> userCustomFields = null;
			var userCustomFieldValueJson = string.Empty;
			var errorReason = ValidateAndAssignCustomFieldValues(userLoginPersonaList[0].UserLoginPersonaId, userProductDetails.UserProfileDetails.CustomFields, out userCustomFields);

			if (!string.IsNullOrEmpty(errorReason))
			{
				response.IsError = true;
				response.ErrorReason = errorReason;
				return response;
			}

			updateObject.CustomFields = userCustomFields;
			if (userCustomFields != null)
			{
				userCustomFieldValueJson = JsonConvert.SerializeObject(userCustomFields);
			}

			WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", null, null, new object[] { "UpdateEnterpriseUnityUser", $"Custom fields json - {userCustomFieldValueJson} for update user with login name {userProductDetails.UserProfileDetails.LoginName}" });

			// check from & thru date supplied
			// if not supplied then use from one already assigned
			updateObject.userLogin.FromDate = userProductDetails.UserProfileDetails.UserEffectiveDate ?? updateUserDetails.FromDate;

			updateObject.userLogin.ThruDate = userProductDetails.UserProfileDetails.UserExpirationDate ?? updateUserDetails.ThruDate;

			// User Login
			updateObject.userLogin.PartyId = updateUserDetails.PersonPartyId; // GetUserPartyId(userProductDetails.UserProfileDetails.UserRealPageId);
			updateObject.userLogin.RealPageId = userProductDetails.UserProfileDetails.UserRealPageId;
			updateObject.userLogin.IsActive = true;
			updateObject.userLogin.Status = UserUiStatusType.Active;
			updateObject.userLogin.Password = userProductDetails.UserProfileDetails.Password;
			updateObject.userLogin.UserRoleType = UserRoleType.User; //userProductDetails.UserProfileDetails.GbUserType ?? "Regular User" :
			updateObject.userLogin.Is3rdPartyIDP = userProductDetails.UserProfileDetails.IsExternalIdp;
			updateObject.userLogin.UserId = updateUserDetails.UserId;
			updateObject.userLogin.LoginName = userProductDetails.UserProfileDetails.LoginName; //??

			// Organization
			updateObject.organization.Add(new Organization
			{
				RealPageId = _userClaims.OrganizationRealPageGuid,
				PartyId = _userClaims.OrganizationPartyId,
				BooksMasterId = _userClaims.OrganizationMasterId,
				Name = _userClaims.OrganizationName
			});

			// persona
			updateObject.Persona = new List<Persona>
			{
				new Persona
				{
					Organization = new Organization
					{
						RealPageId = _userClaims.OrganizationRealPageGuid,
						PartyId = _userClaims.OrganizationPartyId,
						BooksMasterId = _userClaims.OrganizationMasterId,
						Name = _userClaims.OrganizationName
					},
					PersonaTypeId = 3,
					PersonaEnvironmentTypeId = 1,
					hasManageSpendManagementProductAccess = true, //??
					hasViewOnlySupportToolAccess = true, //?
					hasViewOnlySettingsAccess = true, //?
					hasImportUsersAccess = true,
					UserTypeId = 401, // userProductDetails.UserProfileDetails.GbUserType,
					PersonaId = updateUserDetails.PersonaId,
					PersonPartyId = updateUserDetails.PersonPartyId, // GetUserPartyId(userProductDetails.UserProfileDetails.UserRealPageId),
					RealPageId = updateUserDetails.UserRealPageId,
					OrganizationPartyId = _userClaims.OrganizationPartyId,
					Name = "Primary",
					UserId = updateUserDetails.UserId,
					Role = new List<Role>() //?
				}
			};

			updateObject.Password = userProductDetails.UserProfileDetails.Password;
			updateObject.NotificationEmail = userProductDetails.UserProfileDetails.Email;

			updateObject.UserTypeId = 401;

			// user profile details
			updateObject.PartyId = updateUserDetails.PersonPartyId;
			updateObject.RealPageId = updateUserDetails.UserRealPageId;
			updateObject.FirstName = userProductDetails.UserProfileDetails.FirstName.TrimWhiteSpace();
			updateObject.MiddleName = userProductDetails.UserProfileDetails.MiddleName != null ? userProductDetails.UserProfileDetails.MiddleName.TrimWhiteSpace() : "";
			updateObject.LastName = userProductDetails.UserProfileDetails.LastName.TrimWhiteSpace();
			updateObject.Title = userProductDetails.UserProfileDetails.Title;
			updateObject.Suffix = userProductDetails.UserProfileDetails.Suffix;
			updateObject.EmployeeId = userProductDetails.UserProfileDetails.EmployeeId;

			// add product batch
			updateObject.productBatch = GetProductBatchData(userProductDetails.ProductList);

			WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", null, null, new object[] { "UpdateEnterpriseUnityUser", $"Calling UpdateUser for user with editorRelPageId {userProductDetails.EditorRealPageId} and updateObject json {JsonConvert.SerializeObject(updateObject)}" });

			IManageUser mu = new ManageUser(_userClaims);
			var result = mu.UpdateUser(userProductDetails.EditorRealPageId, updateObject);

			if (string.IsNullOrEmpty(result.ErrorMessage))
			{
				response.Data = result.RealPageId.ToString();
			}

			WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", null, null, new object[] { "UpdateEnterpriseUnityUser", $"Received user id {result.RealPageId.ToString()} for update user with login name {userProductDetails.UserProfileDetails.LoginName}" });

			return response;
		}

		/// <summary>
		/// Activate Deactivate User from enterprise user.
		/// </summary>
		public ObjectResponse ActivateDeactivateUser(Guid unityRealPageUserId, UserUiStatusType statusTypeName)
		{
			var response = new ObjectResponse();

			WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", null, null, new object[] { "ActivateDeactivateUser", $"Beginning for update user with unityRealPageUserId {unityRealPageUserId} and status to change {statusTypeName}" });

			// check user exists
			var userLoginRepository = new UserLoginRepository();
			var currentUserLogin = userLoginRepository.GetUserLoginOnly(unityRealPageUserId);
			if (statusTypeName == UserUiStatusType.Active && currentUserLogin.PrimaryOrganization && currentUserLogin.UserRoleTypeId != UserTypeConstants.RegularUserNoEmail && !currentUserLogin.IsRPEmployee)
            {                 
                    UnifiedLoginUserStatusFactory.ProduceUnifiedLoginUserStatusAsync(currentUserLogin.LoginName, true, currentUserLogin.UserDeactivationDate.HasValue ? currentUserLogin.UserDeactivationDate.Value : (DateTime?)null);
			}
			if (statusTypeName == UserUiStatusType.Disabled && currentUserLogin.PrimaryOrganization && currentUserLogin.UserRoleTypeId != UserTypeConstants.RegularUserNoEmail && !currentUserLogin.IsRPEmployee)
            {
                    UnifiedLoginUserStatusFactory.ProduceUnifiedLoginUserStatusAsync(currentUserLogin.LoginName, false, currentUserLogin.UserDeactivationDate.HasValue ? currentUserLogin.UserDeactivationDate.Value : (DateTime?)null);
			}

            if (currentUserLogin == null || string.IsNullOrEmpty(currentUserLogin.LoginName))
			{
				response.IsError = true;
				response.ErrorReason = "Users RealPageUserId is incorrect";
				return response;
			}

			IManageUserLogin userLoginLogic = new ManageUserLogin(_userClaims);
			bool isCreateUpdateUserStatusSucceed = userLoginLogic.CreateUpdateUserStatus(unityRealPageUserId, statusTypeName);

			WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", null, null, new object[] { "ActivateDeactivateUser", $"returned {isCreateUpdateUserStatusSucceed} with unityRealPageUserId {unityRealPageUserId} and status to change {statusTypeName}" });

			if (isCreateUpdateUserStatusSucceed)
			{
				WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", null, null, new object[] { "ActivateDeactivateUser", $"updating product status with unityRealPageUserId {unityRealPageUserId} and status to change {statusTypeName}" });

				IList<UserLoginOnly> userLogins = new List<UserLoginOnly>();
				var ul = new UserLoginOnly() { RealPageId = unityRealPageUserId };
				userLogins.Add(ul);

				UpdateUserProductStatus(userLogins, statusTypeName);

				response.Data = "Success";
			}
			else
			{
				response.IsError = true;
				response.ErrorReason = "Error while changing user status.";
			}

			return response;
		}

		/// <summary>
		/// Get/List Users
		/// </summary>
		/// <param name="organizationPartyId">Company unique partyId</param>
		/// <param name="statusTypeId">Status Type Id</param>
		/// <param name="realPageId">Optional User EnterpriseId</param>
		/// <param name="name">Optional filter by FirstName, LastName, or UserName</param>
		/// <param name="rowsPerPage">Optional Rows Per page to return</param>
		/// <param name="pageNumber">Optional PageNumber</param>
		/// <returns>List of Users object</returns>
		public IList<UsersData> ListUser(long organizationPartyId, int statusTypeId, Guid? realPageId = null, string name = null, int rowsPerPage = 0, int pageNumber = 1)
		{
			IList<UsersData> usersDataList = new List<UsersData>();
			ProductRepository productRepository = new ProductRepository();

			IList<int> filterProductIdList = new List<int>();
			IList<int> productIdList = productRepository.GetProductIdsByCompany(organizationPartyId);

            // only count Ops product for now because that is the only product that works with Enterprise API currently
			if (productIdList.ToList().Any(p => p == (int)ProductEnum.OpsBuyer))
			{
				filterProductIdList.Add((int)ProductEnum.OpsBuyer);
			}
            filterProductIdList.Add((int)ProductEnum.UnifiedPlatform);

			EntUserRepository entUserRepository = new EntUserRepository(_userClaims);
			usersDataList = entUserRepository.ListUsers(organizationPartyId, filterProductIdList, statusTypeId, realPageId, name, rowsPerPage, pageNumber);

			return usersDataList;
		}
		#endregion

		public IList<UserProductDetailLogin> ListUserProductDetailsLoginByPersonaId(long PersonaId)
		{
			try
			{
				IList<UserProductDetailLogin> userProductDetailLogins = new List<UserProductDetailLogin>();

				EntUserRepository entUserRepository = new EntUserRepository(_userClaims);
				IList<UserProductDetailAttribute> userProuctDetailAttributes = entUserRepository.ListUserProductDetailsLoginByPersonaId(PersonaId);

				userProuctDetailAttributes.ToList().ForEach(u =>

                userProductDetailLogins.Add(new UserProductDetailLogin 
				{ 
					ProductId = u.ProductId,
					ProductCode = u.ProductCode,
					
					Details = JsonConvert.DeserializeObject<IList<Dictionary<string,string>>>(u.UserAttribute)
				})) ; 


				return userProductDetailLogins;
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}

		public IList<UserProductDetailLogin> ListUserProductDetailsLoginByLoginName(string loginName)
		{
			try
			{
				WriteToLog(LogEventLevel.Information, "{ActionName} - {state}", null, null, new object[] { "ListUserProductDetailsLoginByLoginName", $"Beginning ListUserProductsSamlDetailByLoginName to the user with LoginName {loginName}" });

				IList<UserProductDetailLogin> userProductDetailLogins = new List<UserProductDetailLogin>();

				WriteToLog(LogEventLevel.Information, "{ActionName} - {state}", null, null, new object[] { "ListUserProductDetailsLoginByLoginName",
					$"Getting the Product SAML Attributes to the user with LoginName {loginName}"});

				EntUserRepository entUserRepository = new EntUserRepository(_userClaims);
				IList<UserProductDetailAttribute> userProuctDetailAttributes = entUserRepository.ListUserProductDetailsLoginByLoginName(loginName);

				WriteToLog(LogEventLevel.Information, "{ActionName} - {state}", null, null, new object[] { "ListUserProductDetailsLoginByLoginName",
					$"Information received for the user with LoginName {loginName}"});

				userProuctDetailAttributes.ToList().ForEach(u =>
				userProductDetailLogins.Add(new UserProductDetailLogin
				{
					ProductId = u.ProductId,
					ProductCode = u.ProductCode,
					Company = u.Company,
					RealPageId = u.RealPageId,
					UserType = u.UserType == "401" ? UserRoleType.User.ToEnumDescription() : u.UserType == "402" ? UserRoleType.SuperUser.ToEnumDescription() : UserRoleType.ExternalUser.ToEnumDescription(),
					Details = JsonConvert.DeserializeObject<IList<Dictionary<string, string>>>(u.UserAttribute)
				}));


				return userProductDetailLogins;
			}
			catch (Exception ex)
			{
				WriteToLog(LogEventLevel.Error, "{ActionName} - {state}", null, ex, new object[] { "ListUserProductDetailsLoginByLoginName",
					$"Error  {ex.Message}"});
				throw ex;
			}
		}
		#region Private Methods
		/// <summary>
		/// Used for both Create and Update user
		/// </summary>
		private string ValidateUserProductDetails(UserProductDetails userProductDetails)
		{
			var organizationRealPageId = userProductDetails.UserProfileDetails.OrganizationRealPageId;

			// password for local
			if (!userProductDetails.UserProfileDetails.IsExternalIdp)
			{
				if (!(userProductDetails.UserProfileDetails.SendInvitationEmail ?? false) && (userProductDetails.UserProfileDetails.Password == null || string.IsNullOrEmpty(userProductDetails.UserProfileDetails.Password.Trim())))
				{
					return "Password is required.";
				}
			}

			// Check if company exists
			var orgRepo = new OrganizationRepository();
			Organization org = orgRepo.GetOrganization(organizationRealPageId);
			if (org == null)
			{
				return "Request has incorrect CompanyId.";
			}

			long organizationPartyId = org.PartyId;
			userProductDetails.UserProfileDetails.CompanyName = org.Name;

			// Check if company has external Identity provider & requested
			if (userProductDetails.UserProfileDetails.IsExternalIdp)
			{
				var orgProviders = orgRepo.GetOrganizationIdentityProviderType(organizationRealPageId);

				var result = orgProviders.Where(i => i.AuthenticationType.ToUpper() != "LOCAL").ToList();
				if (!result.Any())
				{
					// no external IDP exists for company
					return "Company has no external Identity Provider configuration. Send IsExternalIdp as false.";
				}
			}

			// product exists for company
			if (userProductDetails.ProductList != null && userProductDetails.ProductList.Any())
			{
				var productRepo = new ProductRepository();
				var orgProducts = productRepo.GetProductIdsByCompany(organizationRealPageId);
				var prodRepository = new ProductRepository(_userClaims);

				if (orgProducts.Any())
				{
					foreach (var product in userProductDetails.ProductList)
					{
						var productMap = prodRepository.GetBooksMasterProductDetail(product.ProductCode.ToUpper());
						if (productMap == null)
						{
							return $"Product with code {product.ProductCode} is incorrect.";
						}
						var productId = productMap.ProductId;
						if (!orgProducts.Contains(productId))
						{
							return $"Product with code {product.ProductCode} is not available for the company.";
						}

						// OPS requires one role & one property
						if (productId == (int)ProductEnum.OpsBuyer)
						{
							if (product.PropertiesAssigned == null || product.PropertiesAssigned.Count != 1)
							{
								return $"Product with code {product.ProductCode} requires one property.";
							}

							if (product.RolesAssigned == null || product.RolesAssigned.Count != 1)
							{
								return $"Product with code {product.ProductCode} requires one role.";
							}

							// Property & Role has integer id
							int i;
							if (!int.TryParse(product.PropertiesAssigned.FirstOrDefault(), out i))
							{
								return $"Product with code {product.ProductCode} requires integer property Id.";
							}

							if (!int.TryParse(product.RolesAssigned.FirstOrDefault(), out i))
							{
								return $"Product with code {product.ProductCode} requires integer role Id.";
							}
						}
					}
				}
				else
				{
					if (userProductDetails.ProductList.Any())
					{
						return "Organization has no products assigned.";
					}
				}
			}

			// run password rules for organization
			if (!(userProductDetails.UserProfileDetails.SendInvitationEmail ?? false) && !userProductDetails.UserProfileDetails.IsExternalIdp)
			{
				var pwd = new ManageCredential(new DefaultUserClaim());
				var validatePasswordResponse = pwd.ValidatePassword(new ValidatePassword
				{
					PartyId = organizationPartyId,
					PasswordToValidate = userProductDetails.UserProfileDetails.Password,
					EnterpriseUserName = userProductDetails.UserProfileDetails.LoginName
				});

				if (validatePasswordResponse.IsError)
				{
					validatePasswordResponse.IsError = true;
					return validatePasswordResponse.ErrorReason;
				}
			}

			// everything looks good
			return null;
		}

		private IList<ProductBatch> GetProductBatchData(IList<ProductDetail> userProductList)
		{
			var productRepo = new ProductRepository();
			var productBatch = new List<ProductBatch>();

			if (userProductList != null)
			{
				foreach (var product in userProductList)
				{
					productBatch.Add(new ProductBatch
					{
						//CreateUserPersonaId = editorUserPersonaId,
						//AssignUserPersonaId = subjectUserPersonaId,
						ProductId = productRepo.GetBooksMasterProductDetail(product.ProductCode).ProductId,
						//PersonPartyId = 1,
						StatusTypeId = 5,
						InputJson = new RolePropertyList()
						{
							PropertyList = product.PropertiesAssigned,
							RoleList = product.RolesAssigned,
							RegionList = product.RegionsAssigned,
							IsAssigned = product.IsAssigned
						}
					});
				}
			}

			return productBatch;
		}

		private bool SendInvitationEmail(Guid userRealPageId)
		{
			var profileLogic = new ManageProfile(_userClaims);
			IProfileDetail profileDetail = new ProfileDetail();
            profileDetail = profileLogic.GetProfileDetail(userRealPageId, _userClaims.OrganizationPartyId);

			IManageUserRegistrationEmail manageUserRegistrationEmail = new ManageUserRegistrationEmail(_userClaims);
			return manageUserRegistrationEmail.SendNewUserRegistrationEmail(profileDetail);
		}

		private void LogAuditActivity(string logActivityType, LogActivityCategoryType logActivityCategoryType,
			string message, string stepName, UserDetails userDetails)
		{
			string userName = string.IsNullOrEmpty(_userClaims.ImpersonatedByName) ? _userClaims.FirstName + " " + _userClaims.LastName : " RealPage Access (" + _userClaims.ImpersonatedByName + ") ";
			try
			{
				LogActivity.WriteActivity(new ActivityDetails
				{
					LogActivityTypeName = logActivityType,
					LogCategoryName = logActivityCategoryType.ToString(),
					CorrelationId = _userClaims.CorrelationId.ToString(),
					BooksMasterOrganizationId = _userClaims.OrganizationMasterId,
                    OrganizationPartyId = _userClaims.OrganizationPartyId,
					Message = string.Format(message, userDetails.FirstName, userDetails.LastName, userName),
					FromUserLoginName = _userClaims.LoginName,
					FromUserLoginId = _userClaims.UserId,
					FromUserRealpageId = _userClaims.UserRealPageGuid.ToString(),
					FromUserFirstName = _userClaims.FirstName,
					FromUserLastName = _userClaims.LastName,

					ToUserLoginName = userDetails.LoginName,
					ToUserLoginId = userDetails.UserId,
					ToUserFirstName = userDetails.FirstName,
					ToUserLastName = userDetails.LastName,
					ToUserRealpageId = userDetails.UserRealPageId.ToString()
				});
			}
			catch (Exception ex)
			{
				WriteToLog(LogEventLevel.Error, "{ActionName} - {state}", null, ex, new object[] { "LogAuditActivity", $"Error while adding activity message. BooksMasterOrganizationId{_userClaims.OrganizationName}, author user login name {_userClaims.LoginName}" });
			}
		}

        /// <summary>
        /// Used to write to the central log
        /// </summary>
        /// <param name="logType">Log Type</param>
        /// <param name="message">Message template</param>
        /// <param name="logData">Dictionary of additional properties to log</param>
        /// <param name="exception">Exception details</param>
        /// <param name="messageProperties">Message properties</param>
        private void WriteToLog(LogEventLevel logType, string message, Dictionary<string, object> logData = null, Exception exception = null, object[] messageProperties = null)
        {
			try
			{
                string correlationId = "";
                if (_userClaims != null)
                {
                    correlationId = (_userClaims.CorrelationId != Guid.Empty) ? _userClaims.CorrelationId.ToString() : "";

                }
                var logger = Log.Logger;
				if (logData?.Keys != null)
				{
					logger = logger.ForContext("AdditionalInfo", JsonConvert.SerializeObject(logData, Formatting.Indented), false);
				}
				logger = logger.ForContext("ProductModule", this.GetType());
                logger = logger.ForContext("CorrelationId", correlationId);

                logger.Write(level: logType, exception: exception, messageTemplate: message, propertyValue0: messageProperties?[0], propertyValue1: messageProperties?[1]);
            }
			catch
			{
				/*ignored*/
			}
		}

		public string ValidateAndAssignCustomFieldValues(long? userLoginPersonaId, Dictionary<string, string> customFields, out IList<CustomFieldValue> userCustomFieldsOut)
		{
			userCustomFieldsOut = null;

			IManageCustomFields manageCustomFields = new ManageCustomFields(_userClaims);
			// Custom Fields validation and assignment
			try
			{
				var userCustomFields = customFields;

				IList<CustomFieldValue> customFieldValueList = manageCustomFields.GetCustomFieldsValues(organizationPartyId: _userClaims.OrganizationPartyId, userLoginPersonaId: userLoginPersonaId, enabled: true);
				bool customFieldsEnabled = ((customFieldValueList != null) && (customFieldValueList.Count > 0));

				if (customFieldsEnabled)
				{
					if (customFieldValueList.Any())
					{
						// Custom field validations
						foreach (var companyCustomFieldValue in customFieldValueList)
						{
							if (companyCustomFieldValue.Required == true)
							{
								// check user supplied custom field else error
								if (userCustomFields == null || !userCustomFields.Keys.Contains(companyCustomFieldValue.Name))
								{
									return $"{companyCustomFieldValue.Name} is required custom field & not provided.";
								}
							}

							// max-min length check TODO: check for string customFiledType  
							KeyValuePair<string, string> kv = new KeyValuePair<string, string>();
							if (userCustomFields != null)
							{
								kv = userCustomFields.FirstOrDefault(c => c.Key.Equals(companyCustomFieldValue.Name, StringComparison.OrdinalIgnoreCase));
							}

							if (companyCustomFieldValue.MaxCharLength > 0)
							{
								if (kv.Value != null && kv.Key != null)
                                {
                                    if (kv.Value.Length > companyCustomFieldValue.MaxCharLength)
                                    {
                                        return $"{companyCustomFieldValue.Name} is required max characters up to {companyCustomFieldValue.MaxCharLength}.";
                                    }

                                    if (kv.Value.Length < companyCustomFieldValue.MinCharLength)
                                    {
                                        return $"{companyCustomFieldValue.Name} is required minimum characters up to {companyCustomFieldValue.MinCharLength}.";
                                    }
                                }
							}
							companyCustomFieldValue.Value = kv.Value;
						}
					}

					// custom field json
					userCustomFieldsOut = customFieldValueList;
				}
			}
			catch (Exception ex)
			{
				//elastic logging
				WriteToLog(LogEventLevel.Error, "{ActionName} - {state}", null, ex, new object[] { "ValidateAndAssignCustomFieldValues", "Exception while ValidateAndAssignCustomFieldValues" });
			}

			// all ok
			return string.Empty;
		}

		private void UpdateUserProductStatus(IList<UserLoginOnly> userLogins, UserUiStatusType userLoginStatusType)
		{
			IRepositoryResponse response = new RepositoryResponse();
			IManageUser manageUser = new ManageUser(_userClaims);
			IManagePersona managePersona = new ManagePersona(_userClaims);
			Persona persona = managePersona.GetFirstAvailablePersonaByCompany(_userClaims.UserRealPageGuid, _userClaims.OrganizationPartyId);
			manageUser.UpdateUserStatus(_userClaims.UserRealPageGuid, persona.PersonaId, userLogins, userLoginStatusType);
		}

        private IList<ProductDetail> GetProductSharedWithOtherProduct(IList<ProductDetail> productList)
        {
            var prodRepository = new ProductRepository(_userClaims);
            IList<int> organizationProducts = prodRepository.GetProductIdsByCompany(_userClaims.OrganizationPartyId);
            var allProductList = prodRepository.GetAllProducts().ToList();
            var orgProductList = allProductList.Where(p => organizationProducts.Contains(p.ProductId)).ToList();

            var productInternalSettingRepository = new ProductInternalSettingRepository();
            var lstProductsWithDatasharedProduct = productInternalSettingRepository.GetProductSettingByType(SettingConstants.SharedProductSettingName);

            var lstProducts = new List<(int sourceProductId, int targetProductId)>();

            foreach (var product in lstProductsWithDatasharedProduct)
            {
                if (!string.IsNullOrEmpty(product.Value) && int.TryParse(product.Value, out var sourceProductId))
                {
                    lstProducts.Add((product.ProductId, sourceProductId));
                }
            }

            foreach (var (sourceProductId, targetProductId) in lstProducts)
            {
                // Find source and target product codes from orgProductList
                var sourceProduct = orgProductList.FirstOrDefault(p => p.ProductId == sourceProductId);
                var targetProduct = allProductList.FirstOrDefault(p => p.ProductId == targetProductId);

                if (sourceProduct != null && targetProduct != null)
                {
                    // Update ProductCode for all ProductDetail entries with the source code
                    foreach (var pd in productList.Where(p => string.Equals(p.ProductCode, sourceProduct.BooksProductCode, StringComparison.OrdinalIgnoreCase)))
                    {
                        pd.ProductCode = targetProduct.BooksProductCode;
                    }
                }
            }

            return productList;
        }

        public List<string> ValidateProductData(IList<ProductDetail> productList)
		{
			var prodRepository = new ProductRepository(_userClaims);
			List<string> productData = new List<string>();
            productList = GetProductSharedWithOtherProduct(productList);

            foreach (var product in productList)
			{
				var productMap = prodRepository.GetBooksMasterProductDetail(product.ProductCode.ToUpper());
				if (productMap == null)
				{
					productData.Add($"Product with code {product.ProductCode} is incorrect.");
				}

				var productId = productMap.ProductId;
				// OPS requires one role & one property
				if (productId == (int)ProductEnum.OpsBuyer)
				{
					var propertyId = product.PropertiesAssigned.FirstOrDefault();
					var roleId = product.RolesAssigned.FirstOrDefault();

					// validate if propertyId exists for product
					IManageProductOps manageProductOps = new ManageProductOps(_userClaims);
					ListResponse productResponse = manageProductOps.GetCompanyAssets(_userClaims.PersonaId, 0, false, null);
					if (!productResponse.IsError)
					{
						List<AssetGroup> opsFilteredAssetList = productResponse.Records.Cast<AssetGroup>().ToList();
						if (opsFilteredAssetList.All(o => o.ID != propertyId))
						{
                            productData.Add($"Product with code {product.ProductCode} has invalid property Id - {propertyId}");
						}
					}
					else 
					{
                        productData.Add($"Product with code {product.ProductCode} has invalid property Id - {propertyId}");
                    }

					// validate if roleId exists for product 
					productResponse = manageProductOps.GetRoles(_userClaims.PersonaId, 0, "", null);
					if (!productResponse.IsError)
					{
						var filteredList = productResponse.Records.Cast<SharedObjects.Product.ProductRole>().ToList();
						if (filteredList.All(x => x.ID != roleId))
						{
                            productData.Add($"Product with code {product.ProductCode} has invalid role Id - {roleId}");
						}
					}
                    else
                    {
                        productData.Add($"Product with code {product.ProductCode} has invalid role Id - {roleId}");
                    }
                }
			}

			// all ok
			return productData;
		}

		#endregion
	}
}
