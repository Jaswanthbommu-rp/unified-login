using System.Net;
using Xunit;
using Xunit.Abstractions;
using Newtonsoft.Json;
using RPBooksApiTestingFramework;
using RPBooksApiTestingFramework.Controllers;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using System.Data;
using System;

namespace GreenBook.Tests
{
	public class GetClient : TestController
	{
		JsonController jsonManager = new JsonController();
		TestUtilities reusable;
		private readonly ITestOutputHelper XunitTestOutPut;
		DataTable clientDetails = new DataTable();
		private int countClientDetails = 0;
		string logoutUri, clientUri, logoUri, clientSecretType, clientSecretDescription;
		DateTime clientSecretExpiration;

		public GetClient(ITestOutputHelper _xUnitTestOutput)
		{
			this.XunitTestOutPut = _xUnitTestOutput;
		}

		// GetClient=/api/IdentityConfig/GetClient

		//[Fact, Trait("", "Happy Path")]
		public void GetGetClient()
		{
			// Set up the API URL
			clientDetails = reusable.DoSelectClient();
			EndPointUrl = HostIdentityUrl + Properties["GetClient"]	+ "?clientCode=" + clientDetails.Rows[0]["ClientCode"].ToString();

			// Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");

			// Extract API's JSON Response
			
			Client clientResponse = JsonConvert.DeserializeObject<Client>(ResponseString);
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);
			
			// Assert
			Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
			Assert.NotNull(clientResponse.ClientId);
			Assert.True(clientResponse.ClientId == int.Parse(clientDetails.Rows[0]["ClientId"].ToString()), "clientResponse.UserId == int.Parse(clientDetails.Rows[0][\"ClientId\"].ToString())");
			Assert.NotNull(clientResponse.Enabled);
			Assert.True(clientResponse.Enabled == Convert.ToBoolean(clientDetails.Rows[0]["Enabled"]), "clientResponse.Enabled == Convert.ToBoolean(clientDetails.Rows[0][\"Enabled\"])");
			Assert.NotNull(clientResponse.ClientCode);
			Assert.True(clientResponse.ClientCode == clientDetails.Rows[0]["ClientCode"].ToString(), "clientResponse.ClientCode == clientDetails.Rows[0][\"ClientCode\"].ToString()");
			Assert.NotNull(clientResponse.ClientName);
			Assert.True(clientResponse.ClientName == clientDetails.Rows[0]["ClientName"].ToString(), "clientResponse.ClientName == clientDetails.Rows[0][\"ClientName\"].ToString()");

			clientUri = clientDetails.Rows[0]["ClientUri"].ToString().Length <= 0 ? null : clientDetails.Rows[0]["ClientUri"].ToString();

			Assert.True(clientResponse.ClientUri == clientUri, "clientResponse.ClientUri == clientUri");

			logoUri = clientDetails.Rows[0]["LogoUri"].ToString().Length <= 0 ? null : clientDetails.Rows[0]["LogoUri"].ToString();
			
			Assert.True(clientResponse.LogoUri == logoUri, "clientResponse.LogoUri == logoUri");
			Assert.NotNull(clientResponse.RequireConsent);
			Assert.True(clientResponse.RequireConsent == Convert.ToBoolean(clientDetails.Rows[0]["RequireConsent"]), "clientResponse.RequireConsent == Convert.ToBoolean(clientDetails.Rows[0][\"RequireConsent\"])");
			Assert.NotNull(clientResponse.AllowClientCredentialsOnly);
			Assert.True(clientResponse.AllowClientCredentialsOnly == Convert.ToBoolean(clientDetails.Rows[0]["AllowClientCredentialsOnly"]), "clientResponse.AllowClientCredentialsOnly == Convert.ToBoolean(clientDetails.Rows[0][\"AllowClientCredentialsOnly\"])");

			logoutUri = clientDetails.Rows[0]["LogoutUri"].ToString().Length <= 0 ? null : clientDetails.Rows[0]["LogoutUri"].ToString();

