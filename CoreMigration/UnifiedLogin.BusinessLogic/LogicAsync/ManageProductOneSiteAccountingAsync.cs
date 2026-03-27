using System.Text;
using Microsoft.Extensions.Logging;
using UnifiedLogin.BusinessLogic.Logic.Product;
using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Constants;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product.Migration;
using OneSiteAccounting = UnifiedLogin.SharedObjects.Product.OneSiteAccounting;
using blueBook = UnifiedLogin.SharedObjects.BlackBook;

namespace UnifiedLogin.BusinessLogic.LogicAsync;

/// <summary>
/// True async implementation of OneSite Accounting user-management operations.
/// Replaces the stepping-stone wrapper that required a <see cref="DefaultUserClaim"/> at
/// construction time. Context is now resolved internally via
/// <see cref="IProductContextServiceAsync"/> from the supplied persona IDs.
/// </summary>
public sealed class ManageProductOneSiteAccountingAsync : IManageProductOneSiteAccountingAsync
{
    private const int    ProductId               = (int)ProductEnum.FinancialSuite;
    private const string ProductStatusSettingType = "ProductStatus";

    private readonly IProductContextServiceAsync                          _contextService;
    private readonly IProductSettingServiceAsync                          _settingService;
    private readonly IManageBlueBookAsync                                 _blueBook;
    private readonly OneSiteAccounting.IOneSiteAccountingProductService   _service;
    private readonly ILogger<ManageProductOneSiteAccountingAsync>         _logger;

    public ManageProductOneSiteAccountingAsync(
        IProductContextServiceAsync                        contextService,
        IProductSettingServiceAsync                        settingService,
        IManageBlueBookAsync                               blueBook,
        OneSiteAccounting.IOneSiteAccountingProductService service,
        ILogger<ManageProductOneSiteAccountingAsync>       logger)
    {
        ArgumentNullException.ThrowIfNull(contextService); _contextService = contextService;
        ArgumentNullException.ThrowIfNull(settingService); _settingService = settingService;
        ArgumentNullException.ThrowIfNull(blueBook);       _blueBook       = blueBook;
        ArgumentNullException.ThrowIfNull(service);        _service        = service;
        ArgumentNullException.ThrowIfNull(logger);         _logger         = logger;
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

    // ── Private helpers ────────────────────────────────────────────────────

    /// <summary>
    /// Returns the UTF-8 decoded value of a Base64-encoded product internal setting.
    /// Falls back to the raw value if decoding fails (defensive against misconfiguration).
    /// </summary>
    private static string DecodeBase64Setting(List<ProductInternalSetting> settings, string name)
    {
        string? encoded = settings.FirstOrDefault(s => s.Name.ToUpper() == name)?.Value;
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
