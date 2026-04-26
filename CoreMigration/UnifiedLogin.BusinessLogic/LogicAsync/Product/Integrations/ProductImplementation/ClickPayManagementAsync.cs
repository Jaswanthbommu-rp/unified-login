using Microsoft.Extensions.Logging;
using UnifiedLogin.BusinessLogic.CacheHelper;
using UnifiedLogin.BusinessLogic.Logic.ProductIntegration.Helpers;
using UnifiedLogin.BusinessLogic.Logic.ProductIntegration.Model;
using UnifiedLogin.BusinessLogic.Logic.ProductIntegration.Model.ClickPay;
using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects.Audit.Common;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Cache;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.LogicAsync.Product.Integrations.ProductImplementation;

/// <summary>
/// Native-async concrete implementation for the ClickPay product integration.
/// Replaces <c>ClickPayManagement</c> (sync).
/// <para>
/// <b>Key differences from the standard V1 base class:</b>
/// <list type="bullet">
///   <item>Roles are returned as a <see cref="ClickPayRoles"/> wrapper (not a plain role list).
///     <see cref="GetProductRolesAsync"/> unpacks the wrapper and resets <c>OrgsAssignedCount</c>.</item>
///   <item>Users are returned as a <see cref="ClickPayUsers"/> wrapper.
///     <see cref="GetProductUserAsync"/> returns the first entry in the list.</item>
///   <item>Organizations use company / owner / site type filtering with SiteList and LlcName enrichment.</item>
///   <item><c>UnassignUser</c> sends a <b>PUT</b> (not a DELETE) with <c>IsActive = false</c>.</item>
///   <item><c>ProductUserProfileChange</c> fetches the current user object, merges profile fields,
///     then sends a PUT (overrides base PATCH).</item>
///   <item><see cref="UpdateSamlUserAttributeAsync"/> creates missing SAML attributes rather than
///     updating existing ones (ClickPay may have no prior SAML record).</item>
///   <item>Multi-company <see cref="CreateUpdateProductUserAsync"/>: when a user already exists in the
///     product the user ID is fetched and an update is performed instead of creating a duplicate.</item>
/// </list>
/// </para>
/// </summary>
public sealed class ClickPayManagementAsync : StandardV1ProductIntegrationAsync
{
    /// <summary>
    /// Initialises a new instance. Call <see cref="StandardV1ProductIntegrationAsync.InitAsync"/>
    /// before using any public methods.
    /// </summary>
    public ClickPayManagementAsync(
        int                     productId,
        long                    editorPersonaId,
        long                    subjectPersonaId,
        IDataCollectorAsync     dataCollector,
        IProductRepositoryAsync productRepository,
        IManagePersonaAsync     managePersona,
        IManageUserLoginAsync   manageUserLogin,
        IUserClaimsAccessor     userClaimsAccessor,
        IHttpClientFactory      httpClientFactory,
        ITokenHelperAsync       tokenHelper,
        ICacheService           cacheService,
        ILoggerFactory          loggerFactory)
        : base(productId, editorPersonaId, subjectPersonaId,
               dataCollector, productRepository, managePersona, manageUserLogin,
               userClaimsAccessor, httpClientFactory, tokenHelper, cacheService, loggerFactory)
    {
    }

    // ── Overrides ──────────────────────────────────────────────────────────────

