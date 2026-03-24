using System.Data;
using Dapper;
using Microsoft.Extensions.Logging;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Constants;

namespace UnifiedLogin.BusinessLogic.Repository;

/// <summary>
/// Async-first Contact Mechanism Usage Type Repository.
/// Uses injected <see cref="IDbConnection"/> (Dapper) directly —
/// no <see cref="BaseRepository"/> inheritance, no <c>new</c> keyword.
/// </summary>
public sealed class ContactMechanismUsageTypeRepositoryAsync : IContactMechanismUsageTypeRepositoryAsync
{
    private readonly IDbConnection _db;
    private readonly ILogger<ContactMechanismUsageTypeRepositoryAsync> _logger;

    public ContactMechanismUsageTypeRepositoryAsync(
        IDbConnection db,
        ILogger<ContactMechanismUsageTypeRepositoryAsync> logger)
    {
        _db     = db     ?? throw new ArgumentNullException(nameof(db));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task<IList<ContactMechanismUsageType>> ListContactMechanismUsageTypeAsync(
        string ContactMechanismUsageTypeName,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _db.QueryAsync<ContactMechanismUsageType>(
                new CommandDefinition(
                    StoredProcNameConstants.SP_ListContactMechanismUsageType,
                    new { ContactMechanismUsageTypeName },
                    commandType: CommandType.StoredProcedure,
                    cancellationToken: cancellationToken));

            return result.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Method} failed for UsageTypeName={Name}",
                nameof(ListContactMechanismUsageTypeAsync), ContactMechanismUsageTypeName);
            return [];
        }
    }
}