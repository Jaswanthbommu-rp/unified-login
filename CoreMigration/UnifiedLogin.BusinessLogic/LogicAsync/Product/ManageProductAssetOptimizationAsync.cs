using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using UnifiedLogin.BusinessLogic.Logic.Product;
using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Audit.Common;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.BlackBook;
using UnifiedLogin.SharedObjects.Constants;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Exceptions;
using UnifiedLogin.SharedObjects.Extensions;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product;
using UnifiedLogin.SharedObjects.Product.Migration;
using UnifiedLogin.SharedObjects.Saml;

namespace UnifiedLogin.BusinessLogic.LogicAsync.Product;

/// <summary>
/// Native async implementation of Asset Optimization (AO/BI) product management.
/// <para>
/// Replaces the stepping-stone <c>ManageProductAssetOptimizationAsync</c> wrapper and the
/// .NET 4.8 sync class <c>ManageProductAssetOptimization</c> with a fully awaitable service:
/// </para>
/// <list type="bullet">
///   <item><c>IHttpClientFactory</c> replaces <c>new HttpClient()</c> per-call.</item>
///   <item><c>IMemoryCache</c> replaces <c>RPObjectCache</c> / <c>MemoryCache.Default</c>.</item>
///   <item><c>IUserClaimsAccessor</c> replaces the per-call <c>DefaultUserClaim</c> parameter.</item>
///   <item><c>ILogger&lt;T&gt;</c> replaces <c>WriteToDiagnosticLog</c> / <c>WriteToErrorLog</c>.</item>
///   <item>Injected repositories replace <c>new ProductRepository()</c> / <c>new OrganizationRepository(userClaims)</c>.</item>
///   <item><c>ISamlAttributeServiceAsync</c> + <c>IProductSettingServiceAsync</c> replace base-class helpers.</item>
/// </list>
/// </summary>
public sealed class ManageProductAssetOptimizationAsync : IManageProductAssetOptimizationAsync
{
    #region Constants

    private const int CacheTimeSeconds   = 300;
    private const int RoleCacheSeconds   = 10800;
    private const int PropCacheSeconds   = 7200;
    private const string ProductSource   = "AO";
    private const string ProductStatusSettingType = "ProductStatus";
    private const int AoProductId        = (int)ProductEnum.AssetOptimizer;
    private const int AoBiProductId      = (int)ProductEnum.AoBusinessIntelligence;

    // Activity-log message templates (match ManageProductBase constants)
    private const string RolesRemovedMsg     = "Removed Role: RoleName";
    private const string RolesAssignedMsg    = "Assigned Role: RoleName";
    private const string PropsRemovedMsg     = "Removed Property: PropertyName";
    private const string PropsAssignedMsg    = "Assigned Property: PropertyName";

    #endregion

    #region Private records

    private sealed record AoApiSettings(
        string Endpoint,
        string Username,
        string Password,
        string SuperUser,
        string SpecialEditorUser);

    private sealed record AoCallContext(
        string EditorProductUserId,
        string ProductUserId,
        string ProductUsername,
        Persona EditorPersona);

    #endregion

    #region Fields

    private readonly IHttpClientFactory                       _httpClientFactory;
    private readonly IUserClaimsAccessor                     _userClaimsAccessor;
    private readonly IProductContextServiceAsync             _contextService;
    private readonly ISamlAttributeServiceAsync              _samlAttributeService;
    private readonly ISamlRepositoryAsync                    _samlRepository;
    private readonly IProductRepositoryAsync                 _productRepository;
    private readonly IProductSettingServiceAsync             _productSettingService;
    private readonly IOrganizationRepositoryAsync            _organizationRepository;
    private readonly IManagePersonaAsync                     _managePersona;
    private readonly IManagePersonAsync                      _managePerson;
    private readonly IManageUserLoginAsync                   _manageUserLogin;
    private readonly IManageBlueBookAsync                    _manageBlueBook;
    private readonly IManageElectronicAddressAsync           _manageElectronicAddress;
    private readonly IUserLoginRepositoryAsync               _userLoginRepository;
    private readonly IMemoryCache                            _cache;
    private readonly ILogger<ManageProductAssetOptimizationAsync> _logger;

    // Lazy-loaded per scope; scoped services are single-threaded so no lock needed
    private AoApiSettings? _apiSettings;

    #endregion

    #region Constructor

    public ManageProductAssetOptimizationAsync(
        IHttpClientFactory                       httpClientFactory,
        IUserClaimsAccessor                      userClaimsAccessor,
        IProductContextServiceAsync              contextService,
        ISamlAttributeServiceAsync               samlAttributeService,
        ISamlRepositoryAsync                     samlRepository,
        IProductRepositoryAsync                  productRepository,
        IProductSettingServiceAsync              productSettingService,
        IOrganizationRepositoryAsync             organizationRepository,
        IManagePersonaAsync                      managePersona,
        IManagePersonAsync                       managePerson,
        IManageUserLoginAsync                    manageUserLogin,
        IManageBlueBookAsync                     manageBlueBook,
        IManageElectronicAddressAsync            manageElectronicAddress,
        IUserLoginRepositoryAsync                userLoginRepository,
        IMemoryCache                             cache,
        ILogger<ManageProductAssetOptimizationAsync> logger)
    {
        _httpClientFactory      = httpClientFactory;
        _userClaimsAccessor     = userClaimsAccessor;
        _contextService         = contextService;
        _samlAttributeService   = samlAttributeService;
        _samlRepository         = samlRepository;
        _productRepository      = productRepository;
        _productSettingService  = productSettingService;
        _organizationRepository = organizationRepository;
        _managePersona          = managePersona;
        _managePerson           = managePerson;
        _manageUserLogin        = manageUserLogin;
        _manageBlueBook         = manageBlueBook;
        _manageElectronicAddress = manageElectronicAddress;
        _userLoginRepository    = userLoginRepository;
        _cache                  = cache;
        _logger                 = logger;
    }

    #endregion

    // ════════════════════════════════════════════════════════════════════════
    // PUBLIC INTERFACE IMPLEMENTATIONS
    // ════════════════════════════════════════════════════════════════════════

    #region Companies

    public async Task<ListResponse> GetCompaniesAsync(
        long editorPersonaId, long userPersonaId, string productName,
        RequestParameter datafilter, string userLoginName = "",
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("{ActionName} - {State}", "GetCompaniesAsync",
            $"Begin editorPersonaId={editorPersonaId} userPersonaId={userPersonaId} product={productName}");

        var response = new ListResponse();
        try
        {
            var (ctx, error) = await GetCallContextAsync(editorPersonaId, userPersonaId, cancellationToken);
            if (ctx is null)
                return new ListResponse { IsError = true, ErrorReason = error };

            var settings = await GetApiSettingsAsync(cancellationToken);
            var profileUrl = $"{settings.Endpoint}user/profile/{ctx.EditorProductUserId.ToLower()}/";
            var editorProfile = await GetApiAsync<AOUser>(profileUrl, settings, cancellationToken);

            var divisionName = ProductEnumHelper.GetAoDivisionName(ProductEnumHelper.GetAoProductEnum(productName));
            var allCompanies = editorProfile?.Divisions
                .Where(d => d.Division == divisionName)
                .SelectMany(d => d.Companies)
                .ToList() ?? new List<AoCompany>();

            string? productUserId = ctx.ProductUserId;
            if (userPersonaId == 0 && string.IsNullOrEmpty(productUserId) && !string.IsNullOrWhiteSpace(userLoginName))
                productUserId = userLoginName;

            if (!string.IsNullOrEmpty(productUserId))
            {
                var subjectUrl = $"{settings.Endpoint}user/profile/{ctx.EditorProductUserId.ToLower()}/{productUserId.ToLower()}/";
                var subjectProfile = await GetApiAsync<AOUser>(subjectUrl, settings, cancellationToken);
                if (subjectProfile != null)
                {
                    var subjectCompanies = subjectProfile.Divisions
                        .Where(d => d.Division == divisionName)
                        .SelectMany(d => d.Companies)
                        .ToList();
                    allCompanies = FilterAssignedCompanies(allCompanies, subjectCompanies);
                }
            }

            allCompanies = allCompanies.OrderBy(c => c.CompanyName).ToList();

            response = new ListResponse
            {
                Records    = allCompanies.Cast<object>().ToList(),
                TotalRows  = allCompanies.Count,
                RowsPerPage = 9999,
                ErrorReason = string.Empty,
                TotalPages  = 1
            };

            _logger.LogDebug("{ActionName} - {State}", "GetCompaniesAsync",
                $"Done rows={response.TotalRows} editorPersonaId={editorPersonaId}");
        }
        catch (Exception ex)
        {
            response.IsError    = true;
            response.ErrorReason = "There was a problem getting the Companies.";
            _logger.LogError(ex, "{ActionName} - {State}", "GetCompaniesAsync",
                $"Error editorPersonaId={editorPersonaId} userPersonaId={userPersonaId}");
        }
        return response;
    }

    public async Task<ListResponse> GetCompaniesWithRolesAsync(
        long editorPersonaId, long userPersonaId, string productName,
        RequestParameter datafilter, string userLoginName = "",
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("{ActionName} - {State}", "GetCompaniesWithRolesAsync",
            $"Begin editorPersonaId={editorPersonaId}");

        var response = new ListResponse();
        try
        {
            var companiesResult = await GetCompaniesAsync(
                editorPersonaId, userPersonaId, productName, datafilter, userLoginName, cancellationToken);

            if (companiesResult.IsError) return companiesResult;

            var (ctx, error) = await GetCallContextAsync(editorPersonaId, userPersonaId, cancellationToken);
            if (ctx is null) return new ListResponse { IsError = true, ErrorReason = error };

            var settings  = await GetApiSettingsAsync(cancellationToken);
            var allCompanies = companiesResult.Records.Cast<AoCompany>().ToList();
            var companyRoles  = new List<AoCompanyRoles>();

            // Fetch roles per company in parallel with a semaphore to avoid overwhelming the API
            var sem = new SemaphoreSlim(4, 4);
            var tasks = allCompanies.Select(async company =>
            {
                await sem.WaitAsync(cancellationToken);
                try
                {
                    var roles = await GetRolesAsync(
                        company.CompanyId, productName, ctx.ProductUserId,
                        ctx.EditorProductUserId, userPersonaId, settings, cancellationToken);

                    if (roles?.Count > 0)
                    {
                        return new AoCompanyRoles
                        {
                            CompanyId   = company.CompanyId,
                            CompanyName = company.CompanyName,
                            IsAssigned  = company.IsAssigned,
                            Status      = company.Status,
                            Roles       = roles.OrderBy(r => r.DisplayName).ToList()
                        };
                    }
                    return null;
                }
                finally { sem.Release(); }
            });

            var results = await Task.WhenAll(tasks);
            companyRoles.AddRange(results.Where(r => r is not null)!);

            response = new ListResponse
            {
                Records     = companyRoles.Cast<object>().ToList(),
                TotalRows   = companyRoles.Count,
                RowsPerPage = 9999,
                ErrorReason = string.Empty,
                TotalPages  = 1
            };
        }
        catch (Exception ex)
        {
            response.IsError    = true;
            response.ErrorReason = "There was a problem getting companies with roles.";
            _logger.LogError(ex, "{ActionName} - {State}", "GetCompaniesWithRolesAsync",
                $"Error editorPersonaId={editorPersonaId}");
        }
        return response;
    }

    public async Task<ListResponse> GetCompaniesWithPropertiesAsync(
        long editorPersonaId, long userPersonaId, string productName,
        RequestParameter datafilter, string userLoginName = "",
        CancellationToken cancellationToken = default)
    {
        var response = new ListResponse();
        try
        {
            var companiesResult = await GetCompaniesAsync(
                editorPersonaId, userPersonaId, productName, datafilter, userLoginName, cancellationToken);

            if (companiesResult.IsError) return companiesResult;

            var (ctx, error) = await GetCallContextAsync(editorPersonaId, userPersonaId, cancellationToken);
            if (ctx is null) return new ListResponse { IsError = true, ErrorReason = error };

            var settings     = await GetApiSettingsAsync(cancellationToken);
            var allCompanies = companiesResult.Records.Cast<AoCompany>().ToList();
            var companyProps = new List<AoCompanyProperties>();

            foreach (var company in allCompanies)
            {
                var propList = await GetPropertiesAsync(
                    company.CompanyId, productName, ctx.ProductUserId,
                    ctx.EditorProductUserId, userPersonaId, settings, cancellationToken);

                propList.Properties = propList.Properties.OrderBy(p => p.PropertyName).ToList();

                if (propList.Properties != null)
                {
                    companyProps.Add(new AoCompanyProperties
                    {
                        CompanyId         = company.CompanyId,
                        CompanyName       = company.CompanyName,
                        IsAssigned        = company.IsAssigned,
                        Status            = company.Status,
                        AssignedProperties = $"{propList.Properties.Count(p => p.IsAssigned)} of {propList.Properties.Count}",
                        Properties        = propList.Properties
                    });
                }
            }

            response = new ListResponse
            {
                Records     = companyProps.Cast<object>().ToList(),
                TotalRows   = companyProps.Count,
                RowsPerPage = 9999,
                ErrorReason = string.Empty,
                TotalPages  = 1
            };
        }
        catch (Exception ex)
        {
            response.IsError    = true;
            response.ErrorReason = "There was a problem getting companies with properties.";
            _logger.LogError(ex, "{ActionName} - {State}", "GetCompaniesWithPropertiesAsync",
                $"Error editorPersonaId={editorPersonaId}");
        }
        return response;
    }

