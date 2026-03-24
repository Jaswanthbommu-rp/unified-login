using System.Data;
using Dapper;
using Microsoft.Extensions.Logging;
using UnifiedLogin.BusinessLogic.CacheHelper;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.DataAccess.Helper;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Cache;
using UnifiedLogin.SharedObjects.Constants;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product;
using UnifiedLogin.SharedObjects.Product.EmployeeAccess;
using UnifiedLogin.SharedObjects.Product.UnifiedLogin;

namespace UnifiedLogin.BusinessLogic.Repository;

/// <summary>
/// Async-first UnifiedLogin Repository — role/right/property management.
/// Uses injected <see cref="IDbConnection"/> (Dapper) directly —
/// no <see cref="BaseRepository"/> inheritance, no <c>new</c> keyword.
/// <para>
/// <c>getRoleRightsSchemaName</c> is now an async helper backed by
/// <see cref="ICacheService"/> + <see cref="IProductInternalSettingRepositoryAsync"/>.
/// </para>
/// </summary>
public sealed class UnifiedLoginRepositoryAsync : IUnifiedLoginRepositoryAsync
{
    #region Fields

    private readonly IDbConnection _db;
    private readonly IProductInternalSettingRepositoryAsync _internalSettingRepo;
    private readonly ICacheService _cache;
    private readonly ILogger<UnifiedLoginRepositoryAsync> _logger;

    // Replaces: RPObjectCache TTL 60 s
    private static readonly CacheEntryOptions SchemaCacheOptions = new() { ExpirationTimeInMinutes = 1 };

    #endregion

    #region Constructor

    public UnifiedLoginRepositoryAsync(
        IDbConnection db,
        IProductInternalSettingRepositoryAsync internalSettingRepo,
        ICacheService cache,
        ILogger<UnifiedLoginRepositoryAsync> logger)
    {
        _db                  = db                  ?? throw new ArgumentNullException(nameof(db));
        _internalSettingRepo = internalSettingRepo ?? throw new ArgumentNullException(nameof(internalSettingRepo));
        _cache               = cache               ?? throw new ArgumentNullException(nameof(cache));
        _logger              = logger              ?? throw new ArgumentNullException(nameof(logger));
    }

    #endregion

    #region IUnifiedLoginRepositoryAsync — Reads

    /// <inheritdoc/>
    public async Task<List<UserDetail>> ListUsersAsync(
        string filter, string organizationTypeIds = null, CancellationToken cancellationToken = default)
    {
        // Replaces: GetMany<dynamic> + foreach mapping + OrderBy
        var rows = await _db.QueryAsync<dynamic>(
            new CommandDefinition(
                StoredProcNameConstants.SP_ListUsers,
                new { Name = filter, OrganizationTypeIds = organizationTypeIds },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));

        return rows
            .Select(item => new UserDetail
            {
                CompanyId       = long.Parse(item.PartyId.ToString()),
                CompanyName     = item.CompanyName,
                CompanyStatus   = item.CompanyStatus,
                EmailId         = item.NotificationEmail,
                FirstName       = item.FirstName,
                LastName        = item.LastName,
                UserId          = int.Parse(item.UserId.ToString()),
                UserName        = item.Username,
                UserType        = item.UserType,
                Name3rdPartyIDP = item.ThirdPartyIDPDesc,
                PersonaId       = item.PersonaId,
                PersonaRealPageId = item.PersonaRealPageId,
                UserStatus      = item.UserStatus
            })
            .OrderBy(u => u.CompanyName)
            .ToList();
    }

    /// <inheritdoc/>
    public async Task<List<ProductRole>> ListRolesByPartyAsync(
        long partyId, CancellationToken cancellationToken = default)
    {
        var rows = await _db.QueryAsync<dynamic>(
            new CommandDefinition(
                StoredProcNameConstants.SP_ListRolesByParty,
                new { partyId },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));

        return rows
            .Select(item => new ProductRole { ID = item.RoleID.ToString(), Name = item.Value, IsAssigned = false, Alias = item.RoleNickName })
            .ToList();
    }

