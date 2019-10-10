using System.Net;
using Xunit;
using Xunit.Abstractions;
using Newtonsoft.Json;
using RPBooksApiTestingFramework;
using RPBooksApiTestingFramework.Controllers;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using System.Collections.Generic;

namespace GreenBook.Tests
{
	public class IdentityProviderConfiguration : TestController
	{
		JsonController jsonManager = new JsonController();
		DatabaseController dbManager = new DatabaseController("");
		TestUtilities reusable;
		private readonly ITestOutputHelper XunitTestOutPut;

		public IdentityProviderConfiguration(ITestOutputHelper _xUnitTestOutput)
		{
			reusable = new TestUtilities(this);
			this.XunitTestOutPut = _xUnitTestOutput;
		}

		// IdentityProviderConfiguration=/api/identityprovider/configuration/{providerName}

		//[Fact, Trait("", "Happy Path")]
		public void GetIdentityProviderConfiguration()
		{
			// Set up the API URL
			string providerName = "okta";
			EndPointUrl = HostIdentityUrl + Properties["IdentityProviderConfiguration"].Replace("{providerName}", providerName);

			// Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get);

			// Extract API's JSON Response
			IList<ProviderConfiguration> identityProviderConfigurationListResponse = JsonConvert.DeserializeObject<IList<ProviderConfiguration>>(ResponseString);
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);

			// Assert
			Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
			
			Assert.NotNull(identityProviderConfigurationListResponse[0].ProviderPortfolioId);
			Assert.True(identityProviderConfigurationListResponse[0].ProviderPortfolioId == 0, "identityProviderConfigurationListResponse[0].ProviderPortfolioId == 0");
			Assert.NotNull(identityProviderConfigurationListResponse[0].PortfolioIdId);
			Assert.True(identityProviderConfigurationListResponse[0].PortfolioIdId == 0, "identityProviderConfigurationListResponse[0].PortfolioIdId == 0");
			Assert.NotNull(identityProviderConfigurationListResponse[0].AuthenticationMode);
			Assert.True(identityProviderConfigurationListResponse[0].AuthenticationMode >= 0, "identityProviderConfigurationListResponse[0].AuthenticationMode >= 0");
			Assert.NotNull(identityProviderConfigurationListResponse[0].ValidateIssuer);
			Assert.True(identityProviderConfigurationListResponse[0].ValidateIssuer, "identityProviderConfigurationListResponse[0].ValidateIssuer");
			Assert.NotNull(identityProviderConfigurationListResponse[0].ProviderName);
			Assert.True(identityProviderConfigurationListResponse[0].ProviderName.ToLower() == providerName, "identityProviderConfigurationListResponse[0].ProviderName.ToLower() == providerName");
			Assert.NotNull(identityProviderConfigurationListResponse[0].Description);
			Assert.True(identityProviderConfigurationListResponse[0].Description.ToLower().StartsWith(providerName), "identityProviderConfigurationListResponse[0].Description.ToLower().StartsWith(providerName)");
			Assert.NotNull(identityProviderConfigurationListResponse[0].AuthenticationType);
			Assert.True(identityProviderConfigurationListResponse[0].AuthenticationType.ToLower().StartsWith(providerName), "identityProviderConfigurationListResponse[0].AuthenticationType.ToLower().StartsWith(providerName)");
			Assert.NotNull(identityProviderConfigurationListResponse[0].Caption);
			Assert.True(identityProviderConfigurationListResponse[0].Caption.ToLower() == "sign in with " + providerName, "identityProviderConfigurationListResponse[0].Caption.ToLower() == \"sign in with \" + providerName");
			Assert.Null(identityProviderConfigurationListResponse[0].ProviderClientId);
			Assert.NotNull(identityProviderConfigurationListResponse[0].AuthorityUri);
			Assert.True(identityProviderConfigurationListResponse[0].AuthorityUri.StartsWith(Properties["identityClientUrl"].Replace("/connect/token", ""))
				, "identityProviderConfigurationListResponse[0].AuthorityUri.StartsWith(Properties[\"identityClientUrl\"].Replace(\"/connect/token\", \"\"))");
			Assert.Null(identityProviderConfigurationListResponse[0].PostLogoutRedirectUri);
			Assert.NotNull(identityProviderConfigurationListResponse[0].RedirectUri);
			Assert.True(identityProviderConfigurationListResponse[0].RedirectUri.ToLower().StartsWith(HostUrl.Replace("2", "") + "/auth.aspx?idp=" + providerName)
				, "identityProviderConfigurationListResponse[0].RedirectUri.ToLower().StartsWith(HostUrl.Replace(\"2\", \"\")+\"auth.aspx?idp=\" + providerName");
			Assert.Null(identityProviderConfigurationListResponse[0].TokenValidationAuthenticationType);
			Assert.Null(identityProviderConfigurationListResponse[0].Scope);
			Assert.NotNull(identityProviderConfigurationListResponse[0].OktaEntityId);
			Assert.True(identityProviderConfigurationListResponse[0].OktaEntityId.Length == 20, "identityProviderConfigurationListResponse[0].OktaEntityId.Length == 20");
			Assert.NotNull(identityProviderConfigurationListResponse[0].OktaMetadataLocation);
			Assert.True(identityProviderConfigurationListResponse[0].OktaMetadataLocation == "https://dev-489082.oktapreview.com/app/" + identityProviderConfigurationListResponse[0].OktaEntityId + "/sso/saml/metadata");
			Assert.Null(identityProviderConfigurationListResponse[0].ClientSecret);
			
		}
		
		//[Fact, Trait("", "Negative Case")]
		public void GetIdentityProviderConfigurationInvalidProviderName()
		{
			// Set up the API URL
			EndPointUrl = HostIdentityUrl + Properties["IdentityProviderConfiguration"].Replace("{providerName}", "invalidProviderName");

			// Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get);

			// Extract API's JSON Response
			IList<ProviderConfiguration> identityProviderConfigurationListResponse = JsonConvert.DeserializeObject<IList<ProviderConfiguration>>(ResponseString);
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);

			// Assert
			Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
			Assert.NotNull(identityProviderConfigurationListResponse);
			Assert.True(identityProviderConfigurationListResponse.Count <= 0, "identityProviderConfigurationListResponse.Count <= 0");
		}
	}
}
