using System.ComponentModel.DataAnnotations;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using UnifiedLogin.BusinessLogic.Logic.Product;
using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects.Audit.Common;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Constants;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Extensions;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product;
using UnifiedLogin.SharedObjects.Product.Accounting;
using UnifiedLogin.SharedObjects.Product.Migration;
using UnifiedLogin.SharedObjects.Saml;
using OneSiteAccounting = UnifiedLogin.SharedObjects.Product.OneSiteAccounting;
using blueBook = UnifiedLogin.SharedObjects.BlackBook;

namespace UnifiedLogin.BusinessLogic.LogicAsync.Product;

/// <summary>
/// True async implementation of OneSite Accounting user-management operations.
/// Replaces the stepping-stone wrapper that required a <see cref="DefaultUserClaim"/> at
/// construction time. Context is now resolved internally via
/// <see cref="IProductContextServiceAsync"/> from the supplied persona IDs.
/// </summary>
public sealed class ManageProductOneSiteAccountingAsync : IManageProductOneSiteAccountingAsync
{
    private const int    ProductId                = (int)ProductEnum.FinancialSuite;
    private const string ProductStatusSettingType = "ProductStatus";

    // Audit-log message templates (mirrors ManageProductBase constants)
    private const string RoleAssignMessage     = "{\"action\":\"Assigned\",\"value\":\"RoleName\"}";
    private const string RoleRemovedMessage    = "{\"action\":\"Removed\",\"value\":\"RoleName\"}";
    private const string PropAssignMessage     = "{\"action\":\"Assigned\",\"value\":\"PropertyName\"}";
    private const string PropRemovedMessage    = "{\"action\":\"Removed\",\"value\":\"PropertyName\"}";

    private readonly IProductContextServiceAsync                          _contextService;
    private readonly IProductSettingServiceAsync                          _settingService;
    private readonly IManageBlueBookAsync                                 _blueBook;
    private readonly OneSiteAccounting.IOneSiteAccountingProductService   _service;
    private readonly ILogger<ManageProductOneSiteAccountingAsync>         _logger;

    // ── New dependencies for ManageAccountingUser / profile update ──────────
    private readonly ISamlRepositoryAsync                                 _samlRepository;
    private readonly IManagePersonaAsync                                  _managePersona;
    private readonly IManagePersonAsync                                   _managePerson;
    private readonly IManageUserLoginAsync                                _manageUserLogin;
    private readonly IManageElectronicAddressAsync                        _manageElectronicAddress;
    private readonly IUserRepositoryAsync                                 _userRepository;
    private readonly IProductRepositoryAsync                              _productRepository;

    public ManageProductOneSiteAccountingAsync(
        IProductContextServiceAsync                        contextService,
        IProductSettingServiceAsync                        settingService,
        IManageBlueBookAsync                               blueBook,
        OneSiteAccounting.IOneSiteAccountingProductService service,
        ILogger<ManageProductOneSiteAccountingAsync>       logger,
        ISamlRepositoryAsync                               samlRepository,
        IManagePersonaAsync                                managePersona,
        IManagePersonAsync                                 managePerson,
        IManageUserLoginAsync                              manageUserLogin,
        IManageElectronicAddressAsync                      manageElectronicAddress,
        IUserRepositoryAsync                               userRepository,
        IProductRepositoryAsync                            productRepository)
    {
        ArgumentNullException.ThrowIfNull(contextService);       _contextService       = contextService;
        ArgumentNullException.ThrowIfNull(settingService);       _settingService       = settingService;
        ArgumentNullException.ThrowIfNull(blueBook);             _blueBook             = blueBook;
        ArgumentNullException.ThrowIfNull(service);              _service              = service;
        ArgumentNullException.ThrowIfNull(logger);               _logger               = logger;
        ArgumentNullException.ThrowIfNull(samlRepository);       _samlRepository       = samlRepository;
        ArgumentNullException.ThrowIfNull(managePersona);        _managePersona        = managePersona;
        ArgumentNullException.ThrowIfNull(managePerson);         _managePerson         = managePerson;
        ArgumentNullException.ThrowIfNull(manageUserLogin);      _manageUserLogin      = manageUserLogin;
        ArgumentNullException.ThrowIfNull(manageElectronicAddress); _manageElectronicAddress = manageElectronicAddress;
        ArgumentNullException.ThrowIfNull(userRepository);       _userRepository       = userRepository;
        ArgumentNullException.ThrowIfNull(productRepository);    _productRepository    = productRepository;
    }

    // ── Private per-call context ───────────────────────────────────────────

    /// <summary>
    /// Immutable context resolved once per call.
    /// Mirrors the mutable fields that <c>ManageProductOneSiteAccounting.GetCompanyEditorAndUserDetails</c>
    /// used to populate: <c>_companyName</c>, <c>_intactLogin</c>, <c>_intactPassword</c>,
    /// <c>_productUserId</c>, <c>_productUsername</c>.
    /// </summary>
    private sealed record AccountingCtx(
        Persona EditorPersona,
        string  CompanyName,
        string  IntactLogin,
        string  IntactPassword,
        string  ProductUserId   = "",
        string  ProductUsername = "");

    // ── Context loading ────────────────────────────────────────────────────

    /// <summary>
    /// Resolves the full Accounting call context.
    /// <para>
    /// Replaces <c>ManageProductOneSiteAccounting.GetCompanyEditorAndUserDetails</c>:
    /// verifies personas via <see cref="IProductContextServiceAsync"/>, reads Intact credentials
    /// from product internal settings, and derives the Accounting company name from the editor's
    /// SAML USERID attribute (format <c>COMPANY|USERID</c>) or via a BlueBook fallback.
    /// </para>
    /// </summary>
    private async Task<(AccountingCtx? ctx, ListResponse? error)> GetAccountingContextAsync(
        long editorPersonaId, long userPersonaId, CancellationToken ct)
    {
        var (callCtx, ctxError) = await _contextService.GetUserContextAsync(
            editorPersonaId, userPersonaId, ProductId, ct);
        if (ctxError is not null)
            return (null, ctxError);

        // Intact API credentials are stored as Base64 in product internal settings
        List<ProductInternalSetting> settings = await _settingService.GetProductSettingAsync(ProductId, ct);
        string intactLogin    = DecodeBase64Setting(settings, "INTACTUSER");
        string intactPassword = DecodeBase64Setting(settings, "INTACTPASSWORD");

        // Company name: from the editor's SAML USERID (split on separator) or BlueBook fallback
        string companyName = await ResolveCompanyNameAsync(callCtx!.EditorPersona, callCtx.EditorProductUserId, ct);
        if (string.IsNullOrEmpty(companyName))
        {
            _logger.LogError(
                "GetAccountingContextAsync – missing Accounting company name for editorPersonaId={EditorId}",
                editorPersonaId);
            return (null, new ListResponse { IsError = true, ErrorReason = "Missing company name" });
        }

        return (new AccountingCtx(
            callCtx.EditorPersona, companyName, intactLogin, intactPassword,
            callCtx.ProductUserId, callCtx.ProductUsername), null);
    }