    /// <inheritdoc/>
    /// <remarks>
    /// ClickPay returns roles wrapped in a <see cref="ClickPayRoles"/> object.
    /// <c>OrgsAssignedCount</c> from the product is inaccurate and is reset to 0.
    /// When the subject user has a product account, their role–org assignments are
    /// loaded and the <c>OrgsAssignedCount</c> is derived by counting matching
    /// <c>OrganizationRole</c> entries. <c>SelectedItems</c> is populated from the
    /// live organization list (only assigned orgs).
    /// </remarks>
    public override async Task<ListResponse> GetProductRolesAsync(
        RequestParameter dataFilter,
        string? baseUrlAndQuery = null,
        CancellationToken ct = default)
    {
        try
        {
            baseUrlAndQuery = string.Format(
                GetOperationEndPoint(ProductEntityEndpointKeyEnum.GetRoleEndpoint),
                CompanyInstanceSourceId);

            var roleList = (await GetResultFromApiAsync<ClickPayRoles>(baseUrlAndQuery, ct: ct))
                           ?.ClickPayRoleList
                           ?? throw new InvalidOperationException("Null ClickPay role list from product API.");

            // Product count is inaccurate — reset before re-deriving
            foreach (var item in roleList)
            {
                item.OrgsAssignedCount = 0;
                item.IsAssigned        = false;
            }

            if (!string.IsNullOrEmpty(SubjectUserDetails?.ProductUserName))
            {
                var userRoleUrl = string.Format(
                    GetOperationEndPoint(ProductEntityEndpointKeyEnum.GetUserRoleEndpoint),
                    SubjectUserDetails.ProductUserName, CompanyInstanceSourceId);

                var user = await GetProductUserAsync(userRoleUrl, isThrowOnError: false, ct);
                if (user is not null)
                {
                    foreach (var item in roleList)
                    {
                        item.OrgsAssignedCount = user.OrganizationRoles
                            .Count(f => f.RoleId == item.Id);

                        if (item.OrgsAssignedCount > 0)
                        {
                            item.IsAssigned = true;

                            // Populate SelectedItems from live organization list
                            var orgResponse = await GetProductOrganizationsAsync(
                                item.Id, item.OrgType, ct: ct);

                            item.SelectedItems = orgResponse.Records
                                .Cast<ClickPayOrganization>()
                                .Where(x => x.IsAssigned)
                                .Select(y => new ClickPaySelectedItems { Id = y.Id, Value = y.IsAssigned })
                                .ToList();
                        }
                    }
                }
            }

            return ToListResponse(roleList);
        }
        catch (Exception ex)
        {
            LogError(ex, nameof(GetProductRolesAsync));
            return new ListResponse { IsError = true, ErrorReason = ex.Message };
        }
    }

    /// <inheritdoc/>
    /// <remarks>
    /// ClickPay organizations are filtered by type (<c>company</c> vs owner / site).
    /// For non-company types the full organization tree is fetched and enriched:
    /// <list type="bullet">
    ///   <item><c>OWNER</c> — each owner gets a <c>SiteList</c> of child sites.</item>
    ///   <item><c>SITE</c> — each site gets the parent owner <c>LlcName</c>.</item>
    /// </list>
    /// All <c>IsAssigned</c> flags are reset to <c>false</c> before merging the
    /// subject user's existing role–org assignments.
    /// </remarks>
    public override async Task<ListResponse> GetProductOrganizationsAsync(
        string organizationRoleId,
        string organizationType,
        string? baseUrlAndQuery = null,
        CancellationToken ct = default)
    {
        try
        {
            List<ClickPayOrganization> returnOrgList;

            if (string.Equals(organizationType, "company", StringComparison.OrdinalIgnoreCase))
            {
                baseUrlAndQuery = string.Format(
                    GetOperationEndPoint(ProductEntityEndpointKeyEnum.GetCompanyEndpoint), "");

                var allOrgs = (await GetResultFromApiAsync<ClickPayOrganizations>(baseUrlAndQuery, ct: ct))
                              ?.ClickPayOrganizationList
                              ?? throw new InvalidOperationException("Null Org List from product API.");

                returnOrgList = allOrgs.FindAll(x =>
                    x.Type.Equals(organizationType, StringComparison.OrdinalIgnoreCase));
            }
            else
            {
                baseUrlAndQuery = string.Format(
                    GetOperationEndPoint(ProductEntityEndpointKeyEnum.GetParentCompanyEndpoint),
                    CompanyInstanceSourceId);

                var allOrgs = (await GetResultFromApiAsync<ClickPayOrganizations>(baseUrlAndQuery, ct: ct))
                              ?.ClickPayOrganizationList
                              ?? throw new InvalidOperationException("Null Org List from product API.");

                returnOrgList = allOrgs.FindAll(x =>
                    x.Type.Equals(organizationType, StringComparison.OrdinalIgnoreCase));

                if (returnOrgList.Count > 1)
                {
                    if (organizationType.Equals("OWNER", StringComparison.OrdinalIgnoreCase))
                    {
                        foreach (var org in returnOrgList)
                        {
                            org.SiteList = allOrgs
                                .FindAll(x => string.Equals(x.ParentCompanyId, org.Id, StringComparison.OrdinalIgnoreCase)
                                           && string.Equals(x.Type, "site", StringComparison.OrdinalIgnoreCase))
                                .Select(i => new ProductProperties { SetPropertyId = i.Id, SetName = i.Name })
                                .ToList();
                        }
                    }

                    if (organizationType.Equals("SITE", StringComparison.OrdinalIgnoreCase))
                    {
                        foreach (var org in returnOrgList)
                        {
                            org.LlcName = allOrgs.Find(x =>
                                string.Equals(x.Id, org.ParentCompanyId, StringComparison.OrdinalIgnoreCase)
                                && string.Equals(x.Type, "owner", StringComparison.OrdinalIgnoreCase))?.Name;
                        }
                    }
                }
            }

            // Reset assignment state before merging
            returnOrgList.ForEach(x => x.IsAssigned = false);

            if (!string.IsNullOrEmpty(SubjectUserDetails?.ProductUserName) && returnOrgList.Count > 0)
            {
                var user = await GetProductUserAsync(ct: ct);
                if (user is not null)
                    MergeUserOrganizations(returnOrgList, user.OrganizationRoles, organizationType, organizationRoleId);
            }

            return ToListResponse(returnOrgList);
        }
        catch (Exception ex)
        {
            LogError(ex, nameof(GetProductOrganizationsAsync));
            return new ListResponse { IsError = true, ErrorReason = ex.Message };
        }
    }

