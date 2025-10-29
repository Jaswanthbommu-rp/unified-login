using UnifiedLogin.BusinessLogic.Logic.BatchProcessor.Factory;
using UnifiedLogin.SharedObjects.Landing;
using System;
using UnifiedLogin.BusinessLogic.Logic.Product;

namespace UnifiedLogin.BusinessLogic.Logic.BatchProcessor.Process
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