    /// <inheritdoc/>
    public async Task<List<RightRoleDetail>> ListRoleWithRightsAsync(
        long partyId, long ulProductId, List<int> productIdList, CancellationToken cancellationToken = default)
    {
        var param = new DynamicParameters();
        param.Add("@PartyId",        partyId);
        param.Add("@ProductId",      ulProductId);
        param.Add("@TargetProductId",
            TableValueParamHelper.ConvertToTableValuedParameter(productIdList, "enterprise.productidtype"));

        var rows = await _db.QueryAsync<dynamic>(
            new CommandDefinition(
                StoredProcNameConstants.SP_ListRolesAssociatedWithRights,
                param,
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));

        return rows
            .Select(item => new RightRoleDetail
            {
                RoleId          = item.RoleId,      RoleName      = item.Role,
                IsAssigned      = false,             RoleType      = item.RoleType,
                RightName       = item.Right,        RightId       = item.RightId,
                RightValueTypeId = item.RightValueTypeId,
                IsDefaultRole   = (bool)item.DefaultRole,
                RightNickName   = item.RightNickName
            })
            .ToList();
    }

    /// <inheritdoc/>
    public async Task<List<ProductRight>> ListRightForProductsByPartyIdAsync(
        long partyId, long productId, CancellationToken cancellationToken = default)
    {
        var rows = await _db.QueryAsync<dynamic>(
            new CommandDefinition(
                StoredProcNameConstants.SP_ListRightForProductsByPartyId,
                new { PartyId = partyId, ProductId = productId },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));

        return rows
            .Select(item => new ProductRight { ID = item.RightId, Description = item.value, Assigned = false, Alias = item.RightNickName })
            .ToList();
    }

    /// <inheritdoc/>
    public async Task<List<ProductRight>> ListAllRightsForProductsByPartyIdAsync(
        long partyId, long productId, List<int> productIdList, CancellationToken cancellationToken = default)
    {
        var param = new DynamicParameters();
        param.Add("@PartyId",        partyId);
        param.Add("@ProductId",      productId);
        param.Add("@TargetProductId",
            TableValueParamHelper.ConvertToTableValuedParameter(productIdList, "enterprise.productidtype"));

        var rows = await _db.QueryAsync<dynamic>(
            new CommandDefinition(
                StoredProcNameConstants.SP_ListAllRights,
                param,
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));

        return rows
            .Select(item => new ProductRight { ID = item.RightValueTypeId, Description = item.Right, Assigned = false, Alias = item.RightNickName })
            .ToList();
    }

    /// <inheritdoc/>
    public async Task<List<ProductRole>> ListRolesForProductsByPartyIdAsync(
        long partyId, long productId, List<int> productIdList, CancellationToken cancellationToken = default)
    {
        var param = new DynamicParameters();
        param.Add("@PartyId",        partyId);
        param.Add("@ProductId",      productId);
        param.Add("@TargetProductId",
            TableValueParamHelper.ConvertToTableValuedParameter(productIdList, "enterprise.productidtype"));

        var rows = await _db.QueryAsync<dynamic>(
            new CommandDefinition(
                StoredProcNameConstants.SP_ListRolesForProductsByPartyId,
                param,
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));

        return rows
            .Select(item => new ProductRole
            {
                ID          = item.RoleId.ToString(), Name      = item.value,
                IsAssigned  = false,                  Roletype  = item.RoleType,
                Alias       = item.RoleNickName,      DefaultRole = item.DefaultRole.ToString()
            })
            .ToList();
    }

    /// <inheritdoc/>
    public async Task<List<UnifiedLoginCompany>> ListCompaniesAsync(
        string filter = "", string organizationTypeIds = null, CancellationToken cancellationToken = default)
    {
        var rows = await _db.QueryAsync<dynamic>(
            new CommandDefinition(
                StoredProcNameConstants.SP_ListOrganizations,
                new { Filter = filter, OrganizationTypeIds = organizationTypeIds },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));

        return rows
            .Select(item => new UnifiedLoginCompany
            {
                CompanyId              = long.Parse(item.BooksMasterId.ToString()),
                BooksCustomerMasterId  = long.Parse(string.IsNullOrEmpty(item.BooksCustomerMasterId?.ToString()) ? "0" : item.BooksCustomerMasterId.ToString()),
                CompanyName            = item.Name,
                IsActive               = int.Parse(item.IsActive.ToString()) == 1,
                PartyId                = item.PartyId,
                CompanyRealPageId      = item.OrganizationRealPageId.ToString(),
                UserRealPageId         = item.PersonRealPageId.ToString(),
                UserLoginAs            = item.LoginName,
                Domain                 = item.Domain
            })
            .OrderBy(c => c.CompanyName)
            .ToList();
    }

    /// <inheritdoc/>
    public async Task<List<PersonaADGroup>> GetPersonaADGroupsAsync(
        long personaId, CancellationToken cancellationToken = default)
    {
        var rows = await _db.QueryAsync<dynamic>(
            new CommandDefinition(
                StoredProcNameConstants.SP_GetADGroupsByPersonaId,
                new { PersonaId = personaId },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));

        return rows
            .Select(item => new PersonaADGroup
            {
                ADGroupId    = item.ADGroupId,   ADGroupName   = item.ADGroupName,
                ProductsCount = item.ProductsCount, RightsCount = item.RightsCount
            })
            .ToList();
    }

