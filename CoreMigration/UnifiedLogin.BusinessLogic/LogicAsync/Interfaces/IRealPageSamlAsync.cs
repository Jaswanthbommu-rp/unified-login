using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Saml;

namespace UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;

/// <summary>
/// Native-async SAML orchestrator interface.
/// Replaces <c>RealPageSAML</c> (sync) — <c>DefaultUserClaim</c> removed from all signatures.
/// <para>
/// <list type="bullet">
///   <item>Thread-safe: no mutable instance fields.</item>
///   <item>Product SAML settings cached via <c>IMemoryCache</c> (10-minute TTL).</item>
///   <item><c>out</c> params replaced with tuple/record returns.</item>
///   <item><c>System.Web.Http.HttpResponseException</c> removed — .NET 10 compatible.</item>
/// </list>
/// </para>
/// </summary>
public interface IRealPageSamlAsync
{
    /// <summary>
    /// Returns existing SAML attributes for the given persona/product, or triggers a
    /// batch user-creation flow if none exist and <c>IsUserCreationOnTileClick</c> is enabled.
    /// Caller context (org GUID, impersonator) is resolved from the ambient
    /// <c>IUserClaimsAccessor</c> injected into the implementation.
    /// Replaces: <c>RealPageSAML.createUserBatchIfRequired</c>.
    /// </summary>
    /// <param name="targetPersonaId">The persona being provisioned into the product.</param>
    /// <param name="productId">The product ID.</param>
    Task<IList<SamlAttributes>> CreateUserBatchIfRequiredAsync(
        long targetPersonaId, int productId,
        CancellationToken ct = default);

    /// <summary>
    /// Orchestrates the full SAML login flow for a product.
    /// Resolves personas, SAML settings, PMC/Doc Management URL overrides and
    /// builds the signed SAML assertion.
    /// Caller identity and rights are resolved from the ambient <c>IUserClaimsAccessor</c>.
    /// Replaces: <c>RealPageSAML.GetProductDetailsSAML</c>.
    /// </summary>
    /// <param name="targetPersonaId">Persona ID of the user being logged into the product (0 = use caller's own persona).</param>
    Task<SamlProductLoginResponse> GetProductDetailsSamlAsync(
        string unifiedLoginUri, int productId,
        long targetPersonaId,
        string userToken,
        string relayStateSamlAttribute = "", string fallBackUrl = "",
        bool isProductReport = false, string reportParams = "",
        CancellationToken ct = default);

    /// <summary>
    /// Builds the signed SAML assertion and Base64-encodes it.
    /// Appends enterprise/product attributes and resolves the subject/relay values.
    /// <c>EnterpriseUserId</c> and <c>EnterpriseLogin</c> attributes are sourced from
    /// the ambient <c>IUserClaimsAccessor</c> (<c>UserRealPageGuid</c> / <c>LoginName</c>).
    /// Replaces: <c>RealPageSAML.GetSAMLDetails</c>.
    /// </summary>
    Task<SamlGeneratedResponse> GetSamlDetailsAsync(
        string unifiedLoginUri, int productId, string userToken,
        string signingCertThumbprint, string issuer,
        string samlSubjectAttributeName, string samlRelayAttributeName,
        string destination, IList<SamlAttributes> samlList,
        string reportParams = "", bool isProductReport = false,
        CancellationToken ct = default);

    /// <summary>
    /// Resolves product details flags and the filtered persona product list.
    /// Replaces: <c>RealPageSAML.ProductDetails</c> (<c>out</c> params → record return).
    /// </summary>
    Task<SamlProductDetailsResult> GetProductDetailsAsync(
        int productId, long personaId,
        CancellationToken ct = default);
}

// ── Response / result types ───────────────────────────────────────────────────

/// <summary>Replaces <c>RealPageSAML.SAMLResponse</c>.</summary>
public sealed class SamlGeneratedResponse
{
    public string RelayState       { get; set; } = "";
    public string Destination      { get; set; } = "";
    public string SamlBase64Encoded { get; set; } = "";
}

/// <summary>Replaces <c>RealPageSAML.ProductLoginResponse</c>.</summary>
public sealed class SamlProductLoginResponse
{
    public string?               RedirectUrl   { get; set; }
    public SamlGeneratedResponse? SamlResponse  { get; set; }
    public string?               ErrorMessage  { get; set; }
    public bool                  IsSaml        { get; set; }
    public bool                  IsRedirect    { get; set; }
    public string?               AccessToken   { get; set; }
}

/// <summary>
/// Replaces the four <c>out</c> parameters of <c>RealPageSAML.ProductDetails</c>.
/// <see cref="HasError"/> is <c>true</c> when the repository call failed and the
/// caller should return an error response immediately.
/// </summary>
public sealed record SamlProductDetailsResult(
    bool GetOneSitePmcUrl,
    bool GetDocMgtDomain,
    bool GetMarketingCenterUrl,
    IList<PersonaProductUserDetails> ProductList,
    bool HasError = false);
