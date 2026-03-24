using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.Logic.Product.Interfaces;

/// <summary>
/// Creates a <see cref="IManageProductAssetOptimization"/> bound to the
/// supplied user claim.  Registered as Scoped so one instance is created
/// per HTTP request (or Worker Service scope).
/// </summary>
public interface IManageProductAssetOptimizationFactory
{
    IManageProductAssetOptimization Create(DefaultUserClaim userClaim);
}