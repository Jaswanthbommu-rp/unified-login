using System;
using UnifiedLogin.BusinessLogic.Logic.BatchProcessor.Factory;
using UnifiedLogin.BusinessLogic.Logic.Product;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.Logic.BatchProcessor.Process
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