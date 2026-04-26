using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Xml;
using Microsoft.IdentityModel.Tokens;
using Microsoft.IdentityModel.Tokens.Saml2;
using UnifiedLogin.BusinessLogic.Logic.Product.SAML;
using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Saml;

namespace UnifiedLogin.BusinessLogic.LogicAsync.SAML;

/// <summary>
/// Native-async SAML orchestrator. Replaces <see cref="RealPageSAML"/>.
/// <para>
/// Thread-safe: no mutable instance fields — all per-call state is local.
/// Product SAML settings are cached in <see cref="IMemoryCache"/> (10-minute TTL,
/// replacing the legacy <c>RPObjectCache</c> with 600-second TTL).
/// </para>
/// <para>
/// <c>BuildAssertion</c> is extracted to a pure <c>static</c> method — it only
/// performs in-memory XML manipulation and signing, so it stays synchronous.
/// </para>
/// </summary>
public sealed class RealPageSamlAsync : IRealPageSamlAsync
{
    // ── Constants ─────────────────────────────────────────────────────────────
    private const string SettingsCacheKeyPrefix = "samlSettings_";
    private static readonly TimeSpan SettingsCacheTtl = TimeSpan.FromMinutes(10);
    private const string Issuer = "GreenBook";

    // ── Dependencies ──────────────────────────────────────────────────────────
    private readonly ISamlAttributeServiceAsync              _samlAttrService;
    private readonly ISamlRepositoryAsync                    _samlRepository;
    private readonly IManagePersonaAsync                     _managePersona;
    private readonly IManageProductAsync                     _manageProduct;
    private readonly IManageProductOneSiteAsync              _manageOneSite;
    private readonly IManageProductRPDocumentManagementAsync _manageDocMgt;
    private readonly IBatchProductBulkUpdateRepositoryAsync  _batchRepository;
    private readonly IManageOrganizationAsync                _manageOrganization;
    private readonly IUserClaimsAccessor                     _userClaimsAccessor;
    private readonly IMemoryCache                            _cache;
    private readonly ILogger<RealPageSamlAsync>              _logger;

    public RealPageSamlAsync(
        ISamlAttributeServiceAsync              samlAttrService,
        ISamlRepositoryAsync                    samlRepository,
        IManagePersonaAsync                     managePersona,
        IManageProductAsync                     manageProduct,
        IManageProductOneSiteAsync              manageOneSite,
        IManageProductRPDocumentManagementAsync manageDocMgt,
        IBatchProductBulkUpdateRepositoryAsync  batchRepository,
        IManageOrganizationAsync                manageOrganization,
        IUserClaimsAccessor                     userClaimsAccessor,
        IMemoryCache                            cache,
        ILogger<RealPageSamlAsync>              logger)
    {
        ArgumentNullException.ThrowIfNull(samlAttrService);
        ArgumentNullException.ThrowIfNull(samlRepository);
        ArgumentNullException.ThrowIfNull(managePersona);
        ArgumentNullException.ThrowIfNull(manageProduct);
        ArgumentNullException.ThrowIfNull(manageOneSite);
        ArgumentNullException.ThrowIfNull(manageDocMgt);
        ArgumentNullException.ThrowIfNull(batchRepository);
        ArgumentNullException.ThrowIfNull(manageOrganization);
        ArgumentNullException.ThrowIfNull(userClaimsAccessor);
        ArgumentNullException.ThrowIfNull(cache);
        ArgumentNullException.ThrowIfNull(logger);

        _samlAttrService    = samlAttrService;
        _samlRepository     = samlRepository;
        _managePersona      = managePersona;
        _manageProduct      = manageProduct;
        _manageOneSite      = manageOneSite;
        _manageDocMgt       = manageDocMgt;
        _batchRepository    = batchRepository;
        _manageOrganization = manageOrganization;
        _userClaimsAccessor = userClaimsAccessor;
        _cache              = cache;
        _logger             = logger;
    }

