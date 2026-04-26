using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects.Constants;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.Repository;

/// <summary>
/// Async-first Communication Event Repository.
/// <para>
/// Uses <see cref="IDbConnectionFactory"/> to create short-lived <see cref="SqlConnection"/>
/// instances per call so connections are returned to the ADO.NET pool immediately after
/// each stored-procedure call — no long-lived connection is held on the instance.
/// </para>
/// <para>
/// Replaces the previous implementation that injected a single <see cref="IDbConnection"/>
/// (holding it for the lifetime of the scope) and the legacy <see cref="BaseRepository"/>-based
/// <c>CommunicationEventRepository</c> which used synchronous ADO calls.
/// </para>
/// <para><b>DI registration:</b> <c>Scoped</c>.</para>
/// </summary>
public sealed class CommunicationEventRepositoryAsync : ICommunicationEventRepositoryAsync
{
    private readonly IDbConnectionFactory                          _dbFactory;
    private readonly ILogger<CommunicationEventRepositoryAsync>   _logger;

    public CommunicationEventRepositoryAsync(
        IDbConnectionFactory                        dbFactory,
        ILogger<CommunicationEventRepositoryAsync>  logger)
    {
        _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
        _logger    = logger    ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task<RepositoryResponse> CreateCommunicationEventPurposeTypeAsync(
        string description,
        CancellationToken cancellationToken = default)
    {
        await using var connection = _dbFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

        return await connection.QuerySingleOrDefaultAsync<RepositoryResponse>(
            new CommandDefinition(
                StoredProcNameConstants.SP_CreateCommunicationEventPurposeType,
                new { description },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken))
            .ConfigureAwait(false)
            ?? new RepositoryResponse();
    }

    /// <inheritdoc/>
    public async Task<RepositoryResponse> CreateCommunicationEventAsync(
        int      statusTypeId,
        long     fromPartyContactMechanismId,
        long     toPartyContactMechanismId,
        DateTime started,
        DateTime ended,
        string   note,
        CancellationToken cancellationToken = default)
    {
        var param = new
        {
            statusTypeId,
            fromPartyContactMechanismId,
            toPartyContactMechanismId,
            started,
            ended,
            note,
            communicationEventId = (long?)null
        };

        await using var connection = _dbFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

        return await connection.QuerySingleOrDefaultAsync<RepositoryResponse>(
            new CommandDefinition(
                StoredProcNameConstants.SP_CreateCommunicationEvent,
                param,
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken))
            .ConfigureAwait(false)
            ?? new RepositoryResponse();
    }

    /// <inheritdoc/>
    public async Task<RepositoryResponse> CreateCommunicationEventEmailAsync(
        int  communicationEmailTemplateId,
        long communicationEventId,
        CancellationToken cancellationToken = default)
    {
        var param = new
        {
            communicationEmailTemplateId,
            communicationEventId,
            communicationEventEmailId = (long?)null
        };

        await using var connection = _dbFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

        return await connection.QuerySingleOrDefaultAsync<RepositoryResponse>(
            new CommandDefinition(
                StoredProcNameConstants.SP_CreateCommunicationEventEmail,
                param,
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken))
            .ConfigureAwait(false)
            ?? new RepositoryResponse();
    }

    /// <inheritdoc/>
    public async Task<RepositoryResponse> CreateCESCommunicationEventEmailAsync(
        string cesId,
        long   communicationEventId,
        CancellationToken cancellationToken = default)
    {
        var param = new
        {
            cesId,
            communicationEventId,
            cesCommunicationEventId = (long?)null
        };

        await using var connection = _dbFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

        return await connection.QuerySingleOrDefaultAsync<RepositoryResponse>(
            new CommandDefinition(
                StoredProcNameConstants.SP_CreateCESCommunicationEvent,
                param,
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken))
            .ConfigureAwait(false)
            ?? new RepositoryResponse();
    }
}
