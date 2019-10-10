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
	public class GetUserByEnterpriseUserId : TestController
	{
		JsonController jsonManager = new JsonController();
		TestUtilities reusable;
		private readonly ITestOutputHelper XunitTestOutPut;
		DataTable userDetails = new DataTable();

		public GetUserByEnterpriseUserId(ITestOutputHelper _xUnitTestOutput)
		{
			this.XunitTestOutPut = _xUnitTestOutput;
		}

		// GetUserByEnterpriseUserName=/api/IdentityConfig/GetUserByEnterpriseUserName

		/*Disabling this test since GET GetUserByEnterpriseUserName has already been deprecated */
		//[Fact, Trait("", "Happy Path")]
		public void GetGetUserByEnterpriseUserId()
		{
			// Set up the API URL
			userDetails = reusable.DoSelectUser();
			EndPointUrl = HostIdentityUrl + Properties["GetUserByEnterpriseUserId"]
				+ "?enterpriseUserId=" + userDetails.Rows[0]["UserId"].ToString();

			// Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");

			// Extract API's JSON Response
			
			RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.User userResponse
				= JsonConvert.DeserializeObject<RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.User>(ResponseString);
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);
			userDetails.Rows[0]["AccountExpiration"] = userDetails.Rows[0]["AccountExpiration"].ToString().Length <= 0 ? new DateTime() : userDetails.Rows[0]["AccountExpiration"];

			// Assert
			Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
			Assert.NotNull(userResponse.UserId);
			Assert.True(userResponse.UserId == int.Parse(userDetails.Rows[0]["UserId"].ToString()), "userResponse.UserId == int.Parse(userDetails.Rows[0][\"UserId\"].ToString())");
			Assert.NotNull(userResponse.LoginId);
			Assert.True(userResponse.LoginId == userDetails.Rows[0]["LoginId"].ToString(), "userResponse.LoginId == userDetails.Rows[0][\"LoginId\"].ToString()");
			Assert.NotNull(userResponse.Firstname);
			Assert.True(userResponse.Firstname == userDetails.Rows[0]["FirstName"].ToString(), "userResponse.FirstName == userDetails.Rows[0][\"FirstName\"].ToString()");
			Assert.NotNull(userResponse.Lastname);
			Assert.True(userResponse.Lastname == userDetails.Rows[0]["Lastname"].ToString(), "userResponse.Lastname == userDetails.Rows[0][\"Lastname\"].ToString()");
			Assert.NotNull(userResponse.IsActive);
			Assert.True(userResponse.IsActive == Convert.ToBoolean(userDetails.Rows[0]["IsActive"]), "userResponse.IsActive == Convert.ToBoolean(userDetails.Rows[0][\"IsActive\"])");
			Assert.NotNull(userResponse.IsLocked);
			Assert.True(userResponse.IsLocked == Convert.ToBoolean(userDetails.Rows[0]["IsLocked"]), "userResponse.IsLocked == Convert.ToBoolean(userDetails.Rows[0][\"IsLocked\"])");
			Assert.NotNull(userResponse.PasswordHash);
			Assert.True(userResponse.PasswordHash == userDetails.Rows[0]["PasswordHash"].ToString(), "userResponse.PasswordHash == userDetails.Rows[0][\"PasswordHash\"].ToString()");
			Assert.NotNull(userResponse.PasswordSalt);
			Assert.True(userResponse.PasswordSalt == userDetails.Rows[0]["PasswordSalt"].ToString(), "userResponse.PasswordSalt == userDetails.Rows[0][\"PasswordSalt\"].ToString()");
			Assert.NotNull(userResponse.IdentityProvider);
			Assert.True(userResponse.IdentityProvider == userDetails.Rows[0]["IdentityProvider"].ToString(), "userResponse.IdentityProvider == userDetails.Rows[0][\"IdentityProvider\"].ToString()");
			Assert.NotNull(userResponse.LastPasswordModifiedDateTime);
			Assert.True(userResponse.LastPasswordModifiedDateTime == Convert.ToDateTime(userDetails.Rows[0]["LastPasswordModifiedDateTime"]), "userResponse.LastPasswordModifiedDateTime == Convert.ToDateTime(userDetails.Rows[0][\"LastPasswordModifiedDateTime\"])");
			Assert.NotNull(userResponse.AccountExpiration);
			Assert.True(userResponse.AccountExpiration == Convert.ToDateTime(userDetails.Rows[0]["AccountExpiration"]), "userResponse.AccountExpiration == Convert.ToDateTime(userDetails.Rows[0][\"AccountExpiration\"])");
		}
	}
}
