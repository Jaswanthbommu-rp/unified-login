using System.Net;
using Xunit;
using Xunit.Abstractions;
using Newtonsoft.Json;
using RPBooksApiTestingFramework;
using RPBooksApiTestingFramework.Controllers;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;

namespace GreenBook.Tests
{
	public class UserSelectedSecurityQuestions : TestController
	{
		public UserSelectedSecurityQuestions(ITestOutputHelper _xUnitTestOutput)
		{
			reusable = new TestUtilities(this);
			this.XunitTestOutPut = _xUnitTestOutput;
            userCredentials = Properties["enterpriseUsernameForSecurityQuestions"].Split('|');
            _accessToken = GetClientToken(Properties["identityClientUrl"], userCredentials[0], userCredentials[1]);
        }

		private string payload;
		JsonController jsonManager = new JsonController();
		TestUtilities reusable;
		private readonly ITestOutputHelper XunitTestOutPut;
        string[] userCredentials;

        // UserSelectedSecurityQuestions = /api/credential/UserSelectedSecurityQuestions

        [Fact, Trait("", "Happy Path")]
		public void GetUserSelectedSecurityQuestions() // Comment by Ajit- Added temporary test method to demo Authenticated test/ make changes according to your standared later
		{
			// Set up the API URL
			EndPointUrl = HostUrl + Properties["UserSelectedSecurityQuestions"];

			// Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
            GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get);

            // Extract API's JSON Response
            ObjectListOutput<SecurityQuestion, IErrorData> usersAllSecurityQuestionResponse = JsonConvert.DeserializeObject<ObjectListOutput<SecurityQuestion, IErrorData>>(ResponseString);
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);

			// Assert
			Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == response.StatusCode");

            Assert.NotNull(usersAllSecurityQuestionResponse.Status.Success);
            if(usersAllSecurityQuestionResponse.Status.Success.ToString() == "True")
            {
                Assert.NotNull(usersAllSecurityQuestionResponse.Status.ErrorCode);
                Assert.NotNull(usersAllSecurityQuestionResponse.Status.ErrorMsg);
            }
            
            Assert.True(usersAllSecurityQuestionResponse.list.Count > 0, "usersAllSecurityQuestionResponse.SecurityQuestions.Count > 0");
            foreach(var securityQuestions in usersAllSecurityQuestionResponse.list)
            {
                Assert.True(securityQuestions.SecurityQuestionId > 0);
                Assert.NotNull(securityQuestions.Question);
            }
        }


        [Fact, Trait("", "Happy Path")]
        public void PostUserSelectedSecurityQuestions()
        {
            //Set up Payload
            payload = reusable.DoPostUserSelectedSecurityQuestions();
            XunitTestOutPut.WriteLine("Payload:\n" + payload);

            //Set up the API URL
            EndPointUrl = HostUrl + Properties["UserSelectedSecurityQuestions"];

            //Execute API
            XunitTestOutPut.WriteLine("Calling " + HttpVerb.Post + " at " + EndPointUrl);
            GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: payload);

            //Extract API's JSON Response
            SaveUserSelectedSecurityQuestionResponse securityQuestionAnswerresponse = JsonConvert.DeserializeObject<SaveUserSelectedSecurityQuestionResponse>(ResponseString);
            XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);

            //Assert
            Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == response.StatusCode");
            Assert.NotNull(securityQuestionAnswerresponse.IsError);
            Assert.False(securityQuestionAnswerresponse.IsError, "securityQuestionAnswerresponse.isError");
            Assert.Null(securityQuestionAnswerresponse.ErrorReason);
        }

        //[Theory]
        //[Trait("Data-Driven", "Negative Case"),]
        [InlineData("PostUserSelectedSecurityQuestionsWithOnlyTwoSecurityQuestionsAndAnswers", 2)]
        [InlineData("PostUserSelectedSecurityQuestionsWithOnlyOneSecurityQuestionsAndAnswers", 1)]
        public void PostUserSelectedSecurityQuestionsNegativeCases(string testCase, int numberOfSecurityQuestions)
        {
            //Set up Payload
            payload = reusable.DoPostUserSelectedSecurityQuestions(0, "", numberOfSecurityQuestions);
            XunitTestOutPut.WriteLine("Payload:\n" + payload);

            //Set up the API URL
            EndPointUrl = HostUrl + Properties["UserSelectedSecurityQuestions"];

            //Execute API
            XunitTestOutPut.WriteLine("Calling " + HttpVerb.Post + " at " + EndPointUrl);
            GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: payload);

            //Extract API's JSON Response
            SaveUserSelectedSecurityQuestionResponse securityQuestionAnswerresponse = JsonConvert.DeserializeObject<SaveUserSelectedSecurityQuestionResponse>(ResponseString);
            XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);

            //Assert
            Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == response.StatusCode");
            Assert.NotNull(securityQuestionAnswerresponse.IsError);
            Assert.True(securityQuestionAnswerresponse.IsError, "securityQuestionAnswerresponse.isError");
            Assert.NotNull(securityQuestionAnswerresponse.ErrorReason);
            Assert.True(securityQuestionAnswerresponse.ErrorReason == "Incorrect number of questions received from user.", "securityQuestionAnswerresponse.errorReason != 'Incorrect number of questions received from user.'");
        }
    }
}
