using Microsoft.Extensions.Logging;
using UnifiedLogin.BusinessLogic.CacheHelper;
using UnifiedLogin.BusinessLogic.Logic.ProductIntegration.Helpers;
using UnifiedLogin.BusinessLogic.Logic.ProductIntegration.Model;
using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects.Audit.Common;
using UnifiedLogin.SharedObjects.Cache;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Extensions;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.LogicAsync.Product.Integrations.ProductImplementation;

/// <summary>
/// Native-async concrete implementation for the Lead Management product integration.
/// Replaces <c>LeadManagement</c> (sync).
/// <para>
/// <b>Overrides vs base class:</b>
/// <list type="bullet">
///   <item><see cref="ApplySuperUserDataAsync"/> — assigns role "18" (LeadAnalytics) or "17" (all other Lead products).</item>
///   <item><see cref="UpdateSamlUserAttributeAsync"/> — updates <c>productUsername</c> SAML attribute with email
///     when login name and email differ (GB-4715).</item>
///   <item><see cref="CreateUpdateProductUserAsync"/> — custom login-name format:
///     <c>{first[0]}{last}_{BooksProductCode}_{PersonaId}</c>.</item>
/// </list>
/// </para>
/// </summary>
public sealed class LeadManagementAsync : StandardV1ProductIntegrationAsync
{
    /// <summary>
    /// Initialises a new instance. Call <see cref="StandardV1ProductIntegrationAsync.InitAsync"/>
    /// before using any public methods.
    /// </summary>
    public LeadManagementAsync(
        int                     productId,
        long                    editorPersonaId,
        long                    subjectPersonaId,
        IDataCollectorAsync     dataCollector,
        IProductRepositoryAsync productRepository,
        IManagePersonaAsync     managePersona,
        IManageUserLoginAsync   manageUserLogin,
        IUserClaimsAccessor     userClaimsAccessor,
        IHttpClientFactory      httpClientFactory,
        ITokenHelperAsync       tokenHelper,
        ICacheService           cacheService,
        ILoggerFactory          loggerFactory)
        : base(productId, editorPersonaId, subjectPersonaId,
               dataCollector, productRepository, managePersona, manageUserLogin,
               userClaimsAccessor, httpClientFactory, tokenHelper, cacheService, loggerFactory)
    {
    }

    // ── Overrides ──────────────────────────────────────────────────────────────

    /// <inheritdoc/>
    /// <remarks>
    /// LeadAnalytics gets role "18"; all other Lead products get role "17".
    /// Both receive Properties = ["all"] and empty PropertyGroups.
    /// </remarks>
    protected override Task ApplySuperUserDataAsync(IntegrationProductUser productUser, CancellationToken ct)
    {
        productUser.Roles = ProductId == (int)ProductEnum.LeadAnalytics
            ? ["18"]
            : ["17"];

        productUser.Properties    = ["all"];
        productUser.PropertyGroups = [];
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    /// <remarks>
    /// GB-4715: if the product login name does not match the user's email address
    /// the <c>productUsername</c> SAML attribute is updated with the email value.
    /// </remarks>
    protected override async Task UpdateSamlUserAttributeAsync(
        long personaId, int productId,
        string? productUserId, string? productUserLoginName, string? productUserEmail,
        CancellationToken ct)
    {
        LogDebug(nameof(UpdateSamlUserAttributeAsync),
            $"productUserLoginName={productUserLoginName}");

        if (string.IsNullOrEmpty(productUserLoginName) || string.IsNullOrEmpty(productUserEmail))
            return;

        // If login name does not match email, sync the SAML attribute with the email value
        if (!productUserLoginName.Equals(productUserEmail, StringComparison.OrdinalIgnoreCase))
        {
            await _dataCollector.UpdateSamlUserAttributeAsync(
                personaId, productId, SamlAttributeEnum.productUsername, productUserEmail, ct);
        }
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Lead Management requires a deterministic, product-specific login name:
    /// <c>{firstInitial}{lastName}_{BooksProductCode}_{subjectPersonaId}</c> (lowercase).
    /// This overrides the base-class flow which would otherwise generate a name via
    /// <c>GetUniqueProductLoginNameAsync</c>.
    /// </remarks>
    public override async Task<(string result, List<AdditionalParameters> auditParams)> CreateUpdateProductUserAsync(
        ProductUserRolePropertiesGroups userRolePropertiesRegion,
        BatchProcessType batchProcessType = BatchProcessType.CreateUpdateProductUser,
        CancellationToken ct = default)
    {
        LogDebug(nameof(CreateUpdateProductUserAsync),
            $"Editor={EditorUserDetails.PersonaId}");

        var newProductUser = await GenerateProductUserObjectAsync(userRolePropertiesRegion, ct);

        if (string.IsNullOrEmpty(SubjectUserDetails?.ProductUserName))
        {
            // Build deterministic login: first-initial + last + "_" + BooksProductCode + "_" + personaId
            string loginName = (newProductUser.FirstName.TrimWhiteSpace()[..1]
                                + newProductUser.LastName.TrimWhiteSpace()).ToLower()
                               + "_" + BlueBookGbProductMap.BooksProductCode
                               + "_" + SubjectUserDetails!.PersonaId;

            newProductUser.LoginName = loginName;

            bool alreadyExists = await CheckUserExistInProductAsync(loginName, ct: ct);
            if (alreadyExists)
            {
                LogError(null, nameof(CreateUpdateProductUserAsync),
                    $"User {loginName} already exists in product {ProductId}");
                return ($"{loginName} already exist in the product {ProductId}.", []);
            }

            return await CreateUserAsync(newProductUser, batchProcessType, ct);
        }

        // User already exists in product — update
        LogDebug(nameof(CreateUpdateProductUserAsync), "Calling UpdateUser");
        newProductUser.UserId    = SubjectUserDetails!.ProductUserId;
        newProductUser.LoginName = SubjectUserDetails.ProductUserName;
        return await UpdateUserAsync(newProductUser, batchProcessType, ct);
    }
}
