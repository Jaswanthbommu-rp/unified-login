using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using UnifiedLogin.BusinessLogic.Logic.Product;
using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product.IntegrationsMarketplace;
using UnifiedLogin.SharedObjects.Saml;

namespace UnifiedLogin.BusinessLogic.LogicAsync;

/// <summary>
/// Async-first implementation of Integration Marketplace user management.
/// Replaces: sync <see cref="UnifiedLogin.BusinessLogic.Logic.Product.ManageProductIntegrationMarketplace"/>.
/// All I/O is truly async — no <c>.Result</c>, <c>.Wait()</c>, or <c>Task.Run</c> wrappers.
/// Product settings (<c>_apiEndPoint</c>, <c>_adminDefaultRole</c>) are loaded lazily on first
/// use instead of blocking in the constructor.
/// </summary>
public sealed class ManageProductIntegrationMarketplaceAsync : IManageProductIntegrationMarketplaceAsync
{
    private const int ProductId = (int)ProductEnum.IntegrationMarketplace;
    private const string ProductSettingType_ProductStatus = "ProductStatus";

    // SamlAttributeEnum.RoleCode.ToString().ToUpperInvariant()
    private const string RoleCodeAttributeName = "ROLECODE";

    private readonly DefaultUserClaim _userClaims;
    private readonly IManagePersonaAsync _managePersona;
    private readonly IManagePartyRelationshipAsync _managePartyRelationship;
    private readonly ISamlRepositoryAsync _samlRepository;
    private readonly IProductSettingServiceAsync _productSettingService;
    private readonly IProductAuditServiceAsync _productAuditService;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<ManageProductIntegrationMarketplaceAsync> _logger;

    // Lazily loaded from product internal settings on first call to any public method.
    // Replaces the blocking constructor loads from ManageProductBase._productInternalSettingList.
    private string _apiEndPoint;
    private string _adminDefaultRole;
    private volatile bool _settingsLoaded;
    private readonly SemaphoreSlim _initLock = new(1, 1);

