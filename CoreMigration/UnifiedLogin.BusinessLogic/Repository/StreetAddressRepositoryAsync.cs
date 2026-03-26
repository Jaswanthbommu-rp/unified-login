using Dapper;
using System.Data;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Constants;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.Repository;

/// <summary>
/// Async-first street address repository using Dapper + <see cref="IDbConnectionFactory"/>.
/// Each method obtains its own connection from the factory so concurrent callers never share a connection.
/// </summary>
public sealed class StreetAddressRepositoryAsync : IStreetAddressRepositoryAsync
{
    private readonly IDbConnectionFactory _dbFactory;

    public StreetAddressRepositoryAsync(IDbConnectionFactory dbFactory)
    {
        _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
    }

    /// <inheritdoc/>
    public async Task<RepositoryResponse> CreateStreetAddressAsync(
        IStreetAddress streetAddress,
        CancellationToken cancellationToken = default)
    {
        using var db = _dbFactory.CreateConnection();
        var result = await db.QuerySingleOrDefaultAsync<RepositoryResponse>(new CommandDefinition(
            StoredProcNameConstants.SP_CreateStreetAddress,
            new
            {
                streetAddress.ContactMechanismId,
                streetAddress.StreetAddress1,
                streetAddress.StreetAddress2,
                streetAddress.StreetAddress3
            },
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken));

        return result ?? new RepositoryResponse();
    }
}