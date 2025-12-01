using UnifiedLogin.SharedObjects.IdentityConfig;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics.CodeAnalysis;

namespace UnifiedLogin.LandingAPI.Test.Logic.ProductIntegration
{
	[ExcludeFromCodeCoverage]
	public static class IlaTestData
	{
		public static string Environment { get; } = ConfigurationManager.AppSettings["Environment"];
		public static string CompanyInstanceSourceId { get; set; }
		public static string SubjectUserLoginName { get; set; }
		public static string UpdateInputJson { get; set; }

		static IlaTestData()
		{
			//if (Environment.Equals("QA"))
			//	InitQa();

			InitDev();
		}

		public static void InitDev()
		{
			CompanyInstanceSourceId = "PLG";
			SubjectUserLoginName = "sdawson@plpinc.net";
			UpdateInputJson = "{\"PropertyList\":[\"PLG10P01\",\"PLG10P02\",\"PLG10P03\"],\"RoleList\":[\"13\"],\"IsAssigned\":true,\"RegionList\":[\"PLG10\",\"PLG12\"],\"CompanyId\":0}";
		}

		public static List<ProductInternalSetting> Get_DEV_ILA_ProductSettings()
		{
			return new List<ProductInternalSetting>
				{
				new ProductInternalSetting { Name = "ClientId", Value = "ilmla" },
				new ProductInternalSetting { Name = "ClassName", Value = "ilmla"},
				new ProductInternalSetting { Name = "ProductUrl", Value = "product/ilmleadanalytics"},
				new ProductInternalSetting { Name = "TitleId", Value = "Intelligent Lead Management-Leasing Analytics"},
				new ProductInternalSetting { Name = "TitleUniqueId", Value = "4FD51401-4F7A-4BA2-88AA-15B6706BFCEF"},
				new ProductInternalSetting { Name = "IsNewTab", Value = "1"},
				new ProductInternalSetting { Name = "MetatagUniqueId", Value = "ILM-Leasing Analytics"},
				new ProductInternalSetting { Name = "IsResource", Value = "0"},
				new ProductInternalSetting { Name = "IsFavorite", Value = "1"},
				new ProductInternalSetting { Name = "IsFavorite", Value = "1"},
				new ProductInternalSetting { Name = "ApiEndPoint", Value = "http://54.189.37.40/ilmcustmgmt/ilm-la"},
				new ProductInternalSetting { Name = "ProductStatus", Value = "19"},
				new ProductInternalSetting { Name = "ProductStatus", Value = "7"},
				new ProductInternalSetting { Name = "ProductStatus", Value = "8"},
				new ProductInternalSetting { Name = "ShowInUserDetails", Value = "1"},
				new ProductInternalSetting { Name = "RequiresUnifiedLoginRight", Value = "0"},
				new ProductInternalSetting { Name = "ShowInRolesAndRights", Value = "0"},
				new ProductInternalSetting { Name = "ShowInAppSwitcher", Value = "1"},
				new ProductInternalSetting { Name = "ShowInUserListFilter", Value = "1"},
				new ProductInternalSetting { Name = "ProductAPIRequiresUser", Value = "0"},
				new ProductInternalSetting { Name = "GetRoleEndpoint", Value = "/roles"},
				new ProductInternalSetting { Name = "GetRegionEndpoint", Value = "/regions?companyId={0}"},
				new ProductInternalSetting { Name = "GetProfileEndpoint", Value = "/users/profile?loginName={0}"},
				new ProductInternalSetting { Name = "GetUserEndpoint", Value = "/users?loginName={0}"},
				new ProductInternalSetting { Name = "PostUserEndpoint", Value = "/users"},
				new ProductInternalSetting { Name = "PutUserEndpoint", Value = "/users"},
				new ProductInternalSetting { Name = "DeleteUserEndpoint", Value = "/users?loginName={0}"},
				new ProductInternalSetting { Name = "PutProfileEndpoint", Value = "/users/profile"},
				new ProductInternalSetting { Name = "GetUserListEndpoint", Value = "/users"},
				new ProductInternalSetting { Name = "GetPropertyEndpoint", Value = "/properties?companyId={0}"},
				new ProductInternalSetting { Name = "GetPropertyByGroupEndpoint", Value = "/properties?groupId={0}"},
				new ProductInternalSetting { Name = "PatchUserEndpoint", Value = "/users?loginName={0}"},
				new ProductInternalSetting { Name = "PatchProfileEndpoint", Value = "/users/profile"},
				new ProductInternalSetting { Name = "GetPropertyGroupsEndpoint", Value = "/propertyGroups?companyId={0}"},
				new ProductInternalSetting { Name = "GetListUsersEndpoint", Value = "/api/{0}/users?filter={1}&startRow={2}&resultsperpage={3}"},
				new ProductInternalSetting { Name = "PatchMigrateUsersEndpoint", Value = "/api/{0}/migrate-users"},
				new ProductInternalSetting { Name = "PutMigrateUserEndpoint", Value = "{0}/migrate-users"},
				new ProductInternalSetting { Name = "ProductNotAvailableForRegularUserNoEmail", Value = "1"},
				new ProductInternalSetting { Name = "AuthenticationType", Value = "OpenIdCustom"},
				new ProductInternalSetting { Name = "Authentication_OpenId_ProductName", Value = "leadanalytics"},
				new ProductInternalSetting { Name = "Authentication_OpenId_ResponseType", Value = "code"},
				new ProductInternalSetting { Name = "Authentication_OpenId_ScopesForAuth", Value = "openid profile userinfoapi"}
			 };
		}
		public static void InitQa()
		{
			throw new NotImplementedException();
		}
	}
}
