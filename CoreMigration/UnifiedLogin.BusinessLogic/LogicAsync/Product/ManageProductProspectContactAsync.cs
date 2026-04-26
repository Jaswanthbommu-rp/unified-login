using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using NJ = Newtonsoft.Json;
using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects.Audit.Common;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.BlackBook;
using UnifiedLogin.SharedObjects.Constants;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Exceptions;
using UnifiedLogin.SharedObjects.Extensions;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product;
using UnifiedLogin.SharedObjects.Product.Migration;
using UnifiedLogin.SharedObjects.Product.ProspectContactCenter;
using UnifiedLogin.SharedObjects.Saml;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace UnifiedLogin.BusinessLogic.LogicAsync.Product;

/// <summary>
/// Native-async implementation of Prospect Contact Center (PCC) product user management.
/// <para>
/// Replaces <c>ManageProductProspectContact</c>: two <c>DefaultUserClaim</c>-bound constructors
/// → single 11-dependency DI constructor; all <c>.Result</c>/<c>.GetAwaiter().GetResult()</c>
/// blocking calls → <c>await</c>; <c>new HttpClient()</c> per-call → <c>IHttpClientFactory</c>;
/// <c>ExpandoObject</c> + <c>ObjectContent&lt;dynamic&gt;</c> → anonymous type + <c>JsonContent.Create</c>;
/// <c>dynamic</c> JSON result → <c>System.Text.Json.JsonDocument</c>.
/// </para>
/// <para>
/// The <c>out List&lt;AdditionalParameters&gt;</c> parameters on the two manage-user methods
/// are replaced with tuple returns — <c>out</c> is incompatible with <c>async</c>.
/// </para>
/// <para>
/// Local HTTP payload types (<see cref="PccUser"/>, <see cref="PccUserProfile"/>) use
/// <see cref="System.Text.Json.Serialization"/> attributes so <c>PutAsJsonAsync</c> /
/// <c>PostAsJsonAsync</c> serialize correctly without Newtonsoft. Newtonsoft is retained
/// only for SharedObjects typed deserialization (<see cref="MigrateResponse"/>,
/// <see cref="ProductPropertyMap"/>).
/// </para>
/// </summary>
public sealed class ManageProductProspectContactAsync : IManageProductProspectContactAsync
{
    // ── Constants ─────────────────────────────────────────────────────────────

    private const int    ProductId                = (int)ProductEnum.ProspectContactCenter;
    private const string ProductStatusSettingType = "ProductStatus";
    private const string ApiEndPointKey           = "APIENDPOINT";
    private const string PropsAssignTemplate      = "{\"action\":\"Added Properties\",\"value\":\"PropertyName\"}";
    private const string PropsRemoveTemplate      = "{\"action\":\"Removed Properties\",\"value\":\"PropertyName\"}";

    // ── Fields ────────────────────────────────────────────────────────────────

    private readonly IProductContextServiceAsync                _contextService;
    private readonly IProductSettingServiceAsync                _settingService;
    private readonly IManagePersonaAsync                        _managePersona;
    private readonly IManagePersonAsync                         _managePerson;
    private readonly IManageUserLoginAsync                      _manageUserLogin;
    private readonly IManageElectronicAddressAsync              _manageElectronicAddress;
    private readonly ISamlRepositoryAsync                       _samlRepository;
    private readonly IProductRepositoryAsync                    _productRepository;
    private readonly IManageBlueBookAsync                       _manageBlueBook;
    private readonly IHttpClientFactory                         _httpClientFactory;
    private readonly ILogger<ManageProductProspectContactAsync> _logger;

    // ── Constructor ───────────────────────────────────────────────────────────

    public ManageProductProspectContactAsync(
        IProductContextServiceAsync                contextService,
        IProductSettingServiceAsync                settingService,
        IManagePersonaAsync                        managePersona,
        IManagePersonAsync                         managePerson,
        IManageUserLoginAsync                      manageUserLogin,
        IManageElectronicAddressAsync              manageElectronicAddress,
        ISamlRepositoryAsync                       samlRepository,
        IProductRepositoryAsync                    productRepository,
        IManageBlueBookAsync                       manageBlueBook,
        IHttpClientFactory                         httpClientFactory,
        ILogger<ManageProductProspectContactAsync> logger)
    {
        ArgumentNullException.ThrowIfNull(contextService);
        ArgumentNullException.ThrowIfNull(settingService);
        ArgumentNullException.ThrowIfNull(managePersona);
        ArgumentNullException.ThrowIfNull(managePerson);
        ArgumentNullException.ThrowIfNull(manageUserLogin);
        ArgumentNullException.ThrowIfNull(manageElectronicAddress);
        ArgumentNullException.ThrowIfNull(samlRepository);
        ArgumentNullException.ThrowIfNull(productRepository);
        ArgumentNullException.ThrowIfNull(manageBlueBook);
        ArgumentNullException.ThrowIfNull(httpClientFactory);
        ArgumentNullException.ThrowIfNull(logger);

        _contextService          = contextService;
        _settingService          = settingService;
        _managePersona           = managePersona;
        _managePerson            = managePerson;
        _manageUserLogin         = manageUserLogin;
        _manageElectronicAddress = manageElectronicAddress;
        _samlRepository          = samlRepository;
        _productRepository       = productRepository;
        _manageBlueBook          = manageBlueBook;
        _httpClientFactory       = httpClientFactory;
        _logger                  = logger;
    }

