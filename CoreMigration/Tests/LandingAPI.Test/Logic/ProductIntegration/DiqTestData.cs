using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics.CodeAnalysis;
using UnifiedLogin.SharedObjects.IdentityConfig;

namespace UnifiedLogin.LandingAPI.Test.Logic.ProductIntegration
{
	[ExcludeFromCodeCoverage]
	public static class DiqTestData
	{

		public static string Environment { get; } = ConfigurationManager.AppSettings["Environment"];
		public static string CompanyInstanceSourceId { get; set; }
		public static string SubjectUserLoginName { get; set; }
		public static string UpdateInputJson { get; set; }

		static DiqTestData()
		{
			//if (Environment.Equals("QA"))
			//	InitQa();

			InitDev();
		}

		public static void InitDev()
		{
			CompanyInstanceSourceId = "108";
			SubjectUserLoginName = "da0701@test.com";
			UpdateInputJson = "{\"PropertyList\":[\"123\",\"124\",\"125\"],\"RoleList\":[\"agent\"],\"PropertyGroupList\":[],\"IsAssigned\":true,\"CompanyId\":0,\"CanReceiveMonthlyReport\":false}";
		}

		public static List<ProductInternalSetting> Get_DEV_DIQ_ProductSettings()
		{
			return new List<ProductInternalSetting>
				{
					new ProductInternalSetting { Name = "ClientId", Value = "1" },
					new ProductInternalSetting { Name = "ClassName", Value = "depositalternative" },
					new ProductInternalSetting { Name = "ProductUrl", Value = "/product/depositalternative" },
					new ProductInternalSetting { Name = "TitleId", Value = "Deposit Alternative" },
					new ProductInternalSetting { Name = "TitleUniqueId", Value = "082850ad-415a-454b-ab6a-7ee22d4550f7" },
					new ProductInternalSetting { Name = "IsNewTab", Value = "1" },
					new ProductInternalSetting { Name = "MetatagUniqueId", Value = "DepositAlternative" },
					new ProductInternalSetting { Name = "IsResource", Value = "0" },
					new ProductInternalSetting { Name = "IsFavorite", Value = "1" },
					new ProductInternalSetting { Name = "LearnMore", Value = "https://www.realpage.com/" },
					new ProductInternalSetting { Name = "ApiEndPoint", Value = "https://depositiq-uat.realpage.com/unity_api" },
					new ProductInternalSetting { Name = "ApiUserName", Value = "realpageunity" },
					new ProductInternalSetting { Name = "ApiPassword", Value = "MCuR699P47w9T6K734UM" },
					new ProductInternalSetting { Name = "ProductStatus", Value = "7" },
					new ProductInternalSetting { Name = "ProductStatus", Value = "8" },
					new ProductInternalSetting { Name = "ProductStatus", Value = "8" },
					new ProductInternalSetting { Name = "ShowInUserDetails", Value = "1" },
					new ProductInternalSetting { Name = "ShowInRolesAndRights", Value = "0" },
					new ProductInternalSetting { Name = "ShowInAppSwitcher", Value = "1" },
					new ProductInternalSetting { Name = "ShowInUserListFilter", Value = "1" },
					new ProductInternalSetting { Name = "ProductAPIRequiresUser", Value = "0" },
					new ProductInternalSetting { Name = "GetRoleEndpoint", Value = "/roles" },
					new ProductInternalSetting { Name = "GetUserEndpoint", Value = "/users?loginName={0}" },
					new ProductInternalSetting { Name = "PostUserEndpoint", Value = "/users" },
					new ProductInternalSetting { Name = "PutUserEndpoint", Value = "/users" },
					new ProductInternalSetting { Name = "GetPropertyEndpoint", Value = "/properties/{0}" },
					new ProductInternalSetting { Name = "PatchProfileEndpoint", Value = "/user/profile" },
					new ProductInternalSetting { Name = "GetPropertyGroupsEndpoint", Value = "/companies/{0}/groups" },
					new ProductInternalSetting { Name = "GetListUsersEndpoint", Value = "/{0}/users?filter={1}&startRow={2}&resultsperpage={3}" },
					new ProductInternalSetting { Name = "PatchMigrateUsersEndpoint", Value = "/{0}/migrate-users" },
					new ProductInternalSetting { Name = "LockOnProductAccess", Value = "0" },
					new ProductInternalSetting { Name = "ProductNotAvailableForRegularUserNoEmail", Value = "0" },
					new ProductInternalSetting { Name = "AuthenticationType", Value = "Redirect" }
			 };
		}
		public static void InitQa()
		{
			throw new NotImplementedException();
		}
	}
}
