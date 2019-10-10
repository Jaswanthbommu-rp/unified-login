using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.BatchProcessor.Factory;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using System;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.BatchProcessor.Process
{
	/// <summary>
	/// Concrete implementation for Create Update Product User
	/// </summary>
	public class CreateUpdateProductUser : IProcessExecution
	{
		/// <summary>
		/// Execute batch process
		/// </summary>
		public string ExecuteProcess(ProductUserProperitiesRoles batchRecord)
		{
			ManageProductUser manageProduct = new ManageProductUser(new DefaultUserClaim { CorrelationId = Guid.NewGuid() });
			return manageProduct.CreateProductUser(batchRecord);
		}
	}
}