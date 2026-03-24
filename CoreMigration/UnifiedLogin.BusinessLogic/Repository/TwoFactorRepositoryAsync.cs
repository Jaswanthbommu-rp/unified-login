using System.Data;
using Dapper;
using Microsoft.Extensions.Logging;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects.Constants;

namespace UnifiedLogin.BusinessLogic.Repository;

public sealed class TwoFactorRepositoryAsync : ITwoFactorRepositoryAsync
{
    private readonly IDbConnection _db;
    private readonly ILogger<TwoFactorRepositoryAsync> _logger;

    public TwoFactorRepositoryAsync(IDbConnection db, ILogger<TwoFactorRepositoryAsync> logger)
    {
        _db     = db     ?? throw new ArgumentNullException(nameof(db));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task<int> ResetAuthenticatorKeyAsync(
        long userId, string authenticatorKey, CancellationToken cancellationToken = default)
    {
        return await _db.ExecuteAsync(
            new CommandDefinition(
                StoredProcNameConstants.SP_CreateUpdateUserTokenDetail,
                new { UserId = userId, LoginProvider = "AppAuth", Name = "AuthenticatorKey", Value = authenticatorKey },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));
    }

    /// <inheritdoc/>
    public async Task<int> UpdateUserTwoFactorStatusAsync(
        long userId, int status, CancellationToken cancellationToken = default)
    {
        return await _db.ExecuteAsync(
            new CommandDefinition(
                StoredProcNameConstants.SP_UpdateUserLoginTwoFactor,
                new { UserId = userId, Status = status },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));
    }
}