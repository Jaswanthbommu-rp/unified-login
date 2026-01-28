using Newtonsoft.Json;
using System.Net;
using System.Security.Claims;
using UnifiedLogin.BusinessLogic.Logic;
using UnifiedLogin.BusinessLogic.Logic.Enterprise.User;
using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.BusinessLogic.Logic.Product;
using UnifiedLogin.BusinessLogic.Logic.ProductIntegration.Factory;
using UnifiedLogin.BusinessLogic.Logic.Security;
using UnifiedLogin.BusinessLogic.Repository;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Enterprise;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Helper;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product;
using UnifiedLogin.SharedObjects.Product.Ops;
using UnifiedLogin.SharedObjects.ResponseObject;

namespace UnifiedLogin.LandingAPIEnterprise.Services
{
    /// <summary>
    /// Service for querying user and user product information
    /// </summary>
    public interface IUserQueryService
    {
        Task<PagedResponse> GetUsersAsync(long organizationPartyId, int statusTypeId, Guid? unityRealPageUserId, 
            string name, int rowsPerPage, int pageNumber);
        
        Task<PagedResponse> GetUserRoleAssetAsync(Guid realPageId, string productCode, long orgPartyId, DefaultUserClaim userClaims);
        
        Task<PagedResponse> GetProductUsersWithRoleAssetAsync(string productCode, int rowsPerPage, int pageNumber, 
            long personaId, IProductRepository productRepository);
        
        Task<UserProductOutputResult> GetUserProductDetailsAsync(Guid realPageId, long orgPartyId, DefaultUserClaim userClaims);
        
        Task<UserProductOutputResultv2> GetUserProductsByPersonaIdAsync(long? personaId, bool withStatus, 
            SharedObjects.Landing.DefaultUserClaim userClaims);
        
        Task<List<UserProductDetailLogin>> GetUserProductDetailsLoginByPersonaIdAsync(long personaId);
        
        Task<List<UserProductDetailLogin>> GetUserProductDetailsLoginByLoginNameAsync(string loginName);
        
        Task<UserDetails> GetUserDetailsByIdAsync(Guid unityRealPageUserId);
        Task<UserProductOutputResultv2> GetUserOmniBarProductDetailsAsync(Guid realPageId, long orgPartyId, DefaultUserClaim userClaims);

        Task<ListResponse> GetUserCustomFieldsAsync(long organizationPartyId, long? userLoginPersonaId, DefaultUserClaim userClaims);

        Task<ObjectListOutput<PersonaCompanyDetails, IErrorData>> GetEmployeePersonasListAsync(long userId, long organizationPartyId);

        Task<ObjectListOutput<PersonaCompany, IErrorData>> GetPersonasListAsync(Guid userRealPageGuid);

        Task<IList<SharedObjects.Saml.SamlProductAttributes>> GetSamlProductAttributesAsync(int productId);

        Task<ListResponse> GetProductUserPropertiesAsync(
            string productCode, 
            RequestParameter dataFilter, 
            Guid? upfmId, 
            Guid? userRealPageId, 
            ClaimsPrincipal currentUser, 
            DefaultUserClaim userClaims, 
            IProductRepository productRepository);
    }

    public class UserQueryService : IUserQueryService
    {
        private readonly UserManagement _userManagement;
        private readonly IManagePersona _managePersona;
        private readonly IManagePerson _personLogic;
        private readonly IManageProduct _manageProduct;
        private readonly IManageSecurity _manageSecurityLogic;
        private readonly IUserRepository _userRepository;
        private readonly IManageUnifiedSettings _manageSettings;
        private readonly IManageUserLogin _userLoginLogic;
        private readonly IProductFormattingService _productFormattingService;
        private readonly SamlRepository _samlRepository;

