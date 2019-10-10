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
    public class TestBase
    {
        public readonly ILog Logger = LogManager.GetLogger(typeof(TestBase));
        public RestController_2 RestClient = new RestController_2();
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
        public HttpStatusCode HttpStatusCode { get; set; }
        public string HttpStatusCodeDisc { get; set; }

		public ObjectOutput<PasswordPolicy, ErrorData> PasswordPolicyDefault { get;  set;}

		private string PropPath { get; }
        private readonly bool _propertyLoaded;

        protected string _accessToken = string.Empty;

		public string CurrentlyLoggedInUser { get; set; }

		public TestBase()
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

			DatabaseController dbManager = new DatabaseController(DbConnString);
			CurrentlyLoggedInUser = Properties["enterpriseUsername"];
				//dbManager.executeQuery("SELECT TOP 1 pcm.partyid, ul.partyid, ul.loginname "
				//+ "FROM[" + Properties["identityDatabase"] + "].[Ident].[IdentityProviderType] idpt "
				//+ "inner join[" + Properties["identityDatabase"] + "].[Ident].[IdentityProviderSettingType] idpst "
				//+ "inner join[" + Properties["identityDatabase"] + "].[Ident].[IdentityProviderSetting] idps "
				//+ "inner join[" + Properties["identityDatabase"] + "].[Ident].[ContactMechanismIdentity] cmid "
				//+ "inner join[" + Properties["identityDatabase"] + "].[Enterprise].[PartyContactMechanism] pcm "
				//+ "inner join[" + Properties["identityDatabase"] + "].[Enterprise].[Organization] o "
				//+ "inner join[" + Properties["identityDatabase"] + "].[Enterprise].[PartyRelationship] pr "
				//+ "inner join[identity].[Ident].[UserLogin] ul "
				//+ "on ul.partyid = pr.partyidfrom "
				//+ "on pr.partyidto = o.partyid "
				//+ "on pcm.partyid = o.partyid "
				//+ "on pcm.contactmechanismid = cmid.contactmechanismid "
				//+ "on cmid.[IdentityProviderSettingId] = idps.[IdentityProviderSettingId] "
				//+ "on idps.identityprovidersettingtypeid = idpst.identityprovidersettingtypeid "
				//+ "on idpt.identityprovidertypeid = idpst.identityprovidertypeid "
				//+ "where idpt.description = 'IdentityServer' ").Rows[0]["LoginName"].ToString();

			_accessToken = GetClientToken(Properties["identityClientUrl"], CurrentlyLoggedInUser, "P@ssw0rd");			
		}

		protected static string GetClientToken(string _identityClientUrl, string _enterpriseUsername, string _password)
        {
            var client = new TokenClient(
				_identityClientUrl, // To DO: You may get this URL from properies files based on environment 
                "qaautomation", // client id
                "33B5F798-BE55-42BC-8AA8-0025B903DC3B"); // client secret

            var tokenResponse = client.RequestResourceOwnerPasswordAsync(_enterpriseUsername, _password, "rplandingapi").Result;// To Do : some APIs may require diffrent users to test roles etc; think to have method which will take user/pwd which will be in API & pass token to RestController

            if (tokenResponse.IsError)
            {
				Exception tokenError = new Exception($"Error while getting token - error - {tokenResponse.Error} - decription - {tokenResponse.ErrorDescription}");
				return tokenError.Message;
            }

            return tokenResponse.AccessToken;
        }

        /// <summary>
        /// This method facilitates HTTPWebResponseStatusCode for the tests
        /// </summary>
        /// <param name="endPointUrl"></param>
        /// <param name="authHeader"></param>
        /// <param name="httpVerb"></param>
        /// <param name="jsonPayload"></param>
        /// <returns>string response (or Exception.Message) in json format</returns>
        public HttpWebResponse GetHttpWebResponse(string endPointUrl, string authHeader, HttpVerb httpVerb, string jsonPayload = "")
        {
            HttpWebResponse httpWResponse = null;
            try
            {
                Logger.Info("Calling " + httpVerb + " at " + endPointUrl);
                Logger.Debug("Header" + "\n" + authHeader);

                RestClient = new RestController_2(
                    endpoint: endPointUrl, method: httpVerb, contentType: "application/json",
                    accept: "application/json", authorization: authHeader, postData: jsonPayload);
                httpWResponse = RestClient.MakeRequestHttpResponse();
                
                HttpStatusCode = httpWResponse.StatusCode;
                Logger.Debug("response" + "\n" + httpWResponse.StatusDescription);
                HttpStatusCodeDisc = httpWResponse.StatusDescription;
            }

            catch (Exception e)
            {
                String baseExcep = e.GetBaseException().GetType().FullName;

                if (baseExcep != null && baseExcep.Equals("System.Net.WebException"))
                {
                    HttpWebResponse response = (HttpWebResponse)((WebException)e).Response;
                    if (response != null)
                    {
                        HttpStatusCode = response.StatusCode;
                        HttpStatusCodeDisc = response.StatusDescription;
                        httpWResponse = response;
                    }
                    else
                    {
                        Logger.Error("ERROR: " + e.Message);
                        //Reporter.LogEvent("JsonResponse should be received", e.Message, Reporter.fStatus);
                    }
                }
                else
                {
                    Logger.Error("ERROR: " + e.Message);
                    //Reporter.LogEvent("JsonResponse should be received", e.Message, Reporter.fStatus);
                }
            }
            Logger.Info("Response received.");
            if (httpWResponse != null)
            {
                Logger.Debug("\n\nHTTP Status Code: " + httpWResponse.StatusCode + "\n");
                return httpWResponse;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        ///  Get the API's HttpWebResponseValue
        /// </summary>
        /// <returns></returns>
        public string GetHttpWebResponseValue(HttpWebResponse httpWebResponse)
        {
            //EndPointUrl = HostUrl + Properties["endPointCommonUser"] + "?datafilter=" + Payload;
            string response = "";

            // grab the response
            var responseStream = httpWebResponse.GetResponseStream();
            if (responseStream != null)
                using (var reader = new StreamReader(responseStream))
                {
                    response = reader.ReadToEnd();
                }

            Logger.Debug("Response:\n" + response);
            return response;
        }
    }
}

