namespace UnifiedLogin.SharedObjects.Batch
{
    /// <summary>
    /// Represents a company property batch processing job
    /// </summary>
    public class CompanyPropertyBatch
    {
        /// <summary>
        /// Company batch job identifier
        /// </summary>
        public long CompanyBatchJobId { get; set; }

        /// <summary>
        /// Company instance source identifier (GUID as string)
        /// </summary>
        public string CompanyInstanceSourceId { get; set; } = string.Empty;

        /// <summary>
        /// Status type identifier for the batch job
        /// </summary>
        public int StatusTypeId { get; set; }

        /// <summary>
        /// Date and time when the batch job was created
        /// </summary>
        public DateTime CreatedDateTime { get; set; }

        /// <summary>
        /// Date and time of the last run
        /// </summary>
        public DateTime LastRunDateTime { get; set; }

        /// <summary>
        /// User ID who created the batch job
        /// </summary>
        public long CreatedBy { get; set; }

        /// <summary>
        /// Batch processor type identifier
        /// </summary>
        public int BatchProcessorTypeId { get; set; }

        /// <summary>
        /// Persona ID of the user who created the batch job
        /// </summary>
        public long CreateUserPersonaId { get; set; }

        /// <summary>
        /// Indicates if the batch job is active (1) or inactive (0)
        /// </summary>
        public int IsActive { get; set; }
    }
}