        public UserQueryService(
            UserManagement userManagement,
            IManagePersona managePersona,
            IManagePerson personLogic,
            IManageProduct manageProduct,
            IManageSecurity manageSecurityLogic,
            IUserRepository userRepository,
            IManageUnifiedSettings manageSettings,
            IManageUserLogin userLoginLogic,
            IProductFormattingService productFormattingService,
            SamlRepository samlRepository)
        {
            _userManagement = userManagement ?? throw new ArgumentNullException(nameof(userManagement));
            _managePersona = managePersona ?? throw new ArgumentNullException(nameof(managePersona));
            _personLogic = personLogic ?? throw new ArgumentNullException(nameof(personLogic));
            _manageProduct = manageProduct ?? throw new ArgumentNullException(nameof(manageProduct));
            _manageSecurityLogic = manageSecurityLogic ?? throw new ArgumentNullException(nameof(manageSecurityLogic));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _manageSettings = manageSettings ?? throw new ArgumentNullException(nameof(manageSettings));
            _userLoginLogic = userLoginLogic ?? throw new ArgumentNullException(nameof(userLoginLogic));
            _productFormattingService = productFormattingService ?? throw new ArgumentNullException(nameof(productFormattingService));
            _samlRepository = samlRepository ?? throw new ArgumentNullException(nameof(samlRepository));
        }

        public async Task<PagedResponse> GetUsersAsync(long organizationPartyId, int statusTypeId, Guid? unityRealPageUserId, 
            string name, int rowsPerPage, int pageNumber)
        {
            var usersDataList = _userManagement.ListUser(organizationPartyId, statusTypeId, unityRealPageUserId, name, rowsPerPage, pageNumber);
            var usersDataDtoList = new List<UsersDataDto>();

            if (usersDataList != null && usersDataList.Any())
            {
                foreach (var u in usersDataList)
                {
                    var dictionaryCustomFields = ParseCustomFields(u.CustomFields);

                    usersDataDtoList.Add(new UsersDataDto
                    {
                        FirstName = u.FirstName,
                        MiddleName = u.MiddleName,
                        LastName = u.LastName,
                        UnityRealPageUserId = u.UserRealPageId,
                        LoginName = u.LoginName,
                        UserEffectiveDate = u.UserEffectiveDate,
                        UserExpirationDate = u.UserExpirationDate,
                        UserStatus = u.Status,
                        Email = u.Email,
                        CustomFields = dictionaryCustomFields,
                        UserType = u.UserType,
                        IsExternalIdp = u.IsExternalIdp,
                        Product = DeserializeUserProduct(u.Product ?? ""),
                        EmployeeId = u.EmployeeId,
                        LastLogin = u.LastLogin
                    });
                }
            }

            return new PagedResponse
            {
                Data = usersDataDtoList.Cast<object>().ToList(),
                Meta = new Meta
                {
                    CurrentPage = pageNumber,
                    TotalRows = usersDataList?.FirstOrDefault()?.TotalRecords ?? 0,
                    RowsPerPage = rowsPerPage
                }
            };
        }

        public async Task<PagedResponse> GetUserRoleAssetAsync(Guid realPageId, string productCode, long orgPartyId, DefaultUserClaim userClaims)
        {
            var person = _personLogic.GetPerson(realPageId);
            if (person == null)
            {
                throw new KeyNotFoundException("User not found.");
            }

            var persona = _managePersona.GetFirstAvailablePersonaByCompany(realPageId, orgPartyId);
            if (persona == null || persona.OrganizationPartyId != orgPartyId)
            {
                throw new UnauthorizedAccessException("User exists in a different organization.");
            }

            var productRepository = new ProductRepository();
            var products = productRepository.GetAllProducts();
            var productId = ProductEnumHelper.GetProductIdByProductCode(productCode, products);

            if (productId != (int)ProductEnum.OpsBuyer)
            {
                throw new ArgumentException("No valid product code could be found");
            }

            var userRoleAssetDto = await GetOpsUserRoleAssetAsync(persona.PersonaId, userClaims.PersonaId);

            return new PagedResponse
            {
                Data = new List<object> { userRoleAssetDto },
                Meta = new Meta
                {
                    CurrentPage = 1,
                    TotalRows = 1,
                    RowsPerPage = 1
                }
            };
        }

