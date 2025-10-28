using Newtonsoft.Json;
using System;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.SharedObjects.IdentityConfig
{
	/// <summary>
	/// Party Relationship
	/// </summary>
	public class PartyRelationship : IPartyRelationship
	{
		/// <summary>
		/// Unique Party Relationship ID
		/// </summary>
		[JsonProperty(PropertyName = "PartyRelationshipId")]
		public long PartyRelationshipId { get; set; }

		/// <summary>
		/// Party Id in the relationship (From)
		/// </summary>
		[JsonProperty(PropertyName = "PartyIdFrom")]
		public long PartyIdFrom { get; set; }

		/// <summary>
		/// Party Unique Identifier in the relationship (From)
		/// </summary>
		[JsonProperty(PropertyName = "RealPageIdFrom")]
		public Guid RealPageIdFrom { get; set; }

		/// <summary>
		/// Party Id in the relationship (To)
		/// </summary>
		[JsonProperty(PropertyName = "PartyIdTo")]
		public long PartyIdTo { get; set; }

		/// <summary>
		/// Party Unique Identifier in the relationship (To)
		/// </summary>
		[JsonProperty(PropertyName = "RealPageIdTo")]
		public Guid RealPageIdTo { get; set; }

		/// <summary>
		/// Unique RoleType ID in the relationship (From)
		/// </summary>
		[JsonProperty(PropertyName = "RoleTypeIdFrom")]
		public int RoleTypeIdFrom { get; set; }

		/// <summary>
		/// RoleType From Detail
		/// </summary>
		[JsonProperty("RoleTypeFrom", NullValueHandling = NullValueHandling.Ignore)]
		public IRoleType RoleTypeFrom { get; set; }
		
		/// <summary>
		/// Unique RoleType ID in the relationship (To)
		/// </summary>
		[JsonProperty(PropertyName = "RoleTypeIdTo")]
		public int RoleTypeIdTo { get; set; }

		/// <summary>
		/// RoleType To Detail
		/// </summary>
		[JsonProperty("RoleTypeTo", NullValueHandling = NullValueHandling.Ignore)]
		public IRoleType RoleTypeTo { get; set; }
		
		/// <summary>
		/// Type of relationship the parties are in
		/// </summary>
		[JsonProperty(PropertyName = "PartyRelationshipTypeId")]
		public int PartyRelationshipTypeId { get; set; }

		/// <summary>
		/// Type of relationship detail the parties are in
		/// </summary>
		[JsonProperty("PartyRelationshipType", NullValueHandling = NullValueHandling.Ignore)]
		public IRelationshipType PartyRelationshipType { get; set; }

		/// <summary>
		/// Party Relationship From Date
		/// </summary>
		[JsonProperty(PropertyName = "FromDate")]
		public DateTime FromDate { get; set; }

		/// <summary>
		/// Party Relationship thru Date
		/// </summary>
		[JsonProperty(PropertyName = "ThruDate")]
		public DateTime ThruDate { get; set; }

		#region Examples
		/// <summary>
		/// Example for New Party Relationship method
		/// </summary>
		/// <returns>Newly Created PasswordPolicy Id</returns>
		public static PartyRelationshipOutputResult GetNewPartyRelationshipExample()
		{
			PartyRelationshipOutputResult result = new PartyRelationshipOutputResult();
			result.NewPartyRelationshipId = 1;
			return result;
		}

		/// <summary>
		/// Output result for New Party Relationship
		/// </summary>
		public class PartyRelationshipOutputResult
		{
			/// <summary>
			/// Represents the newly created Party Relationship Id
			/// </summary>
			public long NewPartyRelationshipId { get; set; }
		}
		#endregion
	}
}
