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
	public class GetAllScopes : TestController
	{
		JsonController jsonManager = new JsonController();
		TestUtilities reusable;
		private readonly ITestOutputHelper XunitTestOutPut;
		DataTable scopeDetails = new DataTable();
		int countScopes = 0;
		string scopeDisplayName = null, scopeDescription = null, scopeClaimsRule = null;

		public GetAllScopes(ITestOutputHelper _xUnitTestOutput)
		{
			this.XunitTestOutPut = _xUnitTestOutput;
		}

		// GetAllScopes=/api/IdentityConfig/GetAllScopes

		//[Fact, Trait("", "Happy Path")]
		public void GetGetAllScopes()
		{
			// Set up the API URL
			scopeDetails = reusable.DoSelectScopes();
			EndPointUrl = HostIdentityUrl + Properties["GetAllScopes"];

			// Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get);

			// Extract API's JSON Response
			
			IList<Scope> allScopeResponse = JsonConvert.DeserializeObject<IList<Scope>>(ResponseString);
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);

			// Assert
			Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");

			foreach (Scope scope in allScopeResponse)
			{
				Assert.NotNull(scope.ScopeId);
				Assert.True(scope.ScopeId == int.Parse(scopeDetails.Rows[countScopes]["ScopeId"].ToString()), "scope.ScopeId == int.Parse(scopeDetails.Rows[countScopes][\"ScopeId\"].ToString())");
				Assert.NotNull(scope.Enabled);
				Assert.True(scope.Enabled == Convert.ToBoolean(scopeDetails.Rows[countScopes]["Enabled"]), "scope.Enabled == Convert.ToBoolean(scopeDetails.Rows[countScopes][\"Enabled\"])");
				Assert.NotNull(scope.Name);
				Assert.True(scope.Name == scopeDetails.Rows[countScopes]["Name"].ToString(), "scope.Name == scopeDetails.Rows[countScopes][\"Name\"].ToString()");

				scopeDisplayName = scopeDetails.Rows[countScopes]["DisplayName"].ToString().Length > 0 ? scopeDetails.Rows[countScopes]["DisplayName"].ToString() : null;

				Assert.True(scope.DisplayName == scopeDisplayName, "scope.DisplayName == scopeDisplayName");

				scopeDescription = scopeDetails.Rows[countScopes]["Description"].ToString().Length > 0 ? scopeDetails.Rows[countScopes]["Description"].ToString() : null;

				Assert.True(scope.Description == scopeDescription, "scope.Description == scopeDescription");
				Assert.NotNull(scope.Required);
				Assert.True(scope.Required == Convert.ToBoolean(scopeDetails.Rows[countScopes]["Required"]), "scope.Required == Convert.ToBoolean(scopeDetails.Rows[countScopes][\"Required\"])");
				Assert.NotNull(scope.Emphasize);
				Assert.True(scope.Emphasize == Convert.ToBoolean(scopeDetails.Rows[countScopes]["Emphasize"]), "scope.Emphasize == Convert.ToBoolean(scopeDetails.Rows[countScopes][\"Emphasize\"])");
				Assert.NotNull(scope.Type);
				Assert.True(scope.Type == int.Parse(scopeDetails.Rows[countScopes]["Type"].ToString()), "scope.Type == int.Parse(scopeDetails.Rows[countScopes][\"Type\"].ToString())");
				Assert.NotNull(scope.IncludeAllClaimsForUser);
				Assert.True(scope.IncludeAllClaimsForUser == Convert.ToBoolean(scopeDetails.Rows[countScopes]["IncludeAllClaimsForUser"]), "scope.IncludeAllClaimsForUser == Convert.ToBoolean(scopeDetails.Rows[countScopes][\"IncludeAllClaimsForUser\"])");

				scopeClaimsRule = scopeDetails.Rows[countScopes]["ClaimsRule"].ToString().Length > 0 ? scopeDetails.Rows[countScopes]["ClaimsRule"].ToString() : null;

				Assert.True(scope.ClaimsRule == scopeClaimsRule, "scope.ClaimsRule == scopeClaimsRule");
				Assert.NotNull(scope.ShowInDiscoveryDocument);
				Assert.True(scope.ShowInDiscoveryDocument == Convert.ToBoolean(scopeDetails.Rows[countScopes]["ShowInDiscoveryDocument"]), "scope.ShowInDiscoveryDocument == Convert.ToBoolean(scopeDetails.Rows[countScopes][\"ShowInDiscoveryDocument\"])");
				Assert.NotNull(scope.AllowUnrestrictedIntrospection);
				Assert.True(scope.AllowUnrestrictedIntrospection == Convert.ToBoolean(scopeDetails.Rows[countScopes]["AllowUnrestrictedIntrospection"]), "scope.AllowUnrestrictedIntrospection == Convert.ToBoolean(scopeDetails.Rows[countScopes][\"AllowUnrestrictedIntrospection\"])");

				if (scope.ScopeClaims.Count > 0)
				{

				}

				if (scope.ScopeSecrets.Count > 0)
				{

				}

				countScopes++;
			}

			countScopes = 0;
		}
	}
}
