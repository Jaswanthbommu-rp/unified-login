using System.Net;
using Xunit;
using Xunit.Abstractions;
using Newtonsoft.Json;
using RPBooksApiTestingFramework;
using RPBooksApiTestingFramework.Controllers;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using System.Collections.Generic;
using GreenBook.Models;

namespace GreenBook.Tests
{
	public class PasswordPolicy : TestController
	{
		public PasswordPolicy(ITestOutputHelper _xUnitTestOutput)
		{
			reusable = new TestUtilities(this);
			this.XunitTestOutPut = _xUnitTestOutput;
		}

		JsonController jsonManager = new JsonController();
		TestUtilities reusable;
		private readonly ITestOutputHelper XunitTestOutPut;
        private string realPageId = "";

        // PasswordPolicy=/api/passwordpolicies

        [Fact, Trait("", "Happy Path")]
		public void GetPasswordPolicy()
		{
            // Pre-requisite
            realPageId = reusable.GetRealPageId(CurrentlyLoggedInUser);
            EndPointUrl = HostUrl + Properties["LandingOrganization"] + "person" + "/{realPageId}";
            EndPointUrl = EndPointUrl.Replace("{realPageId}", realPageId);

            // Execute API
            XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
            GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");

            // Extract API's JSON Response
            XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);
            LandingOrganizationModel personResponse = JsonConvert.DeserializeObject<LandingOrganizationModel>(ResponseString);
            
            //API - Get password Policy            
            // Set up the API URL
            long partyId = personResponse.data[0].partyId;
			EndPointUrl = HostUrl + Properties["PasswordPolicy"] + partyId;

			// Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");

			// Extract API's JSON Response
			
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);
			ObjectOutput<RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.PasswordPolicy, ErrorData> passwordPolicyResponse 
				= JsonConvert.DeserializeObject<ObjectOutput<RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.PasswordPolicy, ErrorData>>(ResponseString);

			// Extract Expected Response for Assertion
			string GetPasswordPolicyResponsePath = DataPath + "GetPasswordPolicyResponse.json";
			string GetPasswordPolicyResponse = jsonManager.LoadJsonAsString(GetPasswordPolicyResponsePath);
			ObjectOutput<RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.PasswordPolicy, ErrorData> expectedPasswordPolicyResponse
				= JsonConvert.DeserializeObject<ObjectOutput<RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.PasswordPolicy, ErrorData>>(GetPasswordPolicyResponse);

