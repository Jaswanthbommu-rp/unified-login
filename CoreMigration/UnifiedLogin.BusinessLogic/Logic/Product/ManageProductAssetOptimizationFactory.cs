using UnifiedLogin.BusinessLogic.Logic.Product.Interfaces;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.Logic.Product;

/// <summary>
/// Default factory — wraps <see langword="new"/> ManageProductAssetOptimization so
/// ProductService never needs to call <see langword="new"/> directly.
/// </summary>
public sealed class ManageProductAssetOptimizationFactory : IManageProductAssetOptimizationFactory
{
    public IManageProductAssetOptimization Create(DefaultUserClaim userClaim)
        => new ManageProductAssetOptimization(userClaim);
}