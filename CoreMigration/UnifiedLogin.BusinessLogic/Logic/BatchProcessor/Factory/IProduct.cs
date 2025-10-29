using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.Logic.BatchProcessor.Factory
{
	public interface IProduct
	{
		string UpdateProductUserProfile(ProductUserProperitiesRoles batchRecord);
	}
}