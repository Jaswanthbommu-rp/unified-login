using System.Net;
using Xunit;
using Xunit.Abstractions;
using Newtonsoft.Json;
using RPBooksApiTestingFramework;
using RPBooksApiTestingFramework.Controllers;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using System;

namespace GreenBook.Tests
{
    public class SetUserSecurityQuestions : TestController
    {
        private string payload;
        JsonController jsonManager = new JsonController();
        TestUtilities reusable;
        private readonly ITestOutputHelper XunitTestOutPut;

        public SetUserSecurityQuestions(ITestOutputHelper _xUnitTestOutput)
        {
            reusable = new TestUtilities(this);
            this.XunitTestOutPut = _xUnitTestOutput;

            string newLoginName = Guid.NewGuid().ToString().Remove(7) + "@ApiTest.com";
            payload = reusable.DoPostNewUserPayload(newLoginName);

            EndPointUrl = HostUrl + Properties["User"];
            GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: payload);
            CreateUserResponse<ErrorData> createUserResponse = JsonConvert.DeserializeObject<CreateUserResponse<ErrorData>>(ResponseString);

            EndPointUrl = HostUrl + Properties["User"] + "/Validate?enterpriseUserName=" + WebUtility.UrlEncode(newLoginName)
                + "&newUserRegistrationToken=" + createUserResponse.UserToken;

            //Execute API
            XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
            GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get);
        }

        //[Theory]
        //[Trait("Data-Driven", "Happy Path")]
        [InlineData("PostSetUserSecurityQuestions")]
        public void PostSetUserSecurityQuestionsHappyPath(string testCase)
        {
            //Deserialize GetUserValidate response       
            ValidateUserResponse validateResponse = JsonConvert.DeserializeObject<ValidateUserResponse>(ResponseString);
            string enterpriseUsername = validateResponse.EnterpriseUserName;
            string validateUserToken = validateResponse.ValidateUserToken;

            //Set up Payload
            payload = reusable.DoPostSetUserSecurityQuestionsPayload(enterpriseUsername, validateUserToken);
            XunitTestOutPut.WriteLine("Payload:\n" + payload);

            //Set up the API URL
            EndPointUrl = HostUrl + Properties["SetUserSecurityQuestions"];

            //Execute API
            XunitTestOutPut.WriteLine("Calling " + HttpVerb.Post + " at " + EndPointUrl);
            GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: payload);

            //Extract API's JSON Response            
            SetUserSecurityQuestionsResponse securityQuestionAnswerresponse = JsonConvert.DeserializeObject<SetUserSecurityQuestionsResponse>(ResponseString);
            XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);

            //Assert
            Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
            Assert.NotNull(securityQuestionAnswerresponse.EnterpriseUserName);
            Assert.NotNull(securityQuestionAnswerresponse.IsSuccess);
            Assert.True(securityQuestionAnswerresponse.IsSuccess, "securityQuestionAnswerresponse.IsSuccess");
            Assert.NotNull(securityQuestionAnswerresponse.IsError);
            Assert.False(securityQuestionAnswerresponse.IsError, "securityQuestionAnswerresponse.isError");
            Assert.NotNull(securityQuestionAnswerresponse.ErrorReason);
            Assert.True(securityQuestionAnswerresponse.ErrorReason == "", "getSecurityQuestions.errorReason == ''");
        }



        //[Theory]
        //[Trait("Data-Driven", "Negative Case"),]
        [InlineData("PostSetUserSecurityQuestionsWithOutEnterpriseUserName", 3, "No Username specified.")]
        [InlineData("PostSetUserSecurityQuestionsWithOutActivityToken", 3, "Null or empty activity Token.")]
        [InlineData("PostSetUserSecurityQuestionsWithOnlyTwoSecurityQuestionsAndAnswer", 2 , "Incorrect number of questions received from user, 3 questions are required.")]
        [InlineData("PostSetUserSecurityQuestionsWithFourSecurityQuestionsAndAnswers", 4, "Incorrect number of questions received from user, 3 questions are required.")]
        [InlineData("PostSetUserSecurityQuestionsWithThirdPartyActivityToken", 3, "Set security questions is not applicable to users on external identity provider.")]
        public void PostSetUserSecurityQuestionsNegativeCases(string testCase, int numberOfSecurityQuestionsAndAnswer, string errorReason)
        {

            //Deserialize GetUserValidate response
            UserSecurityAnswer userSecurityAnswer = new UserSecurityAnswer();
            ValidateUserResponse validateResponse = new ValidateUserResponse();
            string enterpriseUsername, validateUserToken;

            if (testCase.Contains("PostSetUserSecurityQuestionsWithThirdPartyActivityToken"))
            {
                string getUserValidateExternalIdentityProviderResponsePath = DataPath + "GetUserValidateExternalIdentityProviderResponse.json";
                string getUserValidateExternalIdentityProviderResponse = jsonManager.LoadJsonAsString(getUserValidateExternalIdentityProviderResponsePath);
                validateResponse = JsonConvert.DeserializeObject<ValidateUserResponse>(getUserValidateExternalIdentityProviderResponse);
                enterpriseUsername = validateResponse.EnterpriseUserName;
                validateUserToken = validateResponse.ValidateUserToken;                
            }      
            //Set up Payload
            else
            {
                validateResponse = JsonConvert.DeserializeObject<ValidateUserResponse>(ResponseString);
                enterpriseUsername = validateResponse.EnterpriseUserName;
                validateUserToken = validateResponse.ValidateUserToken;
            }

            payload = reusable.DoPostSetUserSecurityQuestionsPayload(enterpriseUsername, validateUserToken, numberOfSecurityQuestionsAndAnswer);
            if (testCase.Contains("PostSetUserSecurityQuestionsWithOutEnterpriseUserName"))
            {
                userSecurityAnswer = JsonConvert.DeserializeObject<UserSecurityAnswer>(payload);
                userSecurityAnswer.EnterpriseUserName = "";
                payload = JsonConvert.SerializeObject(userSecurityAnswer);
            }
            else if (testCase.Contains("PostSetUserSecurityQuestionsWithOutActivityToken"))
            {
                userSecurityAnswer = JsonConvert.DeserializeObject<UserSecurityAnswer>(payload);
                userSecurityAnswer.ActivityToken = "";
                payload = JsonConvert.SerializeObject(userSecurityAnswer);
            }
            XunitTestOutPut.WriteLine("Payload:\n" + payload);

            //Set up the API URL
            EndPointUrl = HostUrl + Properties["SetUserSecurityQuestions"];

            //Execute API
            XunitTestOutPut.WriteLine("Calling " + HttpVerb.Post + " at " + EndPointUrl);
            GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: payload);

            //Extract API's JSON Response
            SetUserSecurityQuestionsResponse securityQuestionAnswerresponse = JsonConvert.DeserializeObject<SetUserSecurityQuestionsResponse>(ResponseString);
            XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);

            //Assert
            Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
            Assert.Null(securityQuestionAnswerresponse.EnterpriseUserName);
            Assert.NotNull(securityQuestionAnswerresponse.IsSuccess);
            Assert.False(securityQuestionAnswerresponse.IsSuccess, "securityQuestionAnswerresponse.IsSuccess");
            Assert.NotNull(securityQuestionAnswerresponse.IsError);
            Assert.True(securityQuestionAnswerresponse.IsError, "securityQuestionAnswerresponse.isError");
            Assert.NotNull(securityQuestionAnswerresponse.ErrorReason);
            Assert.True(securityQuestionAnswerresponse.ErrorReason == errorReason, $"securityQuestionAnswerresponse.errorReason != {errorReason}");

        }
    }
}
