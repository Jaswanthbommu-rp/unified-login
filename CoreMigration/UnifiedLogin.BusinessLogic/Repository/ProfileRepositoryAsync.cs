using System.Data;
using Dapper;
using Microsoft.Extensions.Logging;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Constants;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.Repository;

/// <summary>
/// Async-first Profile Repository — pure data-access layer.
///
/// IMPORTANT: UpdateProfile and ListPersons are orchestration methods that
/// coordinate 10+ SPs, apply business rules, and call external services.
/// They have been intentionally excluded from this class.
/// Migrate them to a ProfileService that injects the async repositories.
/// </summary>
public sealed class ProfileRepositoryAsync : IProfileRepositoryAsync
{
    private readonly IDbConnection _db;
    private readonly ILogger<ProfileRepositoryAsync> _logger;

    public ProfileRepositoryAsync(IDbConnection db, ILogger<ProfileRepositoryAsync> logger)
    {
        _db     = db     ?? throw new ArgumentNullException(nameof(db));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task<bool> GetOrganizationHasAnyProductAssignmentErrorAsync(
        long orgPartyId, CancellationToken cancellationToken = default)
    {
        return await _db.QuerySingleOrDefaultAsync<bool>(
            new CommandDefinition(
                StoredProcNameConstants.SP_GetOrganizationHasPersonaProductError,
                new { PartyId = orgPartyId },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));
    }

    /// <inheritdoc/>
    public async Task<IList<ProductUsers>> ListPersonsByProductIdAsync(
        int productId, Guid? realPageId = null, long? personaId = null,
        CancellationToken cancellationToken = default)
    {
        // Replaces: repository.GetManyWithSpliOn<ProductUsers, UserLoginCommon, ProductUsers>(...)
        var lookup = new Dictionary<long, ProductUsers>();

        await _db.QueryAsync<ProductUsers, UserLoginCommon, ProductUsers>(
            StoredProcNameConstants.SP_ListPersonsByProductId,
            (user, login) =>
            {
                user.userLogin = login;
                lookup[login.UserId] = user;
                return user;
            },
            new { ProductId = productId, RealPageId = realPageId, PersonaId = personaId },
            splitOn: "UserId",
            commandType: CommandType.StoredProcedure);

        return lookup.Values.ToList();
    }

    /// <inheritdoc/>
    public async Task<ExternalUserRelationship> GetExternalUserRelationshipAsync(
        long organizationPartyId, long userId, CancellationToken cancellationToken = default)
    {
        // Replaces: two sequential sync calls inside the same using block
        var personaListTask = _db.QueryAsync<UserLoginPersona>(
            new CommandDefinition(
                StoredProcNameConstants.SP_GetUserLoginPersona,
                new { UserLoginId = userId, OrganizationPartyId = organizationPartyId },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));

        var personaList = (await personaListTask).ToList();
        var first = personaList.FirstOrDefault();
        if (first is null) return null;

        return await _db.QuerySingleOrDefaultAsync<ExternalUserRelationship>(
            new CommandDefinition(
                StoredProcNameConstants.SP_GetExternalUserRelationship,
                new { UserLoginPersonaId = first.UserLoginPersonaId },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));
    }
}