using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;

/// <summary>
/// Async-first interface for email operations.
/// Replaces: sync <see cref="UnifiedLogin.BusinessLogic.Logic.Interfaces.IManageEmail"/>.
/// </summary>
public interface IManageEmailAsync
{
    // ── Template ──────────────────────────────────────────────────────────

    /// <summary>Returns the email template for the given audience/purpose type combination.</summary>
    Task<CommunicationEmail> GetEmailTemplateAsync(
        int communicationEventAudienceTypeId,
        int communicationEventPurposeTypeId,
        CancellationToken cancellationToken = default);

    // ── Build ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Builds a welcome email body from a template, replacing all placeholders.
    /// Fetches the activity token expiry from the credential repo asynchronously.
    /// </summary>
    Task<Email> CreateWelcomeEmailAsync(
        string loginName, string firstName, string companyName, long orgPartyId,
        CommunicationEmail emailTemplate, string newUserToken,
        string senderEmailAddress = "", string notificationEmail = "",
        CancellationToken cancellationToken = default);

    /// <summary>Pure logic — builds the new-user activation link. No I/O.</summary>
    string BuildNewUserLink(string newUserToken, string loginName);

    // ── Send ──────────────────────────────────────────────────────────────

    /// <summary>Sends an email via the CES SOAP endpoint.</summary>
    Task<string> SendEmailAsync(Email email, CancellationToken cancellationToken = default);

    /// <summary>Sends an email via the SendGrid HTTP API.</summary>
    Task<string> SendGridEmailAsync(
        ISendGridEmail sendGridEmail,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends an email via the Unified Email HTTP API.
    /// Replaces: sync <c>ManageEmail.SendEmailAsync(EmailModel)</c> which used blocking <c>.Result</c> calls.
    /// </summary>
    Task<bool> SendEmailModelAsync(
        EmailModel emailModel,
        CancellationToken cancellationToken = default);
}
