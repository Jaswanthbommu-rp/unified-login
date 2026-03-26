using Microsoft.Extensions.Logging;
using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Constants;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product;
using UnifiedLogin.SharedObjects.Product.UnifiedAmenities;
using UL = UnifiedLogin.SharedObjects.Product.UnifiedLogin;
using UserAssignProductPropertyRole = UnifiedLogin.SharedObjects.Product.UnifiedAmenities.UserAssignProductPropertyRole;

namespace UnifiedLogin.BusinessLogic.LogicAsync;

/// <summary>
/// Async-first implementation of Unified Amenities user management.
/// Replaces: sync <see cref="UnifiedLogin.BusinessLogic.Logic.Product.ManageUnifiedAmenities"/>.
/// All I/O is truly async — no <c>.Result</c>, <c>.Wait()</c>, or <c>Task.Run</c> wrappers.
/// <c>Parallel.ForEach</c> property fan-outs are replaced with <c>Task.WhenAll</c>.
/// </summary>
public sealed class ManageUnifiedAmenitiesAsync : IManageUnifiedAmenitiesAsync
{
    private const int ProductId = (int)ProductEnum.UnifiedAmenities;
    private const string ProductSettingType_ProductStatus = "ProductStatus";

    private readonly DefaultUserClaim _userClaims;
    private readonly IManagePersonaAsync _managePersona;
    private readonly IManagePartyRelationshipAsync _managePartyRelationship;
    private readonly IManageBlueBookAsync _blueBook;
    private readonly IManageUserRoleRightAsync _manageUserRoleRight;
    private readonly IProductRepositoryAsync _productRepository;
    private readonly IPropertyRepositoryAsync _propertyRepository;
    private readonly IUnifiedLoginRepositoryAsync _unifiedLoginRepository;
    private readonly IProductSettingServiceAsync _productSettingService;
    private readonly ISamlRepositoryAsync _samlRepository;
    private readonly ILogger<ManageUnifiedAmenitiesAsync> _logger;