    #endregion

    #region Roles / Properties

    public async Task<ListResponse> GetProductRolesAsync(
        long editorPersonaId, long userPersonaId, string productName,
        RequestParameter datafilter, string userLoginName = "",
        CancellationToken cancellationToken = default)
    {
        var response = new ListResponse();
        try
        {
            var (ctx, error) = await GetCallContextAsync(editorPersonaId, userPersonaId, cancellationToken);
            if (ctx is null) return new ListResponse { IsError = true, ErrorReason = error };

            var settings = await GetApiSettingsAsync(cancellationToken);
            var company  = await GetAoCompanyAsync(ctx.EditorPersona.Organization.RealPageId, cancellationToken);

            if (string.IsNullOrEmpty(company.CompanyInstanceSourceId))
                return new ListResponse { IsError = true, ErrorReason = "Company Setup Error: Please Contact Support." };

            var roles = await GetRolesAsync(
                Convert.ToInt32(company.CompanyInstanceSourceId), productName,
                ctx.ProductUserId, ctx.EditorProductUserId, userPersonaId, settings, cancellationToken);

            var companyRoles = new List<ProductRole>();
            if (roles?.Count > 0)
            {
                int i = 1;
                foreach (var role in roles.OrderBy(r => r.DisplayName))
                {
                    companyRoles.Add(new ProductRole
                    {
                        ID         = (i++).ToString(),
                        Name       = role.Name,
                        Description = role.DisplayName,
                        IsAssigned  = role.IsAssigned
                    });
                }
            }

            response = new ListResponse
            {
                Records     = companyRoles.Cast<object>().ToList(),
                TotalRows   = companyRoles.Count,
                RowsPerPage = companyRoles.Count,
                ErrorReason = string.Empty,
                TotalPages  = 1
            };
        }
        catch (Exception ex) when (ex is BlueBookException)
        {
            response.IsError    = true;
            response.ErrorReason = ex.Message;
        }
        catch (Exception ex)
        {
            response.IsError    = true;
            response.ErrorReason = CommonMessageConstants.RoleErrorMessage;
            _logger.LogError(ex, "{ActionName} - {State}", "GetProductRolesAsync",
                $"Error editorPersonaId={editorPersonaId}");
        }
        return response;
    }

    public async Task<ListResponse> GetProductPropertiesAsync(
        long editorPersonaId, long userPersonaId, string productName,
        RequestParameter datafilter, string userLoginName = "",
        CancellationToken cancellationToken = default)
    {
        var response = new ListResponse();
        var allProperties = new Dictionary<string, bool>();
        try
        {
            var (ctx, error) = await GetCallContextAsync(editorPersonaId, userPersonaId, cancellationToken);
            if (ctx is null) return new ListResponse { IsError = true, ErrorReason = error };

            var settings = await GetApiSettingsAsync(cancellationToken);
            var company  = await GetAoCompanyAsync(ctx.EditorPersona.Organization.RealPageId, cancellationToken);

            if (string.IsNullOrEmpty(company.CompanyInstanceSourceId))
                return new ListResponse { IsError = true, ErrorReason = "Company Setup Error: Please Contact Support." };

            var propList = await GetPropertiesAsync(
                Convert.ToInt32(company.CompanyInstanceSourceId), productName,
                ctx.ProductUserId, ctx.EditorProductUserId, userPersonaId, settings, cancellationToken);

            propList.Properties = propList.Properties.OrderBy(p => p.PropertyName).ToList();
            allProperties["allProperties"] = propList.allProperties;

            var companyProps = propList.Properties.Select(p => new ProductProperty
            {
                ID         = p.PropertyId.ToString(),
                Name       = p.PropertyName,
                IsAssigned = p.IsAssigned,
                State      = p.State
            }).ToList();

            response = new ListResponse
            {
                Records     = companyProps.Cast<object>().ToList(),
                TotalRows   = companyProps.Count,
                RowsPerPage = companyProps.Count,
                ErrorReason = string.Empty,
                TotalPages  = 1,
                Additional  = allProperties
            };
        }
        catch (Exception ex) when (ex is BlueBookException)
        {
            response.IsError    = true;
            response.ErrorReason = ex.Message;
        }
        catch (Exception ex)
        {
            response.IsError    = true;
            response.ErrorReason = CommonMessageConstants.PropertyErrorMessage;
            _logger.LogError(ex, "{ActionName} - {State}", "GetProductPropertiesAsync",
                $"Error editorPersonaId={editorPersonaId}");
        }
        return response;
    }

    public async Task<ListResponse> GetOperatorsAsync(
        long editorPersonaId, long userPersonaId,
        CancellationToken cancellationToken = default)
    {
        var response = new ListResponse();
        try
        {
            var (ctx, error) = await GetCallContextAsync(editorPersonaId, 0, cancellationToken);
            if (ctx is null) return new ListResponse { IsError = true, ErrorReason = error };

            var settings = await GetApiSettingsAsync(cancellationToken);
            var company  = await GetAoCompanyAsync(ctx.EditorPersona.Organization.RealPageId, cancellationToken);
            var aoCompanyId = company.CompanyInstanceSourceId;

            var url = $"{settings.Endpoint}company/{aoCompanyId}/delegated/operators";
            var operators = await GetApiAsync<IList<tag>>(url, settings, cancellationToken);

            if (operators == null)
            {
                response.ErrorReason = "No Operators received for company.";
                return response;
            }

            response = new ListResponse
            {
                Records     = operators.Cast<object>().ToList(),
                TotalRows   = operators.Count,
                RowsPerPage = operators.Count,
                ErrorReason = string.Empty,
                TotalPages  = 1,
                Additional  = new Dictionary<string, bool>()
            };
        }
        catch (Exception ex)
        {
            response.IsError    = true;
            response.ErrorReason = "There was a problem getting the Operators.";
            _logger.LogError(ex, "{ActionName} - {State}", "GetOperatorsAsync",
                $"Error editorPersonaId={editorPersonaId}");
        }
        return response;
    }

    public async Task<ListResponse> GetPropertiesWithOperatorsAsync(
        long editorPersonaId, long userPersonaId,
        string operatorCode, string operatorValue,
        CancellationToken cancellationToken = default)
    {
        var response = new ListResponse();
        try
        {
            var (ctx, error) = await GetCallContextAsync(editorPersonaId, 0, cancellationToken);
            if (ctx is null) return new ListResponse { IsError = true, ErrorReason = error };

            var settings = await GetApiSettingsAsync(cancellationToken);
            var company  = await GetAoCompanyAsync(ctx.EditorPersona.Organization.RealPageId, cancellationToken);

            var url = $"{settings.Endpoint}company/{company.CompanyInstanceSourceId}/delegated/properties"
                    + $"?operatorCode={Uri.EscapeDataString(operatorCode)}&operatorValue={Uri.EscapeDataString(operatorValue)}";

            var apiProps   = await GetApiAsync<IList<AoProperty>>(url, settings, cancellationToken) ?? new List<AoProperty>();
            var properties = apiProps.Select(p => new ProductProperty
            {
                ID   = p.PropertyId.ToString(),
                Name = p.PropertyName
            }).ToList();

            response = new ListResponse
            {
                Records     = properties.Cast<object>().ToList(),
                TotalRows   = properties.Count,
                RowsPerPage = properties.Count,
                ErrorReason = string.Empty,
                TotalPages  = 1,
                Additional  = new Dictionary<string, bool>()
            };
        }
        catch (Exception ex)
        {
            response.IsError    = true;
            response.ErrorReason = "There was a problem getting properties with operators.";
            _logger.LogError(ex, "{ActionName} - {State}", "GetPropertiesWithOperatorsAsync",
                $"Error editorPersonaId={editorPersonaId}");
        }
        return response;
    }

    #endregion

    #region Property groups

    public async Task<ListResponse> GetPropertyGroupsAsync(
        long editorPersonaId, long userPersonaId, string productName,
        IList<string> selectedCompanies,
        CancellationToken cancellationToken = default)
    {
        var response     = new ListResponse();
        var aoPropertyGroups = new List<AoPropertyGroup>();
        try
        {
            var (ctx, error) = await GetCallContextAsync(editorPersonaId, userPersonaId, cancellationToken);
            if (ctx is null) return new ListResponse { IsError = true, ErrorReason = error };

            var settings       = await GetApiSettingsAsync(cancellationToken);
            var assignable     = await GetAssignablePropertyGroupsAsync(
                productName, selectedCompanies, ctx.EditorProductUserId, settings, cancellationToken);

            AOUser? userProfile = null;
            if (!string.IsNullOrEmpty(ctx.ProductUserId))
            {
                var subjectUrl = $"{settings.Endpoint}user/profile/{ctx.EditorProductUserId.ToLower()}/{ctx.ProductUserId.ToLower()}/";
                userProfile    = await GetApiAsync<AOUser>(subjectUrl, settings, cancellationToken);
            }

            IList<AoPropertyGroups> propertyGroups = string.IsNullOrEmpty(ctx.ProductUserId)
                ? GetPropertyGroupsForNewUser(assignable)
                : GetPropertyGroupsForExistingUser(assignable, userProfile!, productName);

            if (propertyGroups == null || propertyGroups.Count == 0)
            {
                return new ListResponse { IsError = true, ErrorReason = "No groups received from product." };
            }

            foreach (var grp in propertyGroups.OrderBy(g => g.GroupName))
            {
                aoPropertyGroups.Add(new AoPropertyGroup
                {
                    ID         = grp.GroupId.ToString(),
                    Name       = grp.GroupName,
                    IsAssigned = grp.IsAssigned
                });
            }

            response = new ListResponse
            {
                Records     = aoPropertyGroups.Cast<object>().ToList(),
                TotalRows   = aoPropertyGroups.Count,
                RowsPerPage = aoPropertyGroups.Count,
                ErrorReason = string.Empty,
                TotalPages  = 1
            };
        }
        catch (Exception ex)
        {
            response.IsError    = true;
            response.ErrorReason = ex is BlueBookException ? ex.Message : CommonMessageConstants.PropertyGroupErrorMessage;
            _logger.LogError(ex, "{ActionName} - {State}", "GetPropertyGroupsAsync",
                $"Error editorPersonaId={editorPersonaId}");
        }
        return response;
    }

    public async Task<ListResponse> GetProductPropertyGroupsAsync(
        long editorPersonaId, long userPersonaId, string productName,
        string userLoginName,
        CancellationToken cancellationToken = default)
    {
        var response     = new ListResponse();
        var aoPropertyGroups = new List<AoPropertyGroup>();
        try
        {
            var (ctx, error) = await GetCallContextAsync(editorPersonaId, userPersonaId, cancellationToken);
            if (ctx is null) return new ListResponse { IsError = true, ErrorReason = error };

            var settings = await GetApiSettingsAsync(cancellationToken);
            var company  = await GetAoCompanyAsync(ctx.EditorPersona.Organization.RealPageId, cancellationToken);

            // Use company ID as the single selected company
            var selectedCompanies = new List<string> { company.CompanyInstanceSourceId };
            var assignable = await GetAssignablePropertyGroupsAsync(
                productName, selectedCompanies, ctx.EditorProductUserId, settings, cancellationToken);

            string? resolvedProductUserId = ctx.ProductUserId;
            if (string.IsNullOrWhiteSpace(resolvedProductUserId) && !string.IsNullOrWhiteSpace(userLoginName))
                resolvedProductUserId = userLoginName;

            AOUser? userProfile = null;
            if (!string.IsNullOrEmpty(resolvedProductUserId))
            {
                var subjectUrl = $"{settings.Endpoint}user/profile/{ctx.EditorProductUserId.ToLower()}/{resolvedProductUserId.ToLower()}/";
                userProfile    = await GetApiAsync<AOUser>(subjectUrl, settings, cancellationToken);
            }

            IList<AoPropertyGroups> propertyGroups = string.IsNullOrEmpty(resolvedProductUserId)
                ? GetPropertyGroupsForNewUser(assignable)
                : GetPropertyGroupsForExistingUser(assignable, userProfile!, productName);

            if (propertyGroups == null || propertyGroups.Count == 0)
                return new ListResponse { IsError = true, ErrorReason = "No groups received from product." };

            foreach (var grp in propertyGroups.OrderBy(g => g.GroupName))
            {
                aoPropertyGroups.Add(new AoPropertyGroup
                {
                    ID         = grp.GroupId.ToString(),
                    Name       = grp.GroupName,
                    IsAssigned = grp.IsAssigned
                });
            }

            response = new ListResponse
            {
                Records     = aoPropertyGroups.Cast<object>().ToList(),
                TotalRows   = aoPropertyGroups.Count,
                RowsPerPage = aoPropertyGroups.Count,
                ErrorReason = string.Empty,
                TotalPages  = 1
            };
        }
        catch (Exception ex)
        {
            response.IsError    = true;
            response.ErrorReason = ex is BlueBookException ? ex.Message : CommonMessageConstants.PropertyGroupErrorMessage;
            _logger.LogError(ex, "{ActionName} - {State}", "GetProductPropertyGroupsAsync",
                $"Error editorPersonaId={editorPersonaId}");
        }
        return response;
    }