    // ── Public interface ──────────────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<IList<SamlAttributes>> CreateUserBatchIfRequiredAsync(
        long targetPersonaId, int productId,
        CancellationToken ct = default)
    {
        _logger.LogDebug("{Action} - TargetPersonaId={Id} ProductId={Pid}",
            nameof(CreateUserBatchIfRequiredAsync), targetPersonaId, productId);

        var settings = await _manageProduct.GetProductInternalSettingsAsync(productId, ct);

        // Parse IsUserCreationOnTileClick
        bool isUserCreationRequired =
            settings.FirstOrDefault(s => s.Name.Equals("IsUserCreationOnTileClick", StringComparison.OrdinalIgnoreCase))
                    ?.Value is string raw && bool.TryParse(raw, out bool parsed) && parsed;

        var samlDetails = await _samlAttrService.GetProductSamlDetailsAsync(targetPersonaId, productId, ct);

        if (samlDetails.Count == 0 && isUserCreationRequired)
        {
            int retryCheckCount  = 5;
            int statusCheckSleep = 5_000;

            if (settings.FirstOrDefault(s => s.Name.Equals("BatchUserProductStatusRetryCount", StringComparison.OrdinalIgnoreCase))
                        ?.Value is string retrySetting)
                int.TryParse(retrySetting, out retryCheckCount);

            if (settings.FirstOrDefault(s => s.Name.Equals("BatchUserProductStatusSleepTimeout", StringComparison.OrdinalIgnoreCase))
                        ?.Value is string sleepSetting)
                int.TryParse(sleepSetting, out statusCheckSleep);

            string defaultUserRoleId = settings
                .FirstOrDefault(s => s.Name.Equals("DefaultUserRoleId", StringComparison.OrdinalIgnoreCase))
                ?.Value ?? string.Empty;

            // Org GUID and impersonator come from the per-request IUserClaimsAccessor
            Guid orgRealPageId = _userClaimsAccessor.OrganizationRealPageGuid;
            if (orgRealPageId == Guid.Empty)
            {
                _logger.LogError("{Action} - Organization not found in claims", nameof(CreateUserBatchIfRequiredAsync));
                return [];
            }

            Guid editorRealPageId    = await _manageOrganization.GetOrganizationAdminUserRealPageIdAsync(orgRealPageId, ct);
            long editorUserPersonaId = await _managePersona.GetActivePersonaIdAsync(editorRealPageId, ct);

            // Impersonator: use 0 when not impersonating (UserId from claims is the caller, not impersonator)
            long impersonatorUserId = _userClaimsAccessor.ImpersonatedBy != Guid.Empty
                ? _userClaimsAccessor.UserId   // claims UserId doubles as impersonator marker when present
                : 0L;

            _logger.LogDebug("{Action} - Starting batch. EditorPersonaId={Eid} TargetPersonaId={Tid} ProductId={Pid}",
                nameof(CreateUserBatchIfRequiredAsync), editorUserPersonaId, targetPersonaId, productId);

            return await _batchRepository.CreateBatchAsync(
                editorUserPersonaId, targetPersonaId, editorRealPageId,
                productId, retryCheckCount, statusCheckSleep,
                defaultUserRoleId, impersonatorUserId, ct);
        }

        return samlDetails;
    }

    /// <inheritdoc/>
    public async Task<SamlProductLoginResponse> GetProductDetailsSamlAsync(
        string unifiedLoginUri, int productId,
        long targetPersonaId,
        string userToken,
        string relayStateSamlAttribute = "", string fallBackUrl = "",
        bool isProductReport = false, string reportParams = "",
        CancellationToken ct = default)
    {
        _logger.LogDebug("{Action} - ProductId={Pid} TargetPersonaId={Tid}",
            nameof(GetProductDetailsSamlAsync), productId, targetPersonaId);

        var response = new SamlProductLoginResponse();

        // ── Rights check (ViewOnlySupportToolAccess blocks SAML login) ─────────
        if (_userClaimsAccessor.Rights.Any(p => p.Equals("ViewOnlySupportToolAccess", StringComparison.OrdinalIgnoreCase)))
        {
            response.ErrorMessage = "AccessDenied";
            return response;
        }

        // ── Resolve target persona ─────────────────────────────────────────────
        var persona = await ResolvePersonaAsync(_userClaimsAccessor.UserRealPageGuid, targetPersonaId, ct);
        if (persona is null)
        {
            response.ErrorMessage = "Invalid persona";
            return response;
        }

        // ── Product SAML settings ──────────────────────────────────────────────
        var productSamlSettings = await GetProductSamlSettingsAsync(productId, ct);
        string samlEndpointUrl  = productSamlSettings.LoginUri;

        // ── Product details + flags ────────────────────────────────────────────
        var details = await GetProductDetailsAsync(productId, persona.PersonaId, ct);
        if (details.HasError)
        {
            response.ErrorMessage = "There was a problem getting the product details";
            return response;
        }

        if (details.ProductList.Count == 0)
        {
            if (string.IsNullOrEmpty(fallBackUrl))
            {
                response.ErrorMessage = "Invalid product id or no product found";
                return response;
            }
            response.RedirectUrl = fallBackUrl;
            response.IsRedirect  = true;
            return response;
        }

        // ── Resolve which product detail record to use ─────────────────────────
        PersonaProductUserDetails productDetail;
        if (persona.PersonaId == 0)
        {
            productDetail = details.ProductList[0];
        }
        else
        {
            productDetail = details.ProductList.FirstOrDefault(p => p.PersonaId == persona.PersonaId)
                         ?? new PersonaProductUserDetails();
        }

        if (productDetail.PersonaId == 0)
        {
            response.ErrorMessage = "Invalid product id or no product found";
            return response;
        }

        long resolvedPersonaId = productDetail.PersonaId;

        // ── Status check ───────────────────────────────────────────────────────
        if (productDetail.ProductStatus != (int)ProductBatchStatusType.Success)
        {
            response.IsRedirect  = true;
            response.RedirectUrl = $"{unifiedLoginUri}error/401";
            return response;
        }

        // ── Product alias mapping ──────────────────────────────────────────────
        int resolvedProductId = productId switch
        {
            (int)ProductEnum.UnifiedUI         => (int)ProductEnum.OneSite,
            (int)ProductEnum.OneSiteConversions => (int)ProductEnum.OneSite,
            (int)ProductEnum.PropertyPhotos     => (int)ProductEnum.MarketingCenter,
            _                                   => productId
        };

        // ── SAML attribute list ────────────────────────────────────────────────
        var samlList = await _samlAttrService.GetProductSamlDetailsAsync(resolvedPersonaId, resolvedProductId, ct);

        // ── OneSite PMC URL rewrite ────────────────────────────────────────────
        if (details.GetOneSitePmcUrl)
            samlEndpointUrl = await GetOneSitePmcUrlAsync(samlEndpointUrl, samlList, ct);

        // ── Doc Management domain rewrite ──────────────────────────────────────
        if (details.GetDocMgtDomain)
            samlEndpointUrl = await GetDocManagementDomainUrlAsync(samlEndpointUrl, resolvedPersonaId, samlList, ct);

        // ── Marketing Center: inject redirect URL + switch endpoint ────────────
        if (details.GetMarketingCenterUrl)
        {
            var mcSettings = await GetProductSamlSettingsAsync((int)ProductEnum.MarketingCenter, ct);
            samlList = [.. samlList, new SamlAttributes
            {
                Name = "RedirectUrl", SamlAttributeId = 0,
                Type = RealPageSAML.AttributeURIs.Basic, Value = samlEndpointUrl
            }];
            samlEndpointUrl = mcSettings.LoginUri;
        }

        // ── Build SAML assertion ───────────────────────────────────────────────
        response.SamlResponse = await GetSamlDetailsAsync(
            unifiedLoginUri, resolvedProductId, userToken,
            productSamlSettings.SigningCertificateThumbprint, Issuer,
            productSamlSettings.SubjectIdSamlAttribute,
            relayStateSamlAttribute, samlEndpointUrl, samlList,
            reportParams, isProductReport, ct);

        response.IsSaml = true;
        return response;
    }

    /// <inheritdoc/>
    public async Task<SamlGeneratedResponse> GetSamlDetailsAsync(
        string unifiedLoginUri, int productId, string userToken,
        string signingCertThumbprint, string issuer,
        string samlSubjectAttributeName, string samlRelayAttributeName,
        string destination, IList<SamlAttributes> samlList,
        string reportParams = "", bool isProductReport = false,
        CancellationToken ct = default)
    {
        string callerGuid  = _userClaimsAccessor.UserRealPageGuid.ToString();
        string callerLogin = _userClaimsAccessor.LoginName;

        var productInternalSettings = await _manageProduct.GetProductInternalSettingsAsync(productId, ct);

        // Build the full attribute list (add enterprise + product-specific attrs)
        List<SamlAttributes> fullList =
        [
            .. samlList,
            new() { Name = "EnterpriseUserId", SamlAttributeId = 0, Type = RealPageSAML.AttributeURIs.Basic, Value = callerGuid },
            new() { Name = "EnterpriseLogin",  SamlAttributeId = 0, Type = RealPageSAML.AttributeURIs.Basic, Value = callerLogin },
            new() { Name = "GreenBookUrl",     SamlAttributeId = 0, Type = RealPageSAML.AttributeURIs.Basic, Value = unifiedLoginUri },
            new() { Name = "GreenBookToken",   SamlAttributeId = 0, Type = RealPageSAML.AttributeURIs.Basic, Value = userToken },
        ];

        if (ProductEnumHelper.GetAoProductList().Contains((ProductEnum)productId))
            fullList.Add(new() { Name = "Product", SamlAttributeId = 0, Type = RealPageSAML.AttributeURIs.Basic, Value = ProductEnumHelper.GetAoProductId((ProductEnum)productId) });

        if (isProductReport)
        {
            fullList.Add(new() { Name = "Product",      SamlAttributeId = 0, Type = RealPageSAML.AttributeURIs.Basic, Value = "ProductReport" });
            fullList.Add(new() { Name = "ReportParams", SamlAttributeId = 0, Type = RealPageSAML.AttributeURIs.Basic, Value = reportParams });
        }

        // Resolve SAML subject
        string samlSubject = FindAttributeValue(fullList, samlSubjectAttributeName)
            ?? (string.IsNullOrEmpty(callerLogin)
                ? throw new InvalidOperationException("Empty SAML Subject")
                : callerLogin);

        // Resolve relay state
        string samlRelay = string.Empty;
        if (!string.IsNullOrEmpty(samlRelayAttributeName))
        {
            string? rawRelay = FindAttributeValue(fullList, samlRelayAttributeName);
            if (rawRelay is not null)
                samlRelay = productId == (int)ProductEnum.FinancialSuite
                    ? rawRelay.Replace("|", ":")
                    : rawRelay;
        }

        X509Certificate2 signingCert = GetSigningCertificate(signingCertThumbprint);
        XmlDocument responseXml      = BuildAssertion(issuer, samlSubject, destination, productId, fullList, signingCert, productInternalSettings);

        return new SamlGeneratedResponse
        {
            SamlBase64Encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(responseXml.OuterXml)),
            RelayState        = samlRelay,
            Destination       = destination
        };
    }

    /// <inheritdoc/>
    public async Task<SamlProductDetailsResult> GetProductDetailsAsync(
        int productId, long personaId, CancellationToken ct = default)
    {
        bool getOneSitePmcUrl    = false;
        bool getDocMgtDomain     = false;
        bool getMarketingCenterUrl = false;
        string? productType      = null;

        // Normalise productId + set flags
        int resolvedProductId = productId switch
        {
            (int)ProductEnum.OneSite             => productId,   // getOneSitePmcUrl below
            (int)ProductEnum.UnifiedUI           => (int)ProductEnum.OneSite,
            (int)ProductEnum.OneSiteConversions  => (int)ProductEnum.OneSite,
            (int)ProductEnum.ClientPortal        => productId,
            (int)ProductEnum.ResearchApplication => productId,
            (int)ProductEnum.MigrationTool       => productId,
            (int)ProductEnum.IntegrationMarketplace => productId,
            (int)ProductEnum.RPDocumentManagement => productId,
            (int)ProductEnum.PropertyPhotos      => (int)ProductEnum.MarketingCenter,
            (int)ProductEnum.AdminSupportPortal  => productId,
            _                                    => productId
        };

        switch (productId)
        {
            case (int)ProductEnum.OneSite:
                getOneSitePmcUrl = true;
                break;
            case (int)ProductEnum.ClientPortal:
            case (int)ProductEnum.ResearchApplication:
            case (int)ProductEnum.MigrationTool:
            case (int)ProductEnum.AdminSupportPortal:
                productType = "IsResource";
                break;
            case (int)ProductEnum.IntegrationMarketplace:
                productType = "IsResource";
                break;
            case (int)ProductEnum.RPDocumentManagement:
                getDocMgtDomain = true;
                break;
            case (int)ProductEnum.PropertyPhotos:
                getMarketingCenterUrl = true;
                break;
        }

        IList<PersonaProductUserDetails> productListAll = [];
        IList<PersonaProductUserDetails> productList    = [];

        try
        {
            productListAll = await _samlRepository.ListAllProductsByPersonaIdAsync(personaId, resolvedProductId, productType ?? string.Empty, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Action} - Error listing products. PersonaId={Id}", nameof(GetProductDetailsAsync), personaId);
            return new SamlProductDetailsResult(getOneSitePmcUrl, getDocMgtDomain, getMarketingCenterUrl, [], HasError: true);
        }

        if (productListAll.Any(a => a.ProductId == resolvedProductId))
            productList = [productListAll.First(a => a.ProductId == resolvedProductId)];

        return new SamlProductDetailsResult(getOneSitePmcUrl, getDocMgtDomain, getMarketingCenterUrl, productList);
    }

    // ── Private — persona resolution ──────────────────────────────────────────

    private async Task<Persona?> ResolvePersonaAsync(Guid callerRealPageId, long targetPersonaId, CancellationToken ct)
    {
        if (targetPersonaId == 0)
        {
            // Same-user login: resolve caller's own active persona (no rights needed)
            return await _managePersona.GetActivePersonaWithoutRightsAsync(callerRealPageId, ct);
        }

        try
        {
            var persona = await _managePersona.GetPersonaAsync(targetPersonaId, withRights: false, cancellationToken: ct);
            bool hasImpersonate = _userClaimsAccessor.Rights
                .Any(p => p.Equals("AccessToUnifiedPlatform", StringComparison.OrdinalIgnoreCase));

            if (persona is null || (persona.RealPageId != callerRealPageId && !hasImpersonate))
                return null;

            return persona;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "{Action} - Persona resolution failed. TargetPersonaId={Id}", nameof(ResolvePersonaAsync), targetPersonaId);
            return null;
        }
    }

    // ── Private — product SAML settings cache ─────────────────────────────────

    /// <summary>
    /// Loads and caches product SAML settings.
    /// Replaces <c>RPObjectCache</c> with <see cref="IMemoryCache"/> (10-min TTL).
    /// AO products are always looked up under the <c>AssetOptimizer</c> product ID.
    /// </summary>
    private async ValueTask<ProductSamlSettings> GetProductSamlSettingsAsync(int productId, CancellationToken ct)
    {
        int lookupId = ProductEnumHelper.GetAoProductList().Contains((ProductEnum)productId)
            ? (int)ProductEnum.AssetOptimizer
            : productId;

        string cacheKey = $"{SettingsCacheKeyPrefix}{lookupId}";
        if (_cache.TryGetValue(cacheKey, out ProductSamlSettings? hit)) return hit!;

        var settings = await _samlRepository.GetProductSamlSettingsByProductIdAsync(lookupId, ct);
        _cache.Set(cacheKey, settings, SettingsCacheTtl);
        return settings;
    }

    // ── Private — OneSite PMC URL rewrite ─────────────────────────────────────

    /// <summary>
    /// Rewrites <paramref name="samlEndpointUrl"/> to use the OneSite PMC server hostname.
    /// The PMC ID is taken from the <c>UserId</c> SAML attribute (first segment before <c>|</c>).
    /// </summary>
    private async Task<string> GetOneSitePmcUrlAsync(
        string samlEndpointUrl, IList<SamlAttributes> samlList, CancellationToken ct)
    {
        string? userIdAttr = FindAttributeValue(samlList, "UserId");
        if (string.IsNullOrEmpty(userIdAttr)) return samlEndpointUrl;
        if (!int.TryParse(userIdAttr.Split('|')[0], out int pmcId)) return samlEndpointUrl;

        var pmcInfo = await _manageOneSite.GetPmcInfoAsync(pmcId, ct);
        if (pmcInfo?.PMCURL is null) return samlEndpointUrl;

        var samlUri = new Uri(samlEndpointUrl);
        return $"{samlUri.Scheme}://{pmcInfo.PMCURL}{samlUri.PathAndQuery}";
    }

    // ── Private — Doc Management domain rewrite ───────────────────────────────

    private async Task<string> GetDocManagementDomainUrlAsync(
        string samlEndpointUrl, long personaId, IList<SamlAttributes> samlList, CancellationToken ct)
    {
        if (!samlList.Any(a => a.Name.Equals("UserId", StringComparison.OrdinalIgnoreCase)))
            return samlEndpointUrl;

        var domainResult = await _manageDocMgt.GetDomainAsync(personaId, ct);
        if (domainResult.Additional is not null)
            samlEndpointUrl = samlEndpointUrl.Replace("{{domain}}", domainResult.Additional.ToString());

        return samlEndpointUrl;
    }

    // ── Private — certificate lookup ──────────────────────────────────────────

    /// <summary>
    /// Locates a signing certificate in <c>LocalMachine\My</c> by thumbprint.
    /// Kept synchronous — <see cref="X509Store"/> has no async API.
    /// </summary>
    private X509Certificate2 GetSigningCertificate(string thumbprint)
    {
        using var certStore = new X509Store(StoreName.My, StoreLocation.LocalMachine);
        certStore.Open(OpenFlags.ReadOnly | OpenFlags.IncludeArchived);

        var found = certStore.Certificates.Find(X509FindType.FindByThumbprint, thumbprint, validOnly: false);
        if (found.Count > 0)
        {
            _logger.LogDebug("{Action} - Certificate found. Thumbprint={Thumbprint}", nameof(GetSigningCertificate), thumbprint);
            return found[0];
        }

        _logger.LogError("{Action} - Certificate not found. Thumbprint={Thumbprint}", nameof(GetSigningCertificate), thumbprint);
        throw new InvalidOperationException($"No certificate found for thumbprint '{thumbprint}'.");
    }

    // ── Private — attribute lookup helper ─────────────────────────────────────

    private static string? FindAttributeValue(IList<SamlAttributes> list, string name)
        => list.FirstOrDefault(a => a.Name.Equals(name, StringComparison.OrdinalIgnoreCase))?.Value;

    // ── Private — pure SAML XML builder (static, synchronous) ────────────────

    /// <summary>
    /// Builds and signs the full SAML 2.0 response <see cref="XmlDocument"/>.
    /// This is CPU-bound in-memory work (SAML2 token generation + XML signing);
    /// no I/O is performed, so it remains synchronous.
    /// <para>
    /// Extracted from the original instance method <c>BuildAssertion()</c> — all
    /// previously-mutable private properties are now explicit parameters, making
    /// the method pure and thread-safe.
    /// </para>
    /// </summary>
    private static XmlDocument BuildAssertion(
        string issuer, string subject, string destination, int productId,
        IList<SamlAttributes> attributeList,
        X509Certificate2 signingCertificate,
        List<ProductInternalSetting> productInternalSettings)
    {
        DateTime issueInstant = DateTime.UtcNow;

        // ── Build Saml2Assertion ───────────────────────────────────────────────
        var assertion = new Saml2Assertion(new Saml2NameIdentifier(issuer, new Uri(RealPageSAML.AssertionUri)))
        {
            Id           = new Saml2Id(),
            IssueInstant = issueInstant,
            Issuer       = new Saml2NameIdentifier(issuer),
            Conditions   = new Saml2Conditions
            {
                NotBefore    = issueInstant.AddHours(-1),
                NotOnOrAfter = issueInstant.AddHours(1)
            }
        };

        // SalesForce-specific subject + audience (ClientPortal / AdminSupportPortal)
        bool isSalesForce = productId is (int)ProductEnum.ClientPortal or (int)ProductEnum.AdminSupportPortal;
        if (isSalesForce)
        {
            assertion.Subject = new Saml2Subject(new Saml2NameIdentifier(subject, new Uri(RealPageSAML.NameIDFormatUris.Email)));

            var conf = new Saml2SubjectConfirmation(new Uri("urn:oasis:names:tc:SAML:2.0:cm:bearer"))
            {
                NameIdentifier = new Saml2NameIdentifier(subject)
                {
                    Format = new Uri(RealPageSAML.NameIDFormatUris.Unspecified)
                },
                SubjectConfirmationData = new Saml2SubjectConfirmationData
                {
                    Recipient    = new Uri(productInternalSettings.First(s => s.Name.ToUpper() == "SAMLRECIPIENT").Value),
                    NotOnOrAfter = issueInstant.AddHours(1)
                }
            };
            assertion.Subject.SubjectConfirmations.Add(conf);

            var sfAudience = new Saml2AudienceRestriction("https://saml.salesforce.com");
            assertion.Conditions.AudienceRestrictions.Add(sfAudience);
        }
        else
        {
            assertion.Subject = new Saml2Subject(new Saml2NameIdentifier(subject, new Uri(RealPageSAML.NameIDFormatUris.Unspecified)));
        }

        // Authentication statement
        var authn = new Saml2AuthenticationStatement(new Saml2AuthenticationContext(new Uri(RealPageSAML.PasswordUri)))
        {
            SessionIndex = Guid.NewGuid().ToString()
        };
        assertion.Statements.Add(authn);

        // Attribute statement
        var samlAttrs = attributeList
            .Select(a => new Saml2Attribute(a.Name, a.Value)
            {
                NameFormat   = new Uri(a.Type),
                FriendlyName = a.Name
            })
            .ToList();
        assertion.Statements.Add(new Saml2AttributeStatement(samlAttrs));

        // Sign using SHA-256
        assertion.SigningCredentials = new X509SigningCredentials(signingCertificate, SecurityAlgorithms.Sha256);

        // ── Write token to XML ─────────────────────────────────────────────────
        var handler = new Saml2SecurityTokenHandler();
        var sw      = new System.IO.StringWriter();
        handler.WriteToken(new XmlTextWriter(sw) { Namespaces = true }, new Saml2SecurityToken(assertion));
        var assertionDoc = new XmlDocument();
        assertionDoc.LoadXml(sw.ToString());
        AddPrefix(assertionDoc.DocumentElement!, "saml");

        // ── Build Response envelope ────────────────────────────────────────────
        var responseDoc     = new XmlDocument();
        var responseElement = responseDoc.CreateElement(RealPageSAML.Prefixes.SAMLP, "Response", RealPageSAML.NamespaceURIs.Protocol);
        string responseId   = new Saml2Id().Value;

        responseElement.SetAttribute("ID",           responseId);
        responseElement.SetAttribute("Version",      RealPageSAML.Version);
        responseElement.SetAttribute("IssueInstant", issueInstant.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"));
        responseElement.SetAttribute("Destination",  destination);

        var issuerEl = responseDoc.CreateElement(RealPageSAML.Prefixes.SAML, "Issuer", RealPageSAML.NamespaceURIs.Assertion);
        issuerEl.InnerText = issuer;
        responseElement.AppendChild(issuerEl);

        var statusEl     = responseDoc.CreateElement(RealPageSAML.Prefixes.SAMLP, "Status",     RealPageSAML.NamespaceURIs.Protocol);
        var statusCodeEl = responseDoc.CreateElement(RealPageSAML.Prefixes.SAMLP, "StatusCode", RealPageSAML.NamespaceURIs.Protocol);
        statusCodeEl.SetAttribute("Value", RealPageSAML.StatusUris.Success);
        statusEl.AppendChild(statusCodeEl);
        responseElement.AppendChild(statusEl);

        // Remove the auto-generated Saml2Assertion signature (will be re-signed correctly below)
        var nsmgr = new XmlNamespaceManager(assertionDoc.NameTable);
        nsmgr.AddNamespace("sig", RealPageSAML.NamespaceURIs.Signature);
        var existingSig = assertionDoc.SelectSingleNode("//sig:Signature", nsmgr);
        if (existingSig is not null)
            assertionDoc.DocumentElement!.RemoveChild(existingSig);

        var importedAssertion = responseElement.OwnerDocument!.ImportNode(assertionDoc.DocumentElement!, deep: true);
        responseElement.AppendChild(importedAssertion);
        responseDoc.AppendChild(responseElement);

        // ── Sign the full Response element ─────────────────────────────────────
        var reference = new Reference { Uri = $"#{responseId}" };

        var signedXml = new SignedXml(responseDoc)
        {
            SigningKey = signingCertificate.GetRSAPrivateKey()
        };
        signedXml.SignedInfo.CanonicalizationMethod = SignedXml.XmlDsigExcC14NTransformUrl;

        reference.AddTransform(new XmlDsigEnvelopedSignatureTransform(false));
        reference.AddTransform(new XmlDsigExcC14NTransform("#default samlp saml ds xs xsi"));
        signedXml.AddReference(reference);

        var keyInfo = new KeyInfo();
        keyInfo.AddClause(new KeyInfoX509Data(signingCertificate));
        signedXml.KeyInfo = keyInfo;

        signedXml.ComputeSignature();
        XmlElement xmlSignature = signedXml.GetXml();

        responseDoc.DocumentElement!.InsertBefore(responseDoc.ImportNode(xmlSignature, deep: true), importedAssertion);

        return responseDoc;
    }

    // ── Private — XML prefix helper ───────────────────────────────────────────

    private static void AddPrefix(XmlNode node, string prefix)
    {
        // Traverse depth-first; assign prefix on the way back up
        foreach (XmlNode child in node.ChildNodes)
            AddPrefix(child, prefix);
        node.Prefix = prefix;
    }
}
