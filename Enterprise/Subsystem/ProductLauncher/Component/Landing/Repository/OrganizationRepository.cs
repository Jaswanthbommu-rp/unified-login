using RP.Enterprise.Foundation.DataAccess.Component;
using RP.Enterprise.Foundation.DataAccess.Component.Helper;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Base;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository
{
    /// <summary>
    /// Organization repository
    /// </summary>
    public class OrganizationRepository : BaseRepository, IOrganizationRepository
    {
        #region Constructor
        /// <summary>
        /// Base constructor
        /// </summary>
        public OrganizationRepository() : base(DbConnectionEnum.IdpConfigurationDb)
        {
        }

        /// <summary>
        /// Unit test constructor
        /// </summary>
        /// <param name="repository"></param>
        public OrganizationRepository(IRepository repository) : base(repository)
        {
        }

        #endregion

        #region public Organization methods

        /// <summary>
        /// Insert the Organization information
        /// </summary>
        /// <param name="organization">Organization object</param>
        /// <returns>Repository response object</returns>
        public RepositoryResponse InsertOrganization(Organization organization)
        {
            RepositoryResponse booksUpdateresult = new RepositoryResponse();
            RepositoryResponse newOrganization = new RepositoryResponse();

            IList<OrganizationType> organizationTypeList = ListOrganizationType();
            int organizationTypeId = organizationTypeList.ToList().Find(t => t.Name.ToUpper() == "Multifamily".ToUpper()).OrganizationTypeId;

            using (var repository = GetRepository())
            {
                repository.UnitOfWork.BeginTransaction();
                try
                {
                    dynamic paramNewOrg = new
                    {
                        OrganizationName = organization.Name,
                        BlueBookId = organization.BooksCustomerMasterId,
                        BlackBookId = organization.BooksMasterId,
                        OrganizationTypeId = organization?.organizationType?.OrganizationTypeId ?? organizationTypeId
                    };

                    newOrganization = repository.Execute<RepositoryResponse>(StoredProcNameConstants.SP_SetupOrganization, paramNewOrg);
                }
                catch (Exception exception)
                {
                    repository.UnitOfWork.Rollback();
                    newOrganization.ErrorMessage = "Failed to create organization";
                }
                repository.UnitOfWork.Commit();
            }
            return newOrganization;
        }

        /// <summary>
        /// Update the Organization information
        /// </summary>
        /// <param name="organization">Organization object</param>
        /// <returns>Repository response object</returns>
        public RepositoryResponse UpdateOrganization(IOrganization organization)
        {
            RepositoryResponse response = new RepositoryResponse();
            RepositoryResponse booksUpdateresult = new RepositoryResponse();

            IList<OrganizationType> organizationTypeList = ListOrganizationType();
            int organizationTypeId = organizationTypeList.ToList().Find(t => t.Name.Equals("Multifamily", StringComparison.OrdinalIgnoreCase)).OrganizationTypeId;

            using (var repository = GetRepository())
            {
                repository.UnitOfWork.BeginTransaction();
                try
                {
                    dynamic paramsList = new
                    {
                        organizationId = organization.RealPageId,
                        organizationName = organization.Name,
                        OrganizationTypeId = organization?.organizationType?.OrganizationTypeId ?? organizationTypeId
                    };

                    response = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_UpdateOrganization, paramsList);
                }
                catch (Exception exception)
                {
                    repository.UnitOfWork.Rollback();
                    response.ErrorMessage = "There was a problem updating the Organization";
                }
                repository.UnitOfWork.Commit();
                return response;
            }
        }

        /// <summary>
        /// Used to get the Organization based on the realPageId
        /// </summary>
        /// <param name="realPageId">Organization unique identifier</param>
        /// <param name="organizationPartyId">Optional organization PartyId</param>
        /// <param name="blueBookId">Optional blueBookId</param>
        /// <param name="blackBookId">Optional blackBookId</param>
        /// <returns>Organization object</returns>
        public Organization GetOrganization(Guid? realPageId = null, long? organizationPartyId = null, long? blueBookId = null, long? blackBookId = null)
        {
            dynamic param = new
            {
                RealPageId = (realPageId == Guid.Empty) ? null : realPageId,
                PartyId = organizationPartyId,
                BlueBookId = blueBookId,
                BlackBookId = blackBookId
            };

            IList<OrganizationType> organizationTypeList = ListOrganizationType();
            using (var repo = GetRepository())
            {
                Organization organization = repo.GetOne<Organization>(StoredProcNameConstants.SP_GetOrganization, param);

                if (organization != null)
                {
                    organization.organizationType = organizationTypeList.ToList().FirstOrDefault(o => o.OrganizationTypeId == organization.OrganizationTypeId);
                }
                return organization;
            }
        }

        /// <summary>
        /// Used to get the list of all Organizations
        /// </summary>
        /// <returns>Organization object</returns>
        public IList<Organization> GetOrganizationList()
        {
            dynamic param = null;

            IList<OrganizationType> organizationTypeList = ListOrganizationType();

            using (var repository = GetRepository())
            {
                IList<Organization> organizationList = repository.GetMany<Organization>(StoredProcNameConstants.SP_GetOrganization, param);

                organizationList.ToList().ForEach(o =>
                    o.organizationType = organizationTypeList.ToList().FirstOrDefault(t => t.OrganizationTypeId == o.OrganizationTypeId)
                );

                return organizationList;
            }
        }

        /// <summary>
        /// Used to get the RealPageId of the admin user of the organization
        /// </summary>
        /// <param name="organizationRealPageId"></param>
        /// <returns></returns>
        public Guid GetOrganizationAdminUserRealPageId(Guid organizationRealPageId)
        {
            Guid realPageEmployeeAccessId = Guid.Empty;
            RPObjectCache rpCache = new RPObjectCache();

            var cacheKey = $"getOrganizationAdminUserRealPageId_{organizationRealPageId}";
            var realPageEmployeeAccessIdString = rpCache.GetFromCache<string>(cacheKey, 180, () =>
            {
                string realPageId = "";
                using (var repository = GetRepository())
                {
                    dynamic param = new
                    {
                        RealPageId = organizationRealPageId
                    };
                    var result = repository.GetMany<dynamic>(StoredProcNameConstants.SP_ListOrganizations, param);
                    if (result != null)
                    {
                        foreach (var item in result)
                        {
                            realPageId = item.PersonRealPageId;
                        }
                    }
                }

                return realPageId;
            });

            if (!string.IsNullOrEmpty(realPageEmployeeAccessIdString))
            {
                realPageEmployeeAccessId = new Guid(realPageEmployeeAccessIdString);
            }

            return realPageEmployeeAccessId;
        }

        /// <summary>
        /// Used to get the Books master id for the given organization
        /// </summary>
        /// <param name="realPageId">The organization id to get the books master id for</param>
        /// <returns>the books master id</returns>
        public BooksMaster GetBooksCompanyMaster(Guid realPageId)
        {
            RPObjectCache rpCache = new RPObjectCache();
            var cacheKey = $"booksMaster_{realPageId}";
            BooksMaster booksMaster = rpCache.GetFromCache<BooksMaster>(cacheKey, 180, () =>
            {
                dynamic param = new
                {
                    RealPageId = realPageId,
                };

                using (var repository = GetRepository())
                {
                    return repository.GetOne<BooksMaster>(StoredProcNameConstants.SP_GetBookIdByOrganization, param);
                }
            });

            return booksMaster;
        }

        /// <summary>
        /// Used to update any company master id records that match the old id to a new id
        /// </summary>
        /// <param name="oldOrganization"></param>
        /// <param name="newOrganization"></param>
        /// <returns></returns>
        public RepositoryResponse UpdateOrganizationBooksCompanyMasterId(Organization oldOrganization, Organization newOrganization)
        {
            RepositoryResponse result = new RepositoryResponse() {Id = 0, ErrorMessage = ""};
            
            dynamic param = new
            {
                @ApplicationId = BookMasterType.CustomerMasterId,
                @PartyId = oldOrganization.PartyId,
                @Original_SourceId = oldOrganization.BooksCustomerMasterId,
                @SourceId = newOrganization.BooksCustomerMasterId,
            };

            using (var repository = GetRepository())
            {
                repository.UnitOfWork.BeginTransaction();
                try
                {
                    result = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_DataImportMappingUpdate, param);
                }
                catch (Exception exception)
                {
                    result.ErrorMessage = exception.Message;
                }
                finally
                {
                    if (result.ErrorMessage.Length == 0)
                    {
                        repository.UnitOfWork.Commit();
                    }
                    else
                    {
                        repository.UnitOfWork.Rollback();
                    }
                }
                return result;
            }
        }

        /// <summary>
        /// Used to get the Organization Identity ProviderType by realPageId
        /// </summary>
        /// <param name="realPageId">Organization unique identifier</param>
        /// <returns>Identity Provider Type object</returns>
        public IList<IdentityProviderType> GetOrganizationIdentityProviderType(Guid realPageId)
        {
            dynamic param = new
            {
                RealPageId = realPageId,
            };
            using (var repository = GetRepository())
            {
                IList<IdentityProviderType> identityProviderTypeList = repository.GetMany<IdentityProviderType>(StoredProcNameConstants.SP_GetOrganizationIdentityProviderType, param);
                return identityProviderTypeList;
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
        /// <param name="productIdList">List of product ids</param>
        /// <returns></returns>
        public RepositoryResponse CreateInitialOrgSuperUser(long organizationId, string firstName, string middleName, string lastName, string title, string suffix, string email, bool defaultIDP, int? idpTypeId, IList<int> productIdList)
        {
            RepositoryResponse response = new RepositoryResponse();

            using (var repository = GetRepository())
            {
                repository.UnitOfWork.BeginTransaction();
                try
                {
                    dynamic param = new
                    {
                        OrganizationId = organizationId,
                        FirstName = firstName,
                        MiddleName = middleName,
                        LastName = lastName,
                        Title = title,
                        Suffix = suffix,
                        Email = email,
                        DefaultIDP = defaultIDP,
                        IDPTypeId = idpTypeId,
                        AssignedProductId = TableValueParamHelper.ConvertToTableValuedParameter(productIdList, "enterprise.productidtype")
                    };

                    repository.ExecuteNonQuery(StoredProcNameConstants.SP_SetupSuperUser, param);
                }
                catch (Exception exception)
                {
                    repository.UnitOfWork.Rollback();
                    response.ErrorMessage = "Failed to create the super user";
                }
                repository.UnitOfWork.Commit();
            }
            return response;
        }

        /// <summary>
        /// Used to get a list of products by company id
        /// </summary>
        /// <param name="organizationRealPageId"></param>
        /// <returns>List of Products</returns>
        public IList<ProductUI> GetProductsByCompany(Guid organizationRealPageId)
        {
            //IList<ProductUI> products = new List<ProductUI>();
            RPObjectCache rpCache = new RPObjectCache();
            var cacheKey = $"getProductsByCompany_{organizationRealPageId}";

            IList<ProductUI> products = rpCache.GetFromCache<IList<ProductUI>>(cacheKey, 180, () =>
            {
                using (var repository = GetRepository())
                {
                    products = repository.GetMany<ProductUI>(StoredProcNameConstants.SP_ListProductsByOrganization, new { OrganizationRealPageId = organizationRealPageId }).ToList();
                }

                return products;
            });

            return products;
        }

        /// <summary>
        /// Used to get a list of product ids by company id
        /// </summary>
        /// <param name="organizationRealPageId"></param>
        /// <returns></returns>
        public IList<int> GetProductIdsByCompany(Guid organizationRealPageId)
        {
            RPObjectCache rpCache = new RPObjectCache();
            var cacheKey = $"getProductIdsByCompany_{organizationRealPageId}";

            IList<int> products = rpCache.GetFromCache<IList<int>>(cacheKey, 180, () =>
            {
                IList<int> productIdList = new List<int>();

                using (var repository = GetRepository())
                {
                    IList<ProductUI> productList = repository.GetMany<ProductUI>(StoredProcNameConstants.SP_ListProductsByOrganization, new { OrganizationRealPageId = organizationRealPageId }).ToList();
                    foreach (ProductUI pui in productList)
                    {
                        productIdList.Add(pui.ProductId);
                    }
                }
                return productIdList;
            });

            return products;
        }
        #endregion

        #region public Organization type methods
        /// <summary>
        /// Used to get the list of all Organization Types
        /// </summary>
        /// <returns>Organization object</returns>
        public IList<OrganizationType> ListOrganizationType()
        {
            RPObjectCache rpCache = new RPObjectCache();

            string cacheKey = $"getListOrganizationType";
            IList<OrganizationType> organizationTypeList = rpCache.GetFromCache<IList<OrganizationType>>(cacheKey, 180, () =>
            {
                dynamic param = null;
                using (var repository = GetRepository())
                {
                    return repository.GetMany<OrganizationType>(StoredProcNameConstants.SP_ListOrganizationType, param);
                }
            });

            return organizationTypeList;
        }
        #endregion
    }
}