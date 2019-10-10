using System.Net;
using Xunit;
using Xunit.Abstractions;
using RPBooksApiTestingFramework;
using RPBooksApiTestingFramework.Controllers;
using System.Data;
using System;

namespace GreenBook.Tests
{
	public class UpdateIdentityTokenExpiry : TestController
	{
		JsonController jsonManager = new JsonController();
		TestUtilities reusable;
		private readonly ITestOutputHelper XunitTestOutPut;
		DataTable tokenDetails = new DataTable();
		string tokenExpiry;

		public UpdateIdentityTokenExpiry(ITestOutputHelper _xUnitTestOutput)
		{
			reusable = new TestUtilities(this);
			this.XunitTestOutPut = _xUnitTestOutput;
		}

		// UpdateIdentityTokenExpiry=/api/IdentityConfig/UpdateIdentityTokenExpiry

		//[Fact, Trait("", "Happy Path")]
		public void GetUpdateIdentityTokenExpiry()
		{
			// Set up the API URL
			tokenDetails = reusable.DoSelectTokens();
			tokenExpiry = DateTimeOffset.Now.ToString("yyyy-MM-dd HH:mm:ss.fffffff zzz");
			EndPointUrl = HostIdentityUrl + Properties["UpdateIdentityTokenExpiry"]
				+ "?tokenKey=" + tokenDetails.Rows[0]["TokenKey"] + "&expiry=" + WebUtility.UrlEncode(tokenExpiry);

			// Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");

			// Extract API's JSON Response
			
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);

			// Assert
			Assert.True(HttpStatusCode.NoContent == ResponseHttpStatusCode, "HttpStatusCode.NoContent == ResponseHttpStatusCode");
		}
	}
}