			// Assert
			Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
			Assert.NotNull(passwordPolicyResponse.obj.PasswordPolicyId);
			Assert.True(passwordPolicyResponse.obj.PasswordPolicyId == expectedPasswordPolicyResponse.obj.PasswordPolicyId
				, "passwordPolicyResponse.obj.PasswordPolicyId == expectedPasswordPolicyResponse.obj.PasswordPolicyId");
			Assert.NotNull(passwordPolicyResponse.obj.PartyId);
			Assert.True(passwordPolicyResponse.obj.PartyId == partyId, "passwordPolicyResponse.obj.PartyId == partyId");
			Assert.NotNull(passwordPolicyResponse.obj.Name);
			Assert.True(passwordPolicyResponse.obj.Name == expectedPasswordPolicyResponse.obj.Name
				, "passwordPolicyResponse.obj.Name == expectedPasswordPolicyResponse.obj.Name");
			Assert.NotNull(passwordPolicyResponse.obj.MinimumLength);
			Assert.True(passwordPolicyResponse.obj.MinimumLength == expectedPasswordPolicyResponse.obj.MinimumLength
				, "passwordPolicyResponse.obj.MinimumLength == expectedPasswordPolicyResponse.obj.MinimumLength");
			Assert.NotNull(passwordPolicyResponse.obj.MaximumLength);
			Assert.True(passwordPolicyResponse.obj.MaximumLength == expectedPasswordPolicyResponse.obj.MaximumLength
				, "passwordPolicyResponse.obj.MaximumLength == expectedPasswordPolicyResponse.obj.MaximumLength");
			Assert.NotNull(passwordPolicyResponse.obj.MinimumLowercase);
			Assert.True(passwordPolicyResponse.obj.MinimumLowercase == expectedPasswordPolicyResponse.obj.MinimumLowercase
				, "passwordPolicyResponse.obj.MinimumLowercase == expectedPasswordPolicyResponse.obj.MinimumLowercase");
			Assert.NotNull(passwordPolicyResponse.obj.MinimumUppercase);
			Assert.True(passwordPolicyResponse.obj.MinimumUppercase == expectedPasswordPolicyResponse.obj.MinimumUppercase
				, "passwordPolicyResponse.obj.MinimumUppercase == expectedPasswordPolicyResponse.obj.MinimumUppercase");
			Assert.NotNull(passwordPolicyResponse.obj.MinimumNumeric);
			Assert.True(passwordPolicyResponse.obj.MinimumNumeric == expectedPasswordPolicyResponse.obj.MinimumNumeric
				, "passwordPolicyResponse.obj.MinimumNumeric == expectedPasswordPolicyResponse.obj.MinimumNumeric");
			Assert.NotNull(passwordPolicyResponse.obj.MinimumSpecialCharacter);
			Assert.True(passwordPolicyResponse.obj.MinimumSpecialCharacter == expectedPasswordPolicyResponse.obj.MinimumSpecialCharacter
				, "passwordPolicyResponse.obj.MinimumSpecialCharacter == expectedPasswordPolicyResponse.obj.MinimumSpecialCharacter");
			Assert.NotNull(passwordPolicyResponse.obj.AllowUsersToChangeOwnPassword);
			Assert.True(passwordPolicyResponse.obj.AllowUsersToChangeOwnPassword == expectedPasswordPolicyResponse.obj.AllowUsersToChangeOwnPassword
				, "passwordPolicyResponse.obj.AllowUsersToChangeOwnPassword == expectedPasswordPolicyResponse.obj.AllowUsersToChangeOwnPassword");
			Assert.NotNull(passwordPolicyResponse.obj.EnablePasswordExpiration);
			Assert.True(passwordPolicyResponse.obj.EnablePasswordExpiration == expectedPasswordPolicyResponse.obj.EnablePasswordExpiration
				, "passwordPolicyResponse.obj.EnablePasswordExpiration == expectedPasswordPolicyResponse.obj.EnablePasswordExpiration");
			Assert.NotNull(passwordPolicyResponse.obj.PasswordExpirationPeriodInDays);
			Assert.True(passwordPolicyResponse.obj.PasswordExpirationPeriodInDays == expectedPasswordPolicyResponse.obj.PasswordExpirationPeriodInDays
				, "passwordPolicyResponse.obj.PasswordExpirationPeriodInDays == expectedPasswordPolicyResponse.obj.PasswordExpirationPeriodInDays");
			Assert.NotNull(passwordPolicyResponse.obj.NumberOfPasswordsToRemember);
			Assert.True(passwordPolicyResponse.obj.NumberOfPasswordsToRemember == expectedPasswordPolicyResponse.obj.NumberOfPasswordsToRemember
				, "passwordPolicyResponse.obj.NumberOfPasswordsToRemember == expectedPasswordPolicyResponse.obj.NumberOfPasswordsToRemember");
			Assert.NotNull(passwordPolicyResponse.obj.UserId);
			Assert.True(passwordPolicyResponse.obj.UserId == expectedPasswordPolicyResponse.obj.UserId
				, "passwordPolicyResponse.obj.UserId == expectedPasswordPolicyResponse.obj.UserId");
			Assert.NotNull(passwordPolicyResponse.obj.SysStartDateTime);
			Assert.NotNull(passwordPolicyResponse.obj.SysStartDateTime);
			Assert.NotNull(passwordPolicyResponse.obj.SysEndDateTime);
			Assert.True(passwordPolicyResponse.obj.SysEndDateTime == expectedPasswordPolicyResponse.obj.SysEndDateTime
				, "passwordPolicyResponse.obj.SysEndDateTime == expectedPasswordPolicyResponse.obj.SysEndDateTime");
		}

		//[Theory]
		//[Trait("Data-Driven", "Negative Case")]
		[InlineData("GetPasswordPolicyWithoutPartyId", "The requested resource does not support http method 'GET'.")]
		[InlineData("GetPasswordPolicyWithInvalidPartyId", "Invalid parameter: PartyId.", "0")]
		[InlineData("GetPasswordPolicyWithAlphabeticCharacterAsPartyId", "The request is invalid.", "invalidPartyId")]
		public void GetPasswordPolicyNegativeCases(string testCase, string errorReason, string partyId = "")
		{
			// Set up the API URL
			EndPointUrl = HostUrl + Properties["PasswordPolicy"] + partyId;

			// Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");

			// Extract API's JSON Response
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);

			// Assert
			if (testCase == "GetPasswordPolicyWithoutPartyId")
			{
				Assert.True(HttpStatusCode.MethodNotAllowed == ResponseHttpStatusCode, "HttpStatusCode.MethodNotAllowed == ResponseHttpStatusCode");
			}
			else
			{
				Assert.True(HttpStatusCode.BadRequest == ResponseHttpStatusCode, "HttpStatusCode.BadRequest == ResponseHttpStatusCode");
			}
			Assert.NotNull(ResponseString);
			Assert.True(ResponseString.Contains(errorReason), $"ResponseString does not contain \"{errorReason}\"");
		}
	}
}
