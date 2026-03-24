using System.Data;
using Dapper;
using Microsoft.Extensions.Logging;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Constants;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.Repository;

/// <summary>
/// Async-first Status Type Repository.
/// Uses injected <see cref="IDbConnection"/> (Dapper) directly —
/// no <see cref="BaseRepository"/> inheritance, no <c>new</c> keyword.
/// </summary>
public sealed class StatusTypeRepositoryAsync : IStatusTypeRepositoryAsync
{
    private readonly IDbConnection _db;
    private readonly ILogger<StatusTypeRepositoryAsync> _logger;

    public StatusTypeRepositoryAsync(IDbConnection db, ILogger<StatusTypeRepositoryAsync> logger)
    {
        _db     = db     ?? throw new ArgumentNullException(nameof(db));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="categoryTypeName"/> or <paramref name="categoryName"/> is blank.
    /// Replaces: <see langword="throw"/> <see cref="Exception"/> in the original.
    /// </exception>
    public async Task<IList<StatusType>> GetStatusTypeAsync(
        string categoryTypeName,
        string categoryName,
        CancellationToken cancellationToken = default)
    {
        // Replaces: throw new Exception("...") → throw ArgumentException (more specific)
        if (string.IsNullOrWhiteSpace(categoryTypeName))
            throw new ArgumentException("Invalid Category TypeName.", nameof(categoryTypeName));

        if (string.IsNullOrWhiteSpace(categoryName))
            throw new ArgumentException("Invalid Category name.", nameof(categoryName));

        var result = await _db.QueryAsync<StatusType>(
            new CommandDefinition(
                StoredProcNameConstants.SP_GetStatusTypes,
                new { StatusTypeCategoryTypeName = categoryTypeName, StatusTypeCategoryName = categoryName },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));

        return result.ToList();
    }
}