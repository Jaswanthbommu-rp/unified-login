namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product.Interfaces
{
    /// <summary>
    /// 
    /// </summary>
    public interface IManageProductAssetOptimization
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="editorPersonaId"></param>
        /// <param name="userName"></param>
        /// <param name="isActive"></param>
        /// <returns></returns>
        bool ChangeUserStatus(long editorPersonaId, string userName, string firstName, string lastName, bool isActive = false);
    }
}