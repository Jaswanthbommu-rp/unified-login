using Microsoft.Extensions.Logging;
using UnifiedLogin.BusinessLogic.CacheHelper;
using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects.Cache;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.LogicAsync.Product.Integrations.ProductImplementation;

/// <summary>
/// Native-async concrete implementation for the Self-Guided Tour product integration.
/// Replaces <c>SelfGuidedTour</c> (sync).
/// <para>
/// No behaviour overrides are required; all operations are handled by the
/// <see cref="StandardV1ProductIntegrationAsync"/> base class.
/// </para>
/// </summary>
public sealed class SelfGuidedTourAsync : StandardV1ProductIntegrationAsync
{
    /// <summary>
    /// Initialises a new instance. Call <see cref="StandardV1ProductIntegrationAsync.InitAsync"/>
    /// before using any public methods.
    /// </summary>
    public SelfGuidedTourAsync(
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
}
