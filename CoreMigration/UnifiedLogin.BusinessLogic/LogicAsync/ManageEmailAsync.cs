using System.Net.Http.Headers;
using System.Text;
using System.Web;
using System.Xml;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using UnifiedLogin.BusinessLogic.Logic.Enterprise.Helpers;
using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Helper;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.LogicAsync;

/// <summary>
/// Async-first implementation of <see cref="IManageEmailAsync"/>.
/// Replaces the stepping-stone that only had <see cref="SendGridEmailAsync"/>.
/// <list type="bullet">
/// <item><c>new HttpClient()</c> replaced by <see cref="IHttpClientFactory"/>.</item>
/// <item><c>.Result</c> blocking HTTP calls replaced by <c>await</c>.</item>
/// <item>Serilog replaced by <see cref="ILogger{TCategoryName}"/>.</item>
/// <item><c>new CredentialRepository()</c> replaced by <see cref="ICredentialRepositoryAsync"/>.</item>
/// </list>
/// </summary>
public sealed class ManageEmailAsync : IManageEmailAsync
{
    #region Fields

    private readonly IEmailRepositoryAsync                   _emailRepository;
    private readonly IProductInternalSettingRepositoryAsync  _productInternalSettingRepository;
    private readonly ICredentialRepositoryAsync              _credentialRepository;
    private readonly ITokenHelperAsync                            _tokenHelper; // sync-only — no async version yet
    private readonly IHttpClientFactory                      _httpClientFactory;
    private readonly IUserClaimsAccessor                     _userClaimsAccessor;
    private readonly ILogger<ManageEmailAsync>               _logger;

    #endregion

    #region Constructor

    public ManageEmailAsync(
        IEmailRepositoryAsync                  emailRepository,
        IProductInternalSettingRepositoryAsync productInternalSettingRepository,
        ICredentialRepositoryAsync             credentialRepository,
        ITokenHelperAsync                      tokenHelper,
        IHttpClientFactory                     httpClientFactory,
        IUserClaimsAccessor                    userClaimsAccessor,
        ILogger<ManageEmailAsync>              logger)
    {
        _emailRepository                  = emailRepository                  ?? throw new ArgumentNullException(nameof(emailRepository));
        _productInternalSettingRepository = productInternalSettingRepository ?? throw new ArgumentNullException(nameof(productInternalSettingRepository));
        _credentialRepository             = credentialRepository             ?? throw new ArgumentNullException(nameof(credentialRepository));
        _tokenHelper                      = tokenHelper                      ?? throw new ArgumentNullException(nameof(tokenHelper));
        _httpClientFactory                = httpClientFactory                ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _userClaimsAccessor               = userClaimsAccessor               ?? throw new ArgumentNullException(nameof(userClaimsAccessor));
        _logger                           = logger                           ?? throw new ArgumentNullException(nameof(logger));
    }

    #endregion

    #region Template

    /// <inheritdoc/>
    public async Task<CommunicationEmail> GetEmailTemplateAsync(
        int communicationEventAudienceTypeId,
        int communicationEventPurposeTypeId,
        CancellationToken cancellationToken = default)
    {
        if (communicationEventAudienceTypeId == 0)
            throw new ArgumentException("Audience Type Id is required.", nameof(communicationEventAudienceTypeId));
        if (communicationEventPurposeTypeId == 0)
            throw new ArgumentException("Purpose Type Id is required.", nameof(communicationEventPurposeTypeId));

        return await _emailRepository.GetEmailTemplateAsync(
            communicationEventAudienceTypeId, communicationEventPurposeTypeId, cancellationToken);
    }

    #endregion

    #region Build

    /// <inheritdoc/>
    public async Task<Email> CreateWelcomeEmailAsync(
        string loginName, string firstName, string companyName, long orgPartyId,
        CommunicationEmail emailTemplate, string newUserToken,
        string senderEmailAddress = "", string notificationEmail = "",
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(firstName))        throw new ArgumentException("FirstName is missing.",              nameof(firstName));
        if (string.IsNullOrEmpty(companyName))      throw new ArgumentException("Organization name is missing.",      nameof(companyName));
        if (orgPartyId == 0)                        throw new ArgumentException("Organization id is missing.",        nameof(orgPartyId));
        ArgumentNullException.ThrowIfNull(emailTemplate,   nameof(emailTemplate));
        if (string.IsNullOrEmpty(notificationEmail))        throw new ArgumentException("Notification Email is missing.", nameof(notificationEmail));
        if (!EmailFormatValidation.IsValidEmail(senderEmailAddress))    throw new ArgumentException("Email Sender has an invalid email format.",      nameof(senderEmailAddress));
        if (!EmailFormatValidation.IsValidEmail(notificationEmail))     throw new ArgumentException("Notification Email has an invalid email format.", nameof(notificationEmail));

