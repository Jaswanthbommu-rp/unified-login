using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.BlackBook;
using UnifiedLogin.SharedObjects.EnterpriseRole;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product.ResidentPortal;

namespace UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;

/// <summary>
/// True async interface for product-panel orchestration.
/// <para>
/// Replaces <c>IManageProductPanel</c> which accepted a <c>DefaultUserClaim</c>-bound
/// constructor. Context is now resolved per-call via injected async services.
/// </para>
/// <para>
/// Methods that delegate to <see cref="IIntegrationTypeFactory"/> (a synchronous factory
/// with no async counterpart) wrap those calls in <c>Task.Run</c> to avoid blocking
/// ASP.NET request threads.
/// </para>
/// </summary>
public interface IManageProductPanelAsync
{
    // ── Properties ────────────────────────────────────────────────────────────

    /// <summary>
    /// Returns the product-specific property list for <paramref name="userPersonaId"/>,
    /// enriched with <c>usePrimaryProperties</c> and other flags in
    /// <c>ListResponse.Additional</c>.
    /// </summary>
    Task<ListResponse> GetProductPropertiesAsync(
        long editorPersonaId, long userPersonaId, int productId,
        RequestParameter datafilter,
        CancellationToken ct = default);

    /// <summary>
    /// Returns the properties that belong to <paramref name="propertyGroupId"/>
    /// for the given product.
    /// </summary>
    Task<ListResponse> GetProductGroupPropertiesAsync(
        long editorPersonaId, long userPersonaId, int productId,
        string propertyGroupId, RequestParameter datafilter,
        CancellationToken ct = default);

    /// <summary>
    /// Returns the product-specific property-group list (e.g. PMC / location groups).
    /// </summary>
    Task<ListResponse> GetProductPropertyGroupsAsync(
        long editorPersonaId, long userPersonaId, int productId,
        RequestParameter datafilter,
        bool assignedOnly = false, string userLoginName = "",
        CancellationToken ct = default);

    /// <summary>
    /// Returns location groups for products that support them (FinancialSuite,
    /// UtilityManagement). UtilityManagement (RUM) is pending a full async refactor
    /// of <c>IManageProductRumAsync</c> and currently returns an empty
    /// <see cref="ListResponse"/>.
    /// </summary>
    Task<ListResponse> GetProductLocationGroupsAsync(
        long editorPersonaId, long userPersonaId, int productId,
        RequestParameter datafilter,
        bool assignedOnly = false, string userLoginName = "",
        CancellationToken ct = default);

    // ── Roles ─────────────────────────────────────────────────────────────────

    /// <summary>
    /// Returns the product-specific role list for the given user and access type.
    /// </summary>
    Task<ListResponse> GetProductRolesAsync(
        long editorPersonaId, long userPersonaId, long partyId,
        int productId, RequestParameter datafilter, AccessType? accessType,
        CancellationToken ct = default);

    /// <summary>
    /// Returns assigned roles across all products the user is provisioned on,
    /// together with a list of products that could not be resolved.
    /// </summary>
    Task<RoleTemplateProductRoleMapping> GetUserProductRolesAsync(
        long editorPersonaId, long userPersonaId, long partyId,
        CancellationToken ct = default);

    /// <summary>
    /// Returns the product-specific user-group list.
    /// </summary>
    Task<ListResponse> GetProductUserGroupsAsync(
        long editorPersonaId, long userPersonaId, long partyId,
        int productId, RequestParameter datafilter,
        CancellationToken ct = default);

    // ── Rights ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Returns rights for a specific role identified by its numeric <paramref name="roleId"/>.
    /// </summary>
    Task<ListResponse> GetProductRightsForRoleAsync(
        long editorPersonaId, int roleId, long partyId,
        int productId, RequestParameter datafilter,
        bool assignedToRoleOnly = false,
        CancellationToken ct = default);

    /// <summary>
    /// Returns rights for a specific role identified by its string <paramref name="roleId"/>.
    /// Routes through <c>StandardV1</c> or numeric-parse path based on integration type.
    /// </summary>
    Task<ListResponse> GetProductRightsForRoleAsync(
        long editorPersonaId, string roleId, long partyId,
        int productId, RequestParameter datafilter,
        bool assignedToRoleOnly = false,
        CancellationToken ct = default);

    /// <summary>
    /// Returns product rights. Currently only UtilityManagement is handled by the
    /// legacy sync layer; this returns an empty <see cref="ListResponse"/> until
    /// <c>IManageProductRumAsync</c> removes the <c>DefaultUserClaim</c> parameter.
    /// </summary>
    Task<ListResponse> GetProductRightsAsync(
        long editorPersonaId, long userPersonaId, long partyId,
        int productId, RequestParameter datafilter,
        CancellationToken ct = default);

    // ── Organizations ─────────────────────────────────────────────────────────

    /// <summary>
    /// Returns the organization list for the given product, role, and organization type.
    /// </summary>
    Task<ListResponse> GetProductOrganizationsAsync(
        long editorPersonaId, long userPersonaId, int productId,
        string organizationRoleId, string organizationType,
        CancellationToken ct = default);

    // ── Property translation ──────────────────────────────────────────────────

    /// <summary>
    /// Compares product properties against primary (UPFM) properties and returns
    /// the translated result. Returns <paramref name="productResult"/> unchanged
    /// when it contains no records.
    /// </summary>
    Task<ListResponse> CompareProductAndPrimaryPropertiesAsync(
        UPFMProperty upfmProperty, int productId, ListResponse productResult,
        CancellationToken ct = default);

    /// <summary>
    /// Translates product-specific property instance IDs to UPFM instance source IDs.
    /// </summary>
    Task<UPFMProperty> TranslateProductPropertiesAsync(
        UPFMProperty upfmProperty, int productId,
        CancellationToken ct = default);

    // ── Persona product properties ────────────────────────────────────────────

    /// <summary>
    /// Returns the persona's primary product-property assignments.
    /// </summary>
    Task<List<PersonaProductProperty>> GetPersonaProductPrimaryPropertiesAsync(
        long userPersonaId,
        CancellationToken ct = default);
}
