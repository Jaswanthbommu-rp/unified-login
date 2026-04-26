using Microsoft.Extensions.Logging;
using UnifiedLogin.BusinessLogic.CacheHelper;
using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
using UnifiedLogin.BusinessLogic.LogicAsync.Product.Integrations.IntegrationTypes;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects.Cache;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.LogicAsync.Product.Integrations.ProductImplementation;

/// <summary>
/// Async counterpart of the sync <c>ManageProductFactory</c> (static class).
/// Creates and initialises the correct <see cref="StandardV1ProductIntegrationAsync"/>
/// subclass for a given product ID.
/// <para>
/// <b>Product → implementation mapping:</b>
/// <list type="bullet">
///   <item>LeadManagement / LeadAnalytics → <see cref="LeadManagementAsync"/></item>
///   <item>PortfolioManagement → <see cref="PortfolioManagementAsync"/></item>
///   <item>DepositAlternative → <see cref="DepositAlternativeManagementAsync"/></item>
///   <item>ClickPay → <see cref="ClickPayManagementAsync"/></item>
///   <item>All other StandardV1 products → <see cref="SelfGuidedTourAsync"/> (no-override fallback)</item>
/// </list>
/// </para>
/// <para>
/// Used by <see cref="LegacyIntegrationTypeAsync.CreateStandardV1Async"/>
/// to replace the previous hard-coded <see cref="SelfGuidedTourAsync"/> fallback.
/// </para>
/// </summary>
internal static class ManageProductFactoryAsync
{
    /// <summary>
    /// Creates the appropriate <see cref="StandardV1ProductIntegrationAsync"/> subclass
    /// for <paramref name="productId"/>, calls
    /// <see cref="StandardV1ProductIntegrationAsync.InitAsync"/>, and returns the
    /// ready-to-use instance.
    /// </summary>
    internal static async Task<StandardV1ProductIntegrationAsync> CreateAndInitAsync(
        int                         productId,
        long                        editorPersonaId,
        long                        subjectPersonaId,
        IDataCollectorAsync         dataCollector,
        IProductRepositoryAsync     productRepository,
        IManagePersonaAsync         managePersona,
        IManageUserLoginAsync       manageUserLogin,
        IUserClaimsAccessor         userClaimsAccessor,
        IHttpClientFactory          httpClientFactory,
        ITokenHelperAsync           tokenHelper,
        ICacheService               cacheService,
        ILoggerFactory              loggerFactory,
        ISamlAttributeServiceAsync  samlAttributeService,
        CancellationToken           ct)
    {
        StandardV1ProductIntegrationAsync impl = (ProductEnum)productId switch
        {
            ProductEnum.LeadManagement or ProductEnum.LeadAnalytics
                => new LeadManagementAsync(
                       productId, editorPersonaId, subjectPersonaId,
                       dataCollector, productRepository, managePersona, manageUserLogin,
                       userClaimsAccessor, httpClientFactory, tokenHelper, cacheService, loggerFactory),

            ProductEnum.PortfolioManagement
                => new PortfolioManagementAsync(
                       productId, editorPersonaId, subjectPersonaId,
                       dataCollector, productRepository, managePersona, manageUserLogin,
                       userClaimsAccessor, httpClientFactory, tokenHelper, cacheService, loggerFactory),

            ProductEnum.DepositAlternative
                => new DepositAlternativeManagementAsync(
                       productId, editorPersonaId, subjectPersonaId,
                       dataCollector, productRepository, managePersona, manageUserLogin,
                       userClaimsAccessor, httpClientFactory, tokenHelper, cacheService, loggerFactory,
                       samlAttributeService),

            ProductEnum.ClickPay
                => new ClickPayManagementAsync(
                       productId, editorPersonaId, subjectPersonaId,
                       dataCollector, productRepository, managePersona, manageUserLogin,
                       userClaimsAccessor, httpClientFactory, tokenHelper, cacheService, loggerFactory),

            _
                => new SelfGuidedTourAsync(
                       productId, editorPersonaId, subjectPersonaId,
                       dataCollector, productRepository, managePersona, manageUserLogin,
                       userClaimsAccessor, httpClientFactory, tokenHelper, cacheService, loggerFactory)
        };

        await impl.InitAsync(ct);
        return impl;
    }
}