			Assert.True(clientResponse.LogoutUri == logoutUri, "clientResponse.LogoutUri == logoutUri");
			Assert.NotNull(clientResponse.LogoutSessionRequired);
			Assert.True(clientResponse.LogoutSessionRequired == Convert.ToBoolean(clientDetails.Rows[0]["LogoutSessionRequired"]), "clientResponse.LogoutSessionRequired == Convert.ToBoolean(clientDetails.Rows[0][\"LogoutSessionRequired\"])");
			Assert.NotNull(clientResponse.RequireSignOutPrompt);
			Assert.True(clientResponse.RequireSignOutPrompt == Convert.ToBoolean(clientDetails.Rows[0]["RequireSignOutPrompt"]), "clientResponse.RequireSignOutPrompt == Convert.ToBoolean(clientDetails.Rows[0][\"RequireSignOutPrompt\"])");
			Assert.NotNull(clientResponse.AllowAccessToAllScopes);
			Assert.True(clientResponse.AllowAccessToAllScopes == Convert.ToBoolean(clientDetails.Rows[0]["AllowAccessToAllScopes"]), "clientResponse.AllowAccessToAllScopes == Convert.ToBoolean(clientDetails.Rows[0][\"AllowAccessToAllScopes\"])");
			Assert.NotNull(clientResponse.IdentityTokenLifetime);
			Assert.True(clientResponse.IdentityTokenLifetime == int.Parse(clientDetails.Rows[0]["IdentityTokenLifetime"].ToString()), "clientResponse.IdentityTokenLifetime == int.Parse(clientDetails.Rows[0][\"IdentityTokenLifetime\"].ToString())");
			Assert.NotNull(clientResponse.AccessTokenLifetime);
			Assert.True(clientResponse.AccessTokenLifetime == int.Parse(clientDetails.Rows[0]["AccessTokenLifetime"].ToString()), "clientResponse.AccessTokenLifetime == int.Parse(clientDetails.Rows[0][\"AccessTokenLifetime\"].ToString())");
			Assert.NotNull(clientResponse.AuthorizationCodeLifetime);
			Assert.True(clientResponse.AuthorizationCodeLifetime == int.Parse(clientDetails.Rows[0]["AuthorizationCodeLifetime"].ToString()), "clientResponse.AccessTokenLifetime == int.Parse(clientDetails.Rows[0][\"AuthorizationCodeLifetime\"].ToString())");
			Assert.NotNull(clientResponse.AbsoluteRefreshTokenLifetime);
			Assert.True(clientResponse.AbsoluteRefreshTokenLifetime == int.Parse(clientDetails.Rows[0]["AbsoluteRefreshTokenLifetime"].ToString()), "clientResponse.AbsoluteRefreshTokenLifetime == int.Parse(clientDetails.Rows[0][\"AbsoluteRefreshTokenLifetime\"].ToString())");
			Assert.NotNull(clientResponse.SlidingRefreshTokenLifetime);
			Assert.True(clientResponse.SlidingRefreshTokenLifetime == int.Parse(clientDetails.Rows[0]["SlidingRefreshTokenLifetime"].ToString()), "clientResponse.SlidingRefreshTokenLifetime == int.Parse(clientDetails.Rows[0][\"SlidingRefreshTokenLifetime\"].ToString())");
			Assert.NotNull(clientResponse.UpdateAccessTokenOnRefresh);
			Assert.True(clientResponse.UpdateAccessTokenOnRefresh == Convert.ToBoolean(clientDetails.Rows[0]["UpdateAccessTokenOnRefresh"]), "clientResponse.UpdateAccessTokenOnRefresh == Convert.ToBoolean(clientDetails.Rows[0][\"UpdateAccessTokenOnRefresh\"])");
			Assert.NotNull(clientResponse.RefreshTokenUsage);
			Assert.True(clientResponse.RefreshTokenUsage == int.Parse(clientDetails.Rows[0]["RefreshTokenUsage"].ToString()), "clientResponse.RefreshTokenUsage == int.Parse(clientDetails.Rows[0][\"RefreshTokenUsage\"].ToString())");
			Assert.NotNull(clientResponse.AccessTokenType);
			Assert.True(clientResponse.AccessTokenType == int.Parse(clientDetails.Rows[0]["AccessTokenType"].ToString()), "clientResponse.AccessTokenType == int.Parse(clientDetails.Rows[0][\"AccessTokenType\"].ToString())");
			Assert.NotNull(clientResponse.EnableLocalLogin);
			Assert.True(clientResponse.EnableLocalLogin == Convert.ToBoolean(clientDetails.Rows[0]["EnableLocalLogin"]), "clientResponse.EnableLocalLogin == Convert.ToBoolean(clientDetails.Rows[0][\"EnableLocalLogin\"])");
			Assert.NotNull(clientResponse.IncludeJwtId);
			Assert.True(clientResponse.IncludeJwtId == Convert.ToBoolean(clientDetails.Rows[0]["IncludeJwtId"]), "clientResponse.IncludeJwtId == Convert.ToBoolean(clientDetails.Rows[0][\"IncludeJwtId\"])");
			Assert.NotNull(clientResponse.AlwaysSendClientClaims);
			Assert.True(clientResponse.AlwaysSendClientClaims == Convert.ToBoolean(clientDetails.Rows[0]["AlwaysSendClientClaims"]), "clientResponse.AlwaysSendClientClaims == Convert.ToBoolean(clientDetails.Rows[0][\"AlwaysSendClientClaims\"])");
			Assert.NotNull(clientResponse.PrefixClientClaims);
			Assert.True(clientResponse.PrefixClientClaims == Convert.ToBoolean(clientDetails.Rows[0]["PrefixClientClaims"]), "clientResponse.PrefixClientClaims == Convert.ToBoolean(clientDetails.Rows[0][\"PrefixClientClaims\"])");
			Assert.NotNull(clientResponse.AllowAccessToAllGrantTypes);
			Assert.True(clientResponse.AllowAccessToAllGrantTypes == Convert.ToBoolean(clientDetails.Rows[0]["AllowAccessToAllGrantTypes"]), "clientResponse.AllowAccessToAllGrantTypes == Convert.ToBoolean(clientDetails.Rows[0][\"AllowAccessToAllGrantTypes\"])");

