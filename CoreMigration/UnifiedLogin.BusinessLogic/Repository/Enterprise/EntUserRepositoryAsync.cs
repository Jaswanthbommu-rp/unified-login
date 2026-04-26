using System.Data;
using System.Data.Common;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using UnifiedLogin.DataAccess.Helper;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Batch;
using UnifiedLogin.SharedObjects.Constants;
using UnifiedLogin.SharedObjects.Enterprise;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing.Enum;

namespace UnifiedLogin.BusinessLogic.Repository.Enterprise;

/// <summary>
/// Async-first enterprise user repository.
/// <para>
/// Replaces <see cref="EntUserRepository"/> which extended <c>BaseRepository</c>
/// and accepted <c>DefaultUserClaim</c> in its constructor.
/// </para>
/// <para>
/// Each read method opens a short-lived connection from <see cref="IDbConnectionFactory"/>
/// so connections are returned to the ADO.NET pool promptly.
/// <see cref="CreateEnterpriseUserAsync"/> opens a single connection for the duration
/// of the transaction and commits/rolls back atomically.
/// </para>
/// </summary>
public sealed class EntUserRepositoryAsync : IEntUserRepositoryAsync
{
    private readonly IDbConnectionFactory _dbFactory;
    private readonly ILogger<EntUserRepositoryAsync> _logger;

    public EntUserRepositoryAsync(
        IDbConnectionFactory dbFactory,
        ILogger<EntUserRepositoryAsync> logger)
    {
        _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
        _logger    = logger    ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task<string> CreateEnterpriseUserAsync(
        UserProductDetails userProductDetails,
        IReadOnlyDictionary<string, int> productCodeToIdMap,
        long editorPersonaId,
        Guid impersonatedBy,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(userProductDetails);
        ArgumentNullException.ThrowIfNull(productCodeToIdMap);

        await using SqlConnection connection = _dbFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
        await using DbTransaction transaction =
            await connection.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken)
                            .ConfigureAwait(false);
        try
        {
            // ── Step 1: create the user record ────────────────────────────
            var createParam = new
            {
                FirstName              = userProductDetails.UserProfileDetails.FirstName,
                MiddleName             = userProductDetails.UserProfileDetails.MiddleName,
                LastName               = userProductDetails.UserProfileDetails.LastName,
                UserTypeId             = userProductDetails.UserProfileDetails.UserType,
                ThirdPartyIDP          = userProductDetails.UserProfileDetails.IsExternalIdp,
                LoginName              = userProductDetails.UserProfileDetails.LoginName,
                NotificationEmail      = userProductDetails.UserProfileDetails.Email,
                UserEffectiveDate      = userProductDetails.UserProfileDetails.UserEffectiveDate!.Value,
                UserExpirationDate     = userProductDetails.UserProfileDetails.UserExpirationDate!.Value,
                Phone                  = userProductDetails.UserProfileDetails.Phone,
                PhoneType              = PhoneType.Work.ToString(),
                PreferredContactMethod = string.Empty,
                Title                  = userProductDetails.UserProfileDetails.Title,
                Suffix                 = userProductDetails.UserProfileDetails.Suffix,
                Pwdhash                = userProductDetails.UserProfileDetails.PasswordHash,
                PwdSalt                = userProductDetails.UserProfileDetails.PasswordSalt,
                CreateUserSourceType   = "RPX",
                OrganizationId         = userProductDetails.UserProfileDetails.OrganizationPartyId,
                EmployeeId             = userProductDetails.UserProfileDetails.EmployeeId
            };

            var spResult = await connection.QuerySingleOrDefaultAsync<dynamic>(
                new CommandDefinition(
                    EnterpriseStoredProcNameConstants.SP_CreateUnityUser,
                    createParam,
                    transaction: transaction,
                    commandType: CommandType.StoredProcedure,
                    cancellationToken: cancellationToken))
                .ConfigureAwait(false);

            string newUserRealPageId = spResult.RealPageId.ToString();

            // ── Step 2: resolve persona and user IDs ──────────────────────
            long newUserPersonaId = await connection.ExecuteScalarAsync<long>(
                new CommandDefinition(
                    StoredProcNameConstants.SP_GetActivePersona,
                    new { RealPageId = newUserRealPageId },
                    transaction: transaction,
                    commandType: CommandType.StoredProcedure,
                    cancellationToken: cancellationToken))
                .ConfigureAwait(false);

            // ── Step 3: resolve impersonator if present ────────────────────
            long impersonatorUserId = 0;
            if (impersonatedBy != Guid.Empty)
            {
                var impersonatorLogin = await connection.QuerySingleOrDefaultAsync<UserLoginOnly>(
                    new CommandDefinition(
                        StoredProcNameConstants.SP_GetUserLoginOnly,
                        new { RealPageId = impersonatedBy },
                        transaction: transaction,
                        commandType: CommandType.StoredProcedure,
                        cancellationToken: cancellationToken))
                    .ConfigureAwait(false);
                impersonatorUserId = impersonatorLogin?.UserId ?? 0;
            }

            // ── Step 4: queue product batch records ────────────────────────
            if (userProductDetails.ProductList?.Count > 0)
            {
                await SaveProductBatchAsync(
                    connection, transaction,
                    editorPersonaId,
                    newUserPersonaId,
                    userProductDetails.EditorRealPageId,
                    userProductDetails.ProductList,
                    productCodeToIdMap,
                    impersonatorUserId,
                    cancellationToken).ConfigureAwait(false);
            }

            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
            return newUserRealPageId;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<IList<UsersData>> ListUsersAsync(
        long organizationPartyId,
        IList<int> productIdList,
        int statusTypeId,
        Guid? realPageId = null,
        string? name = null,
        int rowsPerPage = 0,
        int pageNumber = 1,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var param = new
            {
                OrganizationId = organizationPartyId,
                ProductIds     = TableValueParamHelper.ConvertToTableValuedParameter(productIdList, "enterprise.productidtype"),
                RealPageId     = realPageId,
                StatusTypeId   = statusTypeId,
                Name           = name,
                RowsPerPage    = rowsPerPage,
                PageNumber     = pageNumber
            };

            await using var connection = _dbFactory.CreateConnection();
            var results = await connection.QueryAsync<UsersData>(
                new CommandDefinition(
                    EnterpriseStoredProcNameConstants.SP_ListUserInformation,
                    param,
                    commandType: CommandType.StoredProcedure,
                    cancellationToken: cancellationToken))
                .ConfigureAwait(false);

            return results.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "ListUsersAsync failed. OrganizationPartyId={OrgPartyId} StatusTypeId={StatusTypeId}",
                organizationPartyId, statusTypeId);
            return [];
        }
    }

