using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics.CodeAnalysis;
using UnifiedLogin.SharedObjects.IdentityConfig;

namespace UnifiedLogin.LandingAPI.Test.Logic.ProductIntegration
{
	[ExcludeFromCodeCoverage]
	public static class ClickPayTestData
	{
		public static string Environment { get; } = ConfigurationManager.AppSettings["Environment"];
		public static string CompanyInstanceSourceId { get; set; }
		public static string SubjectUserLoginName { get; set; }
		public static string UpdateInputJson { get; set; }
		static ClickPayTestData()
		{
			//if (Environment.Equals("QA"))
			//	InitQa();

			InitDev();
		}
		public static void InitDev()
		{
			CompanyInstanceSourceId = "31978F5C-4C5A-4DA4-9E5F-49EB748E3952";
			SubjectUserLoginName = "siteOwnerUser";
			UpdateInputJson = "{\"IsAssigned\":true,\"CompanyId\":0,\"OrganizationRoleList\":[{\"OrganizationId\":\"98eb8a26-c821-4601-8598-629b1caab702\",\"RoleId\":\"00000000-0000-0000-0000-000000000300\"},{\"OrganizationId\":\"59e183c4-1413-4674-8ba6-e7ed3d62b3b9\",\"RoleId\":\"00000000-0000-0000-0000-000000000400\"}]} ";
		}

		public static List<ProductInternalSetting> Get_DEV_CPAY_ProductSettings()
		{
			return new List<ProductInternalSetting>
				{
					new ProductInternalSetting { Name = "ClientId", Value = "1" },
					new ProductInternalSetting { Name = "ClassName", Value = "payments" },
					new ProductInternalSetting { Name = "ProductUrl", Value = "/product/payments" },
					new ProductInternalSetting { Name = "TitleId", Value = "Payments" },
					new ProductInternalSetting { Name = "TitleUniqueId", Value = "99d1a2c6-daa2-4b95-8a00-550d7f3442eb" },
					new ProductInternalSetting { Name = "IsNewTab", Value = "1" },
					new ProductInternalSetting { Name = "MetatagUniqueId", Value = "Payments" },
					new ProductInternalSetting { Name = "IsResource", Value = "0" },
					new ProductInternalSetting { Name = "IsFavorite", Value = "1" },
					new ProductInternalSetting { Name = "LearnMore", Value = "https://www.realpage.com/" },
					new ProductInternalSetting { Name = "ApiEndPoint", Value = "https://rfqa.novelpay.com/api/realpage/bluebook" },
					new ProductInternalSetting { Name = "ApiUserName", Value = "test+rfqarpunity@novelpay.com" },
					new ProductInternalSetting { Name = "ApiPassword", Value = "7mgp43EIvc8c!@" },
					new ProductInternalSetting { Name = "ApiKey", Value = "dGVzdCtyZnFhcnB1bml0eUBub3ZlbHBheS5jb206N21ncDQzRUl2YzhjIUA=" },
					new ProductInternalSetting { Name = "ProductStatus", Value = "8" },
					new ProductInternalSetting { Name = "ProductStatus", Value = "8" },
					new ProductInternalSetting { Name = "ShowInUserDetails", Value = "1" },
					new ProductInternalSetting { Name = "ShowInRolesAndRights", Value = "0" },
					new ProductInternalSetting { Name = "ShowInAppSwitcher", Value = "1" },
					new ProductInternalSetting { Name = "ShowInUserListFilter", Value = "1" },
					new ProductInternalSetting { Name = "ProductAPIRequiresUser", Value = "0" },
					new ProductInternalSetting { Name = "GetRoleEndpoint", Value = "/roles?orgId={0}" },
					new ProductInternalSetting { Name = "GetProfileEndpoint", Value = "/users/{0}/profile" },
					new ProductInternalSetting { Name = "GetUserEndpoint", Value = "/users?login={0}" },
					new ProductInternalSetting { Name = "PostUserEndpoint", Value = "/users" },
					new ProductInternalSetting { Name = "PutUserEndpoint", Value = "/users" },
					new ProductInternalSetting { Name = "DeleteUserEndpoint", Value = "/users/[0]" },
					new ProductInternalSetting { Name = "PatchProfileEndpoint", Value = "/users/{0}/profile" },
					new ProductInternalSetting { Name = "GetListUsersEndpoint", Value = "/users?login={0}&orgId={1}" },
					new ProductInternalSetting { Name = "PatchMigrateUsersEndpoint", Value = "/users/{0}/migrate" },
					new ProductInternalSetting { Name = "LockOnProductAccess", Value = "0" },
					new ProductInternalSetting { Name = "ProductNotAvailableForRegularUserNoEmail", Value = "0" },
					new ProductInternalSetting { Name = "AuthenticationType", Value = "Redirect" },
					new ProductInternalSetting { Name = "GetCompanyEndpoint", Value = "/orgs" },
					new ProductInternalSetting { Name = "GetParentCompanyEndpoint", Value = "/orgs?parentOrgId={0}" }
			 };
		}
		public static void InitQa()
		{
			throw new NotImplementedException();
		}
	}
}
