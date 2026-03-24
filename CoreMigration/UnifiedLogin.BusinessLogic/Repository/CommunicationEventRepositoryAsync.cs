using System;
using System.Data;
using Dapper;
using Microsoft.Extensions.Logging;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Constants;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.Repository;

/// <summary>
/// Async-first Communication Event Repository.
/// Uses injected <see cref="IDbConnection"/> (Dapper) directly —
/// no <see cref="BaseRepository"/> inheritance, no <c>new</c> keyword.
/// </summary>
public sealed class CommunicationEventRepositoryAsync : ICommunicationEventRepositoryAsync
{
    #region Fields

    private readonly IDbConnection _db;
    private readonly ILogger<CommunicationEventRepositoryAsync> _logger;

    #endregion

    #region Constructor

    /// <summary>
    /// Primary DI constructor — all dependencies injected, no <c>new</c>.
    /// </summary>
    public CommunicationEventRepositoryAsync(
        IDbConnection db,
        ILogger<CommunicationEventRepositoryAsync> logger)
    {
        _db     = db     ?? throw new ArgumentNullException(nameof(db));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #endregion

    #region ICommunicationEventRepositoryAsync Implementation

    /// <inheritdoc/>
    public async Task<RepositoryResponse> CreateCommunicationEventPurposeTypeAsync(
        string description,
        CancellationToken cancellationToken = default)
    {
        return await _db.QuerySingleOrDefaultAsync<RepositoryResponse>(
            new CommandDefinition(
                StoredProcNameConstants.SP_CreateCommunicationEventPurposeType,
                new { description },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken))
            ?? new RepositoryResponse();
    }

    /// <inheritdoc/>
    public async Task<RepositoryResponse> CreateCommunicationEventAsync(
        int statusTypeId,
        long fromPartyContactMechanismId,
        long toPartyContactMechanismId,
        DateTime started,
        DateTime ended,
        string note,
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
            communicationEventId = (long?)null   // replaces: long? communicationEventId = null
        };

        return await _db.QuerySingleOrDefaultAsync<RepositoryResponse>(
            new CommandDefinition(
                StoredProcNameConstants.SP_CreateCommunicationEvent,
                param,
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken))
            ?? new RepositoryResponse();
    }

    /// <inheritdoc/>
    public async Task<RepositoryResponse> CreateCommunicationEventEmailAsync(
        int communicationEmailTemplateId,
        long communicationEventId,
        CancellationToken cancellationToken = default)
    {
        var param = new
        {
            communicationEmailTemplateId,
            communicationEventId,
            communicationEventEmailId = (long?)null  // replaces: long? communicationEventEmailId = null
        };

        return await _db.QuerySingleOrDefaultAsync<RepositoryResponse>(
            new CommandDefinition(
                StoredProcNameConstants.SP_CreateCommunicationEventEmail,
                param,
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken))
            ?? new RepositoryResponse();
    }

    /// <inheritdoc/>
    public async Task<RepositoryResponse> CreateCESCommunicationEventEmailAsync(
        string cesId,
        long communicationEventId,
        CancellationToken cancellationToken = default)
    {
        var param = new
        {
            cesId,
            communicationEventId,
            cesCommunicationEventId = (long?)null   // replaces: long? cesCommunicationEventId = null
        };

        return await _db.QuerySingleOrDefaultAsync<RepositoryResponse>(
            new CommandDefinition(
                StoredProcNameConstants.SP_CreateCESCommunicationEvent,
                param,
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken))
            ?? new RepositoryResponse();
    }

    #endregion
}