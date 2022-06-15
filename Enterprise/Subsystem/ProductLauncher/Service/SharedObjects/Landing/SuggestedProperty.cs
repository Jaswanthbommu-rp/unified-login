using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects
{
	/// <summary>
	/// Suggested property for a user
	/// </summary>
	public class SuggestedProperty
	{
		/// <summary>
		/// productPropertyId
		/// </summary>
		[JsonProperty(PropertyName = "productPropertyId")]
		public long ProductPropertyId { get; set; } = 0;

		/// <summary>
		///propertyInstanceId
		/// </summary>
		[JsonProperty(PropertyName = "propertyInstanceId")]
		public Guid PropertyInstanceId { get; set; }

		[JsonProperty(PropertyName = "propertyName")]
		public string PropertyName { get; set; }
    }
}