    public ManageUnifiedAmenitiesAsync(
        DefaultUserClaim userClaims,
        IManagePersonaAsync managePersona,
        IManagePartyRelationshipAsync managePartyRelationship,
        IManageBlueBookAsync blueBook,
        IManageUserRoleRightAsync manageUserRoleRight,
        IProductRepositoryAsync productRepository,
        IPropertyRepositoryAsync propertyRepository,
        IUnifiedLoginRepositoryAsync unifiedLoginRepository,
        IProductSettingServiceAsync productSettingService,
        ISamlRepositoryAsync samlRepository,
        ILogger<ManageUnifiedAmenitiesAsync> logger)
    {
        _userClaims = userClaims;
        _managePersona = managePersona;
        _managePartyRelationship = managePartyRelationship;
        _blueBook = blueBook;
        _manageUserRoleRight = manageUserRoleRight;
        _productRepository = productRepository;
        _propertyRepository = propertyRepository;
        _unifiedLoginRepository = unifiedLoginRepository;
        _productSettingService = productSettingService;
        _samlRepository = samlRepository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<string> ManageUnifiedAmenitiesUserAsync(
        long editorPersonaId,
        long userPersonaId,
        UnifiedAmenitiesPropertyRole userAssignProductPropertyRole,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("{ActionName} - {State}",
            "ManageUnifiedAmenitiesUserAsync",
            $"Begin create/update user for user with userPersonaId id - {userPersonaId}.");
        try
        {
            var listResponse = await GetCompanyEditorAndUserDetailsAsync(editorPersonaId, userPersonaId, cancellationToken);
            if (listResponse.IsError)
            {
                _logger.LogError("{ActionName} - {State}",
                    "ManageUnifiedAmenitiesUserAsync",
                    $"Error for user with userPersonaId id - {userPersonaId}. Error - {listResponse.ErrorReason}");
                return listResponse.ErrorReason;
            }

            var productInternalSettingList = await _productSettingService.GetProductSettingAsync(
                (int)ProductEnum.UnifiedPlatform, cancellationToken);
            bool usePropertyInstances = productInternalSettingList
                ?.FirstOrDefault(s => s.Name.Equals("UsePropertyInstanceUnifiedAmenities", StringComparison.OrdinalIgnoreCase))
                ?.Value == "1";

            var userPersona = await _managePersona.GetPersonaAsync(userPersonaId, cancellationToken: cancellationToken);

            // super user — TODO: what to do here?
            if (await IsSuperUserAsync(userPersonaId, userPersona, cancellationToken))
            {
                _logger.LogDebug("{ActionName} - {State}",
                    "ManageUnifiedAmenitiesUserAsync",
                    $"New user is Super user with userPersonaId id - {userPersonaId}.");

                IList<int> productIdList = await _productRepository.GetProductIdsByCompanyAsync(
                    userPersona.OrganizationPartyId, cancellationToken);
                var gbAllRoles = await _productRepository.ListRolesForProductByPartyAsync(
                    userPersona.OrganizationPartyId, productIdList, ProductId, cancellationToken)
                    ?? new List<ProductRole>();

                string superUserRoleId = gbAllRoles
                    .Find(p => p.Name.ToUpper() == "MANAGE AMENITY WITH PRICING")?.ID;

                userAssignProductPropertyRole = new UnifiedAmenitiesPropertyRole
                {
                    PropertyList = new List<string> { "-1" },
                    RoleList = new List<string> { superUserRoleId }
                };
            }

            UL.Role role = new();

            if (userAssignProductPropertyRole != null)
            {
                RepositoryResponse result;
                var productPropertyRole = MapGbObjectToProduct(userAssignProductPropertyRole);
                long existingRoleId = 0;

                if (productPropertyRole.RoleList?.Count > 0)
                {
                    role.RoleID = long.Parse(productPropertyRole.RoleList[0]);

                    var roleList = await GetAssignedRoleForPersonaAsync(userPersonaId, cancellationToken);
                    if (roleList?.Count > 0)
                        existingRoleId = roleList[0].RoleID;

                    if (role.RoleID != existingRoleId)
                    {
                        if (existingRoleId != 0)
                        {
                            _logger.LogDebug("{ActionName} - {State}",
                                "ManageUnifiedAmenitiesUserAsync",
                                $"Removing role for user userPersonaId id - {userPersonaId}, RoleId - {existingRoleId}.");
                            result = await _manageUserRoleRight.InsertAssignedRoleToUserAsync(
                                userPersonaId: userPersonaId,
                                roleId: existingRoleId,
                                userId: _userClaims.UserId,
                                deleteRole: true,
                                cancellationToken: cancellationToken);
                            if (result.Id < 0)
                            {
                                _logger.LogError("{ActionName} - {State}",
                                    "ManageUnifiedAmenitiesUserAsync",
                                    $"Unable to delete role for user with userPersonaId - {userPersonaId}, RoleId - {existingRoleId}");
                                return result.ErrorMessage;
                            }
                        }

                        if (role.RoleID != 0)
                        {
                            _logger.LogDebug("{ActionName} - {State}",
                                "ManageUnifiedAmenitiesUserAsync",
                                $"Adding role for userPersonaId id - {userPersonaId}, RoleId - {role.RoleID}.");
                            result = await _manageUserRoleRight.InsertAssignedRoleToUserAsync(
                                userPersonaId: userPersonaId,
                                roleId: role.RoleID,
                                userId: _userClaims.UserId,
                                deleteRole: false,
                                cancellationToken: cancellationToken);
                            if (result.Id < 0)
                            {
                                _logger.LogError("{ActionName} - {State}",
                                    "ManageUnifiedAmenitiesUserAsync",
                                    $"Unable to add role for user with userPersonaId - {userPersonaId}, RoleId - {role.RoleID}");
                                return result.ErrorMessage;
                            }
                        }
                    }
                }

                if (userAssignProductPropertyRole.PropertyList != null
                    && userAssignProductPropertyRole.PropertyList.Count > 0
                    && userAssignProductPropertyRole.PropertyList[0].ToUpper() == "ALL")
                {
                    IList<int> productIdList = await _productRepository.GetProductIdsByCompanyAsync(
                        userPersona.OrganizationPartyId, cancellationToken);
                    var gbAllRoles = await _productRepository.ListRolesForProductByPartyAsync(
                        userPersona.OrganizationPartyId, productIdList, ProductId, cancellationToken)
                        ?? new List<ProductRole>();

                    if (gbAllRoles != null && productPropertyRole.RoleList?.Count > 0)
                    {
                        role.RoleID = long.Parse(productPropertyRole.RoleList[0]);
                        if (gbAllRoles.Any(r => long.Parse(r.ID) == role.RoleID && r.accessAllProperties))
                            userAssignProductPropertyRole.PropertyList = new List<string> { "-1" };
                    }
                }

                // Calculate property diff using HashSets for O(n) complexity
                List<ProductProperty> propertyList = await _propertyRepository.ListPropertiesByPersonaAsync(
                    userPersonaId, ProductId, cancellationToken);
                List<string> assignedPropertyList = userAssignProductPropertyRole.PropertyList;

                var currentPropertySet = new HashSet<string>(
                    propertyList.Select(p => p.ID), StringComparer.OrdinalIgnoreCase);
                var requestedPropertySet = new HashSet<string>(
                    assignedPropertyList, StringComparer.OrdinalIgnoreCase);

                var unassignedProperties = currentPropertySet.Except(requestedPropertySet).ToList();
                var newAssignedProperties = requestedPropertySet.Except(currentPropertySet).ToList();

                _logger.LogDebug("{ActionName} - {State}",
                    "ManageUnifiedAmenitiesUserAsync",
                    $"Properties to unassign: {unassignedProperties.Count}, Properties to assign: {newAssignedProperties.Count}");

                var operationResult = await ProcessPropertyOperationsAsync(
                    userPersonaId, unassignedProperties, newAssignedProperties, usePropertyInstances, cancellationToken);

                if (!string.IsNullOrEmpty(operationResult))
                {
                    _logger.LogError("{ActionName} - {State}",
                        "ManageUnifiedAmenitiesUserAsync",
                        $"Property operations failed: {operationResult}");
                    // Don't return error — log and continue (matches sync behaviour)
                }
            }

            await _productSettingService.UpdateProductStatusAsync(
                userPersonaId, ProductSettingType_ProductStatus, ProductId,
                (int)ProductBatchStatusType.Success, cancellationToken);

            return string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{ActionName} - {State}",
                "ManageUnifiedAmenitiesUserAsync",
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
        UnifiedAmenitiesPropertyRole userAssignProductPropertyRole,
        CancellationToken cancellationToken = default)
    {
        var listResponse = await GetCompanyEditorAndUserDetailsAsync(editorPersonaId, userPersonaId, cancellationToken);
        if (listResponse.IsError)
        {
            _logger.LogError("{ActionName} - {State}",
                "UnassignUserAsync",
                $"Error for user with userPersonaId:{userPersonaId}. ErrorReason-{listResponse.ErrorReason}");
            return listResponse.ErrorReason;
        }

        var roleList = await GetAssignedRoleForPersonaAsync(userPersonaId, cancellationToken);
        if (roleList?.Count > 0)
        {
            long roleId = roleList[0].RoleID;
            RepositoryResponse result = await _manageUserRoleRight.InsertAssignedRoleToUserAsync(
                userPersonaId: userPersonaId,
                roleId: roleId,
                userId: _userClaims.UserId,
                deleteRole: true,
                cancellationToken: cancellationToken);
            if (result.Id < 0)
            {
                _logger.LogError("{ActionName} - {State}",
                    "UnassignUserAsync",
                    $"Unable to delete record for user with userPersonaId - {userPersonaId}, RoleId - {roleId}");
                return result.ErrorMessage;
            }
        }

        _logger.LogInformation("{ActionName} - {State}",
            "UnassignUserAsync", $"userPersonaId:{userPersonaId}");

        await _productSettingService.UpdateProductStatusAsync(
            userPersonaId, ProductSettingType_ProductStatus, ProductId,
            (int)ProductBatchStatusType.Deleted, cancellationToken);

        return string.Empty;
    }

    /// <inheritdoc/>
    public async Task<ListResponse> GetRolesAsync(
        long editorPersonaId,
        long userPersonaId,
        long partyId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("{ActionName} - {State}",
            "GetRolesAsync",
            $"At beginning of method for user with editorPersona id - {editorPersonaId}");

        var response = new ListResponse();
        try
        {
            ListResponse result = await GetCompanyEditorAndUserDetailsAsync(editorPersonaId, userPersonaId, cancellationToken);
            if (result.IsError)
            {
                _logger.LogError("{ActionName} - {State}",
                    "GetRolesAsync",
                    $"GetCompanyEditorAndUserDetails error for user with editorPersona id - {editorPersonaId} - {result.ErrorReason}");
                return result;
            }

            _logger.LogDebug("{ActionName} - {State}",
                "GetRolesAsync",
                $"Getting all GB roles from GB DB - ocr.ListRolesByParty with party id - {partyId}");

            IList<int> productIdList = await _productRepository.GetProductIdsByCompanyAsync(partyId, cancellationToken);
            var gbAllRoles = await _productRepository.ListRolesForProductByPartyAsync(
                partyId, productIdList, ProductId, cancellationToken)
                ?? new List<ProductRole>();
            gbAllRoles = gbAllRoles.OrderBy(r => r.Name).ToList();

            _logger.LogDebug("{ActionName} - {State}",
                "GetRolesAsync",
                $"MapProductAccessGroupsToGB() completed for user with editorPersona id - {editorPersonaId}");

            if (userPersonaId != 0) // Called during updating an existing user
            {
                _logger.LogDebug("{ActionName} - {State}",
                    "GetRolesAsync",
                    $"MergeSelRolesWithGreenbook calling....for user with editorPersona id - {editorPersonaId}.");
                response = await MergeSelRolesWithGreenbookAsync(gbAllRoles, userPersonaId, cancellationToken);
                _logger.LogDebug("{ActionName} - {State}",
                    "GetRolesAsync",
                    $"MergeSelRolesWithGreenbook completed for user with editorPersona id - {editorPersonaId}.");
            }
            else // Called during creating a new user
            {
                if (gbAllRoles != null
                    && gbAllRoles.Any(s => s.DefaultRole.Equals("True", StringComparison.OrdinalIgnoreCase)))
                {
                    gbAllRoles
                        .FirstOrDefault(s => s.DefaultRole.Equals("True", StringComparison.OrdinalIgnoreCase))
                        .IsAssigned = true;
                }

                response = new ListResponse
                {
                    Records = gbAllRoles.Cast<object>().ToList(),
                    TotalRows = gbAllRoles.Count(),
                    RowsPerPage = 9999,
                    ErrorReason = string.Empty,
                    TotalPages = 1
                };
            }
        }
        catch (Exception ex)
        {
            response.IsError = true;
            response.ErrorReason = CommonMessageConstants.RoleErrorMessage;
            _logger.LogError(ex, "{ActionName} - {State}",
                "GetRolesAsync",
                $"Error for user with editorPersona id - {editorPersonaId}");
        }

        return response;
    }

    /// <inheritdoc/>
    public async Task<ListResponse> GetRightsByRoleAsync(
        long editorPersonaId,
        long partyId,
        long roleId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("{ActionName} - {State}",
            "GetRightsByRoleAsync",
            $"At beginning of method for user with editorPersona id - {editorPersonaId}");

        var response = new ListResponse();
        try
        {
            ListResponse result = await GetCompanyEditorAndUserDetailsAsync(
                editorPersonaId, editorPersonaId, cancellationToken);
            if (result.IsError)
            {
                _logger.LogError("{ActionName} - {State}",
                    "GetRightsByRoleAsync",
                    $"GetCompanyEditorAndUserDetails error for user with editorPersona id - {editorPersonaId} - {result.ErrorReason}");
                return result;
            }

            _logger.LogDebug("{ActionName} - {State}",
                "GetRightsByRoleAsync",
                $"Getting all GB rights from GB DB - ListRightsByRole with party id - {partyId}");

            IList<int> productIdList = await _productRepository.GetProductIdsByCompanyAsync(partyId, cancellationToken);
            var gbAllRights = await _unifiedLoginRepository.ListRightsByRoleAsync(
                partyId, productIdList, ProductId, roleId, cancellationToken)
                ?? new List<ProductRight>();
            gbAllRights = gbAllRights.OrderBy(r => r.Description).ToList();

            _logger.LogDebug("{ActionName} - {State}",
                "GetRightsByRoleAsync",
                $"Completed for user with editorPersona id - {editorPersonaId}");

            response = new ListResponse
            {
                Records = gbAllRights.Cast<object>().ToList(),
                TotalRows = gbAllRights.Count(),
                RowsPerPage = 9999,
                ErrorReason = string.Empty,
                TotalPages = 1
            };
        }
        catch (Exception ex)
        {
            response.IsError = true;
            response.ErrorReason = CommonMessageConstants.RightErrorMessage;
            _logger.LogError(ex, "{ActionName} - {State}",
                "GetRightsByRoleAsync",
                $"Error for user with editorPersona id - {editorPersonaId}");
        }

        return response;
    }

    /// <inheritdoc/>
    public async Task<ListResponse> GetPropertiesAsync(
        long editorPersonaId,
        long userPersonaId,
        bool assignedOnly,
        RequestParameter datafilter,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("{ActionName} - {State}",
            "GetPropertiesAsync",
            $"At beginning of method for user with editorPersona id - {editorPersonaId}");

        var response = new ListResponse();
        try
        {
            ListResponse result = await GetCompanyEditorAndUserDetailsAsync(editorPersonaId, userPersonaId, cancellationToken);
            if (result.IsError)
            {
                _logger.LogError("{ActionName} - {State}",
                    "GetPropertiesAsync",
                    $"GetCompanyEditorAndUserDetails error for user with editorPersona id - {editorPersonaId} - {result.ErrorReason}");
                return result;
            }

            var userPersona = await _managePersona.GetPersonaAsync(userPersonaId, cancellationToken: cancellationToken);
            if (userPersona == null)
            {
                _logger.LogError("{ActionName} - {State}",
                    "GetPropertiesAsync",
                    $"Unable to find persona for userPersonaId - {userPersonaId}");
                return new ListResponse { IsError = true, ErrorReason = "Invalid user persona" };
            }

            // Get properties from BlueBook
            var propertyMapList = await _blueBook.GetVCompanyPropertyMapAsync(
                userPersona.Organization.BooksCustomerMasterId, "", cancellationToken);
            IList<ProductProperty> gbPropertyList = new List<ProductProperty>();

            if (propertyMapList != null && propertyMapList.Any())
            {
                gbPropertyList = propertyMapList.Select(p => new ProductProperty
                {
                    ID = p.CustomerPropertyId.ToString(),
                    Name = p.PropertyName,
                    City = p.PropertyCity,
                    State = p.PropertyState,
                    Street1 = p.PropertyAddress,
                    Zip = string.Empty, // PostalCode not available in CustomerCompanyPropertyMap
                    IsAssigned = false,
                    Active = p.IsActive ? "1" : "0"
                }).ToList();
            }

            if (userPersonaId != 0 && assignedOnly)
            {
                List<ProductProperty> assignedProperties = await _propertyRepository.ListPropertiesByPersonaAsync(
                    userPersonaId, ProductId, cancellationToken);

                if (assignedProperties != null && assignedProperties.Any())
                {
                    foreach (var assignedProp in assignedProperties)
                    {
                        var prop = gbPropertyList.FirstOrDefault(p => p.ID == assignedProp.ID);
                        if (prop != null)
                            prop.IsAssigned = true;
                    }
                }
            }

            _logger.LogDebug("{ActionName} - {State}",
                "GetPropertiesAsync",
                $"Completed for user with editorPersona id - {editorPersonaId}");

            response = new ListResponse
            {
                Records = gbPropertyList.Cast<object>().ToList(),
                TotalRows = gbPropertyList.Count(),
                RowsPerPage = 9999,
                ErrorReason = string.Empty,
                TotalPages = 1
            };
        }
        catch (Exception ex)
        {
            response.IsError = true;
            response.ErrorReason = CommonMessageConstants.PropertyErrorMessage;
            _logger.LogError(ex, "{ActionName} - {State}",
                "GetPropertiesAsync",
                $"Error for user with editorPersona id - {editorPersonaId}");
        }

        return response;
    }

    // ── Private helpers ────────────────────────────────────────────────────

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

        // Populate SAML product attributes for the editor (mirrors base-class field population)
        _ = await _samlRepository.GetProductSamlDetailsAsync(editorPersonaId, ProductId, cancellationToken);

        if (userPersonaId != 0)
        {
            var user = await _managePersona.GetPersonaAsync(userPersonaId, cancellationToken: cancellationToken);
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
        _logger.LogDebug("{ActionName} - {State}",
            "IsSuperUserAsync",
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

        _logger.LogDebug("{ActionName} - {State}",
            "IsSuperUserAsync",
            $"userPersonaId={userPersonaId} isSuperUser={isSuperUser}");

        return isSuperUser;
    }

    /// <summary>
    /// Async equivalent of <c>ManageProductBase.GetAssignedRoleForPersona</c> for UnifiedAmenities.
    /// </summary>
    private async Task<List<UL.Role>> GetAssignedRoleForPersonaAsync(
        long userPersonaId, CancellationToken cancellationToken)
    {
        var roles = await _manageUserRoleRight.GetAssignedRoleForPersonaAsync(
            ProductEnum.UnifiedAmenities,
            userPersonaId: userPersonaId,
            cancellationToken: cancellationToken);
        return roles?.ToList() ?? new List<UL.Role>();
    }

    /// <summary>
    /// Async equivalent of the private <c>MergeSelRolesWithGreenbook</c> method.
    /// </summary>
    private async Task<ListResponse> MergeSelRolesWithGreenbookAsync(
        IList<ProductRole> allRoles, long userPersonaId, CancellationToken cancellationToken)
    {
        _logger.LogDebug("{ActionName} - {State}",
            "MergeSelRolesWithGreenbookAsync",
            $"Getting assigned user roles from GB DB - GetAssignedRoleForPersona with persona id - {userPersonaId}");

        var roleList = await GetAssignedRoleForPersonaAsync(userPersonaId, cancellationToken);

        foreach (var role in roleList)
        {
            var selRole = allRoles.FirstOrDefault(a => a.ID == role.RoleID.ToString());
            if (selRole != null)
                selRole.IsAssigned = true;
        }

        if (allRoles != null
            && !allRoles.Any(s => s.IsAssigned == true)
            && allRoles.Any(s => s.DefaultRole.Equals("True", StringComparison.OrdinalIgnoreCase)))
        {
            allRoles
                .FirstOrDefault(s => s.DefaultRole.Equals("True", StringComparison.OrdinalIgnoreCase))
                .IsAssigned = true;
        }

        return new ListResponse
        {
            Records = allRoles.Cast<object>().ToList(),
            TotalRows = allRoles.Count(),
            RowsPerPage = 9999,
            ErrorReason = string.Empty,
            TotalPages = 1
        };
    }

    /// <summary>
    /// Async replacement for <c>ProcessPropertyOperations</c>.
    /// Replaces <c>Parallel.ForEach</c> batches with <c>Task.WhenAll</c> fan-out.
    /// Errors from individual property operations are collected and returned as a summary
    /// string (matching the sync behaviour of log-and-continue, not abort).
    /// </summary>
    private async Task<string> ProcessPropertyOperationsAsync(
        long userPersonaId,
        List<string> unassignedProperties,
        List<string> assignedProperties,
        bool usePropertyInstances,
        CancellationToken cancellationToken)
    {
        var errors = new System.Collections.Concurrent.ConcurrentBag<string>();
        var sw = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            if (unassignedProperties.Count > 0)
            {
                _logger.LogDebug("{ActionName} - {State}",
                    "ProcessPropertyOperationsAsync",
                    $"Unassigning {unassignedProperties.Count} properties");

                await Task.WhenAll(unassignedProperties.Select(async property =>
                {
                    try
                    {
                        RepositoryResponse result = usePropertyInstances
                            ? await _propertyRepository.InsertRemoveAssignedPropertyInstanceToUserAsync(
                                userPersonaId, ProductId, Convert.ToInt64(property), remove: 1, cancellationToken)
                            : await _propertyRepository.InsertRemoveAssignedPropertyToUserAsync(
                                userPersonaId, ProductEnum.UnifiedAmenities, Convert.ToInt64(property), remove: 1, cancellationToken);

                        if (result.Id < 0)
                            errors.Add($"Failed to unassign property {property}: {result.ErrorMessage}");
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"Exception unassigning property {property}: {ex.Message}");
                        _logger.LogError(ex, "{ActionName} - {State}",
                            "ProcessPropertyOperationsAsync",
                            $"Error unassigning property {property}");
                    }
                }));
            }

            if (assignedProperties.Count > 0)
            {
                _logger.LogDebug("{ActionName} - {State}",
                    "ProcessPropertyOperationsAsync",
                    $"Assigning {assignedProperties.Count} properties");

                await Task.WhenAll(assignedProperties.Select(async property =>
                {
                    try
                    {
                        RepositoryResponse result = usePropertyInstances
                            ? await _propertyRepository.InsertRemoveAssignedPropertyInstanceToUserAsync(
                                userPersonaId, ProductId, Convert.ToInt64(property), remove: 0, cancellationToken)
                            : await _propertyRepository.InsertRemoveAssignedPropertyToUserAsync(
                                userPersonaId, ProductEnum.UnifiedAmenities, Convert.ToInt64(property), remove: 0, cancellationToken);

                        if (result.Id < 0)
                            errors.Add($"Failed to assign property {property}: {result.ErrorMessage}");
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"Exception assigning property {property}: {ex.Message}");
                        _logger.LogError(ex, "{ActionName} - {State}",
                            "ProcessPropertyOperationsAsync",
                            $"Error assigning property {property}");
                    }
                }));
            }

            sw.Stop();
            _logger.LogDebug("{ActionName} - {State}",
                "ProcessPropertyOperationsAsync",
                $"Completed in {sw.ElapsedMilliseconds}ms. Errors: {errors.Count}");

            if (errors.Count > 0)
            {
                var errorSummary = $"Property operations completed with {errors.Count} errors. First 5: {string.Join("; ", errors.Take(5))}";
                _logger.LogError("{ActionName} - {State}", "ProcessPropertyOperationsAsync", errorSummary);
                return errorSummary;
            }

            return string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{ActionName} - {State}",
                "ProcessPropertyOperationsAsync", "Critical error");
            return $"Critical error in property operations: {ex.Message}";
        }
    }

    // ── Pure mapping helper (no I/O) ───────────────────────────────────────

    private static UserAssignProductPropertyRole MapGbObjectToProduct(
        UnifiedAmenitiesPropertyRole userProductPropertyRole)
    {
        var result = new UserAssignProductPropertyRole();
        if (userProductPropertyRole.RoleList?.Count > 0)
        {
            result.RoleList = new List<string>();
            foreach (var roleId in userProductPropertyRole.RoleList)
                result.RoleList.Add(roleId);
        }
        return result;
    }
}
