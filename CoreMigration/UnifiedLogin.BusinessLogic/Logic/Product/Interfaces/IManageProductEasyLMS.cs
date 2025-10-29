using UnifiedLogin.SharedObjects.BlackBook;

namespace UnifiedLogin.BusinessLogic.Logic.Product.Interfaces
{
	/// <summary>
	/// Interface for ManageProductEasyLMS: Used to Get EasyLMS Company API Code and Key from BlueBook
	/// </summary>
	public interface IManageProductEasyLMS
	{
		/// <summary>
		/// Get EasyLMS Company InstanceId from BlueBook
		/// </summary>
		/// <param name="editorPersonaId">Logged-in user PersonaId</param>
		/// <param name="userPersonaId">new user PersonaId</param>
		/// <returns>CompanyMapResource object</returns>
		CustomerCompanyMap GetCompanyAPICodeAndKey(long editorPersonaId, long userPersonaId);
	}
}