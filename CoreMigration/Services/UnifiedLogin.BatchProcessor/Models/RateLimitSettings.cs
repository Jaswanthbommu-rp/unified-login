using System;
using System.Collections.Generic;
using System.Text;

namespace UnifiedLogin.BatchProcessor.Models
{
    /// <summary>
    /// Configuration settings for rate limiting API and database calls.
    /// </summary>
    public class RateLimitSettings
    {
        public const string SectionName = "RateLimitSettings";

        /// <summary>
        /// Maximum concurrent API calls allowed (default: 10)
        /// </summary>
        public int ApiCallsPerSecond { get; set; } = 10;

        /// <summary>
        /// Maximum concurrent database operations allowed (default: 50)
        /// </summary>
        public int DatabaseCallsPerSecond { get; set; } = 50;

        /// <summary>
        /// Maximum number of requests that can be queued (default: 100)
        /// </summary>
        public int QueueLimit { get; set; } = 100;
    }
}
