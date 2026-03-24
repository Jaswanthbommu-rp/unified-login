using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using UnifiedLogin.BusinessLogic.Base;
using UnifiedLogin.BusinessLogic.Logic.Helper;
using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects.Audit.Common;
using UnifiedLogin.SharedObjects.Constants;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product;

namespace UnifiedLogin.BusinessLogic.LogicAsync;

/// <summary>
/// Default implementation of <see cref="IProductAuditServiceAsync"/>.
/// Orchestrates persona / product repository lookups and fires
/// <c>LogActivity.WriteActivity</c> — the seven audit event methods previously
/// duplicated as protected helpers on <c>ManageProductBase</c> now live here.
/// </summary>
public sealed class ProductAuditServiceAsync : IProductAuditServiceAsync
{
    private readonly IPersonaRepositoryAsync _personaRepo;
    private readonly IPersonRepositoryAsync _personRepo;
    private readonly IProductRepositoryAsync _productRepo;
    private readonly IUserClaimsAccessor _userClaimAccessor;
    private readonly IMemoryCache _cache;
    private readonly ILogger<ProductAuditServiceAsync> _logger;
    private readonly IUserLoginRepositoryAsync _userLoginRepo;

    private const int ProductCacheMinutes = 2;

    public ProductAuditServiceAsync(
        IPersonaRepositoryAsync personaRepo,
        IPersonRepositoryAsync personRepo,
        IUserLoginRepositoryAsync userLoginRepo,
        IProductRepositoryAsync productRepo,
        IUserClaimsAccessor userClaimAccessor,
        IMemoryCache cache,
        ILogger<ProductAuditServiceAsync> logger)
    {
        ArgumentNullException.ThrowIfNull(personaRepo);       _personaRepo       = personaRepo;
        ArgumentNullException.ThrowIfNull(personRepo);        _personRepo = personRepo;
        ArgumentNullException.ThrowIfNull(productRepo);       _productRepo       = productRepo;
        ArgumentNullException.ThrowIfNull(userClaimAccessor); _userClaimAccessor = userClaimAccessor;
        ArgumentNullException.ThrowIfNull(cache);             _cache             = cache;
        ArgumentNullException.ThrowIfNull(logger);            _logger            = logger;
        ArgumentNullException.ThrowIfNull(userLoginRepo);     _userLoginRepo     = userLoginRepo;
    }

