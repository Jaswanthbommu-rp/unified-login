using System.Net;
using Xunit;
using Xunit.Abstractions;
using Newtonsoft.Json;
using RPBooksApiTestingFramework;
using RPBooksApiTestingFramework.Controllers;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using System.Linq;

namespace GreenBook.Tests
{
	public class VerifySecurityAnswers : TestController
	{
		public VerifySecurityAnswers(ITestOutputHelper _xUnitTestOutput)
		{
			reusable = new TestUtilities(this);
			this.XunitTestOutPut = _xUnitTestOutput;
			DatabaseController dbManager = new DatabaseController(DbConnString);
			maxActivityAttemptCountQuestionAttempts = int.Parse(dbManager.executeQuery("SELECT DISTINCT TOP 1 [MaxActivityAttemptCount], [ActivityCode] "
				+ "FROM [Identity].[Ident].[Activity] WHERE ActivityCode = 'ForgotPassword'").Rows[0]["MaxActivityAttemptCount"].ToString()) + 1;
			
			EndPointUrl = HostUrl + Properties["GetSecurityQuestions"] + WebUtility.UrlEncode(Properties["enterpriseUsernameForStatusChange"]);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");
			securityQuestionsApiResponse = ResponseString;
		}

		private readonly ITestOutputHelper XunitTestOutPut;
		private string securityQuestionsApiResponse;
		private string payload;
		JsonController jsonManager = new JsonController();
		TestUtilities reusable;
		private int maxActivityAttemptCountQuestionAttempts;
		private UserSecurityAnswer payloadUserSecurityAnswer = new UserSecurityAnswer();

		//VerifySecurityAnswers=/api/credential/VerifySecurityAnswers

		//[Theory]
		//[Trait("Data-Driven", "Happy Path")]
		[InlineData("PostVerifySecurityAnswers")]
		[InlineData("PostVerifySecurityAnswersVerifyAccessAfterMaximumInvalidAttemptsUsingValidPayload")]
		[InlineData("PostVerifySecurityAnswersVerifyAccessAfterInvalidAttemptAfterMaximumInvalidAttemptsUsingValidPayload")]
		public void PostVerifySecurityAnswersHappyPaths(string testCase)
		{
			// Set up Payload
			payload = reusable.DoPostVerifySecurityAnswersPayload(securityQuestionsApiResponse, "validActivityToken", Properties["enterpriseUsernameForStatusChange"]);
			XunitTestOutPut.WriteLine("Payload:\n" + payload);

			// Set up the API URL
			EndPointUrl = HostUrl + Properties["VerifySecurityAnswers"];

			// Execute API
			if (testCase.Contains("MaximumInvalidAttempts"))
			{
				payloadUserSecurityAnswer = JsonConvert.DeserializeObject<UserSecurityAnswer>(payload);
				payloadUserSecurityAnswer.SecurityQuestionAnswers.First().Answer = "invalidAnswer";
				payload = JsonConvert.SerializeObject(payloadUserSecurityAnswer);
				maxActivityAttemptCountQuestionAttempts = testCase.Contains("PostVerifySecurityAnswersInvalidAttemptAfterMaximumInvalidAttempts") ? ++maxActivityAttemptCountQuestionAttempts : maxActivityAttemptCountQuestionAttempts;
				for (int countVerifySecurityAnswersMaximumInvalidAttempts = 0; countVerifySecurityAnswersMaximumInvalidAttempts < (maxActivityAttemptCountQuestionAttempts); countVerifySecurityAnswersMaximumInvalidAttempts++)
				{
					XunitTestOutPut.WriteLine("\n\nPOST VerifySecurityAnswers API Attempt #" + (countVerifySecurityAnswersMaximumInvalidAttempts + 1) + ":\n" + ResponseString);
					GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: payload);
				}
				if (testCase.Contains("ValidPayload"))
				{
					payloadUserSecurityAnswer = JsonConvert.DeserializeObject<UserSecurityAnswer>(payload);
					payloadUserSecurityAnswer.SecurityQuestionAnswers.First().Answer = "real";
					payload = JsonConvert.SerializeObject(payloadUserSecurityAnswer);
					XunitTestOutPut.WriteLine("\n\nUnlock User and try to execute API with valid Security Answers:\n");
					reusable.DoResetMaximumActivityAttempts(Properties["enterpriseUsernameForStatusChange"], EndPointUrl, HttpVerb.Post, payload);
					EndPointUrl = HostUrl + Properties["VerifySecurityAnswers"];
				}
			}
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Post + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: payload);

