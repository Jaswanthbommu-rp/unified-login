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
/// Async-first Password Policy Repository.
/// Uses injected <see cref="IDbConnection"/> (Dapper) directly —
/// no <see cref="BaseRepository"/> inheritance, no <c>new</c> keyword.
/// </summary>
public sealed class PasswordPolicyRepositoryAsync : IPasswordPolicyRepositoryAsync
{
    private readonly IDbConnection _db;
    private readonly ILogger<PasswordPolicyRepositoryAsync> _logger;

    public PasswordPolicyRepositoryAsync(
        IDbConnection db,
        ILogger<PasswordPolicyRepositoryAsync> logger)
    {
        _db     = db     ?? throw new ArgumentNullException(nameof(db));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task<PasswordPolicy> GetPasswordPolicyAsync(
        long partyId,
        CancellationToken cancellationToken = default)
    {
        // Replaces: private GetUnifiedSettings() + inline mapping
        var settings = (await _db.QueryAsync<Setting>(
            new CommandDefinition(
                StoredProcNameConstants.SP_GetUnifiedSetting,
                new { PartyId = partyId, Category = "Security" },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken))).ToList();

        return new PasswordPolicy
        {
            PartyId                       = partyId,
            MinimumLength                 = byte.Parse(settings.First(s => s.Name == "MinimumLength").Value),
            MaximumLength                 = byte.Parse(settings.First(s => s.Name == "MaximumLength").Value),
            MinimumLowercase              = byte.Parse(settings.First(s => s.Name == "MinimumLowercase").Value),
            MinimumUppercase              = byte.Parse(settings.First(s => s.Name == "MinimumUppercase").Value),
            MinimumNumeric                = byte.Parse(settings.First(s => s.Name == "MinimumNumeric").Value),
            MinimumSpecialCharacter       = byte.Parse(settings.First(s => s.Name == "MinimumSpecialCharacter").Value),
            EnablePasswordExpiration      = settings.First(s => s.Name == "EnablePasswordExpiration").Value == "1",
            PasswordExpirationPeriodInDays = short.Parse(settings.First(s => s.Name == "PasswordExpirationPeriodInDays").Value),
            PreventPasswordReuse          = settings.First(s => s.Name == "PreventPasswordReuse").Value == "1",
            NumberOfPasswordsToRemember   = byte.Parse(settings.First(s => s.Name == "NumberOfPasswordsToRemember").Value),
            AllowUsersToChangeOwnPassword = settings.First(s => s.Name == "AllowUsersToChangeOwnPassword").Value == "1"
        };
    }

    /// <inheritdoc/>
    public async Task<RepositoryResponse> UpdatePasswordPolicyAsync(
        IPasswordPolicy passwordPolicy,
        CancellationToken cancellationToken = default)
    {
        return await _db.QuerySingleOrDefaultAsync<RepositoryResponse>(
            new CommandDefinition(
                StoredProcNameConstants.SP_UpdatePasswordPolicy,
                BuildPolicyParam(passwordPolicy),
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken))
            ?? new RepositoryResponse();
    }

    /// <inheritdoc/>
    public async Task<RepositoryResponse> CreatePasswordPolicyAsync(
        IPasswordPolicy passwordPolicy,
        CancellationToken cancellationToken = default)
    {
        return await _db.QuerySingleOrDefaultAsync<RepositoryResponse>(
            new CommandDefinition(
                StoredProcNameConstants.SP_CreatePasswordPolicy,
                BuildPolicyParam(passwordPolicy),
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken))
            ?? new RepositoryResponse();
    }

    // Replaces: duplicated dynamic param blocks in Update and Create
    private static object BuildPolicyParam(IPasswordPolicy p) => new
    {
        p.PasswordPolicyId,
        p.PartyId,
        p.MinimumLength,
        p.MaximumLength,
        p.MinimumLowercase,
        p.MinimumUppercase,
        p.MinimumNumeric,
        p.MinimumSpecialCharacter,
        p.AllowUsersToChangeOwnPassword,
        p.EnablePasswordExpiration,
        p.PasswordExpirationPeriodInDays,
        p.PreventPasswordReuse,
        p.NumberOfPasswordsToRemember,
        p.UserId
    };
}