    /// <summary>
    /// Mirrors <c>ManageProductOneSiteAccounting.GetAccountingCompanyFromPersona</c>.
    /// Extracts the company portion from the editor's SAML USERID attribute (format
    /// <c>COMPANY|USERID</c> or <c>COMPANY:USERID</c>), falling back to a BlueBook
    /// FinancialSuite lookup when no SAML attribute is present.
    /// </summary>
    private async Task<string> ResolveCompanyNameAsync(
        Persona editorPersona, string editorProductUserId, CancellationToken ct)
    {
        if (!string.IsNullOrEmpty(editorProductUserId))
        {
            // Normalise separator (stored as either '|' or ':' depending on source)
            string normalised = editorProductUserId.Replace(":", "|");
            return normalised.Split('|')[0];
        }

        // Fallback: look up the FinancialSuite company via BlueBook
        try
        {
            IList<blueBook.CustomerCompanyMap> companyMap = await _blueBook.GetCompanyMapAsync(
                editorPersona.Organization.RealPageId,
                editorPersona.Organization.BooksCustomerMasterId,
                source: BlueBookProductConstants.FinancialSuite,
                domain: editorPersona.Organization.OrganizationDomain.Name,
                cancellationToken: ct);

            return companyMap
                ?.FirstOrDefault(a => a.Source.ToUpper() == BlueBookProductConstants.FinancialSuite)
                ?.CompanyInstanceSourceId ?? string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "ResolveCompanyNameAsync – BlueBook FinancialSuite lookup failed for personaId={PersonaId}",
                editorPersona.PersonaId);
            return string.Empty;
        }
    }

    // ── ChangeStatusAccountingUserAsync ────────────────────────────────────

    /// <inheritdoc/>
    public async Task<string> ChangeStatusAccountingUserAsync(
        long editorPersonaId, long userPersonaId, bool isActive,
        CancellationToken cancellationToken = default)
    {
        var (ctx, error) = await GetAccountingContextAsync(editorPersonaId, userPersonaId, cancellationToken);
        if (error is not null)
            return error.ErrorReason;

        // Mirrors the sync guard: _productUsername == null means user has no Accounting credentials
        if (string.IsNullOrEmpty(ctx!.ProductUsername))
            return "Invalid user";

        var parameters = new OneSiteAccounting.NameValuePair[]
        {
            new() { Name = "CompanyID",        Value = ctx.CompanyName    },
            new() { Name = "Login",            Value = ctx.IntactLogin    },
            new() { Name = "Password",         Value = ctx.IntactPassword },
            new() { Name = "SystemIdentifier", Value = ctx.ProductUserId  }
        };

        try
        {
            _logger.LogDebug(
                "ChangeStatusAccountingUserAsync – editorPersonaId={EditorId} userPersonaId={UserId} isActive={IsActive}",
                editorPersonaId, userPersonaId, isActive);

            string result;
            // IOneSiteAccountingProductService does not expose async NameValuePair[] overloads for
            // EnableUser / DisableUser; wrap the sync SOAP calls so the thread pool absorbs blocking I/O.
            if (isActive)
            {
                result = await Task.Run(() => _service.EnableUser(parameters), cancellationToken);
                await _settingService.UpdateProductStatusAsync(
                    userPersonaId, ProductStatusSettingType, ProductId,
                    (int)ProductBatchStatusType.Success, cancellationToken);
            }
            else
            {
                result = await Task.Run(() => _service.DisableUser(parameters), cancellationToken);
                await _settingService.UpdateProductStatusAsync(
                    userPersonaId, ProductStatusSettingType, ProductId,
                    (int)ProductBatchStatusType.Inactive, cancellationToken);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "ChangeStatusAccountingUserAsync – failed for userPersonaId={UserId} isActive={IsActive}",
                userPersonaId, isActive);
            return "Update failed";
        }
    }

    // ── ChangeAccountingUserClaimStatusAsync ───────────────────────────────

    /// <inheritdoc/>
    public async Task<bool> ChangeAccountingUserClaimStatusAsync(
        long editorPersonaId, long userPersonaId, bool isLinked,
        CancellationToken cancellationToken = default)
    {
        var (ctx, error) = await GetAccountingContextAsync(editorPersonaId, userPersonaId, cancellationToken);
        if (error is not null || string.IsNullOrEmpty(ctx!.ProductUserId))
            return false;

        try
        {
            _logger.LogDebug(
                "ChangeAccountingUserClaimStatusAsync – userPersonaId={UserId} isLinked={IsLinked}",
                userPersonaId, isLinked);

            // ChangeClaimStatusAsync is exposed directly on IOneSiteAccountingProductService
            await _service.ChangeClaimStatusAsync(
                ctx.ProductUserId, isLinked,
                ctx.IntactLogin, ctx.IntactPassword,
                ctx.ProductUsername);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "ChangeAccountingUserClaimStatusAsync – failed for userPersonaId={UserId} isLinked={IsLinked}",
                userPersonaId, isLinked);
            return false;
        }
    }

    // ── ChangeUserStatusAsync ──────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<bool> ChangeUserStatusAsync(
        long editorPersonaId, string userName,
        CancellationToken cancellationToken = default)
    {
        // userPersonaId=0: only the editor context is needed; company name comes from editor's SAML/BlueBook.
        // The sync implementation defaults isActive=false (disable), and the interface omits that flag,
        // so this method always disables the named Accounting user.
        var (ctx, error) = await GetAccountingContextAsync(editorPersonaId, 0, cancellationToken);
        if (error is not null)
        {
            _logger.LogError(
                "ChangeUserStatusAsync – context error for editorPersonaId={EditorId}: {Reason}",
                editorPersonaId, error.ErrorReason);
            return false;
        }

        var parameters = new OneSiteAccounting.NameValuePair[]
        {
            new() { Name = "CompanyID",        Value = ctx!.CompanyName    },
            new() { Name = "Login",            Value = ctx.IntactLogin    },
            new() { Name = "Password",         Value = ctx.IntactPassword },
            new() { Name = "SystemIdentifier", Value = userName            }
        };

        try
        {
            _logger.LogDebug(
                "ChangeUserStatusAsync – disabling editorPersonaId={EditorId} userName={UserName}",
                editorPersonaId, userName);

            // IOneSiteAccountingProductService does not expose an async NameValuePair[] overload for DisableUser;
            // wrap the sync call so the thread pool absorbs the blocking SOAP I/O.
            string result = await Task.Run(() => _service.DisableUser(parameters), cancellationToken);

            if (result.Trim().ToUpper().Contains("INACTIVATED"))
            {
                _logger.LogDebug("ChangeUserStatusAsync – success for userName={UserName}", userName);
                return true;
            }

            _logger.LogError(
                "ChangeUserStatusAsync – unexpected response for userName={UserName}: {Result}",
                userName, result);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ChangeUserStatusAsync – failed for userName={UserName}", userName);
            return false;
        }
    }

    // ── GetMigrationUsersAsync ─────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<ListResponse> GetMigrationUsersAsync(
        long editorPersonaId, RequestParameter datafilter,
        CancellationToken cancellationToken = default)
    {
        var defaultError = new ListResponse { IsError = true, ErrorReason = "No Users." };

        var (ctx, error) = await GetAccountingContextAsync(editorPersonaId, 0, cancellationToken);
        if (error is not null) { defaultError.ErrorReason = error.ErrorReason; return defaultError; }

        var loginInfo = new List<OneSiteAccounting.NameValuePair>
        {
            new() { Name = "CompanyID",     Value = ctx!.CompanyName    },
            new() { Name = "Login",         Value = ctx.IntactLogin    },
            new() { Name = "Password",      Value = ctx.IntactPassword },
            new() { Name = "OneTimeExport", Value = "true"              }
        };

        var userInfo = new OneSiteAccounting.User[]
        {
            new() { NameValuePair = loginInfo.ToArray() }
        };

        bool filter        = true;
        int  startRow      = 0;
        int  resultPerPage = 1000;

        if (datafilter != null)
        {
            if (datafilter.FilterBy.ContainsKey("filter"))
                filter = datafilter.FilterBy["filter"].ToUpper() == "NONMIGRATED";

            if (datafilter.Pages != null)
            {
                startRow      = datafilter.Pages.StartRow;
                resultPerPage = datafilter.Pages.ResultsPerPage;
            }
        }

        var wsParams = new OneSiteAccounting.FilterSortParameters
        {
            StartPosition = startRow.ToString(),
            PageLength    = resultPerPage.ToString(),
            FilterConditionList = new OneSiteAccounting.FilterConditionList[]
            {
                new()
                {
                    LogicalOperator = "OR",
                    FilterCondition = new OneSiteAccounting.FilterCondition[]
                    {
                        new()
                        {
                            PropertyName        = "excludeassign",
                            ComparisionOperator = "equalto",
                            SearchValue         = filter ? "1" : "0"
                        }
                    }
                }
            }
        };

        try
        {
            _logger.LogDebug("GetMigrationUsersAsync – editorPersonaId={EditorId}", editorPersonaId);

            // No async overload for the User[]+FilterSortParameters+out TotalRows[] signature on the interface;
            // wrap in Task.Run so the thread pool absorbs the blocking SOAP I/O.
            OneSiteAccounting.TotalRows[] results2 = Array.Empty<OneSiteAccounting.TotalRows>();
            var users = await Task.Run(
                () => _service.GetAllUsers(userInfo, wsParams, out results2), cancellationToken);

            var totalRow = results2.FirstOrDefault() ?? new OneSiteAccounting.TotalRows { TotalRows1 = "0" };

            if (users == null)
            {
                if (totalRow.TotalRows1?.ToUpper().Contains("NOT A VALID USERID") == true)
                    defaultError.ErrorReason = "Invalid user.";

                _logger.LogError(
                    "GetMigrationUsersAsync – null response for editorPersonaId={EditorId}: {Reason}",
                    editorPersonaId, defaultError.ErrorReason);
                return defaultError;
            }

            var migrationUsers = users.Select(u => new MigrationUser
            {
                FirstName               = u.FirstName,
                LastName                = u.LastName,
                Username                = u.UserID,
                UserId                  = u.UserID,
                Email                   = u.EmailAddress,
                CompanyInstanceSourceId = ctx.CompanyName,
                Title                   = u.TITLE,
                MiddleName              = u.MIDDLENAME,
                LastActivity            = u.LASTACCESSDATE,
                Phone                   = u.PHONENUMBER,
                Status                  = u.USERSTATUS == "F" ? "Disabled" : "Active"
            }).ToList();

            _logger.LogDebug(
                "GetMigrationUsersAsync – received {Count} users for editorPersonaId={EditorId}",
                migrationUsers.Count, editorPersonaId);

            return new ListResponse
            {
                IsError     = false,
                ErrorReason = string.Empty,
                Records     = migrationUsers.Cast<object>().ToList(),
                TotalRows   = string.IsNullOrEmpty(totalRow.TotalRows1) ? 0 : Convert.ToInt32(totalRow.TotalRows1),
                RowsPerPage = Convert.ToInt32(wsParams.PageLength),
                TotalPages  = 1
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetMigrationUsersAsync – failed for editorPersonaId={EditorId}", editorPersonaId);
            return new ListResponse { IsError = true, ErrorReason = ex.Message };
        }
    }

    // ── UpdateUsersMigrationStatusAsync ───────────────────────────────────

    /// <inheritdoc/>
    public async Task<MigrateResponse> UpdateUsersMigrationStatusAsync(
        long editorPersonaId, IList<MigrateUser> migrateUsers,
        CancellationToken cancellationToken = default)
    {
        var migrateResponse = new MigrateResponse { Status = false };

        var (ctx, error) = await GetAccountingContextAsync(editorPersonaId, 0, cancellationToken);
        if (error is not null) { migrateResponse.Message = error.ErrorReason; return migrateResponse; }

        foreach (var migrateUser in migrateUsers)
        {
            var loginInfo = new OneSiteAccounting.NameValuePair[]
            {
                new() { Name = "CompanyID",        Value = ctx!.CompanyName                            },
                new() { Name = "Login",            Value = ctx.IntactLogin                            },
                new() { Name = "Password",         Value = ctx.IntactPassword                         },
                new() { Name = "SystemIdentifier", Value = $"{ctx.CompanyName}|{migrateUser.UserId}" }
            };

            try
            {
                string message;
                // IOneSiteAccountingProductService does not expose async NameValuePair[] overloads for
                // EnableGreenBookUser / DisableGreenBookUser; wrap in Task.Run.
                if (migrateUser.UsingUnifiedLogin)
                {
                    _logger.LogDebug(
                        "UpdateUsersMigrationStatusAsync – EnableGreenBookUser for userId={UserId}", migrateUser.UserId);
                    message = await Task.Run(() => _service.EnableGreenBookUser(loginInfo), cancellationToken);
                }
                else
                {
                    _logger.LogDebug(
                        "UpdateUsersMigrationStatusAsync – DisableGreenBookUser for userId={UserId}", migrateUser.UserId);
                    message = await Task.Run(() => _service.DisableGreenBookUser(loginInfo), cancellationToken);
                }
                migrateResponse.Message += message;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "UpdateUsersMigrationStatusAsync – failed for userId={UserId}", migrateUser.UserId);
                migrateResponse.Message += ex.Message;
            }
        }

        migrateResponse.Status = true;
        return migrateResponse;
    }

    // ── GetRolesCountAsync ─────────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<ListResponse> GetRolesCountAsync(
        long editorPersonaId, RequestParameter datafilter,
        CancellationToken cancellationToken = default)
    {
        // Sync calls GetCompanyEditorAndUserDetails(editorPersonaId, editorPersonaId) so the
        // editor's own SAML attributes are also resolved as the user's product credentials.
        var (ctx, error) = await GetAccountingContextAsync(editorPersonaId, editorPersonaId, cancellationToken);
        if (error is not null) return error;

        var loginInfo = new List<OneSiteAccounting.NameValuePair>
        {
            new() { Name = "CompanyID", Value = ctx!.CompanyName    },
            new() { Name = "Login",     Value = ctx.IntactLogin    },
            new() { Name = "Password",  Value = ctx.IntactPassword }
        };
        if (!string.IsNullOrEmpty(ctx.ProductUserId))
            loginInfo.Add(new() { Name = "SystemIdentifier", Value = ctx.ProductUserId });

        var permissions = new OneSiteAccounting.Permissions[]
        {
            new() { NameValuePair = loginInfo.ToArray() }
        };

        try
        {
            _logger.LogDebug("GetRolesCountAsync – editorPersonaId={EditorId}", editorPersonaId);

            // No async Permissions[] overload on the interface; wrap in Task.Run.
            var permissionList = await Task.Run(
                () => _service.GetApplicationPermissions(permissions), cancellationToken);

            var list = permissionList.ToRoles();
            return new ListResponse
            {
                Records     = list.Cast<object>().ToList(),
                TotalRows   = list.Count,
                RowsPerPage = 9999,
                TotalPages  = 1,
                ErrorReason = string.Empty
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetRolesCountAsync – failed for editorPersonaId={EditorId}", editorPersonaId);
            return new ListResponse { IsError = true, ErrorReason = ex.Message };
        }
    }

    // ── GetRightsForRoleAsync ──────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<ListResponse> GetRightsForRoleAsync(
        long editorPersonaId, RequestParameter datafilter, string roleName, int roleId,
        CancellationToken cancellationToken = default)
    {
        // Sync calls GetCompanyEditorAndUserDetails(editorPersonaId, editorPersonaId)
        var (ctx, error) = await GetAccountingContextAsync(editorPersonaId, editorPersonaId, cancellationToken);
        if (error is not null) return error;

        var loginInfo = new List<OneSiteAccounting.NameValuePair>
        {
            new() { Name = "CompanyID", Value = ctx!.CompanyName    },
            new() { Name = "Login",     Value = ctx.IntactLogin    },
            new() { Name = "Password",  Value = ctx.IntactPassword }
        };
        if (!string.IsNullOrEmpty(ctx.ProductUserId))
            loginInfo.Add(new() { Name = "SystemIdentifier", Value = ctx.ProductUserId });
        if (roleId != 0)
            loginInfo.Add(new() { Name = "RoleName", Value = roleName });

        var permissions = new OneSiteAccounting.Permissions[]
        {
            new() { NameValuePair = loginInfo.ToArray() }
        };

        try
        {
            _logger.LogDebug(
                "GetRightsForRoleAsync – editorPersonaId={EditorId} roleName={RoleName} roleId={RoleId}",
                editorPersonaId, roleName, roleId);

            // No async Permissions[] overload on the interface; wrap in Task.Run.
            var roleList = await Task.Run(
                () => _service.GetRolePermissions(permissions), cancellationToken);

            var list = roleList.ToRights();
            return new ListResponse
            {
                Records     = list.Cast<object>().ToList(),
                TotalRows   = list.Count,
                RowsPerPage = 9999,
                TotalPages  = 1,
                ErrorReason = string.Empty
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "GetRightsForRoleAsync – failed for editorPersonaId={EditorId} roleName={RoleName}",
                editorPersonaId, roleName);
            return new ListResponse { IsError = true, ErrorReason = ex.Message };
        }
    }

    // ── GetUserPropertiesAsync ─────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<ListResponse> GetUserPropertiesAsync(
        long editorPersonaId, long userPersonaId, RequestParameter datafilter,
        CancellationToken cancellationToken = default)
    {
        var (ctx, error) = await GetAccountingContextAsync(editorPersonaId, userPersonaId, cancellationToken);
        if (error is not null) return error;

        var wsParams = ManageProductOneSiteAccountingHelpers.GenerateSearchAndPaging(datafilter, "Name", 0, 9999);
        var loginInfo = BuildLoginInfo(ctx!);
        var prop = new OneSiteAccounting.Property[] { new() { NameValuePair = loginInfo } };

        try
        {
            _logger.LogDebug("GetUserPropertiesAsync – editorPersonaId={EditorId} userPersonaId={UserId}",
                editorPersonaId, userPersonaId);

            OneSiteAccounting.TotalRows[] results2 = Array.Empty<OneSiteAccounting.TotalRows>();
            var location = await Task.Run(
                () => _service.GetAllProperties(prop, wsParams, out results2), cancellationToken);
            var list = location?.ToGBProperties();

            if (list == null || list.Count == 0)
            {
                // Fallback to company-level properties
                var companyProps = await GetAllCompanyPropertiesAsync(ctx, cancellationToken);
                var gbProps = companyProps?.ToGBProperties();
                if (gbProps != null)
                    list = gbProps;
                else if (results2.Length > 0 &&
                         results2[0].TotalRows1?.ToUpper().Contains("NOT A VALID USERID") == true)
                    return new ListResponse { IsError = true, ErrorReason = "Invalid user" };

                list ??= [];
            }

            var allProperties = new Dictionary<string, bool>
            {
                ["allProperties"] = !list.Any(p => p.IsAssigned == true)
            };

            return new ListResponse
            {
                Records     = list.Cast<object>().ToList(),
                TotalRows   = list.Count,
                RowsPerPage = list.Count,
                TotalPages  = 1,
                ErrorReason = string.Empty,
                Additional  = allProperties
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetUserPropertiesAsync – failed for userPersonaId={UserId}", userPersonaId);
            return new ListResponse { IsError = true, ErrorReason = ex.Message };
        }
    }

    // ── GetUserPropertyGroupsAsync ─────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<ListResponse> GetUserPropertyGroupsAsync(
        long editorPersonaId, long userPersonaId, RequestParameter datafilter,
        CancellationToken cancellationToken = default)
    {
        var (ctx, error) = await GetAccountingContextAsync(editorPersonaId, userPersonaId, cancellationToken);
        if (error is not null) return error;

        var wsParams = ManageProductOneSiteAccountingHelpers.GenerateSearchAndPaging(datafilter, "Name", 0, 9999);
        var loginInfo = BuildLoginInfo(ctx!);
        var prop = new OneSiteAccounting.Property[] { new() { NameValuePair = loginInfo } };

        try
        {
            _logger.LogDebug("GetUserPropertyGroupsAsync – editorPersonaId={EditorId} userPersonaId={UserId}",
                editorPersonaId, userPersonaId);

            OneSiteAccounting.TotalRows[] results2 = Array.Empty<OneSiteAccounting.TotalRows>();
            var location = await Task.Run(
                () => _service.GetAllPropertyGroups(prop, wsParams, out results2), cancellationToken);
            var list = location?.ToGBPropertyGroup();

            if (list == null)
            {
                if (results2.Length > 0 &&
                    results2[0].TotalRows1?.ToUpper().Contains("NOT A VALID USERID") == true)
                    return new ListResponse { IsError = true, ErrorReason = "Invalid user" };

                list = [];
            }

            return new ListResponse
            {
                Records     = list.Cast<object>().ToList(),
                TotalRows   = list.Count,
                RowsPerPage = list.Count,
                TotalPages  = 1,
                ErrorReason = string.Empty
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetUserPropertyGroupsAsync – failed for userPersonaId={UserId}", userPersonaId);
            return new ListResponse { IsError = true, ErrorReason = ex.Message };
        }
    }

    // ── GetPropertyGroupEntitiesAsync ──────────────────────────────────────

    /// <inheritdoc/>
    public async Task<ListResponse> GetPropertyGroupEntitiesAsync(
        long editorPersonaId, long userPersonaId, string locationGrpId,
        RequestParameter datafilter, CancellationToken cancellationToken = default)
    {
        var (ctx, error) = await GetAccountingContextAsync(editorPersonaId, userPersonaId, cancellationToken);
        if (error is not null) return error;

        var wsParams = ManageProductOneSiteAccountingHelpers.GenerateSearchAndPaging(datafilter, "Name", 0, 9999);
        var loginInfo = BuildLoginInfo(ctx!);
        if (!string.IsNullOrEmpty(locationGrpId))
            loginInfo = [.. loginInfo, new OneSiteAccounting.NameValuePair { Name = "locGroupIds", Value = locationGrpId }];

        var prop = new OneSiteAccounting.Property[] { new() { NameValuePair = loginInfo } };

        try
        {
            _logger.LogDebug("GetPropertyGroupEntitiesAsync – editorPersonaId={EditorId} locationGrpId={GrpId}",
                editorPersonaId, locationGrpId);

            OneSiteAccounting.TotalRows[] results2 = Array.Empty<OneSiteAccounting.TotalRows>();
            var location = await Task.Run(
                () => _service.GetAllPropertyGroupMembers(prop, wsParams, out results2), cancellationToken);
            var list = location?.ToGBPropertyGroup();

            if (list == null)
            {
                if (results2.Length > 0 &&
                    results2[0].TotalRows1?.ToUpper().Contains("NOT A VALID USERID") == true)
                    return new ListResponse { IsError = true, ErrorReason = "Invalid user" };

                list = [];
            }

            return new ListResponse
            {
                Records     = list.Cast<object>().ToList(),
                TotalRows   = list.Count,
                RowsPerPage = list.Count,
                TotalPages  = 1,
                ErrorReason = string.Empty
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetPropertyGroupEntitiesAsync – failed for userPersonaId={UserId}", userPersonaId);
            return new ListResponse { IsError = true, ErrorReason = ex.Message };
        }
    }

    // ── GetUserPropertiesNewAsync ──────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<ListResponse> GetUserPropertiesNewAsync(
        long editorPersonaId, long userPersonaId, RequestParameter datafilter,
        CancellationToken cancellationToken = default)
    {
        var (ctx, error) = await GetAccountingContextAsync(editorPersonaId, userPersonaId, cancellationToken);
        if (error is not null) return error;

        try
        {
            _logger.LogDebug("GetUserPropertiesNewAsync – editorPersonaId={EditorId} userPersonaId={UserId}",
                editorPersonaId, userPersonaId);

            var companyProps = await GetAllCompanyPropertiesAsync(ctx!, cancellationToken);
            var list = (companyProps ?? [])
                .FindAll(p => !string.IsNullOrEmpty(p.PropertyId) && !string.IsNullOrEmpty(p.PropertyName));

            // For MConsole PMCs, append company ID to disambiguate
            if (list.Count(p => !string.IsNullOrEmpty(p.MConsoleId.Trim())) != 0)
                list.ForEach(x => x.Id = $"{x.Id}|{x.CompanyId}");

            return new ListResponse
            {
                Records     = list.Cast<object>().ToList(),
                TotalRows   = list.Count,
                RowsPerPage = list.Count,
                TotalPages  = 1,
                ErrorReason = string.Empty
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetUserPropertiesNewAsync – failed for userPersonaId={UserId}", userPersonaId);
            return new ListResponse { IsError = true, ErrorReason = CommonMessageConstants.EntityErrorMessage };
        }
    }

    // ── GetUserCompaniesAsync ──────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<ListResponse> GetUserCompaniesAsync(
        long editorPersonaId, long userPersonaId, RequestParameter datafilter,
        CancellationToken cancellationToken = default)
    {
        var (ctx, error) = await GetAccountingContextAsync(editorPersonaId, userPersonaId, cancellationToken);
        if (error is not null) return error;

        try
        {
            _logger.LogDebug("GetUserCompaniesAsync – editorPersonaId={EditorId} userPersonaId={UserId}",
                editorPersonaId, userPersonaId);

            var cmpList = await GetUserCompaniesDetailsAsync(ctx!, cancellationToken);
            var aUser   = new AccountingUser();

            // Check SiteSpendManagement product assignment for the org
            var orgProductIds = await _productRepository.GetProductIdsByCompanyAsync(
                ctx!.EditorPersona.Organization.RealPageId, cancellationToken);
            if (orgProductIds?.Contains((int)ProductEnum.SiteSpendManagement) == true)
                aUser.IsSiteSpendManagementAssignedToCompany = true;

            if (userPersonaId != 0)
            {
                var userNvp = await GetUserAsync(ctx, cancellationToken);
                if (userNvp != null)
                {
                    foreach (var item in userNvp)
                    {
                        switch (item.Name.ToUpperInvariant())
                        {
                            case "UNRESTRICTED":
                                aUser.HasAccessToAllCurrentFutureProperties = item.Value == "true";
                                break;
                            case "RPPORTALUSER":
                                aUser.HasAccessToSiteSpendManagementOnly = item.Value == "true";
                                break;
                            case "ADMIN":
                                aUser.IsAccountingAdmin = item.Value == "true";
                                break;
                        }
                    }
                }
                aUser.HasAccessToAllCurrentFutureProperties =
                    await ComputeFlagBasedOnCompanyAndPropertySelectedAsync(
                        ctx, editorPersonaId, userPersonaId, datafilter, cancellationToken);
            }

            var propertyList = await GetAllCompanyPropertiesAsync(ctx!, cancellationToken);
            aUser.IsMConsolePMC = propertyList?.Count(p => !string.IsNullOrEmpty(p.MConsoleId.Trim())) > 0;

            return new ListResponse
            {
                Records     = cmpList.Cast<object>().ToList(),
                TotalRows   = cmpList.Count,
                RowsPerPage = cmpList.Count,
                TotalPages  = 1,
                ErrorReason = string.Empty,
                Additional  = aUser
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetUserCompaniesAsync – failed for userPersonaId={UserId}", userPersonaId);
            var resp = new ListResponse { IsError = true };
            resp.ErrorReason = CommonMessageConstants.CompanyTabErrorMessage;
            return resp;
        }
    }

    // ── GetUserRolesAsync ──────────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<ListResponse> GetUserRolesAsync(
        long editorPersonaId, long userPersonaId, RequestParameter datafilter,
        CancellationToken cancellationToken = default)
    {
        var (ctx, error) = await GetAccountingContextAsync(editorPersonaId, userPersonaId, cancellationToken);
        if (error is not null) return error;

        var wsParams  = ManageProductOneSiteAccountingHelpers.GenerateSearchAndPaging(datafilter, "Name", 0, 9999);
        var loginInfo = BuildLoginInfo(ctx!);
        var role      = new OneSiteAccounting.Role[] { new() { NameValuePair = loginInfo } };

        try
        {
            _logger.LogDebug("GetUserRolesAsync – editorPersonaId={EditorId} userPersonaId={UserId}",
                editorPersonaId, userPersonaId);

            OneSiteAccounting.TotalRows[] results2 = Array.Empty<OneSiteAccounting.TotalRows>();
            var roleList = await Task.Run(
                () => _service.GetAllRoles(role, wsParams, out results2), cancellationToken);
            var list = roleList?.ToGBRoles();

            if (list == null)
            {
                if (results2.Length > 0 &&
                    results2[0].TotalRows1?.ToUpper().Contains("NOT A VALID USERID") == true)
                    return new ListResponse { IsError = true, ErrorReason = "Invalid user" };

                list = [];
            }

            return new ListResponse
            {
                Records     = list.Cast<object>().ToList(),
                TotalRows   = list.Count,
                RowsPerPage = 9999,
                TotalPages  = 1,
                ErrorReason = string.Empty
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetUserRolesAsync – failed for userPersonaId={UserId}", userPersonaId);
            return new ListResponse { IsError = true, ErrorReason = CommonMessageConstants.RoleErrorMessage };
        }
    }

    // ── AssignAllCurrentCompaniesToUserAsync ──────────────────────────────

    /// <inheritdoc/>
    public async Task<(string error, List<AdditionalParameters> auditParams)> AssignAllCurrentCompaniesToUserAsync(
        long editorPersonaId, long userPersonaId,
        List<string> propertiesToAssign, bool isAccountingAdmin,
        BatchProcessType batchProcessType,
        List<ACProperty>? beforeUpdatePropertiesList = null,
        List<ProductPropertyGroup>? beforeUpdateLocationGrpList = null,
        List<ACProperty>? beforeUpdateEntitiesList = null,
        CancellationToken cancellationToken = default)
    {
        var (ctx, error) = await GetAccountingContextAsync(editorPersonaId, userPersonaId, cancellationToken);
        if (error is not null) return (error.ErrorReason!, []);

        var currentCompanyList = await GetUserCompaniesDetailsAsync(ctx!, cancellationToken);
        propertiesToAssign.Clear();
        propertiesToAssign.AddRange(currentCompanyList.Select(c => c.Id));

        return await UpdatePropertiesToUserInternalAsync(
            ctx!, editorPersonaId, userPersonaId, propertiesToAssign,
            isAccountingAdmin, batchProcessType,
            isUnRestrictedAccessToProp: true,
            beforeUpdatePropertiesList, beforeUpdateLocationGrpList, beforeUpdateEntitiesList,
            cancellationToken);
    }

    // ── UpdatePropertiesToUserAsync ────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<(string error, List<AdditionalParameters> auditParams)> UpdatePropertiesToUserAsync(
        long editorPersonaId, long userPersonaId,
        List<string> propertiesToAssign, bool isAccountingAdmin,
        BatchProcessType batchProcessType = BatchProcessType.CreateUpdateProductUser,
        bool isUnRestrictedAccessToProp = false,
        List<ACProperty>? beforeUpdatePropertiesList = null,
        List<ProductPropertyGroup>? beforeUpdateLocationGrpList = null,
        List<ACProperty>? beforeUpdateEntitiesList = null,
        CancellationToken cancellationToken = default)
    {
        var (ctx, error) = await GetAccountingContextAsync(editorPersonaId, userPersonaId, cancellationToken);
        if (error is not null) return (error.ErrorReason!, []);

        return await UpdatePropertiesToUserInternalAsync(
            ctx!, editorPersonaId, userPersonaId, propertiesToAssign,
            isAccountingAdmin, batchProcessType, isUnRestrictedAccessToProp,
            beforeUpdatePropertiesList, beforeUpdateLocationGrpList, beforeUpdateEntitiesList,
            cancellationToken);
    }

    // ── UpdateRolesToUserAsync ─────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<(string error, List<AdditionalParameters> auditParams)> UpdateRolesToUserAsync(
        long editorPersonaId, long userPersonaId,
        List<string> rolesToAssign, bool isAccountingAdmin,
        BatchProcessType batchProcessType = BatchProcessType.CreateUpdateProductUser,
        ListResponse? currentRolesList = null,
        CancellationToken cancellationToken = default)
    {
        var (ctx, error) = await GetAccountingContextAsync(editorPersonaId, userPersonaId, cancellationToken);
        if (error is not null) return (error.ErrorReason!, []);

        return await UpdateRolesToUserInternalAsync(
            ctx!, editorPersonaId, userPersonaId, rolesToAssign,
            batchProcessType, currentRolesList, cancellationToken);
    }

    // ── ManageAccountingUserAsync ──────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<(string error, List<AdditionalParameters> auditParams)> ManageAccountingUserAsync(
        long editorPersonaId, long userPersonaId,
        List<string> roleList, List<string> propertyList, List<string> companyList,
        bool isAccountingAdmin, bool isSiteSpendManagementUser, bool isUnRestrictedAccessToProp,
        BatchProcessType batchProcessType = BatchProcessType.CreateUpdateProductUser,
        CancellationToken cancellationToken = default)
    {
        var auditParams = new List<AdditionalParameters>();
        try
        {
            var (ctx, ctxError) = await GetAccountingContextAsync(editorPersonaId, userPersonaId, cancellationToken);
            if (ctxError is not null) return (ctxError.ErrorReason!, auditParams);

            // Pre-load current lists before any mutations
            var datafilter       = new RequestParameter();
            var currentPropList  = await GetAllCompanyPropertiesAsync(ctx!, cancellationToken) ?? [];
            var currentLocGrpList= await GetAllPropertyGroupsAsync(ctx!, cancellationToken) ?? [];
            var entitiesResponse = await GetUserPropertiesNewAsync(editorPersonaId, userPersonaId, datafilter, cancellationToken);
            var currentEntities  = entitiesResponse?.Records?.Cast<ACProperty>().ToList() ?? [];
            var currentRoleListResponse = await GetUserRolesAsync(editorPersonaId, userPersonaId, datafilter, cancellationToken);

            // Persona / person / login details
            var userPersona = await _managePersona.GetPersonaAsync(userPersonaId, false, cancellationToken);
            var person      = await _managePerson.GetPersonAsync(userPersona.RealPageId, cancellationToken);
            var userLogin   = await _manageUserLogin.GetUserLoginOnlyAsync(userPersona.RealPageId, cancellationToken);
            bool isSuperUser = await _contextService.IsSuperUserAsync(userPersona, cancellationToken);

            string supervisorId = await GetSupervisorUserDetailsAsync(
                userPersona.UserId, userPersona.OrganizationPartyId,
                ctx!.EditorPersona.Organization.PartyId, cancellationToken);

            string userEmailAddress = await ResolveEmailAddressAsync(
                userLogin.RealPageId, userLogin.LoginName, cancellationToken);
            userEmailAddress = ValidateAndReturnEmail(userEmailAddress);

            string accountingLoginName;
            bool   isNewUser = string.IsNullOrEmpty(ctx.ProductUserId);

            if (isNewUser)
            {
                // Generate a unique Accounting login name derived from first+last name
                string lastNameNoWhiteSpace = person.LastName.TrimWhiteSpace();
                string baseUsername = (person.FirstName.TrimWhiteSpace().Substring(0, 1)
                    + lastNameNoWhiteSpace.Substring(0, Math.Min(lastNameNoWhiteSpace.Length, 19))).ToLower();
                accountingLoginName = baseUsername;
                while (await CheckIfUserLoginIsUsedAsync(ctx, accountingLoginName, cancellationToken))
                    accountingLoginName = baseUsername + Interlocked.Increment(ref _loginNameIncrementor);
            }
            else
            {
                accountingLoginName = ctx.ProductUsername;
            }

            accountingLoginName = RemoveSpecialCharacter(accountingLoginName);
            string randomPassword = Guid.NewGuid().ToString().Replace("-", "");

            string firstName = person.FirstName.Substring(0, Math.Min(person.FirstName.Length, 40));
            string lastName  = person.LastName .Substring(0, Math.Min(person.LastName .Length, 40));

            var parameters = new List<OneSiteAccounting.NameValuePair>
            {
                new() { Name = "CompanyID",             Value = ctx.CompanyName                              },
                new() { Name = "Login",                 Value = ctx.IntactLogin                              },
                new() { Name = "Password",              Value = ctx.IntactPassword                           },
                new() { Name = "LoginId",               Value = accountingLoginName                          },
                new() { Name = "ConInfoFirstName",      Value = firstName                                    },
                new() { Name = "ConInfoLastName",       Value = lastName                                     },
                new() { Name = "ConInfoEmail1",         Value = userEmailAddress                             },
                new() { Name = "ConInfoContactName",    Value = string.Empty                                 },
                new() { Name = "Description",           Value = $"{firstName} {lastName}"                   },
                new() { Name = "LoginDisabled",         Value = "false"                                     },
                new() { Name = "UnRestricted",          Value = (isUnRestrictedAccessToProp || isSuperUser) ? "true" : "false" },
                new() { Name = "SSOEnabled",            Value = "true"                                       },
                new() { Name = "SSOCompanyEnabled",     Value = "Enabled"                                   },
                new() { Name = "Visible",               Value = "true"                                       },
                new() { Name = "Status",                Value = "true"                                       },
                new() { Name = "PortalUser",            Value = isSiteSpendManagementUser ? "true" : "false" },
                new() { Name = "Admin",                 Value = (isSuperUser || isAccountingAdmin) ? "true" : "false" },
                new() { Name = "SupervisorUserId",      Value = supervisorId                                 }
            };

            List<string> rolesToCarryForward      = [];
            List<string> adminRolesCarryForward    = [];
            bool         isAdmin                   = false;
            bool         hasAdminRolesToCarryOver  = false;

            if (isNewUser)
            {
                await _settingService.UpdateProductStatusAsync(
                    userPersonaId, ProductStatusSettingType, ProductId,
                    (int)ProductBatchStatusType.Running, cancellationToken);

                parameters.Add(new() { Name = "UserType",        Value = "business user" });
                parameters.Add(new() { Name = "PWDNeverExpires", Value = "true"          });
                parameters.Add(new() { Name = "PWDQlyNotEnforced", Value = "true"        });

                var userArr = parameters.ToArray();
                _logger.LogDebug("ManageAccountingUserAsync – CreateUser userPersonaId={UserId}", userPersonaId);

                // No async overload for CreateUser NameValuePair[]; wrap in Task.Run.
                var userResult = await Task.Run(() => _service.CreateUser(userArr), cancellationToken);

                if (userResult[0].Value.ToUpper().Contains("CAN'T CREATE THE USER") ||
                    userResult[0].Value.ToUpper().Contains("SECURITY QUESTIONS AND ANSWER COULD NOT BE UPDATED"))
                {
                    _logger.LogError("ManageAccountingUserAsync – CreateUser failed for userPersonaId={UserId}: {Msg}",
                        userPersonaId, userResult[0].Value);
                    await _settingService.UpdateProductStatusAsync(
                        userPersonaId, ProductStatusSettingType, ProductId,
                        (int)ProductBatchStatusType.Error, cancellationToken);
                    return (userResult[0].Value, auditParams);
                }

                // Parse the response and persist SAML attributes
                foreach (var item in userResult)
                {
                    if (item.Name.ToUpperInvariant() != "SYSTEMIDENTIFIER") continue;

                    string pmcUserLogin = item.Value;
                    await _samlRepository.CreateSamlUserAttributeAsync(
                        userPersonaId, ProductId, SamlAttributeEnum.UserId, pmcUserLogin, cancellationToken);
                    await _samlRepository.CreateSamlUserAttributeAsync(
                        userPersonaId, ProductId, SamlAttributeEnum.productUsername, pmcUserLogin.Split('|')[1], cancellationToken);

                    _logger.LogDebug("ManageAccountingUserAsync – persisted SAML login={Login}", pmcUserLogin);

                    // Enable green-book (unified login) SSO for new user
                    var enableLoginInfo = new OneSiteAccounting.NameValuePair[]
                    {
                        new() { Name = "CompanyID",        Value = ctx.CompanyName    },
                        new() { Name = "Login",            Value = ctx.IntactLogin    },
                        new() { Name = "Password",         Value = ctx.IntactPassword },
                        new() { Name = "SystemIdentifier", Value = pmcUserLogin       }
                    };
                    string gbMessage = await Task.Run(() => _service.EnableGreenBookUser(enableLoginInfo), cancellationToken);
                    _logger.LogDebug("ManageAccountingUserAsync – EnableGreenBookUser result={Msg}", gbMessage);
                    break;
                }
            }
            else
            {
                // Existing user — handle user-type transitions
                if (batchProcessType is BatchProcessType.UserTypeAdminToRegular
                                     or BatchProcessType.UserTypeRegularToAdmin
                                     or BatchProcessType.UserTypeAdminToExternal
                                     or BatchProcessType.UserTypeExternalToAdmin)
                {
                    if (batchProcessType is BatchProcessType.UserTypeRegularToAdmin
                                         or BatchProcessType.UserTypeExternalToAdmin)
                        isAdmin = true;

                    _logger.LogDebug("ManageAccountingUserAsync – user-type change batchProcessType={Type} userPersonaId={UserId}",
                        batchProcessType, userPersonaId);
                }

                parameters.Add(new() { Name = "SystemIdentifier", Value = ctx.ProductUserId });

                if (isAdmin)
                    rolesToCarryForward.AddRange(
                        currentRoleListResponse.Records.Cast<ProductRole>()
                        .Where(r => r.IsAssigned)
                        .Select(r => r.ID));

                if (isSuperUser && roleList.Count == 0)
                {
                    hasAdminRolesToCarryOver = true;
                    adminRolesCarryForward.AddRange(
                        currentRoleListResponse.Records.Cast<ProductRole>()
                        .Where(r => r.IsAssigned && !r.Name.Equals("ADMINISTRATOR", StringComparison.OrdinalIgnoreCase))
                        .Select(r => r.ID));
                }

                var userArr = parameters.ToArray();
                _logger.LogDebug("ManageAccountingUserAsync – UpdateUser userPersonaId={UserId}", userPersonaId);

                // No async overload for UpdateUser NameValuePair[]; wrap in Task.Run.
                string updateResult = await Task.Run(() => _service.UpdateUser(userArr), cancellationToken);
                _logger.LogDebug("ManageAccountingUserAsync – UpdateUser result={Result}", updateResult);

                // Re-enable user if it was inactive
                await ChangeStatusAccountingUserAsync(editorPersonaId, userPersonaId, true, cancellationToken);
            }

            await _settingService.UpdateProductStatusAsync(
                userPersonaId, ProductStatusSettingType, ProductId,
                (int)ProductBatchStatusType.Success, cancellationToken);

            // Normalise role/property lists before assignment
            roleList     ??= [];
            propertyList ??= [];
            if (isAdmin)                 roleList = rolesToCarryForward;
            if (hasAdminRolesToCarryOver) roleList = adminRolesCarryForward;

            // Assign roles
            var (rolesError, rolesAudit) = await UpdateRolesToUserInternalAsync(
                ctx, editorPersonaId, userPersonaId, roleList,
                batchProcessType, currentRoleListResponse, cancellationToken);
            auditParams.AddRange(rolesAudit);
            if (!string.IsNullOrEmpty(rolesError)) return (rolesError, auditParams);

            // Resolve property list from company list when admin requests all
            if (isAccountingAdmin && companyList.Count > 0 &&
                propertyList.Count > 0 && propertyList[0].ToUpper() == "ALL")
            {
                propertyList = companyList;
            }

            if (!isSuperUser && !isUnRestrictedAccessToProp && propertyList.Count > 0)
            {
                var (propsError, propsAudit) = await UpdatePropertiesToUserInternalAsync(
                    ctx, editorPersonaId, userPersonaId, propertyList,
                    isAccountingAdmin, batchProcessType,
                    isUnRestrictedAccessToProp: false,
                    currentPropList, currentLocGrpList, currentEntities,
                    cancellationToken);
                auditParams.AddRange(propsAudit);
                if (!string.IsNullOrEmpty(propsError)) return (propsError, auditParams);
            }

            if (isSuperUser || isUnRestrictedAccessToProp)
            {
                var (compError, compAudit) = await AssignAllCurrentCompaniesToUserAsync(
                    editorPersonaId, userPersonaId,
                    propertyList, isAccountingAdmin, batchProcessType,
                    currentPropList, currentLocGrpList, currentEntities,
                    cancellationToken);
                auditParams.AddRange(compAudit);
                if (!string.IsNullOrEmpty(compError)) return (compError, auditParams);
            }

            if (batchProcessType is BatchProcessType.UserTypeRegularToAdmin
                                 or BatchProcessType.UserTypeAdminToRegular
                                 or BatchProcessType.UserTypeAdminToExternal
                                 or BatchProcessType.UserTypeExternalToAdmin)
            {
                _logger.LogInformation(
                    "ManageAccountingUserAsync – user-type change logged for editorPersonaId={EditorId} batchProcessType={Type}",
                    editorPersonaId, batchProcessType);
            }

            return (string.Empty, auditParams);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "ManageAccountingUserAsync – failed for editorPersonaId={EditorId} userPersonaId={UserId}",
                editorPersonaId, userPersonaId);
            return ($"Error - {ex.Message}", auditParams);
        }
    }

    // thread-safe incrementor for login name uniqueness loop
    private int _loginNameIncrementor;

    // ── UpdateAccountingUserProfileAsync ──────────────────────────────────

    /// <inheritdoc/>
    public async Task<string> UpdateAccountingUserProfileAsync(
        long editorPersonaId, long userPersonaId,
        CancellationToken cancellationToken = default)
    {
        var (ctx, ctxError) = await GetAccountingContextAsync(editorPersonaId, userPersonaId, cancellationToken);
        if (ctxError is not null) return ctxError.ErrorReason!;

        if (string.IsNullOrEmpty(ctx!.ProductUserId))
            return string.Empty; // user not yet provisioned — nothing to update

        try
        {
            var userPersona = await _managePersona.GetPersonaAsync(userPersonaId, false, cancellationToken);
            var person      = await _managePerson.GetPersonAsync(userPersona.RealPageId, cancellationToken);
            var userLogin   = await _manageUserLogin.GetUserLoginOnlyAsync(userPersona.RealPageId, cancellationToken);

            string supervisorId = await GetSupervisorUserDetailsAsync(
                userPersona.UserId, userPersona.OrganizationPartyId,
                ctx.EditorPersona.Organization.PartyId, cancellationToken);

            string userEmailAddress = await ResolveEmailAddressAsync(
                userLogin.RealPageId, userLogin.LoginName, cancellationToken);
            userEmailAddress = ValidateAndReturnEmail(userEmailAddress);

            string firstName = person.FirstName.Substring(0, Math.Min(person.FirstName.Length, 40));
            string lastName  = person.LastName .Substring(0, Math.Min(person.LastName .Length, 40));

            var parameters = new List<OneSiteAccounting.NameValuePair>
            {
                new() { Name = "CompanyID",       Value = ctx.CompanyName       },
                new() { Name = "Login",           Value = ctx.IntactLogin       },
                new() { Name = "Password",        Value = ctx.IntactPassword    },
                new() { Name = "LoginId",         Value = ctx.ProductUsername   },
                new() { Name = "FirstName",       Value = firstName             },
                new() { Name = "LastName",        Value = lastName              },
                new() { Name = "Email",           Value = userEmailAddress      },
                new() { Name = "Description",     Value = $"{firstName} {lastName}" },
                new() { Name = "SupervisorUserId",Value = supervisorId          },
                new() { Name = "SystemIdentifier",Value = ctx.ProductUserId     }
            };

            _logger.LogDebug("UpdateAccountingUserProfileAsync – UpdateUserDetails userPersonaId={UserId}", userPersonaId);

            // No async overload for UpdateUserDetails NameValuePair[]; wrap in Task.Run.
            string result = await Task.Run(() => _service.UpdateUserDetails(parameters.ToArray()), cancellationToken);

            if (result.Trim().ToUpper().Contains("SUCCESSFULLY"))
            {
                await _settingService.UpdateProductStatusAsync(
                    userPersonaId, ProductStatusSettingType, ProductId,
                    (int)ProductBatchStatusType.Success, cancellationToken);
                _logger.LogDebug("UpdateAccountingUserProfileAsync – success for userPersonaId={UserId}", userPersonaId);
                return string.Empty;
            }

            _logger.LogError("UpdateAccountingUserProfileAsync – failed for userPersonaId={UserId}: {Result}",
                userPersonaId, result);
            return "Update Profile failed. " + result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UpdateAccountingUserProfileAsync – failed for userPersonaId={UserId}", userPersonaId);
            return $"Error - {ex.Message}";
        }
    }

    // ── ChangeAccountingServiceUserTypeAsync ──────────────────────────────

    /// <inheritdoc/>
    public Task<(string error, List<AdditionalParameters> auditParams)> ChangeAccountingServiceUserTypeAsync(
        long editorPersonaId, long userPersonaId,
        List<string> roleList, List<string> propertyList, List<string> companyList,
        bool isAccountingAdmin, bool isSiteSpendManagementUser, bool isUnRestrictedAccessToProp,
        BatchProcessType batchProcessType,
        CancellationToken cancellationToken = default)
        => ManageAccountingUserAsync(
            editorPersonaId, userPersonaId,
            roleList, propertyList, companyList,
            isAccountingAdmin, isSiteSpendManagementUser, isUnRestrictedAccessToProp,
            batchProcessType, cancellationToken);

    // ── UnassignUserAsync ──────────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<string> UnassignUserAsync(
        long editorPersonaId, long userPersonaId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            string result = await ChangeStatusAccountingUserAsync(
                editorPersonaId, userPersonaId, false, cancellationToken);

            if (result.Trim().ToUpper().Contains("INACTIVATED"))
            {
                await _settingService.UpdateProductStatusAsync(
                    userPersonaId, ProductStatusSettingType, ProductId,
                    (int)ProductBatchStatusType.Deleted, cancellationToken);
                _logger.LogDebug("UnassignUserAsync – success for userPersonaId={UserId}", userPersonaId);
                return string.Empty;
            }

            _logger.LogError("UnassignUserAsync – failed for userPersonaId={UserId}: {Result}",
                userPersonaId, result);
            return "Unassign failed. " + result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UnassignUserAsync – failed for userPersonaId={UserId}", userPersonaId);
            return $"Error - {ex.Message}";
        }
    }

    // ── DeleteAccountingUserAsync ──────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<string> DeleteAccountingUserAsync(
        long editorPersonaId, long userPersonaId,
        CancellationToken cancellationToken = default)
    {
        var (ctx, ctxError) = await GetAccountingContextAsync(editorPersonaId, userPersonaId, cancellationToken);
        if (ctxError is not null) return ctxError.ErrorReason!;

        var parameters = new OneSiteAccounting.NameValuePair[]
        {
            new() { Name = "CompanyID",        Value = ctx!.CompanyName    },
            new() { Name = "Login",            Value = ctx.IntactLogin    },
            new() { Name = "Password",         Value = ctx.IntactPassword },
            new() { Name = "SystemIdentifier", Value = ctx.ProductUserId  }
        };

        try
        {
            _logger.LogDebug("DeleteAccountingUserAsync – deleting userPersonaId={UserId}", userPersonaId);

            // No async overload for DeleteUser NameValuePair[]; wrap in Task.Run.
            await Task.Run(() => _service.DeleteUser(parameters), cancellationToken);

            await _settingService.UpdateProductStatusAsync(
                userPersonaId, ProductStatusSettingType, ProductId,
                (int)ProductBatchStatusType.Deleted, cancellationToken);
            return string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DeleteAccountingUserAsync – failed for userPersonaId={UserId}", userPersonaId);
            return "There was a problem deleting the user";
        }
    }

    // ── Private orchestration helpers ──────────────────────────────────────

    private async Task<(string error, List<AdditionalParameters> auditParams)> UpdateRolesToUserInternalAsync(
        AccountingCtx ctx,
        long editorPersonaId, long userPersonaId,
        List<string> rolesToAssign,
        BatchProcessType batchProcessType,
        ListResponse? providedRoleList,
        CancellationToken ct)
    {
        var auditParams = new List<AdditionalParameters>();
        if (string.IsNullOrEmpty(ctx.ProductUserId))
        {
            _logger.LogDebug("UpdateRolesToUserInternalAsync – missing product user for userPersonaId={UserId}", userPersonaId);
            return ("Missing product user", auditParams);
        }

        var datafilter      = new RequestParameter();
        var currentRoleList = providedRoleList
            ?? await GetUserRolesAsync(editorPersonaId, userPersonaId, datafilter, ct);
        bool isSuperUser = false;
        if (ctx is not null)
        {
            var userPersona = await _managePersona.GetPersonaAsync(userPersonaId, false, ct);
            isSuperUser = await _contextService.IsSuperUserAsync(userPersona, ct);
        }

        string roleIDAddList    = "";
        string roleIDRemoveList = "";
        var    rolesToRemove    = new List<string>();

        if (batchProcessType is BatchProcessType.UserTypeAdminToRegular
                             or BatchProcessType.UserTypeRegularToAdmin
                             or BatchProcessType.UserTypeAdminToExternal
                             or BatchProcessType.UserTypeExternalToAdmin)
        {
            if (batchProcessType is BatchProcessType.UserTypeRegularToAdmin
                                 or BatchProcessType.UserTypeExternalToAdmin)
            {
                // Add all roles including ADMINISTRATOR
                foreach (ProductRole role in currentRoleList.Records.Cast<ProductRole>())
                {
                    if ((role.Name.Equals("ADMINISTRATOR", StringComparison.OrdinalIgnoreCase) && !role.IsAssigned)
                        || role.IsAssigned)
                        rolesToAssign.Add(role.ID);
                }
                if (rolesToAssign.Count > 0)
                    roleIDAddList = string.Join(",", rolesToAssign);
            }
            else // AdminToRegular or AdminToExternal
            {
                // Remove ADMINISTRATOR role
                foreach (ProductRole role in currentRoleList.Records.Cast<ProductRole>())
                {
                    if (role.Name.Equals("ADMINISTRATOR", StringComparison.OrdinalIgnoreCase) && role.IsAssigned)
                        rolesToRemove.Add(role.ID);
                }
                if (rolesToRemove.Count > 0) roleIDRemoveList = string.Join(",", rolesToRemove);
                if (rolesToAssign.Count > 0)  roleIDAddList    = string.Join(",", rolesToAssign);
            }
        }
        else
        {
            if (isSuperUser && string.IsNullOrEmpty(ctx!.ProductUserId))
            {
                foreach (ProductRole role in currentRoleList.Records.Cast<ProductRole>())
                {
                    if (role.Name.Equals("ADMINISTRATOR", StringComparison.OrdinalIgnoreCase) && !role.IsAssigned)
                        rolesToAssign.Add(role.ID);
                }
            }
            else
            {
                bool isSuperExistsInProduct = isSuperUser && !string.IsNullOrEmpty(ctx!.ProductUserId);
                foreach (ProductRole role in currentRoleList.Records.Cast<ProductRole>())
                {
                    if (!rolesToAssign.Contains(role.ID) && role.IsAssigned)
                        rolesToRemove.Add(role.ID);
                    else if (rolesToAssign.Contains(role.ID) && role.IsAssigned)
                        rolesToAssign.Remove(role.ID);

                    if (role.Name.Equals("ADMINISTRATOR", StringComparison.OrdinalIgnoreCase) && isSuperExistsInProduct)
                    {
                        rolesToRemove.Remove(role.ID);
                        if (!role.IsAssigned) rolesToAssign.Add(role.ID);
                    }
                }
            }
            if (rolesToAssign.Count > 0) roleIDAddList    = string.Join(",", rolesToAssign);
            if (rolesToRemove.Count > 0) roleIDRemoveList = string.Join(",", rolesToRemove);
        }

        var user = new OneSiteAccounting.NameValuePair[5]
        {
            new() { Name = "CompanyID",        Value = ctx!.CompanyName    },
            new() { Name = "Login",            Value = ctx.IntactLogin    },
            new() { Name = "Password",         Value = ctx.IntactPassword },
            new() { Name = "SystemIdentifier", Value = ctx.ProductUserId  },
            new() { Name = "replace",          Value = string.Empty       }
        };

        string assignSuccessful = "";
        try
        {
            if (!string.IsNullOrWhiteSpace(roleIDRemoveList))
            {
                user[4] = new() { Name = "RoleIdsToRemove", Value = roleIDRemoveList };
                string removeResult = await Task.Run(() => _service.RemoveRolesFromUser(user), ct);
                if (!removeResult.ToUpper().Contains("REMOVED PROVIDED ROLES SUCCESSFULLY"))
                    return (assignSuccessful += "Failed to remove. " + removeResult, auditParams);
                assignSuccessful = string.Empty;
            }

            if (!string.IsNullOrWhiteSpace(roleIDAddList))
            {
                user[4] = new() { Name = "RoleIdsToAdd", Value = roleIDAddList };
                string addResult = await Task.Run(() => _service.AssignRolesToUser(user), ct);
                if (!addResult.ToUpper().Contains("PROVIDED USER ROLES ADDED SUCCESSFULLY"))
                    return (assignSuccessful += "Failed to assign. " + addResult, auditParams);
                assignSuccessful = string.Empty;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UpdateRolesToUserInternalAsync – SOAP failed for userPersonaId={UserId}", userPersonaId);
            return ("An error occurred. " + ex.Message, auditParams);
        }

        // Build audit entries: compare before/after role assignments
        var updatedRoleList = await GetUserRolesAsync(editorPersonaId, userPersonaId, datafilter, ct);
        var oldRoles = currentRoleList.Records.Cast<ProductRole>().Where(r => r.IsAssigned).Select(r => r.ID).ToList();
        var newRoles = updatedRoleList .Records.Cast<ProductRole>().Where(r => r.IsAssigned).Select(r => r.ID).ToList();

        var assigned = newRoles.Except(oldRoles).ToList();
        var removed  = oldRoles.Except(newRoles).ToList();

        auditParams.AddRange(currentRoleList.Records.Cast<ProductRole>()
            .Where(r => assigned.Contains(r.ID))
            .Select(r => new AdditionalParameters { Key = "Financial Suite Roles", Value = RoleAssignMessage.Replace("RoleName", r.Name) }));
        auditParams.AddRange(currentRoleList.Records.Cast<ProductRole>()
            .Where(r => removed.Contains(r.ID))
            .Select(r => new AdditionalParameters { Key = "Financial Suite Roles", Value = RoleRemovedMessage.Replace("RoleName", r.Name) }));

        return (assignSuccessful, auditParams);
    }

    private async Task<(string error, List<AdditionalParameters> auditParams)> UpdatePropertiesToUserInternalAsync(
        AccountingCtx ctx,
        long editorPersonaId, long userPersonaId,
        List<string> propertiesToAssign,
        bool isAccountingAdmin,
        BatchProcessType batchProcessType,
        bool isUnRestrictedAccessToProp,
        List<ACProperty>? beforeUpdatePropList,
        List<ProductPropertyGroup>? beforeUpdateLocGrpList,
        List<ACProperty>? beforeUpdateEntitiesList,
        CancellationToken ct)
    {
        var auditParams = new List<AdditionalParameters>();
        if (string.IsNullOrEmpty(ctx.ProductUserId))
            return ("Missing product user", auditParams);

        var datafilter        = new RequestParameter();
        var currentPropList   = beforeUpdatePropList   ?? await GetAllCompanyPropertiesAsync(ctx, ct) ?? [];
        var currentLocGrpList = beforeUpdateLocGrpList ?? await GetAllPropertyGroupsAsync(ctx, ct) ?? [];
        var entitiesResp      = beforeUpdateEntitiesList is not null
            ? new ListResponse { Records = beforeUpdateEntitiesList.Cast<object>().ToList() }
            : await GetUserPropertiesNewAsync(editorPersonaId, userPersonaId, datafilter, ct);
        var currentEntities   = entitiesResp?.Records?.Cast<ACProperty>().ToList() ?? [];
        bool isSuperUser      = await _contextService.IsSuperUserAsync(
            await _managePersona.GetPersonaAsync(userPersonaId, false, ct), ct);

        bool   isMConsolePMC        = currentPropList.Count(p => !string.IsNullOrWhiteSpace(p.MConsoleId)) > 0;
        string propertyIDAddList    = "All";
        string propertyIDRemoveList = "";
        var    propertiesToRemove   = new List<string>();

        if (batchProcessType is BatchProcessType.UserTypeAdminToRegular
                             or BatchProcessType.UserTypeRegularToAdmin
                             or BatchProcessType.UserTypeAdminToExternal
                             or BatchProcessType.UserTypeExternalToAdmin)
        {
            if (batchProcessType is BatchProcessType.UserTypeRegularToAdmin
                                 or BatchProcessType.UserTypeExternalToAdmin)
            {
                propertyIDRemoveList = "";
                isMConsolePMC = currentPropList.Count(p => !string.IsNullOrWhiteSpace(p.MConsoleId)) > 0;

                foreach (var prop in currentPropList.Where(p => p.IsAssigned))
                    propertiesToRemove.Add(string.IsNullOrEmpty(prop.MConsoleId) ? prop.PropertyId : prop.MConsoleId);

                foreach (var propLG in currentLocGrpList.Where(p => p.IsAssigned == true))
                    propertiesToRemove.Add(propLG.ID);

                if (propertiesToRemove.Count > 0)
                    propertyIDRemoveList = string.Join(",", propertiesToRemove);
                propertyIDAddList = "All";
            }
            else // AdminToRegular or AdminToExternal
            {
                propertyIDAddList = propertiesToAssign.Count > 0
                    ? string.Join(",", propertiesToAssign)
                    : "";
            }
        }
        else
        {
            if (!isSuperUser && propertiesToAssign.Count > 0 &&
                propertiesToAssign[0].ToUpper() != "ALL")
            {
                propertyIDAddList = "";
                isMConsolePMC = currentPropList.Count(p => !string.IsNullOrWhiteSpace(p.MConsoleId)) > 0;

                foreach (var prop in currentPropList)
                {
                    if (string.IsNullOrEmpty(prop.PropertyId))
                    {
                        // Company-level entry
                        if (!propertiesToAssign.Contains(prop.CompanyId) && prop.IsAssigned)
                            propertiesToRemove.Add(string.IsNullOrEmpty(prop.MConsoleId) ? prop.PropertyId : prop.MConsoleId);
                    }
                    else
                    {
                        if (!propertiesToAssign.Contains(prop.PropertyId) && prop.IsAssigned)
                            propertiesToRemove.Add(string.IsNullOrEmpty(prop.MConsoleId) ? prop.PropertyId : prop.MConsoleId);
                        else if (propertiesToAssign.Contains(prop.PropertyId) && prop.IsAssigned)
                            propertiesToAssign.Remove(string.IsNullOrEmpty(prop.MConsoleId) ? prop.PropertyId : prop.MConsoleId);
                    }
                }

                foreach (var propLG in currentLocGrpList)
                {
                    if (propLG.IsAssigned == true)
                    {
                        if (!propertiesToAssign.Contains(propLG.ID))
                            propertiesToRemove.Add(propLG.ID);
                        else
                            propertiesToAssign.Remove(propLG.ID);
                    }
                }

                if (propertiesToAssign.Count > 0) propertyIDAddList    = string.Join(",", propertiesToAssign);
                if (propertiesToRemove.Count > 0) propertyIDRemoveList = string.Join(",", propertiesToRemove);
            }
        }

        if (isSuperUser)
        {
            if (propertiesToAssign.Count > 0 && propertiesToAssign[0].ToUpper() != "ALL")
                propertyIDAddList = string.Join(",", propertiesToAssign);

            if (batchProcessType is not BatchProcessType.UserTypeRegularToAdmin
                                and not BatchProcessType.UserTypeExternalToAdmin)
                propertyIDRemoveList = "";
        }

        var user = new OneSiteAccounting.NameValuePair[5]
        {
            new() { Name = "CompanyID",        Value = ctx.CompanyName    },
            new() { Name = "Login",            Value = ctx.IntactLogin    },
            new() { Name = "Password",         Value = ctx.IntactPassword },
            new() { Name = "SystemIdentifier", Value = ctx.ProductUserId  },
            new() { Name = "replace",          Value = string.Empty       }
        };

        string assignSuccessful = "";
        try
        {
            if (!string.IsNullOrWhiteSpace(propertyIDRemoveList))
            {
                user[4] = new() { Name = "PropertyIdsToRemove", Value = propertyIDRemoveList };
                string removeResult = await Task.Run(() => _service.RemovePropertiesFromUser(user), ct);
                if (removeResult != null &&
                    !removeResult.ToUpper().Contains("PROVIDED USER PROPERTIES REMOVED SUCCESSFULLY") &&
                    !removeResult.ToUpper().Contains("PROVIDED USER PROPERTIES DELETED SUCCESSFULLY"))
                    return (assignSuccessful += "Failed to remove. " + removeResult, auditParams);
                assignSuccessful = string.Empty;
            }

            // Re-check MConsole after possible mutation
            var latestPropList = await GetAllCompanyPropertiesAsync(ctx, ct);
            isMConsolePMC = latestPropList?.Count(p => !string.IsNullOrWhiteSpace(p.MConsoleId)) > 0;

            if (!string.IsNullOrWhiteSpace(propertyIDAddList) && (isMConsolePMC || !isUnRestrictedAccessToProp))
            {
                user[4] = new() { Name = "PropertyIdsToAdd", Value = propertyIDAddList };
                string addResult = await Task.Run(() => _service.AssignPropertiesToUser(user), ct);
                if (addResult != null && !addResult.ToUpper().Contains("PROVIDED USER PROPERTIES ADDED SUCCESSFULLY"))
                    return (assignSuccessful += "Failed to assign. " + addResult, auditParams);
                assignSuccessful = string.Empty;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UpdatePropertiesToUserInternalAsync – SOAP failed for userPersonaId={UserId}", userPersonaId);
            return ("An error occurred. " + ex.Message, auditParams);
        }

        auditParams.AddRange(await GetPropertiesAdditionalParametersAsync(
            ctx, editorPersonaId, userPersonaId, datafilter,
            currentPropList, currentLocGrpList, currentEntities,
            isMConsolePMC, ct));

        return (assignSuccessful, auditParams);
    }

    // ── Private SOAP helpers ───────────────────────────────────────────────

    /// <summary>Returns the Accounting SOAP user details for the context's product user.</summary>
    private async Task<OneSiteAccounting.NameValuePair[]?> GetUserAsync(
        AccountingCtx ctx, CancellationToken ct)
    {
        var loginInfo = BuildLoginInfo(ctx);
        var user = new OneSiteAccounting.User[] { new() { NameValuePair = loginInfo } };
        try
        {
            // No async overload for GetUser NameValuePair[]; wrap in Task.Run.
            return await Task.Run(() => _service.GetUser(user), ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetUserAsync – SOAP failed for productUserId={UserId}", ctx.ProductUserId);
            return null;
        }
    }

    /// <summary>Returns all company-level properties without user-specific assignment.</summary>
    private async Task<List<ACProperty>?> GetAllCompanyPropertiesAsync(
        AccountingCtx ctx, CancellationToken ct)
    {
        var loginInfo = BuildEditorLoginInfo(ctx);
        var comp = new OneSiteAccounting.Company[] { new() { NameValuePair = loginInfo } };
        try
        {
            // No async overload for getPropertiesAPI; wrap in Task.Run.
            var entities = await Task.Run(() => _service.getPropertiesAPI(comp), ct);
            return entities?.ToGBEnteties();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetAllCompanyPropertiesAsync – SOAP failed");
            return null;
        }
    }

    /// <summary>Returns all company-level property groups without user-specific assignment.</summary>
    private async Task<List<ProductPropertyGroup>?> GetAllPropertyGroupsAsync(
        AccountingCtx ctx, CancellationToken ct)
    {
        var wsParams  = ManageProductOneSiteAccountingHelpers.GenerateSearchAndPaging(null, "Name", 0, 9999);
        var loginInfo = BuildLoginInfo(ctx);
        var prop = new OneSiteAccounting.Property[] { new() { NameValuePair = loginInfo } };
        try
        {
            OneSiteAccounting.TotalRows[] results2 = Array.Empty<OneSiteAccounting.TotalRows>();
            var location = await Task.Run(
                () => _service.GetAllPropertyGroups(prop, wsParams, out results2), ct);
            return location?.ToGBPropertyGroup();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetAllPropertyGroupsAsync – SOAP failed");
            return null;
        }
    }

    /// <summary>Returns company-level company list for the context.</summary>
    private async Task<List<ACCompany>> GetUserCompaniesDetailsAsync(
        AccountingCtx ctx, CancellationToken ct)
    {
        var loginInfo = BuildEditorLoginInfo(ctx);
        var comp = new OneSiteAccounting.Company[] { new() { NameValuePair = loginInfo } };
        try
        {
            // No async overload for getCompaniesAPI; wrap in Task.Run.
            var company = await Task.Run(() => _service.getCompaniesAPI(comp), ct);
            return company?.ToGBCompanies() ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetUserCompaniesDetailsAsync – SOAP failed");
            return [];
        }
    }

    /// <summary>
    /// Returns <c>true</c> if the given login name is already in use in Accounting.
    /// Used by the new-user login-name uniqueness loop in <see cref="ManageAccountingUserAsync"/>.
    /// </summary>
    private async Task<bool> CheckIfUserLoginIsUsedAsync(
        AccountingCtx ctx, string userLogin, CancellationToken ct)
    {
        var loginInfo = new OneSiteAccounting.NameValuePair[]
        {
            new() { Name = "CompanyID", Value = ctx.CompanyName    },
            new() { Name = "Login",     Value = ctx.IntactLogin    },
            new() { Name = "Password",  Value = ctx.IntactPassword },
            new() { Name = "UserID",    Value = userLogin          }
        };

        try
        {
            // No async overload for CheckIfUserIDIsUsed; wrap in Task.Run.
            string result = await Task.Run(() => _service.CheckIfUserIDIsUsed(loginInfo), ct);
            return result.ToUpper() == "YES";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CheckIfUserLoginIsUsedAsync – SOAP failed for login={Login}", userLogin);
            throw;
        }
    }

    /// <summary>
    /// Computes the "access to all current and future properties" flag by comparing
    /// the company, property, and property-group selections.
    /// </summary>
    private async Task<bool> ComputeFlagBasedOnCompanyAndPropertySelectedAsync(
        AccountingCtx ctx,
        long editorPersonaId, long userPersonaId,
        RequestParameter datafilter,
        CancellationToken ct)
    {
        bool flag = false;

        var companyList  = await GetUserCompaniesDetailsAsync(ctx, ct);
        var propResp     = await GetUserPropertiesNewAsync(editorPersonaId, userPersonaId, datafilter, ct);
        var propGrpResp  = await GetUserPropertyGroupsAsync(editorPersonaId, userPersonaId, datafilter, ct);

        int totalCompanies          = companyList.Count;
        int totalCompaniesSelected  = companyList.Count(c => c.isAssigned);
        int totalProperties         = propResp.Records.Count;
        int totalPropertiesUnselected = propResp.Records.Count(p => ((ACProperty)p).IsAssigned == false);
        int totalPropertiesSelected   = propResp.Records.Count(p => ((ACProperty)p).IsAssigned == true);

        if (totalCompanies == totalCompaniesSelected && totalProperties == totalPropertiesUnselected)
            flag = true;

        int totalPropGroups = propGrpResp.Records.Count;
        var selectedGroups  = propGrpResp.Records.Where(p => ((ProductPropertyGroup)p).IsAssigned == true).ToList();

        if (totalPropGroups > 0 && selectedGroups.Count > 0 && totalPropertiesSelected == 0)
        {
            var selectedLocEntities = selectedGroups
                .SelectMany(g => ((ProductPropertyGroup)g).AssignedProperties)
                .ToList();

            if (propResp.Records.Count > 0)
                flag = propResp.Records.Any(x => selectedLocEntities.Contains(((ACProperty)x).PropertyName));
        }

        return flag;
    }

    /// <summary>
    /// Builds property-delta audit entries after an UpdatePropertiesToUser SOAP operation.
    /// </summary>
    private async Task<List<AdditionalParameters>> GetPropertiesAdditionalParametersAsync(
        AccountingCtx ctx,
        long editorPersonaId, long userPersonaId,
        RequestParameter datafilter,
        List<ACProperty> currentPropList,
        List<ProductPropertyGroup> currentLocGrpList,
        List<ACProperty> currentEntities,
        bool isMConsolePMC,
        CancellationToken ct)
    {
        var logs = new List<AdditionalParameters>();
        try
        {
            var updatedPropList   = await GetAllCompanyPropertiesAsync(ctx, ct) ?? [];
            var updatedLocGrpList = await GetAllPropertyGroupsAsync(ctx, ct) ?? [];
            var updatedEntities   = (await GetUserPropertiesNewAsync(editorPersonaId, userPersonaId, datafilter, ct))
                ?.Records?.Cast<ACProperty>().ToList() ?? [];

            List<string> oldProps, newProps;
            if (isMConsolePMC)
            {
                oldProps = currentPropList .Where(p => p.IsAssigned).Select(p => p.MConsoleId).ToList();
                newProps = updatedPropList.Where(p => p.IsAssigned).Select(p => p.MConsoleId).ToList();
            }
            else
            {
                oldProps = currentPropList .Where(p => p.IsAssigned).Select(p => p.PropertyId)
                           .Concat(currentLocGrpList.Where(p => p.IsAssigned == true).Select(p => p.ID)).ToList();
                newProps = updatedPropList.Where(p => p.IsAssigned).Select(p => p.PropertyId)
                           .Concat(updatedLocGrpList.Where(p => p.IsAssigned == true).Select(p => p.ID)).ToList();
            }

            var toAssign = newProps.Except(oldProps).ToList();
            var toRemove = oldProps.Except(newProps).ToList();

            if (toAssign.Count > 0)
            {
                if (isMConsolePMC)
                {
                    logs.AddRange(toAssign.Where(s => !s.Contains("|")).Distinct()
                        .Select(d => new AdditionalParameters
                            { Key = "Financial Suite Companies", Value = PropAssignMessage.Replace("PropertyName", d) }));
                    logs.AddRange(currentEntities.Where(f => toAssign.Contains(f.MConsoleId))
                        .Select(f => new AdditionalParameters
                            { Key = "Financial Suite Entities", Value = PropAssignMessage.Replace("PropertyName", f.MConsoleId) }));
                }
                else
                {
                    logs.AddRange(currentLocGrpList.Where(f => toAssign.Contains(f.ID))
                        .Select(f => new AdditionalParameters
                            { Key = "Financial Suite Location Groups", Value = PropAssignMessage.Replace("PropertyName", f.Name) }));
                    logs.AddRange(currentEntities.Where(f => toAssign.Contains(f.PropertyId))
                        .Select(f => new AdditionalParameters
                            { Key = "Financial Suite Entities", Value = PropAssignMessage.Replace("PropertyName", f.PropertyName) }));
                }
            }

            if (toRemove.Count > 0)
            {
                if (isMConsolePMC)
                {
                    logs.AddRange(toRemove.Where(s => !s.Contains("|")).Distinct()
                        .Select(d => new AdditionalParameters
                            { Key = "Financial Suite Companies", Value = PropRemovedMessage.Replace("PropertyName", d) }));
                    logs.AddRange(currentEntities.Where(f => toRemove.Contains(f.MConsoleId))
                        .Select(f => new AdditionalParameters
                            { Key = "Financial Suite Entities", Value = PropRemovedMessage.Replace("PropertyName", f.MConsoleId) }));
                }
                else
                {
                    logs.AddRange(currentLocGrpList.Where(f => toRemove.Contains(f.ID))
                        .Select(f => new AdditionalParameters
                            { Key = "Financial Suite Location Groups", Value = PropRemovedMessage.Replace("PropertyName", f.Name) }));
                    logs.AddRange(currentEntities.Where(f => toRemove.Contains(f.PropertyId))
                        .Select(f => new AdditionalParameters
                            { Key = "Financial Suite Entities", Value = PropRemovedMessage.Replace("PropertyName", f.PropertyName) }));
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetPropertiesAdditionalParametersAsync – failed to build audit trail");
        }
        return logs;
    }

    /// <summary>
    /// Resolves the Accounting supervisor product-username for the given user.
    /// Returns an empty string when no supervisor or no product login is found.
    /// </summary>
    private async Task<string> GetSupervisorUserDetailsAsync(
        long userId, long orgPartyId, long editorOrgPartyId,
        CancellationToken ct)
    {
        try
        {
            var supervisorInfo = await _userRepository.GetSuperVisorInformationAsync(userId, orgPartyId, ct);
            if (supervisorInfo is null || supervisorInfo.SuperVisorUserId <= 0)
                return string.Empty;

            var supervisorLogin = await _manageUserLogin.GetUserLoginOnlyAsync(
                supervisorInfo.SuperVisorUserId, ct);
            if (supervisorLogin is null) return string.Empty;

            var personaList = await _managePersona.ListActivePersonaAsync(
                supervisorLogin.RealPageId, false, ct);
            long supervisorPersonaId = personaList
                .Where(p => p.Organization.PartyId == editorOrgPartyId)
                .Select(p => p.PersonaId)
                .FirstOrDefault();
            if (supervisorPersonaId == 0) return string.Empty;

            var productAttrs = await _samlRepository.GetProductSamlDetailsAsync(supervisorPersonaId, ProductId, ct);
            return productAttrs
                ?.FirstOrDefault(a => a.SamlAttributeId == (int)SamlAttributeEnum.productUsername)
                ?.Value ?? string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetSupervisorUserDetailsAsync – failed for userId={UserId}", userId);
            return string.Empty;
        }
    }

    /// <summary>
    /// Resolves the primary email address for the given user, falling back to their login name.
    /// </summary>
    private async Task<string> ResolveEmailAddressAsync(
        Guid realPageId, string loginNameFallback, CancellationToken ct)
    {
        var addresses = await _manageElectronicAddress.ListElectronicAddressForPersonAsync(realPageId, null, ct);

        var primary = addresses.FirstOrDefault(a =>
            a.AddressType?.ToUpper() == "EMAIL" &&
            a.contactMechanismUsageType?.Name.ToUpper() == "PRIMARY" &&
            !string.IsNullOrEmpty(a.AddressString));
        if (primary is not null) return primary.AddressString;

        var emailType = addresses.FirstOrDefault(a =>
            a.AddressType?.ToUpper() == "EMAIL" &&
            a.contactMechanismUsageType?.Name.ToUpper() == "EMAIL" &&
            !string.IsNullOrEmpty(a.AddressString));
        if (emailType is not null) return emailType.AddressString;

        return loginNameFallback;
    }

    // ── NameValuePair array builders ───────────────────────────────────────

    /// <summary>
    /// Builds a <see cref="OneSiteAccounting.NameValuePair"/> array with
    /// CompanyID, Login, Password, and (when present) SystemIdentifier.
    /// </summary>
    private static OneSiteAccounting.NameValuePair[] BuildLoginInfo(AccountingCtx ctx)
    {
        var list = new List<OneSiteAccounting.NameValuePair>
        {
            new() { Name = "CompanyID", Value = ctx.CompanyName    },
            new() { Name = "Login",     Value = ctx.IntactLogin    },
            new() { Name = "Password",  Value = ctx.IntactPassword }
        };
        if (!string.IsNullOrEmpty(ctx.ProductUserId))
            list.Add(new() { Name = "SystemIdentifier", Value = ctx.ProductUserId });
        return list.ToArray();
    }

    /// <summary>
    /// Builds a login array that omits SystemIdentifier (editor-level company operations).
    /// </summary>
    private static OneSiteAccounting.NameValuePair[] BuildEditorLoginInfo(AccountingCtx ctx)
        => new OneSiteAccounting.NameValuePair[]
        {
            new() { Name = "CompanyID", Value = ctx.CompanyName    },
            new() { Name = "Login",     Value = ctx.IntactLogin    },
            new() { Name = "Password",  Value = ctx.IntactPassword }
        };

    // ── Private static helpers ─────────────────────────────────────────────

    /// <summary>
    /// Strips or replaces characters that Accounting rejects in login names,
    /// and guards against reserved system names.
    /// </summary>
    private static string RemoveSpecialCharacter(string accountingLoginName)
    {
        switch (accountingLoginName)
        {
            case "portluser":
            case "realpage":
            case "CPAUser":
            case "ExtUser":
            case "SvcUser":
            case "Services":
            case "CNS_":
                accountingLoginName = $"{accountingLoginName}-1";
                break;
        }

        accountingLoginName = Regex.Replace(accountingLoginName, @"[^\w\s\-\.]", string.Empty);

        if (accountingLoginName.Length > 80)
            accountingLoginName = accountingLoginName.Substring(0, 80);

        return accountingLoginName;
    }

    /// <summary>
    /// Ensures <paramref name="emailAddress"/> looks like a valid e-mail address.
    /// Appends <c>@bogusemail.com</c> or <c>.com</c> as needed so the Accounting SOAP
    /// endpoint does not reject the user creation request.
    /// </summary>
    private static string ValidateAndReturnEmail(string emailAddress)
    {
        if (new EmailAddressAttribute().IsValid(emailAddress))
            return emailAddress;

        try
        {
            var ma = new MailAddress(emailAddress);
            if (!ma.Host.Contains('.'))
                return ValidateAndReturnEmail(emailAddress + ".com");
        }
        catch
        {
            if (!emailAddress.Contains('@'))
                return ValidateAndReturnEmail(emailAddress + "@bogusemail.com");
        }

        return emailAddress;
    }

    /// <summary>
    /// Returns the UTF-8 decoded value of a Base64-encoded product internal setting.
    /// Falls back to the raw value if decoding fails (defensive against misconfiguration).
    /// </summary>
    private static string DecodeBase64Setting(List<ProductInternalSetting> settings, string name)
    {
        string? encoded = settings.FirstOrDefault(
            s => s.Name.Equals(name, StringComparison.OrdinalIgnoreCase))?.Value;
        if (string.IsNullOrEmpty(encoded))
            return string.Empty;

        try
        {
            return Encoding.UTF8.GetString(Convert.FromBase64String(encoded));
        }
        catch
        {
            return encoded;
        }
    }
}
