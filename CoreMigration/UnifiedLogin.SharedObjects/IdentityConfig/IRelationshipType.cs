namespace UnifiedLogin.SharedObjects.IdentityConfig
{
	/// <summary>
	/// Interface for RelationshipType
	/// </summary>
	public interface IRelationshipType
	{
		/// <summary>
		/// RelationshipType Description
		/// </summary>
		string Description { get; set; }

		/// <summary>
		/// RelationshipType Name
		/// </summary>
		string Name { get; set; }

		/// <summary>
		/// Unique RelationshipType ID
		/// </summary>
		int RelationshipTypeId { get; set; }

		/// <summary>
		/// Define the RoleTypeId valid From
		/// </summary>
		int RoleTypeIdValidFrom { get; set; }

		/// <summary>
		/// Define the RoleTypeId valid To
		/// </summary>
		int RoleTypeIdValidTo { get; set; }
	}
}