    // ════════════════════════════════════════════════════════════════════════
    // PUBLIC METHODS
    // ════════════════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    public async Task<ListResponse> GetPropertiesAsync(
        long editorPersonaId, long userPersonaId,
        RequestParameter datafilter,
        CancellationToken ct = default)
    {
        try
        {
            var (ctx, ctxError) = await _contextService.GetUserContextAsync(
                editorPersonaId, userPersonaId, ProductId, ct);
            if (ctxError is not null) return ctxError;

            var apiEndPoint = await GetApiEndPointAsync(ct);
            var company     = await GetCompanyInstanceAsync(ctx!.EditorPersona, ct);

            if (string.IsNullOrEmpty(company?.CompanyInstanceSourceId))
            {
                _logger.LogError("GetPropertiesAsync - Company not found. editorPersonaId={Id}", editorPersonaId);
                return new ListResponse { IsError = true, ErrorReason = "Company Setup Error: Please Contact Support." };
            }

            int companySourceId   = Convert.ToInt32(company.CompanyInstanceSourceId);
            var productProperties = await GetProductPropertiesFromApiAsync(apiEndPoint, companySourceId, ct);

            _logger.LogDebug("GetPropertiesAsync - Found {Count} properties. companySourceId={Id}", productProperties.Count, companySourceId);

            if (userPersonaId != 0 && ctx.ProductUserId is { Length: > 0 })
                return await MergeProductPropertiesWithGreenbookAsync(
                    ctx.ProductUserId, apiEndPoint, productProperties, ct);

            return new ListResponse
            {
                Records     = productProperties.Cast<object>().ToList(),
                TotalRows   = productProperties.Count,
                RowsPerPage = productProperties.Count,
                TotalPages  = 1,
                ErrorReason = string.Empty
            };
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "GetPropertiesAsync - Error. editorPersonaId={Id}", editorPersonaId);
            string reason = ex is BlueBookException ? ex.Message : CommonMessageConstants.PropertyErrorMessage;
            return new ListResponse { IsError = true, ErrorReason = reason };
        }
    }

    /// <inheritdoc/>
    public async Task<string> UnassignUserAsync(
        long editorPersonaId, long userPersonaId,
        CancellationToken ct = default)
    {
        string result = string.Empty;
        try
        {
            var (ctx, ctxError) = await _contextService.GetUserContextAsync(
                editorPersonaId, userPersonaId, ProductId, ct);
            if (ctxError is not null)
            {
                _logger.LogError("UnassignUserAsync - Context error. userPersonaId={Id} Reason={Reason}", userPersonaId, ctxError.ErrorReason);
                return ctxError.ErrorReason;
            }

            var apiEndPoint = await GetApiEndPointAsync(ct);
            var pccUser     = await GetProspectContactCenterUserAsync(ctx!.ProductUserId, apiEndPoint, ct);

            result = await DeactivateCurrentUserAsync(ctx.ProductUserId, ctx.EditorProductUserId, apiEndPoint, ct);
            if (string.IsNullOrEmpty(result))
            {
                await _settingService.UpdateProductStatusAsync(
                    userPersonaId, ProductStatusSettingType, ProductId,
                    (int)ProductBatchStatusType.Deleted, ct);
            }

            var persona   = await _managePersona.GetPersonaAsync(userPersonaId, withRights: false, ct);
            var userLogin = await _manageUserLogin.GetUserLoginOnlyAsync(persona.RealPageId, ct);
            var person    = await _managePerson.GetPersonAsync(persona.RealPageId, ct);
            var userEmail = await ResolveUserEmailAsync(userLogin.RealPageId, userLogin.LoginName, ct);
            var company   = await GetCompanyInstanceAsync(ctx.EditorPersona, ct);

            if (string.IsNullOrEmpty(company?.CompanyInstanceSourceId))
            {
                _logger.LogError("UnassignUserAsync - Company not found. editorPersonaId={Id}", editorPersonaId);
                return "Company Setup Error: Please Contact Support.";
            }

            var prospectUser = new PccUser
            {
                ModifyingUser = ctx.EditorProductUserId,
                User = new PccUserProfile
                {
                    LoginName           = userLogin.LoginName,
                    FirstName           = person.FirstName,
                    LastName            = person.LastName,
                    Email               = userEmail,
                    UserActive          = true,
                    ManagementCompanyID = company.CompanyInstanceSourceId,
                    UserType            = "C",
                    PropertyID          = "0"
                }
            };

            string existingUserType = pccUser?.UserType?.TrimEnd() ?? string.Empty;
            if (existingUserType != prospectUser.User.UserType)
            {
                await ReCreateNewUserAsync(
                    userPersonaId, apiEndPoint, prospectUser, ctx.ProductUserId,
                    isSamlUpdateRequired: true, ct);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "UnassignUserAsync - Error. userPersonaId={Id}", userPersonaId);
            result = ex.Message;
        }

        return result;
    }

    /// <inheritdoc/>
    public Task<(string error, List<AdditionalParameters> additionalParameters)> ChangeProspectContactUserTypeAsync(
        long createUserPersonaId, long assignUserPersonaId,
        ProspectContactPropertyRole roleProp,
        BatchProcessType batchProcessType,
        CancellationToken ct = default)
        => ManageProductProspectContactUserAsync(
            createUserPersonaId, assignUserPersonaId, roleProp, batchProcessType, ct);

    /// <inheritdoc/>
    public async Task<(string error, List<AdditionalParameters> additionalParameters)> ManageProductProspectContactUserAsync(
        long editorPersonaId, long userPersonaId,
        ProspectContactPropertyRole userProspectContactPropertyRole,
        BatchProcessType batchProcessType = BatchProcessType.CreateUpdateProductUser,
        CancellationToken ct = default)
    {
        List<AdditionalParameters> additionalParameters = [];
        try
        {
            var (ctx, ctxError) = await _contextService.GetUserContextAsync(
                editorPersonaId, userPersonaId, ProductId, ct);
            if (ctxError is not null) return (ctxError.ErrorReason, additionalParameters);

            var persona   = await _managePersona.GetPersonaAsync(userPersonaId, withRights: false, ct);
            var userLogin = await _manageUserLogin.GetUserLoginOnlyAsync(persona.RealPageId, ct);
            var person    = await _managePerson.GetPersonAsync(persona.RealPageId, ct);
            var userEmail = await ResolveUserEmailAsync(userLogin.RealPageId, userLogin.LoginName, ct);
            var apiEndPoint = await GetApiEndPointAsync(ct);

            // Super user → assign all properties (PMC level)
            if (await _contextService.IsSuperUserAsync(persona, ct))
                userProspectContactPropertyRole.PropertyList = ["ALL"];

            var productLoginName = ctx!.ProductUsername is { Length: > 0 }
                ? ctx.ProductUsername
                : userLogin.LoginName;

            var company = await GetCompanyInstanceAsync(ctx.EditorPersona, ct);
            if (string.IsNullOrEmpty(company?.CompanyInstanceSourceId))
            {
                _logger.LogError("ManageProductProspectContactUserAsync - Company not found. editorPersonaId={Id}", editorPersonaId);
                return ("Company Setup Error: Please Contact Support.", additionalParameters);
            }

            var prospectUser = new PccUser
            {
                ModifyingUser = ctx.EditorProductUserId,
                User = new PccUserProfile
                {
                    LoginName  = userLogin.LoginName,
                    FirstName  = person.FirstName,
                    LastName   = person.LastName,
                    Email      = userEmail,
                    UserActive = true
                }
            };

            bool isPmc = userProspectContactPropertyRole.PropertyList[0]
                .Trim().Equals("ALL", StringComparison.OrdinalIgnoreCase);
            if (isPmc)
            {
                prospectUser.User.ManagementCompanyID = company.CompanyInstanceSourceId;
                prospectUser.User.Properties          = ["0"];
                prospectUser.User.PropertyID          = "0";
                prospectUser.User.UserType            = "M";
            }
            else
            {
                prospectUser.User.ManagementCompanyID = company.CompanyInstanceSourceId;
                prospectUser.User.UserType            = "C";
                prospectUser.User.PropertyID          = "0";
                prospectUser.User.Properties          = userProspectContactPropertyRole.PropertyList;
            }

            var userBeforeUpdate = ctx.ProductUserId is { Length: > 0 }
                ? await GetProspectContactCenterUserAsync(ctx.ProductUserId, apiEndPoint, ct)
                : new PccUserProfile { Properties = [] };

            string userResult;
            if (string.IsNullOrEmpty(ctx.ProductUsername))
            {
                // ── NEW USER ──────────────────────────────────────────────
                string newLoginBase = $"{person.FirstName.TrimWhiteSpace()[..1]}{person.LastName.TrimWhiteSpace()}".ToLower();
                productLoginName = await GetUniqueProductLoginNameAsync(productLoginName, newLoginBase, apiEndPoint, ct);
                prospectUser.User.LoginName = productLoginName;

                _logger.LogDebug("ManageProductProspectContactUserAsync - Creating new user. editorPersonaId={Id}", editorPersonaId);
                string newProductUserId = await InsertProspectContactCenterUserAsync(
                    $"{apiEndPoint}/User", userPersonaId, editorPersonaId, productLoginName, prospectUser, ct);
                await CreateProductUserInGreenBookAsync(userPersonaId, newProductUserId, productLoginName, ct);

                // TODO: WriteUpdateUserTypeActivityLog — pending IProductAuditServiceAsync.WriteUpdateUserTypeActivityLogAsync

                userResult = string.Empty;
            }
            else
            {
                // ── UPDATE USER ───────────────────────────────────────────
                _logger.LogDebug("ManageProductProspectContactUserAsync - Updating user. editorPersonaId={Id}", editorPersonaId);
                prospectUser.User.SystemIdentifier = ctx.ProductUserId;
                prospectUser.User.LoginName        = ctx.ProductUsername;
                userResult = await UpdateProspectContactCenterPropertyUserAsync(
                    userPersonaId, editorPersonaId, ctx.ProductUserId, apiEndPoint, prospectUser, ct);
            }

            // ── Activity log: property assignment diff ────────────────────
            try
            {
                IList<string> beforeProps = userBeforeUpdate?.Properties ?? [];
                IList<string> afterProps  = prospectUser.User.Properties  ?? [];
                var removedProp = beforeProps.Except(afterProps).ToList();
                var addedProp   = afterProps.Except(beforeProps).ToList();

                if (removedProp.Count > 0 || addedProp.Count > 0)
                {
                    var propsLR  = await GetPropertiesAsync(editorPersonaId, userPersonaId, new RequestParameter(), ct);
                    List<ProductProperty> properties = propsLR.Records is not null
                        ? propsLR.Records.Cast<ProductProperty>().ToList()
                        : [];

                    foreach (string r in removedProp.Where(r => properties.Any(f => f.ID == r)))
                        additionalParameters.Add(new AdditionalParameters
                        {
                            Key   = "Prospect Contact Center Properties",
                            Value = PropsRemoveTemplate.Replace("PropertyName", properties.Find(f => f.ID == r)!.Name)
                        });

                    foreach (string r in addedProp.Where(r => properties.Any(f => f.ID == r)))
                        additionalParameters.Add(new AdditionalParameters
                        {
                            Key   = "Prospect Contact Center Properties",
                            Value = PropsAssignTemplate.Replace("PropertyName", properties.Find(f => f.ID == r)!.Name)
                        });
                }
            }
            catch (Exception e) when (e is not OperationCanceledException)
            {
                _logger.LogError(e, "ManageProductProspectContactUserAsync - Error building activity details");
            }

            return (userResult, additionalParameters);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "ManageProductProspectContactUserAsync - Error. editorPersonaId={Id}", editorPersonaId);
            return ($"Error - {ex.Message}", additionalParameters);
        }
    }

    /// <inheritdoc/>
    public async Task<string> UpdateProspectContactCenterUserProfileAsync(
        long editorPersonaId, long userPersonaId,
        CancellationToken ct = default)
    {
        try
        {
            var (ctx, ctxError) = await _contextService.GetUserContextAsync(
                editorPersonaId, userPersonaId, ProductId, ct);
            if (ctxError is not null)
            {
                _logger.LogError("UpdateProspectContactCenterUserProfileAsync - Context error. editorPersonaId={Id}", editorPersonaId);
                return ctxError.ErrorReason;
            }

            var persona   = await _managePersona.GetPersonaAsync(userPersonaId, withRights: false, ct);
            var userLogin = await _manageUserLogin.GetUserLoginOnlyAsync(persona.RealPageId, ct);
            var person    = await _managePerson.GetPersonAsync(persona.RealPageId, ct);
            var userEmail = await ResolveUserEmailAsync(userLogin.RealPageId, userLogin.LoginName, ct);
            var apiEndPoint = await GetApiEndPointAsync(ct);

            string productLoginName = string.IsNullOrEmpty(ctx!.ProductUsername)
                ? userEmail
                : ctx.ProductUsername;

            // If login name changed in primary org → use the new UL login name
            var orgList = await _manageUserLogin.GetUserPersonaOrganizationAsync(userLogin.LoginName, cancellationToken: ct);
            bool inPrimaryOrg = orgList.Any(o =>
                o.PrimaryOrganization == true &&
                o.OrganizationPartyId == persona.OrganizationPartyId);
            bool loginChanged = !(ctx.ProductUsername ?? string.Empty)
                .Equals(userLogin.LoginName, StringComparison.OrdinalIgnoreCase);

            if (inPrimaryOrg && loginChanged)
                productLoginName = userLogin.LoginName;

            _logger.LogDebug("UpdateProspectContactCenterUserProfileAsync - productLoginName={Login}", productLoginName);

            var prospectUser = new PccUser
            {
                ModifyingUser = ctx.EditorProductUserId,
                User = new PccUserProfile
                {
                    SystemIdentifier = ctx.ProductUserId,
                    LoginName        = productLoginName,
                    FirstName        = person.FirstName,
                    LastName         = person.LastName,
                    Email            = userEmail,
                    UserActive       = true
                }
            };

            var result = await UpdateProspectContactCenterUserAsync(
                $"{apiEndPoint}/User", userPersonaId, editorPersonaId, prospectUser, ct);
            if (!string.IsNullOrEmpty(result)) return result;

            // Refresh SAML ProductUsername attribute in GreenBook
            var productAttributes = await _samlRepository.GetProductSamlDetailsAsync(userPersonaId, ProductId, ct);
            foreach (var attribute in productAttributes)
            {
                if (attribute.Name.Equals("PRODUCTUSERNAME", StringComparison.OrdinalIgnoreCase))
                {
                    attribute.Value = productLoginName;
                    await _samlRepository.UpdateSamlUserAttributeAsync(attribute, ct);
                }
            }

            // TODO: WriteUpdateUserTypeActivityLog(BatchProcessType.ProfileUpdate) — pending IProductAuditServiceAsync

            return result;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "UpdateProspectContactCenterUserProfileAsync - Error. editorPersonaId={Id}", editorPersonaId);
            return ex.Message;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> ChangeUserStatusAsync(
        long editorPersonaId, int userId,
        CancellationToken ct = default)
    {
        try
        {
            var (ctx, ctxError) = await _contextService.GetUserContextAsync(editorPersonaId, 0, ProductId, ct);
            if (ctxError is not null) return false;

            var apiEndPoint = await GetApiEndPointAsync(ct);
            await DeactivateCurrentUserAsync(userId.ToString(), ctx!.EditorProductUserId, apiEndPoint, ct);
            return true;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "ChangeUserStatusAsync - Failed. userId={UserId} editorPersonaId={EditorId}", userId, editorPersonaId);
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task<ListResponse> GetMigrationUsersAsync(
        long editorPersonaId, RequestParameter datafilter,
        CancellationToken ct = default)
    {
        var response = new ListResponse { IsError = true, ErrorReason = "No Users." };
        try
        {
            var (ctx, ctxError) = await _contextService.GetUserContextAsync(editorPersonaId, 0, ProductId, ct);
            if (ctxError is not null)
                return new ListResponse { IsError = true, ErrorReason = ctxError.ErrorReason };

            var apiEndPoint = await GetApiEndPointAsync(ct);
            var company     = await GetCompanyInstanceAsync(ctx!.EditorPersona, ct);
            int companySourceId = int.TryParse(company?.CompanyInstanceSourceId, out int parsed) ? parsed : 0;

            if (companySourceId == 0)
            {
                _logger.LogError("GetMigrationUsersAsync - Company not found. editorPersonaId={Id}", editorPersonaId);
                return new ListResponse { IsError = true, ErrorReason = "Company Setup Error: Please Contact Support." };
            }

            string filter       = "GreenbookUser";
            int    startRow     = 0;
            int    resultPerRow = 1000;
            if (datafilter?.FilterBy?.TryGetValue("filter", out string? filterVal) == true && filterVal is not null)
                filter = filterVal;
            if (datafilter?.Pages is not null)
            {
                startRow     = datafilter.Pages.StartRow;
                resultPerRow = datafilter.Pages.ResultsPerPage;
            }

            var url      = $"{apiEndPoint}/users/{companySourceId}?filter={filter}&startRow={startRow}&resultsPerPage={resultPerRow}";
            var allUsers = await GetResultFromApiAsync<List<PccUserProfile>>(url, ct);

            if (allUsers is null)
            {
                _logger.LogError("GetMigrationUsersAsync - No users from product. editorPersonaId={Id}", editorPersonaId);
                return response;
            }

            // Remove already-migrated users
            string product   = Convert.ToString(ProductId);
            var productUsers = await _productRepository.GetProductUsersByCompanyAsync(
                ctx.EditorPersona.OrganizationPartyId, product, ct);
            if (productUsers?.Count > 0)
                allUsers.RemoveAll(u => productUsers.Any(p => p.ProductUserName == u.LoginName));

            var migrationUsers = allUsers.Select(user => new MigrationUser
            {
                FirstName               = user.FirstName,
                LastName                = user.LastName,
                Username                = user.LoginName,
                UserId                  = user.SystemIdentifier,
                Status                  = user.UserActive ? "Active" : "Disabled",
                LastActivity            = user.LastLogin.ToString(),
                Email                   = user.Email,
                CompanyInstanceSourceId = companySourceId.ToString(),
                Properties              = user.Properties?
                    .Select(p => new MigrationProperty { PropertyInstanceSourceId = p })
                    .ToList() ?? []
            }).ToList();

            _logger.LogDebug("GetMigrationUsersAsync - Received {Count} users. editorPersonaId={Id}", migrationUsers.Count, editorPersonaId);
            response.RowsPerPage = 9999;
            response.ErrorReason = string.Empty;
            response.IsError     = false;
            response.TotalPages  = 1;
            response.Records     = migrationUsers.Cast<object>().ToList();
            response.TotalRows   = migrationUsers.Count;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "GetMigrationUsersAsync - Error. editorPersonaId={Id}", editorPersonaId);
            response = new ListResponse { IsError = true, ErrorReason = ex.Message };
        }

        return response;
    }

    /// <inheritdoc/>
    public async Task<MigrateResponse> UpdateUsersMigrationStatusAsync(
        long editorPersonaId, IList<MigrateUser> migrateUsers,
        CancellationToken ct = default)
    {
        var migrateResponse = new MigrateResponse { Status = false };
        try
        {
            var (ctx, ctxError) = await _contextService.GetUserContextAsync(editorPersonaId, 0, ProductId, ct);
            if (ctxError is not null) { migrateResponse.Message = ctxError.ErrorReason; return migrateResponse; }

            var apiEndPoint = await GetApiEndPointAsync(ct);
            var company     = await GetCompanyInstanceAsync(ctx!.EditorPersona, ct);
            int companyId   = int.TryParse(company?.CompanyInstanceSourceId, out int parsed) ? parsed : 0;

            if (companyId == 0)
            {
                _logger.LogError("UpdateUsersMigrationStatusAsync - Company not found. editorPersonaId={Id}", editorPersonaId);
                migrateResponse.Message = "Company Setup Error: Please Contact Support.";
                return migrateResponse;
            }

            var url    = $"{apiEndPoint}/migrate-users/{companyId}";
            var client = _httpClientFactory.CreateClient();
            using var response = await client.PutAsync(url, JsonContent.Create(migrateUsers), ct);
            var responseContent = await response.Content.ReadAsStringAsync(ct);

            if (response.IsSuccessStatusCode)
            {
                var migrationResponse = NJ.JsonConvert.DeserializeObject<MigrateResponse>(responseContent);
                _logger.LogDebug("UpdateUsersMigrationStatusAsync - Success. editorPersonaId={Id}", editorPersonaId);
                migrateResponse.Message = migrationResponse?.Message;
                migrateResponse.Status  = migrationResponse?.Status ?? false;
                return migrateResponse;
            }

            _logger.LogError("UpdateUsersMigrationStatusAsync - API error. editorPersonaId={Id}", editorPersonaId);
            migrateResponse.Message = "Cannot update user status to migrated.";
            return migrateResponse;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "UpdateUsersMigrationStatusAsync - Error. editorPersonaId={Id}", editorPersonaId);
            return new MigrateResponse { Status = false, Message = ex.Message };
        }
    }

    // ════════════════════════════════════════════════════════════════════════
    // PRIVATE HELPERS
    // ════════════════════════════════════════════════════════════════════════

    private async Task<string> GetApiEndPointAsync(CancellationToken ct)
    {
        var settings = await _settingService.GetProductSettingAsync(ProductId, ct);
        return settings.FirstOrDefault(s =>
            s.Name.Equals(ApiEndPointKey, StringComparison.OrdinalIgnoreCase))?.Value
            ?? throw new InvalidOperationException($"PCC setting '{ApiEndPointKey}' not found in product settings.");
    }

    private async Task<CustomerCompanyMap?> GetCompanyInstanceAsync(Persona editorPersona, CancellationToken ct)
    {
        var mapping = await _manageBlueBook.GetProductCompanyMappingAsync(
            editorPersona.Organization.RealPageId,
            BlueBookProductConstants.ProspectContactCenter, ct);
        return mapping?.FirstOrDefault();
    }

    /// <summary>
    /// Looks up email addresses for the person; falls back to
    /// <see cref="ValidateAndReturnEmailAddress"/> when none are found.
    /// Extracted from three identical blocks in the legacy class.
    /// </summary>
    private async Task<string> ResolveUserEmailAsync(
        Guid realPageId, string loginName, CancellationToken ct)
    {
        var addresses = await _manageElectronicAddress.ListElectronicAddressForPersonAsync(realPageId, cancellationToken: ct);
        string email = addresses?
            .FirstOrDefault(a => a.AddressType.Equals("EMAIL", StringComparison.OrdinalIgnoreCase))
            ?.AddressString ?? string.Empty;
        return email is { Length: > 0 } ? email : ValidateAndReturnEmailAddress(loginName);
    }

    private async Task<IList<ProductProperty>> GetProductPropertiesFromApiAsync(
        string apiEndPoint, int companySourceId, CancellationToken ct)
    {
        var uri     = new Uri(apiEndPoint);
        var baseUri = uri.GetLeftPart(UriPartial.Authority);
        var url     = $"{baseUri}/reportrestservice/ReportParameter/Property?companyId={companySourceId}&mode=All";

        var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Accept.ParseAdd("application/json");
        using var response = await client.GetAsync(url, ct);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("GetProductPropertiesFromApiAsync - API error. companySourceId={Id} url={Url}", companySourceId, url);
            return [];
        }

        var json = await response.Content.ReadAsStringAsync(ct);
        var map  = NJ.JsonConvert.DeserializeObject(json, typeof(IList<ProductPropertyMap>)) as IList<ProductPropertyMap>;
        return ToGBProperties(map) ?? [];
    }

    private async Task<ListResponse> MergeProductPropertiesWithGreenbookAsync(
        string productUserId, string apiEndPoint,
        IList<ProductProperty> propertyList, CancellationToken ct)
    {
        var pccUser = await GetProspectContactCenterUserAsync(productUserId, apiEndPoint, ct);
        if (pccUser is null)
        {
            _logger.LogError("MergeProductPropertiesWithGreenbookAsync - User not found. productUserId={Id}", productUserId);
            return new ListResponse
            {
                IsError     = true,
                ErrorReason = $"User not found in Prospect Center Contact with ProductUserId - {productUserId}"
            };
        }

        Dictionary<string, bool> additionalData = [];
        if (pccUser.UserType?.Trim().Equals("M", StringComparison.OrdinalIgnoreCase) == true)
        {
            additionalData["allProperties"] = true;
        }
        else if (pccUser.Properties?.Count > 0)
        {
            foreach (var item in pccUser.Properties)
            {
                var prop = propertyList.FirstOrDefault(x => x.ID == item);
                if (prop is not null) prop.IsAssigned = true;
            }
            additionalData["allProperties"] = false;
        }

        return new ListResponse
        {
            Records     = propertyList.Cast<object>().ToList(),
            TotalRows   = propertyList.Count,
            RowsPerPage = 9999,
            ErrorReason = string.Empty,
            TotalPages  = 1,
            Additional  = additionalData
        };
    }

    private Task<PccUserProfile?> GetProspectContactCenterUserAsync(
        string productUserId, string apiEndPoint, CancellationToken ct)
        => GetResultFromApiAsync<PccUserProfile>($"{apiEndPoint}/User/{productUserId}", ct);

    private async Task<T?> GetResultFromApiAsync<T>(string url, CancellationToken ct) where T : class
    {
        var client = _httpClientFactory.CreateClient();
        using var response = await client.GetAsync(url, ct);
        if (!response.IsSuccessStatusCode) return null;
        var json = await response.Content.ReadAsStringAsync(ct);
        return JsonSerializer.Deserialize<T>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }

    /// <summary>
    /// Returns <c>true</c> when a HEAD request to the PCC login-name endpoint succeeds
    /// (HTTP 2xx = user exists / name is already in use).
    /// </summary>
    private async Task<bool> IsUsernameInUseAsync(
        string loginName, string apiEndPoint, CancellationToken ct)
    {
        _logger.LogDebug("IsUsernameInUseAsync - Checking '{LoginName}'", loginName);
        var client = _httpClientFactory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Head,
            new Uri($"{apiEndPoint}/User?loginName={loginName}"));
        request.Headers.Accept.ParseAdd("application/json");
        using var response = await client.SendAsync(request, ct);
        return response.IsSuccessStatusCode;
    }

    private async Task<string> GetUniqueProductLoginNameAsync(
        string productLoginName, string newProductUsername,
        string apiEndPoint, CancellationToken ct)
    {
        int incrementor = 0;
        while (await IsUsernameInUseAsync(productLoginName, apiEndPoint, ct))
        {
            incrementor++;
            productLoginName = $"{newProductUsername}{incrementor}";
        }
        return productLoginName;
    }

    private async Task<string> UpdateProspectContactCenterPropertyUserAsync(
        long userPersonaId, long editorPersonaId,
        string productUserId, string apiEndPoint,
        PccUser prospectUser, CancellationToken ct)
    {
        var currentUser = await GetProspectContactCenterUserAsync(productUserId, apiEndPoint, ct);

        // Reactivate deactivated user
        if (currentUser is not null && !currentUser.UserActive && prospectUser.User.UserActive)
            await ReCreateNewUserAsync(userPersonaId, apiEndPoint, prospectUser, productUserId, isSamlUpdateRequired: false, ct);

        // User type changed → soft-delete then recreate
        if (currentUser is not null &&
            currentUser.UserType?.Trim() != prospectUser.User.UserType?.Trim())
        {
            _logger.LogDebug("UpdateProspectContactCenterPropertyUserAsync - UserType changed. Deactivating. personaId={Id}", userPersonaId);
            await DeactivateCurrentUserAsync(productUserId, prospectUser.ModifyingUser, apiEndPoint, ct);

            var renamed = new PccUser
            {
                ModifyingUser = prospectUser.ModifyingUser,
                User = new PccUserProfile
                {
                    LoginName           = $"{currentUser.LoginName}_GB{DateTime.UtcNow.Ticks}",
                    Email               = $"{currentUser.Email}_GB{DateTime.UtcNow.Ticks}",
                    FirstName           = currentUser.FirstName,
                    LastName            = currentUser.LastName,
                    SystemIdentifier    = currentUser.SystemIdentifier,
                    UserType            = currentUser.UserType,
                    ManagementCompanyID = currentUser.ManagementCompanyID,
                    PropertyID          = currentUser.PropertyID,
                    UserActive          = true
                }
            };

            var renameResult = await UpdateProspectContactCenterUserAsync(
                $"{apiEndPoint}/User", userPersonaId, editorPersonaId, renamed, ct);
            if (!string.IsNullOrEmpty(renameResult)) return renameResult;

            return await ReCreateNewUserAsync(
                userPersonaId, apiEndPoint, prospectUser, productUserId, isSamlUpdateRequired: false, ct);
        }

        // Update property assignment
        var propResult = await UpdateUserPropertyAsync(
            prospectUser.User.Properties ?? [],
            prospectUser.ModifyingUser,
            prospectUser.User.ManagementCompanyID ?? string.Empty,
            productUserId, apiEndPoint, ct);
        if (!string.IsNullOrEmpty(propResult)) return propResult;

        var result = await UpdateProspectContactCenterUserAsync(
            $"{apiEndPoint}/User", userPersonaId, editorPersonaId, prospectUser, ct);

        int batchStatus = string.IsNullOrEmpty(result)
            ? (int)ProductBatchStatusType.Success
            : (int)ProductBatchStatusType.Error;
        await _settingService.UpdateProductStatusAsync(
            userPersonaId, ProductStatusSettingType, ProductId, batchStatus, ct);

        if (!string.IsNullOrEmpty(result))
            _logger.LogError("UpdateProspectContactCenterPropertyUserAsync - Error. personaId={Id} Reason={Reason}", userPersonaId, result);

        return result;
    }

    private async Task<string> ReCreateNewUserAsync(
        long userPersonaId, string apiEndPoint,
        PccUser prospectUser, string productUserId,
        bool isSamlUpdateRequired, CancellationToken ct)
    {
        string newLoginBase = $"{prospectUser.User.FirstName!.TrimWhiteSpace()[..1]}{prospectUser.User.LastName!.TrimWhiteSpace()}".ToLower();
        string newLoginName = await GetUniqueProductLoginNameAsync(
            prospectUser.User.LoginName ?? newLoginBase, newLoginBase, apiEndPoint, ct);

        prospectUser.User.LoginName        = newLoginName;
        prospectUser.User.SystemIdentifier = null;

        _logger.LogDebug("ReCreateNewUserAsync - Creating replacement user. personaId={Id}", userPersonaId);
        string newProductUserId = await InsertProspectContactCenterUserAsync(
            $"{apiEndPoint}/User", userPersonaId, 0, newLoginName, prospectUser, ct);

        if (!isSamlUpdateRequired)
        {
            // Update SAML UserId attribute in GreenBook
            var attrs      = await _samlRepository.GetProductSamlDetailsAsync(userPersonaId, ProductId, ct);
            var userIdAttr = attrs.FirstOrDefault(a =>
                a.Name.Equals("UserId", StringComparison.OrdinalIgnoreCase));
            if (userIdAttr is not null)
            {
                userIdAttr.Value = newProductUserId;
                await _samlRepository.UpdateSamlUserAttributeAsync(userIdAttr, ct);
            }

            await _settingService.UpdateProductStatusAsync(
                userPersonaId, ProductStatusSettingType, ProductId,
                (int)ProductBatchStatusType.Success, ct);
        }

        return string.Empty;
    }

    private async Task<string> DeactivateCurrentUserAsync(
        string productUserId, string modifyingUserId,
        string apiEndPoint, CancellationToken ct)
    {
        var client = _httpClientFactory.CreateClient();
        var url    = $"{apiEndPoint}/User?userId={productUserId}&modifyingUser={modifyingUserId}";
        using var response = await client.DeleteAsync(url, ct);
        if (response.IsSuccessStatusCode) return string.Empty;

        string errorContent = string.Empty;
        try { errorContent = await response.Content.ReadAsStringAsync(ct); }
        catch { /* ignored */ }

        _logger.LogError("DeactivateCurrentUserAsync - Error: {ErrorContent}", errorContent);
        throw new Exception($"Deactivate user Failed. errorContent- {errorContent}");
    }

    /// <summary>
    /// PATCH /User/{id}/relationships/property — updates property assignments.
    /// <para>
    /// Replaces the legacy <c>ExpandoObject</c> + <c>ObjectContent&lt;dynamic&gt;</c>
    /// + <c>JsonMediaTypeFormatter</c> (System.Net.Http.Formatting) with an anonymous
    /// type + <c>JsonContent.Create</c>.
    /// </para>
    /// </summary>
    private async Task<string> UpdateUserPropertyAsync(
        IList<string> propertyIds, string modifyingUserId,
        string companyId, string productUserId,
        string apiEndPoint, CancellationToken ct)
    {
        var body   = new { PropertyID = 0, ModifyingUser = modifyingUserId, Properties = propertyIds, ManagementCompanyID = companyId };
        var apiUrl = $"{apiEndPoint}/User/{productUserId}/relationships/property?_HttpMethod=PATCH";

        var client = _httpClientFactory.CreateClient();
        using var request = new HttpRequestMessage(new HttpMethod("PATCH"), apiUrl)
        {
            Content = JsonContent.Create(body)
        };
        using var response = await client.SendAsync(request, ct);

        if (response.IsSuccessStatusCode)
        {
            _logger.LogDebug("UpdateUserPropertyAsync - Success. productUserId={Id}", productUserId);
            return string.Empty;
        }

        string errorContent = string.Empty;
        try { errorContent = await response.Content.ReadAsStringAsync(ct); }
        catch (Exception ex) { _logger.LogWarning(ex, "UpdateUserPropertyAsync - Could not read error content"); }

        _logger.LogError("UpdateUserPropertyAsync - Error: {ErrorContent}", errorContent);
        return $"Error in UpdateUserProperty; errorContent= {errorContent}";
    }

    private async Task<string> UpdateProspectContactCenterUserAsync(
        string apiUrl, long userPersonaId, long editorPersonaId,
        PccUser prospectUser, CancellationToken ct)
    {
        var client = _httpClientFactory.CreateClient();
        using var response = await client.PutAsync(apiUrl, JsonContent.Create(prospectUser), ct);
        if (response.IsSuccessStatusCode) return string.Empty;

        string errorContent = string.Empty;
        try { errorContent = await response.Content.ReadAsStringAsync(ct); }
        catch (Exception ex) { _logger.LogWarning(ex, "UpdateProspectContactCenterUserAsync - Could not read error content"); }

        _logger.LogError("UpdateProspectContactCenterUserAsync - Error. editorPersonaId={Id}: {Reason}", editorPersonaId, errorContent);
        return $"There was a problem updating the user with editorPersona id - {editorPersonaId} - Error-{errorContent}.";
    }

    /// <summary>
    /// POST /User — creates a new PCC user. Returns the new product user ID string.
    /// <para>
    /// Replaces <c>dynamic userResult = JsonConvert.DeserializeObject&lt;dynamic&gt;(json)</c>
    /// with <c>System.Text.Json.JsonDocument.Parse</c>.
    /// </para>
    /// </summary>
    private async Task<string> InsertProspectContactCenterUserAsync(
        string apiUrl, long userPersonaId, long editorPersonaId,
        string productLoginName, PccUser prospectUser, CancellationToken ct)
    {
        var client = _httpClientFactory.CreateClient();
        using var response = await client.PostAsync(apiUrl, JsonContent.Create(prospectUser), ct);

        if (response.IsSuccessStatusCode)
        {
            var json = await response.Content.ReadAsStringAsync(ct);
            using var doc = JsonDocument.Parse(json);
            string result = doc.RootElement.ValueKind == JsonValueKind.String
                ? doc.RootElement.GetString() ?? string.Empty
                : doc.RootElement.TryGetProperty("id", out var idEl)
                    ? idEl.GetString() ?? string.Empty
                    : json.Trim('"');

            _logger.LogDebug("InsertProspectContactCenterUserAsync - Created. productUserId={Id}", result);
            return result;
        }

        string errorContent = string.Empty;
        try { errorContent = await response.Content.ReadAsStringAsync(ct); }
        catch (Exception ex) { _logger.LogWarning(ex, "InsertProspectContactCenterUserAsync - Could not read error content"); }

        _logger.LogError("InsertProspectContactCenterUserAsync - Error. editorPersonaId={Id}: {Reason}", editorPersonaId, errorContent);
        await _settingService.UpdateProductStatusAsync(
            userPersonaId, ProductStatusSettingType, ProductId,
            (int)ProductBatchStatusType.Error, ct);
        throw new Exception($"There was a problem creating the user with editorPersona id - {editorPersonaId}. Error-{errorContent}");
    }

    private async Task CreateProductUserInGreenBookAsync(
        long userPersonaId, string productUserId,
        string productLoginName, CancellationToken ct)
    {
        _logger.LogDebug("CreateProductUserInGreenBookAsync - Inserting SAML data. loginName={Login} userId={Id}", productLoginName, productUserId);
        await _samlRepository.CreateSamlUserAttributeAsync(
            userPersonaId, ProductId, SamlAttributeEnum.productUsername, productLoginName, ct);
        await _samlRepository.CreateSamlUserAttributeAsync(
            userPersonaId, ProductId, SamlAttributeEnum.UserId, productUserId, ct);
        await _settingService.UpdateProductStatusAsync(
            userPersonaId, ProductStatusSettingType, ProductId,
            (int)ProductBatchStatusType.Success, ct);
    }

    // ── Static helpers ────────────────────────────────────────────────────────

    /// <summary>
    /// Returns <paramref name="loginName"/> when it is a valid e-mail address,
    /// otherwise returns <c>{loginName}@realpage.com</c> as a synthetic address.
    /// </summary>
    private static string ValidateAndReturnEmailAddress(string loginName)
    {
        try { _ = new System.Net.Mail.MailAddress(loginName); return loginName; }
        catch { return $"{loginName}@realpage.com"; }
    }

    private static IList<ProductProperty>? ToGBProperties(IList<ProductPropertyMap>? properties)
    {
        if (properties is null) return null;
        return properties.Select(p => new ProductProperty
        {
            ID     = p.PropertyId,
            Name   = p.PropertyName,
            State  = p.State,
            Active = p.Active,
            Status = p.Active == "true" ? "Active" : "Inactive"
        }).ToList();
    }
}

