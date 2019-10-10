using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using System;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects
{
	/// <summary>
	/// Interface for Organization
	/// </summary>
	public interface IOrganization
	{
		/// <summary>
		/// The date the Organization was added to GreenBook
		/// </summary>
		DateTime CreateDate { get; set; }

		/// <summary>
		/// The name of the Organization
		/// </summary>
		string Name { get; set; }

        /// <summary>
        /// The unique id for the Organization in GreenBook
        /// </summary>
        long PartyId { get; set; }

		/// <summary>
		/// The unique id for the Organization in BlueBook
		/// </summary>
		Guid RealPageId { get; set; }

		/// <summary>
		/// Party Relationship
		/// </summary>
		PartyRelationship partyRelationship { get; set; }

        /// <summary>
        /// Used to store the books master id for the organization
        /// </summary>
        long BooksMasterId { get; set; }

        /// <summary>
        /// Used to store the BlackBook Company master id for the organization RPUP id
        /// </summary>
        long BooksCustomerMasterId { get; set; }

		/// <summary>
		/// Organization Type Id
		/// </summary>
		int OrganizationTypeId { get; set; }
		
		/// <summary>
		/// Organization Type
		/// </summary>
		OrganizationType organizationType { get; set; }
	}
}