			if (clientResponse.ClientSecrets != null)
			{
				DataTable expectedClientSecrets = reusable.DoSelectClientSecrets(clientDetails.Rows[0]["ClientId"].ToString());
				foreach (ClientSecret clientSecret in clientResponse.ClientSecrets)
				{
					Assert.NotNull(clientSecret.Value);
					Assert.True(clientSecret.Value == expectedClientSecrets.Rows[countClientDetails]["Value"].ToString(), "clientSecret.Value == expectedClientSecrets.Rows[countClientDetails][\"Value\"].ToString()");

					clientSecretType = expectedClientSecrets.Rows[countClientDetails]["Type"].ToString().Length <= 0 ? null : expectedClientSecrets.Rows[countClientDetails]["Type"].ToString();

					Assert.True(clientSecret.Type == clientSecretType, "clientSecret.Type == clientSecretType");

					clientSecretDescription = expectedClientSecrets.Rows[countClientDetails]["Description"].ToString().Length <= 0 ? null : expectedClientSecrets.Rows[countClientDetails]["Description"].ToString();

					Assert.True(clientSecret.Description == clientSecretDescription, "clientSecret.Description == clientSecretDescription");

					if (expectedClientSecrets.Rows[countClientDetails]["Expiration"].ToString().Length > 0)
					{
						clientSecretExpiration = Convert.ToDateTime(expectedClientSecrets.Rows[countClientDetails]["Expiration"]);
					}
					
					Assert.NotNull(clientSecret.Expiration);
					Assert.True(clientSecret.Expiration.ToString().Contains(clientSecretExpiration.ToString()), "clientSecret.Expiration == clientSecretExpiration");
					Assert.NotNull(clientSecret.ClientId);
					Assert.True(clientSecret.ClientId == int.Parse(expectedClientSecrets.Rows[countClientDetails]["ClientId"].ToString()), "clientSecret.ClientId == int.Parse(expectedClientSecrets.Rows[countClientDetails][\"ClientId\"].ToString()");

					countClientDetails++;
				}
				countClientDetails = 0;
			}