    public ManageProductIntegrationMarketplaceAsync(
        DefaultUserClaim userClaims,
        IManagePersonaAsync managePersona,
        IManagePartyRelationshipAsync managePartyRelationship,
        ISamlRepositoryAsync samlRepository,
        IProductSettingServiceAsync productSettingService,
        IProductAuditServiceAsync productAuditService,
        IHttpClientFactory httpClientFactory,
        ILogger<ManageProductIntegrationMarketplaceAsync> logger)
    {
        _userClaims = userClaims;
        _managePersona = managePersona;
        _managePartyRelationship = managePartyRelationship;
        _samlRepository = samlRepository;
        _productSettingService = productSettingService;
        _productAuditService = productAuditService;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<ListResponse> GetRolesAsync(
        long editorPersonaId,
        long userPersonaId,
        long partyId,
        CancellationToken cancellationToken = default)
    {
        await EnsureInitializedAsync(cancellationToken);
        _logger.LogDebug("{ActionName} - {State}", "GetRolesAsync",
            $"At beginning of method for user with editorPersona id - {editorPersonaId}");

        var response = new ListResponse();
        try
        {
            ListResponse result = await GetCompanyEditorAndUserDetailsAsync(
                editorPersonaId, userPersonaId, cancellationToken);
            if (result.IsError)
            {
                _logger.LogError("{ActionName} - {State}", "GetRolesAsync",
                    $"GetCompanyEditorAndUserDetails error for user with editorPersona id - {editorPersonaId} - {result.ErrorReason}");
                return result;
            }

            var gbAllRoles = await GetIntegrationMarketplaceRolesAsync(cancellationToken);
            if (gbAllRoles == null)
            {
                _logger.LogError("{ActionName} - {State}", "GetRolesAsync",
                    $"No roles received from product for user with editorPersona id - {editorPersonaId}.");
                return new ListResponse { IsError = true, ErrorReason = "No roles received from product." };
            }

            _logger.LogDebug("{ActionName} - {State}", "GetRolesAsync",
                $"MapProductAccessGroupsToGB() completed for user with editorPersona id - {editorPersonaId}");

            if (userPersonaId != 0) // Called during updating an existing user
            {
                _logger.LogDebug("{ActionName} - {State}", "GetRolesAsync",
                    $"MergeSelRolesWithGreenbook calling for user with editorPersona id - {editorPersonaId}.");
                response = await MergeSelRolesWithGreenbookAsync(gbAllRoles, userPersonaId, cancellationToken);
                _logger.LogDebug("{ActionName} - {State}", "GetRolesAsync",
                    $"MergeSelRolesWithGreenbook completed for user with editorPersona id - {editorPersonaId}.");
            }
            else // Called during creating a new user
            {
                response = new ListResponse
                {
                    Records = gbAllRoles.Cast<object>().ToList(),
                    TotalRows = gbAllRoles.Count,
                    RowsPerPage = 9999,
                    ErrorReason = string.Empty,
                    TotalPages = 1
                };
            }
        }
        catch (Exception ex)
        {
            response.IsError = true;
            response.ErrorReason = "IntegrationMarketplace - There was a problem getting the roles.";
            _logger.LogError(ex, "{ActionName} - {State}", "GetRolesAsync",
                $"Error for user with editorPersona id - {editorPersonaId}");
        }

        return response;
    }

    /// <inheritdoc/>
    public Task<string> ChangeIntegrationMarketplaceUserTypeAsync(
        long createUserPersonaId,
        long assignUserPersonaId,
        IntegrationMarketplacePropertyRole rpList,
        BatchProcessType batchProcessType,
        CancellationToken cancellationToken = default)
        => ManageIntegrationMarketplaceUserAsync(
            createUserPersonaId, assignUserPersonaId, rpList, batchProcessType, cancellationToken);

    /// <inheritdoc/>
    public async Task<string> ManageIntegrationMarketplaceUserAsync(
        long editorPersonaId,
        long userPersonaId,
        IntegrationMarketplacePropertyRole userAssignProductPropertyRole,
        BatchProcessType batchProcessType = BatchProcessType.CreateUpdateProductUser,
        CancellationToken cancellationToken = default)
    {
        await EnsureInitializedAsync(cancellationToken);
        _logger.LogDebug("{ActionName} - {State}", "ManageIntegrationMarketplaceUserAsync",
            $"Begin create/update user for user with userPersonaId id - {userPersonaId}.");
        try
        {
            var listResponse = await GetCompanyEditorAndUserDetailsAsync(
                editorPersonaId, userPersonaId, cancellationToken);
            if (listResponse.IsError)
            {
                _logger.LogError("{ActionName} - {State}", "ManageIntegrationMarketplaceUserAsync",
                    $"Error for user with userPersonaId id - {userPersonaId}. Error - {listResponse.ErrorReason}");
                return listResponse.ErrorReason;
            }

            var userPersona = await _managePersona.GetPersonaAsync(
                userPersonaId, cancellationToken: cancellationToken);

            var roleCodeToAssign = string.Empty;

            if (await IsSuperUserAsync(userPersonaId, userPersona, cancellationToken))
            {
                _logger.LogDebug("{ActionName} - {State}", "ManageIntegrationMarketplaceUserAsync",
                    $"New user is Super user with userPersonaId id - {userPersonaId}.");
                roleCodeToAssign = _adminDefaultRole;
            }
            else
            {
                if (userAssignProductPropertyRole == null || !userAssignProductPropertyRole.RoleList.Any())
                {
                    _logger.LogError("{ActionName} - {State}", "ManageIntegrationMarketplaceUserAsync",
                        $"No roles received for user with userPersonaId id - {userPersonaId}");
                    await _productSettingService.UpdateProductStatusAsync(
                        userPersonaId, ProductSettingType_ProductStatus, ProductId,
                        (int)ProductBatchStatusType.Error, cancellationToken);
                    return $"Error - No roles received for user with userPersonaId id - {userPersonaId}";
                }

                var roleIdToAssign = Convert.ToInt32(
                    userAssignProductPropertyRole.RoleList.FirstOrDefault());

                var allImRoles = await GetIntegrationMarketplaceRolesAsync(cancellationToken);
                if (allImRoles == null || !allImRoles.Any())
                {
                    _logger.LogError("{ActionName} - {State}", "ManageIntegrationMarketplaceUserAsync",
                        "GetIntegrationMarketplaceRoles - failed to get roles.");
                    return "Not able to IM get roles.";
                }

                var roleObjectToAssign = allImRoles.FirstOrDefault(x => x.Id == roleIdToAssign);
                if (roleObjectToAssign == null)
                {
                    _logger.LogError("{ActionName} - {State}", "ManageIntegrationMarketplaceUserAsync",
                        "roleObjectToAssign - unable to convert object.");
                    return "Not able to parse roles.";
                }

                roleCodeToAssign = roleObjectToAssign.ShortName;
            }

            IList<SamlAttributes> productAttributes =
                await GetAssignedRoleForPersonaFromSamlAttributeAsync(userPersonaId, cancellationToken);

            var existingRoleCode = string.Empty;
            if (productAttributes.Any(a => a.Name.ToUpper() == RoleCodeAttributeName))
            {
                existingRoleCode = productAttributes
                    .Where(a => a.Name.ToUpper() == RoleCodeAttributeName)
                    .Select(a => a.Value)
                    .FirstOrDefault();
                _logger.LogDebug("{ActionName} - {State}", "ManageIntegrationMarketplaceUserAsync",
                    $"existingRoleCode={existingRoleCode}");
            }

            if (!string.IsNullOrEmpty(roleCodeToAssign)
                && !roleCodeToAssign.Equals(existingRoleCode, StringComparison.OrdinalIgnoreCase))
            {
                if (!string.IsNullOrEmpty(existingRoleCode)) // user already exists — update the role
                {
                    _logger.LogDebug("{ActionName} - {State}", "ManageIntegrationMarketplaceUserAsync",
                        $"Updating role for user userPersonaId id - {userPersonaId}, existingRoleCode - {existingRoleCode}.");

                    int samlUserAttributeId = productAttributes
                        .Where(a => a.Name.ToUpper() == RoleCodeAttributeName)
                        .Select(a => a.SamlUserAttributeId)
                        .FirstOrDefault();
                    _logger.LogDebug("{ActionName} - {State}", "ManageIntegrationMarketplaceUserAsync",
                        $"samlUserAttributeId={samlUserAttributeId}");

                    var samlAttributes = new SamlAttributes
                    {
                        SamlAttributeId = (int)SamlAttributeEnum.RoleCode,
                        Value = roleCodeToAssign,
                        SamlUserAttributeId = samlUserAttributeId
                    };

                    var result = await _samlRepository.UpdateSamlUserAttributeAsync(
                        samlAttributes, cancellationToken);
                    if (result.Id < 0)
                    {
                        _logger.LogError("{ActionName} - {State}", "ManageIntegrationMarketplaceUserAsync",
                            $"Unable to update role for user with userPersonaId - {userPersonaId}, existingRoleCode - {existingRoleCode}");
                        return result.ErrorMessage;
                    }
                    // fall through to the UpdateStatus + WriteUpdateUserTypeActivityLog block below
                }
                else
                {
                    // new user — create the role attribute
                    _logger.LogDebug("{ActionName} - {State}", "ManageIntegrationMarketplaceUserAsync",
                        $"Adding role for userPersonaId id - {userPersonaId}, roleCodeToAssign - {roleCodeToAssign}.");
                    var result = await _samlRepository.CreateSamlUserAttributeAsync(
                        userPersonaId, ProductId, SamlAttributeEnum.RoleCode, roleCodeToAssign, cancellationToken);
                    if (result.Id < 0)
                    {
                        _logger.LogError("{ActionName} - {State}", "ManageIntegrationMarketplaceUserAsync",
                            $"Unable to add role for user with userPersonaId - {userPersonaId}, roleCodeToAssign - {roleCodeToAssign}");
                        return result.ErrorMessage;
                    }

                    await _productSettingService.UpdateProductStatusAsync(
                        userPersonaId, ProductSettingType_ProductStatus, ProductId,
                        (int)ProductBatchStatusType.Success, cancellationToken);
                    return string.Empty; // early return — no user-type log for new user
                }
            }
            else
            {
                // no update needed — role already matches (or roleCodeToAssign is empty)
                await _productSettingService.UpdateProductStatusAsync(
                    userPersonaId, ProductSettingType_ProductStatus, ProductId,
                    (int)ProductBatchStatusType.Success, cancellationToken);

                await _productAuditService.WriteProductEventAsync(
                    editorPersonaId, userPersonaId, ProductId,
                    "No change in role for user {0} {1} in product {2} by user {3} {4}."
                    + $" old role is -{existingRoleCode} new role code {roleCodeToAssign}",
                    cancellationToken);

                return string.Empty;
            }

            // Reached only via the update-existing-role path (not new-user, not same-role)
            await _productSettingService.UpdateProductStatusAsync(
                userPersonaId, ProductSettingType_ProductStatus, ProductId,
                (int)ProductBatchStatusType.Success, cancellationToken);

            if (batchProcessType == BatchProcessType.UserTypeRegularToAdmin
                || batchProcessType == BatchProcessType.UserTypeAdminToRegular
                || batchProcessType == BatchProcessType.UserTypeAdminToExternal
                || batchProcessType == BatchProcessType.UserTypeExternalToAdmin)
            {
                await _productAuditService.WriteUserTypeChangeAsync(
                    editorPersonaId, userPersonaId, ProductId, batchProcessType, cancellationToken);
            }

            return string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{ActionName} - {State}", "ManageIntegrationMarketplaceUserAsync",
                $"Error for user with userPersonaId id - {userPersonaId}");
            await _productSettingService.UpdateProductStatusAsync(
                userPersonaId, ProductSettingType_ProductStatus, ProductId,
                (int)ProductBatchStatusType.Error, cancellationToken);
            return $"Error - {ex.Message}";
        }
    }

