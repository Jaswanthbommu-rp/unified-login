using Newtonsoft.Json;
using RP.Enterprise.Foundation.DataAccess.Component;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.ProductIntegration.Factory;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Audit.Common;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.BlackBook;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.EmployeeAccess;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.OneSite;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.ResidentPortal;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.UnifiedLogin;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.UPFMProduct;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic
{
    public class ManageEmployeeAccess : ManageProductBase, IManageEmployeeAccess
    {
        /// <summary>
        /// User claim
        /// </summary>
        private DefaultUserClaim _userClaim;

        private IManageOrganization _manageOrganization;
        private IManageUser _manageUser;
        private IManagePersona _managePersona;
        private readonly IIntegrationTypeFactory _integrationTypeFactory;
        private readonly IManageProductOneSite _manageProductOneSite;
        readonly IManageUnifiedLogin _manageUnifiedLogin;
        private IManageProductUser _manageProductUser;
        private IUserRepository _userRepository;
        IProductInternalSettingRepository _productInternalSettingRepository;
        private IManageUPFMProductsIntegration _manageUPFMProductsIntegration;
        #region Ctor


        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="userClaim"></param>
        public ManageEmployeeAccess(DefaultUserClaim userClaim) : base((int)ProductEnum.SupportTool, userClaim, productInternalSettingRepository: null, productRepository: null)
        {
            _productId = (int)ProductEnum.SupportTool;
            _userClaim = userClaim;
            _editorRealPageId = _userClaim.UserRealPageGuid;
            _blueBook = new ManageBlueBook(_userClaim);
            _userLoginRepository = new UserLoginRepository();
            _manageOrganization = new ManageOrganization(_userClaim);
            _manageUser = new ManageUser(_userClaim);
            _managePersona = new ManagePersona(_userClaim);
            _manageUnifiedLogin = new ManageUnifiedLogin(_userClaim);
            _manageProductOneSite = new ManageProductOneSite(_userClaim);
            _manageProductUser = new ManageProductUser(_userClaim);
            _userRepository = new UserRepository(_userClaim);
            var manageProduct = new ManageProduct(_userClaim);
            _productInternalSettingRepository = new ProductInternalSettingRepository();
            _integrationTypeFactory = new IntegrationTypeFactory(manageProduct, _manageUnifiedLogin, _manageProductOneSite, _productRepository, _productInternalSettingRepository, _userClaim);
            _manageUPFMProductsIntegration = new ManageUPFMProductsIntegration(_productId, _userClaim);
        }

        /// <summary>
        /// Unit test constructor
        /// </summary>
        /// <param name="userClaim"></param>
        /// <param name="repository"></param>
        /// <param name="messageHandler"></param>
        /// <param name="oneSiteProductService"></param>
        public ManageEmployeeAccess(DefaultUserClaim userClaim, IRepository repository, HttpMessageHandler messageHandler, IOneSiteProductService oneSiteProductService) : base((int)ProductEnum.SupportTool, userClaim, repository, messageHandler)
        {
            _productId = (int)ProductEnum.SupportTool;
            _userClaim = userClaim;
            _editorRealPageId = _userClaim.UserRealPageGuid;
            _blueBook = new ManageBlueBook(userClaim, repository, messageHandler);
            _userLoginRepository = new UserLoginRepository(repository);

            _manageUser = new ManageUser(repository, userClaim, messageHandler);
            _managePersona = new ManagePersona(repository, userClaim, messageHandler);
            _manageUnifiedLogin = new ManageUnifiedLogin(repository, userClaim, messageHandler);
            _manageProductOneSite = new ManageProductOneSite(repository, userClaim, messageHandler, oneSiteProductService);
            _manageProductUser = new ManageProductUser(repository, userClaim, messageHandler, oneSiteProductService);
            _manageOrganization = new ManageOrganization(repository, userClaim, messageHandler, _manageProductOneSite);
            _userRepository = new UserRepository(repository, userClaim, messageHandler);
            _productInternalSettingRepository = new ProductInternalSettingRepository(repository);
            var manageProduct = new ManageProduct(repository, userClaim, messageHandler);
            var productInternalSettingRepository = new ProductInternalSettingRepository(repository);
            _integrationTypeFactory = new IntegrationTypeFactory(manageProduct, _manageUnifiedLogin, _manageProductOneSite, _productRepository, productInternalSettingRepository, _userClaim);
        }

        #endregion

        #region Public Methods


        /// <summary>
        /// Returns all companies from GB
        /// </summary>
        public ListResponse GetCompanies(long editorPersonaId, string filter)
        {
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetCompanies", $"GetCompanies at beginning of method for user with editorPersona id - {editorPersonaId}" });
            var response = new ListResponse();
            try
            {
                ListResponse result = GetCompanyEditorAndUserDetails(editorPersonaId, editorPersonaId);
                if (result.IsError)
                {
                    WriteToErrorLog(
                        "{ActionName} - {state}", messageProperties: new object[] { "GetCompanies", $"GetCompanyEditorAndUserDetails error for user with editorPersona id - {editorPersonaId} - {result.ErrorReason}" });
                    return result;
                }

                // get companies from DB for EmployeeAccess 
                WriteToDiagnosticLog(
                    "{ActionName} - {state}", messageProperties: new object[] { "GetCompanies", $"Getting all GB companies from GB DB - pr.ListCompanies with filter- {filter}" });

                UnifiedLoginRepository umr = new UnifiedLoginRepository();

                List<UnifiedLoginCompany> gbAllCompanies = umr.ListCompanies(filter);
                List<UnifiedLoginCompany> gbAllActiveCompanies = gbAllCompanies?.Where(c => c.IsActive == true).ToList();

                // Get BooksCompanyMasterIds - RPUP id
                //string comIdsRpUp = GetCompanyIds(gbAllCompanies);
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetCompanies", $"GetCompanyIds() completed for user with editorPersona id - {editorPersonaId}" });

                IList<Company> bbCompanies = _blueBook.GetCompanyListByCompIds(gbAllActiveCompanies);
                List<CompanyDetails> mergedCompanies = MergeCompanies(gbAllActiveCompanies, bbCompanies);

                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetCompanies", $"GetCompanyListByCompIds() completed for user with editorPersona id - {editorPersonaId}" });

                response = new ListResponse()
                {
                    Records = mergedCompanies.Cast<object>().ToList(),
                    TotalRows = mergedCompanies.Count(),
                    RowsPerPage = 9999,
                    ErrorReason = string.Empty,
                    TotalPages = 1
                };
            }
            catch (Exception ex)
            {
                response.IsError = true;
                response.ErrorReason = "EmployeeAccess - There was a problem getting the companies.";
                WriteToErrorLog("{ActionName} - {state}", exception: ex, messageProperties: new object[] { "GetCompanies", $"Error for user with editorPersona id - {editorPersonaId}" });
            }

            return response;
        }

        /// <summary>
        /// Returns all Users from GB
        /// </summary>
        public ListResponse GetUsers(long editorPersonaId, string filter)
        {
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetUsers", $"At beginning of method for user with editorPersona id - {editorPersonaId}" });

            var response = new ListResponse();
            try
            {
                ListResponse result = GetCompanyEditorAndUserDetails(editorPersonaId, editorPersonaId);
                if (result.IsError)
                {
                    WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetUsers", $"GetCompanyEditorAndUserDetails error for user with editorPersona id - {editorPersonaId} - {result.ErrorReason}" });
                    return result;
                }

                // get companies from DB for EmployeeAccess 
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetUsers", $"Getting all GB users from GB DB - pr.ListCompanies with filter- {filter}" });

                UnifiedLoginRepository umr = new UnifiedLoginRepository();

                List<UnifiedLoginCompany> gbAllCompanies = umr.ListCompanies();

                List<UserDetail> ulUsersByFilter = umr.ListUsers(filter);
                if (ulUsersByFilter != null && ulUsersByFilter.Count > 0)
                {
                    foreach (var item in ulUsersByFilter)
                    {
                        if (item.Name3rdPartyIDP.ToUpper() == "IDENTITYSERVER")
                        {
                            item.Name3rdPartyIDP = "None";
                        }
                    }
                }

                List<UserDetail> mergedUserCompanies = MergeUserCompanies(gbAllCompanies, ulUsersByFilter);

                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetUsers", $"Completed for user with editorPersona id - {editorPersonaId}" });

                response = new ListResponse()
                {
                    Records = mergedUserCompanies.Cast<object>().ToList(),
                    TotalRows = mergedUserCompanies.Count(),
                    RowsPerPage = 9999,
                    ErrorReason = string.Empty,
                    TotalPages = 1
                };
            }
            catch (Exception ex)
            {
                response.IsError = true;
                response.ErrorReason = "EmployeeAccess - There was a problem getting the users.";
                WriteToErrorLog("{ActionName} - {state}", exception: ex, messageProperties: new object[] { "GetUsers", $"Error for user with editorPersona id - {editorPersonaId}" });
            }

            return response;
        }


        /// <summary>
        /// Gets personaId if existed else, creates and gets one
        /// </summary>
        public EmployeePersona GetOrCreateEmployeePersonaId(Guid companyRealPageId, DefaultUserClaim userClaim)
        {
            EmployeePersona employeePersona = new EmployeePersona();
            IList<UserOrganization> userPersonaOrganizationList = new List<UserOrganization>();
            UserDetails currentUser = new UserDetails();
            if (userClaim.ImpersonatedBy != Guid.Empty)
            {
                employeePersona.RealpageUserId = userClaim.ImpersonatedBy;
                currentUser = _userRepository.GetUserDetails(null, userClaim.ImpersonatedBy.ToString());
                userPersonaOrganizationList = _userLoginRepository.ListOrganizationByLoginName(currentUser.LoginName);
            }
            else
            {
                employeePersona.RealpageUserId = _userClaim.UserRealPageGuid;
                currentUser = _userRepository.GetUserDetails(null, _userClaim.UserRealPageGuid.ToString());
                userPersonaOrganizationList = _userLoginRepository.ListOrganizationByLoginName(userClaim.LoginName);
            }

            if (userPersonaOrganizationList != null && userPersonaOrganizationList.Count > 0)
            {
                //First get count of ad groups and products for employee persona
                //if company doesn't have product do not create second persona
                //rethink how  return correct persona based on ad group data
                var userProductAdGroups = _productRepository.GetPersonaProductsAdGroupsCount(currentUser.PersonaId);
                //Get Organization product id's
                IList<int> orgProducts = _productRepository.GetProductIdsByCompany(companyRealPageId);
                //Filter adgroup data with valid org products
                var matchedProductData = userProductAdGroups.Where(p => orgProducts.Contains(p.ProductId)).ToList();
                int maxCount = 0;
                //Get max adgroup count
                if (matchedProductData?.Count > 0)
                {
                    maxCount = matchedProductData.Max(x => x.ADGroupCount);
                }
                //Get User persona count
                var orgPersonaList = userPersonaOrganizationList.Where(x => x.OrganizationRealPageId == companyRealPageId).ToList();
                int orgPersonaCount = orgPersonaList.Count();
                var isRealPageEmployeeInOrg = true;

                foreach (var userPersona in orgPersonaList)
                {
                    var userOrgInfo = _userRepository.GetUserDetails(userPersona.PersonaId);
                    // see if the employee already exists in the company but not as the new isrpemployee type
                    if (userOrgInfo != null && !userOrgInfo.IsRPEmployee)
                    {
                        isRealPageEmployeeInOrg = false;
                        break;
                    }
                }

                var user = userPersonaOrganizationList.FirstOrDefault(x => x.OrganizationRealPageId == companyRealPageId);
                if (user != null)
                {
                    employeePersona.PersonaId = user.PersonaId;
                }
                else
                {
                    employeePersona.PersonaId = CreatePersonaInCompany(currentUser.LoginName, companyRealPageId, currentUser);
                    orgPersonaCount++;
                }

                if (isRealPageEmployeeInOrg && maxCount > 0 && orgPersonaCount > 0)
                {
                    //add new persons based on max count and existing persona count
                    int newPersonasTobeCreatedCount = 0;
                    newPersonasTobeCreatedCount = maxCount - orgPersonaCount;
                    for (int i = 1; i <= newPersonasTobeCreatedCount; i++)
                    {
                        //set name based on count
                        string personaName = "Secondary";
                        if (orgPersonaCount >= 2)
                        {
                            personaName = personaName + " " + orgPersonaCount.ToString();
                        }
                        var repoResponse = _managePersona.CreateAdditionalPersona(companyRealPageId, currentUser.UserId, currentUser.UserId, personaName);
                        orgPersonaCount++;
                    }
                }

            }
            return employeePersona;
        }

        /// <summary>
        /// Used to create an employee in product 
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="personaId"></param>
        /// <returns></returns>
        public string CreateEmployeeProductUser(int productId, long personaId)
        {
            long adminUserPersonaId = 0;
            var adminCreatorRealPageId = Guid.Empty;

            var productInternalSettingList = _productInternalSettingRepository.GetProductInternalSettings(productId);
            var supportsEmployeeAccess = productInternalSettingList.FirstOrDefault(s => s.Name.Equals("SI_SupportsEmployeeCreation", StringComparison.OrdinalIgnoreCase))?.Value;
            var aDGroupWithoutUserCreation = productInternalSettingList.FirstOrDefault(s => s.Name.Equals("ProductAssignedViaADGroupWithoutUserCreation", StringComparison.OrdinalIgnoreCase))?.Value;
            if (string.IsNullOrEmpty(supportsEmployeeAccess) || supportsEmployeeAccess == "0")
            {
                return "Product does not support employee creation.";
            }

            if (supportsEmployeeAccess == "-1" || aDGroupWithoutUserCreation == "1") // for products that don't need user management but use other products for user info
            {
                return "";
            }

            var userPersona = _managePersona.GetPersona(personaId);
            var personaList = _managePersona.ListPersona(userPersona.RealPageId);
            var companyPersonaList = personaList.Where(p => p.OrganizationPartyId == userPersona.OrganizationPartyId);

            var employeePersona = personaList.FirstOrDefault(p => p.Organization.RealPageId == DefaultUserClaim.EmployeeCompanyRealPageId);
            if (employeePersona == null)
            {
                return "Employee persona could not be found in RealPage Employee company.";
            }

            var productAdGroups = _productRepository.GetAdGroupsForProduct(productId);
            if (productAdGroups.Count > 0)
            {
                var userAdGroups = _productRepository.GetAdGroupsForUser(employeePersona.PersonaId);
                var userProductToADGroups = _userRepository.GetEmployeeProductADGroupMapping(personaId, productId);
                var isProductAssigned = _productRepository.isProductAssigned(personaId, (int)ProductBatchStatusType.Success, productId);
                var existingProductAdGroupInfo = _userRepository.GetEmployeeProductADGroupMapping(personaId, productId).FirstOrDefault();

                if (isProductAssigned)
                {
                    if (productAdGroups.All(p => p.ADGroupId != existingProductAdGroupInfo?.ADGroupId))
                    {
                        ManageProductBase mpb = new ManageProductBase(productId, _userClaim, _productInternalSettingRepository, _productRepository);
                        mpb.UpdateProductSettingProductStatus(personaId, _productSettingType_ProductStatus, (int)ProductBatchStatusType.Deleted);
                        return "DeletedProductLogin";
                    }

                    if (userProductToADGroups.Any(userProductToAdGroup => userAdGroups.Any(p => p.ADGroupId == userProductToAdGroup.ADGroupId)))
                    {
                        return "";
                    }

                    // check if user lost access to the adgroup that was used to assign it to the product
                    if (userAdGroups.All(p => p.ADGroupId != existingProductAdGroupInfo?.ADGroupId))
                    {
                        // see if the user has any other adgroups for the product that might work, otherwise disable product and reject access
                        //if (companyPersonaList.Count() == 1 && userAdGroups.All(p => p.ADGroupId != productAdGroups?.FirstOrDefault(p1 => p1.ADGroupId == p.ADGroupId)?.ADGroupId))
                        if (companyPersonaList.Count() == 1 && userAdGroups.All(p => p.ADGroupId != productAdGroups?.FirstOrDefault(p1 => p1.ADGroupId == p.ADGroupId)?.ADGroupId))
                        {
                            ManageProductBase mpb = new ManageProductBase(productId, _userClaim, _productInternalSettingRepository, _productRepository);
                            mpb.UpdateProductSettingProductStatus(personaId, _productSettingType_ProductStatus, (int)ProductBatchStatusType.Deleted);
                            return "You are no longer in an ADGroup for this product.";
                        }

                        if (userAdGroups.All(p => p.ADGroupId != existingProductAdGroupInfo?.ADGroupId))
                        {
                            ManageProductBase mpb = new ManageProductBase(productId, _userClaim, _productInternalSettingRepository, _productRepository);
                            mpb.UpdateProductSettingProductStatus(personaId, _productSettingType_ProductStatus, (int)ProductBatchStatusType.Deleted);
                            return "DeletedProductLogin";
                        }
                    }

                }
            }

            if (userPersona.Organization.RealPageId != Guid.Empty)
            {
                adminCreatorRealPageId = _manageOrganization.GetOrganizationAdminUserRealPageId(userPersona.Organization.RealPageId);
                if (adminCreatorRealPageId == Guid.Empty)
                {
                    return "Missing company admin user.";
                }
                //recreate clams
                adminUserPersonaId = _managePersona.GetFirstAvailablePersonaByCompany(adminCreatorRealPageId, userPersona.OrganizationPartyId).PersonaId;
                _manageProductUser = new ManageProductUser(_userClaim);
            }

            var productIntegrationType = productInternalSettingList.FirstOrDefault(s => s.Name.Equals("ProductIntegrationType", StringComparison.OrdinalIgnoreCase))?.Value;
            if (productIntegrationType.ToUpper() == "UPFM")
            {
                List<AdditionalParameters> additionalParameters = new List<AdditionalParameters>();
                var productAdGroupsUPFM = _productRepository.GetAdGroupsForProduct(productId);
                var userADGroupsRoles = _productRepository.GetAdGroupRolesByPersona(employeePersona.PersonaId);
                var adGroupIds = userADGroupsRoles?.Where(y => y.ProductId == productId)?.Select(x => x.ADGroupId);
                if (adGroupIds != null && productAdGroupsUPFM != null && productAdGroupsUPFM.Any(y => adGroupIds.Contains(y.ADGroupId)))
                {
                    var hasProperties = productInternalSettingList.FirstOrDefault(s => s.Name.Equals("UPFMProductsHasProperties", StringComparison.OrdinalIgnoreCase))?.Value;
                    List<string> propertyList = hasProperties == "0" ? new List<string>() : new List<string>() { "-1" };
                    List<string> roleList = userADGroupsRoles.Where(x => x.ProductId == productId).Select(y => y.RoleId.ToString()).ToList();
                    UPFMProductPropertyRole upfmPropertyRole = new UPFMProductPropertyRole() { IsAssigned = true, PropertyList = propertyList, RoleList = roleList };
                    _manageUPFMProductsIntegration = new ManageUPFMProductsIntegration(productId, _userClaim);
                    return _manageUPFMProductsIntegration.ManageUPFMProductUser(_userClaim.PersonaId, personaId, upfmPropertyRole, out additionalParameters, true);
                }

                return "No ADGroups for UPFM products.";
            }
            else
            {
                // not used
                var rolePropertyList = new RolePropertyList
                {
                    PropertyList = new List<string>() { "-1" }
                };

                var productUser = new ProductUserProperitiesRoles()
                {
                    RealPageId = adminCreatorRealPageId,
                    ProductId = productId,
                    CreateUserPersonaId = adminUserPersonaId,
                    AssignUserPersonaId = personaId,
                    CorrelationId = _userClaim.CorrelationId,
                    InputJson = JsonConvert.SerializeObject(rolePropertyList),
                    CreateRealPageEmployee = true,
                    RealPageEmployeePersonaId = employeePersona.PersonaId
                };

                return _manageProductUser.CreateEmployeeProductUser(productUser);
            }
        }

        #endregion

        #region Private Methods  

        private List<CompanyDetails> MergeCompanies(List<UnifiedLoginCompany> gbcompanies, IList<Company> bbcompanies)
        {
            List<CompanyDetails> compList = new List<CompanyDetails>();
            foreach (var gb in gbcompanies)
            {
                CompanyDetails cd = new CompanyDetails();
                Company c = bbcompanies.FirstOrDefault((p => p.CustomerCompanyId == gb.BooksCustomerMasterId));
                cd.CompanyName = gb.CompanyName;
                cd.CompanyRealPageId = gb.CompanyRealPageId;
                cd.UserRealPageId = gb.UserRealPageId;
                cd.UserLoginAs = gb.UserLoginAs;
                cd.PartyId = gb.PartyId;
                cd.IsActive = gb.IsActive;

                if (c != null)
                {
                    cd.CompanyId = c.CustomerCompanyId;
                    cd.PhoneNumber = c.PhoneNumber;

                    if (c.CustomerCompanyLocation != null)
                    {
                        foreach (var comp in c.CustomerCompanyLocation)
                        {
                            if (comp.IsPrimary == true)
                            {
                                cd.Address = comp.Address;
                                cd.City = comp.City;
                                cd.Country = comp.Country;
                                cd.County = comp.County;
                                cd.State = comp.State;
                                cd.PostalCode = comp.PostalCode;
                            }
                        }
                    }
                    compList.Add(cd);
                }
                else
                {
                    if (gb.BooksCustomerMasterId == -2)
                    {
                        cd.Address = "REALPAGE INTERNAL USE ONLY!";
                        compList.Add(cd);
                    }
                }
            }
            return compList;
        }

        private List<UserDetail> MergeUserCompanies(List<UnifiedLoginCompany> gbcompanies, List<UserDetail> ulusers)
        {
            List<CompanyDetails> compList = new List<CompanyDetails>();
            foreach (var gb in gbcompanies)
            {
                CompanyDetails cd = new CompanyDetails();
                foreach (var bb in ulusers)
                {
                    if (gb.PartyId == bb.CompanyId)
                    {
                        bb.CompanyRealPageId = gb.CompanyRealPageId;
                        bb.UserRealPageId = gb.UserRealPageId;
                        bb.BooksMasterId = gb.CompanyId;
                    }
                }
            }
            return ulusers;
        }

        private string GetCompanyIds(List<UnifiedLoginCompany> companies)
        {
            string compIds = "";
            foreach (var item in companies)
            {
                if (item.BooksCustomerMasterId > 0)
                {
                    if (compIds == "")
                    {
                        compIds = item.BooksCustomerMasterId.ToString();
                    }

                    compIds += "," + item.BooksCustomerMasterId;
                }
            }

            return compIds;
        }

        private long CreatePersonaInCompany(string loginName, Guid companyRealPageId, UserDetails currentUser)
        {
            long persona = 0;
            var newProfile = CreateNewProfile(companyRealPageId, currentUser);
            newProfile.IsRPEmployee = true;
            if (companyRealPageId != DefaultUserClaim.EmployeeCompanyRealPageId)
            {
                newProfile.ExternalUserRelationship = new ExternalUserRelationship() { ThirdPartyRelationShipId = 5, ThirdPartyRelationShip = "5", ThirdPartyCompanyName = "RealPage" };
                newProfile.UserTypeId = (int)UserRoleType.ExternalUser;
            }
            var newUser = _manageUser.CreateUser(newProfile, newProfile.Persona);
            persona = newUser.PersonaId;
            return persona;
        }

        private ProfileDetail CreateNewProfile(Guid companyRealPageId, UserDetails currentUser)
        {

            ProfileDetail newProfile = new ProfileDetail();
            newProfile.FirstName = currentUser.FirstName;
            newProfile.LastName = currentUser.LastName;
            newProfile.CreateUserSourceType = CreateUserSourceType.UnifiedPlatform;

            IList<Persona> personaList = new List<Persona>();
            Persona persona = new Persona();
            DateTime utcNow = DateTime.UtcNow;
            DateTime utcMaxValue = DateTime.MaxValue.ToUniversalTime();

            IList<PersonaEnvironment> personaEnvironment = _managePersona.GetPersonaEnvironmentType();
            var personaEnv = personaEnvironment.SingleOrDefault<PersonaEnvironment>(p => p.Name == "Production");
            var productInternalSettingList = _productInternalSettingRepository.GetProductInternalSettings((int)ProductEnum.UnifiedPlatform);

            persona.Name = newProfile.UserTypeId == (int)UserRoleType.SuperUser ? "System Administrator" : "Primary";
            persona.PersonaName = "Primary";
            persona.PersonaEnvironmentTypeId = (int)personaEnv.PersonaEnvironmentTypeId;
            persona.FromDate = utcNow.AddMinutes(-5);
            persona.ThruDate = null;
            personaList.Add(persona);
            newProfile.Persona = personaList;

            var org = _manageOrganization.GetOrganization(companyRealPageId);
            newProfile.organization.Add(org);

            newProfile.UserTypeId = 405;

            UserLogin ul = new UserLogin();
            ul.LoginName = currentUser.LoginName;
            ul.IsActive = true;
            ul.IsPending = false;
            ul.IsExpired = false;
            ul.FromDate = DateTime.UtcNow.AddMinutes(-5);
            ul.Is3rdPartyIDP = false;

            newProfile.userLogin = ul;

            List<ProductBatch> pbs = new List<ProductBatch>();
            ProductBatch pb = new ProductBatch();
            pb.ProductId = 3;
            pb.StatusTypeId = 5;
            pb.RetryCount = 0;

            RolePropertyList rpl = new RolePropertyList();
            rpl.IsVendorRecommendationChanges = false;
            rpl.IsInsuranceExpired = false;
            rpl.IsVendorNotLinkedToAnyProperty = false;

            Notifications notification = new Notifications()
            {
                amenitiesViaEmail = false,
                managerFdiViaEmail = false,
                managerMrViaEmail = false
            };

            rpl.Notifications = notification;

            rpl.CanReceiveMonthlyReport = false;
            rpl.IsAssignedNewPropertyByDefault = false;
            rpl.UsePrimaryProperties = false;
            rpl.HasAccessToAllCurrentFutureProperties = false;
            rpl.HasAccessToSiteSpendManagementOnly = false;

            var integration = _integrationTypeFactory.GetIntegration(pb.ProductId);
            ListResponse result = integration.GetRoles(_userClaim.PersonaId, 0, org.PartyId, null, null);

            List<UnifiedLoginRoleRights> roleRights = new List<UnifiedLoginRoleRights>();

            foreach (var item in result.Records)
                roleRights.Add((UnifiedLoginRoleRights)item);

            var defaultRole = productInternalSettingList.FirstOrDefault(s => s.Name.Equals("EmployeeExternelUserDefautRole", StringComparison.OrdinalIgnoreCase))?.Value;
            defaultRole = defaultRole != null ? defaultRole : "User Administrator";
            var role = roleRights.Where(x => x.Role == defaultRole && x.Roletype == "System").FirstOrDefault();

            if (role != null)
                rpl.RoleList = new List<string>() { role.RoleId.ToString() };

            rpl.PropertyList = new List<string>() { "-1" };

            pb.InputJson = rpl;

            pbs.Add(pb);

            newProfile.productBatch = pbs;

            return newProfile;
        }
        #endregion
    }
}
