using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using UnifiedLogin.BusinessLogic.Logic;
using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.BlackBook;
using UnifiedLogin.SharedObjects.Constants;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Extensions;
using UnifiedLogin.SharedObjects.Helper;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product;
using UnifiedLogin.SharedObjects.Product.IntelligentBuilding;
using UL = UnifiedLogin.SharedObjects.Product.UnifiedLogin;
using UserAssignProductPropertyRole = UnifiedLogin.SharedObjects.Product.IntelligentBuilding.UserAssignProductPropertyRole;

namespace UnifiedLogin.BusinessLogic.LogicAsync.Product;

/// <summary>
/// Async-first implementation of Intelligent Building user management.
/// Replaces: sync <see cref="Logic.Product.ManageIntelligentBuilding"/>.
/// All I/O is truly async — no <c>.Result</c>, <c>.Wait()</c>, or <c>Task.Run</c> wrappers.
/// <c>Parallel.ForEach</c> property fan-outs are replaced with <c>Task.WhenAll</c>.
/// </summary>
public sealed class ManageIntelligentBuildingAsync : IManageIntelligentBuildingAsync
{
    private const int ProductId = (int)ProductEnum.IntelligentBuildingTrash;
    private const string ProductSettingType_ProductStatus = "ProductStatus";

    private readonly DefaultUserClaim _userClaims;
    private readonly IProductContextServiceAsync _contextService;
    private readonly IManagePersonaAsync _managePersona;
    private readonly IManageUserLoginAsync _manageUserLogin;
    private readonly IManageBlueBookAsync _blueBook;
    private readonly IManageUserRoleRightAsync _manageUserRoleRight;
    private readonly IProductRepositoryAsync _productRepository;
    private readonly IPropertyRepositoryAsync _propertyRepository;
    private readonly ISamlRepositoryAsync _samlRepository;
    private readonly IUnifiedLoginRepositoryAsync _unifiedLoginRepository;
    private readonly IUserLoginRepositoryAsync _userLoginRepository;
    private readonly ILogger<ManageIntelligentBuildingAsync> _logger;

