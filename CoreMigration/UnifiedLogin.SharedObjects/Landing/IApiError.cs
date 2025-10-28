namespace UnifiedLogin.SharedObjects.Landing
{
	/// <summary>
	/// Interface for ApiError
	/// </summary>
	public interface IApiError
	{
		/// <summary>
		/// Code
		/// </summary>
		string Code { get; set; }

		/// <summary>
		/// Detail
		/// </summary>
		string Detail { get; set; }

		/// <summary>
		/// Error (correlation) Id
		/// </summary>
		string Id { get; set; }

		/// <summary>
		/// Links
		/// </summary>
		string Links { get; set; }

		/// <summary>
		/// Api error source
		/// </summary>
		ApiErrorSource Source { get; set; }

		/// <summary>
		/// Http status
		/// </summary>
		short Status { get; set; }

		/// <summary>
		/// Title
		/// </summary>
		string Title { get; set; }
	}

	/// <summary>
	/// 
	/// </summary>
	public interface IApiErrorSource
	{
		/// <summary>
		/// JsonPointer
		/// </summary>
		string JsonPointer { get; set; }

		/// <summary>
		/// Parameter
		/// </summary>
		string Parameter { get; set; }
	}
}