        public async Task<PagedResponse> GetProductUsersWithRoleAssetAsync(string productCode, int rowsPerPage, int pageNumber, 
            long personaId, IProductRepository productRepository)
        {
            var productList = productRepository.GetAllProducts();
            var productId = ProductEnumHelper.GetProductIdByProductCode(productCode, productList);

            if (productId != (int)ProductEnum.OpsBuyer)
            {
                throw new ArgumentException("No valid product code could be found");
            }

            var requestParameter = new RequestParameter 
            { 
                Pages = new PageRequest 
                { 
                    ResultsPerPage = rowsPerPage, 
                    StartRow = pageNumber 
                } 
            };

            var manageProductOps = new ManageProductOps(new SharedObjects.Landing.DefaultUserClaim { PersonaId = personaId });
            var listResponse = manageProductOps.GetUsers(personaId, requestParameter);

            var opsUserListDto = listResponse.Records.Cast<OpsUser>()
                .Select(u => new OpsUserDataDto
                {
                    Id = u.ID,
                    LoginName = u.Loginname,
                    Status = u.Status,
                    UserType = new OpsUserType { Id = u.UserType.Id, Name = u.UserType.Name },
                    Asset = new OpsAssetGroupDto 
                    { 
                        ID = u.AssetGroup.ID, 
                        Name = u.AssetGroup.Name, 
                        Code = u.AssetGroup.Code, 
                        Status = u.AssetGroup.Status 
                    }
                })
                .ToList();

            return new PagedResponse
            {
                Data = opsUserListDto.Cast<object>().ToList(),
                Meta = new Meta
                {
                    CurrentPage = pageNumber,
                    TotalRows = listResponse.TotalRows,
                    RowsPerPage = rowsPerPage
                }
            };
        }

        public async Task<UserProductOutputResult> GetUserProductDetailsAsync(Guid realPageId, long orgPartyId, DefaultUserClaim userClaims)
        {
            var person = _personLogic.GetPerson(realPageId);
            if (person == null)
            {
                throw new KeyNotFoundException("User not found.");
            }

            var persona = _managePersona.GetFirstAvailablePersonaByCompany(realPageId, orgPartyId);
            if (persona == null || persona.OrganizationPartyId != orgPartyId)
            {
                throw new UnauthorizedAccessException("User exists in a different organization.");
            }

            var productResult = new UserProductOutputResult
            {
                User = new SharedObjects.Enterprise.User
                {
                    FullName = $"{person.FirstName} {person.LastName}",
                    RealPageId = person.RealPageId,
                    Title = persona.Name,
                    CompanyName = persona.Organization.Name
                }
            };

            var routeSecurity = _manageSecurityLogic.GetPersonaRightsAndActionsByRoute(persona.PersonaId, "dashboard");
            var products = _manageProduct.GetUserAssignedProductsByPersona(persona);
            var resources = _manageProduct.GetUserAssignedProductsByPersona(
                persona: persona, 
                productSelectType: ProductSelectType.ResourcesOnly, 
                security: routeSecurity.obj);

            productResult.Products = await _productFormattingService.ConvertDashboardProductsToRAUL(
                products.Where(p => p.ShowInAppSwitcher).ToList(), _manageProduct);
            productResult.Resources = await _productFormattingService.ConvertDashboardProductsToRAUL(
                resources.Where(p => p.ShowInAppSwitcher).ToList(), _manageProduct);

            return productResult;
        }

        public async Task<UserProductOutputResultv2> GetUserOmniBarProductDetailsAsync(Guid realPageId, long orgPartyId, DefaultUserClaim userClaims)
        {
            var person = _personLogic.GetPerson(realPageId);
            if (person == null)
            {
                throw new KeyNotFoundException("User not found.");
            }

            var persona = _managePersona.GetFirstAvailablePersonaByCompany(realPageId, orgPartyId);
            if (persona == null || persona.OrganizationPartyId != orgPartyId)
            {
                throw new UnauthorizedAccessException("User exists in a different organization.");
            }

            var productResult = new UserProductOutputResultv2
            {
                User = new SharedObjects.Enterprise.User
                {
                    FullName = $"{person.FirstName} {person.LastName}",
                    RealPageId = person.RealPageId,
                    Title = persona.Name,
                    CompanyName = persona.Organization.Name
                }
            };

            var routeSecurity = _manageSecurityLogic.GetPersonaRightsAndActionsByRoute(persona.PersonaId, "dashboard");
            var products = _manageProduct.GetUserAssignedProductsByPersona(persona);
            var resources = _manageProduct.GetUserAssignedProductsByPersona(
                persona: persona,
                productSelectType: ProductSelectType.ResourcesOnly,
                security: routeSecurity.obj);

            productResult.Resources = await _productFormattingService.ConvertDashboardProductsToRAUL(
                resources.Where(p => p.ShowInAppSwitcher).ToList(), _manageProduct);

            var userProducts = await _productFormattingService.ConvertDashboardProductsToRAUL(
               products.Where(p => p.ShowInAppSwitcher).ToList(), _manageProduct);
            
            productResult.Products = new Dictionary<string, List<UserProducts>>();
            productResult.Products.Add("Favorites", userProducts.Where(p => p.IsFavorite).ToList());
            foreach (UserProducts up in userProducts)
            {
                if (!productResult.Products.ContainsKey(up.FamilyName))
                {
                    productResult.Products.Add(up.FamilyName, userProducts.Where(p => !p.IsFavorite && p.FamilyName.Equals(up.FamilyName, StringComparison.OrdinalIgnoreCase)).ToList());
                }
            }

            return productResult;
        }

