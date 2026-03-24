using System.Data;
using Dapper;
using Microsoft.Extensions.Logging;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Constants;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.Repository;

public sealed class PersonRepositoryAsync : IPersonRepositoryAsync
{
    private readonly IDbConnection _db;
    private readonly ILogger<PersonRepositoryAsync> _logger;

    public PersonRepositoryAsync(IDbConnection db, ILogger<PersonRepositoryAsync> logger)
    {
        _db     = db     ?? throw new ArgumentNullException(nameof(db));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task<RepositoryResponse> CreatePersonAsync(IPerson person, CancellationToken cancellationToken = default)
    {
        return await _db.QuerySingleOrDefaultAsync<RepositoryResponse>(
            new CommandDefinition(
                StoredProcNameConstants.SP_CreatePerson,
                new
                {
                    person.FirstName, person.MiddleName, person.LastName,
                    person.Title, person.Suffix,
                    person.PreferredContactMethodId, person.RealPageId
                },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken))
            ?? new RepositoryResponse();
    }

    /// <inheritdoc/>
    public async Task<Person> GetPersonAsync(Guid realPageId, CancellationToken cancellationToken = default)
    {
        return await _db.QuerySingleOrDefaultAsync<Person>(
            new CommandDefinition(
                StoredProcNameConstants.SP_GetPerson,
                new { realPageId },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));
    }

    /// <inheritdoc/>
    public async Task<RepositoryResponse> UpdatePersonAsync(Guid realPageId, IPerson person, CancellationToken cancellationToken = default)
    {
        return await _db.QuerySingleOrDefaultAsync<RepositoryResponse>(
            new CommandDefinition(
                StoredProcNameConstants.SP_UpdatePerson,
                new
                {
                    realPageId,
                    person.FirstName, person.MiddleName, person.LastName,
                    person.Title, person.Suffix, person.PreferredContactMethodId
                },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken))
            ?? new RepositoryResponse();
    }

    /// <inheritdoc/>
    public async Task<IList<TelecommunicationNumber>> GetPersonPhoneAsync(Guid realPageId, CancellationToken cancellationToken = default)
    {
        var result = await _db.QueryAsync<TelecommunicationNumber>(
            new CommandDefinition(
                StoredProcNameConstants.SP_ListTelecommunicationNumbersForPerson,
                new { realPageId },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));
        return result.ToList();
    }
}