using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Helper;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
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

        IEmailRepository _emailRepository;

        #endregion

        #region Constructors

        /// <summary>
        /// ManageEmail Constructor
        /// </summary>
        /// <param name="emailRepository">Email Repository</param>
        public ManageEmail(IEmailRepository emailRepository)
        {
            _emailRepository = emailRepository;
        }

        /// <summary>
        /// Create a basic instance of the ManageEmail Controller class
        /// </summary>
        /// 
        public ManageEmail()
        {
            _emailRepository = new EmailRepository();
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
                var newUserRegistrationActivity = actvityDetail.FirstOrDefault(x => x.ActivityTypeId == (int) ActivityType.NewUserRegistration);
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
                //emailBodyBuilder.Replace("{TOKEN}", newUserToken);
                //emailBodyBuilder.Replace("{LOGIN ID}", loginName);
                //emailBodyBuilder.Replace("{LANDING}", ConfigReader.GetLandingUri);
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
            string link = ConfigReader.GetLandingUri;
            if (link.EndsWith("/"))
            {
                link = link.Substring(0, link.Length - 1);
            }

            link += $"/new-user/#/validate/{newUserToken}/{loginName}";

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
                HttpWebRequest myrequest = (HttpWebRequest) WebRequest.Create(CESURL);
                myrequest.Method = "POST";

                myrequest.Headers.Add("SOAPAction", "http://realpage.com/webservices/SendEmail");
                myrequest.KeepAlive = false;
                myrequest.ContentType = "text/xml; charset=utf-8";
                StreamWriter OStream = new StreamWriter(myrequest.GetRequestStream());
                OStream.Write(soap);
                OStream.Close();
                myrequest.Timeout = 999999;

                //Create and get the Web Response from the request
                HttpWebResponse myresponse = (HttpWebResponse) myrequest.GetResponse();

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

        #endregion

    }
}