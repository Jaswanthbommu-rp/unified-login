using System.Net;
using Xunit;
using Xunit.Abstractions;
using Newtonsoft.Json;
using RPBooksApiTestingFramework;
using RPBooksApiTestingFramework.Controllers;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using System.Collections.Generic;
using System.Data;

namespace GreenBook.Tests
{
	public class IdentityOrganization : TestController
	{
		public IdentityOrganization(ITestOutputHelper _xUnitTestOutput)
		{
			reusable = new TestUtilities(this);
			this.XunitTestOutPut = _xUnitTestOutput;

			dbManager = new DatabaseController(DbConnString);
		}

		JsonController jsonManager = new JsonController();
		DatabaseController dbManager;
		TestUtilities reusable;
		private readonly ITestOutputHelper XunitTestOutPut;
        private string realPageId = "";


        // Organization=/api/organization/person/{realPageId}

        //[Fact, Trait("", "Happy Path")]
		public void GetOrganization()
		{
			// Get Expected Response from DB
			UserLogin expectedUserLoginUser = JsonConvert.DeserializeObject<UserLogin>(reusable.DoGetUserLoginUser(CurrentlyLoggedInUser));

			// Set up the API URL
			EndPointUrl = HostIdentityUrl + Properties["Organization"].Replace("{realPageId}", expectedUserLoginUser.RealPageId.ToString());

			// Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");

			// Extract API's JSON Response
			
			IList<RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Organization> organizationsResponse
				= JsonConvert.DeserializeObject<IList<RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Organization>>(ResponseString);
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);

			// Extract Expected Person's Organization
			DataTable expectedOrganization = dbManager.executeQuery("SELECT pr.[PartyIdTo], o.Name, p.RealPageId "
				+ "FROM [" + Properties["identityDatabase"] + "].[Enterprise].[PartyRelationship] pr "
				+ "INNER JOIN [" + Properties["identityDatabase"] + "].[Enterprise].[Organization] o ON pr.PartyIdTo = o.PartyId "
				+ "INNER JOIN [" + Properties["identityDatabase"] + "].[Enterprise].[Party] p ON o.PartyId = p.PartyId "
				+ "WHERE pr.PartyIdFrom = (SELECT PartyId FROM [Identity].[Enterprise].[Party] "
				+ "WHERE RealPageId = '" + expectedUserLoginUser.RealPageId + "')");

			// Assert
			Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");

			for (int countOrganizations = 0; countOrganizations < organizationsResponse.Count; countOrganizations++)
			{
				Assert.NotNull(organizationsResponse[countOrganizations].RealPageId);
				Assert.True(organizationsResponse[countOrganizations].RealPageId.ToString()
					== expectedOrganization.Rows[countOrganizations]["RealPageId"].ToString()
					, "organizationsResponse[countOrganizations].RealPageId.ToString() "
					+ "== expectedOrganization.Rows[countOrganizations][\"RealPageId\"].ToString()");
				Assert.NotNull(organizationsResponse[countOrganizations].PartyId);
				Assert.True(organizationsResponse[countOrganizations].PartyId
					== int.Parse(expectedOrganization.Rows[countOrganizations]["PartyIdTo"].ToString())
					, "organizationsResponse[countOrganizations].PartyId "
					+ "== int.Parse(expectedOrganization.Rows[countOrganizations][\"PartyIdTo\"].ToString())");
				Assert.NotNull(organizationsResponse[countOrganizations].Name);
				Assert.True(organizationsResponse[countOrganizations].Name
					== expectedOrganization.Rows[countOrganizations]["Name"].ToString()
					, "organizationsResponse[countOrganizations].Name "
					+ "== (expectedOrganization.Rows[countOrganizations][\"Name\"].ToString()");
			}
		}

		//[Fact, Trait("", "Negative Case")]
		public void GetOrganizationInvalidRealPageId()
		{
			// Set up the API URL
			EndPointUrl = HostIdentityUrl + Properties["Organization"].Replace("{realPageId}", "invalidRealPageId");

			// Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");

			// Extract API's JSON Response
			
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);

			// Assert
			Assert.True(HttpStatusCode.BadRequest == ResponseHttpStatusCode, "HttpStatusCode.BadRequest == ResponseHttpStatusCode");
			Assert.True(ResponseString.Contains("The request is invalid."), "ResponseString.Contains(\"The request is invalid.\")");
		}
    }
}
