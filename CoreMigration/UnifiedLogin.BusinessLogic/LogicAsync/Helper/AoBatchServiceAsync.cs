using Microsoft.Extensions.Logging;
using UnifiedLogin.BusinessLogic.Logic.Product;
using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product;
using UnifiedLogin.SharedObjects.Saml;
using UnifiedLogin.SharedObjects.IdentityConfig;

namespace UnifiedLogin.BusinessLogic.LogicAsync.Helper;

/// <summary>
/// Builds Asset Optimization (AO/BI) <see cref="ProductBatch"/> records for user-clone operations.
/// <para>
/// Replaces <c>BatchHelper.CreateAoBatchRecords(DefaultUserClaim, …)</c>:
/// <list type="bullet">
///   <item><c>new SamlRepository()</c> → injected <see cref="ISamlRepositoryAsync"/></item>
///   <item><c>new ManageProductAssetOptimization(userClaim)</c> → injected <see cref="IUserClaimsAccessor"/>
///         to remove the <c>DefaultUserClaim</c> constructor parameter</item>
/// </list>
/// </para>
/// <para>
/// <b>Follow-up:</b> <c>IManageProductAssetOptimizationAsync.CopyRegularUserAsync</c> now exists.
/// Inject <c>IManageProductAssetOptimizationAsync</c> here and replace the sync instantiation
/// to fully remove <c>DefaultUserClaim</c> from this service.
/// </para>
/// <para><b>DI registration:</b> <c>Scoped</c>.</para>
/// </summary>
public sealed class AoBatchServiceAsync : IAoBatchServiceAsync
{
    private readonly ISamlRepositoryAsync    _samlRepository;
    private readonly IUserClaimsAccessor     _userClaims;
    private readonly ILogger<AoBatchServiceAsync> _logger;

    public AoBatchServiceAsync(
        ISamlRepositoryAsync      samlRepository,
        IUserClaimsAccessor       userClaims,
        ILogger<AoBatchServiceAsync> logger)
    {
        _samlRepository = samlRepository ?? throw new ArgumentNullException(nameof(samlRepository));
        _userClaims     = userClaims     ?? throw new ArgumentNullException(nameof(userClaims));
        _logger         = logger         ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task<IList<ProductBatch>> CreateAoBatchRecordsAsync(
        long editorPersonaId,
        long newUserPersonaId,
        bool externalUser,
        bool usePrimaryProperties,
        ListResponse propertiesResponse,
        int productId,
        IList<ProductRole>? productRoles = null,
        IList<ProductBatch>? productBatchList = null,
        bool isDeleted = false,
        CancellationToken cancellationToken = default)
    {
        productBatchList ??= [];

        // NOTE: IManageProductAssetOptimizationAsync.CopyRegularUserAsync now exists.
        // AoBatchServiceAsync can be refactored to inject IManageProductAssetOptimizationAsync
        // and call CopyRegularUserAsync directly, removing the DefaultUserClaim dependency.
        var userClaim = _userClaims.GetUserClaim();
        var manageAo  = new ManageProductAssetOptimization(userClaim);

        var productList = propertiesResponse?.Records?.Cast<ProductProperty>().ToList()
                          ?? [];

        // ── Resolve AO/BI company-property-role details for the user ──────────
        IList<AoUserCompanyPropertyRoleDetail> aoBIDetails = [];

        if (externalUser)
        {
            // Fetch the BI username from SAML attributes (async — ISamlRepositoryAsync)
            var samlAttributes = await _samlRepository.GetProductSamlDetailsAsync(
                newUserPersonaId, (int)ProductEnum.AoBusinessIntelligence, cancellationToken)
                .ConfigureAwait(false);

            var aoBIUserName = samlAttributes
                .FirstOrDefault(a => a.Name.Equals("ProductUserName", StringComparison.OrdinalIgnoreCase))
                ?.Value;

            if (!string.IsNullOrEmpty(aoBIUserName))
            {
                // SYNC: CopyRegularUser — pending async port
                aoBIDetails = manageAo.CopyRegularUser(editorPersonaId, newUserPersonaId, aoBIUserName);
            }
        }

        // SYNC: CopyRegularUser — pending async port
        var aoDetails = manageAo.CopyRegularUser(editorPersonaId, newUserPersonaId);

        foreach (var biDetail in aoBIDetails)
            aoDetails.Add(biDetail);

        var aoDetail = aoDetails.FirstOrDefault(d =>
            (int)ProductEnumHelper.GetAoProductEnum(d.ProductName) == productId);

        if (aoDetail is not null)
        {
            aoDetail.SelectedPortfolioValues ??= [];
            aoDetail.PropertyGroups          ??= [];

            var productBatch = new ProductBatch
            {
                ProductId    = (int)ProductEnumHelper.GetAoProductEnum(aoDetail.ProductName),
                StatusTypeId = 5,
                RetryCount   = 0,
                InputJson    = new RolePropertyList
                {
                    PropertyList = isDeleted
                        ? aoDetail.SelectedPortfolioValues.Select(i => i.ToString()).ToList()
                        : propertiesResponse?.Records is { Count: > 0 }
                            ? productList.Select(p => p.ID.ToString()).ToList()
                            : [],
                    RoleList = isDeleted
                        ? [.. aoDetail.SelectedRoleValues]
                        : productRoles is { Count: > 0 }
                            ? productRoles.Select(r => r.Name).ToList()
                            : [],
                    CompanyId         = aoDetail.CompanyId,
                    PropertyGroupList = aoDetail.PropertyGroups.Select(i => i.ToString()).ToList(),
                    UsePrimaryProperties = usePrimaryProperties,
                    IsAssigned        = !isDeleted
                }
            };

            // Correct IsAssigned when no properties are present
            if (propertiesResponse?.Records is null or { Count: 0 })
                productBatch.InputJson.IsAssigned = false;

            if (!productBatchList.Any(b => b.ProductId == productBatch.ProductId)
                && productBatch.ProductId == productId)
            {
                productBatchList.Add(productBatch);
            }
        }
        else if (!productBatchList.Any(b => b.ProductId == productId) && !isDeleted)
        {
            productBatchList.Add(new ProductBatch
            {
                ProductId    = productId,
                StatusTypeId = 5,
                RetryCount   = 0,
                InputJson    = new RolePropertyList
                {
                    PropertyList         = propertiesResponse?.Records is { Count: > 0 }
                                           ? productList.Select(p => p.ID.ToString()).ToList()
                                           : [],
                    RoleList             = productRoles?.Select(r => r.Name).ToList() ?? [],
                    CompanyId            = 0,
                    PropertyGroupList    = [],
                    UsePrimaryProperties = usePrimaryProperties,
                    IsAssigned           = propertiesResponse?.Records is { Count: > 0 }
                }
            });
        }

        return productBatchList;
    }
}
