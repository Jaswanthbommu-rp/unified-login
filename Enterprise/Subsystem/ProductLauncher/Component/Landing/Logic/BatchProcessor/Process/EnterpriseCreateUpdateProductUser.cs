using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.BatchProcessor.Factory;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using System;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product;
using System.Security.Claims;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.BatchProcessor.Process
{
	public class EnterpriseCreateUpdateProductUser : IProcessExecution
	{
		/// <summary>
		/// Execute batch process
		/// </summary>
		public string ExecuteProcess(ProductUserProperitiesRoles batchRecord)
		{
			ManageProductUser manageProduct = new ManageProductUser(new DefaultUserClaim(ClaimsPrincipal.Current));
			return manageProduct.CreateEnterpriseRoleProductUser(batchRecord);
		}
	}
}