    /// <inheritdoc/>
    public async Task<string> UnassignUserAsync(
        long editorPersonaId,
        long userPersonaId,
        IntegrationMarketplacePropertyRole userAssignProductPropertyRole,
        CancellationToken cancellationToken = default)
    {
        var listResponse = await GetCompanyEditorAndUserDetailsAsync(
            editorPersonaId, userPersonaId, cancellationToken);
        if (listResponse.IsError)
        {
            _logger.LogError("{ActionName} - {State}", "UnassignUserAsync",
                $"Error for user with userPersonaId:{userPersonaId}. ErrorReason-{listResponse.ErrorReason}");
            return listResponse.ErrorReason;
        }

        IList<SamlAttributes> productAttributes =
            await GetAssignedRoleForPersonaFromSamlAttributeAsync(userPersonaId, cancellationToken);

        string roleCode = productAttributes
            .Where(a => a.Name.ToUpper() == RoleCodeAttributeName)
            .Select(a => a.Value)
            .FirstOrDefault() ?? string.Empty;

        _logger.LogDebug("{ActionName} - {State}", "UnassignUserAsync", $"roleCode={roleCode}");

        if (!string.IsNullOrEmpty(roleCode))
        {
            RepositoryResponse result = await _samlRepository.DeleteSamlUserProductInfoAndStatusAsync(
                userPersonaId, ProductId, cancellationToken);
            if (result.Id < 0)
            {
                _logger.LogError("{ActionName} - {State}", "UnassignUserAsync",
                    $"Unable to delete record for user with userPersonaId - {userPersonaId}, roleCode - {roleCode}");
                return result.ErrorMessage;
            }
        }

        _logger.LogInformation("{ActionName} - {State}", "UnassignUserAsync",
            $"Success userPersonaId:{userPersonaId}");

        await _productSettingService.UpdateProductStatusAsync(
            userPersonaId, ProductSettingType_ProductStatus, ProductId,
            (int)ProductBatchStatusType.Deleted, cancellationToken);

        return string.Empty;
    }

