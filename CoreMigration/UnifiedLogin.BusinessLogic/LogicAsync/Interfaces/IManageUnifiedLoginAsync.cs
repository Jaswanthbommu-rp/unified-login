using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product;

namespace UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;

/// <summary>
/// Async-first interface for all UnifiedLogin product operations.
/// Replaces: sync <see cref="IManageUnifiedLogin"/> + blocking repository/HTTP calls.
/// </summary>
public interface IManageUnifiedLoginAsync
{
    // ── Property reads ─────────────────────────────────────────────────────

    Task<ListResponse> GetPropertiesAsync(
        long editorPersonaId, long userPersonaId, bool assignedOnly, RequestParameter dataFilter,
        CancellationToken cancellationToken = default);

    Task<ListResponse> GetEnterprisePropertiesAsync(
        long userPersonaId, string? include = null,
        CancellationToken cancellationToken = default);

    Task<ListResponse> GetCustomerPropertiesAsync(
        long userPersonaId, string? include = null,
        CancellationToken cancellationToken = default);

    Task<ListResponse> GetUPFMPropertiesAsync(
        long editorPersonaId, long userPersonaId, bool assignedOnly,
        ProductEnum product, RequestParameter dataFilter,
        CancellationToken cancellationToken = default);

    Task<ListResponse> GetUPFMPropertiesAsync(
        long userPersonaId, string? include = null,
        CancellationToken cancellationToken = default);

    Task<List<ProductInternalSetting>> GetProductInternalSettingByProductIdAsync(
        int productId, CancellationToken cancellationToken = default);

    // ── Role management ────────────────────────────────────────────────────

    Task<ListResponse> AddUpdateRoleAsync(
        long editorPersonaId, long partyId, long roleId, string roleName, string inheritRoleId,
        CancellationToken cancellationToken = default);

    Task<ListResponse> DeleteRoleAsync(
        long editorPersonaId, long roleId,
        CancellationToken cancellationToken = default);

    Task<ListResponse> SetDefaultRoleAsync(
        long editorPersonaId, long partyId, long roleId,
        CancellationToken cancellationToken = default);

    Task<ListResponse> GetRolesAsync(
        long editorPersonaId, long partyId,
        CancellationToken cancellationToken = default);

    Task<ListResponse> GetRolesWithCountAsync(
        long editorPersonaId, long partyId,
        CancellationToken cancellationToken = default);

    Task<ListResponse> GetUserRolesAsync(
        long editorPersonaId, long userPersonaId, long partyId,
        CancellationToken cancellationToken = default);

    Task<ListResponse> GetUserRolesWithRightsAsync(
        long editorPersonaId, long userPersonaId, long partyId,
        CancellationToken cancellationToken = default);

    // ── Right management ───────────────────────────────────────────────────

    Task<ListResponse> UpdateRightsToRoleAsync(
        long editorPersonaId, long roleId,
        List<string> rightsToAdd, List<string> rightsToRemove,
        CancellationToken cancellationToken = default);

    Task<ListResponse> CloneRightsToRoleAsync(
        long editorPersonaId, long roleId,
        List<string> rightsToAdd, List<string> rightsToRemove,
        CancellationToken cancellationToken = default);

    Task<ListResponse> GetRightsAsync(
        long editorPersonaId, long partyId,
        CancellationToken cancellationToken = default);

    Task<ListResponse> GetRightsWithCountAsync(
        long editorPersonaId, long partyId,
        CancellationToken cancellationToken = default);

    Task<ListResponse> GetRightsByRoleAsync(
        long editorPersonaId, long partyId, long roleId,
        CancellationToken cancellationToken = default);

    Task<ListResponse> GetListRightByRoleAsync(
        string productCode, int roleId,
        CancellationToken cancellationToken = default);

    Task<ListResponse> GetAllRightsByRoleAsync(
        long editorPersonaId, long partyId, long roleId,
        CancellationToken cancellationToken = default);

    Task<ListResponse> GetRolesByRightAsync(
        long editorPersonaId, long partyId, long rightId,
        CancellationToken cancellationToken = default);

    Task<ListResponse> UpdateRolesByRightAsync(
        long editorPersonaId, long rightId,
        List<string> rolesToAdd, List<string> rolesToRemove,
        CancellationToken cancellationToken = default);

    // ── Company ────────────────────────────────────────────────────────────

    Task<ListResponse> GetGBCompaniesAsync(
        long editorPersonaId, long partyId,
        CancellationToken cancellationToken = default);
}