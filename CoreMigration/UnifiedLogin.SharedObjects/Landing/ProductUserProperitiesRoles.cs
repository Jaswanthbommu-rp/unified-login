using System;
using UnifiedLogin.SharedObjects.Enum;

namespace UnifiedLogin.SharedObjects.Landing
{
    /// <summary>
    /// Product details for a user 
    /// </summary>
    public class ProductUserProperitiesRoles
    {
        /// <summary>
        /// Product batch Id
        /// </summary>
        public int ProductBatchId { get; set; }

        /// <summary>
        /// Create User RealPage Id
        /// </summary>
        public Guid RealPageId { get; set; }
        /// <summary>
        /// Product Name
        /// </summary>
        public int ProductId { get; set; }

        /// <summary>
        /// Person who is creating or editing user
        /// </summary>
        public long CreateUserPersonaId { get; set; }

        /// <summary>
        /// Person getting created or edited
        /// </summary>
        public long AssignUserPersonaId { get; set; }

        /// <summary>
        /// Input json param values
        /// </summary>
        public string InputJson { get; set; }

	    /// <summary>
	    ///  Process type to execute
	    /// </summary>
	    public BatchProcessType BatchProcessType { get; set; }

	    /// <summary>
	    /// Correlation Id used for tracing the log
	    /// </summary>
	    public Guid CorrelationId { get; set; }

        /// <summary>
	    /// BatchprocessGroup GUID
	    /// </summary>
        public int BatchProcessorGroupId { get; set; }

        /// <summary>
        /// Is the user a RealPage employee?
        /// </summary>
        public bool CreateRealPageEmployee { get; set; }
        public long RealPageEmployeePersonaId { get; set; }

        public long ImpersonatorUserId { get; set; }
    }
}
