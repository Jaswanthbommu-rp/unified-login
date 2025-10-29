using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.Repository.Interfaces
{
	/// <summary>
	/// Interface for StreetAddressRepository
	/// </summary>
	public interface IStreetAddressRepository
	{
		/// <summary>
		/// Create the StreetAddress for a Person
		/// </summary>
		/// <param name="streetAddress">StreetAddress Object.</param>
		/// <returns>Repository response object</returns>
		RepositoryResponse CreateStreetAddress(IStreetAddress streetAddress);
	}
}