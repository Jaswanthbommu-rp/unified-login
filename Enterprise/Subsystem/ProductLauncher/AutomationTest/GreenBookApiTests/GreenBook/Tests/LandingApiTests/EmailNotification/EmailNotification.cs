using System.Net;
using Xunit;
using Xunit.Abstractions;
using Newtonsoft.Json;
using RPBooksApiTestingFramework;
using RPBooksApiTestingFramework.Controllers;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using System;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using System.Collections.Generic;

namespace GreenBook.Tests
{
	public class EmailNotification : TestController
	{
		public EmailNotification(ITestOutputHelper _xUnitTestOutput)
		{
			reusable = new TestUtilities(this);
			this.XunitTestOutPut = _xUnitTestOutput;
			newLoginName = Guid.NewGuid().ToString().Remove(7) + "@ApiTest.com";
			payload = reusable.DoPostNewUserPayload(newLoginName);
			newUserToken = reusable.DoPostNewUserToken(payload);
		}

		private string payload;
		JsonController jsonManager = new JsonController();
		TestUtilities reusable;
		private readonly ITestOutputHelper XunitTestOutPut;
		private string newUserToken, newLoginName;

		//EmailNotification=/api/emailnotification/newprofile

		//[Fact, Trait("", "Happy Path")]
		public void PostEmailNotification()
		{
			// Set up Payload
			XunitTestOutPut.WriteLine("Payload:\n" + payload);

			// Set up the API URL
			EndPointUrl = HostUrl + Properties["EmailNotification"] + "?newUserToken=" + newUserToken;

			// Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Post + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: payload);

			// Extract API's JSON Response
			
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);
			EmailProcessResponse emailProcessResponse = JsonConvert.DeserializeObject<EmailProcessResponse>(ResponseString);

			// Assert
			Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
			Assert.NotNull(emailProcessResponse.EmailResponse);
			Assert.True(emailProcessResponse.EmailResponse.Contains("<!DOCTYPE html>")
				, "emailProcessResponse.EmailResponse.Contains(\"< !DOCTYPE html >\")");
			//Assert.NotNull(emailProcessResponse.IsError);
			//Assert.False(emailProcessResponse.IsError, "emailProcessResponse.isError");
			//Assert.Null(emailProcessResponse.ErrorReason);
		}

		//[Fact, Trait("", "Negative Case")]
		public void PostEmailNotificationWithoutNewUserToken()
		{
			// Set up Payload
			XunitTestOutPut.WriteLine("Payload:\n" + payload);

			// Set up the API URL
			EndPointUrl = HostUrl + Properties["EmailNotification"] + "?newUserToken=";

			// Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Post + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: payload);

			// Extract API's JSON Response
			
			EmailProcessResponse emailProcessResponse = JsonConvert.DeserializeObject<EmailProcessResponse>(ResponseString);
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);

