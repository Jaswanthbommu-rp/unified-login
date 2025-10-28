using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnifiedLogin.SharedObjects.Product.UnifiedLogin
{
	public class UnifiedLoginRight
	{
		/// <summary>
		/// Unique RightId
		/// </summary>
		[JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
		public int RightId { get; set; }

		/// <summary>
		/// Right Name
		/// </summary>
	   [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
		public string Right { get; set; }

		/// <summary>
		/// Right Value TypeId
		/// </summary>
		public int RightValueTypeId { get; set; }

		/// <summary>
		/// Right ShortName (NickName)
		/// </summary>
		public string RightNickName { get; set; }		
	}
}
