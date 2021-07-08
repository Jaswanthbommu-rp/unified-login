using System;
using System.Collections.Generic;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.BatchProcessor.Process;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.BatchProcessor.Factory
{
    /// <summary>
    /// Factory to return instance based on process type
    /// </summary>
    public static class ProcessExecutionFactory
    {
        private static readonly Dictionary<BatchProcessType, Type> Factories =
            new Dictionary<BatchProcessType, Type>();

        /// <summary>
        /// Processes to add
        /// </summary>
        static ProcessExecutionFactory()
        {
            Factories.Add(BatchProcessType.CreateUpdateProductUser, typeof(CreateUpdateProductUser));
            Factories.Add(BatchProcessType.ProfileUpdate, typeof(UpdateProductUserProfile));
            Factories.Add(BatchProcessType.DeactivateProductUser, typeof(DeactivateProductUser));

	        Factories.Add(BatchProcessType.UserTypeAdminToRegular, typeof(ChangeProductUserType));
	        Factories.Add(BatchProcessType.UserTypeRegularToAdmin, typeof(ChangeProductUserType));

            Factories.Add(BatchProcessType.UserTypeAdminToExternal, typeof(ChangeProductUserType));
            Factories.Add(BatchProcessType.UserTypeExternalToAdmin, typeof(ChangeProductUserType));

            Factories.Add(BatchProcessType.EnterpriseRoleCreateUpdateProductUser, typeof(EnterpriseCreateUpdateProductUser));
        }

		/// <summary>
		/// Returns instance of class for execution based on process type
		/// </summary> 
		public static IProcessExecution GetProductLogic(BatchProcessType processType)
        {
            return (IProcessExecution)Activator.CreateInstance(Factories[processType]);
        }
    }
}