using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.Logic.Interfaces
{
	/// <summary>
	/// Interface for Manage Person repository calls
	/// </summary>
	public interface IManagePerson
	{
		/// <summary>
		/// Create a Person
		/// </summary>
		/// <param name="person">Person data object</param>
		/// <returns>RepositoryResponse object</returns>
		RepositoryResponse CreatePerson(IPerson person);

		/// <summary>
		/// Get Person
		/// </summary>
		/// <param name="realPageId">User unique identifier</param>
		/// <returns>Password policy object</returns>
		Person GetPerson(Guid realPageId);

		/// <summary>
		/// Update an existing Person
		/// </summary>
		/// <param name="realPageId">User unique identifier</param>
		/// <param name="person">Person object of the parameter values</param>
		/// <returns>RepositoryResponse object</returns>
		RepositoryResponse UpdatePerson(Guid realPageId, IPerson person);

	}
}