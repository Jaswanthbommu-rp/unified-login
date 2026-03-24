using Aspose.Cells;
using Aspose.Cells.Drawing;
using IdentityModel;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RealPage.UnifiedNotifications;
using System.ComponentModel;
using System.Text;
using UnifiedLogin.BusinessLogic.Base;
using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
using UnifiedLogin.BusinessLogic.Repository;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product.Rum;
using UnifiedLogin.SharedObjects.Product.VendorServices;
using static Dapper.SqlMapper;
using static LaunchDarkly.Sdk.Server.Migrations.MigrationMethod;
using static SkiaSharp.SKImageFilter;
using Notification = RealPage.UnifiedNotifications.Notification;

namespace UnifiedLogin.BusinessLogic.LogicAsync;

/// <summary>
/// Async-first replacement for <see cref="ManagePersona"/>.
/// <para>Key improvements over the sync version:</para>
/// <list type="bullet">
///   <item>All repository calls use <c>await</c> — no <c>.Result</c> deadlock hazard.</item>
///   <item><c>Task.Run(async () => await notification.SendEvent(...)).Result</c> → proper <c>await</c>.</item>
///   <item><c>RPObjectCache</c> replaced by injected <see cref="IMemoryCache"/>.</item>
///   <item>Single DI constructor — no <c>new</c> keyword anywhere.</item>
///   <item>Serilog <c>Log.Logger</c> replaced by injected <see cref="ILogger{T}"/>.</item>
///   <item><c>DefaultUserClaim</c> field replaced by <see cref="IUserClaimAccessor"/>.</item>
///   <item><c>GetPersonaWithRightsToggle</c> merged into <c>GetPersonaAsync</c> via <c>withRights</c> parameter.</item>
/// </list>
/// Summary of every change and the reasoning:
    //Original Refactored  Why
    //4 constructors with new PersonaRepository() etc.Single DI constructor Testability; follows DI principle
    //DefaultUserClaim _userClaim field set at construction Current on each call Avoids stale claim on long-lived instances
    //Task.Run(async () => await notification.SendEvent(...)).Result await notification.SendEvent(...)	Eliminates sync-over - async deadlock hazard
    //RPObjectCache.GetFromCache(key, 120, factory)   GetOrCreateAsync<T>(string, Func<CancellationToken, Task<T>>, CancellationToken) Standard.NET DI cache; no static shared state
    //Log.Logger.ForContext(...).Write(...)   _logger.LogDebug(...)	Injected ILogger<T> respects configured providers
    //GetPersona(long) + GetPersonaWithRightsToggle(long, bool) (two methods) GetPersonaAsync(id, withRights = true)  Single method, optional param covers both paths
    //throw new Exception(...)    ArgumentException / ArgumentOutOfRangeException.ThrowIfEqual.NET 10 built-in guard APIs; correct exception type
    //ApiSecret decoded inline in the constructor Decoded lazily inside ChangeCompanyNotificationAsync(long, CancellationToken)   Settings only loaded when the method is actually called

/// </summary>
public sealed class ManagePersonaAsync : IManagePersonaAsync
{
    #region Fields

    private readonly IPersonaRepositoryAsync _personaRepo;
    private readonly IProductInternalSettingRepositoryAsync _settingRepo;
    private readonly IUserClaimsAccessor _userClaimAccessor;
    private readonly IMemoryCache _cache;
    private readonly ILogger<ManagePersonaAsync> _logger;

    private const int SettingsCacheSeconds = 120;

    #endregion

    #region Constructor