    public ManageIntelligentBuildingAsync(
        DefaultUserClaim userClaims,
        IProductContextServiceAsync contextService,
        IManagePersonaAsync managePersona,
        IManageUserLoginAsync manageUserLogin,
        IManageBlueBookAsync blueBook,
        IManageUserRoleRightAsync manageUserRoleRight,
        IProductRepositoryAsync productRepository,
        IPropertyRepositoryAsync propertyRepository,
        ISamlRepositoryAsync samlRepository,
        IUnifiedLoginRepositoryAsync unifiedLoginRepository,
        IUserLoginRepositoryAsync userLoginRepository,
        ILogger<ManageIntelligentBuildingAsync> logger)
    {
        _userClaims = userClaims;
        _contextService = contextService;
        _managePersona = managePersona;
        _manageUserLogin = manageUserLogin;
        _blueBook = blueBook;
        _manageUserRoleRight = manageUserRoleRight;
        _productRepository = productRepository;
        _propertyRepository = propertyRepository;
        _samlRepository = samlRepository;
        _unifiedLoginRepository = unifiedLoginRepository;
        _userLoginRepository = userLoginRepository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<string> ManageIntelligentBuildingUserAsync(
        long editorPersonaId,
        long userPersonaId,
        IBPropertyRole userAssignProductPropertyRole,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("{ActionName} - {state}", "ManageIntelligentBuildingUserAsync",
            $"Begin create/update user for user with userPersonaId id - {userPersonaId}.");
        try
        {
            var listResponse = await GetCompanyEditorAndUserDetailsAsync(editorPersonaId, userPersonaId, cancellationToken);
            if (listResponse.IsError)
            {
                _logger.LogError("{ActionName} - {state}", "ManageIntelligentBuildingUserAsync",
                    $"Error for user with userPersonaId id - {userPersonaId}. Error - {listResponse.ErrorReason}");
                return listResponse.ErrorReason;
            }

            var userPersona = await _managePersona.GetPersonaAsync(userPersonaId, cancellationToken: cancellationToken);
            var userLogin = await _manageUserLogin.GetUserLoginOnlyAsync(userPersona.RealPageId, cancellationToken);
            var userPropertyIdList = await GetAssignedUPFMPropertyIdsForPersonaAsync(userPersonaId, cancellationToken);

            // super user
            // TODO: what to do here?
            if (await _contextService.IsSuperUserAsync(userPersona, cancellationToken))
            {
                _logger.LogDebug("{ActionName} - {state}", "ManageIntelligentBuildingUserAsync",
                    $"New user is Super user with userPersonaId id - {userPersonaId}.");

                IList<int> productIdList = await _productRepository.GetProductIdsByCompanyAsync(userPersona.OrganizationPartyId, cancellationToken);
                var gbAllRoles = await _productRepository.ListRolesForProductByPartyAsync(
                    userPersona.OrganizationPartyId, productIdList, ProductId, cancellationToken)
                    ?? new List<ProductRole>();

                string superUserRoleId = gbAllRoles
                    .First(a => a.Name.Equals("Portfolio Manager", StringComparison.OrdinalIgnoreCase)).ID;

                List<string> propertiesToRemove = new();
                if (userPropertyIdList?.Count > 0)
                {
                    foreach (var prop in userPropertyIdList)
                    {
                        if (prop != -1)
                            propertiesToRemove.Add(prop.ToString());
                    }
                }

                userAssignProductPropertyRole = new IBPropertyRole
                {
                    PropertyList = new List<string> { "-1" },
                    RemovedPropertyList = propertiesToRemove,
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
                            _logger.LogDebug("{ActionName} - {state}", "ManageIntelligentBuildingUserAsync",
                                $"Removing role for user userPersonaId id - {userPersonaId}, RoleId - {existingRoleId}.");
                            result = await _manageUserRoleRight.InsertAssignedRoleToUserAsync(
                                userPersonaId: userPersonaId,
                                roleId: existingRoleId,
                                userId: _userClaims.UserId,
                                deleteRole: true,
                                cancellationToken: cancellationToken);
                            if (result.Id < 0)
                            {
                                _logger.LogError("{ActionName} - {state}", "ManageIntelligentBuildingUserAsync",
                                    $"Unable to delete role for user with userPersonaId - {userPersonaId}, RoleId - {existingRoleId}.");
                                return result.ErrorMessage;
                            }
                        }

                        if (role.RoleID != 0)
                        {
                            _logger.LogDebug("{ActionName} - {state}", "ManageIntelligentBuildingUserAsync",
                                $"Adding role for userPersonaId id - {userPersonaId}, RoleId - {role.RoleID}.");
                            result = await _manageUserRoleRight.InsertAssignedRoleToUserAsync(
                                userPersonaId: userPersonaId,
                                roleId: role.RoleID,
                                userId: _userClaims.UserId,
                                deleteRole: false,
                                cancellationToken: cancellationToken);
                            if (result.Id < 0)
                            {
                                _logger.LogError("{ActionName} - {state}", "ManageIntelligentBuildingUserAsync",
                                    $"Unable to add role for user with userPersonaId - {userPersonaId}, RoleId - {role.RoleID}.");
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
                        {
                            userAssignProductPropertyRole.PropertyList = new List<string> { "-1" };
                        }
                    }
                }

                List<string> assignedPropertyList = userAssignProductPropertyRole.PropertyList;
                List<string> unAssignedPropertyList = userAssignProductPropertyRole.RemovedPropertyList;

                List<string> unassignedProperties = new();
                List<string> assignedProperties = new();

                if (assignedPropertyList != null)
                {
                    foreach (string propertyId in assignedPropertyList)
                    {
                        if (userPropertyIdList.All(p => p != Convert.ToInt32(propertyId)))
                            assignedProperties.Add(propertyId);
                    }
                }

                if (unAssignedPropertyList != null)
                    unassignedProperties.AddRange(unAssignedPropertyList);

                if ((unAssignedPropertyList == null || unAssignedPropertyList.Count == 0) && assignedProperties?.Count > 0)
                {
                    if (userPropertyIdList.Any(p => p == -1))
                        unassignedProperties.Add("-1");
                }

                // Replace Parallel.ForEach with Task.WhenAll for true async fan-out
                if (unassignedProperties.Count > 0)
                {
                    await Task.WhenAll(unassignedProperties.Select(property =>
                        DeleteAssignedPropertyInstanceDataAsync(userPersonaId, Convert.ToInt64(property), cancellationToken)));
                }

                if (assignedProperties.Count > 0)
                {
                    await Task.WhenAll(assignedProperties.Select(property =>
                        InsertAssignedPropertyInstanceDataAsync(userPersonaId, Convert.ToInt64(property), cancellationToken)));
                }
            }

            await UpdateProductSettingProductStatusAsync(userPersonaId, ProductSettingType_ProductStatus, (int)ProductBatchStatusType.Success, cancellationToken);
            return string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{ActionName} - {state}", "ManageIntelligentBuildingUserAsync",
                $"Error for user with userPersonaId id - {userPersonaId}");
            await UpdateProductSettingProductStatusAsync(userPersonaId, ProductSettingType_ProductStatus, (int)ProductBatchStatusType.Error, cancellationToken);
            return $"Error - {ex.Message}";
        }
    }

