using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.BlackBook;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Maintenance;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.UnifiedLogin;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Configuration;

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
		/// <param name="addProductList"></param>
		/// <param name="processBlueBookMessage"></param>
		/// <returns></returns>
        ObjectOutput<OrganizationCreateResult, IErrorData> CreateOrganization(OrganizationCreate organization, List<int> addProductList, bool processBlueBookMessage = false);

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
		/// <returns>Organization object</returns>
		Organization GetOrganization(Guid realPageId, long? organizationPartyId = null);

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

		/// <summary>
		/// Used to update Organization use primary properties setting
		/// </summary>
		/// <param name="organization"></param>
		/// <returns></returns>
		void UpdateOrganizationUsePrimaryPropertySetting(Organization organization);
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

        /// <summary>
        /// List of Unified Login companies
        /// </summary>       
        /// <returns>List of Unified Login companies including admin user info</returns>
        List<UnifiedLoginCompany> GetUnifiedLoginCompanyList();

		/// <summary>
		/// Get Company list
		/// </summary>
		/// <param name="organizationName">organizationName</param>
		/// <param name="domain">domain</param>
		/// <param name="blueId">blueId</param>
		/// <param name="organizationId">organizationId</param>
		/// <param name="globals">globals</param>
		/// <returns>Company list</returns>
		List<CompanySetup> GetCompanyList(string organizationName, int domain, int? blueId, int organizationId, IDictionary<object, object> globals);

		/// <summary>
		/// Get Properties for a Organization
		/// </summary>
		/// <param name="companyInstanceId">companyInstanceId</param>
		/// <param name="propertyName">PropertyName</param>
		/// <param name="domain">Domain</param>
		/// <param name="blueId">blueId</param>
		/// <param name="status">Status</param>
		/// <param name="globals">datafilter</param>
		/// <param name="editorPersonaId">editorPersonaId</param>
		/// <param name="userPersonaId">userPersonaId</param>
		/// <param name="isSelectedProperties">isSelectedProperties</param>
		/// <param name="selectedProperties">selected/Unselected Properties</param>
		/// <param name="operatorCode">The Code of the operator to filter the property list to</param>
        /// <param name="operatorValue">The Value of the operator to filter the property list to</param>
        /// <returns>List of Properties for a company </returns>
        List<CompanyPropertySetup> GetPropertiesForCompany(Guid companyInstanceId, string propertyName = null, string domain = null, int? blueId = null, 
							int? status = null, IDictionary<object, object> globals = null, long editorPersonaId = 0, long userPersonaId = 0, 
							bool? isSelectedProperties = null, List<Guid> selectedProperties = null, string operatorCode = null, string operatorValue = null);

		/// <summary>
		/// Get Property By PropertyId
		/// </summary>
		/// <param name="propertyInstanceId"></param>
		/// <returns></returns>
		List<UPFMPropertyInstance> GetPropertyByInstanceId(Guid propertyInstanceId);

        /// <summary>
        /// Get Property By PropertyId List
        /// Get Property By PropertyId List
        /// Get Property By PropertyId List
        /// </summary>
        /// <param name="propertyInstanceIds"></param>
        /// <returns></returns>
		List<UPFMPropertyInstance> GetPropertiesByInstanceId(List<Guid> propertyInstanceIds);

        /// <summary>
        /// Process Property List.
        /// </summary>
        /// <param name="propertyInstanceId"></param>
        /// <param name="companyInstanceId"></param>
        /// <returns></returns>
        Task<IRepositoryResponse> ProcessPropertyList(UPFMPropertyInstance propertyInstanceId, Guid companyInstanceId);

        /// <summary>
        /// Update an existing Property Name
        /// </summary>
        /// <param name="property"></param>
        /// <param name="companyInstanceId"></param>
        /// <returns></returns>
        RepositoryResponse UpdateProperty(UPFMPropertyInstance property, Guid companyInstanceId);

		Task<RepositoryResponse> UpdatePropertyList(List<UPFMPropertyInstance> propertyList, Guid companyInstanceId);

        bool UpdatePropertyInSettingsAndActivityLogs(UPFMPropertyInstance property, Guid companyInstanceId, List<UPFMPropertyInstance> oldPropertyList);

        /// <summary>
        /// AddPropertyForOrganization
        /// </summary>
        /// <param name="property">property</param>
        /// <param name="companyInstanceID">company InstanceID</param>
        /// <returns></returns>
        RepositoryResponse AddPropertyForOrganization(UPFMPropertyInstance property, Guid companyInstanceID);

		/// <summary>
		/// AuditCompanyProductPropertiesToUPFM
		/// </summary>
		/// <param name="companyInstanceId">companyInstanceId</param>
		/// <param name="product">product</param>
		/// <returns></returns>
		List<PropertyAudit> AuditCompanyProductPropertiesToUPFM(Guid companyInstanceId, int product);

		/// <summary>
		/// GetSourceProductDetails
		/// </summary>
		/// <param name="propertyInstanceSourceId">propertyInstanceSourceId</param>
		/// <param name="source">source</param>
		/// <returns></returns>
		ProductPropertyDetails GetSourceProductDetails(string propertyInstanceSourceId, string source);
		/// <summary>
		/// Search Property Details By CustomerPropertyId(BlueId)
		/// </summary>
		/// <param name="customerPropertyId">customerPropertyId</param>
		/// <param name="booksCustomerMasterId">booksCustomerMasterId</param>
		/// <returns></returns>
		PropertyInstanceSearch SearchPropertyDetailsByCustomerPropertyId(string customerPropertyId, string booksCustomerMasterId);
		/// <summary>
		/// Insert A UPFM property instance
		/// </summary>
		/// <param name="propertyInstance"></param>
		/// <returns></returns>
		RepositoryResponse InsertUPFMPropertyInstance(UPFMPropertyInstance propertyInstance);

		/// <summary>
		/// Search Company Details By CustomerCompanyId
		/// </summary>
		/// <param name="customerCompanyId">customerPropertyId</param>
		/// <returns></returns>
		CompanyMaster SearchCompanyDetailsByCustomerCompanyId(long customerCompanyId);

		/// <summary>
		/// Delete Property For Organization
		/// </summary>
		/// <param name="propertyInstanceID"></param>
		/// <param name="companyInstanceID"></param>
		/// <returns></returns>
		RepositoryResponse DeletePropertyForOrganization(Guid propertyInstanceID, Guid companyInstanceID);

		/// <summary>
		/// Used to parse the list of valid product codes
		/// </summary>
		/// <param name="productCode"></param>
		/// <param name="addProductList"></param>
		/// <returns></returns>
		List<string> ParseProduct(List<string> productCode, List<int> addProductList);
		/// <summary>
		/// Get Organization setting value
		/// </summary>
		/// <param name="settingName">settingName</param>
		/// <param name="organizationPartyId">Optional organization PartyId</param>
		/// <returns>setting value</returns>
		string GetOrganizationSettingValue(long organizationPartyId, string settingName);

		/// <summary>
		/// UpdateUsePrimaryPropertyForOrganizationProduct
		/// </summary>
		/// <param name="organizationPartyId">organizationPartyId</param>
		/// <param name="productId">productId</param>
		/// <param name="usePrimaryProperty">usePrimaryProperty</param>
		/// <returns></returns>
		RepositoryResponse UpdateUsePrimaryPropertyForOrganizationProduct(long organizationPartyId, int productId, bool usePrimaryProperty);

        void DeleteQueuedOrganizations();

        /// <summary>
        /// Used to add a new organization into the queue to be deleted
        /// </summary>
        /// <param name="organizationRemovalQueue"></param>
        /// <returns></returns>
        OrganizationRemovalQueue InsertOrganizationRemovalQueue(OrganizationRemovalQueue organizationRemovalQueue);
		
		/// <summary>
		/// Enable Product On Other Products Activation
		/// </summary>
		/// <param name="addProductList">ProductsList</param>
		/// <returns></returns>
		List<int> EnableProductOnOtherProductsActivation(List<int> addProductList);

		/// <summary>
		///AddUpdateCompanyToUnifiedSettings
		/// </summary>
		/// <param name="companyInstanceID"></param>
		/// <param name="trasactionType"></param>
		/// <param name="customerEnvironment"></param>
		/// <returns></returns>
		bool AddUpdateCompanyToUnifiedSettings(string companyInstanceID, string trasactionType, string customerEnvironment = null);

        /// <summary>
        /// AddCompanyToJob
        /// </summary>
        /// <param name="companyInstanceID"></param>
        /// <param name="createdBy"></param>
        /// <param name="createUserPersonaId">Persona Id of the user creating the job</param>
        /// <param name="organizationIsActive">Indicates if the organization is active</param>
        /// <returns></returns>
        bool AddCompanyToJob(string companyInstanceID, long createdBy, long createUserPersonaId, int organizationIsActive);

        /// <summary>
        ///UpdateOrganizationThirdPartyIDP
        /// </summary>
        /// <param name="org"></param>
        /// <returns></returns>
        void UpdateOrganizationThirdPartyIDP(Organization org);

        /// <summary>
        /// Update an existing company batch job instance status.
        /// </summary>
        /// <param name="companyBatchJobId">The unique id of the company batch job to update.</param>
        /// <param name="statusTypeId">The status type id to apply to the batch job.</param>
		/// <param name="errorMessage">The error message to log.</param>
        /// <returns>Repository response object.</returns>
        Task<RepositoryResponse> UpdateCompanyInstance(long companyBatchJobId, int statusTypeId, string errorMessage);
    }
}