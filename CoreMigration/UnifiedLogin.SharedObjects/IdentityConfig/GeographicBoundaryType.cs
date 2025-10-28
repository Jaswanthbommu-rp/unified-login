using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnifiedLogin.SharedObjects.IdentityConfig
{
	/// <summary>
	/// Geographic Boundary Type
	/// </summary>
	public class GeographicBoundaryType : IGeographicBoundaryType
	{
		/// <summary>
		/// Geographic Boundary unique type Id
		/// </summary>
		[JsonProperty(PropertyName = "GeographicBoundaryTypeId")]
		public int GeographicBoundaryTypeId { get; set; }

		/// <summary>
		/// Geographic Boundary Type Name
		/// </summary>
		[JsonProperty(PropertyName = "TypeName")]
		public string TypeName { get; set; }
	}
}
