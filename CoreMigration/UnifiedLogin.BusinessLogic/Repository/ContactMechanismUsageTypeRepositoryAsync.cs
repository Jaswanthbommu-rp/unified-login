using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects.Constants;
using UnifiedLogin.SharedObjects.IdentityConfig;

namespace UnifiedLogin.BusinessLogic.Repository;

/// <summary>
/// Async-first Contact Mechanism Usage Type Repository.
/// <para>
/// Uses <see cref="IDbConnectionFactory"/> to open a short-lived <see cref="SqlConnection"/>
/// per call so connections are returned to the ADO.NET pool immediately after the
/// stored-procedure result is read — no long-lived connection is held on the instance.
/// </para>
/// <para>
/// Replaces the previous implementation that injected a single <see cref="IDbConnection"/>
/// (held for the scope lifetime) and the legacy sync <see cref="BaseRepository"/>-based
/// <c>ContactMechanismUsageTypeRepository</c> which silently swallowed all exceptions
/// and returned <c>null</c>.
/// </para>
/// <para><b>DI registration:</b> <c>Scoped</c>.</para>
/// </summary>
public sealed class ContactMechanismUsageTypeRepositoryAsync : IContactMechanismUsageTypeRepositoryAsync
{
    private readonly IDbConnectionFactory                              _dbFactory;
    private readonly ILogger<ContactMechanismUsageTypeRepositoryAsync> _logger;

    public ContactMechanismUsageTypeRepositoryAsync(
        IDbConnectionFactory                                dbFactory,
        ILogger<ContactMechanismUsageTypeRepositoryAsync>  logger)
    {
        _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
        _logger    = logger    ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task<IList<ContactMechanismUsageType>> ListContactMechanismUsageTypeAsync(
        string?           contactMechanismUsageTypeName,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = _dbFactory.CreateConnection();
            await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

            var result = await connection.QueryAsync<ContactMechanismUsageType>(
                new CommandDefinition(
                    StoredProcNameConstants.SP_ListContactMechanismUsageType,
                    new { ContactMechanismUsageTypeName = contactMechanismUsageTypeName },
                    commandType: CommandType.StoredProcedure,
                    cancellationToken: cancellationToken))
                .ConfigureAwait(false);

            return result.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Method} failed for UsageTypeName={Name}",
                nameof(ListContactMechanismUsageTypeAsync), contactMechanismUsageTypeName);
            return [];
        }
    }
}
