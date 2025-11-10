using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using System;
using System.Collections.Generic;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.UnifiedLogin;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Base;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Maintenance;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic;
using System.Threading.Tasks;

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
        RepositoryResponse UpdateOrganization(Organization organization);

        /// <summary>
        /// Used to get the Organization based on the realPageId, party id, customer master or master id
        /// </summary>
        /// <param name="realPageId">Organization unique identifier</param>
        /// <param name="organizationPartyId">Optional organization PartyId</param>
        /// <returns>Organization object</returns>
        Organization GetOrganization(Guid? realPageId = null, long? organizationPartyId = null);

        /// <summary>
        /// Used to update any company master id records that match the old id to a new id
        /// </summary>
        /// <param name="oldOrganization"></param>
        /// <param name="newOrganization"></param>
        /// <returns></returns>
        RepositoryResponse UpdateOrganizationBooksCompanyMasterId(Organization oldOrganization, Organization newOrganization);

        /// <summary>
        /// Used to get the Organization list
        /// </summary>
        /// <returns></returns>
        IList<Organization> GetOrganizationList();

        /// <summary>
        /// Used to get the Organization list by Books Customer master id
        /// </summary>
        /// <returns></returns>
        IList<Organization> GetOrganizationListByBooksCustomerMasterId(long blueBookId);

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
        /// Returns the Organization Setting Value
        /// <param name="settingName">SettingName</param>
        /// <param name="partyId">partyId</param>
        /// </summary>
        string GetOrganizationSettingValue(string settingName, long partyId);
        string GetOrganizationSettingValueByPersonaId(string settingName, long personaId);
        #endregion

        #region Organization Type methods
        /// <summary>
        /// Used to get the list of all Organization Types
        /// </summary>
        /// <returns>Organization object</returns>
        List<OrganizationType> ListOrganizationType();

        /// <summary>
        /// Used to get the list of all Organization Domains
        /// </summary>
        /// <returns>OrganizationDomain list</returns>
        List<OrganizationDomain> ListOrganizationDomain();

        /// <summary>
        /// Used to add a new organization domain
        /// </summary>
        /// <param name="organizationDomain"></param>
        /// <returns></returns>
        RepositoryResponse CreateOrganizationDomain(OrganizationDomain organizationDomain);


        /// <summary>
        /// Used to get a list of products by company id
        /// </summary>
        /// <param name="organizationRealPageId"></param>
        /// <returns>List of Products</returns>
        IList<ProductUI> GetProductsByCompany(Guid organizationRealPageId);

        /// <summary>
        /// List of Unified Login companies
        /// </summary>       
        /// <returns>List of Unified Login companies including admin user info</returns>
        List<UnifiedLoginCompany> GetUnifiedLoginCompanyList();

        /// <summary>
        /// Get Company List
        /// </summary>
        /// <param name="organizationName">organizationName</param>
        /// <param name="domain">domain</param>
        /// <param name="blueId">blueId</param>
        /// <param name="organizationId">organizationId</param>
        /// <param name="dataFilterSort">dataFilterSort</param>
        /// <returns></returns>
        List<CompanySetup> GetCompanyList(string organizationName, int domain, int? blueId, int organizationId, RequestParameter dataFilterSort = null);

        #endregion

        /// <summary>
        /// Used to get a list of organizations to delete
        /// </summary>
        /// <param name="batchSize"></param>
        /// <param name="retryCount"></param>
        /// <param name="includeErrorRecord"></param>
        List<OrganizationRemovalQueue> GetOrganizationToDelete(int batchSize, int retryCount, bool includeErrorRecord);

        /// <summary>
        /// Used to delete the specified company
        /// </summary>
        /// <param name="organizationRemovalQueueId"></param>
        /// <param name="partyId"></param>
        /// <param name="organizationRealPageId"></param>
        /// <returns></returns>
        long DeleteOrganization(int organizationRemovalQueueId, long partyId, Guid organizationRealPageId);

        /// <summary>
        /// Used to update the status of the OrganizationRemovalQueue
        /// </summary>
        /// <param name="organizationRemovalQueueId"></param>
        /// <param name="organizationRemovalQueueStatus"></param>
        /// <returns></returns>
        int UpdateOrganizationRemovalQueueStatus(int organizationRemovalQueueId, string organizationRemovalQueueStatus);

        /// <summary>
        /// Used to insert a new request to remove a UPFM company and data related to it in UDM
        /// </summary>
        /// <param name="orgRemovalQueue"></param>
        /// <returns></returns>
        OrganizationRemovalQueue InsertOrganizationRemovalQueue(OrganizationRemovalQueue orgRemovalQueue);

        /// <summary>
        /// Update the Organization ThirdPartyIDP
        /// </summary>
        /// <param name="organization">Organization object</param>
        /// <returns>Repository response object</returns>
        RepositoryResponse UpdateOrganizationThirdPartyIDP(Organization organization);
        List<IDPNames> GetCompanyIDPList(int organizationPartyId);

        /// <summary>
        /// Adds the specified company (organization) to an existing job.
        /// </summary>
        /// <param name="companyInstanceSourceId">The company instance source id of the organization to add.</param>
        /// <param name="createdBy"></param>
        /// <param name="createUserPersonaId"></param>
        /// <param name="organizationIsActive">Indicates if the organization is active</param>
        /// <returns>Repository response object</returns>
        RepositoryResponse AddCompanyToJob(string companyInstanceSourceId, long createdBy, long createUserPersonaId, int organizationIsActive);

        /// <summary>
        /// Updates the status for a company (organization) identified by its batch job ID.
        /// </summary>
        /// <param name="companyBatchJobId">The company batch job id.</param>
        /// <param name="statusTypeId">The status type id to apply to the company.</param>
        /// <param name="errorMessage">The error message, if any, associated with the status update.</param>
        /// <returns>Repository response object.</returns>
        Task<RepositoryResponse> UpdateCompanyStatus(long companyBatchJobId, int statusTypeId, string errorMessage);
    }
}