    /// <inheritdoc/>
    public async Task<List<OrgTypesADGroups>> GetOrgTypesADGroupsAsync(CancellationToken cancellationToken = default)
    {
        var rows = await _db.QueryAsync<dynamic>(
            new CommandDefinition(
                StoredProcNameConstants.SP_GetOrganizationTypeADGroups,
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));

        return rows
            .Select(item => new OrgTypesADGroups
            {
                OrganizationTypeId   = item.OrganizationTypeId,  OrganizationTypeName = item.OrganizationTypeName,
                ADGroupId            = item.ADGroupId,            ADGroupName          = item.ADGroupName
            })
            .ToList();
    }

    /// <inheritdoc/>
    public async Task<List<RightRoleDetail>> ListRoleRightDetForProductsByPartyIdAsync(
        long partyId, long productId, List<int> productIdList, CancellationToken cancellationToken = default)
    {
        var param = new DynamicParameters();
        param.Add("@PartyId",        partyId);
        param.Add("@ProductId",      productId);
        param.Add("@TargetProductId",
            TableValueParamHelper.ConvertToTableValuedParameter(productIdList, "enterprise.productidtype"));

        var rows = await _db.QueryAsync<dynamic>(
            new CommandDefinition(
                StoredProcNameConstants.SP_ListRightsAssociatedWithRoles,
                param,
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));

        return rows
            .Select(item => new RightRoleDetail
            {
                RoleId = item.RoleId, RoleName = item.Role, IsAssigned = false,
                RoleType = item.RoleType, RightId = item.RightId, RightName = item.Right,
                RightValueTypeId = item.RightValueTypeId
            })
            .ToList();
    }

    /// <inheritdoc/>
    public async Task<List<RightRoleDetail>> ListRightWithRolesAsync(
        long partyId, long productId, IList<int> productIdList, CancellationToken cancellationToken = default)
    {
        var param = new DynamicParameters();
        param.Add("@PartyId",        partyId);
        param.Add("@ProductId",      productId);
        param.Add("@TargetProductId",
            TableValueParamHelper.ConvertToTableValuedParameter(productIdList, "enterprise.productidtype"));

        var rows = await _db.QueryAsync<dynamic>(
            new CommandDefinition(
                StoredProcNameConstants.SP_ListRightsAssociatedWithRoles,
                param,
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));

        return rows
            .Select(item => new RightRoleDetail
            {
                RoleId = item.RoleId, RoleName = item.Role, IsAssigned = false, RoleType = item.RoleType,
                RightName = item.Right, RightId = item.RightId, RightValueTypeId = item.RightValueTypeId,
                RightNickName = item.RightNickName, RightDescription = item.RightDescription
            })
            .ToList();
    }

    /// <inheritdoc/>
    public async Task<List<ProductRight>> ListRightsByRoleAsync(
        long partyId, IList<int> productIdList, long productId, long roleId,
        CancellationToken cancellationToken = default)
    {
        var param = new DynamicParameters();
        param.Add("@PartyId",        partyId);
        param.Add("@ProductId",      productId);
        param.Add("@RoleId",         roleId);
        param.Add("@TargetProductId",
            TableValueParamHelper.ConvertToTableValuedParameter(productIdList, "enterprise.productidtype"));

        var rows = await _db.QueryAsync<dynamic>(
            new CommandDefinition(
                StoredProcNameConstants.SP_ListRolesAssociatedWithRights,
                param,
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));

        return rows
            .Select(item => new ProductRight { ID = item.RightValueTypeId, Description = item.Right, Alias = item.RightNickName, Assigned = true })
            .ToList();
    }

    /// <inheritdoc/>
    public async Task<List<ProductRole>> ListRolesByRightAsync(
        long partyId, long productId, long rightId, CancellationToken cancellationToken = default)
    {
        var rows = await _db.QueryAsync<dynamic>(
            new CommandDefinition(
                StoredProcNameConstants.SP_ListRightsAssociatedWithRoles,
                new { PartyId = partyId, ProductId = productId, RightId = rightId },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));

        return rows
            .Select(item => new ProductRole { ID = item.RoleId.ToString(), Name = item.Role, IsAssigned = true, Alias = item.RoleNickName })
            .ToList();
    }