    /// <inheritdoc/>
    /// <remarks>
    /// ClickPay wraps its user collection in a <see cref="ClickPayUsers"/> envelope.
    /// Returns the first item in <c>ClickPayUserList</c>, or <c>null</c> when the list is empty.
    /// </remarks>
    public override async Task<IntegrationProductUser?> GetProductUserAsync(
        string? baseUrlAndQuery = null,
        bool isThrowOnError = true,
        CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(baseUrlAndQuery))
            baseUrlAndQuery = string.Format(
                GetOperationEndPoint(ProductEntityEndpointKeyEnum.GetUserEndpoint),
                SubjectUserDetails?.ProductUserName);

        var wrapper = await GetResultFromApiAsync<ClickPayUsers>(baseUrlAndQuery, isThrowOnError, ct);
        return wrapper?.ClickPayUserList is { Count: > 0 }
            ? wrapper.ClickPayUserList[0]
            : null;
    }

    /// <inheritdoc/>
    /// <remarks>
    /// ClickPay's user endpoint uses only the login name (no company segment).
    /// </remarks>
    protected override async Task<bool> CheckUserExistInProductAsync(
        string loginNameToCheck, string? baseUrlAndQuery = null, CancellationToken ct = default)
    {
        baseUrlAndQuery = string.Format(
            GetOperationEndPoint(ProductEntityEndpointKeyEnum.GetUserEndpoint),
            loginNameToCheck);

        var user = await GetProductUserAsync(baseUrlAndQuery, isThrowOnError: false, ct);
        return user is { UserId: { Length: > 0 } };
    }

    /// <inheritdoc/>
    /// <remarks>
    /// For existing users, only assigned org–role pairs are included in the PUT payload.
    /// For super users, the "MANAGEMENT ADMIN" role is pre-populated with the company org.
    /// </remarks>
    protected override async Task<IntegrationProductUser> GenerateProductUserObjectAsync(
        ProductUserRolePropertiesGroups userRolePropertiesRegion,
        CancellationToken ct)
    {
        List<OrganizationRole> orgRoleList;

        if (!string.IsNullOrEmpty(SubjectUserDetails?.ProductUserName))
        {
            // Existing user — keep only the assigned org–role pairs
            orgRoleList = [];
            if (userRolePropertiesRegion.OrganizationRoleList is not null)
            {
                foreach (var changedOrgRole in userRolePropertiesRegion.OrganizationRoleList)
                {
                    if (!changedOrgRole.IsAssigned) continue;

                    if (changedOrgRole.RoleType.ToString().Equals("company", StringComparison.OrdinalIgnoreCase))
                        changedOrgRole.OrganizationId = CompanyInstanceSourceId;

                    orgRoleList.Add(new OrganizationRole
                    {
                        OrganizationId = changedOrgRole.OrganizationId,
                        RoleId         = changedOrgRole.RoleId
                    });
                }
            }
        }
        else
        {
            // New user — patch company org-type entries and pass all as-is
            if (userRolePropertiesRegion.OrganizationRoleList is not null)
            {
                foreach (var orgRole in userRolePropertiesRegion.OrganizationRoleList
                             .Where(r => r.IsAssigned
                                      && r.RoleType.ToString().Equals("company", StringComparison.OrdinalIgnoreCase)))
                {
                    orgRole.OrganizationId = CompanyInstanceSourceId;
                }
            }
            orgRoleList = userRolePropertiesRegion.OrganizationRoleList ?? [];
        }

        var productUser = new IntegrationProductUser
        {
            LoginName             = string.IsNullOrEmpty(SubjectUserDetails?.LoginName)
                                        ? SubjectUserDetails?.LoginName
                                        : GetUniqueProductLogin(SubjectUserDetails.LoginName),
            CompanyId             = CompanyInstanceSourceId,
            FirstName             = SubjectUserDetails?.FirstName,
            LastName              = SubjectUserDetails?.LastName,
            Email                 = SubjectUserDetails?.Email,
            Phone                 = SubjectUserDetails?.PhoneNumber,
            IsActive              = true,
            PropertyGroups        = userRolePropertiesRegion.PropertyGroupList,
            Properties            = userRolePropertiesRegion.PropertyList,
            Roles                 = userRolePropertiesRegion.RoleList?.ConvertAll(x => x.ToString()),
            PropertyRoles         = userRolePropertiesRegion.PropertyRoleList,
            OrganizationRoles     = orgRoleList,
            CanReceiveMonthlyReport = userRolePropertiesRegion.CanReceiveMonthlyReport,
            IsMigratedUser        = true
        };

        // Super-user: find "MANAGEMENT ADMIN" role and assign to company org
        if (SubjectUserDetails?.UserRoleTypeId == (int)UserRoleType.SuperUser)
        {
            var rolesResponse = await GetProductRolesAsync(null!, ct: ct);
            var adminRole = rolesResponse.Records
                .Cast<ClickPayRole>()
                .FirstOrDefault(x => x.Name.Equals("MANAGEMENT ADMIN", StringComparison.OrdinalIgnoreCase));

            productUser.OrganizationRoles =
            [
                new OrganizationRole
                {
                    OrganizationId = productUser.CompanyId,
                    RoleId         = adminRole?.Id ?? string.Empty,
                    IsAssigned     = true
                }
            ];

            await ApplySuperUserDataAsync(productUser, ct);
        }

        return productUser;
    }

    /// <inheritdoc/>
    /// <remarks>
    /// ClickPay wraps the migration user list in a <see cref="ClickPayUsers"/> envelope.
    /// </remarks>
    public override async Task<ListResponse> GetMigrationUsersAsync(
        RequestParameter dataFilter,
        CancellationToken ct = default)
    {
        var response = new ListResponse { IsError = true, ErrorReason = "No Users." };

        string filter       = dataFilter?.FilterBy?.GetValueOrDefault("filter") ?? "NonMigrated";
        int    startRow     = dataFilter?.Pages?.StartRow ?? 0;
        int    resultPerRow = dataFilter?.Pages?.ResultsPerPage ?? 1000;

        var baseUrlAndQuery = GetOperationEndPoint(ProductEntityEndpointKeyEnum.GetListUsersEndpoint);
        baseUrlAndQuery = string.Format(baseUrlAndQuery, CompanyInstanceSourceId, filter, startRow, resultPerRow);

        DumpApiCallInfoToDiagnosticLog(baseUrlAndQuery);

        var wrapper = await GetResultFromApiAsync<ClickPayUsers>(baseUrlAndQuery, isThrowOnError: false, ct);
        var result  = wrapper?.ClickPayUserList ?? [];

        response.RowsPerPage = resultPerRow;
        response.ErrorReason = string.Empty;
        response.IsError     = false;
        response.TotalPages  = 1;
        response.Records     = result.Cast<object>().ToList();
        response.TotalRows   = result.Count;
        return response;
    }

    /// <inheritdoc/>
    /// <remarks>
    /// ClickPay's unassign flow sends a <b>PUT</b> with <c>IsActive = false</c>
    /// rather than an HTTP DELETE. After a successful PUT, GreenBook status is updated
    /// to <c>AccountHidden</c> (or <c>Deactivated</c> when the org is already disabled).
    /// </remarks>
    public override async Task<string> UnassignUserAsync(CancellationToken ct = default)
    {
        LogDebug(nameof(UnassignUserAsync),
            $"Editor={EditorUserDetails.PersonaId} — calling PUT with IsActive=false");

        var clickPayUser = await GetProductUserAsync(ct: ct);
        if (clickPayUser is null)
            return "User not found in ClickPay product.";

        clickPayUser.IsActive = false;

        var result = await ClickPayPutUserAsync(clickPayUser, ct);

        if (result.IsSuccessStatusCode)
        {
            LogDebug(nameof(UnassignUserAsync), "PUT succeeded — updating GreenBook status");

            var userLogin = await _manageUserLogin.GetUserLoginOnlyAsync(SubjectUserDetails!.UserRealPageId, ct);
            var persona   = await _managePersona.GetPersonaAsync(SubjectUserDetails.PersonaId, withRights: false, ct);

            var orgStatus = await _manageUserLogin.GetUserOrganizationWithStatusAsync(
                userLogin.UserId, userLogin.LastLogin ?? DateTime.UtcNow, persona.OrganizationPartyId, getPrimaryOrg: false, ct);

            int statusValue = orgStatus.Status.ToString()
                .Equals(UserUiStatusType.Disabled.ToString(), StringComparison.OrdinalIgnoreCase)
                ? (int)UserUiStatusType.Deactivated
                : (int)UserUiStatusType.AccountHidden;

            await _dataCollector.UpdateProductSettingProductStatusAsync(
                SubjectUserDetails.PersonaId, "ProductStatus", ProductId, statusValue, ct);

            return string.Empty;
        }

        LogError(null, nameof(UnassignUserAsync),
            $"PUT failed for persona {SubjectUserDetails?.PersonaId}");
        return result.Content as string ?? "Unknown error";
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Fetches the current ClickPay user by login name, updates <c>IsActive</c>, then sends a PUT.
    /// </remarks>
    public override async Task<bool> ExternalProductUserProfileChangeAsync(
        ProductUserProfile productUserProfile,
        CancellationToken ct = default)
    {
        LogDebug(nameof(ExternalProductUserProfileChangeAsync),
            $"UserId={productUserProfile.UserId}");

        var getUserUrl = string.Format(
            GetOperationEndPoint(ProductEntityEndpointKeyEnum.GetUserEndpoint),
            productUserProfile.LoginName);

        var clickPayUser = await GetProductUserAsync(getUserUrl, isThrowOnError: true, ct);
        if (clickPayUser is null) return false;

        clickPayUser.IsActive = productUserProfile.IsActive;

        var result = await ClickPayPutUserAsync(clickPayUser, ct);

        if (result.IsSuccessStatusCode) return true;

        LogError(null, nameof(ExternalProductUserProfileChangeAsync),
            $"PUT failed for UserId={productUserProfile.UserId}");
        return false;
    }

    /// <inheritdoc/>
    /// <remarks>
    /// ClickPay may not have an existing SAML record for a new user.
    /// If either <c>ProductUserId</c> or <c>ProductUserName</c> is absent in
    /// the existing SAML data a <c>CreateSamlUserAttribute</c> call is issued
    /// (rather than an update).
    /// </remarks>
    protected override async Task UpdateSamlUserAttributeAsync(
        long personaId, int productId,
        string? productUserId, string? productUserLoginName, string? productUserEmail,
        CancellationToken ct)
    {
        LogDebug(nameof(UpdateSamlUserAttributeAsync),
            $"productUserLoginName={productUserLoginName}");

        var samlDetails = await _dataCollector.GetUserDetailsByPersonaAsync(
            SubjectUserDetails!.PersonaId, ProductId, ct);

        if (string.IsNullOrEmpty(samlDetails?.ProductUserId) && !string.IsNullOrEmpty(productUserId))
            await _dataCollector.CreateSamlUserAttributeAsync(
                personaId, productId, SamlAttributeEnum.UserId, productUserId, ct);

        if (string.IsNullOrEmpty(samlDetails?.ProductUserName) && !string.IsNullOrEmpty(productUserLoginName))
            await _dataCollector.CreateSamlUserAttributeAsync(
                personaId, productId, SamlAttributeEnum.productUsername, productUserLoginName, ct);
    }

    /// <inheritdoc/>
    /// <remarks>
    /// ClickPay profile updates are PUTs, not PATCHes.
    /// The current user object is first fetched from the product and then enriched
    /// with the latest profile data (name, email, phone) before being PUT back.
    /// </remarks>
    protected override async Task<ApiResponse> ProductUserProfileChangeAsync(
        ProductUserProfile profile, CancellationToken ct)
    {
        LogDebug(nameof(ProductUserProfileChangeAsync),
            $"UserId={profile.UserId}");

        var clickPayUser = await GetProductUserAsync(ct: ct)
            ?? throw new InvalidOperationException(
                   $"ClickPay user not found for persona {SubjectUserDetails?.PersonaId}");

        clickPayUser.LoginName    = SubjectUserDetails?.ProductUserName;
        clickPayUser.FirstName    = SubjectUserDetails?.FirstName;
        clickPayUser.MiddleName   = SubjectUserDetails?.MiddleName;
        clickPayUser.LastName     = SubjectUserDetails?.LastName;
        clickPayUser.Email        = SubjectUserDetails?.Email;
        clickPayUser.PhoneNumbers = SubjectUserDetails?.PhoneNumbers;
        clickPayUser.Phone        = SubjectUserDetails?.PhoneNumber;
        clickPayUser.IsActive     = true;
        clickPayUser.UserId       = SubjectUserDetails?.ProductUserId;
        clickPayUser.CompanyId    = CompanyInstanceSourceId;

        return await ClickPayPutUserAsync(clickPayUser, ct);
    }

    /// <inheritdoc/>
    /// <remarks>
    /// When the subject user has no product account but the login name already exists
    /// in ClickPay (multi-company scenario), the existing user's ID is retrieved and
    /// an update is issued rather than attempting a duplicate creation.
    /// </remarks>
    public override async Task<(string result, List<AdditionalParameters> auditParams)> CreateUpdateProductUserAsync(
        ProductUserRolePropertiesGroups userRolePropertiesRegion,
        BatchProcessType batchProcessType = BatchProcessType.CreateUpdateProductUser,
        CancellationToken ct = default)
    {
        LogDebug(nameof(CreateUpdateProductUserAsync),
            $"Editor={EditorUserDetails.PersonaId}");

        var newProductUser = await GenerateProductUserObjectAsync(userRolePropertiesRegion, ct);

        if (string.IsNullOrEmpty(SubjectUserDetails?.ProductUserName))
        {
            bool alreadyExists = await CheckUserExistInProductAsync(newProductUser.LoginName!, ct: ct);

            if (alreadyExists)
            {
                // Multi-company: merge into the existing user record
                var existingUrl = string.Format(
                    GetOperationEndPoint(ProductEntityEndpointKeyEnum.GetUserEndpoint),
                    newProductUser.LoginName, CompanyInstanceSourceId);

                var existingUser = await GetProductUserAsync(existingUrl, isThrowOnError: false, ct);
                if (existingUser?.UserId is not null)
                    newProductUser.UserId = existingUser.UserId;

                return await UpdateUserAsync(newProductUser, batchProcessType, ct);
            }

            return await CreateUserAsync(newProductUser, batchProcessType, ct);
        }

        // Existing product user — update
        LogDebug(nameof(CreateUpdateProductUserAsync), "Calling UpdateUser");
        newProductUser.UserId    = SubjectUserDetails!.ProductUserId;
        newProductUser.LoginName = SubjectUserDetails.ProductUserName;
        return await UpdateUserAsync(newProductUser, batchProcessType, ct);
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    /// <summary>
    /// Issues a PUT to the <c>PutUserEndpoint</c> with the supplied ClickPay user object.
    /// Centralises the ClickPay PUT pattern used by unassign, profile change, and external change.
    /// </summary>
    private async Task<ApiResponse> ClickPayPutUserAsync(
        IntegrationProductUser user, CancellationToken ct)
    {
        var baseUrl = GetOperationEndPoint(ProductEntityEndpointKeyEnum.PutUserEndpoint);
        DumpApiCallInfoToDiagnosticLog(baseUrl, user);
        return await CreateApi(baseUrl).PutEntityAsync<IntegrationProductUser>(user, ct: ct);
    }

    /// <summary>
    /// Merges the subject user's existing org–role assignments back into the org list.
    /// </summary>
    private static void MergeUserOrganizations(
        List<ClickPayOrganization> orgList,
        List<OrganizationRole>     userOrgRoles,
        string                     orgType,
        string                     orgRoleId)
    {
        foreach (var userOrgRole in userOrgRoles)
        {
            if (!userOrgRole.RoleId.Equals(orgRoleId, StringComparison.OrdinalIgnoreCase)) continue;

            var match = orgList.Find(x =>
                x.Id == userOrgRole.OrganizationId
                && x.Type.Equals(orgType, StringComparison.OrdinalIgnoreCase));

            if (match is not null)
                match.IsAssigned = true;
        }
    }
}
