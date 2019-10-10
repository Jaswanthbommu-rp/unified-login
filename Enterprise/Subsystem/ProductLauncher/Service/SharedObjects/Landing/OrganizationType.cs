using Newtonsoft.Json;
using System;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects
{
	/// <summary>
	/// Organization Type
	/// </summary>
	public class OrganizationType : IOrganizationType
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