    /// <inheritdoc/>
    public async Task<string> UnassignUserAsync(
        long editorPersonaId,
        long userPersonaId,
        IBPropertyRole userAssignProductPropertyRole,
        CancellationToken cancellationToken = default)
    {
        var listResponse = await GetCompanyEditorAndUserDetailsAsync(editorPersonaId, userPersonaId, cancellationToken);
        if (listResponse.IsError)
        {
            _logger.LogError("{ActionName} - {state}", "UnassignUserAsync",
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
                _logger.LogError("{ActionName} - {state}", "UnassignUserAsync",
                    $"Unable to delete record for user with userPersonaId - {userPersonaId}, RoleId - {roleId}");
                return result.ErrorMessage;
            }

            var propertyList = await _propertyRepository.ListPropertiesByPersonaAsync(
                userPersonaId, ProductId, cancellationToken);
            var unassignedProperties = propertyList.Select(p => p.ID.ToString()).ToList();

            if (unassignedProperties.Count > 0)
            {
                await Task.WhenAll(unassignedProperties.Select(property =>
                    DeleteAssignedPropertyInstanceDataAsync(userPersonaId, Convert.ToInt64(property), cancellationToken)));
            }
        }

        _logger.LogInformation("{ActionName} - {state}", "UnassignUserAsync",
            $"userPersonaId:{userPersonaId}");
        await UpdateProductSettingProductStatusAsync(userPersonaId, ProductSettingType_ProductStatus, (int)ProductBatchStatusType.Deleted, cancellationToken);
        return string.Empty;
    }

