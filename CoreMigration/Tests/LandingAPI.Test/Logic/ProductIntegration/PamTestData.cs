using UnifiedLogin.SharedObjects.IdentityConfig;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics.CodeAnalysis;

namespace UnifiedLogin.LandingAPI.Test.Logic.ProductIntegration
{
	[ExcludeFromCodeCoverage]
	public static class PamTestData
	{
		public static string Environment { get; } = ConfigurationManager.AppSettings["Environment"];
		public static string CompanyInstanceSourceId { get; set; }
		public static string SubjectUserLoginName { get; set; }
		public static string UpdateInputJson { get; set; }

		static PamTestData()
		{
			//if (Environment.Equals("QA"))
			//	InitQa();

			InitDev();
		}

		public static void InitDev()
		{
			CompanyInstanceSourceId = "gandk";
			SubjectUserLoginName = "PamLocalUser@test.com";
			UpdateInputJson = "{\"PropertyList\":[],\"RoleList\":[\"1\"],\"PropertyGroupList\":[],\"IsAssigned\":true,\"CompanyId\":0,\"PropertyRoleList\":[{\"PropertyId\":\"781\",\"Roles\":[\"337\",\"338\"]},{\"PropertyId\":\"782\",\"Roles\":[\"339\",\"336\"]}],\"CanReceiveMonthlyReport\":false}";
		}

		public static List<ProductInternalSetting> Get_DEV_PAM_ProductSettings()
		{
			return new List<ProductInternalSetting>
			{
				new ProductInternalSetting {Name = "ClientId"   , Value =  "1"   },
				new ProductInternalSetting {Name = "ClassName", Value = "portfoliomanagement" },
				new ProductInternalSetting {Name = "ProductUrl" , Value = "product/portfoliomanagement" },
				new ProductInternalSetting {Name = "TitleId"    , Value = "Portfolio Management"    },
				new ProductInternalSetting {Name = "TitleUniqueId"  , Value = "8798E17D-249F-4C7D-89D5-72523437D71D"    },
				new ProductInternalSetting {Name = "IsNewTab", Value = "1"},
				new ProductInternalSetting { Name = "MetatagUniqueId", Value = "PortfolioManagement"},
				new ProductInternalSetting { Name = "IsResource", Value = "0"},
				new ProductInternalSetting { Name = "IsFavorite", Value = "1"},
				new ProductInternalSetting { Name = "LearnMore", Value = "https://www.realpage.com/"},
				new ProductInternalSetting { Name = "ApiEndPoint", Value = "https://wmu-books.asseteye.net"},
				new ProductInternalSetting { Name = "ProductStatus", Value = "7"},
				new ProductInternalSetting { Name = "ProductStatus", Value = "8"},
				new ProductInternalSetting { Name = "ShowInUserDetails", Value = "1"},
				new ProductInternalSetting { Name = "ShowInRolesAndRights", Value = "0"},
				new ProductInternalSetting { Name = "ShowInAppSwitcher", Value = "1"},
				new ProductInternalSetting { Name = "ShowInUserListFilter", Value = "1"},
				new ProductInternalSetting { Name = "ProductAPIRequiresUser", Value = "0"},
				new ProductInternalSetting { Name = "GetRoleEndpoint", Value = "/api/{0}/Roles?isGlobalRoles={1}"},
				new ProductInternalSetting { Name = "GetUserEndpoint", Value = "/api/Users?companyId={0}&loginname={1}"},
				new ProductInternalSetting { Name = "PostUserEndpoint", Value = "/api/users"},
				new ProductInternalSetting { Name = "PutUserEndpoint", Value = "/api/users"},
				new ProductInternalSetting { Name = "DeleteUserEndpoint", Value = "/api/{companyId}/users?loginName={0}"},
				new ProductInternalSetting { Name = "PutProfileEndpoint", Value = "/api/users/profile"},
				new ProductInternalSetting { Name = "GetPropertyEndpoint", Value = "/api/{0}/Properties"},
				new ProductInternalSetting { Name = "TokenClientId", Value = "apiuser"},
				new ProductInternalSetting { Name = "TokenClientSecret", Value = "apiU$3r"},
				new ProductInternalSetting { Name = "GetLoginUrlEndpoint", Value = "/api/{0}/LoginURL"},
				new ProductInternalSetting { Name = "GetListUsersEndpoint", Value = "/api/{0}/users?filter={1}&startRow={2}&resultsperpage={3}"},
				new ProductInternalSetting { Name = "PatchMigrateUsersEndpoint", Value = "/api/{0}/migrate-users"},
				new ProductInternalSetting { Name = "ProductNotAvailableForRegularUserNoEmail", Value = "0"},
				new ProductInternalSetting { Name = "AuthenticationType", Value = "Redirect"},
			};
		}
		public static void InitQa()
		{
			CompanyInstanceSourceId = "QA";
		}
	}

}

