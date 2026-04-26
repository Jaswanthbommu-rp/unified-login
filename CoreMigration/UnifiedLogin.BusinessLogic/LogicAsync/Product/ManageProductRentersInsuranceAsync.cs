using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using UnifiedLogin.BusinessLogic.Logic.Product;
using UnifiedLogin.BusinessLogic.Logic.ProductIntegration.Helpers;
using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
using UnifiedLogin.BusinessLogic.LogicAsync.Models;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects.Audit.Common;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.BlackBook;
using UnifiedLogin.SharedObjects.Constants;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Exceptions;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product;
using UnifiedLogin.SharedObjects.Product.Migration;
using UnifiedLogin.SharedObjects.Product.RentersInsurance;
using SamlAttributeEnum = UnifiedLogin.SharedObjects.Enum.SamlAttributeEnum;

namespace UnifiedLogin.BusinessLogic.LogicAsync.Product;

/// <summary>
/// True-async implementation of Renters Insurance product user management.
/// <para>
/// Replaces <c>ManageProductRentersInsurance</c> (sync stepping-stone).
/// No <c>DefaultUserClaim</c>, no mutable instance fields, no blocking <c>.Result</c> calls.
/// </para>
/// </summary>
public sealed class ManageProductRentersInsuranceAsync : IManageProductRentersInsuranceAsync
{
    #region Constants

    private static readonly int ProductId = (int)ProductEnum.Insurance;

    private const string ConfigCacheKey   = "RI_Config";
    private const string RolesAssignMsg   = "{\"action\":\"Assigned\",\"value\":\"RoleName\"}";
    private const string RolesRemovedMsg  = "{\"action\":\"Removed\",\"value\":\"RoleName\"}";
    private const string PropsAssignMsg   = "{\"action\":\"Assigned\",\"value\":\"PropertyName\"}";
    private const string PropsRemovedMsg  = "{\"action\":\"Removed\",\"value\":\"PropertyName\"}";
    private static readonly TimeSpan ConfigCacheTtl = TimeSpan.FromHours(1);

    #endregion

    #region Nested types

    /// <summary>Immutable cache-friendly config loaded once per hour from internal settings.</summary>
    internal sealed record RentersInsuranceConfig(
        string ApiUsername,
        string ApiPassword,
        int    RequestedBy);

    #endregion

    #region Dependencies

    private readonly IProductContextServiceAsync              _contextService;
    private readonly IProductInternalSettingRepositoryAsync   _internalSettingRepository;
    private readonly IProductRepositoryAsync                  _productRepository;
    private readonly IManageBlueBookAsync                     _blueBook;
    private readonly IManageContactMechanismAsync             _contactMechanism;
    private readonly IManagePersonAsync                       _managePerson;
    private readonly IManageUserLoginAsync                    _userLogin;
    private readonly ISamlAttributeServiceAsync               _samlAttributeService;
    private readonly IInsuranceService                        _insuranceService;
    private readonly IMemoryCache                             _cache;
    private readonly ILogger<ManageProductRentersInsuranceAsync> _logger;

    #endregion

    #region Constructor

    public ManageProductRentersInsuranceAsync(
        IProductContextServiceAsync              contextService,
        IProductInternalSettingRepositoryAsync   internalSettingRepository,
        IProductRepositoryAsync                  productRepository,
        IManageBlueBookAsync                     blueBook,
        IManageContactMechanismAsync             contactMechanism,
        IManagePersonAsync                       managePerson,
        IManageUserLoginAsync                    userLogin,
        ISamlAttributeServiceAsync               samlAttributeService,
        IInsuranceService                        insuranceService,
        IMemoryCache                             cache,
        ILogger<ManageProductRentersInsuranceAsync> logger)
    {
        ArgumentNullException.ThrowIfNull(contextService);
        ArgumentNullException.ThrowIfNull(internalSettingRepository);
        ArgumentNullException.ThrowIfNull(productRepository);
        ArgumentNullException.ThrowIfNull(blueBook);
        ArgumentNullException.ThrowIfNull(contactMechanism);
        ArgumentNullException.ThrowIfNull(managePerson);
        ArgumentNullException.ThrowIfNull(userLogin);
        ArgumentNullException.ThrowIfNull(samlAttributeService);
        ArgumentNullException.ThrowIfNull(insuranceService);
        ArgumentNullException.ThrowIfNull(cache);
        ArgumentNullException.ThrowIfNull(logger);

        _contextService            = contextService;
        _internalSettingRepository = internalSettingRepository;
        _productRepository         = productRepository;
        _blueBook                  = blueBook;
        _contactMechanism          = contactMechanism;
        _managePerson              = managePerson;
        _userLogin                 = userLogin;
        _samlAttributeService      = samlAttributeService;
        _insuranceService          = insuranceService;
        _cache                     = cache;
        _logger                    = logger;
    }

    #endregion

    #region Properties

