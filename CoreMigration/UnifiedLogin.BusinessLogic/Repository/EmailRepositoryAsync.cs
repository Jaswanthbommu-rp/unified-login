using System.Data;
using Dapper;
using Microsoft.Extensions.Logging;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Constants;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.Repository;

/// <summary>
/// Async-first Email Repository.
/// Uses injected <see cref="IDbConnection"/> (Dapper) directly —
/// no <see cref="BaseRepository"/> inheritance, no <c>new</c> keyword.
/// </summary>
public sealed class EmailRepositoryAsync : IEmailRepositoryAsync
{
    private readonly IDbConnection _db;
    private readonly ILogger<EmailRepositoryAsync> _logger;

    public EmailRepositoryAsync(
        IDbConnection db,
        ILogger<EmailRepositoryAsync> logger)
    {
        _db     = db     ?? throw new ArgumentNullException(nameof(db));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task<CommunicationEmail> GetEmailTemplateAsync(
        int communicationEventAudienceTypeId,
        int communicationEventPurposeTypeId,
        CancellationToken cancellationToken = default)
    {
        return await _db.QuerySingleOrDefaultAsync<CommunicationEmail>(
            new CommandDefinition(
                StoredProcNameConstants.SP_ListCommunicationEmailTemplates,
                new { communicationEventAudienceTypeId, communicationEventPurposeTypeId },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));
    }

    /// <inheritdoc/>
    public async Task<IList<CommunicationEmail>> ListEmailTemplatesAsync(
        CancellationToken cancellationToken = default)
    {
        var result = await _db.QueryAsync<CommunicationEmail>(
            new CommandDefinition(
                StoredProcNameConstants.SP_ListCommunicationEmailTemplates,
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));

        return result.ToList();
    }
}