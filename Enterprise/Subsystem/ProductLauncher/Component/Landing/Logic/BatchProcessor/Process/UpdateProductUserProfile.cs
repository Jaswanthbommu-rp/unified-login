using System;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.BatchProcessor.Factory;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.BatchProcessor.Process
{
	/// <summary>
	/// Concrete implementation for UpdateProductUserProfile
	/// </summary>
	public class UpdateProductUserProfile : IProcessExecution
	{
		/// <summary>
		/// Execute batch process
		/// </summary>
		public string ExecuteProcess(ProductUserProperitiesRoles batchRecord)
		{
			ManageProductUser manageProduct = new ManageProductUser(new DefaultUserClaim { CorrelationId = Guid.NewGuid() });
			return manageProduct.UpdateProductUserProfile(batchRecord);
		}
	}
}