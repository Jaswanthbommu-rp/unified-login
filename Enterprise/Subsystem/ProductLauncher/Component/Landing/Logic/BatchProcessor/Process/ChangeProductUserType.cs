using System;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.BatchProcessor.Factory;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using ZiggyCreatures.Caching.Fusion;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.BatchProcessor.Process
{
	public class ChangeProductUserType : IProcessExecution
	{
		/// <summary>
		/// Execute batch process
		/// </summary>
		public string ExecuteProcess(ProductUserProperitiesRoles batchRecord, FusionCache cache)
		{
			if (batchRecord.CorrelationId == null || batchRecord.CorrelationId == Guid.Empty)
				batchRecord.CorrelationId = Guid.NewGuid();

			ManageProductUser manageProduct = new ManageProductUser(new DefaultUserClaim { CorrelationId = batchRecord.CorrelationId }, cache: cache);
			return manageProduct.ChangeUserType(batchRecord);

		}
	}
}