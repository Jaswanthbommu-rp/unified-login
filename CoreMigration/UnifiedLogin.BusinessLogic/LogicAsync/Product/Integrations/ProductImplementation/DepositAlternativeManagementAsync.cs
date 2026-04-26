using Microsoft.Extensions.Logging;
using UnifiedLogin.BusinessLogic.CacheHelper;
using UnifiedLogin.BusinessLogic.Logic.ProductIntegration.Model;
using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Cache;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.LogicAsync.Product.Integrations.ProductImplementation;

/// <summary>
/// Native-async concrete implementation for the Deposit Alternative Management product integration.
/// Replaces <c>DepositAlternativeManagement</c> (sync).
/// <para>
/// <b>Overrides vs base class:</b>
/// <list type="bullet">
///   <item><see cref="GetProductRolesAsync"/> — enriches base response with
///     <c>CanReceiveMonthlyReport</c> flag from the product user record.</item>
///   <item><see cref="MergeUserPropertyGroups"/> — filters by the user's role type
///     (only groups whose <c>GroupType</c> matches the user's first role are marked assigned).</item>
///   <item><see cref="UpdateSamlUserAttributeAsync"/> — syncs <c>productUsername</c>
///     SAML attribute with the user's email when they differ.</item>
///   <item><see cref="GetProductPropertyGroupsAsync"/> — delegate override to ensure the
///     standard endpoint key <c>GetPropertyGroupsEndpoint</c> is used (not <c>GetPropertyGroupEndpoint</c>).</item>
///   <item><see cref="UnassignUserAsync"/> — after the base HTTP delete, also deletes
///     SAML product info via <see cref="ISamlAttributeServiceAsync"/> and updates GreenBook status.</item>
/// </list>
/// </para>
/// </summary>
public sealed class DepositAlternativeManagementAsync : StandardV1ProductIntegrationAsync
{
    private readonly ISamlAttributeServiceAsync _samlAttributeService;

    /// <summary>
    /// Initialises a new instance. Call <see cref="StandardV1ProductIntegrationAsync.InitAsync"/>
    /// before using any public methods.
    /// </summary>
    public DepositAlternativeManagementAsync(
        int                         productId,
        long                        editorPersonaId,
        long                        subjectPersonaId,
        IDataCollectorAsync         dataCollector,
        IProductRepositoryAsync     productRepository,
        IManagePersonaAsync         managePersona,
        IManageUserLoginAsync       manageUserLogin,
        IUserClaimsAccessor         userClaimsAccessor,
        IHttpClientFactory          httpClientFactory,
        ITokenHelperAsync           tokenHelper,
        ICacheService               cacheService,
        ILoggerFactory              loggerFactory,
        ISamlAttributeServiceAsync  samlAttributeService)
        : base(productId, editorPersonaId, subjectPersonaId,
               dataCollector, productRepository, managePersona, manageUserLogin,
               userClaimsAccessor, httpClientFactory, tokenHelper, cacheService, loggerFactory)
    {
        ArgumentNullException.ThrowIfNull(samlAttributeService);
        _samlAttributeService = samlAttributeService;
    }

    // ── Overrides ──────────────────────────────────────────────────────────────

