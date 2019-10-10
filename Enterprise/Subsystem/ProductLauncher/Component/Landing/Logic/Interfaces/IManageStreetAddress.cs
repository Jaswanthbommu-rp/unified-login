using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces
{
	/// <summary>
	/// Interface for ManageStreetAddress
	/// </summary>
	public interface IManageStreetAddress
	{
		/// <summary>
		/// Create the StreetAddress for a Person
		/// </summary>
		/// <param name="streetAddress">StreetAddress Object.</param>
		/// <returns>Repository response object</returns>
		RepositoryResponse CreateStreetAddress(IStreetAddress streetAddress);
	}
}