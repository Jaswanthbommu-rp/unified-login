using System.Net;
using Xunit;
using Xunit.Abstractions;
using Newtonsoft.Json;
using RPBooksApiTestingFramework;
using RPBooksApiTestingFramework.Controllers;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using System.Data;

namespace GreenBook.Tests
{
	public class UserAllSecurityQuestions : TestController
	{
		public UserAllSecurityQuestions(ITestOutputHelper _xUnitTestOutput)
		{
			reusable = new TestUtilities(this);
			this.XunitTestOutPut = _xUnitTestOutput;

			dbManager = new DatabaseController(DbConnString);
		}

		JsonController jsonManager = new JsonController();
		DatabaseController dbManager;
		TestUtilities reusable;
		private readonly ITestOutputHelper XunitTestOutPut;

        // UserAllSecurityQuestions=/api/credential/UserAllSecurityQuestions

        [Fact, Trait("", "Happy Path")]
        public void GetUserAllSecurityQuestions()
		{
			// Set up the API URL
			EndPointUrl = HostUrl + Properties["UserAllSecurityQuestions"] + Properties["enterpriseUsername"];

			// Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");

			// Extract API's JSON Response
			UserAllSecurityQuestionResponse userAllSecurityQuestions = JsonConvert.DeserializeObject<UserAllSecurityQuestionResponse>(ResponseString);
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);
			
			// Assert
			Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
			Assert.NotNull(userAllSecurityQuestions.EnterpriseUserName);
			Assert.True(userAllSecurityQuestions.EnterpriseUserName == Properties["enterpriseUsername"], "userAllSecurityQuestions.enterpriseUserName == Properties[\"enterpriseUsername\"]");
			Assert.True(userAllSecurityQuestions.SecurityQuestions.Count > 0, "userAllSecurityQuestions.securityQuestions.Count > 0");
			foreach (SecurityQuestion securityQuestion in userAllSecurityQuestions.SecurityQuestions)
			{
				Assert.NotNull(securityQuestion.SecurityQuestionId);
				Assert.True(securityQuestion.SecurityQuestionId > 0, "securityQuestion.securityQuestionId > 0");
				Assert.NotNull(securityQuestion.Question);
				Assert.True(securityQuestion.Question.Length > 0, "securityQuestion.question.Length > 0");
			}
			Assert.NotNull(userAllSecurityQuestions.IsError);
			Assert.False(userAllSecurityQuestions.IsError, "getSecurityQuestions.isError");
			Assert.Null(userAllSecurityQuestions.ErrorReason);
		}
		
		//[Fact, Trait("", "Negative Case")]
		//public void GetUserAllSecurityQuestionsInvalidUsername()
		//{
		//	// Set up the API URL
		//	EndPointUrl = HostUrl + Properties["UserAllSecurityQuestions"] + "assumedWrongUsername";

		//	// Execute API
		//	XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
		//	GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");

		//	// Extract API's JSON Response
		//	UserAllSecurityQuestionResponse userAllSecurityQuestions = JsonConvert.DeserializeObject<UserAllSecurityQuestionResponse>(ResponseString);
		//	XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);
			
		//	// Assert
		//	Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
		//	Assert.Null(userAllSecurityQuestions.EnterpriseUserName);
		//	Assert.Null(userAllSecurityQuestions.SecurityQuestions);
		//	Assert.NotNull(userAllSecurityQuestions.IsError);
		//	Assert.True(userAllSecurityQuestions.IsError, "getSecurityQuestions.isError");
		//	Assert.NotNull(userAllSecurityQuestions.ErrorReason);
		//	Assert.True(userAllSecurityQuestions.ErrorReason == "User Name is incorrect or not found.", "getSecurityQuestions.errorReason == 'User Name is incorrect or not found.'");
		//}

		//[Fact, Trait("", "Negative Case")]
		//public void GetUserAllSecurityQuestionsNoUsername()
		//{
		//	// Set up the API URL
		//	EndPointUrl = HostUrl + Properties["UserAllSecurityQuestions"];

