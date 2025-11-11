using Dapper;
using RP.Enterprise.Foundation.DataAccess.Component;
using RP.Enterprise.Foundation.DataAccess.Component.Helper;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Base;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Maintenance;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.UnifiedLogin;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository
{
    /// <summary>
    /// Organization repository
    /// </summary>
    public class OrganizationRepository : BaseRepository, IOrganizationRepository
    {
        private IRepository _repository;

        #region Constructor
        /// <summary>
        /// Base constructor
        /// </summary>
        public OrganizationRepository() : base(DbConnectionEnum.IdpConfigurationDb)
        {
        }

        /// <summary>
        ///  Unit test constructor
        /// </summary>
        /// <param name="userClaim"></param>
        public OrganizationRepository(DefaultUserClaim userClaim) : base(DbConnectionEnum.IdpConfigurationDb)
        {
        }

        /// <summary>
        /// Unit test constructor v2
        /// </summary>
        /// <param name="repository"></param>
        public OrganizationRepository(IRepository repository) : base(repository)
        {
            _repository = repository;
           
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
                        OrganizationTypeId = organization?.organizationType?.OrganizationTypeId ?? organizationTypeId,
                        OrganizationDomainId = organization.OrganizationDomain.OrganizationDomainId,
                        OrganizationStatus = organization.IsActive
                    };

                    newOrganization = repository.Execute<RepositoryResponse>(StoredProcNameConstants.SP_SetupOrganization, paramNewOrg);
                    repository.UnitOfWork.Commit();
                }
                catch (Exception exception)
                {
                    repository.UnitOfWork.Rollback();
                    newOrganization.ErrorMessage = "Failed to create organization";
                }
            }
            return newOrganization;
        }

        /// <summary>
        /// Update the Organization information
        /// </summary>
        /// <param name="organization">Organization object</param>
        /// <returns>Repository response object</returns>
        public RepositoryResponse UpdateOrganization(Organization organization)
        {
            RepositoryResponse response = new RepositoryResponse();

            int organizationTypeId = ListOrganizationType().Find(t => t.Name != null && t.Name.Equals("Multifamily", StringComparison.OrdinalIgnoreCase)).OrganizationTypeId;
            int organizationDomainId = ListOrganizationDomain().ToList().Find(d => d.Name != null && d.Name.Equals("Primary", StringComparison.OrdinalIgnoreCase)).OrganizationDomainId;

            using (var repository = GetRepository())
            {
                repository.UnitOfWork.BeginTransaction();
                try
                {
                    dynamic paramsList = new
                    {
                        organizationId = organization?.RealPageId,
                        organizationName = organization?.Name,
                        OrganizationTypeId = organization?.organizationType?.OrganizationTypeId ?? organizationTypeId,
                        OrganizationDomainId = organization?.OrganizationDomain?.OrganizationDomainId ?? organizationDomainId,
                        OrganizationStatus = organization?.IsActive
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

        public RepositoryResponse AddCompanyToJob(string companyInstanceSourceId, long createdBy, long createUserPersonaId, int organizationIsActive)
        {
            RepositoryResponse response = new RepositoryResponse();
            using (var repository = GetRepository())
            {
                repository.UnitOfWork.BeginTransaction();
                try
                {
                    dynamic param = new
                    {
                        CompanyInstanceSourceId = companyInstanceSourceId,
                        StatusTypeId = 5,
                        CreatedBy = createdBy,
                        CreateUserPersonaId = createUserPersonaId,
                        IsActive = organizationIsActive,
                        BatchProcessTypeId = BatchProcessType.CompanyPropertyUpdate
                    };
                    response = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_InsertBatchCompanyJob, param);
                }
                catch (Exception)
                {
                    repository.UnitOfWork.Rollback();
                    response.ErrorMessage = "There was a problem adding the company to the job";
                }
                repository.UnitOfWork.Commit();
                return response;
            }
        }

        public async Task<RepositoryResponse> UpdateCompanyStatus(long companyBatchJobId, int statusTypeId, string errorMessage)
        {
            var response = new RepositoryResponse();
            using (var repository = GetRepository())
            {
                repository.UnitOfWork.BeginTransaction();
                try
                {
                    var param = new
                    {
                        CompanyBatchJobId = companyBatchJobId,
                        StatusTypeId = statusTypeId,
                        errorMessage = errorMessage
                    };
                    // If an async version exists use it, else keep synchronous call.
                    // response = await repository.GetOneAsync<RepositoryResponse>(StoredProcNameConstants.SP_UpdateCompanyStatus, param);
                    response = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_UpdateCompanyStatus, param);
                    repository.UnitOfWork.Commit();
                }
                catch (Exception)
                {
                    repository.UnitOfWork.Rollback();
                    response.ErrorMessage = "There was a problem updating the company status";
                }
            }
            return response;
        }

        /// <summary>
        /// Update the Organization ThirdPartyIDP
        /// </summary>
        /// <param name="organization">Organization object</param>
        /// <returns>Repository response object</returns>
        public RepositoryResponse UpdateOrganizationThirdPartyIDP(Organization organization)
        {
            RepositoryResponse response = new RepositoryResponse();

            using (var repository = GetRepository())
            {
                repository.UnitOfWork.BeginTransaction();
                try
                {
                    dynamic paramsList = new
                    {
                        organizationPartyId = organization?.PartyId,
                        ThirdPartyIDP = organization?.ThirdPartyIDP
                    };

                    response = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_UpdateOrganizationThirdPartyIDP, paramsList);
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
        /// <returns>Organization object</returns>
        public Organization GetOrganization(Guid? realPageId = null, long? organizationPartyId = null)
        {
            dynamic param = new
            {
                RealPageId = (realPageId == Guid.Empty) ? null : realPageId,
                PartyId = organizationPartyId
            };

            using (var repo = GetRepository())
            {
                Organization organization = repo.GetOne<Organization>(StoredProcNameConstants.SP_GetOrganization, param);

                if (organization != null)
                {
                    var orgType = ListOrganizationType().FirstOrDefault(o => o.OrganizationTypeId == organization.OrganizationTypeId);
                    organization.organizationType = orgType != null ? new OrganizationType {Name = orgType.Name, OrganizationTypeId = orgType.OrganizationTypeId, CreateDate = orgType.CreateDate} : new OrganizationType();
                    var orgDomain = ListOrganizationDomain().FirstOrDefault(d => d.OrganizationDomainId == organization.OrganizationDomainId);
                    organization.OrganizationDomain = orgDomain != null ? new OrganizationDomain {OrganizationDomainId = orgDomain.OrganizationDomainId, Name = orgDomain.Name, CreateDate = orgDomain.CreateDate} : new OrganizationDomain();
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

            using (var repository = GetRepository())
            {
                IList<Organization> organizationList = repository.GetMany<Organization>(StoredProcNameConstants.SP_GetOrganization, param);

                organizationList.ToList().ForEach(o =>
                {
                    var orgType = ListOrganizationType().FirstOrDefault(t => t.OrganizationTypeId == o.OrganizationTypeId);
                    o.organizationType = orgType != null ? new OrganizationType {Name = orgType.Name, OrganizationTypeId = orgType.OrganizationTypeId, CreateDate = orgType.CreateDate} : new OrganizationType();
                    var orgDomain = ListOrganizationDomain().FirstOrDefault(d => d.OrganizationDomainId == o.OrganizationDomainId);
                    o.OrganizationDomain = orgDomain != null ? new OrganizationDomain {OrganizationDomainId = orgDomain.OrganizationDomainId, Name = orgDomain.Name, CreateDate = orgDomain.CreateDate} : new OrganizationDomain();
                }
                );

                return organizationList;
            }
        }

        /// <summary>
        /// Used to get the list of all Organizations
        /// </summary>
        /// <returns>Organization object</returns>
        public IList<Organization> GetOrganizationListByBooksCustomerMasterId(long blueBookId)
        {
            dynamic param = new
            {
                BlueBookId = blueBookId
            };

            using (var repository = GetRepository())
            {
                IList<Organization> organizationList = repository.GetMany<Organization>(StoredProcNameConstants.SP_GetOrganization, param);

                organizationList.ToList().ForEach(o =>
                {
                    var orgType = ListOrganizationType().FirstOrDefault(t => t.OrganizationTypeId == o.OrganizationTypeId);
                    o.organizationType = orgType != null ? new OrganizationType { Name = orgType.Name, OrganizationTypeId = orgType.OrganizationTypeId, CreateDate = orgType.CreateDate } : new OrganizationType();
                    var orgDomain = ListOrganizationDomain().FirstOrDefault(d => d.OrganizationDomainId == o.OrganizationDomainId);
                    o.OrganizationDomain = orgDomain != null ? new OrganizationDomain { OrganizationDomainId = orgDomain.OrganizationDomainId, Name = orgDomain.Name, CreateDate = orgDomain.CreateDate } : new OrganizationDomain();
                }
                );

                return organizationList;
            }
        }

        /// <summary>
        /// List of Unified Login companies
        /// </summary>       
        /// <returns>List of Unified Login companies including admin user info</returns>
        public List<UnifiedLoginCompany> GetUnifiedLoginCompanyList()
        {
            using (var repository = GetRepository())
            {
                List<UnifiedLoginCompany> compList = new List<UnifiedLoginCompany>();
                var result = repository.GetMany<dynamic>(StoredProcNameConstants.SP_ListOrganizations, null);
                if (result != null)
                {
                    foreach (var item in result)
                    {
                        compList.Add(new UnifiedLoginCompany { CompanyId = long.Parse(item.BooksMasterId.ToString()), BooksCustomerMasterId = long.Parse(item.BooksCustomerMasterId.ToString() == string.Empty ? "0" : item.BooksCustomerMasterId.ToString()), CompanyName = item.Name, IsActive = true, PartyId = item.PartyId, CompanyRealPageId = item.OrganizationRealPageId.ToString(),  UserRealPageId = item.PersonRealPageId.ToString(), UserLoginAs = item.LoginName, Domain = item.Domain });
                    }
                    compList = compList.OrderBy(p => p.CompanyName).ToList();
                }
                return compList;
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
                            realPageId = item.PersonRealPageId.ToString();
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
        /// GetCompanyIDPList
        /// </summary>
        public List<IDPNames> GetCompanyIDPList(int OrganizationPartyId)
        {
            dynamic param = new
            {
                OrganizationPartyId = OrganizationPartyId
            };
            using (var repository = GetRepository())
            {
                var IDPList = repository.GetMany<IDPNames>(StoredProcNameConstants.SP_OrganizationIDPList, param);
                return (List<IDPNames>)IDPList;
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
            if (_repository != null)
            {
                // unit test
                using (var repository = GetRepository())
                {
                    return repository.GetMany<ProductUI>(StoredProcNameConstants.SP_ListProductsByOrganization, new { OrganizationRealPageId = organizationRealPageId }).ToList();
                }
            }

            var rpCache = new RPObjectCache();
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

        /// <summary>
        /// Returns the Organization Setting Value
        /// <param name="settingName">SettingName</param>
        /// <param name="partyId">partyId</param>
        /// </summary>
        public string GetOrganizationSettingValue(string settingName, long partyId)
        {
            using (var repository = GetRepository())
            {
                string settingValue = "";
                DynamicParameters param = new DynamicParameters();
                param.Add("@PartyId", partyId, dbType: DbType.Int32, direction: ParameterDirection.Input);
                param.Add("@SettingName", settingName, dbType: DbType.String, direction: ParameterDirection.Input);
                param.Add("@SettingValue", settingValue, dbType: DbType.String, direction: ParameterDirection.Output);

                try
                {
                    repository.Execute(StoredProcNameConstants.SP_GetOrganizationSettingValue, param);
                    settingValue = param.Get<string>("@SettingValue");
                }
                catch
                {
                }

                return settingValue;
            }
        }
        public string GetOrganizationSettingValueByPersonaId(string settingName, long personaId)
        {
            RPObjectCache rpCache = new RPObjectCache();
            var cacheKey = $"getOrganizationSettingValueByPersonaId_{personaId}_{settingName}";
            string settingValue = "";

            settingValue = rpCache.GetFromCache<string>(cacheKey, 180, () =>
            {
                using (var repository = GetRepository())
                {
                    DynamicParameters param = new DynamicParameters();
                    param.Add("@PersonaId", personaId, dbType: DbType.Int32, direction: ParameterDirection.Input);
                    param.Add("@SettingName", settingName, dbType: DbType.String, direction: ParameterDirection.Input);
                    param.Add("@SettingValue", settingValue, dbType: DbType.String, direction: ParameterDirection.Output);

                    repository.Execute(StoredProcNameConstants.SP_GetOrganizationSettingValueByPersonaId, param);
                    settingValue = param.Get<string>("@SettingValue");
                    return settingValue;
                }
            });
            return settingValue;
        }
        #endregion

        #region public Organization type methods
        /// <summary>
        /// Used to get the list of all Organization Types
        /// </summary>
        /// <returns>OrganizationType listobject</returns>
        public List<OrganizationType> ListOrganizationType()
        {
            if (_repository != null)
            {
                // unit test
                dynamic param = null;
                using (var repository = GetRepository())
                {
                    return repository.GetMany<OrganizationType>(StoredProcNameConstants.SP_ListOrganizationType, param);
                }
            }

            var rpCache = new RPObjectCache();
            string cacheKey = $"getListOrganizationType";

            List<OrganizationType> organizationTypeList = rpCache.GetFromCache<List<OrganizationType>>(cacheKey, 180, () =>
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

        #region public Organization Domain methods
        /// <summary>
        /// Used to get the list of all Organization Domains
        /// </summary>
        /// <returns>OrganizationDomain list</returns>
        public List<OrganizationDomain> ListOrganizationDomain()
        {
            if (_repository != null)
            {
                // unit test
                dynamic param = null;
                using (var repository = GetRepository())
                {
                    return repository.GetMany<OrganizationDomain>(StoredProcNameConstants.SP_ListOrganizationDomain, param);
                }
            }

            var rpCache = new RPObjectCache();
            string cacheKey = $"getListOrganizationDomain";

            var organizationDomainList = rpCache.GetFromCache<List<OrganizationDomain>>(cacheKey, 60, () =>
            {
                dynamic param = null;
                using (var repository = GetRepository())
                {
                    return repository.GetMany<OrganizationDomain>(StoredProcNameConstants.SP_ListOrganizationDomain, param);
                }
            });

            return organizationDomainList;
        }

        /// <summary>
        /// Used to add a new organization domain
        /// </summary>
        /// <param name="organizationDomain"></param>
        /// <returns></returns>
        public RepositoryResponse CreateOrganizationDomain(OrganizationDomain organizationDomain)
        {
            RepositoryResponse result;
            dynamic param = new
            {
                DomainName = organizationDomain.Name
            };

            using (var repository = GetRepository())
            {
                result = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_CreateOrganizationDomain, param);
            }

            return result;
        }

        #endregion

        #region GetCompanyList
        public List<CompanySetup> GetCompanyList(string organizationName, int domain, int? blueId, int organizationId, RequestParameter dataFilterSort = null)
        {
            string sortBy = "OrganizationName";
            string sortDirection = "Asc";
            string filterByProduct = null;
            string filterByDomain = null;
            string filterByType = null;
            string filterByStatus = null;

            List<CompanySetup> companylst = new List<CompanySetup>();
            if (dataFilterSort != null)
            {
                if (dataFilterSort.FilterBy != null)
                {
                    foreach (string FilterKey in dataFilterSort.FilterBy.Keys)
                    {
                        switch (FilterKey.ToLower())
                        {
                            case "product":
                                filterByProduct = dataFilterSort.FilterBy[FilterKey];
                                break;
                            case "domain":
                                filterByDomain = dataFilterSort.FilterBy[FilterKey];
                                break;
                            case "type":
                                filterByType = dataFilterSort.FilterBy[FilterKey];
                                break;
                            case "status":
                                filterByStatus = dataFilterSort.FilterBy[FilterKey];
                                break;
                        }
                    }
                }
            }
            if (dataFilterSort != null)
            {
                if (dataFilterSort.SortBy != null)
                {
                    foreach (string SortKey in dataFilterSort.SortBy.Keys)
                    {
                        sortBy = SortKey;
                        sortDirection = dataFilterSort.SortBy[SortKey];
                    }
                }
            }
            dynamic param = new
            {
                OrganizationName = organizationName,
                OrganizationId = organizationId,
                Domain = domain,
                BooksCustomerMasterId = blueId,
                FilterByProduct = filterByProduct,
                FilterByDomain = filterByDomain,
                FilterByType = filterByType,
                FilterByStatus = filterByStatus,
                SortColumn = sortBy,
                SortDirection = sortDirection,
                RowsPerPage = dataFilterSort.Pages.ResultsPerPage == 100 ? 0 : dataFilterSort.Pages.ResultsPerPage,
                PageNumber = ((dataFilterSort.Pages.ResultsPerPage == 100) || (dataFilterSort.Pages.StartRow <= 0)) ? 1 : dataFilterSort.Pages.StartRow
            };
            using (var repository = GetRepository())
            {
                companylst = repository.GetMany<CompanySetup>(StoredProcNameConstants.SP_ListCompanySetup, param);
                return companylst;
            }
        }

        /// <summary>
        /// Used to get a list of organizations to delete
        /// </summary>
        /// <param name="batchSize"></param>
        /// <param name="retryCount"></param>
        /// <param name="includeErrorRecord"></param>
        public List<OrganizationRemovalQueue> GetOrganizationToDelete(int batchSize, int retryCount, bool includeErrorRecord)
        {
            dynamic param = new
            {
                BatchSize = batchSize,
                RetryCount = retryCount
            };
            using (var repository = GetRepository())
            {
                return repository.GetMany<OrganizationRemovalQueue>(StoredProcNameConstants.SP_ListOrganizationToDelete, param);
            }
        }

        /// <summary>
        /// Used to delete the specified company
        /// </summary>
        /// <param name="organizationRemovalQueueId"></param>
        /// <param name="partyId"></param>
        /// <param name="organizationRealPageId"></param>
        /// <returns></returns>
        public long DeleteOrganization(int organizationRemovalQueueId, long partyId, Guid organizationRealPageId)
        {
            dynamic param = new
            {
                OrganizationRemovalQueueId = organizationRemovalQueueId,
                OrganizationPartyId = partyId,
                OrganizationRealPageId = organizationRealPageId
            };
            using (var repository = GetRepository())
            {
                return repository.GetOne<long>(StoredProcNameConstants.SP_DeleteOrganization, param);
            }
        }

        /// <summary>
        /// Used to update the status of the OrganizationRemovalQueue
        /// </summary>
        /// <param name="organizationRemovalQueueId"></param>
        /// <param name="organizationRemovalQueueStatus"></param>
        /// <returns></returns>
        public int UpdateOrganizationRemovalQueueStatus(int organizationRemovalQueueId, string organizationRemovalQueueStatus)
        {
            dynamic param = new
            {
                OrganizationRemovalQueueId = organizationRemovalQueueId,
                OrganizationRemovalQueueStatus = organizationRemovalQueueStatus
            };
            using (var repository = GetRepository())
            {
                return repository.GetOne<int>(StoredProcNameConstants.SP_UpdateOrganizationRemovalQueueStatus, param);
            }
        }

        /// <summary>
        /// Used to insert a new request to remove a UPFM company and data related to it in UDM
        /// </summary>
        /// <param name="orgRemovalQueue"></param>
        /// <returns></returns>
        public OrganizationRemovalQueue InsertOrganizationRemovalQueue(OrganizationRemovalQueue orgRemovalQueue)
        {
            dynamic param = new
            {
                OrganizationPartyId = orgRemovalQueue.OrganizationPartyId,
                OrganizationRealPageId = orgRemovalQueue.OrganizationRealPageId,
                OrganizationCustomerMasterId = orgRemovalQueue.OrganizationCustomerMasterId,
                OrganizationDomain = orgRemovalQueue.OrganizationDomain,
                OrganizationName = orgRemovalQueue.OrganizationName,
                OrganizationRemoveUDMData = orgRemovalQueue.OrganizationRemoveUDMData,
                OrganizationRemovalQueueStatusId = orgRemovalQueue.OrganizationRemovalQueueStatusId,
                OrganizationRemovalRetryCount = orgRemovalQueue.OrganizationRemovalRetryCount,
                RequestedBy = orgRemovalQueue.RequestedBy
            };
            using (var repository = GetRepository())
            {
                return repository.GetOne<OrganizationRemovalQueue>(StoredProcNameConstants.SP_InsertOrganizationRemovalQueue, param);
            }
        }

        #endregion 
    }
}