        public async Task<UserProductOutputResultv2> GetUserProductsByPersonaIdAsync(long? personaId, bool withStatus, SharedObjects.Landing.DefaultUserClaim userClaims)
        {
            if (!personaId.HasValue || personaId == 0)
            {
                personaId = userClaims.PersonaId;
            }

            var persona = _managePersona.GetPersonaWithRightsToggle(personaId.Value, false);
            if (persona == null)
            {
                throw new KeyNotFoundException("Persona not found.");
            }

            ValidatePersonaAccess(persona, userClaims);

            var productResult = new UserProductOutputResultv2
            {
                Products = new Dictionary<string, List<UserProducts>>(),
                Settings = new Dictionary<string, object>(),
                Resources = new List<UserProducts>()
            };

            // Add session timeout setting
            AddSessionTimeoutSetting(productResult, persona.OrganizationPartyId);

            var person = _personLogic.GetPerson(persona.RealPageId);
            if (person == null)
            {
                return productResult;
            }

            // Build user info
            await BuildUserInfoAsync(productResult, person, persona, withStatus, userClaims);

            // Get products
            var productList = _manageProduct.GetAllProductsByPersona(personaId.Value, ProductBatchStatusType.Success);
            var userProducts = await _productFormattingService.ConvertPersonaProductsToRAUL(productList, personaId.Value, _manageProduct);

            // Organize products
            OrganizeProducts(productResult, userProducts);

            // Add navigation resources
            await AddNavigationResourcesAsync(productResult, userClaims.PersonaId);

            // Filter out restricted resources for impersonated users
            FilterImpersonatedResources(productResult, userClaims);

            return productResult;
        }

        public async Task<List<UserProductDetailLogin>> GetUserProductDetailsLoginByPersonaIdAsync(long personaId)
        {
            return (List<UserProductDetailLogin>)_userManagement.ListUserProductDetailsLoginByPersonaId(personaId);
        }

        public async Task<List<UserProductDetailLogin>> GetUserProductDetailsLoginByLoginNameAsync(string loginName)
        {
            return (List<UserProductDetailLogin>)_userManagement.ListUserProductDetailsLoginByLoginName(loginName);
        }

        public async Task<UserDetails> GetUserDetailsByIdAsync(Guid unityRealPageUserId)
        {
            return _userRepository.GetUserDetails(null, unityRealPageUserId.ToString());
        }

        public async Task<ListResponse> GetUserCustomFieldsAsync(long organizationPartyId, long? userLoginPersonaId, DefaultUserClaim userClaims)
        {
            var manageCustomFields = new ManageCustomFields(userClaims);
            var customFieldValueList = manageCustomFields.GetCustomFieldsValues(
                organizationPartyId: organizationPartyId, 
                userLoginPersonaId: userLoginPersonaId, 
                enabled: true);

            return new ListResponse
            {
                Records = customFieldValueList.Cast<object>().ToList(),
                TotalRows = customFieldValueList.Count,
                RowsPerPage = customFieldValueList.Count,
                ErrorReason = string.Empty,
                TotalPages = 1
            };
        }

