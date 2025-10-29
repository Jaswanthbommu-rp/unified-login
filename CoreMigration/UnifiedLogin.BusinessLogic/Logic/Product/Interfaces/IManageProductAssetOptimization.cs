using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.Logic.Product.Interfaces
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
		/// <summary>
		/// 
		/// </summary>
		/// <param name="editorPersonaId"></param>
		/// <param name="userPersonaId"></param>
		
		/// <returns></returns>
		string UpdateUserProfile(long editorPersonaId, long userPersonaId);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="editorPersonaId"></param>
        /// <param name="userPersonaId"></param>
        /// <param name="operatorCode"></param>
        /// <param name="operatorValue"></param>

        /// <returns></returns>
        ListResponse GetPropertiesWithOperators(long editorPersonaId, long userPersonaId, string operatorCode, string operatorValue);


    }
}