    /// <inheritdoc/>
    public async Task<List<IntegrationMarketplaceRole>> GetIntegrationMarketplaceRolesAsync(
        CancellationToken cancellationToken = default)
    {
        await EnsureInitializedAsync(cancellationToken);
        var response = await GetResultFromApiAsync<IntegrationMarketplaceRoleResponse>(
            $"{_apiEndPoint}/roles", cancellationToken);
        return response?.Data;
    }

    // ── Private helpers ────────────────────────────────────────────────────

    /// <summary>
    /// Loads <c>_apiEndPoint</c> and <c>_adminDefaultRole</c> from product internal settings
    /// on the first call; no-op on subsequent calls.
    /// Replaces the blocking constructor initialisation from
    /// <c>ManageProductBase._productInternalSettingList</c>.
    /// </summary>
    private async Task EnsureInitializedAsync(CancellationToken cancellationToken)
    {
        if (_settingsLoaded) return;
        await _initLock.WaitAsync(cancellationToken);
        try
        {
            if (_settingsLoaded) return;
            _logger.LogDebug("{ActionName} - {State}", "EnsureInitializedAsync",
                "Loading IntegrationMarketplace product settings.");

            var settings = await _productSettingService.GetProductSettingAsync(ProductId, cancellationToken);
            _apiEndPoint = settings
                .First(a => a.Name.ToUpper() == "APIENDPOINT")
                .Value;
            _adminDefaultRole = settings
                .First(a => a.Name.Equals("SystemAdminUserDefaultRole", StringComparison.OrdinalIgnoreCase))
                .Value;
            _settingsLoaded = true;

            _logger.LogDebug("{ActionName} - {State}", "EnsureInitializedAsync",
                "IntegrationMarketplace product settings loaded.");
        }
        finally
        {
            _initLock.Release();
        }
    }

