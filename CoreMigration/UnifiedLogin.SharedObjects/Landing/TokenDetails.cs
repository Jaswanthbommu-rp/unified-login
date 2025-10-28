using System;

namespace UnifiedLogin.SharedObjects.Landing
{
    /// <summary>
    /// Holds Token Details
    /// </summary>
    public class TokenDetail
    {
        /// <summary>
        /// EnterpriseUserId
        /// </summary>
        public int EnterpriseUserId { get; set; }

        /// <summary>
        /// Enterprise User's real page Id
        /// </summary>
        public Guid RealPageId { get; set; }

        /// <summary>
        /// Token
        /// </summary>
        public string Token { get; set; }
    }
}
