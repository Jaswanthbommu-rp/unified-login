namespace UnifiedLogin.SharedObjects.Landing
{
	/// <summary>
	/// Interface for ErrorStatus
	/// </summary>
	public interface IStatus<T>
	{
		/// <summary>
		/// Error Code (Major.Minor: Major = Controller and Minor = API verb
		/// </summary>
		string ErrorCode { get; set; }

		/// <summary>
		/// Details about the number of rows affected or read by the query
		/// </summary>
		T ErrorData { get; set; }

		/// <summary>
		/// Technical Error Message
		/// </summary>
		string ErrorMsg { get; set; }

		/// <summary>
		/// Error Success Status
		/// </summary>
		bool Success { get; set; }
	}
}