    /// <inheritdoc/>
    /// <remarks>
    /// Adds <c>CanReceiveMonthlyReport</c> to <see cref="ListResponse.Additional"/>.
    /// When the user has no product account the flag defaults to <c>false</c>.
    /// </remarks>
    public override async Task<ListResponse> GetProductRolesAsync(
        RequestParameter dataFilter,
        string? baseUrlAndQuery = null,
        CancellationToken ct = default)
    {
        var listResponse = await base.GetProductRolesAsync(dataFilter, baseUrlAndQuery, ct);

        bool canReceive = false;
        if (!string.IsNullOrEmpty(SubjectUserDetails?.ProductUserName))
        {
            var user = await GetProductUserAsync(ct: ct);
            canReceive = user?.CanReceiveMonthlyReport ?? false;
        }

        listResponse.Additional = new Dictionary<string, bool>
        {
            { "CanReceiveMonthlyReport", canReceive }
        };

        return listResponse;
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Marks a property group as assigned only when its <c>GroupType</c> matches
    /// the user's first assigned role. This filters groups to the relevant role.
    /// </remarks>
    protected override void MergeUserPropertyGroups(
        IList<ProductPropertyGroups> groupList, IntegrationProductUser user)
    {
        var userPropertyGroups = user.PropertyGroups;
        if (userPropertyGroups is not { Count: > 0 }) return;

        var role = user.Roles?.Count > 0 ? user.Roles[0] : null;
        if (role is null) return;

        foreach (var group in groupList)
        {
            if (group.GroupType == role && userPropertyGroups.Contains(group.GetGroupId))
                group.IsAssigned = true;
        }
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Syncs the <c>productUsername</c> SAML attribute with the user's email
    /// when they differ (same fix as LeadManagement).
    /// </remarks>
    protected override async Task UpdateSamlUserAttributeAsync(
        long personaId, int productId,
        string? productUserId, string? productUserLoginName, string? productUserEmail,
        CancellationToken ct)
    {
        LogDebug(nameof(UpdateSamlUserAttributeAsync),
            $"productUserLoginName={productUserLoginName}");

        if (string.IsNullOrEmpty(productUserLoginName) || string.IsNullOrEmpty(productUserEmail))
            return;

        if (!productUserLoginName.Equals(productUserEmail, StringComparison.OrdinalIgnoreCase))
        {
            await _dataCollector.UpdateSamlUserAttributeAsync(
                personaId, productId, SamlAttributeEnum.productUsername, productUserEmail, ct);
        }
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Uses <c>GetPropertyGroupsEndpoint</c> (plural) explicitly, matching the sync
    /// override that avoids the base-class fallback to <c>GetPropertyGroupEndpoint</c>.
    /// </remarks>
    public override async Task<ListResponse> GetProductPropertyGroupsAsync(
        RequestParameter dataFilter,
        string? baseUrlAndQuery = null,
        CancellationToken ct = default)
    {
        try
        {
            LogDebug(nameof(GetProductPropertyGroupsAsync),
                $"Editor={EditorUserDetails.PersonaId}");

            baseUrlAndQuery ??= GetOperationEndPoint(ProductEntityEndpointKeyEnum.GetPropertyGroupsEndpoint);
            baseUrlAndQuery = string.Format(baseUrlAndQuery, CompanyInstanceSourceId);

            var groupList = await GetResultFromApiAsync<IList<ProductPropertyGroups>>(baseUrlAndQuery, ct: ct);

            if (!string.IsNullOrEmpty(SubjectUserDetails?.ProductUserName))
            {
                var user = await GetProductUserAsync(ct: ct);
                if (user is not null)
                    MergeUserPropertyGroups(groupList!, user);
            }

            if (groupList is null)
                throw new InvalidOperationException("Null Property Group List.");

            return ToListResponse(groupList);
        }
        catch (Exception ex)
        {
            LogError(ex, nameof(GetProductPropertyGroupsAsync));
            return new ListResponse { IsError = true, ErrorReason = ex.Message };
        }
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Extends the base unassign by also deleting all SAML product info via
    /// <see cref="ISamlAttributeServiceAsync.DeleteProductInfoAndStatusAsync"/>.
    /// The base class handles the HTTP delete and GreenBook status update;
    /// this override adds the SAML cleanup when the base reports success.
    /// </remarks>
    public override async Task<string> UnassignUserAsync(CancellationToken ct = default)
    {
        // Base handles: HTTP DELETE → status update (AccountHidden / Deactivated)
        var baseResult = await base.UnassignUserAsync(ct);

        if (!string.IsNullOrEmpty(baseResult))
            return baseResult; // base reported an error — propagate

        // DepositAlternative also removes all SAML product info
        LogDebug(nameof(UnassignUserAsync), "Removing SAML product info");
        await _samlAttributeService.DeleteProductInfoAndStatusAsync(
            SubjectUserDetails!.PersonaId, ProductId, ct);

        return string.Empty;
    }
}