    /// <inheritdoc/>
    public async Task<IList<UserProductDetailAttribute>> ListUserProductDetailsLoginByPersonaIdAsync(
        long personaId,
        CancellationToken cancellationToken = default)
    {
        await using var connection = _dbFactory.CreateConnection();
        var results = await connection.QueryAsync<UserProductDetailAttribute>(
            new CommandDefinition(
                EnterpriseStoredProcNameConstants.SP_ListUsersProductsDetailsLoginByPersonaId,
                new { PersonaId = personaId },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken))
            .ConfigureAwait(false);

        return results.ToList();
    }

    /// <inheritdoc/>
    public async Task<IList<UserProductDetailAttribute>> ListUserProductDetailsLoginByLoginNameAsync(
        string loginName,
        CancellationToken cancellationToken = default)
    {
        await using var connection = _dbFactory.CreateConnection();
        var results = await connection.QueryAsync<UserProductDetailAttribute>(
            new CommandDefinition(
                EnterpriseStoredProcNameConstants.SP_ListUsersProductsDetailsLoginByLoginName,
                new { loginName },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken))
            .ConfigureAwait(false);

        return results.ToList();
    }

    // ── Private helpers ────────────────────────────────────────────────────

    private async Task SaveProductBatchAsync(
        SqlConnection connection,
        DbTransaction transaction,
        long editorPersonaId,
        long subjectPersonaId,
        Guid editorRealPageId,
        IList<ProductDetail> products,
        IReadOnlyDictionary<string, int> productCodeToIdMap,
        long impersonatorUserId,
        CancellationToken cancellationToken)
    {
        var batchGroup = await CreateBatchProcessGroupAsync(connection, transaction, cancellationToken)
            .ConfigureAwait(false);

        foreach (var prod in products)
        {
            if (!productCodeToIdMap.TryGetValue(prod.ProductCode.ToUpperInvariant(), out int productId) || productId == 0)
            {
                throw new InvalidOperationException(
                    $"Product code '{prod.ProductCode}' could not be resolved to a product ID.");
            }

            var batchParam = new
            {
                PersonRealPageId      = editorRealPageId,
                CreateUserPersonaId   = editorPersonaId,
                AssignUserPersonaId   = subjectPersonaId,
                ProductId             = productId,
                StatusTypeId          = 5,
                RetryCount            = 0,
                BatchProcessorGroupId = batchGroup.BatchProcessorGroupId,
                ImpersonatorUserId    = impersonatorUserId,
                InputJson             = JsonConvert.SerializeObject(new RolePropertyList
                {
                    PropertyList = prod.PropertiesAssigned,
                    RoleList     = prod.RolesAssigned,
                    RegionList   = prod.RegionsAssigned,
                    IsAssigned   = true
                }),
                UseAPIV2 = true
            };

            var result = await connection.QuerySingleOrDefaultAsync<dynamic>(
                new CommandDefinition(
                    StoredProcNameConstants.SP_CreateProductBatch,
                    batchParam,
                    transaction: transaction,
                    commandType: CommandType.StoredProcedure,
                    cancellationToken: cancellationToken))
                .ConfigureAwait(false);

            if (result?.Id == 0)
                throw new InvalidOperationException(
                    $"Failed to insert product batch record for product code '{prod.ProductCode}'.");
        }
    }

    private async Task<BatchProcessorGroup> CreateBatchProcessGroupAsync(
        SqlConnection connection,
        DbTransaction transaction,
        CancellationToken cancellationToken)
    {
        var param = new DynamicParameters();
        param.Add("@BatchProcessorGroupID", 0, dbType: DbType.Int32, direction: ParameterDirection.Output);

        try
        {
            await connection.ExecuteAsync(
                new CommandDefinition(
                    StoredProcNameConstants.SP_CreateBatchProcessorGroup,
                    param,
                    transaction: transaction,
                    commandType: CommandType.StoredProcedure,
                    cancellationToken: cancellationToken))
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Method} failed to create batch processor group",
                nameof(CreateBatchProcessGroupAsync));
        }

        return new BatchProcessorGroup
        {
            BatchProcessorGroupId             = param.Get<int>("@BatchProcessorGroupID"),
            BatchProcessorGroupActivityLogged = false
        };
    }
}
