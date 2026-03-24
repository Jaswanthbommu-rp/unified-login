using System;
using System.Collections.Generic;
using System.Linq;
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
/// Async-first Contact Mechanism Repository.
/// Uses injected <see cref="IDbConnection"/> (Dapper) directly —
/// no <see cref="BaseRepository"/> inheritance, no <c>new</c> keyword.
/// </summary>
public sealed class ContactMechanismRepositoryAsync : IContactMechanismRepositoryAsync
{
    #region Fields

    private readonly IDbConnection _db;
    private readonly ILogger<ContactMechanismRepositoryAsync> _logger;

    #endregion

    #region Constructor

    /// <summary>
    /// Primary DI constructor — all dependencies injected, no <c>new</c>.
    /// </summary>
    public ContactMechanismRepositoryAsync(
        IDbConnection db,
        ILogger<ContactMechanismRepositoryAsync> logger)
    {
        _db     = db     ?? throw new ArgumentNullException(nameof(db));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #endregion

    #region IContactMechanismRepositoryAsync Implementation

    /// <inheritdoc/>
    public async Task<RepositoryResponse> CreateContactMechanismAsync(
        CancellationToken cancellationToken = default)
    {
        return await _db.QuerySingleOrDefaultAsync<RepositoryResponse>(
            new CommandDefinition(
                StoredProcNameConstants.SP_CreateContactMechanism,
                new { ContactMechanismId = 0 },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken))
            ?? new RepositoryResponse();
    }

    /// <inheritdoc/>
    public async Task<IList<CommonAddress>> ListContactMechanismForPersonAsync(
        Guid realPageId,
        string ContactMechanismUsageTypeName,
        CancellationToken cancellationToken = default)
    {
        // Replaces: new ContactMechanismUsageTypeRepository() + sequential calls.
        // Both queries run concurrently against the same connection.
        var contactsTask = _db.QueryAsync<CommonAddress>(
            new CommandDefinition(
                StoredProcNameConstants.SP_ListContactMechanismsForPerson,
                new { realPageId },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));

        var usageTypesTask = _db.QueryAsync<ContactMechanismUsageType>(
            new CommandDefinition(
                StoredProcNameConstants.SP_ListContactMechanismUsageType,
                new { ContactMechanismUsageTypeName },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));

        await Task.WhenAll(contactsTask, usageTypesTask);

        var contacts   = (await contactsTask).ToList();
        var usageTypes = (await usageTypesTask).ToList();

        foreach (var contact in contacts)
        {
            var usageType = usageTypes
                .FirstOrDefault(u => u.ContactMechanismUsageTypeId == contact.ContactMechanismUsageTypeId);

            if (usageType is not null)
                contact.contactMechanismUsageType = usageType;
        }

        return contacts;
    }

    /// <inheritdoc/>
    public async Task<RepositoryResponse> LinkContactMechanismToPartyAsync(
        Guid realPageId,
        IPartyContactMechanism partyContactMechanism,
        CancellationToken cancellationToken = default)
    {
        return await _db.QuerySingleOrDefaultAsync<RepositoryResponse>(
            new CommandDefinition(
                StoredProcNameConstants.SP_LinkContactMechanismToParty,
                new
                {
                    realPageId,
                    partyContactMechanism.PartyContactMechanismId,
                    partyContactMechanism.ContactMechanismId,
                    partyContactMechanism.FromDate,
                    partyContactMechanism.ThruDate
                },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken))
            ?? new RepositoryResponse();
    }

    /// <inheritdoc/>
    public async Task<RepositoryResponse> LinkGeographicBoundaryToContactMechanismAsync(
        IContactMechanismBoundary contactMechanismBoundary,
        CancellationToken cancellationToken = default)
    {
        return await _db.QuerySingleOrDefaultAsync<RepositoryResponse>(
            new CommandDefinition(
                StoredProcNameConstants.SP_LinkGeographicBoundaryToContactMechanism,
                new
                {
                    contactMechanismBoundary.ContactMechanismId,
                    contactMechanismBoundary.GeographicBoundaryId,
                    contactMechanismBoundary.FromDate,
                    contactMechanismBoundary.ThruDate
                },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken))
            ?? new RepositoryResponse();
    }

    /// <inheritdoc/>
    public async Task<RepositoryResponse> LinkUsageTypeToPartyContactMechanismAsync(
        long PartyContactMechanismID,
        int? ContactMechanismUsageTypeId,
        CancellationToken cancellationToken = default)
    {
        return await _db.QuerySingleOrDefaultAsync<RepositoryResponse>(
            new CommandDefinition(
                StoredProcNameConstants.SP_LinkUsageTypeToPartyContactMechanism,
                new { PartyContactMechanismID, ContactMechanismUsageTypeId },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken))
            ?? new RepositoryResponse();
    }

    /// <inheritdoc/>
    public async Task<RepositoryResponse> UpdateContactMechanismUsageForPartyAsync(
        long PartyContactMechanismID,
        int? ContactMechanismUsageTypeId,
        CancellationToken cancellationToken = default)
    {
        return await _db.QuerySingleOrDefaultAsync<RepositoryResponse>(
            new CommandDefinition(
                StoredProcNameConstants.SP_UpdateContactMechanismUsageForParty,
                new { PartyContactMechanismID, ContactMechanismUsageTypeId },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken))
            ?? new RepositoryResponse();
    }

    #endregion
}