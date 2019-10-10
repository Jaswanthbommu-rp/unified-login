using System.Net;
using Xunit;
using Xunit.Abstractions;
using Newtonsoft.Json;
using RPBooksApiTestingFramework;
using RPBooksApiTestingFramework.Controllers;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using System.Data;

namespace GreenBook.Tests
{
	public class IdentityProviderProviderConfiguration : TestController
	{
		JsonController jsonManager = new JsonController();
		TestUtilities reusable;
		private readonly ITestOutputHelper XunitTestOutPut;
		DataTable identityProviderTypes = new DataTable();

		public IdentityProviderProviderConfiguration(ITestOutputHelper _xUnitTestOutput)
		{
			reusable = new TestUtilities(this);
			this.XunitTestOutPut = _xUnitTestOutput;
		}

		// IdentityProviderProviderConfiguration=/api/identityprovider/providerconfiguration/{IdentityProviderTypeId}

		/*Disabling this test since the GET IdentityProviderProviderConfiguration has already been deprecated 
		 and replaced by GET /api/identityprovider/configuration/{providerName}.*/
		//[Fact, Trait("", "Happy Path")]
		public void GetIdentityProviderProviderConfiguration()
		{
			// Set up the API URL
			identityProviderTypes = reusable.DoSelectIdentityProviderType();
			EndPointUrl = HostIdentityUrl + Properties["IdentityProviderProviderConfiguration"].Replace("{IdentityProviderTypeId}", identityProviderTypes.Rows[0]["IdentityProviderTypeId"].ToString());

			// Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");

			// Extract API's JSON Response
			
			ProviderConfiguration identityProviderTypeResponse = JsonConvert.DeserializeObject<ProviderConfiguration>(ResponseString);
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);
			
			// Assert
			Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
			Assert.NotNull(identityProviderTypeResponse.ProviderPortfolioId);
			//Assert.True(identityProviderTypeResponse.EnterpriseUserName == Properties["enterpriseUsername"], "getSecurityQuestions.enterpriseUserName == strEnterpriseUserName");
		}
	}
}
