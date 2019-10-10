using System.Net;
using Xunit;
using Xunit.Abstractions;
using Newtonsoft.Json;
using RPBooksApiTestingFramework;
using RPBooksApiTestingFramework.Controllers;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using System.Data;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using System.Collections.Generic;
using GreenBook.Models;

namespace GreenBook.Tests
{
	public class LandingOrganization : TestController
	{
		public LandingOrganization(ITestOutputHelper _xUnitTestOutput)
		{
			reusable = new TestUtilities(this);
			this.XunitTestOutPut = _xUnitTestOutput;
            realPageId = reusable.GetRealPageId(CurrentlyLoggedInUser);
        }
		
		JsonController jsonManager = new JsonController();
		TestUtilities reusable;
		private readonly ITestOutputHelper XunitTestOutPut;
		private string realPageId = "";

        // LandingOrganization=/api/organization/


        // GET /api/organization/{realPageId}/products
        [Fact, Trait("", "Happy Path")]
        public void GetLandingOrganization()
        {
            EndPointUrl = HostUrl + Properties["LandingOrganization"] + "person" + "/{realPageId}";
            EndPointUrl = EndPointUrl.Replace("{realPageId}", realPageId);

            // Execute API
            XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
            GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");

            // Extract API's JSON Response
            XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);
            LandingOrganizationModel personResponse = JsonConvert.DeserializeObject<LandingOrganizationModel>(ResponseString);

            // Assert
            Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");

            // GET /api/organization/{realPageId}/products

            EndPointUrl = HostUrl + Properties["LandingOrganization"] + "/{realPageId}";
            EndPointUrl = EndPointUrl.Replace("{realPageId}", (personResponse.data[0].partyRelationship.realPageIdTo).ToString());

            // Execute API
            XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
            GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");

            Organization orgResponse = JsonConvert.DeserializeObject<Organization>(ResponseString);
        }

        // GET /api/organization/{realPageId}/products
        [Fact, Trait("", "Happy Path")]
		public void GetLandingOrganizationProducts()
		{
            EndPointUrl = HostUrl + Properties["LandingOrganization"] + "person" + "/{realPageId}";
            EndPointUrl = EndPointUrl.Replace("{realPageId}", realPageId);

            // Execute API
            XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
            GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");

            // Extract API's JSON Response
            XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);
            LandingOrganizationModel personResponse = JsonConvert.DeserializeObject<LandingOrganizationModel>(ResponseString);

            // Assert
            Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");

            //GET api/organization/{realPageId}/products

            // Set up the API URL
            EndPointUrl = HostUrl + Properties["LandingOrganization"] + "{realPageId}/" +"Products" + "?allProducts=false&mergePersonaAccess=true";
            EndPointUrl = EndPointUrl.Replace("{realPageId}", (personResponse.data[0].partyRelationship.realPageIdTo).ToString());

            // Execute API
            XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
            GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");

