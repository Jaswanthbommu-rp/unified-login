using System;

namespace RP.Enterprise.Subsystem.ProductLauncher.WinService.UnityBatchProcessor.Model
{
    /// <summary>
    /// Row model for [Batch].[BulkResetPassword]. One instance represents one queued
    /// password-reset request for a single user.
    /// </summary>
    public class BulkResetPasswordBatch
    {
        public long Id { get; set; }
        public Guid RealPageId { get; set; }
        public DateTime CreatedDateTime { get; set; }
    }
}
