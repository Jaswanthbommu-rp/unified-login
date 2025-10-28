using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.SharedObjects.Product.UPFMProduct
{	
	public class UPFMProductPropertyRole
	{
			/// <summary>
			/// Is product assigned or removed
			/// </summary>
			public bool IsAssigned { get; set; } = true;
			/// <summary>
			/// A list of properties to assign to the user
			/// </summary>
			public List<string> PropertyList { get; set; }
			/// <summary>
			/// Role assigned to the user
			/// </summary>
			public List<string> RoleList { get; set; }
			
			/// <summary>
			/// Is Vendor RoleId Override or not
			/// </summary>
			public bool IsVendorRoleIdOverride { get; set; } = false;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
			public List<string> RemovedPropertyList { get; set; }
		}
		/// <summary>
		/// Object to map with Input Json from UI
		/// </summary>
		public class UPFMProductUserAssignedPropertyRole
	{
			/// <summary>
			/// A list of properties to assign to the user
			/// </summary>
			[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
			public List<string> PropertyList { get; set; }
			/// <summary>
			/// A role to assign to the user
			/// </summary>
			[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
			public List<string> RoleList { get; set; }
			/// <summary>
			/// Is product assigned or removed
			/// </summary>
			public bool IsAssigned { get; set; }
			[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
			public List<string> RemovedPropertyList { get; set; }
		}
	
}