    public async Task<ListResponse> GetPropertiesInGroupAsync(
        long editorPersonaId, long userPersonaId, int propertyGroupId,
        CancellationToken cancellationToken = default)
    {
        var response = new ListResponse();
        try
        {
            var (ctx, error) = await GetCallContextAsync(editorPersonaId, 0, cancellationToken);
            if (ctx is null) return new ListResponse { IsError = true, ErrorReason = error };

            if (string.IsNullOrEmpty(ctx.EditorProductUserId))
                return new ListResponse { ErrorReason = $"User does not exist in AO product with editorPersonaId {editorPersonaId}." };

            var settings = await GetApiSettingsAsync(cancellationToken);
            var allGroups = await GetAllPropertyGroupsAsync(ctx.EditorProductUserId, settings.SuperUser, settings, cancellationToken);

            var props = allGroups.Groups
                .Where(g => g.Properties != null && g.GroupId == propertyGroupId)
                .SelectMany(g => g.Properties)
                .Select(p => new AoProperty { PropertyId = p.PropertyId, PropertyName = p.PropertyName })
                .OrderBy(p => p.PropertyName)
                .ToList();

            response = props.Count > 0
                ? new ListResponse { Records = props.Cast<object>().ToList(), TotalRows = props.Count, RowsPerPage = 9999, ErrorReason = string.Empty, TotalPages = 1 }
                : new ListResponse { TotalRows = 0, RowsPerPage = 9999, ErrorReason = $"No properties in group {propertyGroupId}." };
        }
        catch (Exception ex)
        {
            response.IsError    = true;
            response.ErrorReason = "There was a problem getting properties in group.";
            _logger.LogError(ex, "{ActionName} - {State}", "GetPropertiesInGroupAsync",
                $"Error editorPersonaId={editorPersonaId}");
        }
        return response;
    }

    public async Task<ListResponse> GetGroupPropertiesAsync(
        long editorPersonaId, long userPersonaId, int propertyGroupId, int productId,
        CancellationToken cancellationToken = default)
    {
        var response = new ListResponse();
        try
        {
            var (ctx, error) = await GetCallContextAsync(editorPersonaId, 0, cancellationToken);
            if (ctx is null) return new ListResponse { IsError = true, ErrorReason = error };

            var settings    = await GetApiSettingsAsync(cancellationToken);
            var productList = await _productRepository.GetAllProductsAsync(cancellationToken);
            var productCode = ProductEnumHelper.GetProductCodeByProductId(productId, productList);

            var groupUrl = $"{settings.Endpoint}user/{ctx.EditorProductUserId.ToLower()}/groups/assignable/properties?groupId={propertyGroupId}";
            var groups   = await GetApiAsync<List<VisibleGroupProperty>>(groupUrl, settings, cancellationToken)
                          ?? new List<VisibleGroupProperty>();

            var props = groups
                .Where(pg => pg.Products.Contains(productCode))
                .Select(pg => new ProductProperty { ID = pg.PropertyId.ToString(), Name = pg.PropertyName, State = "" })
                .OrderBy(p => p.Name)
                .ToList();

            response = props.Count > 0
                ? new ListResponse { Records = props.Cast<object>().ToList(), TotalRows = props.Count, RowsPerPage = 9999, ErrorReason = string.Empty, TotalPages = 1 }
                : new ListResponse { TotalRows = 0, RowsPerPage = 9999, ErrorReason = $"No properties for group {propertyGroupId}." };
        }
        catch (Exception ex)
        {
            response.IsError    = true;
            response.ErrorReason = "There was a problem getting group properties.";
            _logger.LogError(ex, "{ActionName} - {State}", "GetGroupPropertiesAsync",
                $"Error editorPersonaId={editorPersonaId}");
        }
        return response;
    }

    #endregion

    #region Migration

    public async Task<ListResponse> GetMigrationUsersAsync(
        long editorPersonaId, RequestParameter datafilter,
        CancellationToken cancellationToken = default)
    {
        var response = new ListResponse { IsError = true, ErrorReason = "No Users." };
        try
        {
            var (ctx, error) = await GetCallContextAsync(editorPersonaId, 0, cancellationToken);
            if (ctx is null) { response.ErrorReason = error; return response; }

            var settings = await GetApiSettingsAsync(cancellationToken);
            var url      = $"{settings.Endpoint}unity/migration/users/{ctx.EditorProductUserId.ToLower()}/";
            var migration = await GetApiAsync<IList<AssetOptimizationMigrationUser>>(url, settings, cancellationToken);

            if (migration == null)
            {
                _logger.LogError("{ActionName} - {State}", "GetMigrationUsersAsync",
                    $"No users from API editorPersonaId={editorPersonaId}");
                return response;
            }

            var aoCompanyInfo = await GetAoCompanyAsync(ctx.EditorPersona.Organization.RealPageId, cancellationToken);
            var productUserList = await _productRepository.GetProductUsersByCompanyAsync(
                ctx.EditorPersona.OrganizationPartyId, Convert.ToString(AoProductId), cancellationToken);

            var orgUsers = migration
                .Where(m => m.CompanySourceInstanceId != null &&
                            m.CompanySourceInstanceId.Equals(aoCompanyInfo.CompanyInstanceSourceId))
                .ToList();

            if (productUserList?.Count > 0)
                orgUsers = orgUsers.Where(o => !productUserList.Any(p => p.ProductUserName == o.UserName)).ToList();

            var migrationUsers = orgUsers.Select(u => new MigrationUser
            {
                CompanyInstanceSourceId = u.CompanySourceInstanceId,
                FirstName  = u.FirstName,
                LastName   = u.LastName,
                UserId     = u.UserId,
                Username   = u.UserName,
                Email      = u.Email,
                LastActivity = u.Activity.ToString(),
                Extra      = string.Join("|", u.Products),
                Status     = string.IsNullOrWhiteSpace(u.Status) || u.Status.ToLower() == "active" ? "Active" : "Disabled"
            }).ToList();

            response = new ListResponse
            {
                Records     = migrationUsers.Cast<object>().ToList(),
                TotalRows   = migration.Count,
                RowsPerPage = migration.Count,
                ErrorReason = string.Empty,
                IsError     = false,
                TotalPages  = 1
            };
        }
        catch (Exception ex)
        {
            response = new ListResponse { IsError = true, ErrorReason = ex.Message };
            _logger.LogError(ex, "{ActionName} - {State}", "GetMigrationUsersAsync",
                $"Error editorPersonaId={editorPersonaId}");
        }
        return response;
    }

    public async Task<MigrateResponse> UpdateUsersMigrationStatusAsync(
        long editorPersonaId, IList<MigrateUser> migrateUsers,
        CancellationToken cancellationToken = default)
    {
        var migrateResponse = new MigrateResponse { Status = false };

        var (ctx, error) = await GetCallContextAsync(editorPersonaId, 0, cancellationToken);
        if (ctx is null) { migrateResponse.Message = error; return migrateResponse; }

        // Remove external users from the migrate list
        var filtered = new List<MigrateUser>();
        foreach (var migrateUser in migrateUsers)
        {
            var orgs = await _userLoginRepository.ListOrganizationByLoginNameAsync(migrateUser.UnifiedLoginUserName);
            bool isExternal = orgs.Any(m =>
                m.OrganizationPartyId == _userClaimsAccessor.OrganizationPartyId &&
                m.PartyRoleTypeId     == UserTypeConstants.ExternalUser);
            if (!isExternal) filtered.Add(migrateUser);
        }

        if (!filtered.Any())
        {
            migrateResponse.Status  = true;
            migrateResponse.Message = "success";
            return migrateResponse;
        }

        var settings = await GetApiSettingsAsync(cancellationToken);
        var url      = $"{settings.Endpoint}unity/migration/users";
        var userIds  = filtered.Select(x => x.UserId).ToList();

        var responseContent = await PutApiAsync(url, userIds, settings, cancellationToken);

        if (!string.IsNullOrWhiteSpace(responseContent))
        {
            try
            {
                var migrationResult = JsonConvert.DeserializeObject<IList<AOMigrateResponse>>(responseContent);
                if (migrationResult?.Any(x => x.Status) == true)
                {
                    migrateResponse.Status  = true;
                    migrateResponse.Message = "success";
                }
            }
            catch
            {
                migrateResponse.Message = responseContent;
            }
        }
        else
        {
            migrateResponse.Message = "Cannot update user status to migrated.";
        }
        return migrateResponse;
    }

    #endregion

    #region User status / profile

    public async Task<bool> ChangeUserStatusAsync(
        long editorPersonaId, string userName, string firstName, string lastName,
        CancellationToken cancellationToken = default)
    {
        var (ctx, error) = await GetCallContextAsync(editorPersonaId, 0, cancellationToken);
        if (ctx is null) return false;

        var settings = await GetApiSettingsAsync(cancellationToken);
        var aoUser = new AOUser
        {
            IsInternalUser = false,
            IsEnabled      = false,
            IsSuperUser    = false,
            Email          = userName,
            Login          = userName,
            OldUserId      = userName,
            UserId         = userName,
            FirstName      = firstName,
            LastName       = lastName
        };

        var copied = await CopyRegularUserAsync(
            editorPersonaId, 0, settings, ctx.EditorProductUserId,
            productUserName: userName, cancellationToken);

        aoUser.GroupsModel = GetBundledGroups(copied);
        aoUser.Divisions   = new List<Divisions>();
        aoUser.Model       = GetModel(copied);

        var result = await PutApiAsync(
            $"{settings.Endpoint}user/profile/{ctx.EditorProductUserId.ToLower()}/",
            aoUser, settings, cancellationToken);

        return string.IsNullOrEmpty(result);
    }

