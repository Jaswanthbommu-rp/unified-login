using System;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects
{
	/// <summary>
	/// Interface for Organization Type
	/// </summary>
	public interface IOrganizationType
	{
		/// <summary>
		/// The date the Organization Type was added to GreenBook
		/// </summary>
		DateTime CreateDate { get; set; }

		/// <summary>
		/// Organization Type Name
		/// </summary>
		string Name { get; set; }

		/// <summary>
		/// Unique Organization Type Id
		/// </summary>
		int OrganizationTypeId { get; set; }
	}
}