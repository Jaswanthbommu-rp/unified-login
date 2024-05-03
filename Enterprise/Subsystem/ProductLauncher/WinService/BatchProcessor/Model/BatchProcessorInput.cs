using System;

namespace RP.Enterprise.Subsystem.ProductLauncher.WinService.UnityBatchProcessor.Model
{
    /// <summary>
    /// Product details for a user 
    /// </summary>
    public class BatchProcessorInput
    {
        /// <summary>
        /// Product batch Id
        /// </summary>
        public int ProductBatchId { get; set; }

        /// <summary>
        /// Subject User RealPage Id
        /// </summary>
        public Guid RealPageId { get; set; }
        /// <summary>
        /// Product Id from ProductEnum
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
        /// Batch process type 
        /// </summary>
        public int BatchProcessType { get; set; }

        /// <summary>
        /// Trace Id for logging purposes
        /// </summary>
        public Guid CorrelationId { get; set; } // used as trace id to track log

        /// <summary>
        /// API Endpoint to execute process
        /// </summary>
        public string ProcessApiEndPoint { get; set; }

        /// <summary>
        /// Batch Process Group
        /// </summary>
        public int BatchProcessorGroupId { get; set; }

        /// <summary>
        /// LoggedInUser for RP Employee
        /// </summary>
        public long ImpersonatorUserId { get; set; }

        /// <summary>
        /// Getting multicompany user primary company organization partyid
        /// </summary>
        public long PrimaryOrganizationPartyId { get; set; }
    }
}
