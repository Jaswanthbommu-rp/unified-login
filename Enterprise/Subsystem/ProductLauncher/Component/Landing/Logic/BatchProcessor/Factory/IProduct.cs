using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.BatchProcessor.Factory
{
	public interface IProduct
	{
		string UpdateProductUserProfile(ProductUserProperitiesRoles batchRecord);
	}
}