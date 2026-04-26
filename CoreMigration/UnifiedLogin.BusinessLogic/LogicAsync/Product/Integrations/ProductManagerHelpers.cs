using System.ComponentModel.DataAnnotations;
using System.Net.Mail;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.LogicAsync.Product.Integrations;

/// <summary>
/// Pure static utilities extracted from <c>ManageProductBase</c>.
/// No I/O — safe to call from any context without injection.
/// </summary>
public static class ProductManagerHelpers
{
    /// <summary>
    /// Builds a standard paged <see cref="ListResponse"/> error.
    /// Replaces: <c>ManageProductBase</c> inline <c>new ListResponse { IsError = true, ... }</c>.
    /// </summary>
    public static ListResponse ErrorResponse(string reason)
        => new() { IsError = true, ErrorReason = reason };

    /// <summary>
    /// Validates or repairs an e-mail address so it resembles a valid format.
    /// Replaces: <c>ManageProductBase.ValidateAndReturnEmailAddress</c>.
    /// </summary>
    public static string ValidateAndReturnEmailAddress(string emailAddress)
    {
        if (new EmailAddressAttribute().IsValid(emailAddress))
            return emailAddress;

        try
        {
            var ma = new MailAddress(emailAddress);
            return !ma.Host.Contains('.')
                ? ValidateAndReturnEmailAddress(emailAddress + ".com")
                : emailAddress;
        }
        catch
        {
            return !emailAddress.Contains('@')
                ? ValidateAndReturnEmailAddress(emailAddress + "@bogusemail.com")
                : emailAddress;
        }
    }
}