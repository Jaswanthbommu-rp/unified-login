using System;
using System.Collections.Generic;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing
{
    /// <summary>
    /// Response for POST /api/userlogins/bulkresetpassword.
    /// Returns the number of users queued for password reset and the list of
    /// users that were skipped because they failed an eligibility check.
    /// </summary>
    public class BulkResetPasswordResponse
    {
        /// <summary>
        /// Number of users successfully inserted into the bulk reset password queue.
        /// </summary>
        public int EligibleCount { get; set; }

        /// <summary>
        /// Users that were rejected by the eligibility filter, with the reason.
        /// </summary>
        public IList<IneligibleBulkResetPasswordUser> IneligibleUsers { get; set; }
            = new List<IneligibleBulkResetPasswordUser>();
    }

    /// <summary>
    /// A single user that was rejected by the bulk reset password eligibility filter.
    /// </summary>
    public class IneligibleBulkResetPasswordUser
    {
        public Guid RealPageId { get; set; }
        public string Reason { get; set; }
    }
}
