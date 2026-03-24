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
/// Async-first Telecommunication Number Repository.
/// Uses injected <see cref="IDbConnection"/> (Dapper) directly —
/// no <see cref="BaseRepository"/> inheritance, no <c>new</c> keyword.
/// </summary>
public sealed class TelecommunicationNumberRepositoryAsync : ITelecommunicationNumberRepositoryAsync
{
    private readonly IDbConnection _db;
    private readonly IContactMechanismUsageTypeRepositoryAsync _usageTypeRepository;
    private readonly ILogger<TelecommunicationNumberRepositoryAsync> _logger;

    public TelecommunicationNumberRepositoryAsync(
        IDbConnection db,
        IContactMechanismUsageTypeRepositoryAsync usageTypeRepository,
        ILogger<TelecommunicationNumberRepositoryAsync> logger)
    {
        _db                  = db                  ?? throw new ArgumentNullException(nameof(db));
        _usageTypeRepository = usageTypeRepository ?? throw new ArgumentNullException(nameof(usageTypeRepository));
        _logger              = logger              ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task<RepositoryResponse> CreateTelecommunicationNumberAsync(
        ITelecommunicationNumber telecommunicationNumber,
        CancellationToken cancellationToken = default)
    {
        return await _db.QuerySingleOrDefaultAsync<RepositoryResponse>(
            new CommandDefinition(
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
                cancellationToken: cancellationToken))
            ?? new RepositoryResponse();
    }

    /// <inheritdoc/>
    public async Task<IList<TelecommunicationNumber>> ListTelecommunicationNumberForPersonAsync(
        Guid realPageId,
        string contactMechanismUsageTypeName = "",
        CancellationToken cancellationToken = default)
    {
        // Replaces: new ContactMechanismUsageTypeRepository() + sequential sync calls
        var phoneRowsTask  = _db.QueryAsync<TelecommunicationNumber>(
            new CommandDefinition(
                StoredProcNameConstants.SP_ListTelecommunicationNumbersForPerson,
                new { realPageId },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));

        var usageTypesTask = _usageTypeRepository
            .ListContactMechanismUsageTypeAsync(contactMechanismUsageTypeName, cancellationToken);

        // Both queries are independent — run concurrently
        await Task.WhenAll(phoneRowsTask, usageTypesTask);

        var phones     = (await phoneRowsTask).ToList();
        var usageTypes = await usageTypesTask;

        // Replaces: .First(...) that throws → .FirstOrDefault(...) that is safe
        foreach (var phone in phones)
        {
            var usageType = usageTypes
                .FirstOrDefault(u => u.ContactMechanismUsageTypeId == phone.ContactMechanismUsageTypeId);

            if (usageType is not null)
                phone.contactMechanismUsageType = usageType;
        }

        return phones;
    }
}