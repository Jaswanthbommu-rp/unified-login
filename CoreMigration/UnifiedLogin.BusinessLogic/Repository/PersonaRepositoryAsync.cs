using Dapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Data;
using System.Security.Claims;
using UnifiedLogin.BusinessLogic.Base;
using UnifiedLogin.BusinessLogic.CacheHelper;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.BusinessLogic.Services.User;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Cache;
using UnifiedLogin.SharedObjects.Constants;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Helper;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Landing.Enum;

namespace UnifiedLogin.BusinessLogic.Repository;

/// <summary>
/// Async-first Persona Repository.
/// Uses injected <see cref="IDbConnection"/> (Dapper) directly —
/// no <see cref="BaseRepository"/> inheritance, no <c>new</c> keyword.
/// </summary>
public sealed class PersonaRepositoryAsync : IPersonaRepositoryAsync
{
    #region Fields

    private readonly IDbConnection _db;    // read-write
    private readonly IDbConnection _dbRo;  // read-only replica
    private readonly ICacheService _cache;
    private readonly IOrganizationRepositoryAsync _organizationRepository;
    private readonly IUserLoginRepositoryAsync _userLoginRepository;
    private readonly IUserClaimsAccessor _userClaimAccessor;

    // Replaces: new UserRoleRightRepository() in AddRightsToPersona
    private readonly IUserRoleRightRepositoryAsync _userRoleRightRepository;

    private readonly ILogger<PersonaRepositoryAsync> _logger;

    // Cache TTLs (seconds)
    //private static readonly TimeSpan RoleRightsTtl = TimeSpan.FromSeconds(180);
    //private static readonly TimeSpan ProductListTtl = TimeSpan.FromSeconds(180);
    private static readonly CacheEntryOptions RoleRightsTtlCacheOptions = new() { ExpirationTimeInMinutes = 3 };
    private static readonly CacheEntryOptions ProductListTtlCacheOptions = new() { ExpirationTimeInMinutes = 3 };
    #endregion

    #region Constructor