			if (clientResponse.ClientRedirectUris != null)
			{
				DataTable expectedClientRedirectUris = reusable.DoSelectClientRedirectUris(clientDetails.Rows[0]["ClientId"].ToString());
				foreach (ClientRedirectUri clientRedirectUri in clientResponse.ClientRedirectUris)
				{
					Assert.NotNull(clientRedirectUri.Uri);
					Assert.True(clientRedirectUri.Uri == expectedClientRedirectUris.Rows[countClientDetails]["Uri"].ToString(), "clientRedirectUri.Uri == expectedClientSecrets.Rows[countClientDetails][\"Uri\"].ToString()");
					Assert.NotNull(clientRedirectUri.ClientId);
					Assert.True(clientRedirectUri.ClientId == int.Parse(expectedClientRedirectUris.Rows[countClientDetails]["ClientId"].ToString()), "clientRedirectUri.ClientId == int.Parse(expectedClientSecrets.Rows[countClientDetails][\"ClientId\"].ToString()");

					countClientDetails++;
				}
				countClientDetails = 0;
			}

			if (clientResponse.ClientScopes != null)
			{
				DataTable expectedClientScopes = reusable.DoSelectClientScopes(clientDetails.Rows[0]["ClientId"].ToString());
				foreach (ClientScope clientScope in clientResponse.ClientScopes)
				{
					Assert.NotNull(clientScope.Scope);
					Assert.True(clientScope.Scope == expectedClientScopes.Rows[countClientDetails]["Scope"].ToString(), "clientRedirectUri.Scope == expectedClientScopes.Rows[countClientDetails][\"Scope\"].ToString()");
					Assert.NotNull(clientScope.ClientId);
					Assert.True(clientScope.ClientId == int.Parse(expectedClientScopes.Rows[countClientDetails]["ClientId"].ToString()), "clientScope.ClientId == int.Parse(expectedClientScopes.Rows[countClientDetails][\"ClientId\"].ToString()");

					countClientDetails++;
				}
				countClientDetails = 0;
			}

			if (clientResponse.ClientPostLogoutRedirectUris != null)
			{
				DataTable expectedClientPostLogoutRedirectUris = reusable.DoSelectClientPostLogoutRedirectUris(clientDetails.Rows[0]["ClientId"].ToString());
				foreach (ClientPostLogoutRedirectUri clientPostLogoutRedirectUri in clientResponse.ClientPostLogoutRedirectUris)
				{
					Assert.NotNull(clientPostLogoutRedirectUri.Uri);
					Assert.True(clientPostLogoutRedirectUri.Uri == expectedClientPostLogoutRedirectUris.Rows[countClientDetails]["Uri"].ToString(), "clientPostLogoutRedirectUri.Uri == expectedClientPostLogoutRedirectUris.Rows[countClientDetails][\"Uri\"].ToString()");
					Assert.NotNull(clientPostLogoutRedirectUri.ClientId);
					Assert.True(clientPostLogoutRedirectUri.ClientId == int.Parse(expectedClientPostLogoutRedirectUris.Rows[countClientDetails]["ClientId"].ToString()), "clientPostLogoutRedirectUri.ClientId == int.Parse(expectedClientPostLogoutRedirectUris.Rows[countClientDetails][\"ClientId\"].ToString()");

					countClientDetails++;
				}
				countClientDetails = 0;
			}
		}
	}
}