			// Extract API's JSON Response
			SecurityAnswerResponse verifySecurityAnswers = JsonConvert.DeserializeObject<SecurityAnswerResponse>(ResponseString);
			if (!string.IsNullOrEmpty(verifySecurityAnswers.ErrorReason))
			{
				if (verifySecurityAnswers.ErrorReason.Contains("locked"))
				{
					verifySecurityAnswers = JsonConvert.DeserializeObject<SecurityAnswerResponse>(reusable.DoResetMaximumActivityAttempts(Properties["enterpriseUsernameForStatusChange"], EndPointUrl, HttpVerb.Post, payload));
				}
			}
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);

			// Assert
			Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
			Assert.NotNull(verifySecurityAnswers.EnterpriseUserName);
			Assert.True(verifySecurityAnswers.EnterpriseUserName == Properties["enterpriseUsernameForStatusChange"]
				, "verifySecurityAnswers.enterpriseUserName != Properties[\"enterpriseUsernameForStatusChange\"]");
			Assert.NotNull(verifySecurityAnswers.IsAnswersCorrect);
			Assert.True(verifySecurityAnswers.IsAnswersCorrect, "verifySecurityAnswers.isAnswersCorrect");
			Assert.NotNull(verifySecurityAnswers.CorrectAnswerToken);
			Assert.True(verifySecurityAnswers.CorrectAnswerToken.Length > 0, "verifySecurityAnswers.correctAnswerToken.Length > 0");
			Assert.Null(verifySecurityAnswers.SecurityQuestions);
			Assert.NotNull(verifySecurityAnswers.IsError);
			Assert.False(verifySecurityAnswers.IsError, "verifySecurityAnswers.isError");
			Assert.NotNull(verifySecurityAnswers.ErrorReason);
		}

		//[Theory]
		//[Trait("Data-Driven", "Negative Case")]
		[InlineData("PostVerifySecurityAnswersNoUsername", "No Username specified.", "")]
		[InlineData("PostVerifySecurityAnswersNullUsername", "No Username specified.", null)]
		[InlineData("PostVerifySecurityAnswersInvalidUsername", "User Name is incorrect or not found.", "invalidUsername")]
		[InlineData("PostVerifySecurityAnswersNoActivityToken", "Null or empty security Forgot Password Activity Token.", "validUsername", "")]
		[InlineData("PostVerifySecurityAnswersNullActivityToken", "Null or empty security Forgot Password Activity Token.", "validUsername", null)]
		[InlineData("PostVerifySecurityAnswersInvalidActivityToken", "Forgot Password Activity Token is expired.", "validUsername", "invalidActivityToken")]
		[InlineData("PostVerifySecurityAnswersExpiredActivityToken", "Forgot Password Activity Token is expired.")]
		[InlineData("PostVerifySecurityAnswersNoFirstSecurityAnswerProvided", "One or more of your answers are incorrect. Please try again with a new set of questions.", "validUsername", "validActivityToken", "")]
		[InlineData("PostVerifySecurityAnswersNoSecondSecurityAnswerProvided", "One or more of your answers are incorrect. Please try again with a new set of questions.", "validUsername", "validActivityToken", "real", "")]
		[InlineData("PostVerifySecurityAnswersNoSecurityAnswersProvided", "One or more of your answers are incorrect. Please try again with a new set of questions.", "validUsername", "validActivityToken", "", "")]
		[InlineData("PostVerifySecurityAnswersInvalidFirstSecurityAnswerProvided", "One or more of your answers are incorrect. Please try again with a new set of questions.", "validUsername", "validActivityToken", "invalidAnswer")]
		[InlineData("PostVerifySecurityAnswersInvalidSecondSecurityAnswerProvided", "One or more of your answers are incorrect. Please try again with a new set of questions.", "validUsername", "validActivityToken", "real", "invalidAnswer")]
		[InlineData("PostVerifySecurityAnswersInvalidSecurityAnswersProvided", "One or more of your answers are incorrect. Please try again with a new set of questions.", "validUsername", "validActivityToken", "invalidAnswer", "invalidAnswer")]
		[InlineData("PostVerifySecurityAnswersOnlyOneSecurityAnswerJsonProvided", "One or more of your answers are incorrect. Please try again with a new set of questions.")]
		[InlineData("PostVerifySecurityAnswersNoSecurityAnswerJsonProvided", "No questions received from user.")]
		[InlineData("PostVerifySecurityAnswersMaximumInvalidAttempts", "Max attempts to answer security questions exceeded. Your account is locked.", "validUsername", "validActivityToken", "invalidAnswer")]
		[InlineData("PostVerifySecurityAnswersInvalidAttemptAfterMaximumInvalidAttempts", "Your account is locked.", "validUsername", "validActivityToken", "invalidAnswer")]
		[InlineData("PostVerifySecurityAnswersVerifyAccessAfterMaximumInvalidAttemptsUsingInvalidPayload", "One or more of your answers are incorrect. Please try again with a new set of questions.", "validUsername", "validActivityToken", "invalidAnswer")]
		[InlineData("PostVerifySecurityAnswersVerifyAccessAfterInvalidAttemptAfterMaximumInvalidAttemptsUsingInvalidPayload", "One or more of your answers are incorrect. Please try again with a new set of questions.", "validUsername", "validActivityToken", "invalidAnswer")]
		public void PostVerifySecurityAnswersNegativeCases(string testCase, string errorReason, string username = "validUsername"
			, string activityToken = "validActivityToken", string firstSecurityAnswer = "real", string secondSecurityAnswer = "real")
		{
			// Set up Payload
			username = username == "validUsername" ? Properties["enterpriseUsernameForStatusChange"] : username;
			if (testCase == "PostVerifySecurityAnswersExpiredActivityToken")
			{
				for (int countGetSecurityQuestions = 0; countGetSecurityQuestions < 2; countGetSecurityQuestions++)
				{
					EndPointUrl = HostUrl + Properties["GetSecurityQuestions"] + WebUtility.UrlEncode(Properties["enterpriseUsernameForStatusChange"]);
					GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");
					securityQuestionsApiResponse = countGetSecurityQuestions < 1? ResponseString : securityQuestionsApiResponse;
				}
			}
			if (testCase.Contains("MaximumInvalidAttempts"))
			{
				EndPointUrl = HostUrl + Properties["GetSecurityQuestions"] + WebUtility.UrlEncode(Properties["enterpriseUsernameForForgotPassword"]);
				GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");

				payload = reusable.DoPostVerifySecurityAnswersPayload(ResponseString, activityToken, Properties["enterpriseUsernameForForgotPassword"]);
			}
			else
			{
				payload = reusable.DoPostVerifySecurityAnswersPayload(securityQuestionsApiResponse, activityToken, Properties["enterpriseUsernameForStatusChange"])
				.Replace(Properties["enterpriseUsernameForStatusChange"], username);
			}
			payloadUserSecurityAnswer = JsonConvert.DeserializeObject<UserSecurityAnswer>(payload);
			if (firstSecurityAnswer != "real" || secondSecurityAnswer != "real")
			{
				payloadUserSecurityAnswer.SecurityQuestionAnswers.First().Answer = firstSecurityAnswer;
				payloadUserSecurityAnswer.SecurityQuestionAnswers.Last().Answer = secondSecurityAnswer;
			}
			if (testCase.Contains("JsonProvided"))
			{
				int countTotalSecurityQuestionAnswers = payloadUserSecurityAnswer.SecurityQuestionAnswers.Count;
				for (int countSecurityQuestionAnswers = 0; countSecurityQuestionAnswers < countTotalSecurityQuestionAnswers; countSecurityQuestionAnswers++)
				{
					payloadUserSecurityAnswer.SecurityQuestionAnswers.Remove(payloadUserSecurityAnswer.SecurityQuestionAnswers.First());
					if (testCase == "PostVerifySecurityAnswersOnlyOneSecurityAnswerJsonProvided")
					{
						break;
					}
				}
			}
			payload = JsonConvert.SerializeObject(payloadUserSecurityAnswer);
			XunitTestOutPut.WriteLine("Payload:\n" + payload);

			// Set up the API URL
			EndPointUrl = HostUrl + Properties["VerifySecurityAnswers"];

			// Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Post + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: payload);

			// Extract API's JSON Response
			SecurityAnswerResponse verifySecurityAnswers = JsonConvert.DeserializeObject<SecurityAnswerResponse>(ResponseString);
			if (testCase.Contains("MaximumInvalidAttempts"))
			{
				maxActivityAttemptCountQuestionAttempts = testCase == "PostVerifySecurityAnswersInvalidAttemptAfterMaximumInvalidAttempts" ? ++maxActivityAttemptCountQuestionAttempts : maxActivityAttemptCountQuestionAttempts;
				for (int countVerifySecurityAnswersMaximumInvalidAttempts = 0; countVerifySecurityAnswersMaximumInvalidAttempts < (maxActivityAttemptCountQuestionAttempts); countVerifySecurityAnswersMaximumInvalidAttempts++)
				{
					XunitTestOutPut.WriteLine("\n\nPOST VerifySecurityAnswers API Attempt #" + (countVerifySecurityAnswersMaximumInvalidAttempts + 1) + ":\n" + ResponseString);
					GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: payload);
				}
				if (testCase.Contains("InvalidPayload"))
				{
					XunitTestOutPut.WriteLine("\n\nUnlock User and try to execute API with invalid Security Answers:\n");
					reusable.DoResetMaximumActivityAttempts(Properties["enterpriseUsernameForStatusChange"], EndPointUrl, HttpVerb.Post, payload);
				}
				verifySecurityAnswers = JsonConvert.DeserializeObject<SecurityAnswerResponse>(ResponseString);
			}
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);

			// Unlock user if Maximum Attempt is reached.
			if (!string.IsNullOrEmpty(verifySecurityAnswers.ErrorReason))
			{
				if (verifySecurityAnswers.ErrorReason.Contains("locked"))
				{
					payloadUserSecurityAnswer.SecurityQuestionAnswers.First().Answer = "real";
					payloadUserSecurityAnswer.SecurityQuestionAnswers.Last().Answer = "real";
					payload = JsonConvert.SerializeObject(payloadUserSecurityAnswer);
					reusable.DoResetMaximumActivityAttempts(Properties["enterpriseUsernameForStatusChange"], EndPointUrl, HttpVerb.Post, payload);
				}
			}

			// Assert
			Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
			Assert.Null(verifySecurityAnswers.EnterpriseUserName);
			Assert.NotNull(verifySecurityAnswers.IsAnswersCorrect);
			Assert.False(verifySecurityAnswers.IsAnswersCorrect, "verifySecurityAnswers.isAnswersCorrect");
			Assert.Null(verifySecurityAnswers.CorrectAnswerToken);
			if ((firstSecurityAnswer != "real" || secondSecurityAnswer != "real") && !testCase.EndsWith("MaximumInvalidAttempts"))
			{
				Assert.NotNull(verifySecurityAnswers.SecurityQuestions);
				Assert.True(verifySecurityAnswers.SecurityQuestions.Count == 2);
			}
			else
			{
				Assert.Null(verifySecurityAnswers.SecurityQuestions);
			}
			Assert.NotNull(verifySecurityAnswers.IsError);
			Assert.True(verifySecurityAnswers.IsError, "verifySecurityAnswers.isError");
			Assert.NotNull(verifySecurityAnswers.ErrorReason);
			Assert.True(verifySecurityAnswers.ErrorReason == errorReason, $"verifySecurityAnswers.errorReason is not \"{errorReason}\"");
		}
		
		//[Theory]
		//[Trait("Data-Driven", "Negative Case")]
		[InlineData("PostVerifySecurityAnswersNullFirstSecurityAnswerProvided", null)]
		[InlineData("PostVerifySecurityAnswersNullSecondSecurityAnswerProvided", "real", null)]
		[InlineData("PostVerifySecurityAnswersNullSecurityAnswersProvided", null, null)]
		public void PostVerifySecurityAnswersNegativeCasesInternalServerError(string testCase, string firstSecurityAnswer = "real", string secondSecurityAnswer = "real")
		{
			// Set up Payload
			payload = reusable.DoPostVerifySecurityAnswersPayload(securityQuestionsApiResponse);
			payloadUserSecurityAnswer = JsonConvert.DeserializeObject<UserSecurityAnswer>(payload);
			payloadUserSecurityAnswer.SecurityQuestionAnswers.First().Answer = firstSecurityAnswer;
			payloadUserSecurityAnswer.SecurityQuestionAnswers.Last().Answer = secondSecurityAnswer;
			payload = JsonConvert.SerializeObject(payloadUserSecurityAnswer);
			XunitTestOutPut.WriteLine("Payload:\n" + payload);

			// Set up the API URL
			EndPointUrl = HostUrl + Properties["VerifySecurityAnswers"];

			// Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Post + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: payload);

			// Extract API's JSON Response
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);

			// Assert
			Assert.True(HttpStatusCode.InternalServerError == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
			Assert.True(ResponseString.StartsWith("Internal System Error. Please contact RealPage support with error reference Id - "),
				"ResponseString.StartsWith(\"Internal System Error. Please contact RealPage support with error reference Id - \")");
		}
	}
}
