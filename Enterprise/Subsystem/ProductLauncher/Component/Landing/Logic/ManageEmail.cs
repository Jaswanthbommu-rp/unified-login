using Newtonsoft.Json;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Helper;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using Serilog;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic
{
    /// <summary>
    /// Manage Email Notification repository calls
    /// </summary>
    public class ManageEmail : IManageEmail
    {
        #region Private Variables
        DefaultUserClaim _defaultUserClaim;
        IEmailRepository _emailRepository;
        IProductInternalSettingRepository _productInternalSettingRepository;
        #endregion

        #region Constructors
        /// <summary>
        /// ManageEmail  (xUnit Tests)
        /// </summary>
        /// <param name="defaultUserClaim">User Claim</param>
        /// <param name="emailRepository">Email Repository</param>
        /// <param name="productInternalSettingRepository">Product Setting repository</param>
        public ManageEmail(DefaultUserClaim defaultUserClaim, IEmailRepository emailRepository, IProductInternalSettingRepository productInternalSettingRepository)
        {
            _defaultUserClaim = defaultUserClaim;
            _emailRepository = emailRepository;
            _productInternalSettingRepository = productInternalSettingRepository;
        }

        /// <summary>
        /// Create a basic instance of the ManageEmail Controller class
        /// </summary>
        /// 
        public ManageEmail(DefaultUserClaim defaultUserClaim)
        {
            _defaultUserClaim = defaultUserClaim;
            _emailRepository = new EmailRepository();
            _productInternalSettingRepository = new ProductInternalSettingRepository();
        }
        #endregion

        #region Public ManageEmail methods
        /// <summary>
        /// Get Email Template
        /// </summary>
        /// <param name="communicationEventAudienceTypeId">CommunicationEventAudienceTypeId</param>
        /// <param name="communicationEventPurposeTypeId">CommunicationEventPurposeTypeId</param>
        /// <returns>CommunicationEmail object</returns>
        public CommunicationEmail GetEmailTemplate(int communicationEventAudienceTypeId, int communicationEventPurposeTypeId)
        {
            if (communicationEventAudienceTypeId == 0)
            {
                throw new ArgumentNullException(nameof(communicationEventAudienceTypeId), "Audience Type Id is required.");
            }
            else if (communicationEventPurposeTypeId == 0)
            {
                throw new ArgumentNullException(nameof(communicationEventPurposeTypeId), "Purpose Type Id is required.");
            }

            return _emailRepository.GetEmailTemplate(communicationEventAudienceTypeId, communicationEventPurposeTypeId);
        }

        /// <summary>
        /// Build Welcome Email for New Users
        /// </summary>
        /// <returns>Email object</returns>
        public Email CreateWelcomeEmail(string loginName, string firstName, string companyName, long orgPartyId, CommunicationEmail emailTemplate, string newUserToken, string senderEmailAddress = "", string notificationEmail = "")
        {
            Email cesEmail = new Email();

            if (string.IsNullOrEmpty(firstName))
            {
                throw new ArgumentNullException("FirstName name is missing.");
            }
            else if (string.IsNullOrEmpty(companyName))
            {
                throw new ArgumentNullException("Organization name is missing.");
            }
            else if (orgPartyId == 0)
            {
                throw new ArgumentNullException("Organization id is missing.");
            }
            else if (emailTemplate == null)
            {
                throw new ArgumentNullException(nameof(emailTemplate), "Email template is missing.");
            }
            else if (string.IsNullOrEmpty(notificationEmail) == true)
            {
                throw new ArgumentNullException(nameof(notificationEmail), "Notification Email is missing.");
            }
            else if (!EmailFormatValidation.IsValidEmail(senderEmailAddress))
            {
                throw new ArgumentNullException(nameof(senderEmailAddress), "Email Sender has an invalid email format.");
            }
            else if (!EmailFormatValidation.IsValidEmail(notificationEmail))
            {
                throw new ArgumentNullException(nameof(notificationEmail), "Notification Email has an invalid email format.");
            }

            try
            {
                String emailAddressTo = notificationEmail;
                int expiryDays = 0;

                ICredentialRepository _credentialRepository = new CredentialRepository();
                var actvityDetail = _credentialRepository.GetActivities(orgPartyId);
                var newUserRegistrationActivity = actvityDetail.FirstOrDefault(x => x.ActivityTypeId == (int)ActivityType.NewUserRegistration);
                if (newUserRegistrationActivity != null && newUserRegistrationActivity.ActivityTokenExpirationMinutes > 0)
                {
                    expiryDays = newUserRegistrationActivity.ActivityTokenExpirationMinutes / 1440;
                }

                cesEmail.EmailFrom = string.IsNullOrEmpty(senderEmailAddress) ? cesEmail.EmailFrom : senderEmailAddress;
                cesEmail.EmailTo = emailAddressTo;
                cesEmail.EmailSubject = emailTemplate.Subject;

                var emailBodyBuilder = new StringBuilder(emailTemplate.Body);
                emailBodyBuilder.Replace("{COMPANY NAME}", companyName);
                emailBodyBuilder.Replace("{FIRST NAME}", firstName);
                emailBodyBuilder.Replace("{LINK}", BuildNewUserLink(newUserToken, loginName));
                emailBodyBuilder.Replace("{IMAGES}", ConfigReader.GetImagesUri);
                emailBodyBuilder.Replace("{UNIFIED}", ConfigReader.GetDocumentUri);
                emailBodyBuilder.Replace("{EXPIRYDAYS}", Convert.ToString(expiryDays));
                cesEmail.EmailBody = emailBodyBuilder.ToString();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }

            return cesEmail;
        }

        public string BuildNewUserLink(string newUserToken, string loginName)
        {
            string link = ConfigReader.GetIssuerUri;
            if (link.EndsWith("/"))
            {
                link = link.Substring(0, link.Length - 1);
            }

            string encodedToken = Convert.ToBase64String(Encoding.UTF8.GetBytes(newUserToken));

            link += $"/PasswordRecovery/RedirectFromInviteEmail?newUserToken={encodedToken}&loginName={loginName}";

            return link;
        }

        /// <summary>
        /// Send an Email through CES
        /// </summary>
        /// <param name="email">Email data object</param>
        /// <returns>Email Status</returns>
        public string SendEmail(Email email)
        {
            if (email == null)
            {
                throw new ArgumentNullException(nameof(email), "Null Email.");
            }

            //CES
            StringBuilder errorXML = new StringBuilder();
            XmlDocument xmlCESInput = new XmlDocument();
            xmlCESInput.LoadXml("<Input xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns='EmailMessage.xsd'><Parameters><EmailMessages><EmailMessage><ClientUniqueID></ClientUniqueID><ClientProductName>OneSite-Letters And Notices</ClientProductName><EntityID></EntityID><SiteID></SiteID><To></To><From></From><Subject></Subject><Body></Body><Cc /><Bcc /><ReplyTo></ReplyTo><AttachmentPath></AttachmentPath></EmailMessage></EmailMessages></Parameters></Input>");

            XmlNamespaceManager nsmgr = new XmlNamespaceManager(xmlCESInput.NameTable);
            nsmgr.AddNamespace("inp", "EmailMessage.xsd");
            xmlCESInput.SelectSingleNode("//inp:ClientUniqueID", nsmgr).InnerText = email.ClientUniqueID.ToString();
            xmlCESInput.SelectSingleNode("//inp:EntityID", nsmgr).InnerText = email.EntityID;
            xmlCESInput.SelectSingleNode("//inp:SiteID", nsmgr).InnerText = email.SiteID;
            xmlCESInput.SelectSingleNode("//inp:To", nsmgr).InnerText = email.EmailTo;
            xmlCESInput.SelectSingleNode("//inp:From", nsmgr).InnerText = email.EmailFrom;
            xmlCESInput.SelectSingleNode("//inp:Subject", nsmgr).InnerText = email.EmailSubject;
            xmlCESInput.SelectSingleNode("//inp:ReplyTo", nsmgr).InnerText = email.EmailReplyTo;
            xmlCESInput.SelectSingleNode("//inp:AttachmentPath", nsmgr).InnerText = email.EmailAttachment;
            errorXML.Append(xmlCESInput.InnerXml);
            xmlCESInput.SelectSingleNode("//inp:Body", nsmgr).InnerText = email.EmailBody.ToString();

            try
            {
                string soap = "<?xml version='1.0' encoding='utf-8'?><soap:Envelope xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:soap='http://schemas.xmlsoap.org/soap/envelope/'><soap:Body><SendEmail xmlns='http://realpage.com/webservices'><InputXML>";
                soap += "<![CDATA[" + xmlCESInput.InnerXml + "]]>";
                soap += "</InputXML></SendEmail></soap:Body></soap:Envelope>";

                string CESURL = ConfigReader.GetCESURL;
                HttpWebRequest myrequest = (HttpWebRequest)WebRequest.Create(CESURL);
                myrequest.Method = "POST";

                myrequest.Headers.Add("SOAPAction", "http://realpage.com/webservices/SendEmail");
                myrequest.KeepAlive = false;
                myrequest.ContentType = "text/xml; charset=utf-8";
                StreamWriter OStream = new StreamWriter(myrequest.GetRequestStream());
                OStream.Write(soap);
                OStream.Close();
                myrequest.Timeout = 999999;

                //Create and get the Web Response from the request
                HttpWebResponse myresponse = (HttpWebResponse)myrequest.GetResponse();

                XmlDocument oXMLTemp = new XmlDocument();
                XmlDocument xmlWebResponse = new XmlDocument();

                XmlNamespaceManager nsMgr = new XmlNamespaceManager(xmlWebResponse.NameTable);
                nsMgr.AddNamespace("ns", "http://realpage.com/webservices");
                // Load Response
                Stream str = myresponse.GetResponseStream();
                xmlWebResponse.Load(str);
                // Close Response
                myresponse.Close();
                xmlWebResponse.LoadXml(xmlWebResponse.InnerXml);
                oXMLTemp.LoadXml(HttpUtility.HtmlDecode(xmlWebResponse.SelectSingleNode("//ns:SendEmailResult", nsMgr).InnerXml));
                XmlNode oNode;
                oNode = oXMLTemp.SelectSingleNode("//Output/Result");
                if (oNode != null)
                {
                    if (oNode.Attributes["Status"].Value == "1")
                    {
                        return "Email sent successfully.";
                    }
                    else
                    {
                        return "An error occured when sending the email.";
                    }
                }
                else
                {
                    return "An error occured when sending the email.";
                }
            }
            catch (Exception ex)
            {
                return "An error occured when sending the email.";
            }
        }

        /// <summary>
        /// Send an Email through SendGrid
        /// </summary>
        /// <param name="sendGridEmail">SendGrid Email object</param>
        /// <returns>Email Status</returns>
        public string SendGridEmail(ISendGridEmail sendGridEmail)
        {
            if (sendGridEmail == null)
            {
                throw new ArgumentNullException(nameof(sendGridEmail), "Null email object.");
            }

            if ((sendGridEmail.toAddress == null) || (sendGridEmail.toAddress.Count == 0))
            {
                throw new ArgumentNullException(nameof(sendGridEmail), "No recipient was provided in the  email object.");
            }
            sendGridEmail.toAddress.ToList().ForEach(e =>
            {
                if (!EmailFormatValidation.IsValidEmail(e.email))
                {
                    throw new ArgumentNullException(nameof(e.email), "Email recipient has an invalid email format.");
                }
            });

            if (sendGridEmail.fromAddress == null)
            {
                throw new ArgumentNullException(nameof(sendGridEmail), "No sender was provided in the  email object.");
            }
            if (!EmailFormatValidation.IsValidEmail(sendGridEmail.fromAddress.email))
            {
                throw new ArgumentNullException(nameof(sendGridEmail.fromAddress.email), "Email sender has an invalid email format.");
            }

            try
            {
                Dictionary<string, object> logData = new Dictionary<string, object>()
                {
                    {"SendGrid",  sendGridEmail}
                };
                WriteToLog(LogEventLevel.Information, "ManageEmail.SendGridEmail: Email details.", logData, null);

                IList<ProductInternalSetting> productSettingList = _productInternalSettingRepository.GetProductInternalSettings(ProductId: (int)ProductEnum.UnifiedPlatform);
                if (productSettingList.Count > 0)
                {
                    bool IsSendGridEnabled = false;
                    if (productSettingList.ToList().Any(s => s.Name.Equals("IsSendGridEnabled", StringComparison.OrdinalIgnoreCase)))
                    {
                        IsSendGridEnabled = productSettingList.ToList().FirstOrDefault(s => s.Name.Equals("IsSendGridEnabled", StringComparison.OrdinalIgnoreCase)).Value.Equals("1");
                    }
                    if (IsSendGridEnabled)
                    {
                        string SendGridApiEndPoint = productSettingList.ToList().FirstOrDefault(s => s.Name.Equals("SendGridApiEndPoint", StringComparison.OrdinalIgnoreCase)).Value;
                        string SendGridSendEmailEndPoint = productSettingList.ToList().FirstOrDefault(s => s.Name.Equals("SendGridSendEmailEndPoint", StringComparison.OrdinalIgnoreCase)).Value;
                        string SendGridSendEmailUrl = string.Concat(SendGridApiEndPoint, SendGridSendEmailEndPoint);
                        HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, SendGridSendEmailUrl)
                        {
                            Content = new StringContent(JsonConvert.SerializeObject(sendGridEmail), Encoding.UTF8, "application/json")
                        };

                        HttpClient httpClient = new HttpClient();
                        Task<HttpResponseMessage> httpResponseMessage = httpClient.SendAsync(httpRequestMessage);
                        if (httpResponseMessage.Result.IsSuccessStatusCode)
                        {
                            string toEmai = string.Join(";", sendGridEmail.toAddress.ToList().Select(e => e.email).ToArray());
                            logData = new Dictionary<string, object>()
                            {
                                {"Response",  httpResponseMessage.Result}
                            };
                            WriteToLog(LogEventLevel.Information, $"ManageEmail.SendGridEmail: Email from {sendGridEmail.fromAddress.email} to {toEmai} sent successfully.", logData, null);
                            return "Email sent successfully.";
                        }
                        else
                        {
                            logData = new Dictionary<string, object>()
                            {
                                {"Response",  httpResponseMessage.Result}
                            };
                            WriteToLog(LogEventLevel.Information, "ManageEmail.SendGridEmail: An error occured when sending the email.", logData, null);
                            return "An error occured when sending the email.";
                        }
                    }
                    else
                    {
                        WriteToLog(LogEventLevel.Information, "ManageEmail.SendGridEmail: SendGrid emails is disabled.", null, null);
                        return "SendGrid emails is disabled.";
                    }
                }
                else
                {
                    WriteToLog(LogEventLevel.Information, "ManageEmail.SendGridEmail: Invalid product settings for Unified Platform.", null, null);
                    return "Invalid product settings for Unified Platform.";
                }
            }
            catch (Exception ex)
            {
                WriteToLog(LogEventLevel.Information, "ManageEmail.SendGridEmail: An error occured when sending the email.", null, ex);
                return "An error occured when sending the email.";
            }
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Used to write to the log
        /// </summary>
        /// <param name="logType">Message log type</param>
        /// <param name="message">Message</param>
        /// <param name="logData">Additional log data</param>
        /// <param name="exception">Exception</param>
        private void WriteToLog(LogEventLevel logType, string message, Dictionary<string, object> logData = null, Exception exception = null)
        {
            string correlationId = "";
            if (_defaultUserClaim != null)
            {
                correlationId = (_defaultUserClaim.CorrelationId != Guid.Empty) ? _defaultUserClaim.CorrelationId.ToString() : "";
            }

            var logger = Log.Logger;
            if (logData?.Keys != null)
            {
                logger = logger.ForContext("AdditionalInfo", JsonConvert.SerializeObject(logData, Newtonsoft.Json.Formatting.Indented), false);
            }
			logger = logger.ForContext("ProductModule", this.GetType());
            logger = logger.ForContext("CorrelationId", correlationId);
            logger.Write(logType, exception, message );
        }

        #endregion
    }
}