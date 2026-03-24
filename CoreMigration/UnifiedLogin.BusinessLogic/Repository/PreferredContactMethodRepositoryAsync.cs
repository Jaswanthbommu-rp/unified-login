using System.Data;
using Dapper;
using Microsoft.Extensions.Logging;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects.Constants;
using UnifiedLogin.SharedObjects.IdentityConfig;

namespace UnifiedLogin.BusinessLogic.Repository;

public sealed class PreferredContactMethodRepositoryAsync : IPreferredContactMethodRepositoryAsync
{
    private readonly IDbConnection _db;
    private readonly ILogger<PreferredContactMethodRepositoryAsync> _logger;

    public PreferredContactMethodRepositoryAsync(IDbConnection db, ILogger<PreferredContactMethodRepositoryAsync> logger)
    {
        _db     = db     ?? throw new ArgumentNullException(nameof(db));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task<IList<PreferredContactMethod>> ListPreferredContactMethodAsync(CancellationToken cancellationToken = default)
    {
        var result = await _db.QueryAsync<PreferredContactMethod>(
            new CommandDefinition(
                StoredProcNameConstants.SP_ListPreferredContactMethods,
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));
        return result.ToList();
    }
}