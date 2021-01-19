using System;
using System.Collections.Generic;
using System.Linq;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.ProductIntegration.Factory;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.ProductIntegration.Helpers;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.ProductIntegration.Model;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.ProductIntegration.Model.ClickPay;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Base;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.ProductIntegration.ProductImplementation
{
	public sealed class ClickPayManagement : ManageProductInvokerBase, IManageProductIntegration
	{
		#region Ctor
		private IManagePersona _managePersona;
		private const string PRODUCT_SETTINGTYPE_STATUS = "ProductStatus";
		public ClickPayManagement(ProductEnum productType, long editorPersonaId, long subjectPersonaId, DefaultUserClaim userClaims) : base(productType, editorPersonaId, subjectPersonaId, userClaims)
		{ }
		public ClickPayManagement(ProductEnum productType, long editorPersonaId, long subjectPersonaId, DefaultUserClaim userClaims, IDataCollector injectedDataCollector, IManagePersona injectedManagePersona, IProductInternalSettingRepository productInternalSettingRepository) :
			base(productType, editorPersonaId, subjectPersonaId, userClaims, injectedDataCollector, injectedManagePersona, productInternalSettingRepository)
		{ }

		#endregion

		public override ListResponse GetProductRoles(RequestParameter dataFilter, string baseUrlAndQuery = null)
		{
			try
			{
				WriteToDiagnosticLog(
					$"ClickPayManagement.GetProductRoles - editorPersona id - {EditorUserDetails.PersonaId}. At beginning of the method.");

				baseUrlAndQuery = string.Format(GetOperationEndPoint(ProductEntityEndpointKeyEnum.GetRoleEndpoint), CompanyInstanceSourceId);

				WriteToDiagnosticLog(
					$"ClickPayManagement.GetProductRoles - editorPersona id - {EditorUserDetails.PersonaId}. At API calling - {baseUrlAndQuery}");

				var roleList = GetResultFromApi<ClickPayRoles>(baseUrlAndQuery).ClickPayRoleList;

                // The OrgsAssignedCount received from product is not correct, so resetting the count to 0
                WriteToDiagnosticLog(
                       $"ClickPayManagement.GetProductRoles - editorPersona id - {EditorUserDetails.PersonaId}. Resetting OrgsAssignedCount to 0 ");

                foreach (var item in roleList)
                {
                    item.OrgsAssignedCount = 0;
					item.IsAssigned = false;
                }

                // Get Orgs assigned to Role count for User
                if (!string.IsNullOrEmpty(SubjectUserDetails?.ProductUserName))
                {
                    WriteToDiagnosticLog(
                        $"ClickPayManagement.GetProductRoles - editorPersona id - {EditorUserDetails.PersonaId}. Calling GetProductUser for subject persona Id -{SubjectUserDetails.PersonaId}");

                    var user = GetProductUser();

                    if (user != null)
                    {
                        WriteToDiagnosticLog(
                            $"ClickPayManagement.GetProductRoles - editorPersona id - {EditorUserDetails.PersonaId}. Calling Merge OrgsAssignedCount for subject persona Id -{SubjectUserDetails.PersonaId}");

                        foreach (var item in roleList)
                        {
                            item.OrgsAssignedCount = user.OrganizationRoles.FindAll(f => f.RoleId == item.Id).Count;
							if (item.OrgsAssignedCount > 0)
							{
								item.IsAssigned = true;
								var selectedItemsObj = GetProductOrganizations(item.Id, item.OrgType, null).Records;
								//item.SelectedItems = new List<ClickPaySelectedItems>();
								item.SelectedItems = selectedItemsObj.Cast<ClickPayOrganization>().Where(x => x.IsAssigned == true)
													.Select(y => new ClickPaySelectedItems() { Id = y.Id, Value = y.IsAssigned })
													.ToList();

							}
								
                        }

                    }
                }

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
				WriteToErrorLog($"ClickPayManagement.GetProductRoles - editorPersona id - {EditorUserDetails.PersonaId}. Error - {ex.Message}", null, ex);
				return new ListResponse()
				{
					ErrorReason = ex.Message,
					IsError = true
				};
			}
		}
		public override ListResponse GetProductOrganizations(string organizationRoleId, string organizationType, RequestParameter dataFilter, string baseUrlAndQuery = null)
		{
			try
			{
				WriteToDiagnosticLog(
					$"ClickPayManagement.GetProductOrganizations - editorPersona id - {EditorUserDetails.PersonaId}. At beginning of the method.");

				//baseUrlAndQuery = string.Format(GetOperationEndPoint(ProductEntityEndpointKeyEnum.GetParentCompanyEndpoint), CompanyInstanceSourceId);

				//WriteToDiagnosticLog(
				//	$"ClickPayManagement.GetProductOrganizations - Product {ProductType} editorPersona id - {EditorUserDetails.PersonaId}. At API calling - {baseUrlAndQuery}");

				List<ClickPayOrganization> returnOrgList;
				if (string.Equals(organizationType, "company", StringComparison.CurrentCultureIgnoreCase))
				{
                    baseUrlAndQuery = string.Format(GetOperationEndPoint(ProductEntityEndpointKeyEnum.GetCompanyEndpoint), "");

                    WriteToDiagnosticLog(
                        $"ClickPayManagement.GetProductOrganizations - editorPersona id - {EditorUserDetails.PersonaId}. At API calling - {baseUrlAndQuery}");

                   
                    var allOrganizationList = GetResultFromApi<ClickPayOrganizations>(baseUrlAndQuery)
                        .ClickPayOrganizationList;

                    if (allOrganizationList == null)
                        throw new Exception("Null Org List.");

                    returnOrgList =
                      allOrganizationList.FindAll(x => x.Type.ToUpper() == organizationType.ToUpper());

                    WriteToDiagnosticLog(
                        $"ClickPayManagement.GetProductOrganizations - editorPersona id - {EditorUserDetails.PersonaId}. Received roleList with count = {returnOrgList.Count}");
                }
				else
				{
                    baseUrlAndQuery = string.Format(GetOperationEndPoint(ProductEntityEndpointKeyEnum.GetParentCompanyEndpoint), CompanyInstanceSourceId);

                    WriteToDiagnosticLog(
                        $"ClickPayManagement.GetProductOrganizations - editorPersona id - {EditorUserDetails.PersonaId}. At API calling - {baseUrlAndQuery}");

                    var allOrganizationList = GetResultFromApi<ClickPayOrganizations>(baseUrlAndQuery)
						.ClickPayOrganizationList;

					if (allOrganizationList == null)
						throw new Exception("Null Org List.");

					returnOrgList =
					  allOrganizationList.FindAll(x => x.Type.ToUpper() == organizationType.ToUpper());

					WriteToDiagnosticLog(
						$"ClickPayManagement.GetProductOrganizations - editorPersona id - {EditorUserDetails.PersonaId}. Received roleList with count = {returnOrgList.Count}");

					if (returnOrgList.Count > 1)
					{
						if (organizationType.ToUpper().Equals("OWNER"))
						{
							foreach (var org in returnOrgList)
							{
								org.SiteList = allOrganizationList.FindAll(x =>
										string.Equals(x.ParentCompanyId, org.Id,StringComparison.CurrentCultureIgnoreCase)
									 && string.Equals(x.Type, "site", StringComparison.CurrentCultureIgnoreCase))
									.Select(i => new ProductProperties { SetPropertyId = i.Id, SetName = i.Name }).ToList();
							}
						}

						if (organizationType.ToUpper().Equals("SITE"))
						{
							foreach (var org in returnOrgList)
							{
								org.LlcName = allOrganizationList.Find(x =>
									string.Equals(x.Id, org.ParentCompanyId, StringComparison.CurrentCultureIgnoreCase)
									&& string.Equals(x.Type, "owner", StringComparison.CurrentCultureIgnoreCase))?.Name;
							}
						}
					}
				}
				if(returnOrgList.Count > 0)
				{
					returnOrgList.ForEach(x => x.IsAssigned = false);
				}
				if (!string.IsNullOrEmpty(SubjectUserDetails?.ProductUserName) && returnOrgList.Count > 0)
				{
					WriteToDiagnosticLog(
						$"ClickPayManagement.GetProductOrganizations - editorPersona id - {EditorUserDetails.PersonaId}. Calling GetUser for subject persona Id -{SubjectUserDetails.PersonaId}");

					var user = GetProductUser();

					// map user roles
					if (user != null)
					{
						WriteToDiagnosticLog(
							$"ClickPayManagement.GetProductOrganizations - editorPersona id - {EditorUserDetails.PersonaId}. Calling Merge for subject persona Id -{SubjectUserDetails.PersonaId}");

						MergeUserOrganizations(returnOrgList, user.OrganizationRoles, organizationType,
							organizationRoleId);
					}
				}

				return new ListResponse
				{
					Records = returnOrgList.Cast<object>().ToList(),
					TotalRows = returnOrgList.Count,
					RowsPerPage = 9999,
					ErrorReason = string.Empty,
					TotalPages = 1
				};
			}
			catch (Exception ex)
			{
				WriteToErrorLog($"ClickPayManagement.GetProductOrganizations - editorPersona id - {EditorUserDetails.PersonaId}. Error - {ex.Message}", null, ex);
				return new ListResponse()
				{
					ErrorReason = ex.Message,
					IsError = true
				};
			}
		}
		public override IntegrationProductUser GetProductUser(string baseUrlAndQuery = null, bool isThrowOnError = true)
		{
			WriteToDiagnosticLog(
				$"ClickPayManagement.GetProductUser - editorPersona id - {EditorUserDetails.PersonaId}. At beginning of the method.");

			// Get partial api query based on end point
			if (string.IsNullOrEmpty(baseUrlAndQuery))
				baseUrlAndQuery = string.Format(GetOperationEndPoint(ProductEntityEndpointKeyEnum.GetUserEndpoint), SubjectUserDetails.ProductUserName);

			WriteToDiagnosticLog(
				$"ClickPayManagement.GetProductUser - editorPersona id - {EditorUserDetails.PersonaId}. Calling API - {baseUrlAndQuery}.");

			var users = GetResultFromApi<ClickPayUsers>(baseUrlAndQuery, isThrowOnError);
			if (users?.ClickPayUserList != null && users.ClickPayUserList.Count > 0)
				return users.ClickPayUserList[0];

			return null;
		}
		protected override bool CheckUserExistInProduct(string loginNameToCheck, string baseUrlAndQuery = null)
		{
			baseUrlAndQuery = string.Format(GetOperationEndPoint(ProductEntityEndpointKeyEnum.GetUserEndpoint), loginNameToCheck, CompanyInstanceSourceId);
			return base.CheckUserExistInProduct("", baseUrlAndQuery);
		}
		protected override IntegrationProductUser GenerateProductUserObject(ProductUserRolePropertiesGroups changedUserRolePropertiesRegion)
		{
			List<OrganizationRole> productUserOrgRoleList = new List<OrganizationRole>();
			IntegrationProductUser user = new IntegrationProductUser();
			if (changedUserRolePropertiesRegion.OrganizationRoleList != null)
			{
				foreach (var changedUserOrgRoles in changedUserRolePropertiesRegion.OrganizationRoleList)
				{
					if (changedUserOrgRoles.IsAssigned)
					{
						if (changedUserOrgRoles.RoleType.ToString().ToLower().Equals("company"))
						{
							changedUserOrgRoles.OrganizationId = CompanyInstanceSourceId;
						}
					}
				}
			}

			if (!string.IsNullOrEmpty(SubjectUserDetails.ProductUserName))
			{
				user = GetProductUser();
				if(changedUserRolePropertiesRegion.OrganizationRoleList != null)
				{
					foreach (var changedUserOrgRoles in changedUserRolePropertiesRegion.OrganizationRoleList)
					{
						if (changedUserOrgRoles.IsAssigned)
						{
							productUserOrgRoleList.Add(new OrganizationRole
							{
								OrganizationId = changedUserOrgRoles.OrganizationId,
								RoleId = changedUserOrgRoles.RoleId
							});
						}
					}
				}				
			}
			else
			{
				productUserOrgRoleList = changedUserRolePropertiesRegion.OrganizationRoleList;
			}

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
				PropertyGroups = changedUserRolePropertiesRegion.PropertyGroupList,
				Properties = changedUserRolePropertiesRegion.PropertyList,
				Roles = changedUserRolePropertiesRegion.RoleList?.ConvertAll<string>(x => x.ToString()),
				PropertyRoles = changedUserRolePropertiesRegion.PropertyRoleList,
				OrganizationRoles = productUserOrgRoleList,//changedUserRolePropertiesRegion.OrganizationRoleList,
				CanReceiveMonthlyReport = changedUserRolePropertiesRegion.CanReceiveMonthlyReport,
				IsMigratedUser = true,
				UserId = user?.UserId
            };

			if (SubjectUserDetails.UserRoleTypeId == (int)UserRoleType.SuperUser)
			{
				var roles = GetProductRoles(null, "").Records.Cast<ClickPayRole>();
				var role = roles.FirstOrDefault(x => x.Name.ToUpperInvariant() == "MANAGEMENT ADMIN");
				var orgrole = new OrganizationRole() { OrganizationId = productUser.CompanyId, RoleId = role.Id, IsAssigned = true };
				productUser.OrganizationRoles = new List<OrganizationRole>();
				productUser.OrganizationRoles.Add(orgrole);
				ApplySuperUserData(productUser);
			}

			return productUser;
		}
		public override ListResponse GetMigrationUsers(RequestParameter datafilter)
		{
			var response = new ListResponse()
			{
				IsError = true,
				ErrorReason = "No Users."
			};

			var filter = "NonMigrated";
			var startRow = 0;
			var resultPerRow = 1000;
			if (datafilter != null)
			{
				if (datafilter.FilterBy.ContainsKey("filter"))
				{
					filter = datafilter.FilterBy["filter"];
				}
				if (datafilter.Pages != null)
				{
					startRow = datafilter.Pages.StartRow;
					resultPerRow = datafilter.Pages.ResultsPerPage;
				}
			}
			var baseUrlAndQuery = GetOperationEndPoint(ProductEntityEndpointKeyEnum.GetListUsersEndpoint);

			baseUrlAndQuery = string.Format(baseUrlAndQuery, CompanyInstanceSourceId, filter, startRow, resultPerRow);

			// dump API call info
			DumpApiCallInfoToDiagnosticLog(baseUrlAndQuery);

			var integration = new ApiIntegration(_httpClient, baseUrlAndQuery);
			var result = integration.GetEntityFromApi<ClickPayUsers>().ClickPayUserList;

			if (result == null)
			{
				WriteToErrorLog($"ClickPayManagement.GetMigrationUsers - no users received from product.");
				return response;
			}

			WriteToDiagnosticLog($"ClickPayManagement.GetMigrationUsers - Received users from product.");
			response.RowsPerPage = resultPerRow;
			response.ErrorReason = string.Empty;
			response.IsError = false;
			response.TotalPages = 1;
			response.Records = result.Cast<object>().ToList();
			response.TotalRows = result.Count();
			return response;
		}

		public override string UnassignUser()
		{
			WriteToDiagnosticLog(
				$"ClickPayManagement.UnassignUser - editorPersona id - {EditorUserDetails.PersonaId}. At beginning of the method, calling DeleteUser().");
			var clickpayProductUser = GetProductUser();
			clickpayProductUser.IsActive = false;
			
			// Delete / deactivate uer in the product
			var result = DeleteUser(clickpayProductUser);

			if (result.IsSuccessStatusCode)
			{
				WriteToDiagnosticLog(
					$"ClickPayManagement.UnassignUser - editorPersona id - {EditorUserDetails.PersonaId}. DeleteUser() returns success, updating Greenboook status.");

				IManageUserLogin manageUserLogin = new ManageUserLogin();
				IUserLoginRepository userLoginRepository = new UserLoginRepository();
				if (_managePersona == null)
					_managePersona = new ManagePersona();

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
				_dataCollector.UpdateProductSettingProductStatus(SubjectUserDetails.PersonaId, PRODUCT_SETTINGTYPE_STATUS, ProductId, statusValue);

				// Activity Logging
				ProductActivityLogger.WriteUnassignUserActivityLog(EditorUserDetails, SubjectUserDetails, BlueBookGbProductMap.Name, BlueBookGbProductMap.BooksProductCode, CorrelationId);

				return string.Empty;
			}

			Dictionary<string, object> logData = new Dictionary<string, object> { { "result", result } };
			WriteToErrorLog($"ClickPayManagement.UnassignUser - editorPersona id - {EditorUserDetails.PersonaId}. DeleteUser() returns fail", logData);

			return result.Content;
		}

		protected override ApiResponse ProductUserProfileChange(ProductUserProfile productUserProfile)
		{
			WriteToDiagnosticLog(
				$"ClickPayManagement.ProductUserProfileChange - editorPersona id - " +
				$"{EditorUserDetails.PersonaId}, productUserProfile.UserId - {productUserProfile.UserId}. At beginning of the method.");

			var clickpayProductUser = GetProductUser();
			clickpayProductUser.LoginName = SubjectUserDetails.ProductUserName;
			clickpayProductUser.FirstName = SubjectUserDetails.FirstName;
			clickpayProductUser.MiddleName = SubjectUserDetails.MiddleName;
			clickpayProductUser.LastName = SubjectUserDetails.LastName;
			clickpayProductUser.Email = SubjectUserDetails.Email;
			clickpayProductUser.PhoneNumbers = SubjectUserDetails.PhoneNumbers;
			clickpayProductUser.Phone = SubjectUserDetails.PhoneNumber;
			clickpayProductUser.IsActive = true;
			clickpayProductUser.UserId = SubjectUserDetails.ProductUserId;
			clickpayProductUser.CompanyId = CompanyInstanceSourceId;

			var baseUrlAndQuery = GetOperationEndPoint(ProductEntityEndpointKeyEnum.PutUserEndpoint);

			WriteToDiagnosticLog(
				$"ClickPayManagement.ProductUserProfileChange - editorPersona id - " +
				$"{EditorUserDetails.PersonaId}  productUserProfile.UserId - {productUserProfile.UserId}. Calling API - {baseUrlAndQuery}.");

			// dump API call info
			DumpApiCallInfoToDiagnosticLog(baseUrlAndQuery, clickpayProductUser);

			var integration = new ApiIntegration(_httpClient, baseUrlAndQuery);
			return integration.PutEntity<ProductUserProfile>(clickpayProductUser);
		}

		private ApiResponse DeleteUser(IntegrationProductUser profile = null)
		{
			WriteToDiagnosticLog(
				$"ClickPayManagement.DeleteUser - editorPersona id - {EditorUserDetails.PersonaId}. At beginning of the method.");

			// patch to se isActive flag to false
			var baseUrlAndQuery = GetOperationEndPoint(ProductEntityEndpointKeyEnum.PutUserEndpoint);

			WriteToDiagnosticLog(
				$"ClickPayManagement.DeleteUser - editorPersona id - {EditorUserDetails.PersonaId}. Calling API - {baseUrlAndQuery}.");

			DumpApiCallInfoToDiagnosticLog(baseUrlAndQuery, profile);

			var integration = new ApiIntegration(_httpClient, baseUrlAndQuery);
			return integration.PutEntity<string>(profile);
		}

		private void MergeUserOrganizations(List<ClickPayOrganization> orgList, List<OrganizationRole> userOrganizationRoles, string orgType, string orgRoleId)
		{
			foreach (var userOrganizationRole in userOrganizationRoles)
			{
				if (userOrganizationRole.RoleId.ToUpper() == orgRoleId.ToUpper())
				{
					orgList.Find(x => x.Id == userOrganizationRole.OrganizationId && x.Type.ToUpper() == orgType.ToUpper()).IsAssigned = true;
				}
			}
		}

		/// <summary>
		/// Create or update product user
		/// Gets called from Product-Batch
		/// </summary> 
		public override string CreateUpdateProductUser(ProductUserRolePropertiesGroups userRolePropertiesRegion, BatchProcessType batchProcessType = BatchProcessType.CreateUpdateProductUser)
		{
			string result;
			WriteToDiagnosticLog($"ClickPayManagement.CreateUpdateProductUser - editorPersona id - {EditorUserDetails.PersonaId}. At beginning of method.");

			// Get product user object 
			var newProductUser = GenerateProductUserObject(userRolePropertiesRegion);

			if (string.IsNullOrEmpty(SubjectUserDetails.ProductUserName))
			{
				WriteToDiagnosticLog($"ClickPayManagement.CreateUpdateProductUser - editorPersona id - {EditorUserDetails.PersonaId}. Calling CreateUser.");
				if (CheckUserExistInProduct(newProductUser.LoginName))
				{
					//Multi Company user. Get the user from product and combine old new and old company roles
					string baseUrlAndQuery = string.Format(GetOperationEndPoint(ProductEntityEndpointKeyEnum.GetUserEndpoint), newProductUser.LoginName, CompanyInstanceSourceId);
					var productUser = GetProductUser(baseUrlAndQuery, false);
					if (productUser.OrganizationRoles != null && productUser.OrganizationRoles.Count > 0)
					{
						newProductUser.OrganizationRoles.AddRange(productUser.OrganizationRoles);
					}
					result = UpdateUser(newProductUser, batchProcessType);
				}
				else
				{
					// Create User
					result = CreateUser(newProductUser);
				}
			}
			else
			{
				WriteToDiagnosticLog(
					$"ManageProductInvokerBase.CreateUpdateProductUser - Product {ProductType} editorPersona id - {EditorUserDetails.PersonaId}. Calling UpdateUser.");
				// Update user with Id/Login from product
				newProductUser.UserId = SubjectUserDetails.ProductUserId;
				newProductUser.LoginName = SubjectUserDetails.ProductUserName;

				result = UpdateUser(newProductUser, batchProcessType);
			}
			return result;
		}
	}
}