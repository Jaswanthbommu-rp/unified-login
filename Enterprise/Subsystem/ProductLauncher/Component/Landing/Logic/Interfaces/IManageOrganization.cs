using System;
using System.Collections.Generic;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces
{
	/// <summary>
	/// Manage Organization repository calls
	/// </summary>
	public interface IManageOrganization
	{
		#region Organization methods

		/// <summary>
		/// Used to create a new organization with product details
		/// </summary>
		/// <param name="organization"></param>
		/// <param name="processBlueBookMessage"></param>
        ObjectOutput<OrganizationCreateResult, IErrorData> CreateOrganization(OrganizationCreate organization, bool processBlueBookMessage = false);

		/// <summary>
		/// Used to insert a new Organization
		/// </summary>
		/// <param name="organization"></param>
		/// <returns></returns>
		RepositoryResponse InsertOrganization(Organization organization);

		/// <summary>
		/// Used to update a new Organization
		/// </summary>
		/// <param name="organization"></param>
		/// <returns></returns>
		RepositoryResponse UpdateOrganization(Organization organization);

		/// <summary>
		/// Get Organization details
		/// </summary>
		/// <param name="realPageId">Organization unique identifier</param>
		/// <param name="organizationPartyId">Optional organization PartyId</param>
		/// <param name="blueBookId">Optional blueBookId</param>
		/// <param name="blackBookId">Optional blackBookId</param>
		/// <returns>Organization object</returns>
		Organization GetOrganization(Guid realPageId, long? organizationPartyId = null, long? blueBookId = null, long? blackBookId = null);

		/// <summary>
		/// Used to get a list of Organizations
		/// </summary>
		/// <returns></returns>
		IList<Organization> GetOrganizationList();

        /// <summary>
        /// Used to get the RealPageId of the admin user of the organization
        /// </summary>
        /// <param name="organizationRealPageId"></param>
        /// <returns></returns>
        Guid GetOrganizationAdminUserRealPageId(Guid organizationRealPageId);

        /// <summary>
        /// Used to get the Organization Identity ProviderType by realPageId
        /// </summary>
        /// <param name="realPageId">Organization unique identifier</param>
        /// <returns>Identity Provider Type object</returns>
        IList<IdentityProviderType> GetOrganizationIdentityProviderType(Guid realPageId);

		/// <summary>
		/// Check if organization is the same
		/// </summary>
		/// <param name="organizationMasterId">The master id for the RealPage company</param>
		/// <param name="realPageId">User RealPageId</param>
		/// <param name="organizationId">User Organization RealPageId</param>
		bool ValidateOrganization(long organizationMasterId, Guid realPageId, Guid organizationId);

		/// <summary>
		/// Used to create the initial Super User for the new Organization
		/// </summary>
		/// <param name="organizationId">The partyId of the organization where the user will be added</param>
		/// <param name="firstName">The users first name</param>
		/// <param name="middleName">The users middle name</param>
		/// <param name="lastName">The users last name</param>
		/// <param name="title">The users title</param>
		/// <param name="suffix">The users suffix</param>
		/// <param name="email">The users email address</param>
		/// <param name="defaultIDP">Should the user be assigned to the default IDP</param>
		/// <param name="idpTypeId">The id of the IDP to assign the user to</param>
		/// <param name="organizationRealPageId">Organization Enterprise RealPageId</param>
		/// <returns>RepositoryResponse object</returns>
		RepositoryResponse CreateInitialOrgSuperUser(long organizationId, string firstName, string middleName, string lastName, string title, string suffix, string email, bool defaultIDP, int? idpTypeId, Guid organizationRealPageId);
		#endregion

		#region Organization Type methods
		/// <summary>
		/// Used to list the Organization Types
		/// </summary>
		/// <returns>list of OrganizationType objects</returns>
		List<OrganizationType> ListOrganizationType();
		#endregion

        /// <summary>
        /// Used to list the Organization Domains
        /// </summary>
        /// <returns>list of OrganizationDomain objects</returns>
        List<OrganizationDomain> ListOrganizationDomain();

        /// <summary>
        /// Used to add a new organization domain
        /// </summary>
        /// <param name="organizationDomain"></param>
        /// <returns></returns>
        RepositoryResponse CreateOrganizationDomain(OrganizationDomain organizationDomain);
    }
}