        // ── Fetch token expiry async (removes new CredentialRepository()) ─
        int expiryDays = 0;
        var activities = await _credentialRepository.GetActivitiesAsync(orgPartyId, cancellationToken);
        var newUserActivity = activities?.FirstOrDefault(x => x.ActivityTypeId == (int)ActivityType.NewUserRegistration);
        if (newUserActivity is not null && newUserActivity.ActivityTokenExpirationMinutes > 0)
            expiryDays = newUserActivity.ActivityTokenExpirationMinutes / 1440;

        var cesEmail = new Email
        {
            EmailFrom    = string.IsNullOrEmpty(senderEmailAddress) ? new Email().EmailFrom : senderEmailAddress,
            EmailTo      = notificationEmail,
            EmailSubject = emailTemplate.Subject,
            EmailBody    = new StringBuilder(emailTemplate.Body)
                .Replace("{COMPANY NAME}", companyName)
                .Replace("{FIRST NAME}",   firstName)
                .Replace("{LINK}",         BuildNewUserLink(newUserToken, loginName))
                .Replace("{IMAGES}",       ConfigReader.GetImagesUri)
                .Replace("{UNIFIED}",      ConfigReader.GetDocumentUri)
                .Replace("{EXPIRYDAYS}",   expiryDays.ToString())
                .ToString()
        };

