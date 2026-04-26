using Microsoft.Extensions.Logging;
using UnifiedLogin.BusinessLogic.Logic.ProductIntegration.Model;
using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects.BlackBook;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Saml;

namespace UnifiedLogin.BusinessLogic.LogicAsync.Product.Integrations;

/// <summary>
/// Native-async data-collector service. Replaces <c>DataCollector</c>.
/// <para>
/// <b>IDbConnectionFactory pattern</b>: all four repository fields that were previously
/// initialised with <c>new ProductRepository()</c> / <c>new SamlRepository()</c> etc.
/// are now injected via constructor. Each repository internally uses
/// <see cref="IDbConnectionFactory"/> to obtain short-lived <c>SqlConnection</c>s from
/// the ADO.NET pool — no long-lived connections are held.
/// </para>
/// <para>
/// <b>DefaultUserClaim removed</b>: <c>GetProductCompanyMapAsync</c> previously required
/// a <c>DefaultUserClaim</c> parameter to build a <c>ManageBlueBook</c> instance per call.
/// The caller context is now resolved from the ambient <see cref="IUserClaimsAccessor"/>
/// injected at construction time.
/// </para>
/// <para>
/// <b>Thread-safety</b>: no mutable instance state. Safe for <c>Scoped</c> DI lifetime.
/// </para>
/// </summary>
public sealed class DataCollectorAsync : IDataCollectorAsync
{
    private const string ProductStatusSettingType = "ProductStatus";

    // ── Dependencies ──────────────────────────────────────────────────────────
    private readonly IProductRepositoryAsync                _productRepository;
    private readonly ISamlRepositoryAsync                   _samlRepository;
    private readonly IUserRepositoryAsync                   _userRepository;
    private readonly ITelecommunicationNumberRepositoryAsync _telecomRepository;
    private readonly IManageBlueBookAsync                   _manageBlueBook;
    private readonly IUserClaimsAccessor                    _userClaimsAccessor;
    private readonly ILogger<DataCollectorAsync>             _logger;

    public DataCollectorAsync(
        IProductRepositoryAsync                productRepository,
        ISamlRepositoryAsync                   samlRepository,
        IUserRepositoryAsync                   userRepository,
        ITelecommunicationNumberRepositoryAsync telecomRepository,
        IManageBlueBookAsync                   manageBlueBook,
        IUserClaimsAccessor                    userClaimsAccessor,
        ILogger<DataCollectorAsync>             logger)
    {
        ArgumentNullException.ThrowIfNull(productRepository);
        ArgumentNullException.ThrowIfNull(samlRepository);
        ArgumentNullException.ThrowIfNull(userRepository);
        ArgumentNullException.ThrowIfNull(telecomRepository);
        ArgumentNullException.ThrowIfNull(manageBlueBook);
        ArgumentNullException.ThrowIfNull(userClaimsAccessor);
        ArgumentNullException.ThrowIfNull(logger);

        _productRepository  = productRepository;
        _samlRepository     = samlRepository;
        _userRepository     = userRepository;
        _telecomRepository  = telecomRepository;
        _manageBlueBook     = manageBlueBook;
        _userClaimsAccessor = userClaimsAccessor;
        _logger             = logger;
    }

