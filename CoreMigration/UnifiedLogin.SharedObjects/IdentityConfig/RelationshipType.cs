using Newtonsoft.Json;

namespace UnifiedLogin.SharedObjects.IdentityConfig
{
	/// <summary>
	/// RelationshipType
	/// </summary>
	public class RelationshipType : IRelationshipType
	{
		/// <summary>
		/// Unique RelationshipType ID
		/// </summary>
		[JsonProperty(PropertyName = "RelationshipTypeId")]
		public int RelationshipTypeId { get; set; }

		/// <summary>
		/// Define the RoleTypeId valid From
		/// </summary>
		[JsonProperty(PropertyName = "RoleTypeIdValidFrom")]
		public int RoleTypeIdValidFrom { get; set; }

		/// <summary>
		/// Define the RoleTypeId valid To
		/// </summary>
		[JsonProperty(PropertyName = "RoleTypeIdValidTo")]
		public int RoleTypeIdValidTo { get; set; }

		/// <summary>
		/// RelationshipType Name
		/// </summary>
		[JsonProperty(PropertyName = "Name")]
		public string Name { get; set; }

		/// <summary>
		/// RelationshipType Description
		/// </summary>
		[JsonProperty(PropertyName = "Description")]
		public string Description { get; set; }
	}
}
