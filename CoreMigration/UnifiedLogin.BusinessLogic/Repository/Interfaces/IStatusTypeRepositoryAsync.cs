using System.Collections.Generic;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.Repository.Interfaces
{
	/// <summary>
	/// Interface for StatusType Repository
	/// </summary>
	public interface IStatusTypeRepositoryAsync
	{
		/// <summary>
		/// List StatusTypes
		/// </summary>
		/// <param name="CategoryTypeName">Category TypeName (e.g. Status)</param>
		/// <param name="CategoryName">Category Name (e.g. User Status)</param>
		/// <param name="cancellationToken"></param>
		/// <returns>List of StatusType object</returns>
		Task<IList<StatusType>> GetStatusTypeAsync(string CategoryTypeName, string CategoryName, CancellationToken cancellationToken = default);
	}
}