    /// <inheritdoc/>
    public async Task<ListResponse> ListPropertiesAsync(
        long editorPersonaId,
        long userPersonaId,
        RequestParameter datafilter,
        CancellationToken ct = default)
    {
        var listResponse = new ListResponse();
        _logger.LogDebug("{Action} - Beginning. editorPersonaId={EditorId}", nameof(ListPropertiesAsync), editorPersonaId);

        try
        {
            var (ctx, error) = await _contextService.GetUserContextAsync(
                editorPersonaId, userPersonaId, ProductId, ct);
            if (error is not null) return error;

            var companyMap = await GetRentersInsuranceCompanyMapAsync(ctx!, ct);
            long companyInstanceId = companyMap?.CompanyInstanceId ?? 0;

            if (companyInstanceId == 0)
            {
                _logger.LogError("{Action} - Company instance id not found. editorPersonaId={EditorId}", nameof(ListPropertiesAsync), editorPersonaId);
                return new ListResponse { IsError = true, ErrorReason = "Company Setup Error: Please Contact Support." };
            }

            var companyProperties = await _blueBook.GetCompanyPropertyInstanceAsync(companyInstanceId, ct);
            var blueBookPropertyList = companyProperties?.MapBlueBookToGBProperties() ?? [];

            // Existing user — merge assigned flags from the insurance API
            if (userPersonaId != 0 && !string.IsNullOrWhiteSpace(ctx!.ProductUserId))
            {
                listResponse = await MergeProductPropertiesAsync(
                    ctx.ProductUserId, blueBookPropertyList, ct);
            }
            else
            {
                listResponse = new ListResponse
                {
                    Records     = blueBookPropertyList.Cast<object>().ToList(),
                    TotalRows   = blueBookPropertyList.Count,
                    RowsPerPage = blueBookPropertyList.Count,
                    TotalPages  = 1,
                    ErrorReason = string.Empty,
                    Additional  = new Dictionary<string, bool> { ["allProperties"] = false }
                };
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            listResponse.IsError = true;
            listResponse.ErrorReason = ex is BlueBookException
                ? ex.Message
                : CommonMessageConstants.PropertyErrorMessage;
            _logger.LogError(ex, "{Action} - Error. editorPersonaId={EditorId}", nameof(ListPropertiesAsync), editorPersonaId);
        }

        return listResponse;
    }

    /// <inheritdoc/>
    public async Task<ObjectListOutput<PropertyInstance, IErrorData>> ListPropertiesByPMCIDAsync(
        long editorPersonaId,
        long userPersonaId,
        CancellationToken ct = default)
    {
        var output    = new ObjectListOutput<PropertyInstance, IErrorData>();
        var errStatus = new Status<IErrorData>();
        output.Status = errStatus;

        _logger.LogDebug("{Action} - Beginning. editorPersonaId={EditorId}", nameof(ListPropertiesByPMCIDAsync), editorPersonaId);

        try
        {
            var (ctx, error) = await _contextService.GetUserContextAsync(
                editorPersonaId, userPersonaId, ProductId, ct);
            if (error is not null)
            {
                errStatus.Success  = false;
                errStatus.ErrorMsg = error.ErrorReason;
                output.Status      = errStatus;
                return output;
            }

            var companyMap = await GetRentersInsuranceCompanyMapAsync(ctx!, ct);
            long companyInstanceId = companyMap?.CompanyInstanceId ?? 0;

            if (companyInstanceId == 0)
            {
                _logger.LogError("{Action} - Company instance not found. editorPersonaId={EditorId}", nameof(ListPropertiesByPMCIDAsync), editorPersonaId);
                errStatus.Success  = false;
                errStatus.ErrorMsg = "Company Setup Error: Please Contact Support.";
                output.Status      = errStatus;
                return output;
            }

            var propertyList = (await _blueBook.GetPropertyInstanceAsync(companyInstanceId, ct)).ToList();

            var listPropertyByPMCIDResponse = _insuranceService.GetListPropertyByPMCID((int)companyInstanceId);
            if (listPropertyByPMCIDResponse?.PropertyList is not null)
            {
                foreach (var bb in propertyList)
                    bb.IsActive = listPropertyByPMCIDResponse.PropertyList.Any(
                        ri => ri.PropertyID.ToString() == bb.PropertyInstanceSourceId);
            }

            errStatus.Success  = true;
            errStatus.ErrorMsg = string.Empty;
            output.Status      = errStatus;
            output.list        = propertyList;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            errStatus.Success  = false;
            errStatus.ErrorMsg = ex is BlueBookException
                ? ex.Message
                : $"ManageProductRentersInsuranceAsync.{nameof(ListPropertiesByPMCIDAsync)} - Error {ex.Message}";
            output.Status      = errStatus;
            _logger.LogError(ex, "{Action} - Error. editorPersonaId={EditorId}", nameof(ListPropertiesByPMCIDAsync), editorPersonaId);
        }

        return output;
    }

    #endregion

    #region Roles

    /// <inheritdoc/>
    public async Task<ListResponse> ListRolesAsync(
        long editorPersonaId,
        long userPersonaId,
        CancellationToken ct = default)
    {
        var (ctx, error) = await _contextService.GetUserContextAsync(
            editorPersonaId, userPersonaId, ProductId, ct);
        if (error is not null) return error;

        var config = await GetConfigAsync(ct);
        GetUserByIDResponse getUserByIDResponse = new();

        if (!string.IsNullOrWhiteSpace(ctx!.ProductUserId))
        {
            var userActionRequest = BuildUserActionRequest(config, Convert.ToInt32(ctx.ProductUserId));
            getUserByIDResponse = _insuranceService.GetUserByID(userActionRequest);
        }

        var listOfUserRolesResponse = _insuranceService.GetListOfUserRoles();
        var productRoleList = listOfUserRolesResponse.ToGBRoles();

        if (getUserByIDResponse?.UserInfo is not null)
        {
            var assignedRole = productRoleList.FirstOrDefault(
                item => item.Name == getUserByIDResponse.UserInfo.Role);
            if (assignedRole is not null)
                assignedRole.IsAssigned = true;
        }

        // Apply UI display-name overrides
        foreach (var item in productRoleList)
        {
            item.Name = Convert.ToInt32(item.ID) switch
            {
                2  => "Corporate User",
                21 => "Corporate User with RPX",
                22 => "Property Manager with RPX",
                _  => item.Name
            };
        }

        return new ListResponse
        {
            Records     = productRoleList.Cast<object>().ToList(),
            TotalRows   = productRoleList.Count,
            RowsPerPage = productRoleList.Count,
            TotalPages  = 1,
            ErrorReason = string.Empty
        };
    }

    #endregion

    #region Create / Update

    /// <inheritdoc/>
    public async Task<(ObjectOutput<UserAPIResponse, IErrorData> result, List<AdditionalParameters> auditParams)> ManageRentersInsuranceUserAsync(
        long editorPersonaId,
        long userPersonaId,
        RentersInsuranceRoleAndPropertyList rentersInsuranceRoleAndPropertyList,
        BatchProcessType batchProcessType = BatchProcessType.CreateUpdateProductUser,
        CancellationToken ct = default)
    {
        List<AdditionalParameters> auditParams = [];
        var output    = new ObjectOutput<UserAPIResponse, IErrorData>();
        var errStatus = new Status<IErrorData>();

        _logger.LogDebug("{Action} - Begin create/update. userPersonaId={UserId}", nameof(ManageRentersInsuranceUserAsync), userPersonaId);

        try
        {
            var (ctx, contextError) = await _contextService.GetUserContextAsync(
                editorPersonaId, userPersonaId, ProductId, ct);
            if (contextError is not null)
            {
                errStatus.Success  = false;
                errStatus.ErrorMsg = contextError.ErrorReason;
                output.Status      = errStatus;
                return (output, auditParams);
            }

            // Parallel: config + person + user login + company map
            var configTask      = GetConfigAsync(ct).AsTask();
            var personTask      = _managePerson.GetPersonAsync(ctx!.UserPersona!.RealPageId, ct);
            var userLoginTask   = _userLogin.GetUserLoginOnlyAsync(ctx.UserPersona.RealPageId, ct);
            var companyMapTask  = GetRentersInsuranceCompanyMapAsync(ctx, ct);

            await Task.WhenAll(configTask, personTask, userLoginTask, companyMapTask);

            var config     = await configTask;
            var person     = await personTask;
            var userLogin  = await userLoginTask;
            var companyMap = await companyMapTask;

            long companyInstanceSourceId = Convert.ToInt64(companyMap?.CompanyInstanceSourceId ?? "0");
            long companyInstanceId       = companyMap?.CompanyInstanceId ?? 0;

            var companyProperties    = await _blueBook.GetCompanyPropertyInstanceAsync(companyInstanceId, ct);
            var blueBookPropertyList = companyProperties?.MapBlueBookToGBProperties() ?? [];

            // ── Email / username resolution ─────────────────────────────────
            string userEmailAddress = string.Empty;
            string productUserName  = string.Empty;

            bool isNonEmailLogin = !userLogin!.LoginName.Contains('@');
            if (userPersonaId > 0 && isNonEmailLogin)
            {
                productUserName = userLogin.LoginName;
                var addresses = await _contactMechanism.ListContactMechanismForPersonAsync(
                    userLogin.RealPageId, string.Empty, ct);
                userEmailAddress = addresses?
                    .FirstOrDefault(a => a.AddressType.Equals("EMAIL", StringComparison.OrdinalIgnoreCase))
                    ?.AddressString ?? string.Empty;
            }
            else
            {
                userEmailAddress = IsValidEmail(userLogin.LoginName) ? userLogin.LoginName : string.Empty;
                productUserName  = userEmailAddress;
            }

            string userEmail = string.IsNullOrWhiteSpace(userEmailAddress)
                ? userEmailAddress
                : userEmailAddress[..Math.Min(userEmailAddress.Length, 155)];

            // ── Existing vs new user ────────────────────────────────────────
            GetUserByIDResponse getUserByIDResponse = new();

            if (string.IsNullOrWhiteSpace(ctx.ProductUserId))
            {
                // New user: find a unique login name
                bool foundUserName = false;
                int  incrementor   = 0;
                string newProductUsername = productUserName;
                var checkLogin = new CheckUserLoginExists
                {
                    Login       = config.ApiUsername,
                    Password    = config.ApiPassword,
                    RequestedBy = config.RequestedBy,
                    UserLogin   = newProductUsername
                };

                while (!foundUserName)
                {
                    var checkResult = _insuranceService.CheckUserLogin(checkLogin);
                    if (checkResult != null && checkResult.ErrorCode == "-1")
                    {
                        incrementor++;
                        string[] parts = newProductUsername.Split('@');
                        newProductUsername = parts.Length == 2
                            ? $"{parts[0]}{incrementor}@{parts[1]}"
                            : $"{newProductUsername}{incrementor}";
                        checkLogin.UserLogin = newProductUsername;
                    }
                    else
                    {
                        foundUserName = true;
                    }
                }

                productUserName = newProductUsername;
            }
            else
            {
                productUserName = ctx.ProductUsername;
                if (batchProcessType == BatchProcessType.ProfileUpdate)
                {
                    rentersInsuranceRoleAndPropertyList = new RentersInsuranceRoleAndPropertyList();
                    getUserByIDResponse = _insuranceService.GetUserByID(
                        BuildUserActionRequest(config, Convert.ToInt32(ctx.ProductUserId))) ?? new();
                    if (getUserByIDResponse.UserInfo is not null)
                    {
                        rentersInsuranceRoleAndPropertyList.RoleList =
                            [getUserByIDResponse.UserInfo.RoleID.ToString()];
                    }
                }
            }

            var userBeforeUpdate = !string.IsNullOrEmpty(ctx.ProductUserId)
                ? _insuranceService.GetUserByID(BuildUserActionRequest(config, Convert.ToInt32(ctx.ProductUserId)))
                : new GetUserByIDResponse();

            // ── Property list ────────────────────────────────────────────────
            IList<UserProperty> userPropertyList = BuildUserPropertyList(
                rentersInsuranceRoleAndPropertyList, batchProcessType, getUserByIDResponse, blueBookPropertyList);

            // ── UserInfo ─────────────────────────────────────────────────────
            var userInfo = new UserInfo
            {
                CompanyId     = Convert.ToInt32(companyInstanceSourceId),
                DateLastLogin = null,
                Email         = userEmail,
                FailedCounter = 0,
                FirstName     = TruncateTo50(person.FirstName),
                IsActive      = true,
                LastName      = TruncateTo50(person.LastName),
                RoleID        = rentersInsuranceRoleAndPropertyList.RoleList?.Count > 0
                                    ? Convert.ToInt32(rentersInsuranceRoleAndPropertyList.RoleList[0])
                                    : 2,
                Role         = null,
                UserId       = !string.IsNullOrWhiteSpace(ctx.ProductUserId)
                                    ? Convert.ToInt32(ctx.ProductUserId)
                                    : 0,
                User         = productUserName,
                PropertyList = userPropertyList.ToArray()
            };

            var addUpdateRequest = new AddUpdateUserRequest
            {
                Login       = config.ApiUsername,
                Password    = config.ApiPassword,
                RequestedBy = config.RequestedBy,
                User        = userInfo
            };

            UserAPIResponse userAPIResponse;
            if (string.IsNullOrWhiteSpace(ctx.ProductUserId))
            {
                userInfo.Password = PasswordGenerator.GeneratePassword(20, 5);
                userAPIResponse   = _insuranceService.AddUser(addUpdateRequest);
            }
            else
            {
                userInfo.Password = null;
                userAPIResponse   = _insuranceService.UpdateUser(addUpdateRequest);
            }

            _logger.LogDebug("{Action} - End create/update. userPersonaId={UserId}", nameof(ManageRentersInsuranceUserAsync), userPersonaId);

            if (userAPIResponse.IsSuccess && !string.IsNullOrWhiteSpace(userAPIResponse.UserId.ToString()))
            {
                await _samlAttributeService.UpsertAttributesAsync(
                    userPersonaId, ProductId,
                    new Dictionary<SamlAttributeEnum, string>
                    {
                        [SamlAttributeEnum.productUsername] = productUserName,
                        [SamlAttributeEnum.UserId]          = userAPIResponse.UserId.ToString()
                    }, ct);

                await _productRepository.UpdateProductSettingProductStatusAsync(
                    userPersonaId, ProductId, "ProductStatus", (int)ProductBatchStatusType.Success, ct);

                errStatus.Success  = true;
                errStatus.ErrorMsg = string.Empty;
                output.obj         = userAPIResponse;
                output.Status      = errStatus;

                // ── Audit parameters ─────────────────────────────────────────
                try
                {
                    auditParams = BuildAuditParameters(
                        userBeforeUpdate, userInfo, blueBookPropertyList, rentersInsuranceRoleAndPropertyList);
                }
                catch (Exception auditEx)
                {
                    _logger.LogError(auditEx, "{Action} - Error building audit params. userPersonaId={UserId}",
                        nameof(ManageRentersInsuranceUserAsync), userPersonaId);
                }
            }
            else
            {
                _logger.LogWarning("{Action} - API returned failure. userPersonaId={UserId}",
                    nameof(ManageRentersInsuranceUserAsync), userPersonaId);
                errStatus.Success  = false;
                errStatus.ErrorMsg = "Failed to create a renters insurance user.";
                output.Status      = errStatus;
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "{Action} - Error. userPersonaId={UserId}",
                nameof(ManageRentersInsuranceUserAsync), userPersonaId);
            errStatus.Success  = false;
            errStatus.ErrorMsg = $"Error - {ex.Message}";
            output.Status      = errStatus;
        }

        return (output, auditParams);
    }

    /// <inheritdoc/>
    public Task<(ObjectOutput<UserAPIResponse, IErrorData> result, List<AdditionalParameters> auditParams)> ChangeRentersInsuranceUserTypeAsync(
        long createUserPersonaId,
        long assignUserPersonaId,
        RentersInsuranceRoleAndPropertyList rentersInsuranceRoleAndPropertyList,
        BatchProcessType batchProcessType,
        CancellationToken ct = default)
        => ManageRentersInsuranceUserAsync(
            createUserPersonaId, assignUserPersonaId,
            rentersInsuranceRoleAndPropertyList, batchProcessType, ct);

    #endregion

    #region Enable / Disable / Unassign / Unlock

    /// <inheritdoc/>
    public async Task<ObjectOutput<UserAPIResponse, IErrorData>> DisableRentersInsuranceUserAsync(
        long editorPersonaId,
        long userPersonaId,
        CancellationToken ct = default)
        => await CallUserActionAsync(editorPersonaId, userPersonaId,
            (svc, req) => svc.DisableUser(req),
            nameof(DisableRentersInsuranceUserAsync), ct);

    /// <inheritdoc/>
    public async Task<ObjectOutput<UserAPIResponse, IErrorData>> EnableRentersInsuranceUserAsync(
        long editorPersonaId,
        long userPersonaId,
        CancellationToken ct = default)
        => await CallUserActionAsync(editorPersonaId, userPersonaId,
            (svc, req) => svc.EnableUser(req),
            nameof(EnableRentersInsuranceUserAsync), ct);

    /// <inheritdoc/>
    public async Task<ObjectOutput<UserAPIResponse, IErrorData>> UnlockRentersInsuranceUserAsync(
        long editorPersonaId,
        long userPersonaId,
        CancellationToken ct = default)
        => await CallUserActionAsync(editorPersonaId, userPersonaId,
            (svc, req) => svc.UnlockUser(req),
            nameof(UnlockRentersInsuranceUserAsync), ct);

    /// <inheritdoc/>
    public async Task<ObjectOutput<UserAPIResponse, IErrorData>> UnassignRentersInsuranceUserAsync(
        long editorPersonaId,
        long userPersonaId,
        CancellationToken ct = default)
    {
        var output    = new ObjectOutput<UserAPIResponse, IErrorData>();
        var errStatus = new Status<IErrorData>();

        try
        {
            var (ctx, error) = await _contextService.GetUserContextAsync(
                editorPersonaId, userPersonaId, ProductId, ct);
            if (error is not null)
            {
                errStatus.Success  = false;
                errStatus.ErrorMsg = error.ErrorReason;
                output.Status      = errStatus;
                return output;
            }

            // Mark deleted before API call — mirrors sync behaviour
            await _productRepository.UpdateProductSettingProductStatusAsync(
                userPersonaId, ProductId, "ProductStatus", (int)ProductBatchStatusType.Deleted, ct);

            var config    = await GetConfigAsync(ct);
            var apiResult = _insuranceService.DisableUser(
                BuildUserActionRequest(config, Convert.ToInt32(ctx!.ProductUserId)));

            _logger.LogDebug("{Action} - Completed. userPersonaId={UserId}", nameof(UnassignRentersInsuranceUserAsync), userPersonaId);

            errStatus.Success  = true;
            errStatus.ErrorMsg = string.Empty;
            output.obj         = apiResult;
            output.Status      = errStatus;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "{Action} - Error. editorPersonaId={EditorId} userPersonaId={UserId}",
                nameof(UnassignRentersInsuranceUserAsync), editorPersonaId, userPersonaId);
            errStatus.Success  = false;
            errStatus.ErrorMsg = $"ManageProductRentersInsuranceAsync.{nameof(UnassignRentersInsuranceUserAsync)} - Error {ex.Message}";
            output.Status      = errStatus;
        }

        return output;
    }

