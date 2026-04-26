using UnifiedLogin.SharedObjects.Audit.Common;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product.UPFMProduct;

namespace UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;

/// <summary>
/// True-async interface for UPFM product user management.
/// Replaces <c>IManageUPFMProductsIntegration</c> (stepping-stone) — no <c>DefaultUserClaim</c>,
/// no <c>out</c> parameters, no blocking calls.
/// </summary>
public interface IManageUPFMProductsIntegrationAsync
{
    /// <summary>Returns all roles available for the given party/product, marking any already assigned to the user.</summary>
    Task<ListResponse> GetRolesAsync(
        long editorPersonaId,
        long userPersonaId,
        long partyId,
        CancellationToken cancellationToken = default);

    /// <summary>Returns all rights for a role within the party/product context.</summary>
    Task<ListResponse> GetRightsByRoleAsync(
        long editorPersonaId,
        long partyId,
        long roleId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns enterprise UPFM property instances assigned to the given user, translated
    /// via BlueBook into the target product's property IDs.
    /// </summary>
    Task<ListResponse> GetEnterpriseUPFMPropertiesAsync(
        long editorPersonaId,
        long userPersonaId,
        int product,
        string productCode,
        string? include = null,
        bool isMultiCompany = false,
        string? multiCompanyRealPageId = null,
        CancellationToken cancellationToken = default);

    /// <summary>Returns all UPFM property instances for the editor's company, marking those assigned to the user.</summary>
    Task<ListResponse> GetUPFMPropertiesAsync(
        long editorPersonaId,
        long userPersonaId,
        bool assignedOnly,
        RequestParameter datafilter,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates or updates a user's role and property assignments for the UPFM product.
    /// Returns the result message and audit parameters as a tuple (replaces the <c>out</c> parameter).
    /// </summary>
    Task<(string result, List<AdditionalParameters> auditParams)> ManageUPFMProductUserAsync(
        long editorPersonaId,
        long userPersonaId,
        UPFMProductPropertyRole userAssignProductPropertyRole,
        bool isEmpAccess = false,
        CancellationToken cancellationToken = default);

    /// <summary>Removes the user's roles and properties, setting product status to Deleted.</summary>
    Task<string> UnassignUserAsync(
        long editorPersonaId,
        long userPersonaId,
        UPFMProductPropertyRole userAssignProductPropertyRole,
        CancellationToken cancellationToken = default);

    /// <summary>Returns the product company instance source ID from BlueBook for the given company and product.</summary>
    Task<string> GetProductCompanyInstanceIdAsync(
        Guid organizationRealPageId,
        long booksCustmerMasterId,
        string blueBookProductName,
        string domain,
        string includeExtra = "",
        bool useTranslate = true,
        CancellationToken cancellationToken = default);

    /// <summary>Returns UPFM properties across all companies the logged-in user belongs to.</summary>
    Task<List<UserCompaniesProperties>?> GetUPFMMultiCompanyPropertiesAsync(
        long editorPersonaId,
        string productCode,
        CancellationToken cancellationToken = default);
}