    /// <summary>
    /// Primary DI constructor — all dependencies injected, no <c>new</c>.
    /// </summary>
    public PersonaRepositoryAsync(
        [FromKeyedServices("rw")] IDbConnection db,
        [FromKeyedServices("ro")] IDbConnection dbRo,
        ICacheService cacheService,
        IOrganizationRepositoryAsync organizationRepositoryAsync,
        IUserLoginRepositoryAsync userLoginRepositoryAsync,
        IUserClaimsAccessor userClaimAccessor,
        IUserRoleRightRepositoryAsync userRoleRightRepository,
        ILogger<PersonaRepositoryAsync> logger)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _dbRo = dbRo ?? throw new ArgumentNullException(nameof(dbRo));
        _cache = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
        _organizationRepository = organizationRepositoryAsync ?? throw new ArgumentNullException(nameof(organizationRepositoryAsync));
        _userLoginRepository = userLoginRepositoryAsync ?? throw new ArgumentNullException(nameof(userLoginRepositoryAsync));
        _userClaimAccessor = userClaimAccessor ?? throw new ArgumentNullException(nameof(userClaimAccessor));
        _userRoleRightRepository = userRoleRightRepository ?? throw new ArgumentNullException(nameof(userRoleRightRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #endregion

    #region Async Implementations

    /// <inheritdoc/>
    public async Task<IList<PersonaEnvironment>> GetPersonaEnvironmentTypeAsync(
        CancellationToken cancellationToken = default)
    {
        var result = await _dbRo.QueryAsync<PersonaEnvironment>(
            new CommandDefinition(
                StoredProcNameConstants.SP_GetPersonaEnvironment,
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));

        return result.ToList();
    }

    /// <inheritdoc/>
    public async Task<RepositoryResponse> CreatePersonaAsync(
        Guid personRealPageId,
        Guid organizationRealPageId,
        IPersona persona,
        CancellationToken cancellationToken = default)
    {
        var param = new
        {
            PersonRealPageId = personRealPageId,
            OrganizationRealPageId = organizationRealPageId,
            persona.PersonaTypeId,
            persona.FromDate,
            persona.ThruDate,
            PersonaId = 0L
        };

        return await _db.QuerySingleOrDefaultAsync<RepositoryResponse>(
            new CommandDefinition(
                StoredProcNameConstants.SP_CreatePersona,
                param,
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));
    }

    /// <inheritdoc/>
    public async Task<Persona> GetPersonaAsync(
        long personaId,
        bool withRights = true,
        CancellationToken cancellationToken = default)
    {
        var persona = await _dbRo.QuerySingleOrDefaultAsync<Persona>(
            new CommandDefinition(
                StoredProcNameConstants.SP_GetPersona,
                new { personaId },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));

        if (persona is null) return null!;

        // Replaces: _organizationRepository.GetOrganization(organizationPartyId: persona.OrganizationPartyId)
        var organization = await _organizationRepository.GetOrganizationAsync(
            organizationPartyId: persona.OrganizationPartyId);

        if (organization is not null)
            persona.Organization = organization;

        if (withRights)
            persona = await AddRightsToPersonaAsync(persona, cancellationToken);

        return persona;
    }

    /// <inheritdoc/>
    public async Task<IList<Persona>> ListPersonaAsync(
        Guid realPageId,
        CancellationToken cancellationToken = default)
    {
        // Fetch persona list and org list concurrently
        var personaTask = _db.QueryAsync<Persona>(
            new CommandDefinition(
                StoredProcNameConstants.SP_ListPersona,
                new { RealPageId = realPageId },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));

        var orgTask =  _userLoginRepository.ListOrganizationByEnterpriseUserIdAsync(
            realPageId, null);

        await Task.WhenAll(personaTask, orgTask);

        var personas = (await personaTask).ToList();
        var orgList = await orgTask;

        personas.ForEach(p =>
            p.Organization = orgList.FirstOrDefault(o => o.PartyId == p.OrganizationPartyId));

        return personas;
    }

    /// <inheritdoc/>
    public async Task<IList<Persona>> ListActivePersonaAsync(
        Guid realPageId,
        bool includeOrganization,
        CancellationToken cancellationToken = default)
    {
        var personaTask = _db.QueryAsync<Persona>(
            new CommandDefinition(
                StoredProcNameConstants.SP_ListActivePersona,
                new { RealPageId = realPageId },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));

        var orgTask = _userLoginRepository.ListOrganizationByEnterpriseUserIdAsync(
            realPageId, null);

        await Task.WhenAll(personaTask, orgTask);

        var personas = (await personaTask).ToList();
        var orgList = await orgTask;

        personas.ForEach(p =>
            p.Organization = orgList.FirstOrDefault(o => o.PartyId == p.OrganizationPartyId));

        return personas;
    }

    /// <inheritdoc/>
    public async Task<IList<Persona>> ListEmployeePersonasAsync(
        long userId,
        long orgPartyId,
        CancellationToken cancellationToken = default)
    {
        var result = await _db.QueryAsync<Persona>(
            new CommandDefinition(
                StoredProcNameConstants.SP_ListEmployeePersonas,
                new { UserId = userId, OrgPartyId = orgPartyId },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));

        return result.ToList();
    }

    /// <inheritdoc/>
    public async Task<IList<Persona>> ListPersonaByOrganizationPartyIdAsync(
        long organizationPartyId,
        bool? isDefault = null,
        int? userRoleType = null,
        CancellationToken cancellationToken = default)
    {
        // Fetch persona list and org concurrently
        var personaTask = _db.QueryAsync<Persona>(
            new CommandDefinition(
                StoredProcNameConstants.SP_ListPersonaByOrganizationPartyId,
                new { OrganizationPartyId = organizationPartyId, IsDefault = isDefault, UserRoleType = userRoleType },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));

        var orgTask = _organizationRepository.GetOrganizationAsync(
            organizationPartyId: organizationPartyId);

        await Task.WhenAll(personaTask, orgTask);

        var personas = (await personaTask).ToList();
        var organization = await orgTask;

        personas.ForEach(p => p.Organization = organization);
        return personas;
    }

    /// <inheritdoc/>
    public async Task<long> GetActivePersonaIdAsync(
        Guid realPageId,
        CancellationToken cancellationToken = default)
    {
        return await _db.QuerySingleOrDefaultAsync<long>(
            new CommandDefinition(
                StoredProcNameConstants.SP_GetActivePersona,
                new { RealPageId = realPageId },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));
    }

    /// <inheritdoc/>
    public async Task<RepositoryResponse> UpdateActivePersonaAsync(
        Guid personRealPageId,
        long personaId,
        CancellationToken cancellationToken = default)
    {
        return await _db.QuerySingleOrDefaultAsync<RepositoryResponse>(
            new CommandDefinition(
                StoredProcNameConstants.SP_UpdateActivePersona,
                new { RealPageId = personRealPageId, PersonaId = personaId },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));
    }

    /// <inheritdoc/>
    public async Task<List<ProductSettingList>> GetPersonaProductSettingsAsync(
        long personaId,
        CancellationToken cancellationToken = default)
    {
        var result = await _db.QueryAsync<ProductSettingList>(
            new CommandDefinition(
                StoredProcNameConstants.SP_ListProductSettingsByPersonaId,
                new { PersonaId = personaId },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));

        return result.ToList();
    }

    /// <inheritdoc/>
    public async Task<RepositoryResponse> CreateAdditionalPersonaAsync(
        Guid organizationRealPageId,
        long userId,
        long createdBy,
        string personaName,
        CancellationToken cancellationToken = default)
    {
        var param = new
        {
            OrganizationRealPageId = organizationRealPageId,
            UserId = userId,
            CreatedBy = createdBy,
            PersonaName = personaName,
            PersonaId = 0L
        };

        return await _db.QuerySingleOrDefaultAsync<RepositoryResponse>(
            new CommandDefinition(
                StoredProcNameConstants.SP_CreateAdditionalPersona,
                param,
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));
    }

    #endregion

    #region Private Helpers

    /// <summary>
    /// Async version of AddRightsToPersona.
    /// Replaces: <c>new UserRoleRightRepository()</c>, <c>new RPObjectCache()</c>,
    /// <c>new SharedDataRepository()</c> with injected dependencies.
    /// </summary>
    private async Task<Persona> AddRightsToPersonaAsync(
        Persona persona,
        CancellationToken cancellationToken)
    {
        if (persona is null) return null!;

        persona.hasViewOnlySupportToolAccess = false;

        // IUserClaimAccessor replaces the stored _userClaim field
        var currentClaim = _userClaimAccessor.Current;
        ClaimsPrincipal? principal = currentClaim.ClaimsPrincipal;
        List<string> rights = currentClaim.Rights;

        // NOT Super user — apply product access rights from claims
        if (persona.UserTypeId != UserTypeConstants.SuperUser)
        {
            ApplyProductRightsFromClaims(persona, rights);
        }

        if (principal?.Identity?.IsAuthenticated != true)
            return persona;

        // Apply authenticated user rights (settings, impersonation-specific)
        ApplyAuthenticatedRights(persona, rights);

        // Impersonation rights supplementation
        if (currentClaim.ImpersonatedBy != Guid.Empty
            && NeedsImpersonatorRightsCheck(persona))
        {
            await SupplementImpersonatorRightsAsync(
                persona, currentClaim.ImpersonatedBy, cancellationToken);
        }

        return persona;
    }

    /// <summary>
    /// Applies product-level access rights from the editor's claim rights list.
    /// Extracted from the original 30-line block in <c>AddRightsToPersona</c>.
    /// </summary>
    private static void ApplyProductRightsFromClaims(Persona persona, List<string> rights)
    {
        persona.hasResidentPortalUserAccess = CheckUserRight.CheckUserHasAccess(rights, "AddEditResidentPortalUser");
        persona.hasManageAccountingProductAccess = rights.Contains(ProductRightEnum.ManageAccountingProductAccess.ToString(), StringComparer.OrdinalIgnoreCase);
        persona.hasManageAssetOptimizationProductAccess = rights.Contains(ProductRightEnum.ManageAssetOptimizationProductAccess.ToString(), StringComparer.OrdinalIgnoreCase);
        persona.hasManageClientPortalProductAccess = rights.Contains(ProductRightEnum.ManageClientPortalProductAccess.ToString(), StringComparer.OrdinalIgnoreCase);
        persona.hasManageDocumentManagementProductAccess = rights.Contains(ProductRightEnum.ManageDocumentManagementProductAccess.ToString(), StringComparer.OrdinalIgnoreCase);
        persona.hasManageILMLeadManagemementProductAccess = rights.Contains(ProductRightEnum.ManageILMLeadManagemementProductAccess.ToString(), StringComparer.OrdinalIgnoreCase);
        persona.hasManageILMLeasingAnalyticsProductAccess = rights.Contains(ProductRightEnum.ManageILMLeasingAnalyticsProductAccess.ToString(), StringComparer.OrdinalIgnoreCase);
        persona.hasManageLead2LeaseProductAccess = rights.Contains(ProductRightEnum.ManageLead2LeaseProductAccess.ToString(), StringComparer.OrdinalIgnoreCase);
        persona.hasManageMarketingCenterProductAccess = rights.Contains(ProductRightEnum.ManageMarketingCenterProductAccess.ToString(), StringComparer.OrdinalIgnoreCase);
        persona.hasManageOneSiteProductAccess = rights.Contains(ProductRightEnum.ManageOneSiteProductAccess.ToString(), StringComparer.OrdinalIgnoreCase);
        persona.hasManageOnSiteProductAccess = rights.Contains(ProductRightEnum.ManageOnSiteProductAccess.ToString(), StringComparer.OrdinalIgnoreCase);
        persona.hasProspectContactCenterProductAccess = rights.Contains(ProductRightEnum.ProspectContactCenterProductAccess.ToString(), StringComparer.OrdinalIgnoreCase);
        persona.hasManageRentersInsuranceProductAccess = rights.Contains(ProductRightEnum.ManageRentersInsuranceProductAccess.ToString(), StringComparer.OrdinalIgnoreCase);
        persona.hasManageSpendManagementProductAccess = rights.Contains(ProductRightEnum.ManageSpendManagementProductAccess.ToString(), StringComparer.OrdinalIgnoreCase);
        persona.hasManageUnifiedAmenitiesProductAccess = rights.Contains(ProductRightEnum.ManageUnifiedAmenitiesProductAccess.ToString(), StringComparer.OrdinalIgnoreCase);
        persona.hasManageUtilityManagementProductAccess = rights.Contains(ProductRightEnum.ManageUtilityManagementProductAccess.ToString(), StringComparer.OrdinalIgnoreCase);
        persona.hasManageVendorComplianceProductAccess = rights.Contains(ProductRightEnum.ManageVendorComplianceProductAccess.ToString(), StringComparer.OrdinalIgnoreCase);
        persona.hasManageRealConnectProductAccess = rights.Contains(ProductRightEnum.ManageRealConnectProductAccess.ToString(), StringComparer.OrdinalIgnoreCase);
        persona.hasManagePortfolioManagementProductAccess = rights.Contains(ProductRightEnum.ManagePortfolioManagementProductAccess.ToString(), StringComparer.OrdinalIgnoreCase);
        persona.hasManageIntegrationMarketplaceProductAccess = rights.Contains(ProductRightEnum.AccessIntegrationMarketplace.ToString(), StringComparer.OrdinalIgnoreCase);
        persona.hasManagePlatFormSecurity = rights.Contains(ProductRightEnum.ManagePlatFormSecurity.ToString(), StringComparer.OrdinalIgnoreCase);
        persona.hasManageCustomFields = rights.Contains(ProductRightEnum.ManageCustomFields.ToString(), StringComparer.OrdinalIgnoreCase);
        persona.hasManageUnifiedSettings = rights.Contains(ProductRightEnum.ManageUnifiedSettings.ToString(), StringComparer.OrdinalIgnoreCase);
        persona.hasManageClickPayProductAccess = rights.Contains(ProductRightEnum.ManageClickPayProductAccess.ToString(), StringComparer.OrdinalIgnoreCase);
        persona.hasManageDepositAlternativeProductAccess = rights.Contains(ProductRightEnum.ManageDepositAlternativeProductAccess.ToString(), StringComparer.OrdinalIgnoreCase);
        persona.hasManageSettingsTemplates = rights.Contains(ProductRightEnum.ManageSettingsTemplates.ToString(), StringComparer.OrdinalIgnoreCase);
        persona.hasnotificationsAccess = rights.Contains("ManageNotifications", StringComparer.OrdinalIgnoreCase);
        persona.hasManageIntelligentBuildingTrashProductAccess = rights.Contains(ProductRightEnum.ManageIntelligentBuildingTrashProductAccess.ToString(), StringComparer.OrdinalIgnoreCase);
        persona.hasManageIntelligentBuildingEnergyProductAccess = rights.Contains(ProductRightEnum.ManageIntelligentBuildingEnergyProductAccess.ToString(), StringComparer.OrdinalIgnoreCase);
        persona.hasManageIntelligentBuildingWaterProductAccess = rights.Contains(ProductRightEnum.ManageIntelligentBuildingWaterProductAccess.ToString(), StringComparer.OrdinalIgnoreCase);
        persona.hasManageHandsOnTrainingSystemAccess = rights.Contains(ProductRightEnum.ManageHandsOnTrainingSystemProductAccess.ToString(), StringComparer.OrdinalIgnoreCase);
        persona.hasManageLeaseLabsAccess = rights.Contains(ProductRightEnum.ManageLeaseLabsProductAccess.ToString(), StringComparer.OrdinalIgnoreCase);
        persona.hasManageLeadScoringAccess = rights.Contains(ProductRightEnum.ManageLeadScoringProductAccess.ToString(), StringComparer.OrdinalIgnoreCase);
        persona.hasPlatformAlertsAccess = rights.Contains("CreatePlatformAlerts", StringComparer.OrdinalIgnoreCase)
                                       || rights.Contains("ApprovePlatformAlerts", StringComparer.OrdinalIgnoreCase);
        persona.hasImportUsersAccess = rights.Contains("AbilityToImportUsers", StringComparer.OrdinalIgnoreCase);
        persona.hasManageSmartWasteCommercialProductAccess = rights.Contains(ProductRightEnum.ManageSmartWasteCommercialProductAccess.ToString(), StringComparer.OrdinalIgnoreCase);
        persona.hasManageAdminSupportPortalProductAccess = rights.Contains(ProductRightEnum.ManageAdminSupportPortalProductAccess.ToString(), StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Applies rights that apply to any authenticated user regardless of role.
    /// </summary>
    private static void ApplyAuthenticatedRights(Persona persona, List<string> rights)
    {
        persona.hasViewOnlySupportToolAccess = rights.Contains("ViewOnlySupportToolAccess", StringComparer.OrdinalIgnoreCase);
        persona.hasViewOnlySettingsAccess = rights.Contains("ViewUnifiedSettings", StringComparer.OrdinalIgnoreCase);
        persona.hasManageUnifiedSettings = rights.Contains("ManageUnifiedSetting", StringComparer.OrdinalIgnoreCase);
        persona.hasManageCustomFields = rights.Contains("ManageCustomFields", StringComparer.OrdinalIgnoreCase);
        persona.hasManagePlatFormSecurity = rights.Contains("ManagePlatFormSecurity", StringComparer.OrdinalIgnoreCase);
        persona.hasAccessSettingsAdmin = rights.Contains("AccessSettingsAdmin", StringComparer.OrdinalIgnoreCase);
        persona.hasManageSettingsTemplates = rights.Contains("ManageSettingsTemplates", StringComparer.OrdinalIgnoreCase);
        persona.hasnotificationsAccess = rights.Contains("ManageNotifications", StringComparer.OrdinalIgnoreCase);
        persona.hasPlatformAlertsAccess = rights.Contains("CreatePlatformAlerts", StringComparer.OrdinalIgnoreCase)
                                       || rights.Contains("ApprovePlatformAlerts", StringComparer.OrdinalIgnoreCase);
        persona.hasImportUsersAccess = rights.Contains("AbilityToImportUsers", StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Returns true when any settings rights are missing and therefore
    /// need to be re-checked against the impersonator's roles.
    /// </summary>
    private static bool NeedsImpersonatorRightsCheck(Persona persona)
        => !persona.hasViewOnlySettingsAccess
        || !persona.hasManageUnifiedSettings
        || !persona.hasManageCustomFields
        || !persona.hasManagePlatFormSecurity
        || !persona.hasManageSettingsTemplates;

    /// <summary>
    /// Supplements persona rights from the impersonator's role list.
    /// Replaces: <c>new UserRoleRightRepository()</c>, <c>new RPObjectCache()</c>,
    ///           <c>new SharedDataRepository()</c> inline in the old <c>AddRightsToPersona</c>.
    /// </summary>
    private async Task SupplementImpersonatorRightsAsync(
        Persona persona,
        Guid impersonatedBy,
        CancellationToken cancellationToken)
    {
        long activePersonaId = await GetActivePersonaIdAsync(impersonatedBy, cancellationToken);
        Persona impersonatorPersona = await GetPersonaAsync(activePersonaId, false, cancellationToken);

        if (impersonatorPersona is null)
        {
            _logger.LogWarning(
                "SupplementImpersonatorRights: no impersonator persona for ImpersonatedBy={Id}",
                impersonatedBy);
            return;
        }

        // Replaces: new UserRoleRightRepository().ListRoleByPersona(...)
        var userRoles = await _userRoleRightRepository.ListRoleByPersonaAsync(
            (int)ProductEnum.UnifiedPlatform,
            impersonatorPersona.PersonaId,
            impersonatorPersona.OrganizationPartyId);

        // Replaces: new RPObjectCache().GetFromCache(...) + new SharedDataRepository().GetProductIdsByCompany(...)
        var cacheKey = $"getRolesByParty_{impersonatorPersona.OrganizationPartyId}_{(int)ProductEnum.UnifiedPlatform}";

        IList<UserRoleRights> roleList = await _cache.GetOrSetAsync(
            cacheKey,
            async ct =>
            {
                // Replaces: new SharedDataRepository().GetProductIdsByCompany(orgPartyId)
                // Direct Dapper call removes the SharedDataRepository dependency
                var products = await _db.QueryAsync<int>(
                    new CommandDefinition(
                        StoredProcNameConstants.SP_ListProductsByOrganization,
                        new { PartyId = impersonatorPersona.OrganizationPartyId },
                        commandType: CommandType.StoredProcedure,
                        cancellationToken: ct));

                // Replaces: new UserRoleRightRepository().GetAllRoleRights(...)
                return await _userRoleRightRepository.GetAllRoleRightsAsync(
                    impersonatorPersona.OrganizationPartyId,
                    products.ToList(),
                    (int)ProductEnum.UnifiedPlatform);
            },
            RoleRightsTtlCacheOptions,
            cancellationToken);

        foreach (var userRole in userRoles)
        {
            var matchedRole = roleList.FirstOrDefault(r => r.RoleId == userRole.RoleID);
            if (matchedRole is null) continue;

            foreach (var right in matchedRole.UserRights)
            {
                if (string.IsNullOrEmpty(right.RightNickName)) continue;

                if (!persona.hasViewOnlySettingsAccess
                    && right.RightNickName.Equals("ViewUnifiedSettings", StringComparison.OrdinalIgnoreCase))
                    persona.hasViewOnlySettingsAccess = true;

                if (!persona.hasManageUnifiedSettings
                    && right.RightNickName.Equals("ManageUnifiedSetting", StringComparison.OrdinalIgnoreCase))
                    persona.hasManageUnifiedSettings = true;

                if (!persona.hasManageCustomFields
                    && right.RightNickName.Equals("ManageCustomFields", StringComparison.OrdinalIgnoreCase))
                    persona.hasManageCustomFields = true;

                if (!persona.hasManagePlatFormSecurity
                    && right.RightNickName.Equals("ManagePlatFormSecurity", StringComparison.OrdinalIgnoreCase))
                    persona.hasManagePlatFormSecurity = true;

                if (!persona.hasManageSettingsTemplates
                    && right.RightNickName.Equals("ManageSettingsTemplates", StringComparison.OrdinalIgnoreCase))
                    persona.hasManageSettingsTemplates = true;
            }
        }
    }

    #endregion
}