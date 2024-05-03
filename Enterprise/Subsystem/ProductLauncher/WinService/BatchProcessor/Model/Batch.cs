using System;

namespace RP.Enterprise.Subsystem.ProductLauncher.WinService.UnityBatchProcessor.Model
{
	public class Batch
	{
		public int BatchProcessorId { get; set; }
		public Guid CorrelationId { get; set; } // used as trace id to track log
		public long EditorUserPersonaId { get; set; }
		public Guid EditorUserRealPageId { get; set; }
		public long SubjectUserPersonaId { get; set; }
		public int ProductId { get; set; }
		public int StatusTypeId { get; set; }
		public int BatchProcessTypeId { get; set; }
		public string InputJson { get; set; }
		public int RetryCount { get; set; }
        public int BatchProcessorGroupId { get; set; }
        //public DateTime CreatedDateTime { get; set; }
        //public DateTime LastRunDateTime { get; set; }
        public long ImpersonatorUserId { get; set; }
        public long PrimaryOrganizationPartyId { get; set; }
    }
}
