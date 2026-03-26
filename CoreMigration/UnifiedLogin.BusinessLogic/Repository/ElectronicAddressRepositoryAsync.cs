using Dapper;
using System.Data;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Constants;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.Repository;

/// <summary>
/// Async-first electronic address repository using Dapper + <see cref="IDbConnectionFactory"/>.
/// Each method obtains its own connection from the factory so concurrent callers never share a connection.
/// The usage-type enrichment fetch runs concurrently with the main address fetch via <see cref="Task.WhenAll"/>.
/// </summary>
public sealed class ElectronicAddressRepositoryAsync : IElectronicAddressRepositoryAsync
{
    private readonly IDbConnectionFactory _dbFactory;

    public ElectronicAddressRepositoryAsync(IDbConnectionFactory dbFactory)
    {
        _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
    }

    /// <inheritdoc/>
    public async Task<RepositoryResponse> CreateElectronicAddressAsync(
        IElectronicAddress electronicAddress,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(electronicAddress);

        using var db = _dbFactory.CreateConnection();
        var result = await db.QuerySingleOrDefaultAsync<RepositoryResponse>(new CommandDefinition(
            StoredProcNameConstants.SP_CreateElectronicAddress,
            new
            {
                electronicAddress.ContactMechanismId,
                ElectronicAddressString = electronicAddress.AddressString,
                ElectronicAddressType   = electronicAddress.AddressType
            },
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken));

        return result ?? new RepositoryResponse();
    }

    /// <inheritdoc/>
    public async Task<IList<ElectronicAddress>> ListElectronicAddressForPersonAsync(
        Guid realPageId,
        string? contactMechanismUsageTypeName = null,
        CancellationToken cancellationToken = default)
    {
        // Both SP calls run concurrently — each uses its own factory connection.
        var addressTask    = FetchByRealPageIdAsync(realPageId, cancellationToken);
        var usageTypesTask = FetchUsageTypesAsync(contactMechanismUsageTypeName, cancellationToken);

        await Task.WhenAll(addressTask, usageTypesTask);

        return Enrich(await addressTask, await usageTypesTask);
    }

    /// <inheritdoc/>
    public async Task<IList<ElectronicAddress>> ListElectronicAddressForPersonAsync(
        string loginName,
        long orgPartyId,
        string? contactMechanismUsageTypeName = null,
        CancellationToken cancellationToken = default)
    {
        var addressTask    = FetchByLoginNameAsync(loginName, orgPartyId, cancellationToken);
        var usageTypesTask = FetchUsageTypesAsync(contactMechanismUsageTypeName, cancellationToken);

        await Task.WhenAll(addressTask, usageTypesTask);

        return Enrich(await addressTask, await usageTypesTask);
    }

    // ── Private helpers ───────────────────────────────────────────────────

    private async Task<List<ElectronicAddress>> FetchByRealPageIdAsync(
        Guid realPageId, CancellationToken cancellationToken)
    {
        using var db = _dbFactory.CreateConnection();
        var result = await db.QueryAsync<ElectronicAddress>(new CommandDefinition(
            StoredProcNameConstants.SP_ListEmailsForPerson,
            new { realPageId },
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken));
        return result.ToList();
    }

    private async Task<List<ElectronicAddress>> FetchByLoginNameAsync(
        string loginName, long orgPartyId, CancellationToken cancellationToken)
    {
        using var db = _dbFactory.CreateConnection();
        var result = await db.QueryAsync<ElectronicAddress>(new CommandDefinition(
            StoredProcNameConstants.SP_GetNotificationEmailForPerson,
            new { loginName, orgPartyId },
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken));
        return result.ToList();
    }

    private async Task<List<ContactMechanismUsageType>> FetchUsageTypesAsync(
        string? contactMechanismUsageTypeName, CancellationToken cancellationToken)
    {
        using var db = _dbFactory.CreateConnection();
        var result = await db.QueryAsync<ContactMechanismUsageType>(new CommandDefinition(
            StoredProcNameConstants.SP_ListContactMechanismUsageType,
            new { ContactMechanismUsageTypeName = contactMechanismUsageTypeName },
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken));
        return result.ToList();
    }

    private static IList<ElectronicAddress> Enrich(
        List<ElectronicAddress> addresses,
        List<ContactMechanismUsageType> usageTypes)
    {
        foreach (var address in addresses)
        {
            var usageType = usageTypes.FirstOrDefault(
                u => u.ContactMechanismUsageTypeId == address.ContactMechanismUsageTypeId);
            if (usageType is not null)
                address.contactMechanismUsageType = usageType;
        }
        return addresses;
    }
}