			// Extract API's JSON Response
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);
            ObjectListOutput<ProductUI, IErrorData> productFamilyResponse = JsonConvert.DeserializeObject<ObjectListOutput<ProductUI, IErrorData>>(ResponseString);

			// Assert
			Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
		}

        // GET /api/organization/person/{realPageId}
        [Fact, Trait("", "Happy Path")]
        public void GetLandingOrganizationPerson()
        {
            // Set up the API URL
            EndPointUrl = HostUrl + Properties["LandingOrganization"] + "person"+ "/{realPageId}";
            EndPointUrl = EndPointUrl.Replace("{realPageId}", realPageId);

            // Execute API
            XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
            GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");

            // Extract API's JSON Response
            XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);
            LandingOrganizationModel personResponse = JsonConvert.DeserializeObject<LandingOrganizationModel>(ResponseString);

            // Assert
            Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
        }

        // GET /api/organization/{realPageId}/products
        [Fact, Trait("", "Happy Path")]
        public void GetLandingProvidertype()
        {
            EndPointUrl = HostUrl + Properties["LandingOrganization"] + "person" + "/{realPageId}";
            EndPointUrl = EndPointUrl.Replace("{realPageId}", realPageId);

            // Execute API
            XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
            GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");

            // Extract API's JSON Response
            XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);
            LandingOrganizationModel personResponse = JsonConvert.DeserializeObject<LandingOrganizationModel>(ResponseString);

            // Assert
            Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");

            // GET /api/organization/{realPageId}/products

            EndPointUrl = HostUrl + Properties["LandingOrganization"] + "/Providertype"+"?realPageId="+"{realPageId}";
            EndPointUrl = EndPointUrl.Replace("{realPageId}", (personResponse.data[0].partyRelationship.realPageIdTo).ToString());

            // Execute API
            XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
            GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");

            Organization orgResponse = JsonConvert.DeserializeObject<Organization>(ResponseString);
        }


        //[Fact, Trait("", "Data-Driven")]
        public void GetLandingOrganizationProductsWithValidOptionalParameterValues()
		{
			// Set up the API URL
			DatabaseController dbManager = new DatabaseController(DbConnString);
			string realPageId = JsonConvert.DeserializeObject<UserLogin>(reusable.DoGetUserLoginUser(CurrentlyLoggedInUser)).RealPageId.ToString();

			// Set up the Organization API URL
			EndPointUrl = HostIdentityUrl + Properties["Organization"].Replace("{realPageId}", realPageId);

			// Execute Organization API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");

			// Extract Organization API's JSON Response
			IList<RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Organization> organizationsResponse
				= JsonConvert.DeserializeObject<IList<RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Organization>>(ResponseString);

			DataTable personaForTesting = dbManager.executeQuery("EXEC [" + Properties["identityDatabase"] + "].[Person].[ListPersona] '"
				+ realPageId + "', 0");

			//EndPointUrl = HostUrl + Properties["LandingOrganization"] + "Products?organizationId="
			//	+ organizationsResponse[0].OrganizationPartyId + "&personaId=" + personaForTesting.Rows[0]["personaId"].ToString()
			//	+ "&searchCriteria=" + WebUtility.UrlEncode("Onesite");

			// Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");

			// Extract API's JSON Response
			
			//ObjectListOutput<ProductFamily, ErrorData> productFamilyResponse = JsonConvert.DeserializeObject<ObjectListOutput<ProductFamily, ErrorData>>(ResponseString);
			//XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);

			//// Assert
			//Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
			//foreach (ProductFamily actualProductFamily in productFamilyResponse.list)
			//{
			//	Assert.NotNull(actualProductFamily.ProductTypeGUID);
			//}
		}

		//[Fact, Trait("", "Negative Case")]
		public void GetLandingOrganizationProductsWithInvalidOptionalParameterValues()
		{
			// Set up the API URL
			EndPointUrl = HostUrl + Properties["LandingOrganization"] + "Products?organizationId=0&personaId=0&searchCriteria="
				+ WebUtility.UrlEncode("0n3 S!+=");

			// Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");

			// Extract API's JSON Response
			 
			//ObjectListOutput<ProductFamily, ErrorData> productFamilyResponse = JsonConvert.DeserializeObject<ObjectListOutput<ProductFamily, ErrorData>>(ResponseString);
			//XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);

			//// Assert
			//Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
			//foreach (ProductFamily actualProductFamily in productFamilyResponse.list)
			//{
			//	Assert.NotNull(actualProductFamily.ProductTypeGUID);
			//}
		}

		//[Fact, Trait("", "Happy Path")]
		public void GetLandingOrganizationProductsGroupedByFamily()
		{
			// Set up the API URL
			EndPointUrl = HostUrl + Properties["LandingOrganization"] + "ProductsGroupedByFamily";

			// Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");

			// Extract API's JSON Response
			//ObjectListOutput<ProductFamily, ErrorData> productFamilyResponse = JsonConvert.DeserializeObject<ObjectListOutput<ProductFamily, ErrorData>>(ResponseString);
			//XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);

			//// Assert
			//Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
			//foreach (ProductFamily actualProductFamily in productFamilyResponse.list)
			//{
			//	Assert.NotNull(actualProductFamily.ProductTypeGUID);
			//}
		}

		//[Fact, Trait("", "Data-Driven")]
		public void GetLandingOrganizationProductsGroupedByFamilyWithValidOptionalParameterValues()
		{
			// Set up the API URL
			DatabaseController dbManager = new DatabaseController(DbConnString);
			string realPageId = JsonConvert.DeserializeObject<UserLogin>(reusable.DoGetUserLoginUser(CurrentlyLoggedInUser)).RealPageId.ToString();

			// Set up the Organization API URL
			EndPointUrl = HostIdentityUrl + Properties["Organization"].Replace("{realPageId}", realPageId);

			// Execute Organization API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");

			// Extract Organization API's JSON Response
			IList<RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Organization> organizationsResponse
				= JsonConvert.DeserializeObject<IList<RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Organization>>(ResponseString);

			DataTable personaForTesting = dbManager.executeQuery("EXEC [" + Properties["identityDatabase"] + "].[Person].[ListPersona] '"
				+ realPageId + "', 0");

			//EndPointUrl = HostUrl + Properties["LandingOrganization"] + "ProductsGroupedByFamily?organizationId=" 
			//	+ organizationsResponse[0].OrganizationPartyId + "&personaId=" + personaForTesting.Rows[0]["personaId"].ToString()
			//	+ "&searchCriteria=" + WebUtility.UrlEncode("Onesite");

			// Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");

			// Extract API's JSON Response
			
			//ObjectListOutput<ProductFamily, ErrorData> productFamilyResponse = JsonConvert.DeserializeObject<ObjectListOutput<ProductFamily, ErrorData>>(ResponseString);
			//XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);

			//// Assert
			//Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
			//foreach (ProductFamily actualProductFamily in productFamilyResponse.list)
			//{
			//	Assert.NotNull(actualProductFamily.ProductTypeGUID);
			//}
		}

		//[Fact, Trait("", "Negative Case")]
		public void GetLandingOrganizationProductsGroupedByFamilyWithInvalidOptionalParameterValues()
		{
			// Set up the API URL
			EndPointUrl = HostUrl + Properties["LandingOrganization"] + "ProductsGroupedByFamily?organizationId=0&personaId=0&searchCriteria="
				+ WebUtility.UrlEncode("0n3 S!+=");

			// Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");

			// Extract API's JSON Response
			//ObjectListOutput<ProductFamily, ErrorData> productFamilyResponse = JsonConvert.DeserializeObject<ObjectListOutput<ProductFamily, ErrorData>>(ResponseString);
			//XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);

			//// Assert
			//Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
			//foreach (ProductFamily actualProductFamily in productFamilyResponse.list)
			//{
			//	Assert.NotNull(actualProductFamily.ProductTypeGUID);
			//}
		}

		//[Fact, Trait("", "Happy Path")]
		public void GetOrganizationProducts()
		{

			// Set up the API URL
			//realPageId = JsonConvert.DeserializeObject<UserLogin>(reusable.DoGetUserLoginUser("james@test.com")).RealPageId.ToString();
			_accessToken = GetClientToken(Properties["identityClientUrl"], CurrentlyLoggedInUser, "P@ssw0rd");
			realPageId = "9e9410ae-2c41-47d2-81d1-109c08cd151c";
			EndPointUrl = HostUrl + Properties["OrganizationByProducts"].Replace("{realPageId}", realPageId);


			// Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
            GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get);

            // Extract API's JSON Response
			ObjectListOutput<ProductUI, IErrorData> organizationsOrganizationProductsResponse = JsonConvert.DeserializeObject<ObjectListOutput<ProductUI, IErrorData>>(ResponseString);
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);

			// Extract Expected JSON Response
			ProductUI expectedOrganizationProducts = JsonConvert.DeserializeObject<ProductUI>(reusable.DoGetOrganizationProducts());

			// Assert
			Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");

			for (int countOrganizationProducts = 0; countOrganizationProducts < organizationsOrganizationProductsResponse.list.Count; countOrganizationProducts++)
			{
				Assert.True(organizationsOrganizationProductsResponse.list[0].ProductId == expectedOrganizationProducts.ProductId,
					"organizationsOrganizationProductsResponse.list[0].ProductId == expectedOrganizationProducts.ProductId");
				Assert.True(organizationsOrganizationProductsResponse.list[0].TitleUniqueId == expectedOrganizationProducts.TitleUniqueId,
					"organizationsOrganizationProductsResponse.list[0].TitleUniqueId == expectedOrganizationProducts.TitleUniqueId");
				Assert.True(organizationsOrganizationProductsResponse.list[0].ClassName == expectedOrganizationProducts.ClassName,
					"organizationsOrganizationProductsResponse.list[0].ClassName == expectedOrganizationProducts.ClassName");
				//                Assert.True(organizationsOrganizationProductsResponse.list[0].ClientId == expectedOrganizationProducts.ClientId,
				//                    "organizationsOrganizationProductsResponse.list[0].ClientId == expectedOrganizationProducts.ClientId");
				Assert.True(organizationsOrganizationProductsResponse.list[0].SettingsUrl == expectedOrganizationProducts.SettingsUrl,
					"organizationsOrganizationProductsResponse.list[0].SettingsUrl == expectedOrganizationProducts.SettingsUrl");
				//Personaid               Assert.True(organizationsOrganizationProductsResponse.list[0].ProductUrl == expectedOrganizationProducts.ProductUrl,
				//                    "organizationsOrganizationProductsResponse.list[0].ProductUrl == expectedOrganizationProducts.ProductUrl");
				Assert.True(organizationsOrganizationProductsResponse.list[0].IsNewTab == expectedOrganizationProducts.IsNewTab,
				   "organizationsOrganizationProductsResponse.list[0].IsNewTab == expectedOrganizationProducts.IsNewTab");
				Assert.True(organizationsOrganizationProductsResponse.list[0].ProductName == expectedOrganizationProducts.ProductName,
					"organizationsOrganizationProductsResponse.list[0].ProductName == expectedOrganizationProducts.ProductName");
				Assert.True(organizationsOrganizationProductsResponse.list[0].ProductDescription == expectedOrganizationProducts.ProductDescription,
					"organizationsOrganizationProductsResponse.list[0].ProductDescription == expectedOrganizationProducts.ProductDescription");
				//Assert.True(organizationsOrganizationProductsResponse.list[0].IsFavorite == expectedOrganizationProducts.IsFavorite,
				//		"organizationsOrganizationProductsResponse.list[0].IsFavorite == expectedOrganizationProducts.IsFavorite");
				Assert.True(organizationsOrganizationProductsResponse.list[0].FamilyId == expectedOrganizationProducts.FamilyId,
					"organizationsOrganizationProductsResponse.list[0].FamilyId == expectedOrganizationProducts.FamilyId");
				Assert.True(organizationsOrganizationProductsResponse.list[0].Family == expectedOrganizationProducts.Family,
					"organizationsOrganizationProductsResponse.list[0].Family == expectedOrganizationProducts.Family");
				Assert.True(organizationsOrganizationProductsResponse.list[0].SolutionId == expectedOrganizationProducts.SolutionId,
					"organizationsOrganizationProductsResponse.list[0].SolutionId == expectedOrganizationProducts.SolutionId");
				Assert.True(organizationsOrganizationProductsResponse.list[0].Solution == expectedOrganizationProducts.Solution,
					"organizationsOrganizationProductsResponse.list[0].Solution == expectedOrganizationProducts.Solution");
				Assert.True(organizationsOrganizationProductsResponse.list[0].Subsolution == expectedOrganizationProducts.Subsolution,
					"organizationsOrganizationProductsResponse.list[0].Subsolution == expectedOrganizationProducts.Subsolution");
				Assert.True(organizationsOrganizationProductsResponse.list[0].IsResource == expectedOrganizationProducts.IsResource,
					"organizationsOrganizationProductsResponse.list[0].IsResource == expectedOrganizationProducts.IsResource");
				Assert.True(organizationsOrganizationProductsResponse.list[0].LearnMore == expectedOrganizationProducts.LearnMore,
					"organizationsOrganizationProductsResponse.list[0].LearnMore == expectedOrganizationProducts.LearnMore");
			}
		}
	}
}
