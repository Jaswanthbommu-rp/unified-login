using System;
using Newtonsoft.Json;
using UnifiedLogin.SharedObjects.Enum;

namespace UnifiedLogin.SharedObjects.Landing
{
    /// <summary>
	/// Product Batch Status
	/// </summary>
    public class ProductBatchStatus
    {
        /// <summary>
		/// Unique Product Batch Id
		/// </summary>
		public int ProductBatchId { get; set; }

        /// <summary>
        /// Person PartyId
        /// </summary>
        public long PersonPartyId { get; set; }

        /// <summary>
        /// Created By PersomaId
        /// </summary>
        public long CreateUserPersonaId { get; set; }

        /// <summary>
        /// </summary>
        /// Assigned to PersonaId
        public long AssignUserPersonaId { get; set; }

        /// <summary>
        /// ProductId
        /// </summary>
        [JsonIgnore]
        public int ProductId { get; set; }

        /// <summary>
        /// ProductId
        /// </summary>
        public string ProductName => ((ProductEnum)ProductId).ToString();

        /// <summary>
        /// Product Batch Status (Waiting, Running, Error, and Success)
        /// </summary>
        [JsonIgnore]
        public int StatusTypeId { get; set; }

        /// <summary>
        /// Product Batch Status (Waiting, Running, Error, and Success)
        /// </summary>
        public string Status => ((ProductBatchStatusType)StatusTypeId).ToString();

        /// <summary>
        /// Retry count - used to call the API for this Product
        /// </summary>
        public byte RetryCount { get; set; }

        /// <summary>
        /// Product API (List of Properties and Roles) input JSON
        /// </summary>
        [JsonIgnore]
        public string InputJson { get; set; }

        /// <summary>
        /// Product API Last run datetime 
        /// </summary>
        public DateTime LastRunDate { get; set; }

        /// <summary>
        /// Product batch create datetime
        /// </summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// Product batch modified datetime
        /// </summary>
        public DateTime ModifiedDate { get; set; }

        /// <summary>
        /// Error details
        /// </summary>
        public string ErrorDetails { get; set; }
    }
}