    /// <inheritdoc/>
    public async Task<List<ProductRole>> ListRolesAssignedToPersonaAsync(
        long userPersonaId, CancellationToken cancellationToken = default)
    {
        var rows = await _db.QueryAsync<dynamic>(
            new CommandDefinition(
                StoredProcNameConstants.SP_ListRolesByParty,
                new { userPersonaId },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));

        return rows
            .Select(item => new ProductRole { ID = item.RoleID.ToString(), Name = item.Value, IsAssigned = false, Alias = item.RoleNickName })
            .ToList();
    }

    /// <inheritdoc/>
    public async Task<List<PropertyRole>> ListPropertyMappingByPersonaAsync(
        long userPersonaId, long productId, CancellationToken cancellationToken = default)
    {
        var rows = await _db.QueryAsync<dynamic>(
            new CommandDefinition(
                StoredProcNameConstants.SP_ListPropertyMapping,
                new { PersonaID = userPersonaId, ProductID = productId },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));

        return rows
            .Select(item => new PropertyRole { RoleID = item.RoleID, PropID = item.PropertyID })
            .ToList();
    }

    /// <inheritdoc/>
    public async Task<List<Property>> ListPropByPersonaAsync(
        long userPersonaId, long productId, CancellationToken cancellationToken = default)
    {
        var rows = await _db.QueryAsync<dynamic>(
            new CommandDefinition(
                StoredProcNameConstants.SP_ListPropertyMapping,
                new { PersonaID = userPersonaId, ProductID = productId },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));

        return rows
            .Select(item => new Property { PropID = item.PropertyID })
            .ToList();
    }

    /// <inheritdoc/>
    public async Task<List<CategoryType>> GetCategoryTypeAsync(CancellationToken cancellationToken = default)
    {
        var rows = await _db.QueryAsync<dynamic>(
            new CommandDefinition(
                StoredProcNameConstants.SP_ListSecurityStatus,
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));

        return rows
            .Select(item => new CategoryType { CategoryName = item.CategoryType, Status = item.Status, StatusTypeid = item.StatusTypeId })
            .ToList();
    }

    #endregion

    #region IUnifiedLoginRepositoryAsync — Writes

    /// <inheritdoc/>
    public async Task<int> LinkRightsToRoleAsync(
        IEnumerable<RightRoleAddRem> rightsList, int userId, CancellationToken cancellationToken = default)
    {
        // Replaces: ExecuteStoredProcWithTvp — same intent with DynamicParameters TVP
        var param = new DynamicParameters();
        param.Add("@CreatedBy", userId);
        param.Add("@NewRightID", 0);
        param.Add("@ManageRight",
            TableValueParamHelper.ConvertToTableValuedParameter(
                rightsList, "dbo.TYPROLE",
                new List<string> { "RoleId", "RightValueTypeID", "IsDeleted" }));

        return await _db.ExecuteAsync(
            new CommandDefinition(
                StoredProcNameConstants.SP_LinkRightsToRoles,
                param,
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));
    }

