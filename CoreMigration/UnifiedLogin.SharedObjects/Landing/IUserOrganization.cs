using System;

namespace UnifiedLogin.SharedObjects.Landing
{
	/// <summary>
	/// Interface for UserPersonaOrganization
	/// </summary>
	public interface IUserOrganization
	{
        /// <summary>
        /// Role name
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// The name of the Organization
        /// </summary>
        long OrganizationPartyId { get; set; }

        /// <summary>
        /// The unique id for the Organization
        /// </summary>
        Guid OrganizationRealPageId { get; set; }

        /// <summary>
        /// Party RoleTypeId
        /// </summary>
        int PartyRoleTypeId { get; set; }

        /// <summary>
        /// Is the org the primary login org for the user
        /// </summary>
        bool PrimaryOrganization { get; set; }

        /// <summary>
        /// Used to store the BlueBook master id for the organization
        /// </summary>
        long BooksMasterId { get; set; }

        /// <summary>
        /// Used to store the BlackBook Company master id for the organization RPUP id
        /// </summary>
        long BooksCustomerMasterId { get; set; }
    }
}