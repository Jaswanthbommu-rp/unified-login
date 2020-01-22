using System;
using System.Collections.Generic;
using System.Linq;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.ProductIntegration.Factory;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.ProductIntegration.Helpers;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.ProductIntegration.Model;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.ProductIntegration.Model.ClickPay;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Base;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.ProductIntegration.ProductImplementation
{
	public sealed class ClickPayManagement : ManageProductInvokerBase, IManageProductIntegration
	{
		#region Ctor

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
					$"ClickPayManagement.GetProductRoles - Product {ProductType} editorPersona id - {EditorUserDetails.PersonaId}. At beginning of the method.");

				baseUrlAndQuery = string.Format(GetOperationEndPoint(ProductEntityEndpointKeyEnum.GetRoleEndpoint), CompanyInstanceSourceId);

				WriteToDiagnosticLog(
					$"ClickPayManagement.GetProductRoles - Product {ProductType} editorPersona id - {EditorUserDetails.PersonaId}. At API calling - {baseUrlAndQuery}");

				var roleList = GetResultFromApi<ClickPayRoles>(baseUrlAndQuery).ClickPayRoleList;

                // The OrgsAssignedCount received from product is not correct, so resetting the count to 0
                WriteToDiagnosticLog(
                       $"ClickPayManagement.GetProductRoles - Product {ProductType} editorPersona id - {EditorUserDetails.PersonaId}. Resetting OrgsAssignedCount to 0 ");

                foreach (var item in roleList)
                {
                    item.OrgsAssignedCount = 0;
                }

                // Get Orgs assigned to Role count for User
                if (!string.IsNullOrEmpty(SubjectUserDetails?.ProductUserName))
                {
                    WriteToDiagnosticLog(
                        $"ClickPayManagement.GetProductRoles - Product {ProductType} editorPersona id - {EditorUserDetails.PersonaId}. Calling GetProductUser for subject persona Id -{SubjectUserDetails.PersonaId}");

                    var user = GetProductUser();

                    if (user != null)
                    {
                        WriteToDiagnosticLog(
                            $"ClickPayManagement.GetProductRoles - Product {ProductType} editorPersona id - {EditorUserDetails.PersonaId}. Calling Merge OrgsAssignedCount for subject persona Id -{SubjectUserDetails.PersonaId}");

                        foreach (var item in roleList)
                        {
                            item.OrgsAssignedCount = user.OrganizationRoles.FindAll(f => f.RoleId == item.Id).Count;
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
				WriteToErrorLog($"ClickPayManagement.GetProductRoles - Product {ProductType} editorPersona id - {EditorUserDetails.PersonaId}. Error - {ex.Message}", null, ex);
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
					$"ClickPayManagement.GetProductOrganizations - Product {ProductType} editorPersona id - {EditorUserDetails.PersonaId}. At beginning of the method.");

				//baseUrlAndQuery = string.Format(GetOperationEndPoint(ProductEntityEndpointKeyEnum.GetParentCompanyEndpoint), CompanyInstanceSourceId);

				//WriteToDiagnosticLog(
				//	$"ClickPayManagement.GetProductOrganizations - Product {ProductType} editorPersona id - {EditorUserDetails.PersonaId}. At API calling - {baseUrlAndQuery}");

				List<ClickPayOrganization> returnOrgList;
				if (string.Equals(organizationType, "company", StringComparison.CurrentCultureIgnoreCase))
				{
                    baseUrlAndQuery = string.Format(GetOperationEndPoint(ProductEntityEndpointKeyEnum.GetCompanyEndpoint), "");

                    WriteToDiagnosticLog(
                        $"ClickPayManagement.GetProductOrganizations - Product {ProductType} editorPersona id - {EditorUserDetails.PersonaId}. At API calling - {baseUrlAndQuery}");

                   
                    var allOrganizationList = GetResultFromApi<ClickPayOrganizations>(baseUrlAndQuery)
                        .ClickPayOrganizationList;

                    if (allOrganizationList == null)
                        throw new Exception("Null Org List.");

                    returnOrgList =
                      allOrganizationList.FindAll(x => x.Type.ToUpper() == organizationType.ToUpper());

                    WriteToDiagnosticLog(
                        $"ClickPayManagement.GetProductOrganizations - Product {ProductType} editorPersona id - {EditorUserDetails.PersonaId}. Received roleList with count = {returnOrgList.Count}");
                }
				else
				{
                    baseUrlAndQuery = string.Format(GetOperationEndPoint(ProductEntityEndpointKeyEnum.GetParentCompanyEndpoint), CompanyInstanceSourceId);

                    WriteToDiagnosticLog(
                        $"ClickPayManagement.GetProductOrganizations - Product {ProductType} editorPersona id - {EditorUserDetails.PersonaId}. At API calling - {baseUrlAndQuery}");

                    var allOrganizationList = GetResultFromApi<ClickPayOrganizations>(baseUrlAndQuery)
						.ClickPayOrganizationList;

					if (allOrganizationList == null)
						throw new Exception("Null Org List.");

					returnOrgList =
					  allOrganizationList.FindAll(x => x.Type.ToUpper() == organizationType.ToUpper());

					WriteToDiagnosticLog(
						$"ClickPayManagement.GetProductOrganizations - Product {ProductType} editorPersona id - {EditorUserDetails.PersonaId}. Received roleList with count = {returnOrgList.Count}");

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

				if (!string.IsNullOrEmpty(SubjectUserDetails?.ProductUserName) && returnOrgList.Count > 0)
				{
					WriteToDiagnosticLog(
						$"ClickPayManagement.GetProductOrganizations - Product {ProductType} editorPersona id - {EditorUserDetails.PersonaId}. Calling GetUser for subject persona Id -{SubjectUserDetails.PersonaId}");

					var user = GetProductUser();

					// map user roles
					if (user != null)
					{
						WriteToDiagnosticLog(
							$"ClickPayManagement.GetProductOrganizations - Product {ProductType} editorPersona id - {EditorUserDetails.PersonaId}. Calling Merge for subject persona Id -{SubjectUserDetails.PersonaId}");

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
				WriteToErrorLog($"ClickPayManagement.GetProductOrganizations - Product {ProductType} editorPersona id - {EditorUserDetails.PersonaId}. Error - {ex.Message}", null, ex);
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
				$"ClickPayManagement.GetProductUser - Product {ProductType} editorPersona id - {EditorUserDetails.PersonaId}. At beginning of the method.");

			// Get partial api query based on end point
			if (string.IsNullOrEmpty(baseUrlAndQuery))
				baseUrlAndQuery = string.Format(GetOperationEndPoint(ProductEntityEndpointKeyEnum.GetUserEndpoint), SubjectUserDetails.ProductUserName);

			WriteToDiagnosticLog(
				$"ClickPayManagement.GetProductUser - Product {ProductType} editorPersona id - {EditorUserDetails.PersonaId}. Calling API - {baseUrlAndQuery}.");

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
			List<OrganizationRole> productUserOrgRoleList;
			if (!string.IsNullOrEmpty(SubjectUserDetails.ProductUserName))
			{
				var user = GetProductUser();
				productUserOrgRoleList = user.OrganizationRoles;

				foreach (var changedUserOrgRoles in changedUserRolePropertiesRegion.OrganizationRoleList)
				{
					if (changedUserOrgRoles.IsAssigned)
					{
						if (!productUserOrgRoleList.Exists(x =>
							x.OrganizationId == changedUserOrgRoles.OrganizationId &&
							x.RoleId == changedUserOrgRoles.RoleId))
						{
							// add new role
							productUserOrgRoleList.Add(new OrganizationRole
							{
								OrganizationId = changedUserOrgRoles.OrganizationId,
								RoleId = changedUserOrgRoles.RoleId
							});
						}
					}
					else if (!changedUserOrgRoles.IsAssigned)
					{
                        // remove role
                        //productUserOrgRoleList.Remove(changedUserOrgRoles);
                        productUserOrgRoleList.RemoveAll(x => x.OrganizationId == changedUserOrgRoles.OrganizationId && x.RoleId == changedUserOrgRoles.RoleId);
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
                IsMigratedUser = true
            };

			if (SubjectUserDetails.UserRoleTypeId == (int)UserRoleType.SuperUser)
			{

                //if(productUser.OrganizationRoles == null)
                //{
                //   var roles = GetProductRoles(null, "").Records.Cast<ClickPayRole>();
                //   var role = roles.FirstOrDefault(x => x.Name.ToUpperInvariant() == "MANAGEMENT SUPER ADMIN");
                    
                //    var orgrole = new OrganizationRole() { OrganizationId = productUser.CompanyId, RoleId = role.Id, IsAssigned = true };
                //    productUser.OrganizationRoles = new List<OrganizationRole>();
                //    productUser.OrganizationRoles.Add(orgrole);
                //}

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
	}
}