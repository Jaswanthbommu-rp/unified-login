using System;
using UnifiedLogin.BusinessLogic.Logic.BatchProcessor.Factory;
using UnifiedLogin.BusinessLogic.Logic.Product;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.Logic.BatchProcessor.Process
{
	public class ChangeProductUserType : IProcessExecution
	{
		/// <summary>
		/// Execute batch process
		/// </summary>
		public string ExecuteProcess(ProductUserProperitiesRoles batchRecord)
		{
			if (batchRecord.CorrelationId == null || batchRecord.CorrelationId == Guid.Empty)
				batchRecord.CorrelationId = Guid.NewGuid();

			ManageProductUser manageProduct = new ManageProductUser(new DefaultUserClaim { CorrelationId = batchRecord.CorrelationId });
			return manageProduct.ChangeUserType(batchRecord);

		}
	}
}