using System.Collections.Generic;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces
{
	/// <summary>
	/// Interface for StatusType Repository
	/// </summary>
	public interface IStatusTypeRepository
	{
		/// <summary>
		/// List StatusTypes
		/// </summary>
		/// <param name="CategoryTypeName">Category TypeName (e.g. Status)</param>
		/// <param name="CategoryName">Category Name (e.g. User Status)</param>
		/// <returns>List of StatusType object</returns>
		IList<StatusType> GetStatusType(string CategoryTypeName, string CategoryName);
	}
}