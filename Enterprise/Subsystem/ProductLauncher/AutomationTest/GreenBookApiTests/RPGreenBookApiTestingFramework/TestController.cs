using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using IdentityModel.Client;
using log4net;
using RPBooksApiTestingFramework.Controllers;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using System.Data;

namespace RPBooksApiTestingFramework
{
    public class TestController
    {
        public readonly ILog Logger = LogManager.GetLogger(typeof(TestController));
        public RestController RestClient;
        public string HostUrl { get; set; }
        public string HostIdentityUrl { get; set; }
        public string EndPointUrl { get; set; }
        public string DbConnString { get; set; }
        public string DataPath = AppDomain.CurrentDomain.BaseDirectory + "\\data\\";
        public Dictionary<string, string> Properties;
        public string Payload;
        public string JsonRequestPath;
        //JsonController JsonManager = new JsonController();
        public DateTime CDate;
        public string AuthHeader { get; set; }
        public Dictionary<String, String> Auth;
		public string ResponseString { get; set; }
		public HttpStatusCode ResponseHttpStatusCode { get; set; }

		public ObjectOutput<PasswordPolicy, ErrorData> PasswordPolicyDefault { get;  set;}

		public string _identityClientId, _identityClientSecret;

		private string PropPath { get; }
        private readonly bool _propertyLoaded;

        public string _accessToken = string.Empty;

		public string CurrentlyLoggedInUser { get; set; }
        public string[] loginUser { get; set; }

		public TestController()
        {
            if (_propertyLoaded) return;
            //Find properties.ini path 
            PropPath = PropertiesController.GetPropertiesPath();

            //Load properties from .ini file
            Properties = PropertiesController.ReadProperties(PropPath);

            //Set hostUrl
            if (HostUrl == null) HostUrl = Properties["hostUrl"];
            if (HostIdentityUrl == null) HostIdentityUrl = Properties["hostIdentityUrl"];

            //Set dbConnString
            if (DbConnString == null) DbConnString = Properties["dbConnection"];

            _propertyLoaded = true;
			
			CurrentlyLoggedInUser = Properties["enterpriseUsername"];
            loginUser = Properties["LoginUser"].Split('|');

            _identityClientId = Properties["identityClientId"];
			_identityClientSecret = Properties["identityClientSecret"];
			_accessToken = GetClientToken(Properties["identityClientUrl"], loginUser[0], loginUser[1]);			
		}

		public string GetClientToken(string _identityClientUrl, string _enterpriseUsername, string _password)
        {
            var client = new TokenClient(
				_identityClientUrl, // To DO: You may get this URL from properies files based on environment 
                _identityClientId, // client id
                _identityClientSecret); // client secret

            var tokenResponse = client.RequestResourceOwnerPasswordAsync(_enterpriseUsername, _password, "rplandingapi").Result;// To Do : some APIs may require diffrent users to test roles etc; think to have method which will take user/pwd which will be in API & pass token to RestController

            if (tokenResponse.IsError)
            {
				Exception tokenError = new Exception($"Error while getting token - error - {tokenResponse.Error} - decription - {tokenResponse.ErrorDescription}");
				return tokenError.Message;
            }

            return "Bearer " + tokenResponse.AccessToken;
        }

        /// <summary>
        /// This method facilitates HTTPWebResponseStatusCode for the tests
        /// </summary>
        /// <param name="endPointUrl"></param>
        /// <param name="authHeader"></param>
        /// <param name="httpVerb"></param>
        /// <param name="jsonPayload"></param>
        /// <returns>string response (or Exception.Message) in json format</returns>
        public void GetHttpWebResponse(string endPointUrl, string authHeader, HttpVerb httpVerb, string jsonPayload = null)
		{
			Logger.Info("Calling " + httpVerb + " at " + endPointUrl);
            Logger.Debug("Header" + "\n" + authHeader);

            RestClient = new RestController(
                endpoint: endPointUrl, method: httpVerb, contentType: "application/json",
                accept: "application/json", authorization: _accessToken, postData: jsonPayload);
            var httpWResponse = RestClient.MakeRequestHttpResponse();
                
            ResponseHttpStatusCode = httpWResponse.Item1;
			ResponseString = httpWResponse.Item2;
			Logger.Info("ResponseHttpStatusCode : " + ResponseHttpStatusCode);
			Logger.Debug("ResponseString : " + ResponseString);
		}

        public void GetHttpWebResponseIdentity(string endPointUrl, string authHeader, HttpVerb httpVerb, string jsonPayload = null)
        {
            Logger.Info("Calling " + httpVerb + " at " + endPointUrl);
            Logger.Debug("Header" + "\n" + authHeader);

            RestClient = new RestController(
                endpoint: endPointUrl, method: httpVerb, contentType: "application/json",
                accept: "application/json", postData: jsonPayload);
            var httpWResponse = RestClient.MakeRequestHttpResponseIdentity();

            ResponseHttpStatusCode = httpWResponse.Item1;
            ResponseString = httpWResponse.Item2;
            Logger.Info("ResponseHttpStatusCode : " + ResponseHttpStatusCode);
            Logger.Debug("ResponseString : " + ResponseString);


            
        }
        

    }
}

