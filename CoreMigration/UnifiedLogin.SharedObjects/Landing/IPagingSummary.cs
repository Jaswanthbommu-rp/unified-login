namespace UnifiedLogin.SharedObjects.Landing
{
	/// <summary>
	/// Interface for Paging Summary
	/// </summary>
	public interface IPagingSummary
	{
		/// <summary>
		/// Total number of records count (without any paging if the response is limited by paging)
		/// </summary>
		int TotalPages { get; set; }

		/// <summary>
		/// Total number of records count (without any paging if the response is limited by paging)
		/// </summary>
		int TotalRecords { get; set; }
	}
}