    // ════════════════════════════════════════════════════════════════════════
    // 1. Generic event writer
    // ════════════════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    public async Task WriteProductEventAsync(
        long fromPersonaId, long toPersonaId, int productId,
        string messageTemplate, CancellationToken ct = default)
    {
        var from   = await GetUserActivityLogInfoAsync(fromPersonaId, ct);
        var to     = await GetUserActivityLogInfoAsync(toPersonaId, ct);
        var detail = await GetProductDetailAsync(productId, ct);

        string message = string.Format(messageTemplate,
            to.FirstName,    to.LastName,
            detail?.Name     ?? string.Empty,
            from.FirstName,  from.LastName);

        Push(from, to, detail?.BooksProductCode ?? string.Empty, message);
    }

    // ════════════════════════════════════════════════════════════════════════
    // 2–4. Lifecycle events
    // ════════════════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    public Task WriteUnassignAsync(
        long fromPersonaId, long toPersonaId, int productId, CancellationToken ct = default)
        => WriteProductEventAsync(fromPersonaId, toPersonaId, productId,
            "{0} {1} is unassigned in product {2} by user {3} {4}.", ct);

    /// <inheritdoc/>
    public Task WriteDeactivateAsync(
        long fromPersonaId, long toPersonaId, int productId, CancellationToken ct = default)
        => WriteProductEventAsync(fromPersonaId, toPersonaId, productId,
            "{0} {1} is deactivated in product {2} by user {3} {4}.", ct);

    /// <inheritdoc/>
    public Task WriteReactivateAsync(
        long fromPersonaId, long toPersonaId, int productId, CancellationToken ct = default)
        => WriteProductEventAsync(fromPersonaId, toPersonaId, productId,
            "{0} {1} is re-activated in product {2} by user {3} {4}.", ct);

    // ════════════════════════════════════════════════════════════════════════
    // 5–7. User management events
    // ════════════════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    public Task WriteCreateUserAsync(
        long fromPersonaId, long toPersonaId, int productId, CancellationToken ct = default)
        => WriteProductEventAsync(fromPersonaId, toPersonaId, productId,
            "{0} {1} created in product {2} by user {3} {4}.", ct);

    /// <inheritdoc/>
    public Task WriteResetVerificationCodeAsync(
        long fromPersonaId, long toPersonaId, int productId, CancellationToken ct = default)
        => WriteProductEventAsync(fromPersonaId, toPersonaId, productId,
            "{3} {4} reset the OneSite verification code for {0} {1}.", ct);

    /// <inheritdoc/>
    /// <remarks>
    /// Uses a switch expression over <paramref name="batchProcessType"/> to select the
    /// correct message template — matches the original if/else chain in
    /// <c>ManageProductBase.WriteUpdateUserTypeActivityLog</c>.
    /// </remarks>
    public Task WriteUserTypeChangeAsync(
        long fromPersonaId, long toPersonaId, int productId,
        BatchProcessType batchProcessType, CancellationToken ct = default)
    {
        string? template = batchProcessType switch
        {
            BatchProcessType.UserTypeRegularToAdmin  =>
                "{0} {1} user type changed from Regular User to admin in product {2} by user {3} {4}.",
            BatchProcessType.UserTypeAdminToRegular  =>
                "{0} {1} user type changed from admin to Regular User in product {2} by user {3} {4}.",
            BatchProcessType.UserTypeAdminToExternal =>
                "{0} {1} user type changed from admin to External User in product {2} by user {3} {4}.",
            BatchProcessType.UserTypeExternalToAdmin =>
                "{0} {1} user type changed from External User to admin in product {2} by user {3} {4}.",
            _ => null
        };

        return template is null
            ? Task.CompletedTask
            : WriteProductEventAsync(fromPersonaId, toPersonaId, productId, template, ct);
    }

    // ════════════════════════════════════════════════════════════════════════
    // Private helpers
    // ════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Resolves <see cref="UserActivityLogInfoAsync"/> from the persona repository.
    /// Mirrors <c>ManageProductBaseAsync.GetUserActivityLogInfoAsync</c> — reads
    /// first/last name directly from <see cref="Persona"/> (no separate Person lookup).
    /// </summary>
    private async Task<UserActivityLogInfoAsync> GetLogInfoAsync(long personaId, CancellationToken ct)
    {
        var persona = await _personaRepo.GetPersonaAsync(personaId, false, ct);
        var person = await _personRepo.GetPersonAsync(persona.RealPageId, ct);
        var userLogin = await _userLoginRepo.GetUserLoginOnlyAsync(persona.RealPageId);
        return new UserActivityLogInfoAsync
        {
            FirstName                 = person?.FirstName                   ?? string.Empty,
            LastName                  = person?.LastName                    ?? string.Empty,
            LoginName                 = userLogin?.LoginName        ?? string.Empty,
            UserId                    = persona?.UserId                      ?? 0,
            RealPageId                = persona?.RealPageId                  ?? Guid.Empty,
            BooksOrganizationMasterId = persona?.Organization?.BooksMasterId ?? 0,
            OrganizationPartyId       = persona?.OrganizationPartyId         ?? 0
        };
    }

    /// <summary>
    /// Fetches product details with a 2-minute cache — same TTL as
    /// <c>ManageProductBaseAsync.GetProductDetailAsync</c>.
    /// </summary>
    private async Task<GbProductMap?> GetProductDetailAsync(int productId, CancellationToken ct)
    {
        string key = $"productDetails_{productId}";
        if (_cache.TryGetValue(key, out GbProductMap? hit))
            return hit;

        var detail = await _productRepo.GetBooksMasterProductDetailAsync(productId);
        if (detail is not null)
            _cache.Set(key, detail, TimeSpan.FromMinutes(ProductCacheMinutes));
        return detail;
    }

    /// <summary>
    /// Low-level fire-and-forget queue push.
    /// <c>LogActivity.WriteActivity</c> is synchronous — exceptions are caught and
    /// logged as warnings so they never break the caller's business operation.
    /// </summary>
    private void Push(
        UserActivityLogInfoAsync from, UserActivityLogInfoAsync to,
        string booksProductCode, string message)
    {
        var claims = _userClaimAccessor.Current;
        try
        {
            LogActivity.WriteActivity(new ActivityDetails
            {
                LogActivityTypeName       = LogActivityTypeConstants.PRODUCT_ACCESS,
                LogCategoryName           = LogActivityCategoryType.ProductAccess.ToString(),
                CorrelationId             = claims.CorrelationId.ToString(),
                BooksMasterOrganizationId = to.BooksOrganizationMasterId,
                OrganizationPartyId       = to.OrganizationPartyId,
                Message                   = message,
                FromUserLoginName         = from.LoginName,
                FromUserLoginId           = from.UserId,
                FromUserFirstName         = from.FirstName,
                FromUserLastName          = from.LastName,
                FromUserRealpageId        = from.RealPageId.ToString(),
                ToUserLoginId             = to.UserId,
                ToUserLoginName           = to.LoginName,
                ToUserFirstName           = to.FirstName,
                ToUserLastName            = to.LastName,
                ToUserRealpageId          = to.RealPageId.ToString(),
                BooksProductCode          = booksProductCode
            });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "ProductAuditServiceAsync.Push failed booksProductCode={Code}", booksProductCode);
        }
    }

    // ════════════════════════════════════════════════════════════════════════
    // Public helper — exposed on the interface so any manager can reuse it
    // ════════════════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    /// <remarks>
    /// Promoted from <c>private GetLogInfoAsync</c> — now shared across all managers
    /// that previously duplicated this persona-lookup pattern.
    /// </remarks>
    public async Task<UserActivityLogInfoAsync> GetUserActivityLogInfoAsync(
        long personaId, CancellationToken ct = default)
    {
        var persona   = await _personaRepo.GetPersonaAsync(personaId, false, ct);
        var person    = await _personRepo.GetPersonAsync(persona.RealPageId, ct);
        var userLogin = await _userLoginRepo.GetUserLoginOnlyAsync(persona.RealPageId);
        return new UserActivityLogInfoAsync
        {
            FirstName                 = person?.FirstName                   ?? string.Empty,
            LastName                  = person?.LastName                    ?? string.Empty,
            LoginName                 = userLogin?.LoginName                ?? string.Empty,
            UserId                    = persona?.UserId                     ?? 0,
            RealPageId                = persona?.RealPageId                 ?? Guid.Empty,
            BooksOrganizationMasterId = persona?.Organization?.BooksMasterId ?? 0,
            OrganizationPartyId       = persona?.OrganizationPartyId        ?? 0
        };
    }
}