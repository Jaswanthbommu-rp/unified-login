using System.Data;
using Dapper;
using Microsoft.Extensions.Logging;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects.Constants;
using UnifiedLogin.SharedObjects.IdentityConfig;

namespace UnifiedLogin.BusinessLogic.Repository;

public sealed class PostalAddressRepositoryAsync : IPostalAddressRepositoryAsync
{
    private readonly IDbConnection _db;
    private readonly IContactMechanismUsageTypeRepositoryAsync _usageTypeRepository;
    private readonly ILogger<PostalAddressRepositoryAsync> _logger;

    public PostalAddressRepositoryAsync(
        IDbConnection db,
        IContactMechanismUsageTypeRepositoryAsync usageTypeRepository,
        ILogger<PostalAddressRepositoryAsync> logger)
    {
        _db                  = db                  ?? throw new ArgumentNullException(nameof(db));
        _usageTypeRepository = usageTypeRepository ?? throw new ArgumentNullException(nameof(usageTypeRepository));
        _logger              = logger              ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task<IList<PostalAddress>> ListPostalAddressForPersonAsync(
        Guid realPageId,
        string contactMechanismUsageTypeName,
        CancellationToken cancellationToken = default)
    {
        // Replaces: new ContactMechanismUsageTypeRepository() + sequential calls
        var addressesTask  = _db.QueryAsync<PostalAddress>(
            new CommandDefinition(
                StoredProcNameConstants.SP_ListPostalAddressesForPerson,
                new { realPageId },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));

        var usageTypesTask = _usageTypeRepository
            .ListContactMechanismUsageTypeAsync(contactMechanismUsageTypeName, cancellationToken);

        await Task.WhenAll(addressesTask, usageTypesTask);

        var addresses  = (await addressesTask).ToList();
        var usageTypes = await usageTypesTask;

        foreach (var address in addresses)
        {
            var usageType = usageTypes
                .FirstOrDefault(u => u.ContactMechanismUsageTypeId == address.ContactMechanismUsageTypeId);

            if (usageType is not null)
                address.contactMechanismUsageType = usageType;
        }

        return addresses;
    }
}