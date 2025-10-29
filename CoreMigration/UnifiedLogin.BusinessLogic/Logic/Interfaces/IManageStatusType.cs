using System.Collections.Generic;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.Logic.Interfaces
{
	/// <summary>
	/// Interface for Manage StatusType repository calls
	/// </summary>
	public interface IManageStatusType
	{
		/// <summary>
		/// List StatusTypes
		/// </summary>
		/// <param name="CategoryTypeName">Category TypeName (e.g. Status)</param>
		/// <param name="CategoryName">Category Name (e.g. User Status)</param>
		/// <returns>List of StatusType objects</returns>
		IList<StatusType> GetStatusType(string CategoryTypeName, string CategoryName);
	}
}