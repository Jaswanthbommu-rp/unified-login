using UnifiedLogin.BusinessLogic.Logic.ProductIntegration.Model;
using UnifiedLogin.SharedObjects.BlackBook;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;

/// <summary>
/// Native-async data-collector contract.
/// Replaces <see cref="UnifiedLogin.BusinessLogic.Logic.ProductIntegration.Helpers.IDataCollector"/>.
/// <para>
/// <list type="bullet">
///   <item><c>DefaultUserClaim userClaims</c> removed from
///     <c>GetProductCompanyMapAsync</c> — caller context resolved internally
///     via <c>IUserClaimsAccessor</c>.</item>
///   <item>All <c>void</c> write operations return <c>Task</c> so failures can
///     be observed and awaited without blocking.</item>
///   <item>Read operations that may return nothing are annotated with
///     nullable return types (<c>T?</c>).</item>
///   <item><c>CancellationToken ct = default</c> appended to every method.</item>
/// </list>
/// </para>
/// </summary>
public interface IDataCollectorAsync
{
    // ── GreenBook sync ────────────────────────────────────────────────────────

    /// <summary>
    /// Creates a product-user record in GreenBook for the given
    /// <paramref name="subjectPersonaId"/>.
    /// <para>
    /// <c>dynamic userResult</c> carries the heterogeneous product API response;
    /// the concrete implementation casts it per-product. A future phase may
    /// replace <c>dynamic</c> with a typed discriminated union.
    /// </para>
    /// </summary>
    Task CreateProductUserInGreenBookAsync(
        long subjectPersonaId,
        dynamic userResult,
        int productId,
        IntegrationProductUser productUser,
        CancellationToken ct = default);

    /// <summary>Updates an existing product-user record in GreenBook.</summary>
    Task UpdateProductUserInGreenBookAsync(
        long subjectPersonaId,
        dynamic userResult,
        int productId,
        IntegrationProductUser productUser,
        CancellationToken ct = default);

    // ── Product / company map lookups ─────────────────────────────────────────

    /// <summary>
    /// Returns the BlueBook product map for <paramref name="productId"/>.
    /// Result is typically cached by the implementation.
    /// </summary>
    Task<GbProductMap?> GetBlueBookProductMapAsync(
        int productId,
        CancellationToken ct = default);

    /// <summary>
    /// Returns the customer→company mapping for the given BlueBook product code.
    /// <para>
    /// <c>DefaultUserClaim</c> was removed — the implementation resolves caller
    /// context from the ambient <c>IUserClaimsAccessor</c>.
    /// </para>
    /// </summary>
    Task<CustomerCompanyMap?> GetProductCompanyMapAsync(
        string blueBookProductCode,
        int booksMasterId,
        string domain,
        CancellationToken ct = default);

    // ── User lookups ──────────────────────────────────────────────────────────

    /// <summary>
    /// Returns the <see cref="UserDetails"/> record for <paramref name="personaId"/>
    /// in the context of <paramref name="productId"/>. Returns <c>null</c> when not found.
    /// </summary>
    Task<UserDetails?> GetUserDetailsByPersonaAsync(
        long personaId,
        int productId,
        CancellationToken ct = default);

    /// <summary>
    /// Returns the Azure AD user details for <paramref name="userId"/>.
    /// Returns <c>null</c> when the user has no AD record.
    /// </summary>
    Task<AdUserDetail?> GetAzureUserDetailsAsync(
        long userId,
        CancellationToken ct = default);

    // ── Product settings & SAML attributes ───────────────────────────────────

    /// <summary>
    /// Updates a product-setting value of type <typeparamref name="T"/> for
    /// <paramref name="subjectPersonaId"/>.
    /// </summary>
    Task UpdateProductSettingProductStatusAsync<T>(
        long subjectPersonaId,
        string settingType,
        int productId,
        T value,
        CancellationToken ct = default);

    /// <summary>Updates an existing SAML attribute value for the given persona + product.</summary>
    Task UpdateSamlUserAttributeAsync(
        long personaId,
        int productId,
        SamlAttributeEnum attributeType,
        string newValue,
        CancellationToken ct = default);

    /// <summary>Creates a new SAML attribute entry for the given persona + product.</summary>
    Task CreateSamlUserAttributeAsync(
        long subjectPersonaId,
        int productId,
        SamlAttributeEnum samlAttributeEnum,
        string value,
        CancellationToken ct = default);

    // ── Employee / AD group mapping ───────────────────────────────────────────

    /// <summary>
    /// Adds or updates the Azure AD group mapping for the given persona + product.
    /// </summary>
    Task AddUpdateEmployeeProductADGroupMappingAsync(
        long personaId,
        int productId,
        int adGroupId,
        CancellationToken ct = default);

    /// <summary>
    /// Returns the list of Azure AD group mappings for the given persona + product.
    /// Returns an empty list when no mappings exist.
    /// </summary>
    Task<IList<EmployeeProductMapping>> GetEmployeeProductADGroupMappingAsync(
        long personaId,
        int productId,
        CancellationToken ct = default);
}
