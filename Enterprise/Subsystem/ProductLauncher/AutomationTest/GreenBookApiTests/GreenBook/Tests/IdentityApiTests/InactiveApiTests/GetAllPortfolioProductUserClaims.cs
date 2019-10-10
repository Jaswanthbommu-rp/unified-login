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
	public class GetAllPortfolioProductUserClaims : TestController
	{
		JsonController jsonManager = new JsonController();
		TestUtilities reusable;
		private readonly ITestOutputHelper XunitTestOutPut;
		DataTable portfolioProductUserClaims = new DataTable();
		int countportfolioProductUserClaims = 0;

		public GetAllPortfolioProductUserClaims(ITestOutputHelper _xUnitTestOutput)
		{
			this.XunitTestOutPut = _xUnitTestOutput;
		}

		// GetAllPortfolioProductUserClaims=/api/IdentityConfig/GetAllPortfolioProductUserClaims

		//[Fact, Trait("", "Happy Path")]
		public void GetGetAllPortfolioProductUserClaims()
		{
			// Set up the API URL
			portfolioProductUserClaims = reusable.DoSelectPortfolioProductUserClaims();
			EndPointUrl = HostIdentityUrl + Properties["GetAllPortfolioProductUserClaims"] + "?organizationId=" + portfolioProductUserClaims.Rows[0]["PortfolioId"] 
				+ "&clientCode=" + portfolioProductUserClaims.Rows[0]["ClientId"] + "&enterpriseUserId=" + portfolioProductUserClaims.Rows[0]["UserId"];

			// Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");

			//Extract API's JSON Response
			
			IList<PortfolioProductUserClaims> portfolioProductUserClaimsResponse = JsonConvert.DeserializeObject<IList<PortfolioProductUserClaims>>(ResponseString);
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);

			//Assert
			Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");

			foreach (PortfolioProductUserClaims portfolioProductUserClaim in portfolioProductUserClaimsResponse)
			{
				Assert.NotNull(portfolioProductUserClaim.Id);
				Assert.True(portfolioProductUserClaim.Id == int.Parse(portfolioProductUserClaims.Rows[countportfolioProductUserClaims]["Id"].ToString())
					, "portfolioProductUserClaim.Id == int.Parse(portfolioProductUserClaims.Rows[countportfolioProductUserClaims][\"Id\"].ToString())");
				Assert.NotNull(portfolioProductUserClaim.PortfolioId);
				Assert.True(portfolioProductUserClaim.PortfolioId == int.Parse(portfolioProductUserClaims.Rows[countportfolioProductUserClaims]["PortfolioId"].ToString())
					, "portfolioProductUserClaim.PortfolioId == int.Parse(portfolioProductUserClaims.Rows[countportfolioProductUserClaims][\"PortfolioId\"].ToString())");
				Assert.NotNull(portfolioProductUserClaim.ClientId);
				Assert.True(portfolioProductUserClaim.ClientId == portfolioProductUserClaims.Rows[countportfolioProductUserClaims]["ClientId"].ToString()
					, "portfolioProductUserClaim.ClientId == portfolioProductUserClaims.Rows[countportfolioProductUserClaims][\"ClientId\"].ToString()");
				Assert.NotNull(portfolioProductUserClaim.UserId);
				Assert.True(portfolioProductUserClaim.UserId == int.Parse(portfolioProductUserClaims.Rows[countportfolioProductUserClaims]["UserId"].ToString())
					, "portfolioProductUserClaim.UserId == int.Parse(portfolioProductUserClaims.Rows[countportfolioProductUserClaims][\"UserId\"].ToString())");
				Assert.NotNull(portfolioProductUserClaim.Type);
				Assert.True(portfolioProductUserClaim.Type == portfolioProductUserClaims.Rows[countportfolioProductUserClaims]["Type"].ToString()
					, "portfolioProductUserClaim.Type == portfolioProductUserClaims.Rows[countportfolioProductUserClaims][\"Type\"].ToString()");
				Assert.NotNull(portfolioProductUserClaim.Value);
				Assert.True(portfolioProductUserClaim.Value == portfolioProductUserClaims.Rows[countportfolioProductUserClaims]["Value"].ToString()
					, "portfolioProductUserClaim.Value == portfolioProductUserClaims.Rows[countportfolioProductUserClaims][\"Value\"].ToString()");

				if (portfolioProductUserClaim.Scope != null)
				{

				}

				countportfolioProductUserClaims++;
			}

			countportfolioProductUserClaims = 0;
		}
	}
}
