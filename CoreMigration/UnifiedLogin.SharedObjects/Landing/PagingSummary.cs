namespace UnifiedLogin.SharedObjects.Landing
{
	/// <summary>
	/// Paging Summary
	/// </summary>
	public class PagingSummary : IPagingSummary
	{
		/// <summary>
		/// Total number of records count (without any paging if the response is limited by paging)
		/// </summary>
		public int TotalRecords { get; set; }

		/// <summary>
		/// Total number of Pages Ceiling(TotalRecords / PageSize)
		/// </summary>
		public int TotalPages { get; set; }
	}
}
