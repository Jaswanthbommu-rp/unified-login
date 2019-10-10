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
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using System;

namespace GreenBook.Tests
{
	public class Personas : TestController
	{
        private long personaId;
        private string existingLoginName;
         private Guid realpageId;


        public Personas(ITestOutputHelper _xUnitTestOutput)
		{
			reusable = new TestUtilities(this);
			this.XunitTestOutPut = _xUnitTestOutput;

			personaApiTestEnterpriseUsername = CurrentlyLoggedInUser;

			dbManager = new DatabaseController(DbConnString);
            //realPageId = JsonConvert.DeserializeObject<UserLogin>(reusable.DoGetUserLoginUser(personaApiTestEnterpriseUsername)).RealPageId.ToString();
            realPageId = reusable.GetRealPageId(CurrentlyLoggedInUser);
			personaForTesting = dbManager.executeQuery("SELECT TOP 1 [PersonaId], [PersonPartyId], [OrganizationPartyId] FROM[Identity].[Person].[Persona] "
				+ "where personPartyId = (select partyid from[Identity].enterprise.party "
				+ "where realpageid = '" + realPageId + "')");
		}

		JsonController jsonManager = new JsonController();
		DatabaseController dbManager = new DatabaseController("");
		TestUtilities reusable;
		private readonly ITestOutputHelper XunitTestOutPut;
		DataTable personaForTesting = new DataTable();
		private string realPageId, personaApiTestEnterpriseUsername;
        private string payload;
        private string productId;

        // Personas=/api/personas/

        [Fact, Trait("", "Happy Path")]
		public void GetPersonasProductsProductsWithFavorites()
		{
			// Set up the API URL
			EndPointUrl = HostUrl + Properties["Personas"] + "products?productSelectType=ProductsWithFavorites";

			// Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");

			// Extract API's JSON Response
			
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);
			ObjectListOutput<PersonaProductUserDetails, ErrorData> personaResponse
				= JsonConvert.DeserializeObject<ObjectListOutput<PersonaProductUserDetails, ErrorData>>(ResponseString);

			// Assert
			Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
			Assert.NotNull(personaResponse.list[0].PersonaId);
			Assert.True(personaResponse.list[0].PersonaId == int.Parse(personaForTesting.Rows[0]["PersonaId"].ToString())
				, "personaResponse.list[0].PersonaId == int.Parse(personaForTesting.Rows[0][\"PersonaId\"].ToString())");
			Assert.NotNull(personaResponse.list[0].PersonPartyId);
			//Assert.True(personaResponse.list[0].PersonPartyId == int.Parse(personaForTesting.Rows[0]["PersonPartyId"].ToString())
				//, "personaResponse.list[0].PersonPartyId == int.Parse(personaForTesting.Rows[0][\"PersonPartyId\"].ToString())");
			Assert.NotNull(personaResponse.list[0].OrganizationPartyId);
			Assert.True(personaResponse.list[0].OrganizationPartyId == int.Parse(personaForTesting.Rows[0]["OrganizationPartyId"].ToString())
				, "personaResponse.list[0].OrganizationPartyId == int.Parse(personaForTesting.Rows[0][\"OrganizationPartyId\"].ToString())");

		}

		//[Fact, Trait("", "Data-Driven")]
		public void GetPersonasProductsResourcesOnly()
		{
			// Set up the API URL
			EndPointUrl = HostUrl + Properties["Personas"] + "products?productSelectType=ResourcesOnly";

			// Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");

			// Extract API's JSON Response
			
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);
			ObjectListOutput<PersonaProductUserDetails, ErrorData> personaResponse
				= JsonConvert.DeserializeObject<ObjectListOutput<PersonaProductUserDetails, ErrorData>>(ResponseString);
			
