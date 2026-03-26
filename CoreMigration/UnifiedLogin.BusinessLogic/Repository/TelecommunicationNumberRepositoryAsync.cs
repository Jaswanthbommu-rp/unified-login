using Dapper;
using System.Data;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Constants;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.Repository;

/// <summary>
/// Async-first telecommunication number repository using Dapper + <see cref="IDbConnectionFactory"/>.
/// Each method obtains its own connection from the factory so concurrent callers never share a connection.
/// </summary>
public sealed class TelecommunicationNumberRepositoryAsync : ITelecommunicationNumberRepositoryAsync
{
    private readonly IDbConnectionFactory _dbFactory;

    public TelecommunicationNumberRepositoryAsync(IDbConnectionFactory dbFactory)
    {
        _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
    }

    /// <inheritdoc/>
    public async Task<RepositoryResponse> CreateTelecommunicationNumberAsync(
        ITelecommunicationNumber telecommunicationNumber,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(telecommunicationNumber);

        using var db = _dbFactory.CreateConnection();
        var result = await db.QuerySingleOrDefaultAsync<RepositoryResponse>(new CommandDefinition(
            StoredProcNameConstants.SP_CreateTelecommunicationNumber,
            new
            {
                ContactMechanismId = telecommunicationNumber.ContactMechanismId,
                AreaCode           = telecommunicationNumber.AreaCode,
                CountryCode        = telecommunicationNumber.CountryCode,
                PhoneNumber        = telecommunicationNumber.PhoneNumber,
                ISOCode            = telecommunicationNumber.ISOCode,
                Default            = telecommunicationNumber.IsDefault
            },
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken));

        return result ?? new RepositoryResponse();
    }

    /// <inheritdoc/>
    public async Task<IList<TelecommunicationNumber>> ListTelecommunicationNumberForPersonAsync(
        Guid realPageId,
        string? contactMechanismUsageTypeName = null,
        CancellationToken cancellationToken = default)
    {
        // Run both SP calls concurrently — each uses its own connection from the factory.
        var numbersTask = FetchNumbersAsync(realPageId, cancellationToken);
        var usageTypesTask = FetchUsageTypesAsync(contactMechanismUsageTypeName, cancellationToken);

        await Task.WhenAll(numbersTask, usageTypesTask);

        var numbers    = await numbersTask;
        var usageTypes = await usageTypesTask;

        foreach (var number in numbers)
        {
            var usageType = usageTypes.FirstOrDefault(
                u => u.ContactMechanismUsageTypeId == number.ContactMechanismUsageTypeId);
            if (usageType is not null)
                number.contactMechanismUsageType = usageType;
        }

        return numbers;
    }

    // ── Private helpers ───────────────────────────────────────────────────

    private async Task<List<TelecommunicationNumber>> FetchNumbersAsync(
        Guid realPageId, CancellationToken cancellationToken)
    {
        using var db = _dbFactory.CreateConnection();
        var result = await db.QueryAsync<TelecommunicationNumber>(new CommandDefinition(
            StoredProcNameConstants.SP_ListTelecommunicationNumbersForPerson,
            new { realPageId },
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
}