    /// <inheritdoc/>
    public async Task<ListResponse> GetRolesAsync(
        long editorPersonaId,
        long userPersonaId,
        long partyId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("{ActionName} - {state}", "GetRolesAsync",
            $"At beginning of method for user with editorPersona id - {editorPersonaId}");

        var response = new ListResponse();
        try
        {
            ListResponse result = await GetCompanyEditorAndUserDetailsAsync(editorPersonaId, userPersonaId, cancellationToken);
            if (result.IsError)
            {
                _logger.LogError("{ActionName} - {state}", "GetRolesAsync",
                    $"GetCompanyEditorAndUserDetails error for user with editorPersona id - {editorPersonaId} - {result.ErrorReason}");
                return result;
            }

            _logger.LogDebug("{ActionName} - {state}", "GetRolesAsync",
                $"Getting all GB roles from GB DB - ocr.ListRolesByParty with party id - {partyId}");
            IList<int> productIdList = await _productRepository.GetProductIdsByCompanyAsync(partyId, cancellationToken);
            var gbAllRoles = await _productRepository.ListRolesForProductByPartyAsync(
                partyId, productIdList, ProductId, cancellationToken)
                ?? new List<ProductRole>();
            gbAllRoles = gbAllRoles.OrderBy(r => r.Name).ToList();

            _logger.LogDebug("{ActionName} - {state}", "GetRolesAsync",
                $"Completed for user with editorPersona id - {editorPersonaId}");

            if (userPersonaId != 0) // Called during updating an existing user
            {
                _logger.LogDebug("{ActionName} - {state}", "GetRolesAsync",
                    $"MergeSelRolesWithGreenbook calling....for user with editorPersona id - {editorPersonaId}.");
                response = await MergeSelRolesWithGreenbookAsync(gbAllRoles, userPersonaId, cancellationToken);
                _logger.LogDebug("{ActionName} - {state}", "GetRolesAsync",
                    $"MergeSelRolesWithGreenbook completed for user with editorPersona id - {editorPersonaId}.");
            }
            else // Called during creating a new user
            {
                if (gbAllRoles != null
                    && gbAllRoles.Any(s => s.DefaultRole.Equals("True", StringComparison.OrdinalIgnoreCase)))
                {
                    gbAllRoles.FirstOrDefault(s => s.DefaultRole.Equals("True", StringComparison.OrdinalIgnoreCase)).IsAssigned = true;
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
            _logger.LogError(ex, "{ActionName} - {state}", "GetRolesAsync",
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
        _logger.LogDebug("{ActionName} - {state}", "GetRightsByRoleAsync",
            $"At beginning of method for user with editorPersona id - {editorPersonaId}");

        var response = new ListResponse();
        try
        {
            ListResponse result = await GetCompanyEditorAndUserDetailsAsync(editorPersonaId, editorPersonaId, cancellationToken);
            if (result.IsError)
            {
                _logger.LogError("{ActionName} - {state}", "GetRightsByRoleAsync",
                    $"GetCompanyEditorAndUserDetails error for user with editorPersona id - {editorPersonaId} - {result.ErrorReason}");
                return result;
            }

            _logger.LogDebug("{ActionName} - {state}", "GetRightsByRoleAsync",
                $"Getting all rights from GB DB - pr.ListRightsByRole with party id - {partyId}");
            IList<int> productIdList = await _productRepository.GetProductIdsByCompanyAsync(partyId, cancellationToken);
            var gbAllRights = await _unifiedLoginRepository.ListRightsByRoleAsync(
                partyId, productIdList, ProductId, roleId, cancellationToken)
                ?? new List<ProductRight>();
            gbAllRights = gbAllRights.OrderBy(r => r.Description).ToList();

            _logger.LogDebug("{ActionName} - {state}", "GetRightsByRoleAsync",
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
            _logger.LogError(ex, "{ActionName} - {state}", "GetRightsByRoleAsync",
                $"Error for user with editorPersona id - {editorPersonaId}");
        }

        return response;
    }

    /// <inheritdoc/>
    public async Task<ListResponse> GetUPFMPropertiesAsync(
        long userPersonaId,
        string include = null,
        CancellationToken cancellationToken = default)
    {
        var response = new ListResponse();
        var userPropertyIdList = await GetAssignedUPFMPropertyIdsForPersonaAsync(userPersonaId, cancellationToken);
        List<ProductProperty> userPropertyList = new();
        List<ProductProperty> translatedUserPropertyList = new();
        List<UPFMPropertyInstance> customerPropertyList = new();

        if (userPropertyIdList != null)
        {
            var booksPropertyList = await _blueBook.GetUPFMPropertyInstancesAsync(
                _userClaims.OrganizationRealPageGuid.ToString(), cancellationToken);

            if (booksPropertyList != null)
            {
                customerPropertyList = await _propertyRepository.ListUPFMPropertyInstanceIdByInstanceIdsAsync(
                    booksPropertyList, cancellationToken);
            }

            if (userPropertyIdList.Count == 1 && userPropertyIdList[0] == -1)
            {
                customerPropertyList.ForEach(cp =>
                    userPropertyList.Add(ConvertUPFMPropertyInstanceToProductProperty(cp, true)));
            }
            else
            {
                customerPropertyList
                    .FindAll(b => userPropertyIdList.Any(p => p == b.PropertyInstanceId))
                    .ForEach(cp => userPropertyList.Add(ConvertUPFMPropertyInstanceToProductProperty(cp, true)));
            }
        }

        if (userPropertyIdList?.Count > 0)
        {
            // call translate with UPFM properties to get IB property id and merge propertyInstanceId with translated id
            // note: save UPFM id into Alias field before updating with translated id
            UPFMProperty upfmProperties = new()
            {
                id = userPropertyList.Select(p => p.InstanceId.ToLower()).ToList()
            };

            var translatedData = await _blueBook.GetTranslatePropertiesFromUPFMToProductv3Async(
                upfmProperties,
                ProductEnum.IntelligentBuildingTrash.ToEnumDescription(),
                cancellationToken);

            if (translatedData != null)
            {
                foreach (var attributes in translatedData.Data.Attributes)
                {
                    foreach (var propertyData in attributes.TranslatedPropertyInstances)
                    {
                        if (propertyData.Source == ProductEnum.IntelligentBuildingTrash.ToEnumDescription())
                        {
                            var translatedProductProperty = userPropertyList
                                .FirstOrDefault(u => u.InstanceId == attributes.PropertyInstanceSourceId);
                            if (translatedProductProperty != null)
                            {
                                translatedProductProperty.ID = propertyData.PropertyInstanceSourceId;
                                translatedProductProperty.Alias = null;
                                translatedUserPropertyList.Add(translatedProductProperty);
                            }
                        }
                    }
                }
            }

            bool bIncludeFields = !string.IsNullOrWhiteSpace(include) && include.Split(',').Length > 0;
            if (bIncludeFields)
            {
                DynamicContractResolver dynamicContractResolver = new(include);
                string serialized = JsonConvert.SerializeObject(
                    translatedUserPropertyList,
                    new JsonSerializerSettings { ContractResolver = dynamicContractResolver });
                translatedUserPropertyList = JsonConvert.DeserializeObject<List<ProductProperty>>(serialized);
            }

            translatedUserPropertyList.ForEach(p => { p.IsAssigned = null; p.disableSelection = null; });

            response.IsError = false;
            response.Records = translatedUserPropertyList.Cast<object>().ToList();
            response.TotalRows = translatedUserPropertyList.Count;
            response.RowsPerPage = translatedUserPropertyList.Count;
            response.TotalPages = 1;
            response.ErrorReason = string.Empty;
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

        var editorPersona = await _managePersona.GetPersonaAsync(editorPersonaId, cancellationToken: cancellationToken);
        if (editorPersona == null || editorPersona.RealPageId != _userClaims.UserRealPageGuid)
            return new ListResponse { IsError = true, ErrorReason = "Invalid persona", TotalPages = 1 };

        // Fetch product SAML attributes for the editor (mirrors base-class field population)
        var productAttributes = await _samlRepository.GetProductSamlDetailsAsync(editorPersonaId, ProductId, cancellationToken);
        _ = productAttributes.FirstOrDefault(a => a.Name.Equals("PRODUCTUSERNAME", StringComparison.OrdinalIgnoreCase))?.Value ?? string.Empty;
        _ = productAttributes.FirstOrDefault(a => a.Name.Equals("USERID", StringComparison.OrdinalIgnoreCase))?.Value ?? string.Empty;

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
    /// Async equivalent of <c>ManageProductBase.GetAssignedRoleForPersona</c>.
    /// </summary>
    private async Task<List<UL.Role>> GetAssignedRoleForPersonaAsync(
        long userPersonaId, CancellationToken cancellationToken)
    {
        var roles = await _manageUserRoleRight.GetAssignedRoleForPersonaAsync(
            ProductEnum.IntelligentBuildingTrash,
            userPersonaId: userPersonaId,
            cancellationToken: cancellationToken);
        return roles?.ToList() ?? new List<UL.Role>();
    }

    /// <summary>
    /// Async equivalent of <c>ManageProductBase.GetAssignedUPFMPropertyIdsForPersona</c>.
    /// </summary>
    private Task<List<int>> GetAssignedUPFMPropertyIdsForPersonaAsync(
        long userPersonaId, CancellationToken cancellationToken)
        => _propertyRepository.ListUPFMPropertyInstanceIdByPersonaAsync(
            userPersonaId, ProductEnum.IntelligentBuildingTrash, cancellationToken);

    /// <summary>
    /// Async equivalent of <c>ManageProductBase.DeleteAssignedUserPropertyInstanceData</c>.
    /// </summary>
    private async Task DeleteAssignedPropertyInstanceDataAsync(
        long userPersonaId, long propertyInstanceId, CancellationToken cancellationToken)
    {
        _logger.LogDebug("{ActionName} - {state}", "DeleteAssignedPropertyInstanceDataAsync",
            $"START - userPersonaId={userPersonaId}, propertyInstanceId={propertyInstanceId}");

        var result = await _propertyRepository.InsertRemoveAssignedPropertyInstanceToUserAsync(
            userPersonaId: userPersonaId,
            productId: ProductId,
            propertyInstanceId: propertyInstanceId,
            remove: 1,
            cancellationToken: cancellationToken);

        if (result.Id < 0)
        {
            _logger.LogError("{ActionName} - {state}", "DeleteAssignedPropertyInstanceDataAsync",
                $"Unable to delete record for userPersonaId={userPersonaId}, propertyInstanceId={propertyInstanceId}");
        }
    }

    /// <summary>
    /// Async equivalent of <c>ManageProductBase.InsertAssignedUserPropertyInstanceData</c>.
    /// </summary>
    private async Task InsertAssignedPropertyInstanceDataAsync(
        long userPersonaId, long propertyInstanceId, CancellationToken cancellationToken)
    {
        _logger.LogDebug("{ActionName} - {state}", "InsertAssignedPropertyInstanceDataAsync",
            $"START - userPersonaId={userPersonaId}, propertyInstanceId={propertyInstanceId}");

        var result = await _propertyRepository.InsertRemoveAssignedPropertyInstanceToUserAsync(
            userPersonaId: userPersonaId,
            productId: ProductId,
            propertyInstanceId: propertyInstanceId,
            remove: 0,
            cancellationToken: cancellationToken);

        if (result.Id < 0)
        {
            _logger.LogError("{ActionName} - {state}", "InsertAssignedPropertyInstanceDataAsync",
                $"Unable to insert record for userPersonaId={userPersonaId}, propertyInstanceId={propertyInstanceId}");
        }
    }

    /// <summary>
    /// Async equivalent of <c>ManageProductBase.UpdateProductSettingProductStatus</c>.
    /// Fetches org status to detect disabled users and demote to Deactivated when appropriate.
    /// </summary>
    private async Task UpdateProductSettingProductStatusAsync<T>(
        long userPersonaId, string settingType, T value, CancellationToken cancellationToken)
    {
        string statusValue = value.ToString();

        if (int.TryParse(statusValue, out int statusInt)
            && (statusInt == (int)ProductBatchStatusType.Deleted || statusInt == (int)ProductBatchStatusType.Inactive))
        {
            var persona = await _managePersona.GetPersonaAsync(userPersonaId, cancellationToken: cancellationToken);
            var userLogin = await _manageUserLogin.GetUserLoginOnlyAsync(persona.RealPageId, cancellationToken);
            var orgStatus = await _userLoginRepository.GetUserOrganizationWithStatusAsync(
                userLogin.UserId, userLogin.LastLogin, persona.OrganizationPartyId, getPrimaryOrg: false);

            if (string.Equals(orgStatus.Status.ToString(), UserUiStatusType.Disabled.ToString(), StringComparison.OrdinalIgnoreCase))
                statusValue = ((int)UserUiStatusType.Deactivated).ToString();
        }

        var productSettingTypes = await _productRepository.ListProductSettingTypeAsync(cancellationToken);
        if (productSettingTypes.Any(a => a.Name.Equals(settingType, StringComparison.OrdinalIgnoreCase)))
        {
            int productStatusTypeId = productSettingTypes
                .First(a => a.Name.Equals(settingType, StringComparison.OrdinalIgnoreCase))
                .ProductSettingTypeId;
            await _productRepository.CreateProductSettingAsync(
                userPersonaId, ProductId, productStatusTypeId, statusValue, cancellationToken);
        }
    }

    /// <summary>
    /// Async equivalent of the private <c>MergeSelRolesWithGreenbook</c> method.
    /// </summary>
    private async Task<ListResponse> MergeSelRolesWithGreenbookAsync(
        IList<ProductRole> allRoles, long userPersonaId, CancellationToken cancellationToken)
    {
        _logger.LogDebug("{ActionName} - {state}", "MergeSelRolesWithGreenbookAsync",
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
            allRoles.FirstOrDefault(s => s.DefaultRole.Equals("True", StringComparison.OrdinalIgnoreCase)).IsAssigned = true;
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

    // ── Pure mapping helpers (no I/O — no async needed) ───────────────────

    private static UserAssignProductPropertyRole MapGbObjectToProduct(IBPropertyRole userProductPropertyRole)
    {
        var result = new UserAssignProductPropertyRole();
        if (userProductPropertyRole.RoleList?.Count > 0)
            result.RoleList = userProductPropertyRole.RoleList.ToList();
        return result;
    }

    private static ProductProperty ConvertUPFMPropertyInstanceToProductProperty(
        UPFMPropertyInstance upfmPropertyInstance, bool isAssigned)
        => new ProductProperty
        {
            ID = upfmPropertyInstance.CustomerPropertyId.ToString(),
            Name = upfmPropertyInstance.Name,
            Street1 = upfmPropertyInstance.Address,
            City = upfmPropertyInstance.City,
            State = upfmPropertyInstance.State,
            Zip = upfmPropertyInstance.PostalCode,
            IsAssigned = isAssigned,
            InstanceId = upfmPropertyInstance.InstanceId.ToString(),
            Latitude = upfmPropertyInstance.Latitude,
            Longitude = upfmPropertyInstance.Longitude,
            Alias = upfmPropertyInstance.PropertyInstanceId.ToString(),
            CustomerPropertyId = upfmPropertyInstance.CustomerPropertyId
        };
}