    /// <summary>
    /// Async equivalent of <c>ManageProductBase.GetCompanyEditorAndUserDetails</c>.
    /// Verifies the editor persona belongs to the calling user and that the subject persona
    /// belongs to the same company.
    /// </summary>
    private async Task<ListResponse> GetCompanyEditorAndUserDetailsAsync(
        long editorPersonaId, long userPersonaId, CancellationToken cancellationToken)
    {
        if (editorPersonaId == 0)
            return new ListResponse { IsError = true, ErrorReason = "Invalid persona", TotalPages = 1 };

        var editorPersona = await _managePersona.GetPersonaAsync(
            editorPersonaId, cancellationToken: cancellationToken);
        if (editorPersona == null || editorPersona.RealPageId != _userClaims.UserRealPageGuid)
            return new ListResponse { IsError = true, ErrorReason = "Invalid persona", TotalPages = 1 };

        // Mirrors base-class SAML attribute population for the editor
        _ = await _samlRepository.GetProductSamlDetailsAsync(editorPersonaId, ProductId, cancellationToken);

        if (userPersonaId != 0)
        {
            var user = await _managePersona.GetPersonaAsync(
                userPersonaId, cancellationToken: cancellationToken);
            if (user == null || user.Organization.PartyId != editorPersona.Organization.PartyId)
                return new ListResponse { IsError = true, ErrorReason = "Invalid user persona", TotalPages = 1 };
        }

        return new ListResponse
        {
            IsError = false,
            ErrorReason = string.Empty,
            Records = new List<object> { editorPersona },
            TotalPages = 1
        };
    }