			// Assert
			Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
			Assert.NotNull(personaResponse.list[0].PersonaId);
			Assert.True(personaResponse.list[0].PersonaId == int.Parse(personaForTesting.Rows[0]["PersonaId"].ToString())
				, "personaResponse.list[0].PersonaId == int.Parse(personaForTesting.Rows[0][\"PersonaId\"].ToString())");
			Assert.NotNull(personaResponse.list[0].PersonPartyId);
			Assert.True(personaResponse.list[0].PersonPartyId == int.Parse(personaForTesting.Rows[0]["PersonPartyId"].ToString())
				, "personaResponse.list[0].PersonPartyId == int.Parse(personaForTesting.Rows[0][\"PersonPartyId\"].ToString())");
			Assert.NotNull(personaResponse.list[0].OrganizationPartyId);
			Assert.True(personaResponse.list[0].OrganizationPartyId == int.Parse(personaForTesting.Rows[0]["OrganizationPartyId"].ToString())
				, "personaResponse.list[0].OrganizationPartyId == int.Parse(personaForTesting.Rows[0][\"OrganizationPartyId\"].ToString())");

		}

		//[Fact, Trait("", "Data-Driven")]
		public void GetPersonasProductsFavoritesOnly()
		{
			// Set up the API URL
			EndPointUrl = HostUrl + Properties["Personas"] + "products?productSelectType=FavoritesOnly";

			// Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");

			// Extract API's JSON Response
			
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);
			ObjectListOutput<PersonaProductUserDetails, ErrorData> personaResponse
				= JsonConvert.DeserializeObject<ObjectListOutput<PersonaProductUserDetails, ErrorData>>(ResponseString);

			// Assert
			Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
			Assert.NotNull(personaResponse.list[0].PersonaId);
			Assert.True(personaResponse.list[0].PersonaId == int.Parse(personaForTesting.Rows[0]["PersonaId"].ToString())
				, "personaResponse.list[0].PersonaId == int.Parse(personaForTesting.Rows[0][\"PersonaId\"].ToString())");
			Assert.NotNull(personaResponse.list[0].PersonPartyId);
			Assert.True(personaResponse.list[0].PersonPartyId == int.Parse(personaForTesting.Rows[0]["PersonPartyId"].ToString())
				, "personaResponse.list[0].PersonPartyId == int.Parse(personaForTesting.Rows[0][\"PersonPartyId\"].ToString())");
			Assert.NotNull(personaResponse.list[0].OrganizationPartyId);
			Assert.True(personaResponse.list[0].OrganizationPartyId == int.Parse(personaForTesting.Rows[0]["OrganizationPartyId"].ToString())
				, "personaResponse.list[0].OrganizationPartyId == int.Parse(personaForTesting.Rows[0][\"OrganizationPartyId\"].ToString())");

		}
		
		//[Fact, Trait("", "Data-Driven")]
		public void GetPersonasProductsWithoutProductSelectType()
		{
			// Set up the API URL
			EndPointUrl = HostUrl + Properties["Personas"] + "products?productSelectType=";

			// Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");

			// Extract API's JSON Response
			
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);
			ObjectListOutput<PersonaProductUserDetails, ErrorData> personaResponse
				= JsonConvert.DeserializeObject<ObjectListOutput<PersonaProductUserDetails, ErrorData>>(ResponseString);

			// Assert
			Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
			Assert.NotNull(personaResponse.list[0].PersonaId);
			Assert.True(personaResponse.list[0].PersonaId == int.Parse(personaForTesting.Rows[0]["PersonaId"].ToString())
				, "personaResponse.list[0].PersonaId == int.Parse(personaForTesting.Rows[0][\"PersonaId\"].ToString())");
			Assert.NotNull(personaResponse.list[0].PersonPartyId);
			Assert.True(personaResponse.list[0].PersonPartyId == int.Parse(personaForTesting.Rows[0]["PersonPartyId"].ToString())
				, "personaResponse.list[0].PersonPartyId == int.Parse(personaForTesting.Rows[0][\"PersonPartyId\"].ToString())");
			Assert.NotNull(personaResponse.list[0].OrganizationPartyId);
			Assert.True(personaResponse.list[0].OrganizationPartyId == int.Parse(personaForTesting.Rows[0]["OrganizationPartyId"].ToString())
				, "personaResponse.list[0].OrganizationPartyId == int.Parse(personaForTesting.Rows[0][\"OrganizationPartyId\"].ToString())");

		}

        //PUT PersonasProductSettings=/api/personas/products/{productId}/productSettings
        [Fact, Trait("", "Happy Path")]
        public void PutPersonasProductSettings()
        {
            // Set up Payload
            payload = reusable.DoPutPersonasProductSettings(1, "isFavorite", "1");
            XunitTestOutPut.WriteLine("Payload:\n" + payload);

            // Set up the API URL
            productId = "1";
            EndPointUrl = HostUrl + Properties["PersonasProductSettings"].Replace("{productId}", productId);

            // Execute API
            XunitTestOutPut.WriteLine("Calling " + HttpVerb.Put + " at " + EndPointUrl);
            GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Put, jsonPayload: payload);

            // Extract API's JSON Response
            
            Status<IErrorData> repositoryResponse = JsonConvert.DeserializeObject<Status<IErrorData>>(ResponseString);
            XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);

            // Assert
            Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
            Assert.NotNull(repositoryResponse.Success);
            Assert.True(repositoryResponse.Success, "repositoryResponse.isSuccess");
            Assert.NotNull(repositoryResponse.ErrorCode);
            Assert.True(repositoryResponse.ErrorCode == "", "repositoryResponse.ErrorCode == ");
            Assert.NotNull(repositoryResponse.ErrorMsg);
            Assert.True(repositoryResponse.ErrorMsg == "", "repositoryResponse.ErrorMsg == ");
        }

		[Fact, Trait("", "Happy Path")]
		public void GetPersonaEnvironment()
		{
			// Set up the API URL
			EndPointUrl = HostUrl + Properties["Persona"] + "/Environment";

			// Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");

			// Extract API's JSON Response
			
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);
			ObjectOutput<List<PersonaEnvironment>, ErrorData> personaEnvironmentResponse
				= JsonConvert.DeserializeObject<ObjectOutput<List<PersonaEnvironment>, ErrorData>>(ResponseString);

			// Extract Expected Response for Assertion
			string GetPersonaEnvironmentResponsePath = DataPath + "GetPersonaEnvironmentResponse.json";
			string GetPersonaEnvironmentResponse = jsonManager.LoadJsonAsString(GetPersonaEnvironmentResponsePath);
			ObjectOutput<List<PersonaEnvironment>, ErrorData> expectedPersonaEnvironmentResponse
				= JsonConvert.DeserializeObject<ObjectOutput<List<PersonaEnvironment>, ErrorData>>(GetPersonaEnvironmentResponse);

			// Assert
			Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
			for (int countPersonaEnvironment = 0; countPersonaEnvironment < personaEnvironmentResponse.obj.Count; countPersonaEnvironment++)
			{
				Assert.NotNull(personaEnvironmentResponse.obj[countPersonaEnvironment].PersonaEnvironmentTypeId);
				Assert.True(personaEnvironmentResponse.obj[countPersonaEnvironment].PersonaEnvironmentTypeId
					== expectedPersonaEnvironmentResponse.obj[countPersonaEnvironment].PersonaEnvironmentTypeId
					, "personaEnvironmentResponse.obj[countPersonaEnvironment].PersonaEnvironmentTypeId "
					+ "== expectedPersonaEnvironmentResponse.obj[countPersonaEnvironment].PersonaEnvironmentTypeId");
				Assert.NotNull(personaEnvironmentResponse.obj[countPersonaEnvironment].Name);
				Assert.True(personaEnvironmentResponse.obj[countPersonaEnvironment].Name
					== expectedPersonaEnvironmentResponse.obj[countPersonaEnvironment].Name
					, "personaEnvironmentResponse.obj[countPersonaEnvironment].Name "
					+ "== expectedPersonaEnvironmentResponse.obj[countPersonaEnvironment].Name");
			}
		}

        //[Fact, Trait("", "Happy Path")]
        public void GetPersona()
        {
            personaId = JsonConvert.DeserializeObject<Persona>(reusable.DoGetPersona()).PersonaId;
            realpageId = JsonConvert.DeserializeObject<Persona>(reusable.DoGetPersona(personaId)).RealPageId;
            existingLoginName = JsonConvert.DeserializeObject<UserLogin>(reusable.DoGetUserLogins(realpageId)).LoginName;
                        
            // Set up the API URL
            EndPointUrl = HostUrl + Properties["Persona"] + "?personaId="+personaId;

            // Execute API
            XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
            GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");

            // Extract API's JSON Response

            XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);
            Persona personaResponse = JsonConvert.DeserializeObject<Persona>(ResponseString);

            Assert.True(personaResponse.Organization.RealPageId != null);
            Assert.True(personaResponse.Organization.PartyId != null);
            Assert.True(personaResponse.Organization.BooksMasterId != null);
            Assert.True(personaResponse.Organization.Name != null);

            Assert.True(personaResponse.PersonaTypeId > 0);
            Assert.True(personaResponse.PersonaEnvironmentTypeId > 0);
            Assert.True(personaResponse.FromDate != null);
            //Assert.True(personaResponse.ThruDate != null);
            Assert.True(personaResponse.IsDefault != null);
            Assert.True(personaResponse.PersonaId > 0);
            Assert.True(personaResponse.PersonPartyId > 0);
            Assert.True(personaResponse.RealPageId != null);
            Assert.True(personaResponse.OrganizationPartyId > 0);
            Assert.True(personaResponse.Name != null);
            Assert.True(personaResponse.UserId > 0);
            //Assert.True(personaResponse.role != null);

            // Additional Asserts
            Assert.True(personaResponse.PersonaId == personaId);
            Assert.True(personaResponse.RealPageId.ToString() == realPageId);


        }
    }
}
