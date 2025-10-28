using System;

namespace UnifiedLogin.SharedObjects.IdentityConfig
{
	/// <summary>
	/// Interface for PartyRelationship
	/// </summary>
	public interface IPartyRelationship
	{
		/// <summary>
		/// Party Relationship thru Date
		/// </summary>
		DateTime FromDate { get; set; }

		/// <summary>
		/// Party Id in the relationship (From)
		/// </summary>
		long PartyIdFrom { get; set; }

		/// <summary>
		/// Party Unique Identifier in the relationship (From)
		/// </summary>
		Guid RealPageIdFrom { get; set; }

		/// <summary>
		/// Party Id in the relationship (To)
		/// </summary>
		long PartyIdTo { get; set; }

		/// <summary>
		/// Party Unique Identifier in the relationship (To)
		/// </summary>
		Guid RealPageIdTo { get; set; }

		/// <summary>
		/// Unique Party Relationship ID
		/// </summary>
		long PartyRelationshipId { get; set; }

		/// <summary>
		/// Type of relationship the parties are in
		/// </summary>
		int PartyRelationshipTypeId { get; set; }

		/// <summary>
		/// Type of relationship detail the parties are in
		/// </summary>
		IRelationshipType PartyRelationshipType { get; set; }
		
		/// <summary>
		/// Unique RoleType ID in the relationship (From)
		/// </summary>
		int RoleTypeIdFrom { get; set; }

		/// <summary>
		/// RoleType From Detail
		/// </summary>
		IRoleType RoleTypeFrom { get; set; }

		/// <summary>
		/// Unique RoleType ID in the relationship (To)
		/// </summary>
		int RoleTypeIdTo { get; set; }

		/// <summary>
		/// RoleType To Detail
		/// </summary>
		IRoleType RoleTypeTo { get; set; }

		/// <summary>
		/// Party Relationship thru Date
		/// </summary>
		DateTime ThruDate { get; set; }
	}
}