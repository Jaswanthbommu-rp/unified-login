using Newtonsoft.Json;
using System.Collections.Generic;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.SharedObjects.Product.UnifiedAmenities
{
	/// <summary>
	/// Used to store information about Unified Amenities for a user
	/// </summary>
	public class UnifiedAmenitiesUser
	{
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

	}
}
