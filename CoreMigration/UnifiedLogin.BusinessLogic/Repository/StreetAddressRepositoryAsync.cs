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
/// Async-first Street Address Repository.
/// Uses injected <see cref="IDbConnection"/> (Dapper) directly —
/// no <see cref="BaseRepository"/> inheritance, no <c>new</c> keyword.
/// </summary>
public sealed class StreetAddressRepositoryAsync : IStreetAddressRepositoryAsync
{
    private readonly IDbConnection _db;
    private readonly ILogger<StreetAddressRepositoryAsync> _logger;

    public StreetAddressRepositoryAsync(IDbConnection db, ILogger<StreetAddressRepositoryAsync> logger)
    {
        _db     = db     ?? throw new ArgumentNullException(nameof(db));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task<RepositoryResponse> CreateStreetAddressAsync(
        IStreetAddress streetAddress, CancellationToken cancellationToken = default)
    {
        return await _db.QuerySingleOrDefaultAsync<RepositoryResponse>(
            new CommandDefinition(
                StoredProcNameConstants.SP_CreateStreetAddress,
                new
                {
                    streetAddress.ContactMechanismId,
                    streetAddress.StreetAddress1,
                    streetAddress.StreetAddress2,
                    streetAddress.StreetAddress3
                },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken))
            ?? new RepositoryResponse();
    }
}