    #endregion

    #region Migration

    /// <inheritdoc/>
    public async Task<ListResponse> GetMigrationUsersAsync(
        long editorPersonaId,
        RequestParameter datafilter,
        CancellationToken ct = default)
    {
        var response = new ListResponse { IsError = true, ErrorReason = "No Users." };

        try
        {
            var (ctx, error) = await _contextService.GetUserContextAsync(
                editorPersonaId, 0, ProductId, ct);
            if (error is not null) return error;

            var config     = await GetConfigAsync(ct);
            var companyMap = await GetRentersInsuranceCompanyMapAsync(ctx!, ct);
            string companyInstanceSourceId = companyMap?.CompanyInstanceSourceId ?? string.Empty;

            if (string.IsNullOrWhiteSpace(companyInstanceSourceId))
            {
                _logger.LogError("{Action} - Company instance source id not found. editorPersonaId={EditorId}",
                    nameof(GetMigrationUsersAsync), editorPersonaId);
                response.ErrorReason = "Company Setup Error: Please Contact Support.";
                return response;
            }

            string filter       = "NonMigrated";
            int    startRow     = 0;
            int    resultPerRow = 1000;

            if (datafilter is not null)
            {
                if (datafilter.FilterBy?.ContainsKey("filter") == true)
                    filter = datafilter.FilterBy["filter"];
                if (datafilter.Pages is not null)
                {
                    startRow     = datafilter.Pages.StartRow;
                    resultPerRow = datafilter.Pages.ResultsPerPage;
                }
            }

            var request = new UserActionByPMCIDRequest
            {
                CompanyId     = companyInstanceSourceId,
                Login         = config.ApiUsername,
                Password      = config.ApiPassword,
                RequestedBy   = config.RequestedBy,
                FilterType    = filter,
                StartRow      = startRow,
                Resultsperpage = resultPerRow
            };

            var allUsers = _insuranceService.GetUsersByPMC(request);
            if (allUsers is null)
            {
                _logger.LogError("{Action} - No users from product. editorPersonaId={EditorId}",
                    nameof(GetMigrationUsersAsync), editorPersonaId);
                return response;
            }

            List<MigrationUser> migrationUsers = [];
            foreach (var user in allUsers.UserList)
            {
                var migrationUser = new MigrationUser
                {
                    CompanyInstanceSourceId = companyInstanceSourceId,
                    UserId    = user.UserId.ToString(),
                    FirstName = user.FirstName,
                    LastName  = user.LastName,
                    Username  = user.User,
                    Email     = user.Email,
                    LastActivity = user.DateLastLogin,
                    Status    = user.IsActive ? "Active" : "Disabled"
                };
                if (user.PropertyList is { Length: > 0 })
                {
                    foreach (var property in user.PropertyList)
                        migrationUser.Properties.Add(new MigrationProperty
                            { PropertyInstanceSourceId = property.PropertyID.ToString() });
                }
                migrationUsers.Add(migrationUser);
            }

            response.IsError     = false;
            response.ErrorReason = string.Empty;
            response.RowsPerPage = resultPerRow;
            response.TotalPages  = 1;
            response.Records     = migrationUsers.Cast<object>().ToList();
            response.TotalRows   = migrationUsers.Count;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "{Action} - Error. editorPersonaId={EditorId}",
                nameof(GetMigrationUsersAsync), editorPersonaId);
            response.IsError     = true;
            response.ErrorReason = ex.Message;
        }