    /// <inheritdoc/>
    public async Task<RepositoryResponse> InsertDelAssignedPropRoleToUserAsync(
        long userPersonaId, long productId, UserLocation property, UserAccessGroup role,
        long del = 0, CancellationToken cancellationToken = default)
    {
        var rows = await _db.ExecuteAsync(
            new CommandDefinition(
                StoredProcNameConstants.SP_CreatePropertyMapping,
                new { PersonaID = userPersonaId, ProductID = productId, RoleID = int.Parse(role.AccessGroupCode), PropertyID = int.Parse(property.PropertyId), Deleted = del },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));

        return new RepositoryResponse { Id = rows };
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Replaces: sync <c>getRoleRightsSchemaName()</c> (RPObjectCache + IProductInternalSettingRepository)
    /// with async cached lookup — schema name determines which SP is called.
    /// </remarks>
    public async Task<RepositoryResponse> InsertAssignedRoleToUserAsync(
        long userPersonaId, UserAccessGroup role, int userId,
        long del = 0, CancellationToken cancellationToken = default)
    {
        var schemaName = await GetRoleRightsSchemaNameAsync(cancellationToken);
        var procName   = schemaName?.Length > 0
            ? $"{schemaName}.LinkPersonaToRole"
            : StoredProcNameConstants.SP_LinkPersonaToRole;

        var rows = await _db.ExecuteAsync(
            new CommandDefinition(
                procName,
                new { PersonaID = userPersonaId, RoleID = int.Parse(role.AccessGroupCode), CreatedBy = userId },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));

        return new RepositoryResponse { Id = rows };
    }

    /// <inheritdoc/>
    public async Task<RepositoryResponse> InsertDelAssignedPropRoleToUserNewAsync(
        long userPersonaId, long productId, long propertyId, long roleId,
        long del = 0, CancellationToken cancellationToken = default)
    {
        var rows = await _db.ExecuteAsync(
            new CommandDefinition(
                StoredProcNameConstants.SP_CreatePropertyMapping,
                new { PersonaID = userPersonaId, ProductID = productId, RoleID = roleId, PropertyID = propertyId, Deleted = del },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));

        return new RepositoryResponse { Id = rows };
    }

    /// <inheritdoc/>
    public async Task<RepositoryResponse> AddCustomRoleAsync(
        string roleName, string desc, long roleTypeId, long roleCategoryId,
        long partyId, int userId, string organizationType, CancellationToken cancellationToken = default)
    {
        var result = await _db.QuerySingleOrDefaultAsync<dynamic>(
            new CommandDefinition(
                StoredProcNameConstants.SP_CreateRole,
                new { RoleName = roleName, Description = desc, RoleTypeID = roleTypeId, RoleCategoryId = roleCategoryId, PartyID = partyId, CreatedBy = userId, OrganizationType = organizationType, RoleID = 0 },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));

        return new RepositoryResponse
        {
            Id           = result?.RoleID ?? 0,
            ErrorMessage = result?.ErrorMessage?.Trim() != string.Empty ? result?.ErrorMessage : null
        };
    }

    /// <inheritdoc/>
    public async Task<RepositoryResponse> DeleteRoleAsync(long roleId, CancellationToken cancellationToken = default)
    {
        var result = await _db.QuerySingleOrDefaultAsync<dynamic>(
            new CommandDefinition(
                StoredProcNameConstants.SP_DeleteRole,
                new { RoleId = roleId },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));

        return new RepositoryResponse
        {
            Id           = roleId,
            ErrorMessage = result != null && result.ErrorMessage?.Trim() != string.Empty ? result.ErrorMessage : null
        };
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Replaces: schema-branching <c>dynamic param</c> — schema resolved via async cache.
    /// </remarks>
    public async Task<RepositoryResponse> SetDefaultRoleAsync(
        long roleId, long partyId, int userId, CancellationToken cancellationToken = default)
    {
        var schemaName = await GetRoleRightsSchemaNameAsync(cancellationToken);

        object param = schemaName == "Security"
            ? new { RoleId = roleId, PartyId = partyId, CreatedBy = userId }
            : (object)new { RoleId = roleId, CreatedBy = userId };

        var result = await _db.QuerySingleOrDefaultAsync<dynamic>(
            new CommandDefinition(
                StoredProcNameConstants.SP_SetDefaulteRole,
                param,
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));

        return new RepositoryResponse
        {
            Id           = roleId,
            ErrorMessage = result != null && result.ErrorMessage?.Trim() != string.Empty ? result.ErrorMessage : null
        };
    }

    /// <inheritdoc/>
    public async Task<RepositoryResponse> UpdateCustomRoleAsync(
        long roleId, string roleName, string desc, int userId, CancellationToken cancellationToken = default)
    {
        var result = await _db.QuerySingleOrDefaultAsync<dynamic>(
            new CommandDefinition(
                StoredProcNameConstants.SP_UpdateRole,
                new { RoleName = roleName, Description = desc, RoleId = roleId, CreatedBy = userId },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));

        return new RepositoryResponse
        {
            Id           = roleId,
            ErrorMessage = result?.ErrorMessage?.Trim() != string.Empty ? result?.ErrorMessage : null
        };
    }

    #endregion

    #region Private helpers

    /// <summary>
    /// Async + cached replacement for the sync <c>getRoleRightsSchemaName()</c> helper.
    /// Replaces: <c>new RPObjectCache().GetFromCache(...)</c> + direct <c>IProductInternalSettingRepository</c>.
    /// </summary>
    private async Task<string> GetRoleRightsSchemaNameAsync(CancellationToken cancellationToken)
    {
        var cacheKey = $"getRoleRightsSchemaName_{(int)ProductEnum.UnifiedPlatform}";

        return await _cache.GetOrSetAsync(
            cacheKey,
            async ct =>
            {
                var settings = await _internalSettingRepo
                    .GetProductInternalSettingsAsync((int)ProductEnum.UnifiedPlatform, ct);

                return settings
                    .FirstOrDefault(s => s.Name.Equals("RolesRightsSchemaName", StringComparison.OrdinalIgnoreCase))
                    ?.Value;
            },
            SchemaCacheOptions,
            cancellationToken);
    }

    #endregion
}