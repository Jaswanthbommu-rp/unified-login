using System.Net;
using Xunit;
using Xunit.Abstractions;
using Newtonsoft.Json;
using RPBooksApiTestingFramework;
using RPBooksApiTestingFramework.Controllers;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;

namespace GreenBook.Tests
{
	public class IdentityProviderType : TestController
	{
		public IdentityProviderType(ITestOutputHelper _xUnitTestOutput)
		{
			reusable = new TestUtilities(this);
			this.XunitTestOutPut = _xUnitTestOutput;
		}

		JsonController jsonManager = new JsonController();
		TestUtilities reusable;
		private readonly ITestOutputHelper XunitTestOutPut;

		// IdentityProviderTypes=/api/identityprovider/type

		//[Theory]
		//[Trait("Data-Driven", "Happy Path")]
		[InlineData("GetIdentityProviderType", "local")]
		[InlineData("GetIdentityProviderTypeAzureIdpUser", "aad")]
		[InlineData("GetIdentityProviderTypeGoogleIdpUser", "Google")]
		[InlineData("GetIdentityProviderTypeOktaIdpUser", "ok")]
		public void GetIdentityProviderTypeHappyPaths(string testCase, string identityProvider)
		{
			// Set up the API URL
			string username = Properties["enterpriseUsername"];
			switch (testCase)
			{
				case "GetIdentityProviderTypeAzureIdpUser":
					username = Properties["enterpriseUsernameAzure"];
					break;
				case "GetIdentityProviderTypeGoogleIdpUser":
					username = Properties["enterpriseUsernameGoogle"];
					break;
				case "GetIdentityProviderTypeOktaIdpUser":
					username = Properties["enterpriseUsernameOkta"];
					break;
			}
			EndPointUrl = HostIdentityUrl + Properties["IdentityProviderType"] + "?enterpriseUserName=" + WebUtility.UrlEncode(username);

			// Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get);

			// Extract API's JSON Response
			IdentityProviderTypeOutput identityProviderTypeResponse = JsonConvert.DeserializeObject<IdentityProviderTypeOutput>(ResponseString);
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);

			// Assert
			Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
			Assert.NotNull(identityProviderTypeResponse.identityProviderType.AuthenticationType);
			Assert.True(identityProviderTypeResponse.identityProviderType.AuthenticationType.Contains(identityProvider), $"identityProviderTypeResponse.identityProviderType.AuthenticationType is not \"{identityProvider}\".");
		}
		
		//[Theory]
		//[Trait("Data-Driven", "Negative Case")]
		[InlineData("GetIdentityProviderTypeNoUsername", "BadRequest", "Invalid parameter: enterpriseUserName")]
		[InlineData("GetIdentityProviderTypeInvalidUsername", "NoContent", "", "invalidUsername")]
		public void GetIdentityProviderTypeNegativeCases(string testCase, string httpStatusCode, string errorReason, string username = "")
		{
			// Set up the API URL
			EndPointUrl = HostIdentityUrl + Properties["IdentityProviderType"] + "?enterpriseUserName=" + username;

			// Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get);

			// Extract API's JSON Response
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);

			// Assert
			if (httpStatusCode == "BadRequest")
			{
				Assert.True(HttpStatusCode.BadRequest == ResponseHttpStatusCode, "HttpStatusCode.BadRequest != ResponseHttpStatusCode");
				Assert.True(ResponseString.Contains(errorReason), $"!ResponseString.Contains(\"{errorReason}\")");
			}
			else if (httpStatusCode == "NoContent")
			{
				Assert.True(HttpStatusCode.NoContent == ResponseHttpStatusCode, "HttpStatusCode.NoContent != ResponseHttpStatusCode");
			}
		}
	}
}
