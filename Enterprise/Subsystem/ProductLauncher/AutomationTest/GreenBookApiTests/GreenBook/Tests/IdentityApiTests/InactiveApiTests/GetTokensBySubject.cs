using System.Net;
using Xunit;
using Xunit.Abstractions;
using Newtonsoft.Json;
using RPBooksApiTestingFramework;
using RPBooksApiTestingFramework.Controllers;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using System.Data;
using System;
using System.Collections.Generic;

namespace GreenBook.Tests
{
	public class GetTokensBySubject : TestController
	{
		JsonController jsonManager = new JsonController();
		TestUtilities reusable;
		private readonly ITestOutputHelper XunitTestOutPut;
		DataTable tokenDetails = new DataTable();
		string tokenExpiry, tokenAuthCodeChallenge = null, tokenAuthCodeChallengeMethod = null
			, tokenNonce = null, tokenRedirectUri = null, tokenSessionId = null;
		bool? tokenIsOpenId = null, tokenWasConsentShown = null;
		int countToken = 0;

		public GetTokensBySubject(ITestOutputHelper _xUnitTestOutput)
		{
			this.XunitTestOutPut = _xUnitTestOutput;
		}

		// GetTokensBySubject=/api/IdentityConfig/GetTokensBySubject

		//[Fact, Trait("", "Happy Path")]
		public void GetGetTokensBySubject()
		{
			// Set up the API URL
			tokenDetails = reusable.DoSelectTokens();
			EndPointUrl = HostIdentityUrl + Properties["GetTokensBySubject"]
				+ "?subjectCode=" + tokenDetails.Rows[0]["SubjectCode"] + "&tokenType=" + tokenDetails.Rows[0]["TokenType"];

			// Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");

			// Extract API's JSON Response
			
			IList<Token> tokenResponse = JsonConvert.DeserializeObject<IList<Token>>(ResponseString);
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);

			// Assert
			Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");

			if (tokenResponse != null)
			{
				foreach (Token token in tokenResponse)
				{
					Assert.NotNull(token.TokenKey);
					Assert.True(token.TokenKey == tokenDetails.Rows[countToken]["TokenKey"].ToString(), "token.TokenKey == tokenDetails.Rows[countToken][\"TokenKey\"].ToString()");
					Assert.NotNull(token.TokenType);
					Assert.True(token.TokenType == Convert.ToInt32(tokenDetails.Rows[countToken]["TokenType"]), "token.TokenKey == Convert.ToInt32(tokenDetails.Rows[countToken][\"TokenType\"])");
					Assert.NotNull(token.ClientCode);
					Assert.True(token.ClientCode == tokenDetails.Rows[countToken]["ClientCode"].ToString(), "token.ClientCode == tokenDetails.Rows[countToken][\"ClientCode\"].ToString()");
					Assert.NotNull(token.SubjectCode);
					Assert.True(token.SubjectCode == tokenDetails.Rows[countToken]["SubjectCode"].ToString(), "token.SubjectCode == tokenDetails.Rows[countToken][\"SubjectCode\"].ToString()");

					if (tokenDetails.Rows[countToken]["Expiry"].ToString().Length > 0)
					{
						tokenExpiry = string.Concat(tokenDetails.Rows[countToken]["Expiry"]);
					}

					Assert.NotNull(token.Expiry);
					Assert.True(token.Expiry.ToString().Contains(tokenExpiry), "token.Expiry.ToString().Contains(tokenExpiry.ToString())");
					Assert.NotNull(token.JsonCode);
					Assert.True(token.JsonCode == tokenDetails.Rows[countToken]["JsonCode"].ToString(), "token.JsonCode == tokenDetails.Rows[countToken][\"JsonCode\"].ToString()");

					if (tokenDetails.Rows[countToken]["AuthCodeChallenge"].ToString().Length > 0)
					{
						tokenAuthCodeChallenge = tokenDetails.Rows[countToken]["AuthCodeChallenge"].ToString();
					}

					Assert.True(token.AuthCodeChallenge == tokenAuthCodeChallenge, "token.AuthCodeChallenge == tokenAuthCodeChallenge");

					if (tokenDetails.Rows[countToken]["AuthCodeChallengeMethod"].ToString().Length > 0)
					{
						tokenAuthCodeChallengeMethod = tokenDetails.Rows[countToken]["AuthCodeChallengeMethod"].ToString();
					}

					Assert.True(token.AuthCodeChallengeMethod == tokenAuthCodeChallengeMethod, "token.AuthCodeChallengeMethod == tokenAuthCodeChallengeMethod");

					if (tokenDetails.Rows[countToken]["IsOpenId"].ToString().Length > 0)
					{
						tokenIsOpenId = Convert.ToBoolean(tokenDetails.Rows[countToken]["IsOpenId"]);
					}

					Assert.True(token.IsOpenId == tokenIsOpenId, "token.IsOpenId == tokenIsOpenId");

					if (tokenDetails.Rows[countToken]["Nonce"].ToString().Length > 0)
					{
						tokenNonce = tokenDetails.Rows[countToken]["Nonce"].ToString();
					}

					Assert.True(token.Nonce == tokenNonce, "token.Nonce == tokenNonce");

					if (tokenDetails.Rows[countToken]["RedirectUri"].ToString().Length > 0)
					{
						tokenRedirectUri = tokenDetails.Rows[countToken]["RedirectUri"].ToString();
					}

					Assert.True(token.RedirectUri == tokenRedirectUri, "token.RedirectUri == tokenRedirectUri");

					if (tokenDetails.Rows[countToken]["SessionId"].ToString().Length > 0)
					{
						tokenSessionId = tokenDetails.Rows[countToken]["SessionId"].ToString();
					}

					Assert.True(token.SessionId == tokenSessionId, "token.SessionId == tokenSessionId");

					if (tokenDetails.Rows[countToken]["WasConsentShown"].ToString().Length > 0)
					{
						tokenWasConsentShown = Convert.ToBoolean(tokenDetails.Rows[countToken]["WasConsentShown"]);
					}

					Assert.True(token.WasConsentShown == tokenWasConsentShown, "token.WasConsentShown == tokenWasConsentShown");

					countToken++;
				}
				countToken = 0;
			}
		}
	}
}
