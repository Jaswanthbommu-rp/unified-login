using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using System;
using System.Collections.Generic;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces
{
    /// <summary>
    /// Interface for the Organization repository
    /// </summary>
    public interface IOrganizationRepository
    {
        #region Organization methods
        
        /// <summary>
        /// Insert the Organization information
        /// </summary>
        /// <param name="organization">Organization object</param>
        /// <returns>Repository response object</returns>
        RepositoryResponse InsertOrganization(Organization organization);

        /// <summary>
        /// Update the Organization information
        /// </summary>
        /// <param name="organization">Organization object</param>
        /// <returns>Repository response object</returns>
        RepositoryResponse UpdateOrganization(IOrganization organization);

        /// <summary>
        /// Used to get the Organization based on the realPageId, party id, customer master or master id
        /// </summary>
        /// <param name="realPageId">Organization unique identifier</param>
        /// <param name="organizationPartyId">Optional organization PartyId</param>
        /// <param name="blueBookId">Optional blueBookId</param>
        /// <param name="blackBookId">Optional blackBookId</param>
        /// <returns>Organization object</returns>
        Organization GetOrganization(Guid? realPageId = null, long? organizationPartyId = null, long? blueBookId = null, long? blackBookId = null);

        /// <summary>
        /// Used to get the Organization list
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
        /// 
        /// </summary>
        /// <param name="realPageId"></param>
        /// <returns></returns>
        BooksMaster GetBooksCompanyMaster(Guid realPageId);

        /// <summary>
        /// Used to get the Organization Identity ProviderType by realPageId
        /// </summary>
        /// <param name="realPageId">Organization unique identifier</param>
        /// <returns>Identity Provider Type object</returns>
        IList<IdentityProviderType> GetOrganizationIdentityProviderType(Guid realPageId);

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
        /// <param name="productIdList">List of product ids</param>   
        /// <returns></returns>
        RepositoryResponse CreateInitialOrgSuperUser(long organizationId, string firstName, string middleName, string lastName, string title, string suffix, string email, bool defaultIDP, int? idpTypeId, IList<int> productIdList);

        /// <summary>
        /// Used to get or list the RealPageId of the admin user(s) of the organization
        /// </summary>
        /// <param name="organizationRealPageId">Optional organization enterprise Id</param>
        /// <returns>List of admin user of the organization</returns>
        IList<dynamic> ListOrganizationAdmin(Guid? organizationRealPageId = null);
        #endregion

        #region Organization Type methods
        /// <summary>
        /// Used to get the list of all Organization Types
        /// </summary>
        /// <returns>Organization object</returns>
        IList<OrganizationType> ListOrganizationType();


        /// <summary>
        /// Used to get a list of products by company id
        /// </summary>
        /// <param name="organizationRealPageId"></param>
        /// <returns>List of Products</returns>
        IList<ProductUI> GetProductsByCompany(Guid organizationRealPageId);

        #endregion
    }
}