using System.Data;
using Dapper;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Batch;
using UnifiedLogin.SharedObjects.Constants;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Hots;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.Repository;

/// <summary>
/// Async-first HOTS Clone User Repository.
/// Uses injected <see cref="IDbConnection"/> (Dapper) directly —
/// no <see cref="BaseRepository"/> inheritance, no <c>new</c> keyword.
/// </summary>
public sealed class HOTSCloneUserRepositoryAsync : IHOTSCloneUserRepositoryAsync
{
    private readonly IDbConnection _db;
    private readonly ILogger<HOTSCloneUserRepositoryAsync> _logger;

    public HOTSCloneUserRepositoryAsync(
        IDbConnection db,
        ILogger<HOTSCloneUserRepositoryAsync> logger)
    {
        _db     = db     ?? throw new ArgumentNullException(nameof(db));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task<IList<BaseLineCustomerCompanyUser>> ListUsersAsync(
        long organizationId,
        CancellationToken cancellationToken = default)
    {
        var result = await _db.QueryAsync<BaseLineCustomerCompanyUser>(
            new CommandDefinition(
                StoredProcNameConstants.SP_ListHotsBaseOrganizationUsers,
                new { OrganizationId = organizationId },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));
        return result.ToList();
    }

    /// <inheritdoc/>
    public async Task<List<PersonaProductUserDetails>> GetUserProductsAsync(
        long personaId,
        CancellationToken cancellationToken = default)
    {
        var result = await _db.QueryAsync<PersonaProductUserDetails>(
            new CommandDefinition(
                StoredProcNameConstants.SP_ListProductsByPersonaId,
                new { PersonaId = personaId, ProductStatusValue = ((int)ProductBatchStatusType.Success).ToString() },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));
        return result.ToList();
    }

    /// <inheritdoc/>
    public async Task<Guid> GetBaseCompanyUPFMIdAsync(
        Guid cloneUpfmId,
        CancellationToken cancellationToken = default)
    {
        return await _db.QuerySingleOrDefaultAsync<Guid>(
            new CommandDefinition(
                StoredProcNameConstants.SP_GetBaseCompanyUPFMId,
                new { RealPageId = cloneUpfmId },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));
    }

    /// <inheritdoc/>
    public async Task<UserLoginOnly> GetUserLoginOnlyAsync(
        string enterpriseUserName,
        CancellationToken cancellationToken = default)
    {
        return await _db.QuerySingleOrDefaultAsync<UserLoginOnly>(
            new CommandDefinition(
                StoredProcNameConstants.SP_GetUserLoginOnly,
                new { EnterpriseUserName = enterpriseUserName },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));
    }

    /// <inheritdoc/>
    public async Task<IList<Persona>> ListPersonaAsync(
        Guid realPageId,
        CancellationToken cancellationToken = default)
    {
        var result = await _db.QueryAsync<Persona>(
            new CommandDefinition(
                StoredProcNameConstants.SP_ListPersona,
                new { RealPageId = realPageId },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));
        return result.ToList();
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Multi-step transactional write — replaces the original single
    /// <c>using (var repository = GetRepository())</c> block.
    /// All steps share one transaction so a failure rolls back completely.
    /// </remarks>
    public async Task<HotsUser> CreateUserAsync(
        DefaultUserClaim cloneCompanyAdminUserClaim,
        long partyId,
        BaseLineCustomerCompanyUser user,
        IProfileDetail baseUserProfile,
        List<ProductBatch> productBatch,
        UserLogin userLogin,
        CancellationToken cancellationToken = default)
    {
        var hotsUser = new HotsUser();
        var loginName = BuildLoginName(partyId, baseUserProfile);

        OpenIfClosed();
        using var tx = _db.BeginTransaction();
        try
        {
            // Optional: resolve impersonator login info
            UserLoginOnly impersonatorLogin = new();
            if (cloneCompanyAdminUserClaim.ImpersonatedBy != Guid.Empty)
            {
                impersonatorLogin = await _db.QuerySingleOrDefaultAsync<UserLoginOnly>(
                    new CommandDefinition(
                        StoredProcNameConstants.SP_GetUserLoginOnly,
                        new { RealPageId = cloneCompanyAdminUserClaim.ImpersonatedBy },
                        transaction: tx,
                        commandType: CommandType.StoredProcedure,
                        cancellationToken: cancellationToken)) ?? new UserLoginOnly();
            }

            // Step 1 — create user
            var createParam = new
            {
                FirstName              = baseUserProfile.FirstName,
                MiddleName             = baseUserProfile.MiddleName,
                LastName               = baseUserProfile.LastName,
                UserTypeId             = baseUserProfile.UserTypeId,
                ThirdPartyIDP          = true,
                LoginName              = loginName,
                NotificationEmail      = string.Empty,
                UserEffectiveDate      = DateTime.UtcNow,
                UserExpirationDate     = new DateTime(9999, 12, 31),
                Phone                  = baseUserProfile.TelecommunicationNumber?.Count > 0 ? baseUserProfile.TelecommunicationNumber[0].PhoneNumber : string.Empty,
                PhoneType              = PhoneType.Work.ToString(),
                PreferredContactMethod = string.Empty,
                Title                  = baseUserProfile.Title,
                Suffix                 = baseUserProfile.Suffix,
                Pwdhash                = string.Empty,
                PwdSalt                = string.Empty,
                CreateUserSourceType   = "HOTS",
                OrganizationId         = partyId,
                EmployeeId             = baseUserProfile.EmployeeId
            };

            var spResult = await _db.QuerySingleOrDefaultAsync<dynamic>(
                new CommandDefinition(
                    EnterpriseStoredProcNameConstants.SP_CreateUnityUser,
                    createParam,
                    transaction: tx,
                    commandType: CommandType.StoredProcedure,
                    cancellationToken: cancellationToken));

            var newUserRealPageId = spResult.RealPageId.ToString();

            // Step 2 — resolve persona and user ids
            var newUserPersonaId = await _db.QuerySingleOrDefaultAsync<long>(
                new CommandDefinition(
                    StoredProcNameConstants.SP_GetActivePersona,
                    new { RealPageId = newUserRealPageId },
                    transaction: tx,
                    commandType: CommandType.StoredProcedure,
                    cancellationToken: cancellationToken));

            var userId = await _db.QuerySingleOrDefaultAsync<long>(
                new CommandDefinition(
                    StoredProcNameConstants.SP_GetUserLoginOnly,
                    new { RealPageId = newUserRealPageId },
                    transaction: tx,
                    commandType: CommandType.StoredProcedure,
                    cancellationToken: cancellationToken));

            // Step 3 — optional password update
            if (!string.IsNullOrEmpty(userLogin.Password)
                && !string.IsNullOrEmpty(userLogin.PasswordSalt)
                && !string.IsNullOrEmpty(userLogin.PasswordHash))
            {
                var pwdResponse = await _db.QuerySingleOrDefaultAsync<RepositoryResponse>(
                    new CommandDefinition(
                        StoredProcNameConstants.SP_UpdateHotsCloneUserPassword,
                        new { UserId = userId, PasswordHash = userLogin.PasswordHash, PasswordSalt = userLogin.PasswordSalt },
                        transaction: tx,
                        commandType: CommandType.StoredProcedure,
                        cancellationToken: cancellationToken));

                if (pwdResponse?.Id == userId)
                    hotsUser.ClonePassword = userLogin.Password;
            }

            // Step 4 — product batch
            if (productBatch?.Count > 0)
            {
                await SaveProductBatchAsync(
                    cloneCompanyAdminUserClaim.PersonaId, newUserPersonaId,
                    cloneCompanyAdminUserClaim.UserRealPageGuid, productBatch,
                    superUser: baseUserProfile.UserTypeId == 402,
                    impersonatorUserId: impersonatorLogin.UserId,
                    tx, cancellationToken);
            }

            tx.Commit();

            hotsUser.BaselineUserId   = baseUserProfile.userLogin.UserId;
            hotsUser.BaselineUserName = baseUserProfile.userLogin.LoginName;
            hotsUser.CloneUserId      = userId;
            hotsUser.CloneUserName    = loginName;
            hotsUser.ClonePersonaId   = newUserPersonaId;
            return hotsUser;
        }
        catch (Exception ex)
        {
            tx.Rollback();
            _logger.LogError(ex, "{Method} failed for partyId={PartyId}", nameof(CreateUserAsync), partyId);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<RepositoryResponse> InsertHotsCompanyRelationshipAsync(
        Guid baselineCompanyRealPageId,
        Guid cloneCompanyRealPageId,
        int userId,
        CancellationToken cancellationToken = default)
    {
        return await _db.QuerySingleOrDefaultAsync<RepositoryResponse>(
            new CommandDefinition(
                StoredProcNameConstants.SP_InsertHotsCompanyRelationship,
                new { BaseLineCompany = baselineCompanyRealPageId, CloneCompany = cloneCompanyRealPageId, UserId = userId },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken))
            ?? new RepositoryResponse();
    }

    /// <inheritdoc/>
    public async Task<RepositoryResponse> InsertHotsPropertyRelationshipAsync(
        Guid baselinePropertyInstanceId,
        Guid clonePropertyInstanceId,
        Guid cloneCompanyRealPageId,
        int userId,
        CancellationToken cancellationToken = default)
    {
        return await _db.QuerySingleOrDefaultAsync<RepositoryResponse>(
            new CommandDefinition(
                StoredProcNameConstants.SP_InsertHotsPropertyRelationship,
                new { BaseLineProperty = baselinePropertyInstanceId, CloneProperty = clonePropertyInstanceId, CloneCompany = cloneCompanyRealPageId, UserId = userId },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken))
            ?? new RepositoryResponse();
    }

    // ------------------------------------------------------------------ //

    private async Task SaveProductBatchAsync(
        long editorPersonaId, long subjectPersonaId, Guid editorRealPageId,
        IList<ProductBatch> products, bool superUser, long impersonatorUserId,
        IDbTransaction tx, CancellationToken cancellationToken)
    {
        var batchGroup = await CreateBatchProcessGroupAsync(tx, cancellationToken);
        var adminJson  = new RolePropertyList { PropertyRoleList = [], PropertyList = [], RoleList = [], IsAssigned = true };

        foreach (var prod in products)
        {
            var inputJson = JsonConvert.SerializeObject(superUser ? adminJson : prod.InputJson);

            var result = await _db.QuerySingleOrDefaultAsync<dynamic>(
                new CommandDefinition(
                    StoredProcNameConstants.SP_CreateProductBatch,
                    new
                    {
                        PersonRealPageId      = editorRealPageId,
                        CreateUserPersonaId   = editorPersonaId,
                        AssignUserPersonaId   = subjectPersonaId,
                        ProductId             = prod.ProductId,
                        BatchProcessorGroupId = batchGroup.BatchProcessorGroupId,
                        StatusTypeId          = 5,
                        RetryCount            = 0,
                        InputJson             = inputJson,
                        CorrelationId         = Guid.NewGuid().ToString(),
                        ImpersonatorUserId    = impersonatorUserId,
                        BatchProcessTypeId    = BatchProcessType.CreateUpdateProductUser,
                        UseAPIV2              = true
                    },
                    transaction: tx,
                    commandType: CommandType.StoredProcedure,
                    cancellationToken: cancellationToken));

            if (result?.Id == 0)
                throw new InvalidOperationException($"Exception while inserting product with code {prod.ProductId} in the product batch.");
        }
    }

    private async Task<BatchProcessorGroup> CreateBatchProcessGroupAsync(
        IDbTransaction tx, CancellationToken cancellationToken)
    {
        // Replaces: DynamicParameters output param via sync Execute
        var param = new DynamicParameters();
        param.Add("@BatchProcessorGroupID", 0, dbType: DbType.Int32, direction: ParameterDirection.Output);

        try
        {
            await _db.ExecuteAsync(
                new CommandDefinition(
                    StoredProcNameConstants.SP_CreateBatchProcessorGroup,
                    param,
                    transaction: tx,
                    commandType: CommandType.StoredProcedure,
                    cancellationToken: cancellationToken));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Method} failed", nameof(CreateBatchProcessGroupAsync));
        }

        return new BatchProcessorGroup
        {
            BatchProcessorGroupId              = param.Get<int>("@BatchProcessorGroupID"),
            BatchProcessorGroupActivityLogged  = false
        };
    }

    private static string BuildLoginName(long partyId, IProfileDetail profile)
        => $"{profile.FirstName}{profile.LastName}{partyId}@realpage.com";

    private void OpenIfClosed()
    {
        if (_db.State != ConnectionState.Open)
            _db.Open();
    }
}