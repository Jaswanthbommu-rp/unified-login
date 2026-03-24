using System.Data;
using Dapper;
using Microsoft.Extensions.Logging;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Constants;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.Repository;

public sealed class UserLoginPersonaRepositoryAsync : IUserLoginPersonaRepositoryAsync
{
    private readonly IDbConnection _db;
    private readonly ILogger<UserLoginPersonaRepositoryAsync> _logger;

    public UserLoginPersonaRepositoryAsync(IDbConnection db, ILogger<UserLoginPersonaRepositoryAsync> logger)
    {
        _db     = db     ?? throw new ArgumentNullException(nameof(db));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task<RepositoryResponse> CreateUserLoginPersonaAsync(
        UserLoginPersona userLoginPersona, CancellationToken cancellationToken = default)
    {
        return await _db.QuerySingleOrDefaultAsync<RepositoryResponse>(
            new CommandDefinition(
                StoredProcNameConstants.SP_CreateUserLoginPersona,
                new
                {
                    UserLoginId         = userLoginPersona.UserLoginId,
                    StatusTypeId        = userLoginPersona.StatusTypeId,
                    PrimaryOrganization = userLoginPersona.PrimaryOrganization,
                    FromDate            = userLoginPersona.FromDate,
                    ThruDate            = userLoginPersona.ThruDate
                },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken))
            ?? new RepositoryResponse();
    }

    /// <inheritdoc/>
    public async Task<IList<UserLoginPersona>> ListUserLoginPersonaAsync(
        long? userLoginPersonaId, long? userLoginId, long? organizationPartyId,
        CancellationToken cancellationToken = default)
    {
        var result = await _db.QueryAsync<UserLoginPersona>(
            new CommandDefinition(
                StoredProcNameConstants.SP_GetUserLoginPersona,
                new { UserLoginPersonaId = userLoginPersonaId, UserLoginId = userLoginId, OrganizationPartyId = organizationPartyId },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));

        return result.ToList();
    }
}