        return cesEmail;
    }

    /// <inheritdoc/>
    public string BuildNewUserLink(string newUserToken, string loginName)
    {
        string link = ConfigReader.GetIssuerUri.TrimEnd('/');
        string encodedToken = Convert.ToBase64String(Encoding.UTF8.GetBytes(newUserToken));
        return $"{link}/PasswordRecovery/RedirectFromInviteEmail?newUserToken={encodedToken}&loginName={HttpUtility.UrlEncode(loginName)}";
    }

    #endregion

    #region Send — CES SOAP

    /// <inheritdoc/>
    public async Task<string> SendEmailAsync(Email email, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(email);

        try
        {
            var xmlDoc = BuildCesSoapXml(email);
            string soap = WrapInSoapEnvelope(xmlDoc);

            using var httpClient = _httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Add("SOAPAction", "http://realpage.com/webservices/SendEmail");

            using var request = new HttpRequestMessage(HttpMethod.Post, ConfigReader.GetCESURL)
            {
                Content = new StringContent(soap, Encoding.UTF8, "text/xml")
            };

            using var response = await httpClient.SendAsync(request, cancellationToken);
            string responseBody  = await response.Content.ReadAsStringAsync(cancellationToken);

            var xmlResponse = new XmlDocument();
            xmlResponse.LoadXml(responseBody);
            var nsMgr = new XmlNamespaceManager(xmlResponse.NameTable);
            nsMgr.AddNamespace("ns", "http://realpage.com/webservices");

            var resultXml = new XmlDocument();
            resultXml.LoadXml(HttpUtility.HtmlDecode(
                xmlResponse.SelectSingleNode("//ns:SendEmailResult", nsMgr)?.InnerXml ?? string.Empty));

            var statusNode = resultXml.SelectSingleNode("//Output/Result");
            return statusNode?.Attributes?["Status"]?.Value == "1"
                ? "Email sent successfully."
                : "An error occurred when sending the email.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CES email send failed for {EmailTo}", email.EmailTo);
            return "An error occurred when sending the email.";
        }
    }

    #endregion

    #region Send — SendGrid

    /// <inheritdoc/>
    public async Task<string> SendGridEmailAsync(
        ISendGridEmail sendGridEmail,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(sendGridEmail);

        if (sendGridEmail.toAddress is null || sendGridEmail.toAddress.Count == 0)
            throw new ArgumentException("No recipient was provided.", nameof(sendGridEmail));
        foreach (var recipient in sendGridEmail.toAddress)
        {
            if (!EmailFormatValidation.IsValidEmail(recipient.email))
                throw new ArgumentException($"Invalid recipient email: {recipient.email}", nameof(sendGridEmail));
        }
        if (sendGridEmail.fromAddress is null)
            throw new ArgumentException("No sender was provided.", nameof(sendGridEmail));
        if (!EmailFormatValidation.IsValidEmail(sendGridEmail.fromAddress.email))
            throw new ArgumentException("Invalid sender email.", nameof(sendGridEmail));

        try
        {
            var settings = await _productInternalSettingRepository
                .GetProductInternalSettingsAsync((int)ProductEnum.UnifiedPlatform, cancellationToken);

            if (settings is null || settings.Count == 0)
                return "Invalid product settings for Unified Platform.";

            bool isSendGridEnabled = settings
                .FirstOrDefault(s => s.Name.Equals("IsSendGridEnabled", StringComparison.OrdinalIgnoreCase))
                ?.Value?.Equals("1") ?? false;

            if (!isSendGridEnabled)
                return "SendGrid emails is disabled.";

            string apiEndPoint  = settings.First(s => s.Name.Equals("SendGridApiEndPoint",      StringComparison.OrdinalIgnoreCase)).Value;
            string sendEndPoint = settings.First(s => s.Name.Equals("SendGridSendEmailEndPoint", StringComparison.OrdinalIgnoreCase)).Value;

            using var httpClient = _httpClientFactory.CreateClient();
            using var request    = new HttpRequestMessage(HttpMethod.Post, apiEndPoint + sendEndPoint)
            {
                Content = new StringContent(JsonConvert.SerializeObject(sendGridEmail), Encoding.UTF8, "application/json")
            };

            using var response = await httpClient.SendAsync(request, cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("SendGrid email sent to {To}", string.Join(";", sendGridEmail.toAddress.Select(e => e.email)));
                return "Email sent successfully.";
            }

            _logger.LogWarning("SendGrid email failed. StatusCode={Status}", response.StatusCode);
            return "An error occured when sending the email.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SendGrid email failed");
            return "An error occured when sending the email.";
        }
    }

    #endregion

    #region Send — Unified Email API

    /// <inheritdoc/>
    public async Task<bool> SendEmailModelAsync(
        EmailModel emailModel,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(emailModel);

        try
        {
            var settings = await _productInternalSettingRepository
                .GetProductInternalSettingsAsync((int)ProductEnum.UnifiedPlatform, cancellationToken);

            string baseAddress   = settings.First(s => s.Name.Equals("UnifiedEmailBaseAddress", StringComparison.OrdinalIgnoreCase)).Value;
            string endPoint      = settings.First(s => s.Name.Equals("UnifiedEmailEndPoint",     StringComparison.OrdinalIgnoreCase)).Value;
            bool useDefaultTmpl  = settings.FirstOrDefault(s => s.Name.Equals("UseDefaultTemplate", StringComparison.OrdinalIgnoreCase))?.Value == "1";
            string emailUrl      = $"{baseAddress}{endPoint}?useDefaultTemplate={useDefaultTmpl}";

            // ITokenHelper is sync-only — no async version available yet
            string token = await _tokenHelper.GetUnifiedLoginServerTokenAsync("emailsapi", cancellationToken);

            using var httpClient = _httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            using var request = new HttpRequestMessage(HttpMethod.Post, emailUrl)
            {
                Content = new StringContent(
                    JsonConvert.SerializeObject(emailModel), Encoding.UTF8, "application/json")
            };

            using var response = await httpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                string error = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("SendEmailModel failed. Endpoint={EndPoint} Response={Response}", endPoint, error);
                return false;
            }

            string responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
            return JsonConvert.DeserializeObject<bool>(responseBody);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SendEmailModel exception");
            return false;
        }
    }

    #endregion

    #region Private Helpers

    private static XmlDocument BuildCesSoapXml(Email email)
    {
        var doc = new XmlDocument();
        doc.LoadXml("<Input xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns='EmailMessage.xsd'><Parameters><EmailMessages><EmailMessage><ClientUniqueID></ClientUniqueID><ClientProductName>OneSite-Letters And Notices</ClientProductName><EntityID></EntityID><SiteID></SiteID><To></To><From></From><Subject></Subject><Body></Body><Cc /><Bcc /><ReplyTo></ReplyTo><AttachmentPath></AttachmentPath></EmailMessage></EmailMessages></Parameters></Input>");

        var ns = new XmlNamespaceManager(doc.NameTable);
        ns.AddNamespace("inp", "EmailMessage.xsd");

        doc.SelectSingleNode("//inp:ClientUniqueID",  ns)!.InnerText = email.ClientUniqueID.ToString();
        doc.SelectSingleNode("//inp:EntityID",        ns)!.InnerText = email.EntityID;
        doc.SelectSingleNode("//inp:SiteID",          ns)!.InnerText = email.SiteID;
        doc.SelectSingleNode("//inp:To",              ns)!.InnerText = email.EmailTo;
        doc.SelectSingleNode("//inp:From",            ns)!.InnerText = email.EmailFrom;
        doc.SelectSingleNode("//inp:Subject",         ns)!.InnerText = email.EmailSubject;
        doc.SelectSingleNode("//inp:ReplyTo",         ns)!.InnerText = email.EmailReplyTo;
        doc.SelectSingleNode("//inp:AttachmentPath",  ns)!.InnerText = email.EmailAttachment;
        doc.SelectSingleNode("//inp:Body",            ns)!.InnerText = email.EmailBody;

        return doc;
    }

    private static string WrapInSoapEnvelope(XmlDocument payload) =>
        $"<?xml version='1.0' encoding='utf-8'?>" +
        $"<soap:Envelope xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:soap='http://schemas.xmlsoap.org/soap/envelope/'>" +
        $"<soap:Body><SendEmail xmlns='http://realpage.com/webservices'><InputXML>" +
        $"<![CDATA[{payload.InnerXml}]]>" +
        $"</InputXML></SendEmail></soap:Body></soap:Envelope>";

    #endregion
}
