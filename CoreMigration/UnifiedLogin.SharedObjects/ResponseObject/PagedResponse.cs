using System.Collections.Generic;
using Newtonsoft.Json;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.SharedObjects.ResponseObject
{
	public class PagedResponse : ResponseBase
	{
		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public Links Links { get; set; }

		public IList<object> Data { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public Meta Meta { get; set; }
	}

	public class Meta
	{
		/// <summary>
		/// Rows Per Page
		/// </summary>
		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public int RowsPerPage { get; set; }

		/// <summary>
		/// Current Page
		/// </summary>
		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public int CurrentPage { get; set; }

		/// <summary>
		/// Total Rows
		/// </summary>
		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public int TotalRows { get; set; }

		/// <summary>
		/// Key-Value pair - dictionary
		/// </summary>
		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public object Sources { get; set; }
	}

	public class Links
	{
		public string Self { get; set; }
		public string First { get; set; }
		public string Last { get; set; }
		public string Prev { get; set; }
		public string Next { get; set; }
	}
}