using System.Security.Claims;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.ResponseObject;

namespace UnifiedLogin.LandingAPIEnterprise.Services.Role
{
    /// <summary>
    /// Handles client credential authentication for enterprise API endpoints.
    /// </summary>
    public interface IClientCredentialAuthenticator
    {
        /// <summary>
        /// Authenticates using client credentials and resolves the effective user claims.
        /// </summary>
        /// <param name="user">The current claims principal from the HTTP context</param>
        /// <param name="currentClaims">The current user claims</param>
        /// <param name="upfmId">Optional UPFM organization ID for client credential authentication</param>
        /// <returns>Authentication result with effective claims or error</returns>
        ClientCredentialAuthResult Authenticate(ClaimsPrincipal user, DefaultUserClaim currentClaims, Guid? upfmId);
    }

    /// <summary>
    /// Result of client credential authentication attempt.
    /// </summary>
    public sealed class ClientCredentialAuthResult
    {
        public static ClientCredentialAuthResult Success(DefaultUserClaim effectiveClaims) =>
            new ClientCredentialAuthResult(effectiveClaims, null);

        public static ClientCredentialAuthResult Error(ErrorResponse error) =>
            new ClientCredentialAuthResult(null, error);

        private ClientCredentialAuthResult(DefaultUserClaim effectiveClaims, ErrorResponse error)
        {
            EffectiveClaims = effectiveClaims;
            ErrorResponse = error;
        }

        public DefaultUserClaim EffectiveClaims { get; }
        /// <summary>
        /// Gets the error response if authentication failed.
        /// </summary>
        public ErrorResponse ErrorResponse { get; }
        public bool IsError => ErrorResponse != null;
    }
}
