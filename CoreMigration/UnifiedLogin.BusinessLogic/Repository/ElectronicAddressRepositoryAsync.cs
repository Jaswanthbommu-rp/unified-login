using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dapper;
using Microsoft.Extensions.Logging;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Constants;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.Repository;

/// <summary>
/// Async-first Electronic Address Repository.
/// Uses injected <see cref="IDbConnection"/> (Dapper) directly —
/// no <see cref="BaseRepository"/> inheritance, no <c>new</c> keyword.
/// </summary>
public sealed class ElectronicAddressRepositoryAsync : IElectronicAddressRepositoryAsync
{
    #region Fields

    private readonly IDbConnection _db;
    private readonly IContactMechanismUsageTypeRepositoryAsync _usageTypeRepository;
    private readonly ILogger<ElectronicAddressRepositoryAsync> _logger;

    #endregion

    #region Constructor

    /// <summary>
    /// Primary DI constructor — all dependencies injected, no <c>new</c>.
    /// </summary>
    public ElectronicAddressRepositoryAsync(
        IDbConnection db,
        IContactMechanismUsageTypeRepositoryAsync usageTypeRepository,
        ILogger<ElectronicAddressRepositoryAsync> logger)
    {
        _db                  = db                  ?? throw new ArgumentNullException(nameof(db));
        _usageTypeRepository = usageTypeRepository ?? throw new ArgumentNullException(nameof(usageTypeRepository));
        _logger              = logger              ?? throw new ArgumentNullException(nameof(logger));
    }

    #endregion

    #region IElectronicAddressRepositoryAsync Implementation

    /// <inheritdoc/>
    public async Task<RepositoryResponse> CreateElectronicAddressAsync(
        IElectronicAddress electronicAddress,
        CancellationToken cancellationToken = default)
    {
        return await _db.QuerySingleOrDefaultAsync<RepositoryResponse>(
            new CommandDefinition(
                StoredProcNameConstants.SP_CreateElectronicAddress,
                new
                {
                    electronicAddress.ContactMechanismId,
                    ElectronicAddressString = electronicAddress.AddressString,
                    ElectronicAddressType   = electronicAddress.AddressType
                },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken))
            ?? new RepositoryResponse();
    }

    /// <inheritdoc/>
    public async Task<IList<ElectronicAddress>> ListElectronicAddressForPersonAsync(
        Guid realPageId,
        string contactMechanismUsageTypeName = "",
        CancellationToken cancellationToken = default)
    {
        // Replaces: new ContactMechanismUsageTypeRepository() + sequential calls.
        // Both queries start together; results are joined in memory.
        var addressesTask   = _db.QueryAsync<ElectronicAddress>(
            new CommandDefinition(
                StoredProcNameConstants.SP_ListEmailsForPerson,
                new { realPageId },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));

        var usageTypesTask = _usageTypeRepository
            .ListContactMechanismUsageTypeAsync(contactMechanismUsageTypeName, cancellationToken);

        await Task.WhenAll(addressesTask, usageTypesTask);

        var addresses  = (await addressesTask).ToList();
        var usageTypes = await usageTypesTask;

        return EnrichWithUsageType(addresses, usageTypes);
    }

    /// <inheritdoc/>
    public async Task<IList<ElectronicAddress>> ListElectronicAddressForPersonAsync(
        string loginName,
        long orgPartyId,
        string contactMechanismUsageTypeName = "",
        CancellationToken cancellationToken = default)
    {
        // Replaces: new ContactMechanismUsageTypeRepository() instantiated inside the method
        var addressesTask  = _db.QueryAsync<ElectronicAddress>(
            new CommandDefinition(
                StoredProcNameConstants.SP_GetNotificationEmailForPerson,
                new { loginName, orgPartyId },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));

        var usageTypesTask = _usageTypeRepository
            .ListContactMechanismUsageTypeAsync(contactMechanismUsageTypeName, cancellationToken);

        await Task.WhenAll(addressesTask, usageTypesTask);

        var addresses  = (await addressesTask).ToList();
        var usageTypes = await usageTypesTask;

        return EnrichWithUsageType(addresses, usageTypes);
    }

    #endregion

    #region Private Helpers

    /// <summary>
    /// Joins addresses with their usage type metadata.
    /// Replaces: foreach + <c>First()</c> (throws on miss) →
    ///           <c>FirstOrDefault()</c> (safe) + null guard.
    /// </summary>
    private static IList<ElectronicAddress> EnrichWithUsageType(
        IList<ElectronicAddress> addresses,
        IList<ContactMechanismUsageType> usageTypes)
    {
        foreach (var address in addresses)
        {
            var usageType = usageTypes
                .FirstOrDefault(u => u.ContactMechanismUsageTypeId == address.ContactMechanismUsageTypeId);

            if (usageType is not null)
                address.contactMechanismUsageType = usageType;
        }

        return addresses;
    }

    #endregion
}