		//	// Execute API
		//	XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
		//	GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");

		//	// Extract API's JSON Response
		//	XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);

		//	// Assert
		//	Assert.True(HttpStatusCode.InternalServerError == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
		//	Assert.True(ResponseString.StartsWith("Internal System Error. Please contact RealPage support with error reference Id - "),
		//		"ResponseString.StartsWith(\"Internal System Error. Please contact RealPage support with error reference Id - \")");
		//}
		
		//[Fact, Trait("", "Data-Driven")]
		//public void GetUserAllSecurityQuestionsUsernameWithoutSecurityQuestions()
		//{
		//	// Set up the API URL
		//	DataTable newEnterpriseUsername = dbManager.executeQuery("SELECT distinct TOP 1 pcm.partyid, ul.partyid, ul.loginname "
		//		+ "FROM ["+ Properties["identityDatabase"] + "].[Ident].[IdentityProviderType] idpt "
		//		+ "inner join[" + Properties["identityDatabase"] + "].[Ident].[IdentityProviderSettingType] idpst "
		//		+ "inner join[" + Properties["identityDatabase"] + "].[Ident].[IdentityProviderSetting] idps "
		//		+ "inner join[" + Properties["identityDatabase"] + "].[Ident].[ContactMechanismIdentity] cmid "
		//		+ "inner join[" + Properties["identityDatabase"] + "].[Enterprise].[PartyContactMechanism] pcm "
		//		+ "inner join[" + Properties["identityDatabase"] + "].[Enterprise].[Organization] o "
		//		+ "inner join[" + Properties["identityDatabase"] + "].[Enterprise].[PartyRelationship] pr "
		//		+ "inner join[" + Properties["identityDatabase"] + "].[Ident].[UserLogin] ul "
		//		+ "on ul.partyid = pr.partyidfrom "
		//		+ "on pr.partyidto = o.partyid "
		//		+ "on pcm.partyid = o.partyid "
		//		+ "on pcm.contactmechanismid = cmid.contactmechanismid "
		//		+ "on cmid.[IdentityProviderSettingId] = idps.[IdentityProviderSettingId] "
		//		+ "on idps.identityprovidersettingtypeid = idpst.identityprovidersettingtypeid "
		//		+ "on idpt.identityprovidertypeid = idpst.identityprovidertypeid "
		//		+ "where idpt.description = 'IdentityServer' "
		//		+ "and userid not in (select userid from[identity].[Ident].[usersecurityanswer])");
		//	EndPointUrl = HostUrl + Properties["UserAllSecurityQuestions"] + newEnterpriseUsername.Rows[0]["LoginName"];

		//	// Execute API
		//	XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
		//	GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");

		//	// Extract API's JSON Response
		//	UserAllSecurityQuestionResponse userAllSecurityQuestions = JsonConvert.DeserializeObject<UserAllSecurityQuestionResponse>(ResponseString);
		//	XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);

		//	if (userAllSecurityQuestions.ErrorReason == "Max attempts to get security questions exceeded.")
		//	{
		//		userAllSecurityQuestions = JsonConvert.DeserializeObject<UserAllSecurityQuestionResponse>(reusable.DoResetMaximumActivityAttempts(newEnterpriseUsername.Rows[0][0].ToString(), EndPointUrl, HttpVerb.Get));
		//	}

		//	// Assert
		//	Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
		//	Assert.Null(userAllSecurityQuestions.EnterpriseUserName);
		//	Assert.Null(userAllSecurityQuestions.SecurityQuestions);
		//	Assert.NotNull(userAllSecurityQuestions.IsError);
		//	Assert.True(userAllSecurityQuestions.IsError, "getSecurityQuestions.isError");
		//	Assert.NotNull(userAllSecurityQuestions.ErrorReason);
		//	Assert.True(userAllSecurityQuestions.ErrorReason == "User has no security questions defined.", "getSecurityQuestions.errorReason == 'User has no security questions defined.'");
		//}
	}
}