        public async Task<ObjectListOutput<PersonaCompanyDetails, IErrorData>> GetEmployeePersonasListAsync(long userId, long organizationPartyId)
        {
            var personaList = _managePersona.ListEmployeePersonas(userId, organizationPartyId);
            
            var personaCompanyDetailsList = personaList.Select(persona => new PersonaCompanyDetails
            {
                PersonaId = persona.PersonaId,
                Name = persona.Name
            }).ToList();

            return new ObjectListOutput<PersonaCompanyDetails, IErrorData>
            {
                list = personaCompanyDetailsList
            };
        }

        public async Task<ObjectListOutput<PersonaCompany, IErrorData>> GetPersonasListAsync(Guid userRealPageGuid)
        {
            // Get active personas with rights for the user
            var personaList = _managePersona.ListActivePersona(userRealPageGuid, true);
            
            // Group personas by company, excluding external company
            var personaCompanyList = new List<PersonaCompany>();
            
            foreach (var persona in personaList)
            {
                // Skip external company personas
                if (persona.Organization.RealPageId == DefaultUserClaim.ExternalCompanyRealPageId)
                    continue;

                // Find or create company entry
                var companyEntry = personaCompanyList.FirstOrDefault(p => 
                    p.CompanyRealPageId == persona.Organization.RealPageId);
                
                if (companyEntry == null)
                {
                    companyEntry = new PersonaCompany
                    {
                        CompanyName = persona.Organization.Name,
                        CompanyRealPageId = persona.Organization.RealPageId,
                        Personas = new List<PersonaCompanyDetails>()
                    };
                    personaCompanyList.Add(companyEntry);
                }

                // Add persona to company's persona list
                companyEntry.Personas.Add(new PersonaCompanyDetails
                {
                    PersonaId = persona.PersonaId,
                    Name = persona.Name
                });
            }

            // Sort by company name
            var sortedList = personaCompanyList.OrderBy(p => p.CompanyName).ToList();

            return new ObjectListOutput<PersonaCompany, IErrorData>
            {
                list = sortedList
            };
        }

        public async Task<IList<SharedObjects.Saml.SamlProductAttributes>> GetSamlProductAttributesAsync(int productId)
        {
            return _samlRepository.GetSamlProductAttributes(productId);
        }

