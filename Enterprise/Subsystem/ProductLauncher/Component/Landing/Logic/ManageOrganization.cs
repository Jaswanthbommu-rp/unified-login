using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Helper;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using System;
using System.Collections.Generic;
using System.Linq;
using RP.Enterprise.Foundation.DataAccess.Component;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic
{
    /// <summary>
    /// Manage Organization repository calls
    /// </summary>
    public class ManageOrganization : IManageOrganization
    {
        #region Private Variables
        IOrganizationRepository _organizationRepository;
        ICredentialRepository _credentialRepository;
        #endregion

        #region Constructors
        /// <summary>
        /// Constructor
        /// </summary>
        public ManageOrganization(IRepository repository)
        {
            _organizationRepository = new OrganizationRepository(repository);
            _credentialRepository = new CredentialRepository(repository);
        }

        /// <summary>
        /// Create a basic instance of the ManageOrganization Controller class
        /// </summary>
        public ManageOrganization()
        {
            _organizationRepository = new OrganizationRepository();
            _credentialRepository = new CredentialRepository();
        }
        #endregion

        #region Public Organization methods

        /// <summary>
        /// Used to insert a new Organization
        /// </summary>
        /// <param name="organization">Organization Object</param>
        /// <returns>RepositoryResponse object</returns>
        public RepositoryResponse InsertOrganization(Organization organization)
        {
            if (organization == null)
            {
                throw new ArgumentNullException(nameof(organization), "Null Organization.");
            }
            // see if the organization.BooksMasterId already exists
            if (organization.RealPageId != Guid.Empty)
            {
                return _organizationRepository.UpdateOrganization(organization);
            }
            else
            {
                return _organizationRepository.InsertOrganization(organization);
            }
        }

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
        public RepositoryResponse CreateInitialOrgSuperUser(long organizationId, string firstName, string middleName, string lastName, string title, string suffix, string email, bool defaultIDP, int? idpTypeId, Guid organizationRealPageId)
        {
            ProductRepository productRepository = new ProductRepository();

            IList<int> productIdList = productRepository.GetProductIdsByCompany(organizationRealPageId);

            //Exclude following products from RealPage Employee Access admin user
            //Unified Platform, Asset Optimization, RealPage Accounting, Client Portal, Product Updates, EasyLMS
            productIdList.Remove(productIdList.FirstOrDefault(p => p == (int)ProductEnum.UnifiedLogin));
            productIdList.Remove(productIdList.FirstOrDefault(p => p == (int)ProductEnum.AssetOptimizer));
            productIdList.Remove(productIdList.FirstOrDefault(p => p == (int)ProductEnum.FinancialSuite));
            productIdList.Remove(productIdList.FirstOrDefault(p => p == (int)ProductEnum.ClientPortal));
            productIdList.Remove(productIdList.FirstOrDefault(p => p == (int)ProductEnum.ProductUpdates));
            productIdList.Remove(productIdList.FirstOrDefault(p => p == (int)ProductEnum.EasyLMS));

            return _organizationRepository.CreateInitialOrgSuperUser(organizationId, firstName, middleName, lastName, title, suffix, email, defaultIDP, idpTypeId, productIdList);
        }

        /// <summary>
        /// Used to update an existing Organization
        /// </summary>
        /// <param name="organization">Organization Object</param>
        /// <returns>RepositoryResponse object</returns>
        public RepositoryResponse UpdateOrganization(IOrganization organization)
        {
            if (organization == null)
            {
                throw new ArgumentNullException(nameof(organization), "Null Organization.");
            }

            if (organization.RealPageId == Guid.Empty)
            {
                throw new Exception("Invalid parameter realPageId.");
            }
            return _organizationRepository.UpdateOrganization(organization);
        }

        /// <summary>
        /// Get Organization details
        /// </summary>
        /// <param name="realPageId">Organization unique identifier</param>
        /// <param name="organizationPartyId">Optional organization PartyId</param>
        /// <param name="blueBookId">Optional blueBookId</param>
        /// <param name="blackBookId">Optional blackBookId</param>
        /// <returns>Organization object</returns>
        public Organization GetOrganization(Guid realPageId, long? organizationPartyId = null, long? blueBookId = null, long? blackBookId = null)
        {
            if ((realPageId == Guid.Empty) && (organizationPartyId == null) && (blueBookId == null) && (blackBookId == null))
            {
                throw new Exception("Invalid parameter: Organization realPageId, partyId, blueBook Id, or blackBook Id is required.");
            }
            Organization organization = _organizationRepository.GetOrganization(realPageId, organizationPartyId, blueBookId, blackBookId);
            return organization;
        }

        /// <summary>
        /// Used to get the list of organizations
        /// </summary>
        /// <returns>List of Organization</returns>
        public IList<Organization> GetOrganizationList()
        {
            IList<Organization> orgList = _organizationRepository.GetOrganizationList();
            return orgList;
        }

        /// <summary>
        /// Used to get the RealPageId of the admin user of the organization
        /// </summary>
        /// <param name="organizationRealPageId"></param>
        /// <returns></returns>
        public Guid GetOrganizationAdminUserRealPageId(Guid organizationRealPageId)
        {
            return _organizationRepository.GetOrganizationAdminUserRealPageId(organizationRealPageId);
        }

        /// <summary>
        /// Used to get the master id for the given organization
        /// </summary>
        /// <param name="realPageId"></param>
        /// <returns>BooksMaster object</returns>
            public BooksMaster GetBooksCompanyMaster(Guid realPageId)
        {
            if (realPageId == Guid.Empty)
            {
                throw new Exception("Invalid parameter realPageId.");
            }
            return _organizationRepository.GetBooksCompanyMaster(realPageId); ;
        }

        /// <summary>
        /// Used to get the Organization Identity ProviderType by realPageId
        /// </summary>
        /// <param name="realPageId">Organization unique identifier</param>
        /// <returns>Identity Provider Type object</returns>
        public IList<IdentityProviderType> GetOrganizationIdentityProviderType(Guid realPageId)
        {
            return _organizationRepository.GetOrganizationIdentityProviderType(realPageId);
        }

        /// <summary>
        /// Check if organization is the same
        /// </summary>
        /// <param name="organizationMasterId">The master id for the RealPage company</param>
        /// <param name="realPageId">User RealPageId</param>
        /// <param name="organizationId">User Organization RealPageId</param>
        public bool ValidateOrganization(long organizationMasterId, Guid realPageId, Guid organizationId)
        {
            if (organizationMasterId == 0)
            {
                throw new ArgumentNullException(nameof(realPageId), "OrganizationMasterId is required.");
            }

            if (realPageId == null)
            {
                throw new ArgumentNullException(nameof(realPageId), "RealPageId is required.");
            }

            if (organizationId == null)
            {
                throw new ArgumentException(nameof(organizationId), "OrganizationId is required.");
            }

            IList<Organization> listOrg = _credentialRepository.ListOrganizationByRealPageId(realPageId);
            if (listOrg != null)
            {
                if (organizationMasterId == Convert.ToInt64(ConfigReader.OrgMasterId) || listOrg.Any(a => a.RealPageId == organizationId))
                {
                    return true;
                }
            }
            return false;
        }
        #endregion

        #region Public Organization Type methods
        /// <summary>
        /// Used to list the Organization Types
        /// </summary>
        /// <returns>list of OrganizationType objects</returns>
        public IList<OrganizationType> ListOrganizationType()
        {
            return _organizationRepository.ListOrganizationType();
        }
        #endregion
    }
}