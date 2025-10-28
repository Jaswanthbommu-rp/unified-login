namespace UnifiedLogin.SharedObjects.Enum
{
    /// <summary>
    /// 
    /// </summary>
    public enum EmailStatusType : int
    {
		/// <summary>
		/// Pending State
		/// </summary>
		EmailPending = 2,
		/// <summary>
		/// An error was encounterd
		/// </summary>
		EmailError = 7,
        /// <summary>
        /// Email sent successfully
        /// </summary>
        EmailSuccess = 8
    }

}