// ── PCC HTTP payload models ───────────────────────────────────────────────────
// Local sealed classes used only as request/response bodies for the PCC REST API.
// STJ attributes are used (not Newtonsoft) so PutAsJsonAsync / PostAsJsonAsync
// serialise correctly without an additional Newtonsoft pass.

internal sealed class PccUser
{
    [JsonPropertyName("ModifyingUser")] public string       ModifyingUser { get; set; } = string.Empty;
    [JsonPropertyName("User")]          public PccUserProfile User         { get; set; } = new();
}

internal sealed class PccUserProfile
{
    [JsonPropertyName("FirstName")]
    public string? FirstName { get; set; }

    [JsonPropertyName("LastName")]
    public string? LastName { get; set; }

    [JsonPropertyName("LoginName")]
    public string? LoginName { get; set; }

    [JsonPropertyName("UserActive")]
    public bool UserActive { get; set; }

    [JsonPropertyName("UserType")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? UserType { get; set; }

    [JsonPropertyName("ManagementCompanyID")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ManagementCompanyID { get; set; }

    [JsonPropertyName("PropertyID")]
    public string? PropertyID { get; set; }

    [JsonPropertyName("Email")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Email { get; set; }

    [JsonPropertyName("SystemIdentifier")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? SystemIdentifier { get; set; }

    [JsonPropertyName("LastLogin")]
    public DateTime LastLogin { get; set; }

    [JsonPropertyName("Properties")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IList<string>? Properties { get; set; }
}
