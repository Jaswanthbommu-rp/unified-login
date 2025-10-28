namespace UnifiedLogin.SharedObjects.Landing
{
	/// <summary>
	/// Api error response
	/// </summary>
	public class ApiError : IApiError
	{
		/// <summary>
		/// Error (correlation) Id
		/// </summary>
		public string Id { get; set; }

		/// <summary>
		/// Http status
		/// </summary>
		public short Status { get; set; }

		/// <summary>
		/// Code
		/// </summary>
		public string Code { get; set; }

		/// <summary>
		/// Links
		/// </summary>
		public string Links { get; set; }

		/// <summary>
		/// Title
		/// </summary>
		public string Title { get; set; }

		/// <summary>
		/// Detail
		/// </summary>
		public string Detail { get; set; }

		/// <summary>
		/// Api error source
		/// </summary>
		public ApiErrorSource Source { get; set; }
	}

	/// <summary>
	/// Api error source
	/// </summary>
	public class ApiErrorSource : IApiErrorSource
	{
		/// <summary>
		/// JsonPointer
		/// </summary>
		public string JsonPointer { get; set; }

		/// <summary>
		/// Parameter
		/// </summary>
		public string Parameter { get; set; }
	}
}