    public ManagePersonaAsync(
        IPersonaRepositoryAsync personaRepo,
        IProductInternalSettingRepositoryAsync settingRepo,
        IUserClaimsAccessor userClaimAccessor,
        IMemoryCache cache,
        ILogger<ManagePersonaAsync> logger)
    {
        _personaRepo = personaRepo ?? throw new ArgumentNullException(nameof(personaRepo));
        _settingRepo = settingRepo ?? throw new ArgumentNullException(nameof(settingRepo));
        _userClaimAccessor = userClaimAccessor ?? throw new ArgumentNullException(nameof(userClaimAccessor));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #endregion

    // ════════════════════════════════════════════════════════════════════════
    // IManagePersonaAsync — environment type
    // ════════════════════════════════════════════════════════════════════════

    #region GetPersonaEnvironmentTypeAsync

    /// <inheritdoc/>
    public Task<IList<PersonaEnvironment>> GetPersonaEnvironmentTypeAsync(
        CancellationToken cancellationToken = default)
        => _personaRepo.GetPersonaEnvironmentTypeAsync(cancellationToken);

    #endregion

    // ════════════════════════════════════════════════════════════════════════
    // IManagePersonaAsync — create
    // ════════════════════════════════════════════════════════════════════════

    #region CreatePersonaAsync / CreateAdditionalPersonaAsync

    /// <inheritdoc/>
    public Task<RepositoryResponse> CreatePersonaAsync(
        Guid personRealPageId, Guid organizationRealPageId, IPersona persona,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfEqual(personRealPageId, Guid.Empty, nameof(personRealPageId));
        ArgumentOutOfRangeException.ThrowIfEqual(organizationRealPageId, Guid.Empty, nameof(organizationRealPageId));
        ArgumentNullException.ThrowIfNull(persona, nameof(persona));

        return _personaRepo.CreatePersonaAsync(personRealPageId, organizationRealPageId, persona, cancellationToken);
    }

    /// <inheritdoc/>
    public Task<RepositoryResponse> CreateAdditionalPersonaAsync(
        Guid organizationRealPageId, long userId, long createdBy, string personaName,
        CancellationToken cancellationToken = default)
    {
        if (userId == 0)
            throw new ArgumentException("Invalid parameter userId.", nameof(userId));
        ArgumentOutOfRangeException.ThrowIfEqual(organizationRealPageId, Guid.Empty, nameof(organizationRealPageId));

        return _personaRepo.CreateAdditionalPersonaAsync(
            organizationRealPageId, userId, createdBy, personaName, cancellationToken);
    }

    #endregion

    // ════════════════════════════════════════════════════════════════════════
    // IManagePersonaAsync — single persona reads
    // ════════════════════════════════════════════════════════════════════════

    #region GetPersonaAsync

    /// <inheritdoc/>
    /// <remarks>
    /// Replaces both <c>GetPersona(personaId)</c> and <c>GetPersonaWithRightsToggle(personaId, withRights)</c>
    /// from the sync version — the <paramref name="withRights"/> parameter covers both cases.
    /// </remarks>
    public Task<Persona> GetPersonaAsync(
        long personaId, bool withRights = true, CancellationToken cancellationToken = default)
    {
        if (personaId == 0) throw new ArgumentException("Invalid parameter personaId.", nameof(personaId));
        return _personaRepo.GetPersonaAsync(personaId, withRights, cancellationToken);
    }

    #endregion

    // ════════════════════════════════════════════════════════════════════════
    // IManagePersonaAsync — list reads
    // ════════════════════════════════════════════════════════════════════════

    #region ListPersonaAsync / ListActivePersonaAsync / ListEmployeePersonasAsync / ListPersonaByOrganizationPartyIdAsync

    /// <inheritdoc/>
    public Task<IList<Persona>> ListPersonaAsync(
        Guid realPageId, CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfEqual(realPageId, Guid.Empty, nameof(realPageId));
        return _personaRepo.ListPersonaAsync(realPageId, cancellationToken);
    }

    /// <inheritdoc/>
    public Task<IList<Persona>> ListActivePersonaAsync(
        Guid realPageId, bool includeOrganization, CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfEqual(realPageId, Guid.Empty, nameof(realPageId));
        return _personaRepo.ListActivePersonaAsync(realPageId, includeOrganization, cancellationToken);
    }

    /// <inheritdoc/>
    public Task<IList<Persona>> ListEmployeePersonasAsync(
        long userId, long orgPartyId, CancellationToken cancellationToken = default)
    {
        if (userId == 0) throw new ArgumentException("Invalid parameter userId.", nameof(userId));
        if (orgPartyId == 0) throw new ArgumentException("Invalid parameter orgPartyId.", nameof(orgPartyId));
        return _personaRepo.ListEmployeePersonasAsync(userId, orgPartyId, cancellationToken);
    }

    /// <inheritdoc/>
    public Task<IList<Persona>> ListPersonaByOrganizationPartyIdAsync(
        long organizationPartyId, bool? isDefault = null, int? userRoleType = null,
        CancellationToken cancellationToken = default)
    {
        if (organizationPartyId == 0)
            throw new ArgumentException("Invalid parameter organizationPartyId.", nameof(organizationPartyId));

        return _personaRepo.ListPersonaByOrganizationPartyIdAsync(
            organizationPartyId, isDefault, userRoleType, cancellationToken);
    }

    #endregion

    // ════════════════════════════════════════════════════════════════════════
    // IManagePersonaAsync — active persona
    // ════════════════════════════════════════════════════════════════════════

    #region GetActivePersonaIdAsync / GetActivePersonaAsync / GetActivePersonaWithoutRightsAsync

    /// <inheritdoc/>
    public Task<long> GetActivePersonaIdAsync(Guid realPageId, CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfEqual(realPageId, Guid.Empty, nameof(realPageId));
        return _personaRepo.GetActivePersonaIdAsync(realPageId, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<Persona> GetActivePersonaAsync(
        Guid realPageId, CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfEqual(realPageId, Guid.Empty, nameof(realPageId));

        long personaId = await _personaRepo.GetActivePersonaIdAsync(realPageId, cancellationToken);
        return personaId > 0
            ? await GetPersonaAsync(personaId, withRights: true, cancellationToken)
            : new Persona();
    }

    /// <inheritdoc/>
    public async Task<Persona> GetActivePersonaWithoutRightsAsync(
        Guid realPageId, CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfEqual(realPageId, Guid.Empty, nameof(realPageId));

        long personaId = await _personaRepo.GetActivePersonaIdAsync(realPageId, cancellationToken);
        return personaId > 0
            ? await GetPersonaAsync(personaId, withRights: false, cancellationToken)
            : new Persona();
    }

    #endregion

    #region GetFirstAvailablePersonaByCompanyAsync

    /// <inheritdoc/>
    /// <remarks>
    /// Replaces: <c>GetFirstAvailablePersonaByCompany(Guid, long)</c> from the sync version.
    /// <para>
    /// The RealPage-Employee fallback now reads the current user claim via
    /// <see cref="IUserClaimAccessor"/> instead of a constructor-time <c>DefaultUserClaim</c> field.
    /// </para>
    /// </remarks>
    public async Task<Persona?> GetFirstAvailablePersonaByCompanyAsync(
        Guid realPageId, long orgPartyId, CancellationToken cancellationToken = default)
    {
        var personaList = await _personaRepo.ListPersonaAsync(realPageId, cancellationToken);
        if (personaList.Count == 0) return null;

        // RP-Employee fallback: if no persona exists for the requested company,
        // use the first available company instead.
        if (personaList.All(p => p.OrganizationPartyId != orgPartyId))
        {
            if (_userClaimAccessor.Current.RealPageEmployee)
                orgPartyId = personaList[0].OrganizationPartyId;
            else
                return null;
        }

        long personaId = personaList
            .FirstOrDefault(p => p.OrganizationPartyId == orgPartyId)
            ?.PersonaId ?? 0;

        return personaId > 0
            ? await GetPersonaAsync(personaId, withRights: true, cancellationToken)
            : null;
    }

    #endregion

    // ════════════════════════════════════════════════════════════════════════
    // IManagePersonaAsync — update
    // ════════════════════════════════════════════════════════════════════════

    #region UpdateActivePersonaAsync

    /// <inheritdoc/>
    public Task<RepositoryResponse> UpdateActivePersonaAsync(
        Guid realPageId, long personaId, CancellationToken cancellationToken = default)
        => _personaRepo.UpdateActivePersonaAsync(realPageId, personaId, cancellationToken);

    #endregion

    // ════════════════════════════════════════════════════════════════════════
    // IManagePersonaAsync — notifications
    // ════════════════════════════════════════════════════════════════════════

    #region ChangeCompanyNotificationAsync

    /// <inheritdoc/>
    /// <remarks>
    /// Replaces: blocking <c>Task.Run(async () => await notification.SendEvent(...)).Result</c>
    /// with a direct <c>await</c>.  Product settings are fetched via the cached
    /// <see cref="GetProductInternalSettingsAsync"/> helper.
    /// </remarks>
    public async Task<Guid> ChangeCompanyNotificationAsync(
        long personaId, CancellationToken cancellationToken = default)
    {
        var settings = await GetProductInternalSettingsAsync(ProductEnum.UnifiedPlatform, cancellationToken);

        string Get(string key) => settings
            .First(a => a.Name.Equals(key, StringComparison.OrdinalIgnoreCase))
            .Value;

        string eventName = Get("NotificationsEventChangeCompany");
        string apiEndPoint = Get("NotificationsApiEndPoint");
        string eventsEndPoint = Get("NotificationsEventsEndPoint");
        string tokenEndpoint = Get("TokenEndPoint");
        string clientId = Get("UnifiedLoginServerClientName");
        string apiSecret = Encoding.UTF8.GetString(
                                        Convert.FromBase64String(Get("UnifiedLoginServerClientSecret")));

        var nEvent = new NotificationEvent
        {
            Method = eventName,
            ProductCode = "UL",
            Users = [personaId.ToString()],
            Data = new NotificationEventData { PersonaId = personaId }
        };

        _logger.LogDebug("ChangeCompanyNotificationAsync → sending event for personaId={Id}", personaId);

        var notification = new Notification(
            clientId, apiSecret, tokenEndpoint,
            $"{apiEndPoint}/v1/notifications",
            $"{apiEndPoint}/{eventsEndPoint}");

        // ── FIX: was Task.Run(async () => await ...).Result (sync-over-async deadlock hazard) ──
        var result = await notification.SendEvent(
            nEvent.ProductCode, nEvent.Users.ToList(), nEvent.Method, nEvent.Data);

        Guid notificationGuid = !string.IsNullOrWhiteSpace(result?.Id)
            ? new Guid(result.Id)
            : Guid.Empty;

        _logger.LogDebug("ChangeCompanyNotificationAsync → complete. Guid={G}", notificationGuid);

        return notificationGuid;
    }

    #endregion

    // ════════════════════════════════════════════════════════════════════════
    // Private — helpers
    // ════════════════════════════════════════════════════════════════════════

    #region GetProductInternalSettingsAsync

    /// <summary>
    /// Replaces: <c>RPObjectCache.GetFromCache(key, 120, () => _productRepository.GetProductInternalSettings(...))</c>
    /// — now uses <see cref="IMemoryCache.GetOrCreateAsync{T}"/> with the same 120-second TTL.
    /// </summary>
    private async Task<List<ProductInternalSetting>> GetProductInternalSettingsAsync(
        ProductEnum product, CancellationToken ct)
    {
        string cacheKey = $"productInternalSetting_{(int)product}";

        return await _cache.GetOrCreateAsync<List<ProductInternalSetting>>(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(SettingsCacheSeconds);
            var settings = await _settingRepo.GetProductInternalSettingsAsync((int)product, ct);
            return settings?.ToList() ?? [];
        }) ?? [];
    }

    #endregion
}