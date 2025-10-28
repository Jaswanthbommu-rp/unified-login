using Newtonsoft.Json;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace UnifiedLogin.SharedObjects.Landing
{
	/// <summary>
	/// UI Output JSON.  Used for the result UI.
	/// </summary>
	/// <typeparam name="T1">data list Generic type</typeparam>
	/// <typeparam name="T2">error Generic type</typeparam>
	[ExcludeFromCodeCoverage]
	public class ObjectUserListOutput<T1, T2> : IObjectUserListOutput<T1, T2>
	{
		/// <summary>
		/// List of collection of data
		/// </summary>
		[JsonProperty(PropertyName = "data")]
		public IList<T1> list { get; set; }

		/// <summary>
		/// Paging Summary
		/// </summary>
		[JsonProperty("PagingSummary", NullValueHandling = NullValueHandling.Ignore)]
		public PagingSummary pagingSummary { get; set; }

		/// <summary>
		/// Error status object - API/UI Call Success/Error Communication
		/// </summary>
		[JsonProperty("Status", NullValueHandling = NullValueHandling.Ignore)]
		public Status<T2> Status { get; set; }
		public bool OrganizationHasProductAssignmentError { get; set; }
	}
}