			// Assert
			Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
			Assert.NotNull(emailProcessResponse.EmailResponse);
			Assert.True(emailProcessResponse.EmailResponse.Contains("<!DOCTYPE html>")
				, "emailProcessResponse.EmailResponse.Contains(\"< !DOCTYPE html >\")");
			//Assert.NotNull(emailProcessResponse.IsError);
			//Assert.False(emailProcessResponse.IsError, "emailProcessResponse.isError");
			//Assert.Null(emailProcessResponse.ErrorReason);
		}

		//[Fact, Trait("", "Negative Case")]
		public void PostEmailNotificationWithoutLoginName()
		{
			// Set up Payload
			payload = reusable.DoPostNewUserPayload("");
			XunitTestOutPut.WriteLine("Payload:\n" + payload);

			// Set up the API URL
			EndPointUrl = HostUrl + Properties["EmailNotification"] + "?newUserToken=" + newUserToken;

			// Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Post + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: payload);

			// Extract API's JSON Response
			
			EmailProcessResponse emailProcessResponse = JsonConvert.DeserializeObject<EmailProcessResponse>(ResponseString);
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);

			// Assert
			Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
			Assert.NotNull(emailProcessResponse.EmailResponse);
			Assert.True(emailProcessResponse.EmailResponse.Contains("<!DOCTYPE html>")
				, "emailProcessResponse.EmailResponse.Contains(\"< !DOCTYPE html >\")");
			//Assert.NotNull(emailProcessResponse.IsError);
			//Assert.False(emailProcessResponse.IsError, "emailProcessResponse.isError");
			//Assert.Null(emailProcessResponse.ErrorReason);
		}

		//[Fact, Trait("", "Negative Case")]
		public void PostEmailNotificationWithNullLoginName()
		{
			// Set up Payload
			payload = reusable.DoPostNewUserPayload(null);
			XunitTestOutPut.WriteLine("Payload:\n" + payload);

			// Set up the API URL
			EndPointUrl = HostUrl + Properties["EmailNotification"] + "?newUserToken=" + newUserToken;

			// Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Post + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: payload);

			// Extract API's JSON Response
			
			EmailProcessResponse emailProcessResponse = JsonConvert.DeserializeObject<EmailProcessResponse>(ResponseString);
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);

			// Assert
			Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
			Assert.NotNull(emailProcessResponse.EmailResponse);
			Assert.True(emailProcessResponse.EmailResponse.Contains("<!DOCTYPE html>")
				, "emailProcessResponse.EmailResponse.Contains(\"< !DOCTYPE html >\")");
			//Assert.NotNull(emailProcessResponse.IsError);
			//Assert.False(emailProcessResponse.IsError, "emailProcessResponse.isError");
			//Assert.Null(emailProcessResponse.ErrorReason);
		}

		//[Fact, Trait("", "Negative Case")]
		public void PostEmailNotificationWithoutFirstName()
		{
			// Set up Payload
			payload = reusable.DoPostNewUserPayload(newLoginName, "");
			XunitTestOutPut.WriteLine("Payload:\n" + payload);

			// Set up the API URL
			EndPointUrl = HostUrl + Properties["EmailNotification"] + "?newUserToken=" + newUserToken;

			// Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Post + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: payload);

			// Extract API's JSON Response
			
			EmailProcessResponse emailProcessResponse = JsonConvert.DeserializeObject<EmailProcessResponse>(ResponseString);
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);

			// Assert
			Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
			Assert.NotNull(emailProcessResponse.EmailResponse);
			Assert.True(emailProcessResponse.EmailResponse.Contains("<!DOCTYPE html>")
				, "emailProcessResponse.EmailResponse.Contains(\"< !DOCTYPE html >\")");
			//Assert.NotNull(emailProcessResponse.IsError);
			//Assert.False(emailProcessResponse.IsError, "emailProcessResponse.isError");
			//Assert.Null(emailProcessResponse.ErrorReason);
		}

		//[Fact, Trait("", "Negative Case")]
		public void PostEmailNotificationWithNullFirstName()
		{
			// Set up Payload
			payload = reusable.DoPostNewUserPayload(newLoginName, null);
			XunitTestOutPut.WriteLine("Payload:\n" + payload);

			// Set up the API URL
			EndPointUrl = HostUrl + Properties["EmailNotification"] + "?newUserToken=" + newUserToken;

			// Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Post + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: payload);

			// Extract API's JSON Response
			
			EmailProcessResponse emailProcessResponse = JsonConvert.DeserializeObject<EmailProcessResponse>(ResponseString);
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);

			// Assert
			Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
			Assert.NotNull(emailProcessResponse.EmailResponse);
			Assert.True(emailProcessResponse.EmailResponse.Contains("<!DOCTYPE html>")
				, "emailProcessResponse.EmailResponse.Contains(\"< !DOCTYPE html >\")");
			//Assert.NotNull(emailProcessResponse.IsError);
			//Assert.False(emailProcessResponse.IsError, "emailProcessResponse.isError");
			//Assert.Null(emailProcessResponse.ErrorReason);
		}

		//[Fact, Trait("", "Negative Case")]
		public void PostEmailNotificationWithoutLastName()
		{
			// Set up Payload
			payload = reusable.DoPostNewUserPayload(newLoginName, "", "");
			XunitTestOutPut.WriteLine("Payload:\n" + payload);

			// Set up the API URL
			EndPointUrl = HostUrl + Properties["EmailNotification"] + "?newUserToken=" + newUserToken;

			// Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Post + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: payload);

			// Extract API's JSON Response
			
			EmailProcessResponse emailProcessResponse = JsonConvert.DeserializeObject<EmailProcessResponse>(ResponseString);
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);

			// Assert
			Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
			Assert.NotNull(emailProcessResponse.EmailResponse);
			Assert.True(emailProcessResponse.EmailResponse.Contains("<!DOCTYPE html>")
				, "emailProcessResponse.EmailResponse.Contains(\"< !DOCTYPE html >\")");
			//Assert.NotNull(emailProcessResponse.IsError);
			//Assert.False(emailProcessResponse.IsError, "emailProcessResponse.isError");
			//Assert.Null(emailProcessResponse.ErrorReason);
		}

		//[Fact, Trait("", "Negative Case")]
		public void PostEmailNotificationWithNullLastName()
		{
			// Set up Payload
			payload = reusable.DoPostNewUserPayload(newLoginName, "", null);
			XunitTestOutPut.WriteLine("Payload:\n" + payload);

			// Set up the API URL
			EndPointUrl = HostUrl + Properties["EmailNotification"] + "?newUserToken=" + newUserToken;

			// Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Post + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: payload);

			// Extract API's JSON Response
			
			EmailProcessResponse emailProcessResponse = JsonConvert.DeserializeObject<EmailProcessResponse>(ResponseString);
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);

			// Assert
			Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
			Assert.NotNull(emailProcessResponse.EmailResponse);
			Assert.True(emailProcessResponse.EmailResponse.Contains("<!DOCTYPE html>")
				, "emailProcessResponse.EmailResponse.Contains(\"< !DOCTYPE html >\")");
			//Assert.NotNull(emailProcessResponse.IsError);
			//Assert.False(emailProcessResponse.IsError, "emailProcessResponse.isError");
			//Assert.Null(emailProcessResponse.ErrorReason);
		}

		//[Fact, Trait("", "Negative Case")]
		public void PostEmailNotificationWithoutRealPageId()
		{
			// Set up Payload
			ProfileDetail profileDetailRequest = JsonConvert.DeserializeObject<ProfileDetail>(payload);
			string jsonProfileDetailOrganization = JsonConvert.SerializeObject(profileDetailRequest.organization).
				Replace(profileDetailRequest.organization[0].RealPageId.ToString(), "");
			profileDetailRequest.organization = JsonConvert.DeserializeObject<IList<
				RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Organization>>(jsonProfileDetailOrganization);
			XunitTestOutPut.WriteLine("Payload:\n" + payload);

			// Set up the API URL
			EndPointUrl = HostUrl + Properties["EmailNotification"] + "?newUserToken=" + newUserToken;

			// Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Post + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: payload);

			// Extract API's JSON Response
			
			EmailProcessResponse emailProcessResponse = JsonConvert.DeserializeObject<EmailProcessResponse>(ResponseString);
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);

			// Assert
			Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
			Assert.NotNull(emailProcessResponse.EmailResponse);
			Assert.True(emailProcessResponse.EmailResponse.Contains("<!DOCTYPE html>")
				, "emailProcessResponse.EmailResponse.Contains(\"< !DOCTYPE html >\")");
			//Assert.NotNull(emailProcessResponse.IsError);
			//Assert.False(emailProcessResponse.IsError, "emailProcessResponse.isError");
			//Assert.Null(emailProcessResponse.ErrorReason);
		}

		//[Fact, Trait("", "Negative Case")]
		public void PostEmailNotificationWithNullRealPageId()
		{
			// Set up Payload
			ProfileDetail profileDetailRequest = JsonConvert.DeserializeObject<ProfileDetail>(payload);
			string jsonProfileDetailOrganization = JsonConvert.SerializeObject(profileDetailRequest.organization).
				Replace("\"" + profileDetailRequest.organization[0].RealPageId.ToString() + "\"", "null" );
			profileDetailRequest.organization = JsonConvert.DeserializeObject<IList<
				RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Organization>>(jsonProfileDetailOrganization);
			XunitTestOutPut.WriteLine("Payload:\n" + payload);

			// Set up the API URL
			EndPointUrl = HostUrl + Properties["EmailNotification"] + "?newUserToken=" + newUserToken;

			// Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Post + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: payload);

			// Extract API's JSON Response
			
			EmailProcessResponse emailProcessResponse = JsonConvert.DeserializeObject<EmailProcessResponse>(ResponseString);
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);

			// Assert
			Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
			Assert.NotNull(emailProcessResponse.EmailResponse);
			Assert.True(emailProcessResponse.EmailResponse.Contains("<!DOCTYPE html>")
				, "emailProcessResponse.EmailResponse.Contains(\"< !DOCTYPE html >\")");
			//Assert.NotNull(emailProcessResponse.IsError);
			//Assert.False(emailProcessResponse.IsError, "emailProcessResponse.isError");
			//Assert.Null(emailProcessResponse.ErrorReason);
		}
	}
}
