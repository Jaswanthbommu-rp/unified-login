using System.Data;
using Dapper;
using Microsoft.Extensions.Logging;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Constants;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Saml;

namespace UnifiedLogin.BusinessLogic.Repository;

/// <summary>
/// Async-first SAML Repository.
/// Uses injected <see cref="IDbConnection"/> (Dapper) directly —
/// no <see cref="BaseRepository"/> inheritance, no <c>new</c> keyword.
/// </summary>
public sealed class SamlRepositoryAsync : ISamlRepositoryAsync
{
    private readonly IDbConnection _db;
    private readonly ILogger<SamlRepositoryAsync> _logger;

    public SamlRepositoryAsync(IDbConnection db, ILogger<SamlRepositoryAsync> logger)
    {
        _db     = db     ?? throw new ArgumentNullException(nameof(db));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task<IList<SamlAttributes>> GetProductSamlDetailsAsync(
        long personaId, int productId, CancellationToken cancellationToken = default)
    {
        var result = await _db.QueryAsync<SamlAttributes>(
            new CommandDefinition(
                StoredProcNameConstants.SP_GetProductSamlDetails,
                new { PersonaId = personaId, ProductId = productId },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));
        return result.ToList();
    }

    /// <inheritdoc/>
    public async Task<IList<PersonaProductUserDetails>> ListActiveProductsByPersonaIdAsync(
        long personaId, int productId, string productType, CancellationToken cancellationToken = default)
    {
        var result = await _db.QueryAsync<PersonaProductUserDetails>(
            new CommandDefinition(
                StoredProcNameConstants.SP_ListProductsByPersonaId,
                new { PersonaId = personaId, ProductStatusValue = ((int)UserUiStatusType.AccountCreationSuccessful).ToString() },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));
        return result.ToList();
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Replaces: personaId range-guard + no-parameter fallback in sync version.
    /// </remarks>
    public async Task<IList<PersonaProductUserDetails>> ListAllProductsByPersonaIdAsync(
        long personaId, int productId, string productType, CancellationToken cancellationToken = default)
    {
        if (personaId > int.MaxValue || personaId < int.MinValue)
            return [];

        var result = await _db.QueryAsync<PersonaProductUserDetails>(
            new CommandDefinition(
                StoredProcNameConstants.SP_ListProductsByPersonaId,
                new { PersonaId = personaId },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));
        return result.ToList();
    }

    /// <inheritdoc/>
    public async Task<RepositoryResponse> CreateSamlUserAttributeAsync(
        long personaId, int productId, SamlAttributeEnum samlAttributeId, string value,
        CancellationToken cancellationToken = default)
    {
        return await _db.QuerySingleOrDefaultAsync<RepositoryResponse>(
            new CommandDefinition(
                StoredProcNameConstants.SP_CreateSamlUserAttribute,
                new { PersonaId = personaId, ProductId = productId, SamlAttributeId = (int)samlAttributeId, Value = value },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken))
            ?? new RepositoryResponse();
    }

    /// <inheritdoc/>
    public async Task<RepositoryResponse> UpdateSamlUserAttributeAsync(
        SamlAttributes samlAttributes, CancellationToken cancellationToken = default)
    {
        return await _db.QuerySingleOrDefaultAsync<RepositoryResponse>(
            new CommandDefinition(
                StoredProcNameConstants.SP_UpdateSamlUserAttribute,
                new { SamlUserAttributeId = (int)samlAttributes.SamlUserAttributeId, Value = samlAttributes.Value },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken))
            ?? new RepositoryResponse();
    }

    /// <inheritdoc/>
    public async Task<RepositoryResponse> DeleteSamlUserProductInfoAndStatusAsync(
        long personaId, int productId, CancellationToken cancellationToken = default)
    {
        return await _db.QuerySingleOrDefaultAsync<RepositoryResponse>(
            new CommandDefinition(
                StoredProcNameConstants.SP_DeleteSamlUserProductInfoAndStatus,
                new { PersonaId = personaId, ProductId = productId },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken))
            ?? new RepositoryResponse();
    }

    /// <inheritdoc/>
    public async Task<IList<ProductSamlDetails>> ListPersonaProductsSamlDetailsAsync(
        long personaId, CancellationToken cancellationToken = default)
    {
        var result = await _db.QueryAsync<ProductSamlDetails>(
            new CommandDefinition(
                StoredProcNameConstants.SP_ListPersonaProductsSamlDetails,
                new { PersonaId = personaId },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));
        return result.ToList();
    }

    /// <inheritdoc/>
    public async Task<RepositoryResponse> DeletePersonaProductErrorAsync(
        long personaId, CancellationToken cancellationToken = default)
    {
        return await _db.QuerySingleOrDefaultAsync<RepositoryResponse>(
            new CommandDefinition(
                StoredProcNameConstants.SP_ManagePersonaProductError,
                new { PersonaId = personaId },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken))
            ?? new RepositoryResponse();
    }

    /// <inheritdoc/>
    public async Task<IList<SamlProductAttributes>> GetSamlProductAttributesAsync(
        int productId, CancellationToken cancellationToken = default)
    {
        var result = await _db.QueryAsync<SamlProductAttributes>(
            new CommandDefinition(
                StoredProcNameConstants.SP_GetSamlProductAttributes,
                new { ProductId = productId },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));
        return result.ToList();
    }

    /// <inheritdoc/>
    public async Task<ProductSamlSettings> GetProductSamlSettingsByProductIdAsync(
        int productId, CancellationToken cancellationToken = default)
    {
        return await _db.QuerySingleOrDefaultAsync<ProductSamlSettings>(
            new CommandDefinition(
                StoredProcNameConstants.SP_GetProductSamlSettings,
                new { productId },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));
    }

    /// <inheritdoc/>
    public async Task<RepositoryResponse> RemoveSamlUserAttributeBySamlAttributeIdAsync(
        long personaId, int productId, SamlAttributeEnum samlAttributeId,
        CancellationToken cancellationToken = default)
    {
        return await _db.QuerySingleOrDefaultAsync<RepositoryResponse>(
            new CommandDefinition(
                StoredProcNameConstants.SP_RemoveSamlUserAttributeBySamlAttributeId,
                new { PersonaId = personaId, ProductId = productId, SamlAttributeId = (int)samlAttributeId },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken))
            ?? new RepositoryResponse();
    }
}