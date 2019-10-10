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
	public class GetAllScopeClaims : TestController
	{
		JsonController jsonManager = new JsonController();
		TestUtilities reusable;
		private readonly ITestOutputHelper XunitTestOutPut;
		DataTable allScopeClaims = new DataTable();
		int countScopeClaims = 0;

		public GetAllScopeClaims(ITestOutputHelper _xUnitTestOutput)
		{
			this.XunitTestOutPut = _xUnitTestOutput;
		}

		// GetAllScopeClaims=/api/IdentityConfig/GetAllScopeClaims

		//[Fact, Trait("", "Happy Path")]
		public void GetGetAllScopeClaims()
		{
			// Set up the API URL
			allScopeClaims = reusable.DoSelectScopeClaims();
			EndPointUrl = HostIdentityUrl + Properties["GetAllScopeClaims"];

			// Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");

			// Extract API's JSON Response
			
			IList<ScopeClaim> allScopeClaimResponse = JsonConvert.DeserializeObject<IList<ScopeClaim>>(ResponseString);
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);

			// Assert
			Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");

			if (allScopeClaimResponse.Count > 0)
			{
				foreach (ScopeClaim scopeClaim in allScopeClaimResponse)
				{
					Assert.NotNull(scopeClaim.Id);
					Assert.True(scopeClaim.Id == int.Parse(allScopeClaims.Rows[countScopeClaims]["ScopeClaimId"].ToString()), "scopeClaim.Id == int.Parse(allScopeClaims.Rows[countScopeClaims][\"ScopeClaimId\"].ToString())");
					Assert.NotNull(scopeClaim.Name);
					Assert.True(scopeClaim.Name == allScopeClaims.Rows[countScopeClaims]["Name"].ToString(), "scopeClaim.Name == allScopeClaims.Rows[countScopeClaims][\"Name\"].ToString()");
					Assert.NotNull(scopeClaim.Description);
					Assert.True(scopeClaim.Description == allScopeClaims.Rows[countScopeClaims]["Description"].ToString(), "scopeClaim.Description == allScopeClaims.Rows[countScopeClaims][\"Description\"].ToString()");
					Assert.NotNull(scopeClaim.AlwaysIncludeInIdToken);
					Assert.True(scopeClaim.AlwaysIncludeInIdToken == Convert.ToBoolean(allScopeClaims.Rows[countScopeClaims]["AlwaysIncludeInIdToken"]), "scopeClaim.AlwaysIncludeInIdToken == Convert.ToBoolean(allScopeClaims.Rows[countScopeClaims][\"AlwaysIncludeInIdToken\"])");
					Assert.NotNull(scopeClaim.ScopeId);
					Assert.True(scopeClaim.ScopeId == int.Parse(allScopeClaims.Rows[countScopeClaims]["ScopeId"].ToString()), "scopeClaim.ScopeId == int.Parse(allScopeClaims.Rows[countScopeClaims][\"ScopeId\"].ToString())");

					if (scopeClaim.Scope != null)
					{

					}

					countScopeClaims++;
				}

				countScopeClaims = 0;
			}
		}
	}
}