        return response;
    }

    /// <inheritdoc/>
    public async Task<MigrateResponse> UpdateUsersMigrationStatusAsync(
        long editorPersonaId,
        IList<MigrateUser> migrateUsers,
        CancellationToken ct = default)
    {
        var migrateResponse = new MigrateResponse { Status = false };

        try
        {
            var (ctx, error) = await _contextService.GetUserContextAsync(
                editorPersonaId, 0, ProductId, ct);
            if (error is not null) { migrateResponse.Message = error.ErrorReason; return migrateResponse; }

            var config     = await GetConfigAsync(ct);
            var companyMap = await GetRentersInsuranceCompanyMapAsync(ctx!, ct);
            string companyInstanceSourceId = companyMap?.CompanyInstanceSourceId ?? string.Empty;

            if (string.IsNullOrWhiteSpace(companyInstanceSourceId))
            {
                migrateResponse.Message = "Company Setup Error: Please Contact Support.";
                return migrateResponse;
            }

            var migratedArray = migrateUsers.Select(mu => new MigrateUserrequest
            {
                unifiedLoginUserName = mu.UnifiedLoginUserName,
                userid               = mu.UserId,
                usingUnifiedLogin    = mu.UsingUnifiedLogin.ToString()
            }).ToArray();

            migrateResponse.Message = _insuranceService.MigrateUser(migratedArray);
            migrateResponse.Status  = true;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "{Action} - Error. editorPersonaId={EditorId}",
                nameof(UpdateUsersMigrationStatusAsync), editorPersonaId);
            migrateResponse.Message = ex.Message;
            migrateResponse.Status  = false;
        }

        return migrateResponse;
    }

    #endregion

    #region Status Toggle

    /// <inheritdoc/>
    public async Task<bool> ChangeUserStatusAsync(
        long editorPersonaId,
        int userId,
        bool isActive = false,
        CancellationToken ct = default)
    {
        try
        {
            var (_, error) = await _contextService.GetUserContextAsync(
                editorPersonaId, 0, ProductId, ct);
            if (error is not null) return false;

            var config  = await GetConfigAsync(ct);
            var request = BuildUserActionRequest(config, userId);

            var apiResponse = isActive
                ? _insuranceService.EnableUser(request)
                : _insuranceService.DisableUser(request);

            if (apiResponse is null || !apiResponse.IsSuccess ||
                string.IsNullOrWhiteSpace(apiResponse.UserId.ToString()))
                return false;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "{Action} - Error. userId={UserId} editorPersonaId={EditorId}",
                nameof(ChangeUserStatusAsync), userId, editorPersonaId);
            return false;
        }

        return true;
    }

    #endregion

    #region Private Helpers

    /// <summary>
    /// Loads credential configuration from the internal settings table; cached for 1 hour.
    /// </summary>
    private async ValueTask<RentersInsuranceConfig> GetConfigAsync(CancellationToken ct)
    {
        if (_cache.TryGetValue(ConfigCacheKey, out RentersInsuranceConfig? cached) && cached is not null)
            return cached;

        var settings = await _internalSettingRepository.GetProductInternalSettingsAsync(ProductId, ct);
        var config = new RentersInsuranceConfig(
            ApiUsername:  settings.First(s => s.Name.Equals("APIUSERNAME",  StringComparison.OrdinalIgnoreCase)).Value,
            ApiPassword:  settings.First(s => s.Name.Equals("APIPASSWORD",  StringComparison.OrdinalIgnoreCase)).Value,
            RequestedBy:  Convert.ToInt32(settings.First(s => s.Name.Equals("REQUESTEDBY", StringComparison.OrdinalIgnoreCase)).Value));

        _cache.Set(ConfigCacheKey, config, ConfigCacheTtl);
        return config;
    }

    /// <summary>
    /// Resolves the BlueBook company map entry for the Insurance product using the editor's organisation.
    /// Equivalent of <c>GetRentersInsuranceCompanyInstanceId()</c> in the sync class.
    /// </summary>
    private async Task<CustomerCompanyMap?> GetRentersInsuranceCompanyMapAsync(
        ProductCallContext ctx, CancellationToken ct)
    {
        var companyProductList = await _blueBook.GetCompanyMapAsync(
            ctx.EditorPersona.Organization.RealPageId,
            ctx.EditorPersona.Organization.BooksCustomerMasterId,
            source: BlueBookProductConstants.Insurance,
            domain: ctx.EditorPersona.Organization.OrganizationDomain?.Name ?? string.Empty,
            useTranslate: false,
            cancellationToken: ct);

        return companyProductList?
            .FirstOrDefault(a => a.Source.Equals(BlueBookProductConstants.Insurance,
                StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Merges the insurance API's property list into the BlueBook list, marking each
    /// property <c>IsAssigned = true</c> if the user has access to it.
    /// </summary>
    private async Task<ListResponse> MergeProductPropertiesAsync(
        string productUserId,
        IList<ProductProperty> blueBookPropertyList,
        CancellationToken ct)
    {
        var config    = await GetConfigAsync(ct);
        var userByID  = _insuranceService.GetUserByID(
            BuildUserActionRequest(config, Convert.ToInt32(productUserId)));

        bool allProperties = false;

        if (userByID?.UserInfo?.PropertyList is not null)
        {
            var assignedIds = new HashSet<string>(
                userByID.UserInfo.PropertyList.Select(p => p.PropertyID.ToString()));

            foreach (var property in blueBookPropertyList)
            {
                if (assignedIds.Contains(property.ID))
                    property.IsAssigned = true;
            }

            // If assigned count matches total, treat as "all properties"
            allProperties = userByID.UserInfo.PropertyList.Length >= blueBookPropertyList.Count;
        }

        return new ListResponse
        {
            Records     = blueBookPropertyList.Cast<object>().ToList(),
            TotalRows   = blueBookPropertyList.Count,
            RowsPerPage = blueBookPropertyList.Count,
            TotalPages  = 1,
            ErrorReason = string.Empty,
            Additional  = new Dictionary<string, bool> { ["allProperties"] = allProperties }
        };
    }

    /// <summary>
    /// Shared logic for the simple user-action methods (Disable / Enable / Unlock).
    /// </summary>
    private async Task<ObjectOutput<UserAPIResponse, IErrorData>> CallUserActionAsync(
        long editorPersonaId,
        long userPersonaId,
        Func<IInsuranceService, UserActionRequest, UserAPIResponse> apiCall,
        string actionName,
        CancellationToken ct)
    {
        var output    = new ObjectOutput<UserAPIResponse, IErrorData>();
        var errStatus = new Status<IErrorData>();

        try
        {
            var (ctx, error) = await _contextService.GetUserContextAsync(
                editorPersonaId, userPersonaId, ProductId, ct);
            if (error is not null)
            {
                errStatus.Success  = false;
                errStatus.ErrorMsg = error.ErrorReason;
                output.Status      = errStatus;
                return output;
            }

            var config     = await GetConfigAsync(ct);
            var apiResult  = apiCall(_insuranceService,
                BuildUserActionRequest(config, Convert.ToInt32(ctx!.ProductUserId)));

            _logger.LogDebug("{Action} - Completed. userPersonaId={UserId}", actionName, userPersonaId);

            errStatus.Success  = true;
            errStatus.ErrorMsg = string.Empty;
            output.obj         = apiResult;
            output.Status      = errStatus;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "{Action} - Error. editorPersonaId={EditorId} userPersonaId={UserId}",
                actionName, editorPersonaId, userPersonaId);
            errStatus.Success  = false;
            errStatus.ErrorMsg = $"ManageProductRentersInsuranceAsync.{actionName} - Error {ex.Message}";
            output.Status      = errStatus;
        }

        return output;
    }

    /// <summary>
    /// Builds a <see cref="UserActionRequest"/> with the shared API credentials.
    /// </summary>
    private static UserActionRequest BuildUserActionRequest(RentersInsuranceConfig config, int userId)
        => new()
        {
            Login       = config.ApiUsername,
            Password    = config.ApiPassword,
            RequestedBy = config.RequestedBy,
            UserId      = userId
        };

    /// <summary>
    /// Constructs the property list for an add/update user request based on
    /// <paramref name="batchProcessType"/> — mirrors the logic in the sync class.
    /// </summary>
    private static IList<UserProperty> BuildUserPropertyList(
        RentersInsuranceRoleAndPropertyList roleAndPropertyList,
        BatchProcessType batchProcessType,
        GetUserByIDResponse getUserByIDResponse,
        IList<ProductProperty> blueBookPropertyList)
    {
        IList<UserProperty> userPropertyList = [];

        bool isTypeChange =
            batchProcessType is BatchProcessType.UserTypeAdminToRegular
                             or BatchProcessType.UserTypeRegularToAdmin
                             or BatchProcessType.UserTypeAdminToExternal
                             or BatchProcessType.UserTypeExternalToAdmin;

        if (isTypeChange)
        {
            bool toAdmin = batchProcessType is BatchProcessType.UserTypeRegularToAdmin
                                            or BatchProcessType.UserTypeExternalToAdmin;

            if (toAdmin)
            {
                bool isAllOrEmpty = roleAndPropertyList.PropertyList is null or { Count: 0 }
                    || (roleAndPropertyList.PropertyList.Count == 1 &&
                        roleAndPropertyList.PropertyList[0].Equals("ALL", StringComparison.OrdinalIgnoreCase));

                if (isAllOrEmpty)
                {
                    foreach (var property in blueBookPropertyList)
                        userPropertyList.Add(new UserProperty
                        {
                            PropertyID   = Convert.ToInt32(property.ID),
                            PropertyName = property.Name
                        });
                }
            }
            else  // AdminToRegular / AdminToExternal
            {
                foreach (string property in roleAndPropertyList.PropertyList ?? [])
                {
                    var propertyData = blueBookPropertyList.FirstOrDefault(item => item.ID == property);
                    if (propertyData is not null)
                        userPropertyList.Add(new UserProperty
                        {
                            PropertyID   = Convert.ToInt32(propertyData.ID),
                            PropertyName = propertyData.Name
                        });
                }
            }
        }
        else if (batchProcessType == BatchProcessType.ProfileUpdate &&
                 getUserByIDResponse?.UserInfo?.PropertyList is not null)
        {
            userPropertyList = getUserByIDResponse.UserInfo.PropertyList;
        }
        else
        {
            bool isAllOrEmpty = roleAndPropertyList.PropertyList is null or { Count: 0 }
                || (roleAndPropertyList.PropertyList.Count == 1 &&
                    roleAndPropertyList.PropertyList[0].Equals("ALL", StringComparison.OrdinalIgnoreCase));

            if (isAllOrEmpty)
            {
                foreach (var property in blueBookPropertyList)
                    userPropertyList.Add(new UserProperty
                    {
                        PropertyID   = Convert.ToInt32(property.ID),
                        PropertyName = property.Name
                    });
            }
            else
            {
                foreach (string property in roleAndPropertyList.PropertyList ?? [])
                {
                    var propertyData = blueBookPropertyList.FirstOrDefault(item => item.ID == property);
                    if (propertyData is not null)
                        userPropertyList.Add(new UserProperty
                        {
                            PropertyID   = Convert.ToInt32(propertyData.ID),
                            PropertyName = propertyData.Name
                        });
                }
            }
        }

        return userPropertyList;
    }

    /// <summary>
    /// Builds audit parameters by diffing the user state before and after the update.
    /// </summary>
    private static List<AdditionalParameters> BuildAuditParameters(
        GetUserByIDResponse userBefore,
        UserInfo userAfter,
        IList<ProductProperty> blueBookPropertyList,
        RentersInsuranceRoleAndPropertyList roleAndPropertyList)
    {
        List<AdditionalParameters> result = [];
        _ = roleAndPropertyList; // Kept for API compatibility; role diff uses userBefore/After

        // Role change
        if (userBefore?.UserInfo?.RoleID != userAfter.RoleID)
        {
            var afterRoleName = blueBookPropertyList
                .FirstOrDefault(p => p.ID == userAfter.RoleID.ToString())?.Name
                ?? userAfter.RoleID.ToString();

            result.Add(new AdditionalParameters
            {
                Key   = "Renters Insurance Roles",
                Value = RolesAssignMsg.Replace("RoleName", afterRoleName)
            });

            if (userBefore?.UserInfo?.RoleID is not null)
            {
                var beforeRoleName = blueBookPropertyList
                    .FirstOrDefault(p => p.ID == userBefore.UserInfo.RoleID.ToString())?.Name
                    ?? userBefore.UserInfo.RoleID.ToString();

                result.Add(new AdditionalParameters
                {
                    Key   = "Renters Insurance Roles",
                    Value = RolesRemovedMsg.Replace("RoleName", beforeRoleName)
                });
            }
        }

        // Property changes
        var oldProperties = userBefore?.UserInfo?.PropertyList?.Select(p => p.PropertyID)
                            ?? [];
        var newProperties = userAfter.PropertyList?.Select(p => p.PropertyID) ?? [];

        var removedProperties = oldProperties.Except(newProperties).ToList();
        var addedProperties   = newProperties.Except(oldProperties).ToList();

        var propById = blueBookPropertyList.ToDictionary(p => p.ID, StringComparer.OrdinalIgnoreCase);

        foreach (int p in removedProperties)
            result.Add(new AdditionalParameters
            {
                Key   = "Renters Insurance Properties",
                Value = PropsRemovedMsg.Replace("PropertyName",
                    propById.TryGetValue(p.ToString(), out var pp) ? pp.Name : p.ToString())
            });

        foreach (int p in addedProperties)
            result.Add(new AdditionalParameters
            {
                Key   = "Renters Insurance Properties",
                Value = PropsAssignMsg.Replace("PropertyName",
                    propById.TryGetValue(p.ToString(), out var pp) ? pp.Name : p.ToString())
            });

        return result;
    }

    private static string TruncateTo50(string? value)
        => string.IsNullOrWhiteSpace(value) ? value ?? string.Empty : value[..Math.Min(value.Length, 50)];

    private static bool IsValidEmail(string input)
        => !string.IsNullOrWhiteSpace(input) && input.Contains('@') && input.Contains('.');

    #endregion
}
