using UnifiedLogin.BusinessLogic.Logic.BatchProcessor.Factory;
using UnifiedLogin.SharedObjects.Landing;
using System;
using UnifiedLogin.BusinessLogic.Logic.Product;
using System.Security.Claims;

namespace UnifiedLogin.BusinessLogic.Logic.BatchProcessor.Process
{
	public class EnterpriseCreateUpdateProductUser : IProcessExecution
	{
		/// <summary>
		/// Execute batch process
		/// </summary>
		public string ExecuteProcess(ProductUserProperitiesRoles batchRecord)
		{
			ManageProductUser manageProduct = new ManageProductUser(new DefaultUserClaim { CorrelationId = Guid.NewGuid() });
			return manageProduct.CreateEnterpriseRoleProductUser(batchRecord);
		}
	}
}