    public async Task<string> UpdateUserProfileAsync(
        long editorPersonaId, long userPersonaId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("{ActionName} - {State}", "UpdateUserProfileAsync",
            $"Begin editorPersonaId={editorPersonaId} userPersonaId={userPersonaId}");
        try
        {
            var (ctx, error) = await GetCallContextAsync(editorPersonaId, userPersonaId, cancellationToken);
            if (ctx is null) return error ?? "Context error.";

            var settings   = await GetApiSettingsAsync(cancellationToken);
            var persona    = await _managePersona.GetPersonaAsync(userPersonaId, false, cancellationToken);
            var person     = await _managePerson.GetPersonAsync(persona.RealPageId, cancellationToken);
            var userLogin  = await _manageUserLogin.GetUserLoginOnlyAsync(persona.RealPageId, cancellationToken);
            var emailAddress = await GetUserEmailAddressAsync(persona.RealPageId, userLogin.LoginName, userPersonaId, cancellationToken, persona);

            string? productUserName = await GetSamlProductUserNameAsync(userPersonaId, AoProductId, cancellationToken);
            bool loginNameChanged = !string.IsNullOrEmpty(productUserName) &&
                                    !productUserName.Equals(userLogin.LoginName, StringComparison.OrdinalIgnoreCase);

            var copied = await CopyRegularUserAsync(
                editorPersonaId, userPersonaId, settings, ctx.EditorProductUserId,
                cancellationToken: cancellationToken);
            var existingProducts = copied;

            var aoUser = new AOUser
            {
                IsInternalUser = false,
                IsEnabled      = true,
                IsSuperUser    = false,
                Email          = emailAddress.ToLower(),
                Login          = userLogin.LoginName.ToLower(),
                OldUserId      = ctx.ProductUserId.ToLower(),
                UserId         = userLogin.LoginName.ToLower(),
                FirstName      = person.FirstName,
                LastName       = person.LastName,
                GroupsModel    = GetBundledGroups(copied),
                Divisions      = new List<Divisions>(),
                Model          = GetModel(copied)
            };

            var result = await PutApiAsync(
                $"{settings.Endpoint}user/profile/{ctx.EditorProductUserId.ToLower()}/",
                aoUser, settings, cancellationToken);

            if (string.IsNullOrEmpty(result))
            {
                await UpdateProductUserInGreenBookAsync(
                    editorPersonaId, userPersonaId, userLogin.LoginName.ToLower(),
                    existingProducts, copied, ctx.EditorProductUserId,
                    settings, cancellationToken, loginNameChanged, persona);
            }
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{ActionName} - {State}", "UpdateUserProfileAsync",
                $"Error editorPersonaId={editorPersonaId} userPersonaId={userPersonaId}");
            return ex.Message;
        }
    }

    #endregion

    #region Product assignment

    public async Task<(string Result, List<AdditionalParameters> ActivityLog)> ManageAssetOptimizationUserAsync(
        long editorPersonaId, long productUserPersonaId,
        IList<AoUserCompanyPropertyRoleDetail> aoGbUserCompanyPropertyRoleDetails,
        BatchProcessType batchProcessType = BatchProcessType.CreateUpdateProductUser,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("{ActionName} - {State}", "ManageAssetOptimizationUserAsync",
            $"Begin editorPersonaId={editorPersonaId} userPersonaId={productUserPersonaId}");

        var activityLog = new List<AdditionalParameters>();
        try
        {
            var (ctx, error) = await GetCallContextAsync(editorPersonaId, productUserPersonaId, cancellationToken);
            if (ctx is null) return (error ?? "Context error.", activityLog);

            var settings  = await GetApiSettingsAsync(cancellationToken);
            var persona   = await _managePersona.GetPersonaAsync(productUserPersonaId, false, cancellationToken);
            var realPageId = persona.RealPageId;
            var person    = await _managePerson.GetPersonAsync(realPageId, cancellationToken);
            var userGbLogin = await _manageUserLogin.GetUserLoginOnlyAsync(realPageId, cancellationToken);
            var userLogin = await _manageUserLogin.GetUserLoginAsync(realPageId, persona.OrganizationPartyId, cancellationToken);

            // Use existing AO product username if found (PME-204114 fix)
            string? productUserName = await GetSamlProductUserNameAsync(productUserPersonaId, AoProductId, cancellationToken);
            string effectiveLoginName = !string.IsNullOrEmpty(productUserName)
                ? productUserName
                : userGbLogin.LoginName;

            var personaList    = await _managePersona.ListActivePersonaAsync(realPageId, false, cancellationToken);
            var orgList        = await _userLoginRepository.ListOrganizationByEnterpriseUserIdAsync(realPageId, null);
            var personaOrg     = orgList.FirstOrDefault(o => o.PartyId == persona.OrganizationPartyId);
            bool hasMultiCompany = personaList.Count(p =>
                p.OrganizationPartyId != persona.OrganizationPartyId &&
                p.Organization.RealPageId != DefaultUserClaim.ExternalCompanyRealPageId) > 0;

            var blueAOCompanyInfo = await GetAoCompanyAsync(persona.Organization.RealPageId, cancellationToken);
            if (blueAOCompanyInfo.CompanyInstanceSourceId == null)
                return ("Company Setup Error: Please Contact Support.", activityLog);

            string emailAddress = await GetUserEmailAddressAsync(realPageId, effectiveLoginName, productUserPersonaId, cancellationToken, persona);
            if (string.IsNullOrEmpty(emailAddress))
                return ("Valid Email Address Error: Please Contact Support.", activityLog);

            var aoUser = new AOUser
            {
                IsInternalUser = false,
                IsEnabled      = true,
                IsSuperUser    = false,
                Email          = emailAddress.ToLower(),
                Login          = effectiveLoginName.ToLower(),
                OldUserId      = string.Empty,
                UserId         = effectiveLoginName.ToLower(),
                FirstName      = person.FirstName,
                LastName       = person.LastName
            };

            if (!await _contextService.IsSuperUserAsync(persona, cancellationToken) && userLogin.IsActive == false)
                aoUser.IsEnabled = false;

            // ── RealPage access (company admin) branch ───────────────────────
            var companyAdminRpId = await _organizationRepository.GetOrganizationAdminUserRealPageIdAsync(
                persona.Organization.RealPageId);

            if (companyAdminRpId == realPageId)
            {
                _logger.LogDebug("{ActionName} - {State}", "ManageAssetOptimizationUserAsync",
                    $"RealPage access user editorPersonaId={editorPersonaId}");

                var productsUrl  = $"{settings.Endpoint}company/{blueAOCompanyInfo.CompanyInstanceSourceId}/products";
                var allAOProducts = await GetApiAsync<IList<GroupModel>>(productsUrl, settings, cancellationToken)
                                   ?? new List<GroupModel>();

                var groupsModel = new List<GroupModel>();
                var modelList   = new List<Model>();
                foreach (var item in allAOProducts.Where(p => ProductEnumHelper.CheckAoProductSupportedByGreenBook(p.ProductName)))
                {
                    groupsModel.Add(new GroupModel { Division = item.Division, ProductName = item.ProductName, IsEnabled = true });
                    modelList.Add(new Model
                    {
                        CompanyId             = Convert.ToInt32(blueAOCompanyInfo.CompanyInstanceSourceId),
                        DivisionName          = item.Division,
                        Product               = item.ProductName,
                        SelectedPortfolioValues = new List<int>(),
                        SelectedRoleValues    = new List<string>(),
                        allProperties         = true
                    });
                }
                aoUser.GroupsModel = groupsModel;
                aoUser.Model       = modelList;

                string returnResult = string.IsNullOrEmpty(productUserName)
                    ? await PostApiAsync($"{settings.Endpoint}user/profile/{settings.SpecialEditorUser.ToLower()}/", aoUser, settings, cancellationToken)
                    : await PutApiAsync($"{settings.Endpoint}user/profile/{settings.SpecialEditorUser.ToLower()}/", aoUser, settings, cancellationToken);

                if (string.IsNullOrEmpty(returnResult))
                {
                    var productList = aoUser.Model.Select(m => m.Product).Distinct().ToList();
                    await CreateProductUserInGreenBookAsync(editorPersonaId, productUserPersonaId, productList, effectiveLoginName.ToLower(), cancellationToken);
                }
                return (returnResult, activityLog);
            }

            // ── Regular user branch ──────────────────────────────────────────
            if (await _contextService.IsSuperUserAsync(persona, cancellationToken) &&
                aoGbUserCompanyPropertyRoleDetails.Any(m => !m.IsAssigned))
            {
                aoUser.IsEnabled = false;
            }

            if (aoGbUserCompanyPropertyRoleDetails != null)
            {
                foreach (var data in aoGbUserCompanyPropertyRoleDetails.Where(d => d.CompanyId == 0))
                    data.CompanyId = Convert.ToInt32(blueAOCompanyInfo.CompanyInstanceSourceId);
            }

            var userAOProducts   = await GetAOProductsForNewMultiCompanyUserAsync(editorPersonaId, effectiveLoginName, cancellationToken);
            var inputProducts    = aoGbUserCompanyPropertyRoleDetails!.Select(s => s.ProductName).ToList();
            var deletedProducts  = userAOProducts.Except(inputProducts).ToList();
            var addedProducts    = userAOProducts.Concat(inputProducts).Intersect(inputProducts).ToList();
            var mergedList       = addedProducts.Concat(deletedProducts).Distinct().ToList();

            // Pre-fetch roles / properties / groups needed for activity log
            var requiredRoles  = new Dictionary<string, List<ProductRole>>();
            var requiredProps  = new Dictionary<string, List<ProductProperty>>();
            var requiredGroups = new Dictionary<string, List<AoPropertyGroup>>();

            foreach (var prod in mergedList)
            {
                var rolesResult = await GetProductRolesAsync(editorPersonaId, productUserPersonaId, prod, new RequestParameter(), "", cancellationToken);
                requiredRoles[prod] = rolesResult.Records?.Cast<ProductRole>().ToList() ?? new List<ProductRole>();
            }

            var noPropertiesSettings = await _productSettingService.GetProductSettingAsync(3, cancellationToken);
            var productsWithNoProperties = new List<int>();
            var noPropSetting = noPropertiesSettings?.Find(s => s.Name.Equals("UserAccessDetails_ProductsWithNoProperties", StringComparison.OrdinalIgnoreCase));
            if (noPropSetting != null)
                productsWithNoProperties = noPropSetting.Value.Split(',').Select(int.Parse).ToList();

            var allProducts         = await _productRepository.GetAllProductsAsync(cancellationToken);
            var aoPropsProducts     = allProducts
                .Where(x => x.UDMSourceCode == "AO" && mergedList.Contains(x.BooksProductCode) && !productsWithNoProperties.Contains(x.ProductId))
                .Select(x => x.BooksProductCode)
                .ToList();

            foreach (var prod in aoPropsProducts)
            {
                var propsResult = await GetProductPropertiesAsync(editorPersonaId, productUserPersonaId, prod, new RequestParameter(), "", cancellationToken);
                if (propsResult.Records != null)
                    requiredProps[prod] = propsResult.Records.Cast<ProductProperty>().ToList();

                var grpResult = await GetProductPropertyGroupsAsync(editorPersonaId, productUserPersonaId, prod, "", cancellationToken);
                if (grpResult.Records != null)
                    requiredGroups[prod] = grpResult.Records.Cast<AoPropertyGroup>().ToList();
            }

            // Ensure multi-company user has AO record before updates
            if (string.IsNullOrEmpty(ctx.ProductUsername) && orgList?.Count > 1 && userAOProducts?.Count > 0)
            {
                await CreateProductUserInGreenBookAsync(editorPersonaId, productUserPersonaId, userAOProducts, effectiveLoginName.ToLower(), cancellationToken);
            }

            bool aoProductAssigned = aoGbUserCompanyPropertyRoleDetails.Any(p => p.IsAssigned);

            // Super-user: copy all editor assignments
            if (await _contextService.IsSuperUserAsync(persona, cancellationToken))
            {
                aoGbUserCompanyPropertyRoleDetails = await CopyEditorUserToCreateSuperUserAsync(
                    editorPersonaId, ctx.EditorProductUserId, settings, cancellationToken);

                // Ensure US market group for Investment Analytics (MA)
                var allGroups = await GetAllPropertyGroupsAsync(ctx.EditorProductUserId, settings.SuperUser, settings, cancellationToken);
                var usGroupId = allGroups.Groups.FirstOrDefault(g => g.GroupName == "US")?.GroupId;
                if (usGroupId is not null and not 0)
                {
                    foreach (var c in aoGbUserCompanyPropertyRoleDetails.Where(c => c.ProductName == "MA"))
                    {
                        if (!c.PropertyGroups.Contains(usGroupId.Value))
                            c.PropertyGroups.Add(usGroupId.Value);
                    }
                }
            }

            // Expand "all properties" sentinel (-1)
            if (!await _contextService.IsSuperUserAsync(persona, cancellationToken))
            {
                foreach (var item in aoGbUserCompanyPropertyRoleDetails
                    .Where(x => x.SelectedPortfolioValues?.Count > 0 && x.SelectedPortfolioValues[0] == -1))
                {
                    var propList = await GetPropertiesAsync(item.CompanyId, item.ProductName,
                        null, ctx.EditorProductUserId, 0, settings, cancellationToken);
                    item.allProperties         = true;
                    item.SelectedPortfolioValues = propList.Properties.Select(p => p.PropertyId).ToList();
                }
            }

            string returnVal = string.Empty;
            if (!aoGbUserCompanyPropertyRoleDetails.Any()) return (returnVal, activityLog);

            if (!userAOProducts.Any())
            {
                // ── New user ─────────────────────────────────────────────────
                aoUser.GroupsModel = GetBundledGroups(aoGbUserCompanyPropertyRoleDetails);
                aoUser.Divisions   = new List<Divisions>();
                aoUser.Model       = GetModel(aoGbUserCompanyPropertyRoleDetails);
                if (!aoUser.Model.Any()) aoUser.IsEnabled = false;

                returnVal = string.IsNullOrEmpty(productUserName)
                    ? await PostApiAsync($"{settings.Endpoint}user/profile/{ctx.EditorProductUserId.ToLower()}/", aoUser, settings, cancellationToken)
                    : await PutApiAsync($"{settings.Endpoint}user/profile/{ctx.EditorProductUserId.ToLower()}/", aoUser, settings, cancellationToken);

                if (string.IsNullOrEmpty(returnVal))
                {
                    var productList = aoUser.Model.Select(m => m.Product).Distinct().ToList();
                    await CreateProductUserInGreenBookAsync(editorPersonaId, productUserPersonaId, productList, effectiveLoginName.ToLower(), cancellationToken);
                }

                if (addedProducts.Any())
                    activityLog.AddRange(ExtractActivityDetailLogs(addedProducts, requiredRoles, requiredProps, requiredGroups, aoUser, aoPropsProducts));
                if (deletedProducts.Any())
                    activityLog.AddRange(ExtractActivityDetailLogs(deletedProducts, requiredRoles, requiredProps, requiredGroups, aoUser, aoPropsProducts));

                return (returnVal, activityLog);
            }

            // ── Existing user update ─────────────────────────────────────────
            var copiedDetails = await CopyRegularUserAsync(
                editorPersonaId, productUserPersonaId, settings,
                ctx.EditorProductUserId, cancellationToken: cancellationToken);
            var existingAoProducts = copiedDetails;

            await UpdateProductRolePropertyDetailsAsync(aoGbUserCompanyPropertyRoleDetails, copiedDetails, persona, cancellationToken);

            aoUser.GroupsModel = GetBundledGroups(copiedDetails);
            aoUser.Divisions   = new List<Divisions>();
            aoUser.Model       = GetModel(copiedDetails);
            aoUser.UserId      = ctx.ProductUserId.ToLower();
            aoUser.OldUserId   = ctx.ProductUserId.ToLower();

            returnVal = await PutApiAsync(
                $"{settings.Endpoint}user/profile/{ctx.EditorProductUserId.ToLower()}/",
                aoUser, settings, cancellationToken);

            if (await _contextService.IsSuperUserAsync(persona, cancellationToken))
            {
                foreach (var u in aoGbUserCompanyPropertyRoleDetails)
                    u.IsAssigned = aoProductAssigned;
            }

            if (string.IsNullOrEmpty(returnVal))
            {
                await UpdateProductUserInGreenBookAsync(
                    editorPersonaId, productUserPersonaId, effectiveLoginName.ToLower(),
                    existingAoProducts, aoGbUserCompanyPropertyRoleDetails,
                    ctx.EditorProductUserId, settings, cancellationToken, userPersona: persona);

                if (addedProducts.Any())
                    activityLog.AddRange(ExtractActivityDetailLogs(addedProducts, requiredRoles, requiredProps, requiredGroups, aoUser, aoPropsProducts));
                if (deletedProducts.Any())
                    activityLog.AddRange(ExtractActivityDetailLogs(deletedProducts, requiredRoles, requiredProps, requiredGroups, aoUser, aoPropsProducts));
            }
            else
            {
                // Check for "must have at least one company/role" error
                try
                {
                    if (await _contextService.IsSuperUserAsync(persona, cancellationToken))
                    {
                        foreach (var u in aoGbUserCompanyPropertyRoleDetails)
                            u.IsAssigned = true;
                    }

                    var jsObj = JsonConvert.DeserializeObject<dynamic>(returnVal);
                    string apiMessage = jsObj?.errorResults?[0]?.message?.Value ?? string.Empty;

                    if (apiMessage.Equals("A user must be attached to at least one company and one role",
                        StringComparison.OrdinalIgnoreCase))
                    {
                        // Disable user while keeping existing data
                        var freshCopy = await CopyRegularUserAsync(
                            editorPersonaId, productUserPersonaId, settings,
                            ctx.EditorProductUserId, cancellationToken: cancellationToken);
                        var freshExisting = freshCopy;

                        await UnAssignProductRolePropertyDetailsAsync(aoGbUserCompanyPropertyRoleDetails, freshCopy, persona, cancellationToken);
                        aoUser.GroupsModel = GetBundledGroups(freshCopy);
                        aoUser.Divisions   = new List<Divisions>();
                        aoUser.Model       = GetModel(freshCopy);
                        aoUser.IsEnabled   = false;

                        var disableResult = await PutApiAsync(
                            $"{settings.Endpoint}user/profile/{ctx.EditorProductUserId.ToLower()}/",
                            aoUser, settings, cancellationToken);

                        if (await _contextService.IsSuperUserAsync(persona, cancellationToken))
                        {
                            foreach (var u in aoGbUserCompanyPropertyRoleDetails)
                                u.IsAssigned = aoProductAssigned;
                        }

                        if (string.IsNullOrEmpty(disableResult))
                        {
                            await UpdateProductUserInGreenBookAsync(
                                editorPersonaId, productUserPersonaId, effectiveLoginName.ToLower(),
                                freshExisting, aoGbUserCompanyPropertyRoleDetails,
                                ctx.EditorProductUserId, settings, cancellationToken, userPersona: persona);
                            return (string.Empty, activityLog);
                        }

                        return ($"Error disabling user {aoUser.Login} — {disableResult}", activityLog);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "{ActionName} - {State}", "ManageAssetOptimizationUserAsync",
                        $"Error parsing PUT response for user={effectiveLoginName}");
                }
            }

            return (returnVal, activityLog);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{ActionName} - {State}", "ManageAssetOptimizationUserAsync",
                $"Exception editorPersonaId={editorPersonaId} userPersonaId={productUserPersonaId}");
            return (ex.Message, activityLog);
        }
    }

    #endregion

    #region Editor product discovery

    public async Task<IList<string>> GetGbSupportedAoEditorUserProductsToAssignAsync(
        long editorPersonaId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("{ActionName} - {State}", "GetGbSupportedAoEditorUserProductsToAssignAsync",
            $"Begin editorPersonaId={editorPersonaId}");

        var products = new List<string>();
        var samlName = await GetSamlProductUserNameAsync(editorPersonaId, AoProductId, cancellationToken);
        if (string.IsNullOrEmpty(samlName)) return products;

        var settings = await GetApiSettingsAsync(cancellationToken);
        var url      = $"{settings.Endpoint}user/divisions/{samlName.ToLower()}/";
        var divProducts = await GetApiAsync<IList<AoDivisionProduct>>(url, settings, cancellationToken);

        if (divProducts?.Count > 0)
        {
            products.AddRange(
                divProducts.SelectMany(c => c.Products)
                           .Select(p => p.Product)
                           .Where(ProductEnumHelper.CheckAoProductSupportedByGreenBook));
        }

        _logger.LogDebug("{ActionName} - {State}", "GetGbSupportedAoEditorUserProductsToAssignAsync",
            $"End count={products.Count}");
        return products;
    }

    public async Task<IList<string>> GetGbSupportedAoProductsWithUserAdminRoleAsync(
        long editorPersonaId,
        CancellationToken cancellationToken = default)
    {
        var products = new List<string>();
        var samlName = await GetSamlProductUserNameAsync(editorPersonaId, AoProductId, cancellationToken);
        if (string.IsNullOrEmpty(samlName)) return products;

        var (ctx, _) = await GetCallContextAsync(editorPersonaId, 0, cancellationToken);
        if (ctx is null) return products;

        var settings       = await GetApiSettingsAsync(cancellationToken);
        var aoCompanyInfo  = await GetAoCompanyAsync(ctx.EditorPersona.Organization.RealPageId, cancellationToken);

        var url         = $"{settings.Endpoint}user/divisions/unity/{samlName.ToLower()}/";
        var divProducts = await GetApiAsync<IList<AoDivisionProduct>>(url, settings, cancellationToken);

        if (divProducts?.Count > 0)
        {
            products.AddRange(
                divProducts.SelectMany(c => c.Products)
                           .Where(p => p.CompanyId.Equals(aoCompanyInfo.CompanyInstanceSourceId))
                           .Select(p => p.Product)
                           .Where(ProductEnumHelper.CheckAoProductSupportedByGreenBook));
        }
        return products;
    }

    public async Task<List<string>> GetAOProductsForNewMultiCompanyUserAsync(
        long editorPersonaId, string loginName,
        CancellationToken cancellationToken = default)
    {
        var products = new List<string>();
        try
        {
            var (ctx, _) = await GetCallContextAsync(editorPersonaId, 0, cancellationToken);
            if (ctx is null) return products;

            var settings      = await GetApiSettingsAsync(cancellationToken);
            var aoCompanyInfo = await GetAoCompanyAsync(ctx.EditorPersona.Organization.RealPageId, cancellationToken);

            var url     = $"{settings.Endpoint}user/ao-token?userId={loginName}";
            var objData = await GetApiAsync<AoUserConfigAuthorities>(url, settings, cancellationToken);

            if (objData != null)
            {
                products = objData.ysconfigAuthorities
                    .Where(c => c.company.Equals(aoCompanyInfo.CompanyInstanceSourceId))
                    .Select(a => a.product)
                    .Distinct()
                    .ToList();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{ActionName} - {State}", "GetAOProductsForNewMultiCompanyUserAsync",
                $"Error loginName={loginName}");
        }
        return products;
    }

    #endregion

    // ════════════════════════════════════════════════════════════════════════
    // PRIVATE ASYNC HELPERS
    // ════════════════════════════════════════════════════════════════════════

    #region Settings

    private async ValueTask<AoApiSettings> GetApiSettingsAsync(CancellationToken ct)
    {
        if (_apiSettings is not null) return _apiSettings;

        var settings = await _productSettingService.GetProductSettingAsync(AoProductId, ct);

        string Get(string name) =>
            settings.First(s => s.Name.Equals(name, StringComparison.OrdinalIgnoreCase)).Value;

        var rawPassword = Get("APIPassword");
        var password    = Encoding.UTF8.GetString(Convert.FromBase64String(rawPassword));

        _apiSettings = new AoApiSettings(
            Endpoint:         Get("APIEndPoint"),
            Username:         Get("APIUserName"),
            Password:         password,
            SuperUser:        Get("ProductSuperUserLoginName"),
            SpecialEditorUser: settings
                .FirstOrDefault(s => s.Name.Equals("AOSpecialEditorUser", StringComparison.OrdinalIgnoreCase))
                ?.Value ?? string.Empty);

        return _apiSettings;
    }

    #endregion

    #region Call context (replaces ManageProductBase.GetCompanyEditorAndUserDetails)

    private async Task<(AoCallContext? ctx, string? error)> GetCallContextAsync(
        long editorPersonaId, long userPersonaId, CancellationToken ct)
    {
        // Load editor persona
        var editorPersona = await _managePersona.GetPersonaAsync(editorPersonaId, false, ct);
        if (editorPersona is null)
            return (null, $"Editor persona {editorPersonaId} not found.");

        // Load editor SAML attributes → _editorProductUserId (from USERID attribute)
        var editorAttrs = await _samlRepository.GetProductSamlDetailsAsync(editorPersonaId, AoProductId, ct);
        string editorProductUserId = editorAttrs
            .FirstOrDefault(a => a.Name.Equals("UserId", StringComparison.OrdinalIgnoreCase))?.Value
            ?? string.Empty;

        if (string.IsNullOrEmpty(editorProductUserId))
            return (null, $"Editor (personaId={editorPersonaId}) has no AO product UserId in SAML.");

        string productUserId   = string.Empty;
        string productUsername = string.Empty;

        if (userPersonaId > 0)
        {
            var subjectAttrs = await _samlRepository.GetProductSamlDetailsAsync(userPersonaId, AoProductId, ct);
            productUserId   = subjectAttrs
                .FirstOrDefault(a => a.Name.Equals("UserId", StringComparison.OrdinalIgnoreCase))?.Value
                ?? string.Empty;
            productUsername = subjectAttrs
                .FirstOrDefault(a => a.Name.Equals("ProductUserName", StringComparison.OrdinalIgnoreCase))?.Value
                ?? string.Empty;
        }

        return (new AoCallContext(editorProductUserId, productUserId, productUsername, editorPersona), null);
    }

    #endregion

    #region BlueBook company lookup

    private async Task<CustomerCompanyMap> GetAoCompanyAsync(Guid orgRealPageId, CancellationToken ct)
    {
        var maps = await _manageBlueBook.GetProductCompanyMappingAsync(orgRealPageId, ProductSource, ct);
        return maps?.FirstOrDefault()
               ?? throw new BlueBookException($"AO company mapping not found for org {orgRealPageId}.");
    }

    #endregion

    #region HTTP helpers (IHttpClientFactory)

    private string BuildAuthHeader(AoApiSettings s) =>
        "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes($"{s.Username}:{s.Password}"));

    private async Task<T?> GetApiAsync<T>(string url, AoApiSettings s, CancellationToken ct) where T : class
    {
        var client = _httpClientFactory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Authorization = AuthenticationHeaderValue.Parse(BuildAuthHeader(s));

        using var response = await client.SendAsync(request, ct);
        if (response.IsSuccessStatusCode)
        {
            var json = await response.Content.ReadAsStringAsync(ct);
            return JsonConvert.DeserializeObject(json, typeof(T)) as T;
        }

        _logger.LogError("{ActionName} - {State}", "GetApiAsync",
            $"Non-200 response. URL={url} Status={response.StatusCode}");
        return null;
    }

    private async Task<string> PostApiAsync(string url, object body, AoApiSettings s, CancellationToken ct)
    {
        var client = _httpClientFactory.CreateClient();
        var json   = JsonConvert.SerializeObject(body);
        using var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };
        request.Headers.Authorization = AuthenticationHeaderValue.Parse(BuildAuthHeader(s));

        using var response = await client.SendAsync(request, ct);
        var content = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("{ActionName} - {State}", "PostApiAsync",
                $"Non-200. URL={url} Status={response.StatusCode} Body={content}");
            return content.Length > 0 ? "Error -" + content : string.Empty;
        }

        return content;
    }

    private async Task<string> PutApiAsync(string url, object body, AoApiSettings s, CancellationToken ct)
    {
        var client = _httpClientFactory.CreateClient();
        var json   = JsonConvert.SerializeObject(body);
        using var request = new HttpRequestMessage(HttpMethod.Put, url)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };
        request.Headers.Authorization = AuthenticationHeaderValue.Parse(BuildAuthHeader(s));

        using var response = await client.SendAsync(request, ct);
        var content = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("{ActionName} - {State}", "PutApiAsync",
                $"Non-200. URL={url} Status={response.StatusCode} Body={content}");
        }
        return response.IsSuccessStatusCode ? string.Empty : content;
    }

    #endregion

    #region SAML helpers

    private async Task<string?> GetSamlProductUserNameAsync(long personaId, int productId, CancellationToken ct)
    {
        if (personaId == 0) return null;
        var attrs = await _samlRepository.GetProductSamlDetailsAsync(personaId, productId, ct);
        return attrs.FirstOrDefault(a => a.Name.Equals("ProductUserName", StringComparison.OrdinalIgnoreCase))?.Value;
    }

    #endregion

    #region Roles (cached)

    private async Task<List<AORoles>> GetRolesAsync(
        int companyId, string productName,
        string? productUserId, string editorProductUserId,
        long userPersonaId,
        AoApiSettings settings, CancellationToken ct)
    {
        // BI product: look up by BI-specific SAML name
        string? resolvedProductUserId = productUserId;
        if (productName == "BI" && userPersonaId > 0)
            resolvedProductUserId = await GetSamlProductUserNameAsync(userPersonaId, AoBiProductId, ct);

        if (string.IsNullOrEmpty(resolvedProductUserId))
        {
            // New user — cache available roles only
            var cacheKey = $"AO_NEW_ROLES_{editorProductUserId.ToLower()}_{companyId}_{productName.ToUpper()}";
            return await _cache.GetOrCreateAsync(cacheKey, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(RoleCacheSeconds);
                var url = $"{settings.Endpoint}user/roles/available/{editorProductUserId.ToLower()}/{companyId}/{productName}";
                return await GetApiAsync<List<AORoles>>(url, settings, ct) ?? new List<AORoles>();
            }) ?? new List<AORoles>();
        }

        // Existing user — cache the available roles, then overlay assignments on a clone
        var existingCacheKey = $"AO_Existing_Roles_{editorProductUserId.ToLower()}_{companyId}_{productName.ToUpper()}";
        var allRoles = await _cache.GetOrCreateAsync(existingCacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(RoleCacheSeconds);
            var url = $"{settings.Endpoint}user/roles/available/{editorProductUserId.ToLower()}/{editorProductUserId.ToLower()}/{companyId}/{productName}";
            return await GetApiAsync<List<AORoles>>(url, settings, ct) ?? new List<AORoles>();
        }) ?? new List<AORoles>();

        // Clone before marking assignments so cache is not mutated
        var rolesSnapshot = allRoles.Select(r => new AORoles
        {
            Name        = r.Name,
            DisplayName = r.DisplayName,
            IsCustom    = r.IsCustom,
            IsAssigned  = false
        }).ToList();

        var authUrl      = $"{settings.Endpoint}user/active-authorities/{editorProductUserId.ToLower()}/{resolvedProductUserId.ToLower()}/";
        var authorities  = await GetApiAsync<List<AoActiveAuthorities>>(authUrl, settings, ct);
        return CheckAuthorities(rolesSnapshot, authorities, productName, companyId);
    }

    private static List<AORoles> CheckAuthorities(
        List<AORoles> roles,
        IList<AoActiveAuthorities>? authorities,
        string productName, int companyId)
    {
        if (authorities == null || !authorities.Any()) return roles;

        var assigned = new HashSet<string>(
            authorities.Where(a => a.Products != null)
                       .SelectMany(a => a.Products)
                       .Where(p => p.Product == productName && p.CompanyId == companyId)
                       .Select(p => p.AuthortyName?.ToLowerInvariant() ?? string.Empty),
            StringComparer.OrdinalIgnoreCase);

        foreach (var role in roles)
        {
            if (assigned.Contains(role.Name?.ToLowerInvariant() ?? string.Empty))
                role.IsAssigned = true;
        }
        return roles;
    }

    #endregion

    #region Properties (cached)

    private async Task<AoPropertyList> GetPropertiesAsync(
        long companyId, string productName,
        string? productUserId, string editorProductUserId,
        long userPersonaId,
        AoApiSettings settings, CancellationToken ct)
    {
        string? resolvedProductUserId = productUserId;
        if (productName == "BI" && userPersonaId > 0)
            resolvedProductUserId = await GetSamlProductUserNameAsync(userPersonaId, AoBiProductId, ct);

        var divisionName = ProductEnumHelper.GetAoDivisionName(ProductEnumHelper.GetAoProductEnum(productName));
        var baseUrl      = $"{settings.Endpoint}company/propertiesByDivision/{companyId}/{divisionName}?editor={editorProductUserId}";
        var cacheKey     = $"AO_Properties_{editorProductUserId.ToLower()}_{companyId}_{productName.ToUpper()}";

        // Cache only raw (unassigned) property list
        var cached = await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(PropCacheSeconds);

            var raw     = await GetApiAsync<IList<AoProperties>>(baseUrl, settings, ct);
            var rawProps = raw?.FirstOrDefault()?.Properties ?? new List<AoProperty>();

            var mapped = rawProps
                .Where(p => p.PropertyProducts?.Contains(productName) == true)
                .Select(p => new AoProperty
                {
                    CompanyId      = p.CompanyId,
                    PropertyId     = p.PropertyId,
                    PropertyName   = p.PropertyName,
                    Relationship   = p.Relationship,
                    Products       = p.Products?.Select(prod => new AoProduct
                    {
                        Product    = prod.Product,
                        IsEnabled  = prod.IsEnabled,
                        IsAssigned = prod.IsAssigned,
                        GbProductId = prod.GbProductId,
                        CompanyId  = prod.CompanyId
                    }).ToList(),
                    State          = p.State,
                    PropertyProducts = p.PropertyProducts != null ? new List<string>(p.PropertyProducts) : new List<string>(),
                    IsAssigned     = false
                })
                .ToList();

            return new AoPropertyList
            {
                Properties  = mapped,
                Division    = divisionName,
                ProductName = productName,
                allProperties = false
            };
        }) ?? new AoPropertyList { Properties = new List<AoProperty>(), Division = divisionName, ProductName = productName };

        // Work on a deep clone so cached state is never mutated
        var result = new AoPropertyList
        {
            Properties = cached.Properties.Select(p => new AoProperty
            {
                CompanyId      = p.CompanyId,
                PropertyId     = p.PropertyId,
                PropertyName   = p.PropertyName,
                Relationship   = p.Relationship,
                Products       = p.Products?.Select(prod => new AoProduct
                {
                    Product    = prod.Product,
                    IsEnabled  = prod.IsEnabled,
                    IsAssigned = prod.IsAssigned,
                    GbProductId = prod.GbProductId,
                    CompanyId  = prod.CompanyId
                }).ToList(),
                State          = p.State,
                PropertyProducts = p.PropertyProducts != null ? new List<string>(p.PropertyProducts) : new List<string>(),
                IsAssigned     = false
            }).ToList(),
            Division      = cached.Division,
            ProductName   = cached.ProductName,
            allProperties = false
        };

        if (!string.IsNullOrWhiteSpace(resolvedProductUserId))
        {
            var allPropsUrl   = $"{settings.Endpoint}user/products/{resolvedProductUserId.ToLower()}/{companyId}";
            var portfolioUrl  = $"{settings.Endpoint}user/active-portfolio/{editorProductUserId.ToLower()}/{resolvedProductUserId.ToLower()}/";

            result.allProperties = await GetAllPropertiesStatusAsync(allPropsUrl, productName, settings, ct);
            result.Properties    = await MarkAssignedPropertiesAsync(result.Properties, portfolioUrl, productName, settings, ct);
        }

        result.Properties = result.Properties.OrderBy(p => p.PropertyName).ToList();
        return result;
    }

    private async Task<bool> GetAllPropertiesStatusAsync(string url, string productName, AoApiSettings s, CancellationToken ct)
    {
        var props = await GetApiAsync<IList<AoPropertyList>>(url, s, ct);
        return props?.Any(x => x.allProperties && x.ProductName.Equals(productName)) == true;
    }

    private async Task<IList<AoProperty>> MarkAssignedPropertiesAsync(
        IList<AoProperty> allProps, string url, string productName, AoApiSettings s, CancellationToken ct)
    {
        var activePortfolio = await GetApiAsync<IList<AoProperties>>(url, s, ct);
        if (activePortfolio == null) return allProps;

        var assignedIds = activePortfolio.SelectMany(x => x.Properties)
            .Where(p => p.Products.Any(d => d.Product == productName && d.IsEnabled))
            .Select(p => p.PropertyId)
            .ToHashSet();

        foreach (var p in allProps)
        {
            if (assignedIds.Contains(p.PropertyId))
                p.IsAssigned = true;
        }
        return allProps;
    }

    private async Task<IList<int>?> GetActivePropertiesAsync(
        string editorId, string subjectId, string productName, int companyId,
        AoApiSettings s, CancellationToken ct)
    {
        var url      = $"{s.Endpoint}user/active-portfolio/{editorId.ToLower()}/{subjectId.ToLower()}/";
        var portfolio = await GetApiAsync<IList<AoProperties>>(url, s, ct);
        if (portfolio == null) return null;

        return portfolio.SelectMany(x => x.Properties)
            .Where(p => p.Products.Any(d => d.Product == productName && d.IsEnabled) && p.CompanyId == companyId)
            .Select(p => p.PropertyId)
            .ToList();
    }

    #endregion

    #region Property groups (cached)

    private async Task<AoVisiblePropertyGroups> GetAllPropertyGroupsAsync(
        string editorProductUserId, string aoSuperUser,
        AoApiSettings settings, CancellationToken ct)
    {
        var cacheKey = $"propertyGroups_AO_{editorProductUserId.ToLower()}";
        return await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(CacheTimeSeconds);
            var url = $"{settings.Endpoint}user/groups/visible/{aoSuperUser.ToLower()}/{editorProductUserId.ToLower()}/";
            return await GetApiAsync<AoVisiblePropertyGroups>(url, settings, ct)
                   ?? new AoVisiblePropertyGroups { Groups = new List<VisibleGroup>() };
        }) ?? new AoVisiblePropertyGroups { Groups = new List<VisibleGroup>() };
    }

    private async Task<IList<AoAssignableDivisionGroups>> GetAssignablePropertyGroupsAsync(
        string productName, IList<string>? selectedCompanies,
        string editorProductUserId, AoApiSettings settings, CancellationToken ct)
    {
        var companiesKey = (selectedCompanies?.Count > 0)
            ? string.Join("_", selectedCompanies.OrderBy(x => x))
            : "NONE";
        var cacheKey = $"AO_AssignableGroups_{editorProductUserId.ToLower()}_{productName.ToUpper()}_{companiesKey}";

        return await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(CacheTimeSeconds);
            var url    = $"{settings.Endpoint}user/{editorProductUserId.ToLower()}/groups/assignable?editingUser={editorProductUserId.ToLower()}";
            var result = await GetApiAsync<AoVisiblePropertyGroups>(url, settings, ct);

            var response    = new AoAssignableDivisionGroups { Groups = new List<AssignableGroup>() };
            var finalResult = new List<AoAssignableDivisionGroups>();

            if (result?.Groups != null)
            {
                foreach (var grp in result.Groups)
                {
                    response.Groups.Add(new AssignableGroup
                    {
                        PropertyGroupId = grp.GroupId,
                        GroupName       = grp.GroupName,
                        Products        = new List<DivisionGroupProduct>
                        {
                            new DivisionGroupProduct { Product = productName, Valid = true, Assigned = false }
                        }
                    });
                }
                finalResult.Add(response);
            }
            return (IList<AoAssignableDivisionGroups>)finalResult;
        }) ?? new List<AoAssignableDivisionGroups>();
    }

    #endregion

    #region Copy user helpers

    /// <inheritdoc/>
    public async Task<IList<AoUserCompanyPropertyRoleDetail>> CopyRegularUserAsync(
        long editorPersonaId, long subjectPersonaId,
        string? productUserName = null,
        CancellationToken cancellationToken = default)
    {
        var (ctx, _) = await GetCallContextAsync(editorPersonaId, 0, cancellationToken);
        if (ctx is null) return [];
        var settings = await GetApiSettingsAsync(cancellationToken);
        return await CopyRegularUserAsync(
            editorPersonaId, subjectPersonaId, settings,
            ctx.EditorProductUserId, productUserName, cancellationToken);
    }

    private async Task<IList<AoUserCompanyPropertyRoleDetail>> CopyRegularUserAsync(
        long editorPersonaId, long subjectPersonaId,
        AoApiSettings settings, string editorProductUserId,
        string? productUserName = null,
        CancellationToken cancellationToken = default)
    {
        var details = new List<AoUserCompanyPropertyRoleDetail>();

        string editorSamlName  = (await GetSamlProductUserNameAsync(editorPersonaId, AoProductId, cancellationToken))?.ToLower()
                                 ?? string.Empty;
        string subjectSamlName = !string.IsNullOrEmpty(productUserName)
            ? productUserName.ToLower()
            : (await GetSamlProductUserNameAsync(subjectPersonaId, AoProductId, cancellationToken))?.ToLower()
              ?? string.Empty;

        if (string.IsNullOrEmpty(editorSamlName) || string.IsNullOrEmpty(subjectSamlName))
            throw new InvalidOperationException(
                $"CopyRegularUser: missing product UserName for editorPersonaId={editorPersonaId} subjectPersonaId={subjectPersonaId}");

        var authUrl        = $"{settings.Endpoint}user/active-authorities/{editorSamlName}/{subjectSamlName}/";
        var authorities    = await GetApiAsync<IList<AoActiveAuthorities>>(authUrl, settings, cancellationToken)
                             ?? new List<AoActiveAuthorities>();

        var assignedProducts = GetGbSupportedAoSubjectProductsAssigned(authorities);
        var allGroups        = await GetSubjectUserAssignedPropertyGroupsAsync(editorSamlName, subjectSamlName, settings, cancellationToken);

        foreach (var product in assignedProducts)
        {
            var companyIds = GetSubjectUserAssignedCompaniesForProduct(authorities, product).Distinct();

            var productGroups = allGroups.Where(g => g.Assignments.Contains(product)).Select(g => g.GroupId).ToList();

            foreach (var companyId in companyIds)
            {
                var roleNames = authorities
                    .Where(a => a.Products != null)
                    .SelectMany(a => a.Products)
                    .Where(p => p.Product == product && p.CompanyId == companyId)
                    .Select(p => p.AuthortyName)
                    .ToList();

                var props = await GetActivePropertiesAsync(editorSamlName, subjectSamlName, product, companyId, settings, cancellationToken);

                var allPropsUrl = $"{settings.Endpoint}user/products/{subjectSamlName.ToLower()}/{companyId}";
                bool allProperties = await GetAllPropertiesStatusAsync(allPropsUrl, product, settings, cancellationToken);

                details.Add(new AoUserCompanyPropertyRoleDetail
                {
                    CompanyId              = companyId,
                    DivisionName           = ProductEnumHelper.GetAoDivisionName(ProductEnumHelper.GetAoProductEnum(product)),
                    ProductName            = product,
                    PropertyGroups         = productGroups,
                    SelectedPortfolioValues = props ?? new List<int>(),
                    SelectedRoleValues     = roleNames,
                    IsAssigned             = true,
                    allProperties          = allProperties
                });
            }
        }
        return details;
    }

    private async Task<IList<AoUserCompanyPropertyRoleDetail>> CopyEditorUserToCreateSuperUserAsync(
        long editorPersonaId, string editorProductUserId,
        AoApiSettings settings, CancellationToken ct)
    {
        var details  = new List<AoUserCompanyPropertyRoleDetail>();
        var products = await GetGbSupportedAoEditorUserProductsToAssignAsync(editorPersonaId, ct);
        var allGroups = await GetEditorUserAssignedPropertyGroupsAsync(editorPersonaId, editorProductUserId, settings, ct);

        foreach (var product in products)
        {
            var companies    = await GetEditorUserAssignedCompaniesForProductAsync(editorPersonaId, product, settings, ct);
            var productGroups = allGroups.Where(g => g.Assignments.Contains(product)).Select(g => g.GroupId).ToList();

            foreach (var company in companies)
            {
                var roles      = await GetRolesAsync(company.CompanyId, product, null, editorProductUserId, 0, settings, ct);
                var properties = await GetPropertiesAsync(company.CompanyId, product, null, editorProductUserId, 0, settings, ct);

                details.Add(new AoUserCompanyPropertyRoleDetail
                {
                    CompanyId              = company.CompanyId,
                    DivisionName           = ProductEnumHelper.GetAoDivisionName(ProductEnumHelper.GetAoProductEnum(product)),
                    ProductName            = product,
                    PropertyGroups         = productGroups,
                    SelectedPortfolioValues = properties.Properties.Select(p => p.PropertyId).ToList(),
                    SelectedRoleValues     = roles.Select(r => r.Name).ToList(),
                    IsAssigned             = true,
                    allProperties          = true
                });
            }
        }
        return details;
    }

    private async Task<IList<AoCompany>> GetEditorUserAssignedCompaniesForProductAsync(
        long editorPersonaId, string productName,
        AoApiSettings settings, CancellationToken ct)
    {
        var samlName = (await GetSamlProductUserNameAsync(editorPersonaId, AoProductId, ct))?.ToLower();
        if (string.IsNullOrEmpty(samlName))
            return new List<AoCompany>();

        var divisionName = ProductEnumHelper.GetAoDivisionName(ProductEnumHelper.GetAoProductEnum(productName));
        var profileUrl   = $"{settings.Endpoint}user/profile/{samlName}/";
        var profile      = await GetApiAsync<AOUser>(profileUrl, settings, ct);

        return profile?.Divisions
            .Where(d => d.Division == divisionName)
            .SelectMany(d => d.Companies)
            .ToList() ?? new List<AoCompany>();
    }

    private async Task<IList<Groups>> GetEditorUserAssignedPropertyGroupsAsync(
        long editorPersonaId, string editorProductUserId,
        AoApiSettings settings, CancellationToken ct)
    {
        var url     = $"{settings.Endpoint}user/profile/{editorProductUserId.ToLower()}/{editorProductUserId.ToLower()}/";
        var profile = await GetApiAsync<AOUser>(url, settings, ct);
        return profile?.Divisions.Where(d => d.Groups != null).SelectMany(d => d.Groups).ToList()
               ?? new List<Groups>();
    }

    private async Task<IList<Groups>> GetSubjectUserAssignedPropertyGroupsAsync(
        string editorProductUserId, string subjectProductUserId,
        AoApiSettings settings, CancellationToken ct)
    {
        var url     = $"{settings.Endpoint}user/profile/{editorProductUserId.ToLower()}/{subjectProductUserId.ToLower()}/";
        var profile = await GetApiAsync<AOUser>(url, settings, ct);
        return profile?.Divisions.Where(d => d.Groups != null).SelectMany(d => d.Groups).ToList()
               ?? new List<Groups>();
    }

    #endregion

    #region GreenBook write helpers

    private async Task CreateProductUserInGreenBookAsync(
        long editorPersonaId, long userPersonaId,
        IList<string> aoProductList, string productLoginName,
        CancellationToken ct)
    {
        await _samlAttributeService.UpsertAttributeAsync(userPersonaId, AoProductId, SamlAttributeEnum.productUsername, productLoginName, ct);
        await _samlAttributeService.UpsertAttributeAsync(userPersonaId, AoProductId, SamlAttributeEnum.UserId, productLoginName, ct);
        await _productSettingService.UpdateProductStatusAsync(userPersonaId, ProductStatusSettingType, AoProductId, (int)ProductBatchStatusType.Success, ct);

        foreach (var product in aoProductList)
        {
            int productId = (int)ProductEnumHelper.GetAoProductEnum(product);
            await _samlAttributeService.UpsertAttributeAsync(userPersonaId, productId, SamlAttributeEnum.productUsername, productLoginName, ct);
            await _samlAttributeService.UpsertAttributeAsync(userPersonaId, productId, SamlAttributeEnum.UserId, productLoginName, ct);
            await _productSettingService.UpdateProductStatusAsync(userPersonaId, ProductStatusSettingType, productId, (int)ProductBatchStatusType.Success, ct);
        }
    }

    private async Task UpdateProductUserInGreenBookAsync(
        long editorPersonaId, long userPersonaId,
        string productLoginName,
        IList<AoUserCompanyPropertyRoleDetail> existingAoProducts,
        IList<AoUserCompanyPropertyRoleDetail> aoUserDetails,
        string editorProductUserId, AoApiSettings settings,
        CancellationToken ct,
        bool loginNameChanged = false,
        Persona? userPersona = null)
    {
        userPersona ??= await _managePersona.GetPersonaAsync(userPersonaId, false, ct);
        var assigned   = aoUserDetails.Where(x => x.IsAssigned).Select(x => x.ProductName).Distinct().ToList();
        var unAssigned = aoUserDetails.Where(x => !x.IsAssigned).Select(x => x.ProductName).Distinct().ToList();

        // Remove BM from unassigned if not previously assigned
        var bmExisting = existingAoProducts.Where(p => p.ProductName.Equals("BM")).ToList();
        if (unAssigned.Contains("BM") && !bmExisting.Any())
            unAssigned.Remove("BM");

        if (existingAoProducts.Count == unAssigned.Count)
        {
            // All products removed — delete from GB
            await _productSettingService.UpdateProductStatusAsync(
                userPersonaId, ProductStatusSettingType, AoProductId, (int)ProductBatchStatusType.Deleted, ct);

            foreach (var item in aoUserDetails)
            {
                int pid = (int)ProductEnumHelper.GetAoProductEnum(item.ProductName);
                if (!item.IsAssigned)
                {
                    if (!await _contextService.IsSuperUserAsync(userPersona, ct))
                        await _samlAttributeService.DeleteProductInfoAndStatusAsync(userPersonaId, pid, ct);
                    await _productSettingService.UpdateProductStatusAsync(userPersonaId, ProductStatusSettingType, pid, (int)ProductBatchStatusType.Deleted, ct);
                }
                else
                {
                    await _productSettingService.UpdateProductStatusAsync(userPersonaId, ProductStatusSettingType, pid, (int)ProductBatchStatusType.Success, ct);
                }
            }
            return;
        }

        if (assigned.Any())
        {
            var existingAoAttrs = await _samlRepository.GetProductSamlDetailsAsync(userPersonaId, AoProductId, ct);
            await _productSettingService.UpdateProductStatusAsync(userPersonaId, ProductStatusSettingType, AoProductId, (int)ProductBatchStatusType.Success, ct);

            if (!existingAoAttrs.Any())
            {
                await _samlAttributeService.UpsertAttributeAsync(userPersonaId, AoProductId, SamlAttributeEnum.productUsername, productLoginName, ct);
                await _samlAttributeService.UpsertAttributeAsync(userPersonaId, AoProductId, SamlAttributeEnum.UserId, productLoginName, ct);
            }
            else if (loginNameChanged)
            {
                await _samlAttributeService.UpsertAttributesAsync(userPersonaId, AoProductId,
                    new Dictionary<SamlAttributeEnum, string>
                    {
                        [SamlAttributeEnum.productUsername] = productLoginName,
                        [SamlAttributeEnum.UserId]          = productLoginName
                    }, ct);
            }

            foreach (var product in assigned)
            {
                int pid = (int)ProductEnumHelper.GetAoProductEnum(product);
                var attrs = await _samlRepository.GetProductSamlDetailsAsync(userPersonaId, pid, ct);

                if (!attrs.Any())
                {
                    await _samlAttributeService.UpsertAttributeAsync(userPersonaId, pid, SamlAttributeEnum.productUsername, productLoginName, ct);
                    await _samlAttributeService.UpsertAttributeAsync(userPersonaId, pid, SamlAttributeEnum.UserId, productLoginName, ct);
                }
                else if (loginNameChanged)
                {
                    await _samlAttributeService.UpsertAttributesAsync(userPersonaId, pid,
                        new Dictionary<SamlAttributeEnum, string>
                        {
                            [SamlAttributeEnum.productUsername] = productLoginName,
                            [SamlAttributeEnum.UserId]          = productLoginName
                        }, ct);
                }

                await _productSettingService.UpdateProductStatusAsync(userPersonaId, ProductStatusSettingType, pid, (int)ProductBatchStatusType.Success, ct);
            }
        }

        foreach (var product in unAssigned)
        {
            int pid   = (int)ProductEnumHelper.GetAoProductEnum(product);
            var attrs = await _samlRepository.GetProductSamlDetailsAsync(userPersonaId, pid, ct);

            if (attrs.Any())
            {
                if (!await _contextService.IsSuperUserAsync(userPersona, ct))
                    await _samlAttributeService.DeleteProductInfoAndStatusAsync(userPersonaId, pid, ct);
                await _productSettingService.UpdateProductStatusAsync(userPersonaId, ProductStatusSettingType, pid, (int)ProductBatchStatusType.Deleted, ct);
            }
        }

        var remainingProducts = await GetAOProductsForNewMultiCompanyUserAsync(0, productLoginName, ct);
        if (!remainingProducts.Any())
            await _samlAttributeService.DeleteProductInfoAndStatusAsync(userPersonaId, AoProductId, ct);
    }

    #endregion

    #region UpdateProductRolePropertyDetails

    private async Task UpdateProductRolePropertyDetailsAsync(
        IList<AoUserCompanyPropertyRoleDetail> aoGbDetails,
        IList<AoUserCompanyPropertyRoleDetail> copiedDetails,
        Persona persona, CancellationToken ct)
    {
        if (aoGbDetails == null) return;

        var personaList  = await _managePersona.ListActivePersonaAsync(persona.RealPageId, false, ct);
        bool hasMultiCompany = personaList.Any(p =>
            p.OrganizationPartyId != persona.OrganizationPartyId &&
            p.Organization.RealPageId != DefaultUserClaim.ExternalCompanyRealPageId);

        var unAssigned = aoGbDetails.Where(x => !x.IsAssigned).ToList();
        var modified   = aoGbDetails.Where(x => x.IsAssigned).ToList();

        foreach (var ua in unAssigned)
        {
            var matches = copiedDetails.Where(p => p.ProductName == ua.ProductName).ToList();
            foreach (var match in matches)
            {
                if (hasMultiCompany && !persona.Organization.PrimaryOrganization)
                {
                    match.SelectedRoleValues     = new List<string>();
                    match.SelectedPortfolioValues = new List<int>();
                }
                else
                {
                    ((List<AoUserCompanyPropertyRoleDetail>)copiedDetails).Remove(match);
                }
            }
        }

        foreach (var mod in modified)
        {
            var matches = copiedDetails.Where(p => p.ProductName == mod.ProductName && p.CompanyId == mod.CompanyId).ToList();
            if (matches.Any())
            {
                foreach (var match in matches)
                {
                    ((List<AoUserCompanyPropertyRoleDetail>)copiedDetails).Remove(match);
                    ((List<AoUserCompanyPropertyRoleDetail>)copiedDetails).Add(mod);
                }
            }
            else
            {
                ((List<AoUserCompanyPropertyRoleDetail>)copiedDetails).Add(mod);
            }
        }
    }

    private async Task UnAssignProductRolePropertyDetailsAsync(
        IList<AoUserCompanyPropertyRoleDetail> aoGbDetails,
        IList<AoUserCompanyPropertyRoleDetail> copiedDetails,
        Persona persona, CancellationToken ct)
    {
        if (aoGbDetails == null) return;

        var personaList  = await _managePersona.ListActivePersonaAsync(persona.RealPageId, false, ct);
        bool hasMultiCompany = personaList.Any(p =>
            p.OrganizationPartyId != persona.OrganizationPartyId &&
            p.Organization.RealPageId != DefaultUserClaim.ExternalCompanyRealPageId);

        if (!hasMultiCompany && persona.Organization.PrimaryOrganization)
        {
            foreach (var ua in aoGbDetails.Where(x => !x.IsAssigned))
            {
                foreach (var match in copiedDetails.Where(p => p.ProductName == ua.ProductName))
                    match.SelectedRoleValues = new List<string>();
            }
        }
    }

    #endregion

    #region Email address helpers

    private async Task<string> GetUserEmailAddressAsync(
        Guid realPageId, string loginName, long personaId, CancellationToken ct,
        Persona? persona = null)
    {
        var addresses    = await _manageElectronicAddress.ListElectronicAddressForPersonAsync(realPageId, null, ct);
        string emailAddress = addresses?
            .FirstOrDefault(a => a.AddressType.Equals("EMAIL", StringComparison.OrdinalIgnoreCase))
            ?.AddressString ?? string.Empty;

        persona ??= await _managePersona.GetPersonaAsync(personaId, false, ct);
        if (await _contextService.IsRegularUserNoEmailAsync(persona, ct))
        {
            if (string.IsNullOrEmpty(emailAddress))
                return string.Empty; // caller checks for empty
        }
        else if (string.IsNullOrEmpty(emailAddress))
        {
            emailAddress = loginName;
        }

        return ValidateEmailAddress(emailAddress);
    }

    private string ValidateEmailAddress(string email)
    {
        var validator = new System.ComponentModel.DataAnnotations.EmailAddressAttribute();
        if (validator.IsValid(email)) return email;
        if (validator.IsValid(email + ".com")) return email + ".com";
        if (validator.IsValid(email + "@bogusemail.com")) return email + "@bogusemail.com";
        return email;
    }

    private async Task<bool> CheckUniqueAOUserNameAsync(string loginName, AoApiSettings s, CancellationToken ct)
    {
        var url    = $"{s.Endpoint}users/{loginName}/validation";
        var result = await GetApiAsync<dynamic>(url, s, ct);
        if (result != null) return (bool)result.exists;
        throw new InvalidOperationException($"CheckUniqueAOUserName returned null — URL={url}");
    }

    #endregion

    // ════════════════════════════════════════════════════════════════════════
    // PURE SYNC UTILITIES  (no I/O — static where possible)
    // ════════════════════════════════════════════════════════════════════════

    #region Static model helpers

    private static List<AoCompany> FilterAssignedCompanies(
        List<AoCompany> allCompanies, List<AoCompany> productUserCompanies)
    {
        if (productUserCompanies?.Count > 0)
        {
            var assignedIds = productUserCompanies.Select(c => c.CompanyId).ToHashSet();
            foreach (var c in allCompanies)
            {
                if (assignedIds.Contains(c.CompanyId))
                    c.IsAssigned = true;
            }
        }
        return allCompanies;
    }

    private static IList<Model> GetModel(IList<AoUserCompanyPropertyRoleDetail> details) =>
        details.Where(d => d.IsAssigned).Select(d => new Model
        {
            CompanyId             = d.CompanyId,
            DivisionName          = d.DivisionName,
            Product               = d.ProductName,
            SelectedPortfolioValues = d.SelectedPortfolioValues ?? new List<int>(),
            SelectedRoleValues    = d.SelectedRoleValues ?? new List<string>(),
            allProperties         = d.allProperties
        }).ToList();

    private static IList<GroupModel> GetBundledGroups(IList<AoUserCompanyPropertyRoleDetail> details)
    {
        var groups = new List<GroupModel>();
        foreach (var d in details.Where(d => d.PropertyGroups?.Any() == true))
        {
            foreach (var groupId in d.PropertyGroups)
            {
                groups.Add(new GroupModel
                {
                    Division    = d.DivisionName,
                    GroupId     = groupId,
                    ProductName = d.ProductName,
                    IsEnabled   = true
                });
            }
        }
        return groups.GroupBy(g => new { g.Division, g.GroupId, g.IsEnabled, g.ProductName })
                     .Select(g => g.First())
                     .ToList();
    }

    private static IList<AoPropertyGroups> GetPropertyGroupsForNewUser(IList<AoAssignableDivisionGroups> groups)
    {
        var result = new List<AoPropertyGroups>();
        foreach (var div in groups)
            foreach (var grp in div.Groups)
                result.Add(new AoPropertyGroups { GroupId = grp.PropertyGroupId, GroupName = grp.GroupName });
        return result;
    }

    private static IList<AoPropertyGroups> GetPropertyGroupsForExistingUser(
        IList<AoAssignableDivisionGroups> groups, AOUser userProfile, string productName)
    {
        var result = new List<AoPropertyGroups>();
        foreach (var div in groups)
            foreach (var grp in div.Groups.Where(g => g.Products.Any(p => p.Product == productName)))
                result.Add(new AoPropertyGroups { GroupId = grp.PropertyGroupId, GroupName = grp.GroupName });

        var profileGroups = userProfile?.Divisions
            .Where(d => d.Groups != null)
            .SelectMany(d => d.Groups)
            .Where(g => g.Assignments.Any(a => a.Contains(productName)))
            .ToList();

        if (profileGroups != null)
        {
            var assignedIds = profileGroups.Select(g => g.GroupId).ToHashSet();
            foreach (var r in result)
            {
                if (assignedIds.Contains(r.GroupId))
                    r.IsAssigned = true;
            }
        }
        return result;
    }

    private static IList<string> GetGbSupportedAoSubjectProductsAssigned(IList<AoActiveAuthorities> authorities)
    {
        if (authorities == null || !authorities.Any()) return new List<string>();

        return authorities.SelectMany(a => a.Products ?? new List<AoProductAuthority>())
                          .Select(p => p.Product)
                          .Distinct()
                          .Where(ProductEnumHelper.CheckAoProductSupportedByGreenBook)
                          .ToList();
    }

    private static IList<int> GetSubjectUserAssignedCompaniesForProduct(
        IList<AoActiveAuthorities> authorities, string product) =>
        authorities.SelectMany(a => a.Products ?? new List<AoProductAuthority>())
                   .Where(p => p.Product == product)
                   .Select(p => p.CompanyId)
                   .ToList();

    private static List<AdditionalParameters> ExtractActivityDetailLogs(
        List<string> productsList,
        Dictionary<string, List<ProductRole>> requiredRoles,
        Dictionary<string, List<ProductProperty>> requiredProps,
        Dictionary<string, List<AoPropertyGroup>> requiredGroups,
        AOUser aoUser,
        List<string> aoPropsProducts)
    {
        var result = new List<AdditionalParameters>();
        try
        {
            foreach (var prod in productsList)
            {
                var displayName  = ProductEnumHelper.GetAoProductDescription(ProductEnumHelper.GetAoProductEnum(prod));
                var oldRoles     = requiredRoles.GetValueOrDefault(prod)?.FindAll(r => r.IsAssigned) ?? new List<ProductRole>();
                var currentModel = aoUser.Model?.FirstOrDefault(m => m.Product == prod);
                var currentRoles = currentModel?.SelectedRoleValues ?? new List<string>();

                foreach (var r in oldRoles.Select(r => r.Name).Except(currentRoles))
                    result.Add(new AdditionalParameters { Key = displayName + " Roles", Value = RolesRemovedMsg.Replace("RoleName", r) });
                foreach (var r in currentRoles.Except(oldRoles.Select(r => r.Name)))
                    result.Add(new AdditionalParameters { Key = displayName + " Roles", Value = RolesAssignedMsg.Replace("RoleName", r) });
            }

            foreach (var prod in aoPropsProducts)
            {
                var displayName  = ProductEnumHelper.GetAoProductDescription(ProductEnumHelper.GetAoProductEnum(prod));
                var allProp      = requiredProps.GetValueOrDefault(prod) ?? new List<ProductProperty>();
                var oldPropIds   = allProp.FindAll(p => p.IsAssigned == true).Select(p => int.Parse(p.ID)).ToList();
                var currPropIds  = aoUser.Model?.FirstOrDefault(m => m.Product == prod)?.SelectedPortfolioValues ?? new List<int>();

                foreach (var id in oldPropIds.Except(currPropIds))
                {
                    var name = allProp.Find(p => p.ID == id.ToString())?.Name ?? id.ToString();
                    result.Add(new AdditionalParameters { Key = displayName + " Properties", Value = PropsRemovedMsg.Replace("PropertyName", name) });
                }
                foreach (var id in currPropIds.Except(oldPropIds))
                {
                    var name = allProp.Find(p => p.ID == id.ToString())?.Name ?? id.ToString();
                    result.Add(new AdditionalParameters { Key = displayName + " Properties", Value = PropsAssignedMsg.Replace("PropertyName", name) });
                }

                var allGrps   = requiredGroups.GetValueOrDefault(prod) ?? new List<AoPropertyGroup>();
                var oldGrpIds = allGrps.FindAll(g => g.IsAssigned).Select(g => int.Parse(g.ID)).ToList();
                var newGrpIds = aoUser.GroupsModel?.Select(g => g.GroupId).ToList() ?? new List<int>();

                foreach (var id in oldGrpIds.Except(newGrpIds))
                {
                    var name = allGrps.Find(g => g.ID == id.ToString())?.Name ?? id.ToString();
                    result.Add(new AdditionalParameters { Key = displayName + " Property Groups", Value = PropsRemovedMsg.Replace("PropertyName", name) });
                }
                foreach (var id in newGrpIds.Except(oldGrpIds))
                {
                    var name = allGrps.Find(g => g.ID == id.ToString())?.Name ?? id.ToString();
                    result.Add(new AdditionalParameters { Key = displayName + " Property Groups", Value = PropsAssignedMsg.Replace("PropertyName", name) });
                }
            }
        }
        catch (Exception)
        {
            // Activity log failure is non-fatal
        }
        return result;
    }

    #endregion
}
