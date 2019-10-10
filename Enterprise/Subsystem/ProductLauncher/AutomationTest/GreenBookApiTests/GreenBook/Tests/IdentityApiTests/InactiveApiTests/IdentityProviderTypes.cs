using System.Net;
using Xunit;
using Xunit.Abstractions;
using Newtonsoft.Json;
using RPBooksApiTestingFramework;
using RPBooksApiTestingFramework.Controllers;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using System.Data;

namespace GreenBook.Tests
{
	public class IdentityProviderTypes : TestController
	{
		JsonController jsonManager = new JsonController();
		DatabaseController dbManager = new DatabaseController("");
		TestUtilities reusable;
		private readonly ITestOutputHelper XunitTestOutPut;
		string payload;

		public IdentityProviderTypes(ITestOutputHelper _xUnitTestOutput)
		{
			reusable = new TestUtilities(this);
			this.XunitTestOutPut = _xUnitTestOutput;
		}

		// IdentityProviderTypes=/api/identityprovider/types

		//[Fact, Trait("", "Happy Path")]
		public void GetIdentityProviderTypes()
		{
			// Set up the API URL
			EndPointUrl = HostIdentityUrl + Properties["IdentityProviderTypes"] + "?enterpriseUserName=" + WebUtility.UrlEncode(Properties["enterpriseUsername"]);

			// Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");

			// Extract API's JSON Response
			
			IdentityProviderType identityProviderTypeResponse = JsonConvert.DeserializeObject<IdentityProviderType>(ResponseString);
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);
			
			// Assert
			Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
			//Assert.NotNull(identityProviderTypeResponse.IdentityProviderTypeId);
			//Assert.True(identityProviderTypeResponse.EnterpriseUserName == Properties["enterpriseUsername"], "getSecurityQuestions.enterpriseUserName == strEnterpriseUserName");
		}

		//[Fact, Trait("", "Happy Path")]
		public void PostIdentityProviderTypes()
		{
			// Set up Payload
			payload = reusable.DoPostIdentityProviderTypesPayload();
			XunitTestOutPut.WriteLine("Payload:\n" + payload);

			// Set up the API URL
			EndPointUrl = HostIdentityUrl + Properties["IdentityProviderTypes"];

			// Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Post + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: payload);

			// Extract API's JSON Response
			
			IdentityProviderType identityProviderSettingResponse = JsonConvert.DeserializeObject<IdentityProviderType>(ResponseString);
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);

			// Assert
			Assert.True(HttpStatusCode.NoContent == ResponseHttpStatusCode, "HttpStatusCode.NoContent == ResponseHttpStatusCode");
		}
	}
}
