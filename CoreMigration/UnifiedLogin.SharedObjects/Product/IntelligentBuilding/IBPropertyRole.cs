using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnifiedLogin.SharedObjects.Landing;
namespace UnifiedLogin.SharedObjects.Product.IntelligentBuilding
{
	public class IBPropertyRole
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
		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public List<string> RemovedPropertyList { get; set; }
	}
	/// <summary>
	/// Object to map with Input Json from UI
	/// </summary>
	public class UserAssignProductPropertyRole
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