    // ── GreenBook sync ────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task CreateProductUserInGreenBookAsync(
        long subjectPersonaId, dynamic userResult, int productId,
        IntegrationProductUser productUser, CancellationToken ct = default)
    {
        string newId            = userResult.userId  != null ? (string)userResult.userId  : (string)userResult.UserId;
        string newProductLogin  = userResult.loginName != null ? (string)userResult.loginName : productUser.LoginName;

        if (string.IsNullOrEmpty(newId))
            throw new InvalidOperationException($"Unable to get userId from product response. userResult={userResult}");

        var samlAttrs    = await _samlRepository.GetSamlProductAttributesAsync(productId, ct);
        var internalSettings = await _productRepository.GetProductInternalSettingsAsync(productId, ct);

        string? assignBySetting = internalSettings
            .FirstOrDefault(s => s.Name.Equals("AssignSamlAttributeBySetting", StringComparison.OrdinalIgnoreCase))
            ?.Value;

        if (assignBySetting?.Equals("1", StringComparison.OrdinalIgnoreCase) == true
            && samlAttrs is { Count: > 0 })
        {
            foreach (var attr in samlAttrs)
            {
                if (!Enum.TryParse<SamlAttributeEnum>(attr.SamlAttributeName, ignoreCase: true, out var samlEnum))
                    continue;

                string samlValue = ResolveAttributeValue(samlEnum, newId, newProductLogin, productUser, internalSettings);
                if (samlValue is null) continue;

                await CreateSamlUserAttributeAsync(subjectPersonaId, productId, samlEnum, samlValue, ct);
            }
        }
        else
        {
            await CreateSamlUserAttributeAsync(subjectPersonaId, productId, SamlAttributeEnum.productUsername, newProductLogin, ct);
            await CreateSamlUserAttributeAsync(subjectPersonaId, productId, SamlAttributeEnum.UserId,          newId,           ct);
        }

        await UpdateProductSettingProductStatusAsync(
            subjectPersonaId, ProductStatusSettingType, productId, (int)ProductBatchStatusType.Success, ct);
    }

    /// <inheritdoc/>
    public async Task UpdateProductUserInGreenBookAsync(
        long subjectPersonaId, dynamic userResult, int productId,
        IntegrationProductUser productUser, CancellationToken ct = default)
    {
        string newId           = userResult.userId   != null ? (string)userResult.userId   : (string)userResult.UserId;
        string newProductLogin = userResult.loginName != null ? (string)userResult.loginName : productUser.LoginName;

        if (string.IsNullOrEmpty(newId))
            throw new InvalidOperationException($"Unable to get userId from product response. userResult={userResult}");

        var samlAttrs        = await _samlRepository.GetSamlProductAttributesAsync(productId, ct);
        var internalSettings = await _productRepository.GetProductInternalSettingsAsync(productId, ct);

        if (samlAttrs is { Count: > 0 })
        {
            foreach (var attr in samlAttrs)
            {
                if (!Enum.TryParse<SamlAttributeEnum>(attr.SamlAttributeName, ignoreCase: true, out var samlEnum))
                    continue;

                string? samlValue = ResolveAttributeValue(samlEnum, newId, newProductLogin, productUser, internalSettings);
                if (samlValue is null) continue;

                await UpdateSamlUserAttributeAsync(subjectPersonaId, productId, samlEnum, samlValue, ct);
            }
        }
        else
        {
            await UpdateSamlUserAttributeAsync(subjectPersonaId, productId, SamlAttributeEnum.productUsername, newProductLogin, ct);
            await UpdateSamlUserAttributeAsync(subjectPersonaId, productId, SamlAttributeEnum.UserId,          newId,           ct);
        }
    }

    // ── Product / company map lookups ─────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<GbProductMap?> GetBlueBookProductMapAsync(
        int productId, CancellationToken ct = default)
        => await _productRepository.GetBooksMasterProductDetailAsync(productId, ct);

    /// <inheritdoc/>
    /// <remarks>
    /// <c>DefaultUserClaim</c> parameter removed — organisation GUID is resolved from
    /// the injected <see cref="IUserClaimsAccessor.OrganizationRealPageGuid"/>.
    /// </remarks>
    public async Task<CustomerCompanyMap?> GetProductCompanyMapAsync(
        string blueBookProductCode, int booksMasterId, string domain,
        CancellationToken ct = default)
    {
        try
        {
            var orgGuid = _userClaimsAccessor.OrganizationRealPageGuid;

            IList<CustomerCompanyMap> companyList = await _manageBlueBook.GetCompanyMapAsync(
                orgGuid, booksMasterId,
                source: blueBookProductCode.ToUpperInvariant(),
                domain: domain, cancellationToken: ct);

            companyList ??= [];

            return companyList.FirstOrDefault(
                       a => a.Source.Equals(blueBookProductCode, StringComparison.OrdinalIgnoreCase))
                   ?? new CustomerCompanyMap();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Action} - BlueBook lookup failed. ProductCode={Code}",
                nameof(GetProductCompanyMapAsync), blueBookProductCode);
            throw;
        }
    }

    // ── User lookups ──────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<UserDetails?> GetUserDetailsByPersonaAsync(
        long personaId, int productId, CancellationToken ct = default)
    {
        var userDetails = await _userRepository.GetUserDetailsAsync(personaId: personaId, cancellationToken: ct);
        if (userDetails is null) return null;

        var phoneNumbers = await _telecomRepository.ListTelecommunicationNumberForPersonAsync(
            userDetails.UserRealPageId, contactMechanismUsageTypeName: string.Empty, cancellationToken: ct);

        userDetails.PhoneNumbers = phoneNumbers
            .Select(x => $"{x.AreaCode}{x.PhoneNumber}")
            .ToList();

        await EnrichWithSamlDetailsAsync(userDetails, productId, ct);

        return userDetails;
    }

    /// <inheritdoc/>
    public async Task<AdUserDetail?> GetAzureUserDetailsAsync(
        long userId, CancellationToken ct = default)
        => await _userRepository.GetAzureUserDetailsAsync(userId, ct);

    // ── Product settings & SAML attributes ───────────────────────────────────

    /// <inheritdoc/>
    public async Task UpdateProductSettingProductStatusAsync<T>(
        long subjectPersonaId, string settingType, int productId, T value,
        CancellationToken ct = default)
    {
        var settingTypes = await _productRepository.ListProductSettingTypeAsync(ct);

        var match = settingTypes.FirstOrDefault(
            s => s.Name.Equals(settingType, StringComparison.OrdinalIgnoreCase));

        if (match is null)
        {
            _logger.LogWarning("{Action} - ProductSettingType '{Type}' not found; skipping.",
                nameof(UpdateProductSettingProductStatusAsync), settingType);
            return;
        }

        await _productRepository.CreateProductSettingAsync(
            subjectPersonaId, productId, match.ProductSettingTypeId,
            value?.ToString() ?? string.Empty, ct);
    }

    /// <inheritdoc/>
    public async Task UpdateSamlUserAttributeAsync(
        long personaId, int productId, SamlAttributeEnum attributeType,
        string newValue, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(newValue))
            throw new ArgumentException("SAML attribute value must not be null or empty.", nameof(newValue));

        await UpdateSamlUserAttributesAsync(
            personaId, new Dictionary<SamlAttributeEnum, string> { [attributeType] = newValue },
            productId, ct);
    }

    /// <inheritdoc/>
    public async Task CreateSamlUserAttributeAsync(
        long subjectPersonaId, int productId, SamlAttributeEnum samlAttributeEnum,
        string value, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(value))
            throw new ArgumentException("SAML attribute value must not be null or empty.", nameof(value));

        await _samlRepository.CreateSamlUserAttributeAsync(subjectPersonaId, productId, samlAttributeEnum, value, ct);
    }

    // ── Employee / AD group mapping ───────────────────────────────────────────

    /// <inheritdoc/>
    public async Task AddUpdateEmployeeProductADGroupMappingAsync(
        long personaId, int productId, int adGroupId, CancellationToken ct = default)
        => await _userRepository.AddUpdateEmployeeProductADGroupMappingAsync(personaId, productId, adGroupId, ct);

    /// <inheritdoc/>
    public async Task<IList<EmployeeProductMapping>> GetEmployeeProductADGroupMappingAsync(
        long personaId, int productId, CancellationToken ct = default)
        => await _userRepository.GetEmployeeProductADGroupMappingAsync(personaId, productId, ct);

    // ── Private helpers ───────────────────────────────────────────────────────

    /// <summary>
    /// Resolves the SAML attribute value for a given <paramref name="samlEnum"/> using
    /// the product user result and internal settings. Returns <c>null</c> when the
    /// required setting is missing and the attribute should be skipped.
    /// </summary>
    /// <remarks>
    /// Extracted from the duplicated per-attr blocks in <c>CreateProductUserInGreenBook</c>
    /// and <c>UpdateProductUserInGreenBook</c> — reduces the method bodies from ~40 lines
    /// each to a single <c>foreach</c> loop.
    /// </remarks>
    private static string? ResolveAttributeValue(
        SamlAttributeEnum samlEnum,
        string userId,
        string loginName,
        IntegrationProductUser productUser,
        IList<ProductInternalSetting> settings) => samlEnum switch
    {
        SamlAttributeEnum.productUsername => loginName,
        SamlAttributeEnum.UserId          => userId,
        SamlAttributeEnum.RoleCode        => productUser.RoleType,
        SamlAttributeEnum.organization_id =>
            settings.FirstOrDefault(s => s.Name.Equals("OrganizationId", StringComparison.OrdinalIgnoreCase))?.Value,
        SamlAttributeEnum.portal_id =>
            settings.FirstOrDefault(s => s.Name.Equals("PortalId", StringComparison.OrdinalIgnoreCase))?.Value,
        _ => null   // unknown attribute — caller will skip via null check
    };

    /// <summary>
    /// Applies SAML attribute updates from <paramref name="settingList"/> in a single
    /// async iteration over the persona's existing attributes.
    /// Replaces the private sync <c>UpdateSamlUserAttributes</c>.
    /// </summary>
    private async Task UpdateSamlUserAttributesAsync(
        long personaId,
        Dictionary<SamlAttributeEnum, string> settingList,
        int productId,
        CancellationToken ct)
    {
        if (settingList.Count == 0) return;

        var existing = await _samlRepository.GetProductSamlDetailsAsync(personaId, productId, ct);

        foreach (var (attrEnum, value) in settingList)
        {
            var match = existing.FirstOrDefault(a => a.SamlAttributeId == (int)attrEnum);
            if (match is null) continue;

            match.Value = value;
            await _samlRepository.UpdateSamlUserAttributeAsync(match, ct);
        }
    }

    /// <summary>
    /// Enriches <paramref name="userDetails"/> with product-specific SAML fields
    /// (<c>ProductUserName</c>, <c>ProductUserId</c>).
    /// </summary>
    private async Task EnrichWithSamlDetailsAsync(
        UserDetails userDetails, int productId, CancellationToken ct)
    {
        var attrs = await _samlRepository.GetProductSamlDetailsAsync(userDetails.PersonaId, productId, ct);
        if (attrs is not { Count: > 0 }) return;

        userDetails.ProductUserName = attrs
            .FirstOrDefault(a => a.Name.Equals("ProductUserName", StringComparison.OrdinalIgnoreCase))
            ?.Value;

        userDetails.ProductUserId = attrs
            .FirstOrDefault(a => a.Name.Equals("UserId", StringComparison.OrdinalIgnoreCase))
            ?.Value;
    }
}
