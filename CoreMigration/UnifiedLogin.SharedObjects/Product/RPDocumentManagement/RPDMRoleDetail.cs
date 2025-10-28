using System.Collections.Generic;
using Newtonsoft.Json;

namespace UnifiedLogin.SharedObjects.Product.RPDocumentManagement
{
	/// <summary>
	/// Role detail information
	/// </summary>
	public class RPDMRoleDetail
	{
		/// <summary>
		/// The id of the role
		/// </summary>
		[JsonProperty(PropertyName = "id", NullValueHandling = NullValueHandling.Ignore)]
		public string Id { get; set; }

		/// <summary>
		/// The name of the role
		/// </summary>
		[JsonProperty(PropertyName = "name", NullValueHandling = NullValueHandling.Ignore)]
		public string Name { get; set; }

		/// <summary>
		/// The type of the role
		/// </summary>
		[JsonProperty(PropertyName = "type", NullValueHandling = NullValueHandling.Ignore)]
		public string Type { get; set; }

		/// <summary>
		/// The domain of the role
		/// </summary>
		[JsonProperty(PropertyName = "domain", NullValueHandling = NullValueHandling.Ignore)]
		public string Domain { get; set; }

		/// <summary>
		/// The scope of the role
		/// </summary>
		[JsonProperty("scope", NullValueHandling = NullValueHandling.Ignore)]
		public RPDMScope Scope { get; set; }
	}

	/// <summary>
	/// The scope detail for the object
	/// </summary>
	public class RPDMScope
	{
		/// <summary>
		/// The id of the object
		/// </summary>
		[JsonProperty(PropertyName = "id", NullValueHandling = NullValueHandling.Ignore)]
		public string Id { get; set; }

		/// <summary>
		/// The name of the object
		/// </summary>
		[JsonProperty(PropertyName = "name", NullValueHandling = NullValueHandling.Ignore)]
		public string Name { get; set; }

		/// <summary>
		/// The type of the object
		/// </summary>
		[JsonProperty("rel", NullValueHandling = NullValueHandling.Ignore)]
		public string Rel { get; set; }

		/// <summary>
		/// The reference url to the object
		/// </summary>
		[JsonProperty("href", NullValueHandling = NullValueHandling.Ignore)]
		public string HRef { get; set; }
	}

	/// <summary>
	/// Used to store users roles and entity information
	/// </summary>
	public class RPDMUserRoles
	{
		/// <summary>
		/// Role information
		/// </summary>
		[JsonProperty("role", NullValueHandling = NullValueHandling.Ignore)]
		public RPDMScope Role { get; set; }

		/// <summary>
		/// Entity information
		/// </summary>
		[JsonProperty("entity", NullValueHandling = NullValueHandling.Ignore)]
		public RPDMScope Entity { get; set; }
	}


}
