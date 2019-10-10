using System.Net;
using Xunit;
using Xunit.Abstractions;
using Newtonsoft.Json;
using RPBooksApiTestingFramework;
using RPBooksApiTestingFramework.Controllers;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using System.Data;
using System;

namespace GreenBook.Tests
{
	public class GetIdentityToken : TestController
	{
		JsonController jsonManager = new JsonController();
		TestUtilities reusable;
		private readonly ITestOutputHelper XunitTestOutPut;
		DataTable tokenDetails = new DataTable();
		string tokenExpiry, tokenAuthCodeChallenge = null, tokenAuthCodeChallengeMethod = null
			, tokenNonce = null, tokenRedirectUri = null, tokenSessionId = null;
		bool? tokenIsOpenId = null, tokenWasConsentShown = null;

		public GetIdentityToken(ITestOutputHelper _xUnitTestOutput)
		{
			this.XunitTestOutPut = _xUnitTestOutput;
		}

		// GetIdentityToken=/api/IdentityConfig/GetIdentityToken

		//[Fact, Trait("", "Happy Path")]
		public void GetGetIdentityToken()
		{
			// Set up the API URL
			tokenDetails = reusable.DoSelectTokens();
			EndPointUrl = HostIdentityUrl + Properties["GetIdentityToken"]
				+ "?tokenKey=" + tokenDetails.Rows[0]["TokenKey"] + "&tokenType=" + tokenDetails.Rows[0]["TokenType"];

			// Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");

			// Extract API's JSON Response
			
			Token tokenResponse = JsonConvert.DeserializeObject<Token>(ResponseString);
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);

			// Assert
			Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
			Assert.NotNull(tokenResponse.TokenKey);
			Assert.True(tokenResponse.TokenKey == tokenDetails.Rows[0]["TokenKey"].ToString(), "tokenResponse.TokenKey == tokenDetails.Rows[0][\"TokenKey\"].ToString()");
			Assert.NotNull(tokenResponse.TokenType);
			Assert.True(tokenResponse.TokenType == Convert.ToInt32(tokenDetails.Rows[0]["TokenType"]), "tokenResponse.TokenKey == Convert.ToInt32(tokenDetails.Rows[0][\"TokenType\"])");
			Assert.NotNull(tokenResponse.ClientCode);
			Assert.True(tokenResponse.ClientCode == tokenDetails.Rows[0]["ClientCode"].ToString(), "tokenResponse.ClientCode == tokenDetails.Rows[0][\"ClientCode\"].ToString()");
			Assert.NotNull(tokenResponse.SubjectCode);
			Assert.True(tokenResponse.SubjectCode == tokenDetails.Rows[0]["SubjectCode"].ToString(), "tokenResponse.SubjectCode == tokenDetails.Rows[0][\"SubjectCode\"].ToString()");

			if (tokenDetails.Rows[0]["Expiry"].ToString().Length > 0)
			{
				tokenExpiry = string.Concat(tokenDetails.Rows[0]["Expiry"]);
			}

			Assert.NotNull(tokenResponse.Expiry);
			Assert.True(tokenResponse.Expiry.ToString().Contains(tokenExpiry), "tokenResponse.Expiry.ToString().Contains(tokenExpiry.ToString())");
			Assert.NotNull(tokenResponse.JsonCode);
			Assert.True(tokenResponse.JsonCode == tokenDetails.Rows[0]["JsonCode"].ToString(), "tokenResponse.JsonCode == tokenDetails.Rows[0][\"JsonCode\"].ToString()");

			if (tokenDetails.Rows[0]["AuthCodeChallenge"].ToString().Length > 0)
			{
				tokenAuthCodeChallenge = tokenDetails.Rows[0]["AuthCodeChallenge"].ToString();
			}

			Assert.True(tokenResponse.AuthCodeChallenge == tokenAuthCodeChallenge, "tokenResponse.AuthCodeChallenge == tokenAuthCodeChallenge");

			if (tokenDetails.Rows[0]["AuthCodeChallengeMethod"].ToString().Length > 0)
			{
				tokenAuthCodeChallengeMethod = tokenDetails.Rows[0]["AuthCodeChallengeMethod"].ToString();
			}

			Assert.True(tokenResponse.AuthCodeChallengeMethod == tokenAuthCodeChallengeMethod, "tokenResponse.AuthCodeChallengeMethod == tokenAuthCodeChallengeMethod");

			if (tokenDetails.Rows[0]["IsOpenId"].ToString().Length > 0)
			{
				tokenIsOpenId = Convert.ToBoolean(tokenDetails.Rows[0]["IsOpenId"]);
			}

			Assert.True(tokenResponse.IsOpenId == tokenIsOpenId, "tokenResponse.IsOpenId == tokenIsOpenId");

			if (tokenDetails.Rows[0]["Nonce"].ToString().Length > 0)
			{
				tokenNonce = tokenDetails.Rows[0]["Nonce"].ToString();
			}

			Assert.True(tokenResponse.Nonce == tokenNonce, "tokenResponse.Nonce == tokenNonce");

			if (tokenDetails.Rows[0]["RedirectUri"].ToString().Length > 0)
			{
				tokenRedirectUri = tokenDetails.Rows[0]["RedirectUri"].ToString();
			}

			Assert.True(tokenResponse.RedirectUri == tokenRedirectUri, "tokenResponse.RedirectUri == tokenRedirectUri");

			if (tokenDetails.Rows[0]["SessionId"].ToString().Length > 0)
			{
				tokenSessionId = tokenDetails.Rows[0]["SessionId"].ToString();
			}

			Assert.True(tokenResponse.SessionId == tokenSessionId, "tokenResponse.SessionId == tokenSessionId");

			if (tokenDetails.Rows[0]["WasConsentShown"].ToString().Length > 0)
			{
				tokenWasConsentShown = Convert.ToBoolean(tokenDetails.Rows[0]["WasConsentShown"]);
			}

			Assert.True(tokenResponse.WasConsentShown == tokenWasConsentShown, "tokenResponse.WasConsentShown == tokenWasConsentShown");
		}				
	}
}