        public async Task<ListResponse> GetProductUserPropertiesAsync(
            string productCode, 
            RequestParameter dataFilter, 
            Guid? upfmId, 
            Guid? userRealPageId, 
            ClaimsPrincipal currentUser, 
            DefaultUserClaim userClaims, 
            IProductRepository productRepository)
        {
            var result = new ListResponse();
            
            try
            {
                var productList = productRepository.GetAllProducts();
                var productId = ProductEnumHelper.GetProductIdByProductCode(productCode, productList);

                // Special handling for VendorMarketplace
                if (productId == (int)ProductEnum.VendorMarketplace)
                {
                    // Validate internal API scope
                    if (!currentUser.HasClaim(c => c.Type.Equals("scope", StringComparison.OrdinalIgnoreCase) && 
                                                    c.Value.Equals("internalapi", StringComparison.OrdinalIgnoreCase)))
                    {
                        throw new UnauthorizedAccessException("Invalid Claim Scope.");
                    }

                    // Validate required parameters
                    if (upfmId == null || userRealPageId == null)
                    {
                        throw new ArgumentException("Invalid upfmId and userRealPageId.");
                    }

                    // Switch to UnifiedPlatform product for VendorMarketplace
                    productId = (int)ProductEnum.UnifiedPlatform;

                    // Recreate claims for the client user
                    var recreatedClaims = RecreateClaimsForClient(userRealPageId.Value, upfmId.Value);
                    
                    // Get integration with recreated claims
                    var integrationFactory = new IntegrationTypeFactory(
                        _manageProduct,
                        new ManageUnifiedLogin(recreatedClaims),
                        new ManageProductOneSite(recreatedClaims),
                        productRepository,
                        new ProductInternalSettingRepository(),
                        recreatedClaims);

                    var integration = integrationFactory.GetIntegration(productId);
                    result = integration.GetEnterpriseProperties(recreatedClaims.PersonaId, dataFilter);
                }
                else
                {
                    // Normal product handling
                    var integrationFactory = new IntegrationTypeFactory(
                        _manageProduct,
                        new ManageUnifiedLogin(userClaims),
                        new ManageProductOneSite(userClaims),
                        productRepository,
                        new ProductInternalSettingRepository(),
                        userClaims);

                    var integration = integrationFactory.GetIntegration(productId);
                    result = integration.GetEnterpriseProperties(userClaims.PersonaId, dataFilter);
                }

                // Check for forbidden access
                if (result.IsError)
                {
                    result.ErrorReason = "Forbidden - " + result.ErrorReason;
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                result = new ListResponse 
                { 
                    IsError = true, 
                    ErrorReason = ex.Message 
                };
            }
            catch (ArgumentException ex)
            {
                result = new ListResponse 
                { 
                    IsError = true, 
                    ErrorReason = ex.Message 
                };
            }
            catch (Exception ex)
            {
                result = new ListResponse 
                { 
                    IsError = true, 
                    ErrorReason = "Internal server error." 
                };
                // Log the exception (assuming logging is available)
                // _loggingService.WriteToLog(LogEventLevel.Error, "GetProductUserProperties", exception: ex);
            }

            return result;
        }

        private DefaultUserClaim RecreateClaimsForClient(Guid userRealPageId, Guid upfmId)
        {
            if (userRealPageId == Guid.Empty)
            {
                throw new ArgumentException("Invalid userRealPageId.");
            }

            var person = _personLogic.GetPerson(userRealPageId);
            if (person == null)
            {
                throw new Exception($"Missing persona information for client_info user while Recreation of Claims For Client. realPageId: {userRealPageId}");
            }

            var userLogin = _userLoginLogic.GetUserLoginOnly(userRealPageId);
            
            // Active Persona is linked to one organization
            var persona = _managePersona.GetActivePersonaWithoutRights(userRealPageId);
            
            if (persona == null)
            {
                throw new Exception($"No active persona found for user. realPageId: {userRealPageId}");
            }

            // Get roles for the persona
            var manageUserRoleRight = new ManageUserRoleRight();
            var roles = manageUserRoleRight.GetAssignedRoleForPersona(ProductEnum.UnifiedPlatform, persona.PersonaId);

            var recreatedClaim = new DefaultUserClaim
            {
                UserId = (int)userLogin.UserId,
                OrganizationPartyId = persona.Organization.PartyId,
                LoginName = userLogin.LoginName,
                OrganizationMasterId = (long)persona.Organization.BooksMasterId,
                CustomerMasterId = (long)persona.Organization.BooksMasterId,
                OrganizationName = persona.Organization.Name,
                FirstName = person.FirstName,
                LastName = person.LastName,
                PersonaId = persona.PersonaId,
                OrganizationRealPageGuid = persona.Organization.RealPageId,
                UserRealPageGuid = userRealPageId,
                CorrelationId = Guid.NewGuid(),
                RealPageEmployee = persona.Organization.RealPageId == DefaultUserClaim.EmployeeCompanyRealPageId
            };

            // Get rights using ManageProductBatch
            var manageProductBatch = new ManageProductBatch(recreatedClaim);
            recreatedClaim.Rights = manageProductBatch.GetPersonaRoleRights(persona.PersonaId, persona.Organization.PartyId);

            return recreatedClaim;
        }

        #region Private Helper Methods

        private Dictionary<string, string> ParseCustomFields(string customFieldsJson)
        {
            var dictionaryCustomFields = new Dictionary<string, string>();
            
            if (string.IsNullOrWhiteSpace(customFieldsJson))
                return dictionaryCustomFields;

            var customFieldValueList = JsonConvert.DeserializeObject<IList<CustomFieldValue>>(customFieldsJson);
            foreach (var c in customFieldValueList)
            {
                dictionaryCustomFields.Add(c.Name, c.Value);
            }

            return dictionaryCustomFields;
        }

        private IList<UserProductSAMLDetail> DeserializeUserProduct(string userProductJSON)
        {
            if (string.IsNullOrEmpty(userProductJSON))
                return new List<UserProductSAMLDetail>();

            return JsonConvert.DeserializeObject<IList<UserProductSAMLDetail>>(userProductJSON);
        }

        private async Task<UserRoleAssetDto> GetOpsUserRoleAssetAsync(long targetPersonaId, long editorPersonaId)
        {
            var samlRepository = new SamlRepository();
            var productList = samlRepository.ListActiveProductsByPersonaId(targetPersonaId, (int)ProductEnum.OpsBuyer, null);
            
            if (!productList.Any(p => p.ProductStatus == (int)ProductBatchStatusType.Success))
            {
                return new UserRoleAssetDto();
            }

            var manageProductOps = new ManageProductOps(new SharedObjects.Landing.DefaultUserClaim { PersonaId = editorPersonaId });
            var rolesResponse = manageProductOps.GetRoles(editorPersonaId, targetPersonaId, "", null);
            var assetsResponse = manageProductOps.GetCompanyAssets(editorPersonaId, targetPersonaId, false, null);

            if (rolesResponse.IsError)
            {
                throw new InvalidOperationException(rolesResponse.ErrorReason);
            }

            return new UserRoleAssetDto
            {
                ProductRole = rolesResponse.Records.Cast<ProductRole>().Where(p => p.IsAssigned).ToList(),
                AssetGroups = assetsResponse.Records.Cast<AssetGroup>().Where(p => p.IsAssigned).ToList()
            };
        }

        private void ValidatePersonaAccess(Persona persona, SharedObjects.Landing.DefaultUserClaim userClaims)
        {
            if (userClaims.OrganizationPartyId == 0)
            {
                List<Claim> claimList = ClaimsPrincipal.Current.Claims.ToList();
                if (!claimList.Any(p => p.Type.Equals("Scope", StringComparison.OrdinalIgnoreCase) && p.Value.Equals("internalapi", StringComparison.OrdinalIgnoreCase)))
                {
                    throw new KeyNotFoundException("Get User Products by persona: Invalid company id");                   
                }
                // Allow internal API access
                return;
            }

            if (userClaims.ImpersonatedBy == Guid.Empty && userClaims.OrganizationPartyId != persona.OrganizationPartyId)
            {
                throw new UnauthorizedAccessException("Get User Products by persona: Invalid company id");
            }
        }

        private void AddSessionTimeoutSetting(UserProductOutputResultv2 productResult, long organizationPartyId)
        {
            var sessionTimeout = 480;
            var settingList = _manageSettings.GetUnifiedSettingsCached("Security", organizationPartyId);
            var sessionTimeoutSetting = settingList.FirstOrDefault(p => 
                p.Name.Equals("SessionTimeout", StringComparison.OrdinalIgnoreCase));

            if (sessionTimeoutSetting != null && Int32.TryParse(sessionTimeoutSetting.Value, out var trySessionTimeout))
            {
                sessionTimeout = trySessionTimeout;
            }

            productResult.Settings.Add("SessionTimeout", sessionTimeout);
        }

        private async Task BuildUserInfoAsync(UserProductOutputResultv2 productResult, IPerson person, 
            Persona persona, bool withStatus, SharedObjects.Landing.DefaultUserClaim userClaims)
        {
            var personaList = _managePersona.ListActivePersona(persona.RealPageId, false);
            persona.hasMultiPersona = personaList.Count(p => p.OrganizationPartyId == persona.OrganizationPartyId) > 1;
            persona.hasMultiCompany = personaList.Count(p => 
                p.OrganizationPartyId != persona.OrganizationPartyId && 
                p.Organization.RealPageId != SharedObjects.Landing.DefaultUserClaim.ExternalCompanyRealPageId) > 0;

            productResult.User = new SharedObjects.Enterprise.User
            {
                FullName = $"{person.FirstName} {person.LastName}",
                RealPageId = person.RealPageId,
                CompanyName = persona.Organization.Name,
                PersonaId = persona.PersonaId,
                Title = persona.hasMultiPersona ? persona.Name : "",
                HasMultiCompany = persona.hasMultiCompany,
                HasMultiPersona = false
            };

            if (withStatus)
            {
                var userLogin = _userLoginLogic.GetUserLogin(persona.RealPageId, persona.OrganizationPartyId);
                productResult.User.Status = userLogin.Status.ToEnumDescription();
            }

            if (userClaims.IsRPEmployee)
            {
                var employeePersonaList = _managePersona.ListEmployeePersonas(userClaims.UserId, userClaims.OrganizationPartyId);
                productResult.User.HasMultiPersona = employeePersonaList?.Count > 1;
            }
        }

        private void OrganizeProducts(UserProductOutputResultv2 productResult, List<UserProducts> userProducts)
        {
            productResult.Products.Add("Favorites", userProducts.Where(p => p.IsFavorite).ToList());
            
            foreach (var up in userProducts)
            {
                var familyName = up.FamilyName ?? "None";
                if (!up.IsResource && !productResult.Products.ContainsKey(familyName))
                {
                    productResult.Products.Add(familyName, userProducts
                        .Where(p => !p.IsFavorite && !p.IsResource && (p.FamilyName ?? "None").Equals(familyName, StringComparison.OrdinalIgnoreCase))
                        .ToList());
                }

                if (up.IsResource)
                {
                    productResult.Resources.Add(up);
                }
            }
        }

        private async Task AddNavigationResourcesAsync(UserProductOutputResultv2 productResult, long personaId)
        {
            var rights = _manageSecurityLogic.GetPersonaRightsAndActionsByRoute(personaId, "sidemenu")?.obj?.Rights;
            var navigationMenu = _userRepository.GetNavigationMenu();
            var navigationMenuRights = _userRepository.GetNavigationMenuRights();

            var filteredMenuEntries = navigationMenu.Where(nmw =>
                !navigationMenuRights.Any(w => w.NavigationMenuId == nmw.Id) ||
                navigationMenuRights.Where(w => w.NavigationMenuId == nmw.Id).Any(a => rights.Contains(a.RightName))
            ).ToList();

            AddReportingResource(productResult, filteredMenuEntries);
            AddSettingsResource(productResult, filteredMenuEntries);
        }

        private void AddReportingResource(UserProductOutputResultv2 productResult, List<NavigationMenuEntry> menuEntries)
        {
            var reportingMenuEntry = menuEntries.FirstOrDefault(f => f.PageId == "reporting");
            if (reportingMenuEntry == null)
                return;

            var productRepository = new ProductRepository();
            var products = productRepository.GetAllProducts();
            var productcode = ProductEnumHelper.GetProductCodeByProductId(67, products);
            var reportsUrl = new Uri(new Uri(ConfigReader.GetLandingUri), reportingMenuEntry.URL).ToString();

            productResult.Resources.Add(new UserProducts
            {
                Name = "Reports",
                Description = reportingMenuEntry.Title,
                Url = reportsUrl,
                Label = "reports",
                IsNewTab = false,
                IsResource = true,
                ProductCode = productcode,
                Status = 8,
                Id = 67
            });
        }

        private void AddSettingsResource(UserProductOutputResultv2 productResult, List<NavigationMenuEntry> menuEntries)
        {
            var settingsMenuEntry = menuEntries.FirstOrDefault(f => f.PageId == "manage-settings");
            if (settingsMenuEntry == null)
                return;

            var productRepository = new ProductRepository();
            var products = productRepository.GetAllProducts();
            var productcode = ProductEnumHelper.GetProductCodeByProductId(56, products);
            var settingsUri = new Uri(new Uri(ConfigReader.GetLandingUri), settingsMenuEntry.URL).ToString();

            productResult.Resources.Add(new UserProducts
            {
                Name = "Settings",
                Description = settingsMenuEntry.Title,
                Url = settingsUri,
                Label = "settings",
                IsNewTab = false,
                IsResource = true,
                ShowInAppSwitcher = true,
                ProductCode = productcode,
                Status = 8,
                Id = 56
            });
        }

        private void FilterImpersonatedResources(UserProductOutputResultv2 productResult, SharedObjects.Landing.DefaultUserClaim userClaims)
        {
            if (userClaims.ImpersonatedBy == Guid.Empty)
                return;

            productResult.Resources.RemoveAll(r => 
                r.Id == (int)ProductEnum.ClientPortal || 
                r.Id == (int)ProductEnum.AdminSupportPortal);
        }       

        #endregion
    }
}