    /// <summary>
    /// Async equivalent of <c>ManageProductBase.IsSuperUser</c>.
    /// Accepts the already-fetched <paramref name="userPersona"/> to avoid a redundant DB call.
    /// </summary>
    private async Task<bool> IsSuperUserAsync(
        long userPersonaId, Persona userPersona, CancellationToken cancellationToken)
    {
        _logger.LogDebug("{ActionName} - {State}", "IsSuperUserAsync",
            $"Getting superuser status, userPersonaId={userPersonaId}");

        var partyRelationship = await _managePartyRelationship.GetPartyRelationshipAsync(
            userPersona.RealPageId,
            userPersona.Organization.RealPageId,
            roleTypeNameFrom: null,
            roleTypeNameTo: null,
            relationshipTypeName: "User Type",
            cancellationToken: cancellationToken);

        bool isSuperUser = partyRelationship != null
            && partyRelationship.RoleTypeFrom.Name.Equals("SuperUser", StringComparison.OrdinalIgnoreCase);

        _logger.LogDebug("{ActionName} - {State}", "IsSuperUserAsync",
            $"userPersonaId={userPersonaId} isSuperUser={isSuperUser}");

        return isSuperUser;
    }

    /// <summary>
    /// Async equivalent of <c>GetAssignedRoleForpersonaFromSamlAttribute</c>.
    /// </summary>
    private async Task<IList<SamlAttributes>> GetAssignedRoleForPersonaFromSamlAttributeAsync(
        long userPersonaId, CancellationToken cancellationToken)
    {
        _logger.LogDebug("{ActionName} - {State}", "GetAssignedRoleForPersonaFromSamlAttributeAsync",
            $"userPersonaId={userPersonaId}");
        return await _samlRepository.GetProductSamlDetailsAsync(userPersonaId, ProductId, cancellationToken);
    }

    /// <summary>
    /// Async equivalent of <c>GetResultFromApi&lt;T&gt;</c>.
    /// Replaces blocking <c>.GetAsync(...).Result</c> + <c>.ReadAsStringAsync().Result</c>
    /// with true <c>await</c>.
    /// Uses <see cref="IHttpClientFactory"/> instead of the base-class singleton <c>_client</c>.
    /// </summary>
    private async Task<T> GetResultFromApiAsync<T>(string urlAndQuery, CancellationToken cancellationToken)
        where T : class
    {
        using var client = _httpClientFactory.CreateClient();
        var response = await client.GetAsync(urlAndQuery, cancellationToken);
        if (!response.IsSuccessStatusCode) return null;
        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonConvert.DeserializeObject<T>(json);
    }

    /// <summary>
    /// Async equivalent of <c>MergeSelRolesWithGreenbook</c>.
    /// </summary>
    private async Task<ListResponse> MergeSelRolesWithGreenbookAsync(
        IList<IntegrationMarketplaceRole> allRoles, long userPersonaId, CancellationToken cancellationToken)
    {
        _logger.LogDebug("{ActionName} - {State}", "MergeSelRolesWithGreenbookAsync",
            $"Getting assigned user roles from GB SAMLAttributes with persona id - {userPersonaId}");

        IList<SamlAttributes> productAttributes =
            await GetAssignedRoleForPersonaFromSamlAttributeAsync(userPersonaId, cancellationToken);

        string roleCode = string.Empty;
        if (productAttributes.Any(a => a.Name.ToUpper() == RoleCodeAttributeName))
        {
            roleCode = productAttributes
                .Where(a => a.Name.ToUpper() == RoleCodeAttributeName)
                .Select(a => a.Value)
                .FirstOrDefault();
            _logger.LogDebug("{ActionName} - {State}", "MergeSelRolesWithGreenbookAsync",
                $"roleCode={roleCode}");
        }

        var selectedRole = allRoles.FirstOrDefault(
            a => a.ShortName.Equals(roleCode, StringComparison.OrdinalIgnoreCase));
        if (selectedRole != null)
            selectedRole.IsAssigned = true;

        return new ListResponse
        {
            Records = allRoles.Cast<object>().ToList(),
            TotalRows = allRoles.Count,
            RowsPerPage = 9999,
            ErrorReason = string.Empty,
            TotalPages = 1
        };
    }
}
