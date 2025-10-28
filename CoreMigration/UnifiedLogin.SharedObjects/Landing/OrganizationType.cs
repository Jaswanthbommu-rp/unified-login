using Newtonsoft.Json;
using System;

namespace UnifiedLogin.SharedObjects.Landing
{
	/// <summary>
	/// Organization Type
	/// </summary>
	public class OrganizationType
	{
		/// <summary>
		/// The date the Organization Type was added to GreenBook
		/// </summary>
		[JsonProperty(PropertyName = "CreateDate")]
		public DateTime CreateDate { get; set; }

		/// <summary>
		/// Organization Type Name
		/// </summary>
		[JsonProperty(PropertyName = "Name")]
		public string Name { get; set; }

		/// <summary>
		/// Unique Organization Type Id
		/// </summary>
		[JsonProperty(PropertyName = "OrganizationTypeId")]
		public int OrganizationTypeId { get; set; }
	}
}