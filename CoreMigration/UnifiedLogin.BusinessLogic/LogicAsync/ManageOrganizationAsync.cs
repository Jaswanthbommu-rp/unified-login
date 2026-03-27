using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.BusinessLogic.Logic.Product;
using UnifiedLogin.BusinessLogic.Logic.ProductIntegration.Factory;
using UnifiedLogin.BusinessLogic.Logic.ProductIntegration.Model;
using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Audit.Common;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.BlackBook;
using UnifiedLogin.SharedObjects.Constants;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Extensions;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Maintenance;
using UnifiedLogin.SharedObjects.Product;
using UnifiedLogin.SharedObjects.Product.Accounting;
using UnifiedLogin.SharedObjects.Product.Ops;
using UnifiedLogin.SharedObjects.Product.Rum;
using UnifiedLogin.SharedObjects.Product.UnifiedLogin;
using Organization = UnifiedLogin.SharedObjects.Landing.Organization;

namespace UnifiedLogin.BusinessLogic.LogicAsync;

/// <summary>
/// True async implementation of organization management operations.
/// Replaces the stepping-stone wrapper that delegated to the sync <c>IManageOrganization</c>.
/// Sync-only collaborators retained for paths that have no async equivalent yet:
///   <list type="bullet">
///     <item><c>IManageUser</c> — <c>CreateUser</c> has no async counterpart</item>
///     <item><c>IManageProductPanel</c> — <c>GetProductProperties</c> has no async counterpart</item>
///     <item><c>IIntegrationTypeFactory</c> — factory pattern with no async equivalent</item>
///   </list>
/// </summary>
public sealed class ManageOrganizationAsync : IManageOrganizationAsync
{
    #region Private Fields

    private readonly IOrganizationRepositoryAsync _orgRepo;
    private readonly IProductRepositoryAsync _productRepo;
    private readonly IProductInternalSettingRepositoryAsync _productInternalSettingRepo;
    private readonly IOrganizationProductRepositoryAsync _orgProductRepo;
    private readonly IPropertyRepositoryAsync _propertyRepo;
    private readonly IConfigurationSettingRepositoryAsync _configSettingRepo;
    private readonly IManageBlueBookAsync _blueBook;
    private readonly IManageOrganizationProductAsync _manageOrgProduct;
    private readonly IManageUnifiedSettingsAsync _unifiedSettings;
    private readonly IManageProductAsync _manageProduct;
    private readonly IManageProductAssetOptimizationAsync _assetOptimization;
    private readonly IManagePartyRoleAsync _managePartyRole;
    private readonly IManageUserLoginAsync _manageUserLogin;
    private readonly IPersonaRepositoryAsync _personaRepo;
    private readonly IManagePersonAsync _managePerson;
    private readonly ITokenHelperAsync _tokenHelper;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<ManageOrganizationAsync> _logger;

    // Sync-only collaborators retained because no async equivalent exists.
    private readonly IManageUser _manageUser;
    private readonly IManageProductPanel _manageProductPanel;
    private readonly IIntegrationTypeFactory _integrationTypeFactory;

    #endregion

    #region Constructor

    public ManageOrganizationAsync(
        IOrganizationRepositoryAsync orgRepo,
        IProductRepositoryAsync productRepo,
        IProductInternalSettingRepositoryAsync productInternalSettingRepo,
        IOrganizationProductRepositoryAsync orgProductRepo,
        IPropertyRepositoryAsync propertyRepo,
        IConfigurationSettingRepositoryAsync configSettingRepo,
        IManageBlueBookAsync blueBook,
        IManageOrganizationProductAsync manageOrgProduct,
        IManageUnifiedSettingsAsync unifiedSettings,
        IManageProductAsync manageProduct,
        IManageProductAssetOptimizationAsync assetOptimization,
        IManagePartyRoleAsync managePartyRole,
        IManageUserLoginAsync manageUserLogin,
        IPersonaRepositoryAsync personaRepo,
        IManagePersonAsync managePerson,
        IManageUser manageUser,
        IManageProductPanel manageProductPanel,
        IIntegrationTypeFactory integrationTypeFactory,
        ITokenHelperAsync tokenHelper,
        IHttpClientFactory httpClientFactory,
        ILogger<ManageOrganizationAsync> logger)
    {
        ArgumentNullException.ThrowIfNull(orgRepo);
        ArgumentNullException.ThrowIfNull(productRepo);
        ArgumentNullException.ThrowIfNull(productInternalSettingRepo);
        ArgumentNullException.ThrowIfNull(orgProductRepo);
        ArgumentNullException.ThrowIfNull(propertyRepo);
        ArgumentNullException.ThrowIfNull(configSettingRepo);
        ArgumentNullException.ThrowIfNull(blueBook);
        ArgumentNullException.ThrowIfNull(manageOrgProduct);
        ArgumentNullException.ThrowIfNull(unifiedSettings);
        ArgumentNullException.ThrowIfNull(manageProduct);
        ArgumentNullException.ThrowIfNull(assetOptimization);
        ArgumentNullException.ThrowIfNull(managePartyRole);
        ArgumentNullException.ThrowIfNull(manageUserLogin);
        ArgumentNullException.ThrowIfNull(personaRepo);
        ArgumentNullException.ThrowIfNull(managePerson);
        ArgumentNullException.ThrowIfNull(manageUser);
        ArgumentNullException.ThrowIfNull(manageProductPanel);
        ArgumentNullException.ThrowIfNull(integrationTypeFactory);
        ArgumentNullException.ThrowIfNull(tokenHelper);
        ArgumentNullException.ThrowIfNull(httpClientFactory);
        ArgumentNullException.ThrowIfNull(logger);

        _orgRepo = orgRepo;
        _productRepo = productRepo;
        _productInternalSettingRepo = productInternalSettingRepo;
        _orgProductRepo = orgProductRepo;
        _propertyRepo = propertyRepo;
        _configSettingRepo = configSettingRepo;
        _blueBook = blueBook;
        _manageOrgProduct = manageOrgProduct;
        _unifiedSettings = unifiedSettings;
        _manageProduct = manageProduct;
        _assetOptimization = assetOptimization;
        _managePartyRole = managePartyRole;
        _manageUserLogin = manageUserLogin;
        _personaRepo = personaRepo;
        _managePerson = managePerson;
        _manageUser = manageUser;
        _manageProductPanel = manageProductPanel;
        _integrationTypeFactory = integrationTypeFactory;
        _tokenHelper = tokenHelper;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    #endregion

    // ── Organization CRUD ──────────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<ObjectOutput<OrganizationCreateResult, IErrorData>> CreateOrganizationAsync(
        OrganizationCreate organization,
        List<int> addProductList,
        bool processBlueBookMessage = false,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("{ActionName} - organization name {Name}", nameof(CreateOrganizationAsync), organization.Name);

        var outputResult = new ObjectOutput<OrganizationCreateResult, IErrorData>
        {
            Status = new Status<IErrorData> { Success = false }
        };

        if (organization.OrganizationTypeId == 0)
        {
            _logger.LogError("{ActionName} - invalid OrganizationTypeId for {Name}", nameof(CreateOrganizationAsync), organization.Name);
            outputResult.Status.ErrorMsg = $"An invalid Organization Type id was given: {organization.OrganizationTypeId}";
            return outputResult;
        }

        OrganizationAdminUser aUser = organization.AdminUser;
        if (aUser == null)
        {
            _logger.LogError("{ActionName} - AdminUser is null for {Name}", nameof(CreateOrganizationAsync), organization.Name);
            outputResult.Status.ErrorMsg = "No admin user information provided";
            return outputResult;
        }

        // create the organization
        var org = new Organization
        {
            Name = organization.Name,
            BooksMasterId = (long)organization.BooksCompanyId,
            BooksCustomerMasterId = (long)organization.BooksCustomerMasterId,
            organizationType = new OrganizationType { OrganizationTypeId = organization.OrganizationTypeId },
            OrganizationDomain = new OrganizationDomain
            {
                OrganizationDomainId = organization.OrganizationDomainId,
                Name = organization.OrganizationDomain
            },
            IsActive = organization.IsActive
        };

        RepositoryResponse insertResponse;
        if (org.RealPageId != Guid.Empty)
        {
            insertResponse = await _orgRepo.UpdateOrganizationAsync(org);
        }
        else
        {
            insertResponse = await _orgRepo.InsertOrganizationAsync(org);
        }

        if (!string.IsNullOrEmpty(insertResponse.ErrorMessage))
        {
            outputResult.Status.ErrorMsg = insertResponse.ErrorMessage;
            return outputResult;
        }

        Guid organizationRealPageId = insertResponse.RealPageId;
        org = await _orgRepo.GetOrganizationAsync(organizationRealPageId);

        // create/update primary property and enterprise role settings (CreatedBy = 0 — no userClaim in this path)
        await CreatePrimaryPropertyEnterpriseRoleMasterConfigurationSettingAsync(org.PartyId, "PrimaryProperty", organization.EnablePrimaryProperties, 0, cancellationToken);
        await CreatePrimaryPropertyEnterpriseRoleMasterConfigurationSettingAsync(org.PartyId, "EnterpriseRole", organization.EnableEnterpriseRoles, 0, cancellationToken);

        org.EnablePrimaryProperties = organization.EnablePrimaryProperties;
        org.EnableEnterpriseRoles = organization.EnablePrimaryProperties;

        // add products to the new company
        var productResponse = await AddProductsToOrganizationAsync(addProductList, org.PartyId, organization.OrganizationTypeId, organization.Name, cancellationToken);
        if (!string.IsNullOrEmpty(productResponse.ErrorMessage))
        {
            outputResult.Status.ErrorMsg = productResponse.ErrorMessage;
            return outputResult;
        }

        // create the initial super user admin
        aUser.Email = $"{org.PartyId}admin@realpage.com".Replace(" ", "");
        await CreateInitialOrgSuperUserAsync(org.PartyId, aUser.FirstName, string.Empty, aUser.LastName,
            aUser.Title, aUser.Suffix, aUser.Email, true, null, organizationRealPageId, cancellationToken);

        var createOrg = new OrganizationCreateResult
        {
            Org = org,
            adminLogin = aUser.Email,
            BooksCompanyId = org.BooksMasterId,
            BooksCustomerMasterId = org.BooksCustomerMasterId
        };

        // create additional company admin user when requested (BlueBook provisioning path)
        if (processBlueBookMessage && organization.CompanyAdminUser != null
            && !string.IsNullOrWhiteSpace(organization.CompanyAdminUser.Email))
        {
            _logger.LogDebug("{ActionName} - creating company admin user {Email}", nameof(CreateOrganizationAsync), organization.CompanyAdminUser.Email);

            var findExistingUser = await _manageUserLogin.GetUserLoginOnlyAsync(organization.CompanyAdminUser.Email, cancellationToken);

            IList<Persona> personaList = new List<Persona>();
            var profileDetail = new ProfileDetail
            {
                FirstName = organization.CompanyAdminUser.FirstName,
                LastName = organization.CompanyAdminUser.LastName,
                RoleIdList = organization.CompanyAdminUser.RoleIds,
                MiddleName = string.Empty,
                NotificationEmail = string.Empty,
                Password = string.Empty,
                Persona = new List<Persona>(),
                CreateUserSourceType = CreateUserSourceType.UnifiedPlatform,
                TelecommunicationNumber = new List<TelecommunicationNumber>(),
                CustomFields = new List<CustomFieldValue>(),
                UserTypeId = (int)UserRoleType.SuperUser,
                Title = string.Empty,
                userLogin = new UserLogin
                {
                    ThruDate = null,
                    LoginName = organization.CompanyAdminUser.Email,
                    IsActive = true,
                    IsPending = false,
                    IsExpired = false,
                    FromDate = DateTime.UtcNow,
                    Is3rdPartyIDP = false
                },
                productBatch = new List<ProductBatch>(),
                ExternalUserRelationship = new ExternalUserRelationship { ThirdPartyRelationShipId = 8, ThirdPartyRelationShip = "8" }
            };

            if (findExistingUser != null)
            {
                var existingPerson = await _managePerson.GetPersonAsync(findExistingUser.RealPageId, cancellationToken);
                if (existingPerson != null)
                {
                    profileDetail.FirstName = existingPerson.FirstName;
                    profileDetail.LastName = existingPerson.LastName;
                    profileDetail.UserTypeId = (int)UserRoleType.ExternalUser;

                    var productIdList = new List<int> { (int)ProductEnum.UnifiedPlatform };
                    var gbAllRoles = await _productRepo.ListRolesForProductByPartyAsync(org.PartyId, productIdList, (int)ProductEnum.UnifiedPlatform, cancellationToken);
                    var productInternalSettings = await _productInternalSettingRepo.GetProductInternalSettingsAsync((int)ProductEnum.UnifiedPlatform, cancellationToken);
                    var platformAdminRole = productInternalSettings.FirstOrDefault(s => s.Name.Equals("PlatformAdminRole", StringComparison.OrdinalIgnoreCase))?.Value;

                    if (gbAllRoles.Any(p => p.Roletype.Equals("System", StringComparison.OrdinalIgnoreCase)
                        && p.Name.Equals(platformAdminRole, StringComparison.OrdinalIgnoreCase)))
                    {
                        string roleId = gbAllRoles.ToList().Find(p => p.Roletype.Equals("System", StringComparison.OrdinalIgnoreCase)
                            && p.Name.Equals(platformAdminRole, StringComparison.OrdinalIgnoreCase))?.ID;
                        if (!string.IsNullOrEmpty(roleId))
                        {
                            profileDetail.productBatch = new List<ProductBatch>
                            {
                                new ProductBatch
                                {
                                    ProductId = (int)ProductEnum.UnifiedPlatform,
                                    InputJson = new RolePropertyList { RoleList = new List<string> { roleId } }
                                }
                            };
                        }
                    }
                }
            }

            var personaEnvironments = await _personaRepo.GetPersonaEnvironmentTypeAsync(cancellationToken);
            var productionEnv = personaEnvironments.SingleOrDefault(p => p.Name.Equals("Production", StringComparison.OrdinalIgnoreCase));
            if (productionEnv == null)
            {
                outputResult.Status.Success = true;
                outputResult.Status.ErrorMsg = "MessageHandler.Handle - Persona environment is missing!";
                return outputResult;
            }

            var persona = new Persona
            {
                Name = profileDetail.UserTypeId == (int)UserRoleType.SuperUser ? "System Administrator" : "Primary",
                PersonaEnvironmentTypeId = (int)productionEnv.PersonaEnvironmentTypeId,
                FromDate = DateTime.UtcNow,
                ThruDate = null
            };
            personaList.Add(persona);
            profileDetail.Persona = personaList;

            if (profileDetail.organization.Count == 0)
            {
                profileDetail.organization.Add(org);
            }

            _logger.LogDebug("{ActionName} - calling CreateUser for {Login}", nameof(CreateOrganizationAsync), profileDetail.userLogin.LoginName);

            // IManageUser.CreateUser has no async equivalent — kept as sync call.
            CreateUserResponse<IErrorData> errorDataResponse = _manageUser.CreateUser(profileDetail, personaList);
            if (!errorDataResponse.Status.Success)
            {
                _logger.LogError("{ActionName} - CreateUser failed for {Login}: {Error}",
                    nameof(CreateOrganizationAsync), profileDetail.userLogin.LoginName, errorDataResponse.Status.ErrorMsg);
                outputResult.Status.Success = true;
                outputResult.Status.ErrorMsg = $"{profileDetail.userLogin.LoginName}: " + errorDataResponse.Status.ErrorMsg;
                return outputResult;
            }

            IPartyRole partyRole = new PartyRole { RoleTypeId = profileDetail.UserTypeId };
            await _managePartyRole.CreatePartyRoleEnterpriseUserIDAsync(profileDetail.userLogin.RealPageId, partyRole, cancellationToken);
        }

        outputResult.Status.Success = true;
        outputResult.Status.ErrorMsg = string.Empty;
        outputResult.obj = createOrg;
        return outputResult;
    }

    /// <inheritdoc/>
    public async Task<RepositoryResponse> UpdateOrganizationAsync(Organization organization, CancellationToken cancellationToken = default)
    {
        if (organization == null)
            return new RepositoryResponse { ErrorMessage = "Organization is Null" };

        if (organization.RealPageId == Guid.Empty)
            return new RepositoryResponse { ErrorMessage = "Invalid parameter realPageId." };

        return await _orgRepo.UpdateOrganizationAsync(organization);
    }

    /// <inheritdoc/>
    public async Task<Organization> GetOrganizationAsync(Guid realPageId, long? organizationPartyId = null, CancellationToken cancellationToken = default)
    {
        if (realPageId == Guid.Empty && organizationPartyId == null)
            throw new Exception("Invalid parameter: Organization realPageId, partyId is required.");

        var organization = await _orgRepo.GetOrganizationAsync(realPageId, organizationPartyId);
        if (organization == null)
            return null;

        var companyIdps = await _orgRepo.GetOrganizationIdentityProviderTypeAsync(realPageId);
        if (companyIdps != null && companyIdps.Count > 1)
        {
            var idp = companyIdps.FirstOrDefault(i => i.Name != "IdentityServer");
            organization.ThirdPartyIDP = idp?.Name ?? "None";
        }
        else
        {
            organization.ThirdPartyIDP = "None";
        }

        return organization;
    }

    /// <inheritdoc/>
    public async Task<IList<Organization>> GetOrganizationListAsync(CancellationToken cancellationToken = default)
        => await _orgRepo.GetOrganizationListAsync();

    /// <inheritdoc/>
    public async Task UpdateOrganizationUsePrimaryPropertySettingAsync(Organization organization, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(organization);

        if (organization.RealPageId == Guid.Empty)
            throw new Exception("Invalid parameter realPageId.");

        await CreatePrimaryPropertyEnterpriseRoleMasterConfigurationSettingAsync(organization.PartyId, "PrimaryProperty", organization.EnablePrimaryProperties, 0, cancellationToken);
        await CreatePrimaryPropertyEnterpriseRoleMasterConfigurationSettingAsync(organization.PartyId, "EnterpriseRole", organization.EnableEnterpriseRoles, 0, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task UpdateOrganizationThirdPartyIDPAsync(Organization organization, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(organization);

        if (organization.RealPageId == Guid.Empty)
            throw new ArgumentNullException(nameof(organization), "Invalid parameter realPageId.");

        var companyIdps = await _orgRepo.GetOrganizationIdentityProviderTypeAsync(organization.RealPageId);
        if (companyIdps.Count < 2 && organization.ThirdPartyIDP != "None")
        {
            await _orgRepo.UpdateOrganizationThirdPartyIDPAsync(organization);
        }
    }

    /// <inheritdoc/>
    public async Task<Guid> GetOrganizationAdminUserRealPageIdAsync(Guid organizationRealPageId, CancellationToken cancellationToken = default)
        => await _orgRepo.GetOrganizationAdminUserRealPageIdAsync(organizationRealPageId);

    /// <inheritdoc/>
    public async Task<IList<IdentityProviderType>> GetOrganizationIdentityProviderTypeAsync(Guid realPageId, CancellationToken cancellationToken = default)
        => await _orgRepo.GetOrganizationIdentityProviderTypeAsync(realPageId);

    /// <inheritdoc/>
    public async Task<bool> AddCompanyToJobAsync(
        string companyInstanceSourceId, long createdBy, long createUserPersonaId,
        int organizationIsActive, CancellationToken cancellationToken = default)
    {
        var response = await _orgRepo.AddCompanyToJobAsync(companyInstanceSourceId, createdBy, createUserPersonaId, organizationIsActive);
        return response.Id > 0 && string.IsNullOrEmpty(response.ErrorMessage);
    }

    /// <inheritdoc/>
    public async Task<bool> AddUpdateCompanyToUnifiedSettingsAsync(
        string companyInstanceID, string transactionType, string customerEnvironment = null,
        CancellationToken cancellationToken = default)
    {
        var payload = new UnifiedSettingCompanyPropertyPayload
        {
            Payload = new UnifiedSettingCompanyProperty
            {
                Source = ProductEnumHelper.StringValueOf(ProductEnum.UnifiedPlatform),
                Company = new UnifiedSettingCompanyInstance
                {
                    CompanyInstanceSourceId = companyInstanceID.ToString().ToLower()
                },
                Properties = new List<UnifiedSettingCompanyPropertyInstance>(),
                CustomerEnvironment = customerEnvironment
            }
        };

        return await _unifiedSettings.CreateUpdateCompanyInSettingAsync(
            payload,
            transactionType.ToLower() == "create" ? HttpMethod.Post : HttpMethod.Put,
            cancellationToken);
    }

    /// <inheritdoc/>
    public async Task DeleteQueuedOrganizationsAsync(CancellationToken cancellationToken = default)
    {
        var productInternalSettings = await _manageProduct.GetProductInternalSettingsAsync(3, cancellationToken);

        int batchSize = productInternalSettings.Any(p => p.Name.Equals("OrganizationRemovalBatchSize", StringComparison.OrdinalIgnoreCase))
            ? Convert.ToInt32(productInternalSettings.First(p => p.Name.Equals("OrganizationRemovalBatchSize", StringComparison.OrdinalIgnoreCase)).Value)
            : 5;

        int retryCount = productInternalSettings.Any(p => p.Name.Equals("OrganizationRemovalRetryCount", StringComparison.OrdinalIgnoreCase))
            ? Convert.ToInt32(productInternalSettings.First(p => p.Name.Equals("OrganizationRemovalRetryCount", StringComparison.OrdinalIgnoreCase)).Value)
            : 3;

        var orgsToDelete = await _orgRepo.GetOrganizationToDeleteAsync(batchSize, retryCount, false);

        foreach (var p in orgsToDelete)
        {
            try
            {
                var deleteResult = await _orgRepo.DeleteOrganizationAsync(p.OrganizationRemovalQueueId, p.OrganizationPartyId, p.OrganizationRealPageId);

                if (deleteResult == p.OrganizationPartyId)
                {
                    if (p.OrganizationRemoveUDMData)
                    {
                        var propertyToDeleteList = await _blueBook.GetPropertyInstanceForCompanyAsync(p.OrganizationRealPageId, cancellationToken);
                        if (propertyToDeleteList != null)
                        {
                            foreach (var propertyToDelete in propertyToDeleteList)
                            {
                                var propertyInstanceToDelete = new Guid(propertyToDelete.attributes.propertyInstanceSourceId);
                                _logger.LogDebug("{ActionName} - deleting property {Id} from company {Company}",
                                    nameof(DeleteQueuedOrganizationsAsync), propertyInstanceToDelete, p.OrganizationRealPageId);
                                await _propertyRepo.DeleteUPFMPropertyInstanceAsync(propertyInstanceToDelete, cancellationToken);
                                await DeletePropertyForOrganizationAsync(propertyInstanceToDelete, p.OrganizationRealPageId, cancellationToken);
                            }
                        }

                        var udmResult = await _blueBook.DeleteBooksGreenBookCompanyInstanceAsync(
                            new CompanyInstance { CompanyInstanceSourceId = p.OrganizationRealPageId.ToString(), ModifiedBy = "UPFM Delete company" },
                            cancellationToken);
                        await _orgRepo.UpdateOrganizationRemovalQueueStatusAsync(p.OrganizationRemovalQueueId, udmResult ? "UDMData Removed" : "UDMData Removal Failed");
                    }

                    // delete activity log data
                    try
                    {
                        var activityUriSetting = productInternalSettings.FirstOrDefault(s => s.Name.Equals("ActivityLogUri", StringComparison.OrdinalIgnoreCase));
                        if (activityUriSetting != null && !string.IsNullOrEmpty(activityUriSetting.Value))
                        {
                            var activityUri = activityUriSetting.Value;
                            var ulClientToken = await _tokenHelper.GetUnifiedLoginServerTokenAsync("activityreader companyfunctions", cancellationToken);
                            var client = _httpClientFactory.CreateClient();
                            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ulClientToken);
                            var deleteUri = new Uri(activityUri + $"api/activity/organization/{p.OrganizationPartyId}");
                            _logger.LogDebug("{ActionName} - DELETE ActivityLog {Uri}", nameof(DeleteQueuedOrganizationsAsync), deleteUri);

                            var response = await client.DeleteAsync(deleteUri, cancellationToken);
                            var status = response.IsSuccessStatusCode ? "ActivityLog Removed" : "ActivityLog Removal Failed";

                            if (!response.IsSuccessStatusCode)
                            {
                                _logger.LogError("{ActionName} - ActivityLog.Delete failed {StatusCode} for company {Company} queue {Queue}",
                                    nameof(DeleteQueuedOrganizationsAsync), response.StatusCode, p.OrganizationRealPageId, p.OrganizationRemovalQueueId);
                            }

                            await _orgRepo.UpdateOrganizationRemovalQueueStatusAsync(p.OrganizationRemovalQueueId, status);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "{ActionName} - error deleting ActivityLog for company {Company} queue {Queue}",
                            nameof(DeleteQueuedOrganizationsAsync), p.OrganizationRealPageId, p.OrganizationRemovalQueueId);
                    }

                    await _orgRepo.UpdateOrganizationRemovalQueueStatusAsync(p.OrganizationRemovalQueueId, "Complete");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ActionName} - error deleting company {Company} queue {Queue}",
                    nameof(DeleteQueuedOrganizationsAsync), p.OrganizationRealPageId, p.OrganizationRemovalQueueId);
            }
        }
    }

    /// <inheritdoc/>
    public async Task<OrganizationRemovalQueue> InsertOrganizationRemovalQueueAsync(
        OrganizationRemovalQueue organizationRemovalQueue, CancellationToken cancellationToken = default)
        => await _orgRepo.InsertOrganizationRemovalQueueAsync(organizationRemovalQueue);

    // ── Organization types / domains ───────────────────────────────────────

    /// <inheritdoc/>
    public async Task<List<OrganizationType>> ListOrganizationTypeAsync(CancellationToken cancellationToken = default)
        => await _orgRepo.ListOrganizationTypeAsync();

    /// <inheritdoc/>
    public async Task<List<OrganizationDomain>> ListOrganizationDomainAsync(CancellationToken cancellationToken = default)
        => await _orgRepo.ListOrganizationDomainAsync();

    /// <inheritdoc/>
    public async Task<RepositoryResponse> CreateOrganizationDomainAsync(
        OrganizationDomain organizationDomain, CancellationToken cancellationToken = default)
        => await _orgRepo.CreateOrganizationDomainAsync(organizationDomain);

    // ── Product parsing ────────────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<List<string>> ParseProductAsync(
        List<string> productCode, List<int> addProductList, CancellationToken cancellationToken = default)
    {
        var allProducts = await _productRepo.GetAllProductsAsync(cancellationToken);
        var invalidProductList = new List<string>();

        if (productCode != null)
        {
            foreach (var code in productCode)
            {
                var found = allProducts.FirstOrDefault(a => a.BooksProductCode?.Equals(code, StringComparison.OrdinalIgnoreCase) == true);
                if (found != null)
                    addProductList.Add(found.ProductId);
                else
                    invalidProductList.Add(code);
            }
        }

        return invalidProductList;
    }

    /// <inheritdoc/>
    public async Task EnableProductOnOtherProductsActivationAsync(List<int> addProductList, CancellationToken cancellationToken = default)
    {
        var productsToActivate = await _productInternalSettingRepo.GetProductSettingByTypeAsync("EnableProductOnOtherProductsActivation", cancellationToken);
        foreach (var setting in productsToActivate)
        {
            int[] dependencyIds = Array.ConvertAll(setting.Value.Split(','), int.Parse);
            foreach (int dependencyId in dependencyIds)
            {
                if (addProductList.Contains(dependencyId) && !addProductList.Contains(setting.ProductId))
                    addProductList.Add(setting.ProductId);
            }
        }
    }

    /// <inheritdoc/>
    public async Task<RepositoryResponse> UpdateUsePrimaryPropertyForOrganizationProductAsync(
        long organizationPartyId, int productId, bool usePrimaryProperty, CancellationToken cancellationToken = default)
    {
        if (organizationPartyId == 0)
            return new RepositoryResponse { ErrorMessage = "Invalid parameter organizationPartyId." };
        if (productId == 0)
            return new RepositoryResponse { ErrorMessage = "Invalid parameter productId." };

        var org = await _orgRepo.GetOrganizationAsync(organizationPartyId: organizationPartyId);
        if (org.EnablePrimaryProperties != 1)
            return new RepositoryResponse { ErrorMessage = "Primary properties is not turned on at company level." };

        var productSettings = await _productRepo.GetProductSettingsAsync(org.RealPageId, productId, cancellationToken);
        var oldSetting = productSettings
            .FirstOrDefault(p => p.Name.Equals("UsePrimaryProperties", StringComparison.OrdinalIgnoreCase))?.Value
            ?? string.Empty;

        var productSettingTypeId = await _productRepo.GetProductSettingTypeAsync("UsePrimaryProperties", cancellationToken);
        var response = await _orgProductRepo.CreateOrganizationProductSettingAsync(organizationPartyId, productId, productSettingTypeId, usePrimaryProperty ? "1" : "0", cancellationToken);

        return response;
    }

    // ── Company setup ──────────────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<List<CompanySetup>> GetCompanyListAsync(
        string organizationName, int domain, int? blueId, int organizationId,
        IDictionary<object, object> globals, CancellationToken cancellationToken = default)
    {
        RequestParameter dataFilter = new RequestParameter();
        if (globals != null && globals.ContainsKey(BaseType.RequestParameter))
            dataFilter = globals[BaseType.RequestParameter] as RequestParameter;

        var companies = await _orgRepo.GetCompanyListAsync(organizationName, domain, blueId, organizationId, dataFilter);
        return await EnrichCompaniesWithBooksAddressAsync(companies, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<CompanyMaster> SearchCompanyDetailsByCustomerCompanyIdAsync(
        long customerCompanyId, CancellationToken cancellationToken = default)
    {
        var companyMaster = new CompanyMaster
        {
            CompanyDetail = await _blueBook.GetBooksCompanyDetailsByCompanyMasterIdAsync(customerCompanyId, cancellationToken),
            DomainList = await _blueBook.GetListOfDomainsByCompanyAsync(customerCompanyId, cancellationToken)
        };

        var companyInstances = await _blueBook.GetCompanyInstancesByCustomerCompanyIdAsync(customerCompanyId, cancellationToken);
        foreach (var instance in companyInstances)
        {
            if (instance?.attributes != null)
                companyMaster.CompanyInstance.Add(instance.attributes);
        }

        foreach (var domain in companyMaster.DomainList.ToList())
        {
            if (companyMaster.CompanyInstance.Exists(a => a.Domain.Equals(domain.Domain, StringComparison.OrdinalIgnoreCase)))
                companyMaster.DomainList.Remove(companyMaster.DomainList.First(a => a.Domain.Equals(domain.Domain, StringComparison.OrdinalIgnoreCase)));
        }

        return companyMaster;
    }

    // ── Property management ────────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<List<CompanyPropertySetup>> GetPropertiesForCompanyAsync(
        Guid companyInstanceId, string propertyName = null, string domain = null,
        int? blueId = null, int? status = null, IDictionary<object, object> globals = null,
        long editorPersonaId = 0, long userPersonaId = 0, bool? isSelectedProperties = null,
        List<Guid> selectedProperties = null, string operatorCode = null, string operatorValue = null,
        CancellationToken cancellationToken = default)
    {
        RequestParameter dataFilter = new RequestParameter();
        if (globals != null && globals.ContainsKey(BaseType.RequestParameter))
            dataFilter = globals[BaseType.RequestParameter] as RequestParameter;

        List<BooksPropertyInstance> booksPropertyInstance;

        if (string.IsNullOrEmpty(operatorCode) || string.IsNullOrEmpty(operatorValue))
        {
            booksPropertyInstance = await _blueBook.GetPropertyInstanceForCompanyAsync(companyInstanceId, cancellationToken);
        }
        else
        {
            booksPropertyInstance = await _blueBook.GetPropertyInstanceForCompanyAsync(companyInstanceId, cancellationToken);
            var uPFMProperty = new UPFMProperty
            {
                id = booksPropertyInstance.Select(a => a.attributes.propertyInstanceSourceId).ToList()
            };
            // editorPersonaId maps to DefaultUserClaim.PersonaId from the legacy pattern
            var editorClaim = new DefaultUserClaim { PersonaId = editorPersonaId };
            var aoProperties = await _assetOptimization.GetPropertiesWithOperatorsAsync(editorClaim, editorPersonaId, userPersonaId, operatorCode, operatorValue, cancellationToken);
            var productResult = await _blueBook.TranslateProductPrimaryPropertiesDataAsync(uPFMProperty, 4, aoProperties, cancellationToken);
            var propertyInstanceIds = productResult.Records.Cast<ProductProperty>()
                .Where(c => c.InstanceId != null).Select(a => a.InstanceId);
            booksPropertyInstance = booksPropertyInstance?.Where(a => propertyInstanceIds.Contains(a.attributes.propertyInstanceSourceId)).ToList();
        }

        List<Guid> filteredInstanceIds;
        if (domain != null)
        {
            string[] domainFilter = domain.Split(',');
            filteredInstanceIds = booksPropertyInstance?.Where(p => domainFilter.Contains(p.attributes.domain))
                .Select(p => p.attributes.propertyInstanceSourceId)?.Select(Guid.Parse).ToList() ?? new List<Guid>();
        }
        else
        {
            filteredInstanceIds = booksPropertyInstance?.Select(p => p.attributes.propertyInstanceSourceId)?.Select(Guid.Parse).ToList() ?? new List<Guid>();
        }

        List<int> userProperties = null;
        List<UPFMPropertyInstance> selectedPropertyInstances = new List<UPFMPropertyInstance>();

        if (userPersonaId > 0 || (selectedProperties != null && selectedProperties.Count > 0))
        {
            status = 1; // primary properties tab should only show active properties
            userProperties = await _propertyRepo.ListUPFMPropertyInstanceIdByPersonaAsync(userPersonaId, ProductEnum.UnifiedPlatform, cancellationToken);
            selectedPropertyInstances = await _propertyRepo.ListUPFMPropertyInstanceByPersonaAsync(userPersonaId, ProductEnum.UnifiedPlatform, cancellationToken);

            var selectedIds = selectedPropertyInstances?.Select(p => p.InstanceId).ToList();
            if (selectedProperties != null && selectedProperties.Count > 0)
                selectedIds = selectedProperties;

            if (isSelectedProperties == true)
                filteredInstanceIds = selectedIds;
            else if (isSelectedProperties == false)
                filteredInstanceIds = filteredInstanceIds.Except(selectedIds).ToList();
        }

        if ((selectedProperties == null || selectedProperties.Count == 0) && isSelectedProperties == true)
            filteredInstanceIds = new List<Guid>();

        var propertyDetails = new List<PropertySetup>();
        if (filteredInstanceIds != null && filteredInstanceIds.Count > 0)
        {
            propertyDetails = await _propertyRepo.GetPropertiesForCompanyAsync(filteredInstanceIds, propertyName, blueId, status, dataFilter, cancellationToken);
            propertyDetails = AddContractedNameToPropertyList(booksPropertyInstance, propertyDetails, userProperties);
        }

        return new List<CompanyPropertySetup>
        {
            new CompanyPropertySetup
            {
                Domain = booksPropertyInstance?.Where(p => p.attributes.domain != null).Select(p => p.attributes.domain).Distinct().ToList(),
                Property = propertyDetails,
                SelectedPropertyIds = selectedPropertyInstances?.Select(p => p.InstanceId).ToList<Guid>()
            }
        };
    }

    /// <inheritdoc/>
    public async Task<List<UPFMPropertyInstance>> GetPropertiesByInstanceIdAsync(
        List<Guid> propertyInstanceIds, CancellationToken cancellationToken = default)
        => await _propertyRepo.ListUPFMPropertyInstanceIdByInstanceIdsAsync(propertyInstanceIds, cancellationToken);

    /// <inheritdoc/>
    /// <remarks>
    /// <c>userClaim</c> was previously used to construct <c>new ManageOrganization(userClaim)</c>.
    /// In this implementation the claim is not needed for data access; the overload is retained
    /// because the controller still calls it with a claim.
    /// </remarks>
    public async Task<List<UPFMPropertyInstance>> GetPropertiesByInstanceIdAsync(
        List<Guid> propertyInstanceIds, DefaultUserClaim userClaim, CancellationToken cancellationToken = default)
        => await _propertyRepo.ListUPFMPropertyInstanceIdByInstanceIdsAsync(propertyInstanceIds, cancellationToken);

    /// <inheritdoc/>
    public async Task<RepositoryResponse> UpdatePropertyListAsync(
        List<UPFMPropertyInstance> propertyList, Guid companyInstanceId, CancellationToken cancellationToken = default)
    {
        if (propertyList == null || propertyList.Any(m => m.InstanceId == Guid.Empty) || propertyList.Any(m => string.IsNullOrEmpty(m.Name)))
            return new RepositoryResponse { ErrorMessage = "Invalid parameter propertyInstanceId." };

        var response = await _propertyRepo.UpdateUPFMPropertyListAsync(propertyList, cancellationToken);

        if (response != null && response.Id > 0)
        {
            var ack = new BulkPropertyInstanceStatusAck
            {
                propertyInstanceSourceIds = propertyList.Select(m => m.InstanceId.ToString()).ToList(),
                Status = propertyList.FirstOrDefault().IsActive,
                ModifiedBy = ProductEnumHelper.StringValueOf(ProductEnum.UnifiedPlatform)
            };
            bool booksResponse = await _blueBook.AcknowledgeBulkPropertyListUpdateAsync(ack, cancellationToken);
            response = HandleErrorMessage(booksResponse, true, "Error while updating property", response);
        }

        return response;
    }

    /// <inheritdoc/>
    public async Task<RepositoryResponse> UpdatePropertyListAsync(
        List<UPFMPropertyInstance> propertyList, Guid companyInstanceId, DefaultUserClaim userClaim,
        CancellationToken cancellationToken = default)
        => await UpdatePropertyListAsync(propertyList, companyInstanceId, cancellationToken);

    /// <inheritdoc/>
    public async Task<RepositoryResponse> UpdateCompanyInstanceAsync(
        long companyBatchJobId, int statusTypeId, string errorMessage, CancellationToken cancellationToken = default)
        => await _orgRepo.UpdateCompanyStatus(companyBatchJobId, statusTypeId, errorMessage);

    /// <inheritdoc/>
    public async Task<IRepositoryResponse> ProcessPropertyListAsync(
        UPFMPropertyInstance property, Guid companyInstanceId, DefaultUserClaim userClaim,
        CancellationToken cancellationToken = default)
    {
        var currentProperty = await _propertyRepo.ListUPFMPropertyInstanceIdByInstanceIdsAsync(
            new List<Guid> { property.InstanceId }, cancellationToken);

        if (currentProperty != null)
        {
            return await UpdatePropertyAsync(property, companyInstanceId, cancellationToken);
        }

        return new RepositoryResponse();
    }

    /// <inheritdoc/>
    public async Task<bool> UpdatePropertyInSettingsAndActivityLogsAsync(
        UPFMPropertyInstance property, Guid companyInstanceId,
        List<UPFMPropertyInstance> oldPropertyList, DefaultUserClaim userClaim,
        CancellationToken cancellationToken = default)
    {
        var propGuidList = new List<Guid> { property.InstanceId };
        var propertyInstance = (await _propertyRepo.ListUPFMPropertyInstanceIdByInstanceIdsAsync(propGuidList, cancellationToken))?.FirstOrDefault();
        if (propertyInstance == null)
            return false;

        var payload = PreparePropertyObjectToUnifiedSetting(propertyInstance, companyInstanceId);
        return await _unifiedSettings.CreateUpdatePropertyInSettingAsync(payload, HttpMethod.Put, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<RepositoryResponse> AddPropertyForOrganizationAsync(
        UPFMPropertyInstance property, Guid companyInstanceID, CancellationToken cancellationToken = default)
    {
        var response = await _propertyRepo.InsertUPFMPropertyInstanceAsync(property, cancellationToken);
        property.InstanceId = response.RealPageId;

        if (response.ErrorMessage.Length == 0)
        {
            bool booksResponse = await AddPropertyToBooksAsync(property, companyInstanceID, cancellationToken);
            bool settingsResponse = false;
            if (booksResponse)
                settingsResponse = await AddPropertyToUnifiedSettingsAsync(property, companyInstanceID, cancellationToken);

            response = HandleErrorMessage(booksResponse, settingsResponse, "Error while adding property", response);
        }

        return response;
    }

    /// <inheritdoc/>
    public async Task<RepositoryResponse> DeletePropertyForOrganizationAsync(
        Guid propertyInstanceID, Guid companyInstanceID, CancellationToken cancellationToken = default)
    {
        var upfmProperties = new UPFMProperty { id = new List<string> { propertyInstanceID.ToString().ToLower() } };

        // Hard-coded to "SET" — same reason as legacy: no other way to get product code from company setup property delete
        var translatedData = await _blueBook.GetTranslatePropertiesFromUPFMToProductv3Async(upfmProperties, "SET", cancellationToken);
        var settingPropertyInstance = translatedData.Data?.Attributes
            ?.Find(p => p.PropertyInstanceSourceId == propertyInstanceID.ToString())
            ?.TranslatedPropertyInstances?.FirstOrDefault();

        var response = await _propertyRepo.DeleteUPFMPropertyInstanceAsync(propertyInstanceID, cancellationToken);

        if (response.ErrorMessage.Length == 0)
        {
            bool booksResponse = await _blueBook.DeletePropertyFromBooksAsync(propertyInstanceID, cancellationToken);
            bool settingsResponse = settingPropertyInstance != null ? false : true;
            if (booksResponse && settingPropertyInstance != null)
                settingsResponse = await _unifiedSettings.DeletePropertyInSettingAsync(settingPropertyInstance.PropertyInstanceSourceId.ToString().ToLower(), cancellationToken);

            response = HandleErrorMessage(booksResponse, settingsResponse, "Error while deleting property", response);
        }

        return response;
    }

    /// <inheritdoc/>
    public async Task<List<UPFMPropertyInstance>> GetPropertyByInstanceIdAsync(
        Guid propertyInstanceId, CancellationToken cancellationToken = default)
        => await _propertyRepo.ListUPFMPropertyInstanceIdByInstanceIdsAsync(new List<Guid> { propertyInstanceId }, cancellationToken);

    /// <inheritdoc/>
    public async Task<RepositoryResponse> UpdatePropertyAsync(
        UPFMPropertyInstance property, Guid companyInstanceId, CancellationToken cancellationToken = default)
    {
        if (property.InstanceId == Guid.Empty)
            return new RepositoryResponse { ErrorMessage = "Invalid parameter propertyInstanceId." };
        if (string.IsNullOrEmpty(property.Name))
            return new RepositoryResponse { ErrorMessage = "Invalid parameter propertyName." };

        var response = await _propertyRepo.UpdatePropertyAsync(property, cancellationToken);

        if (response.Id > 0)
        {
            var ack = new PropertyInstanceAck
            {
                PropertyInstanceSourceId = property.InstanceId.ToString(),
                Source = ProductEnumHelper.StringValueOf(ProductEnum.UnifiedPlatform),
                PropertyName = property.Name,
                IsActive = property.IsActive,
                Address = new PropertyInstanceAddress
                {
                    Address = property.Address,
                    City = property.City,
                    State = property.State,
                    PostalCode = property.PostalCode,
                    County = property.County,
                    Country = property.Country,
                },
                ModifiedBy = ProductEnumHelper.StringValueOf(ProductEnum.UnifiedPlatform)
            };
            bool booksResponse = await _blueBook.AcknowledgePropertyUpdateAsync(ack, cancellationToken);
            bool settingsResponse = false;
            if (booksResponse)
            {
                var propGuidList = new List<Guid> { property.InstanceId };
                var instance = (await _propertyRepo.ListUPFMPropertyInstanceIdByInstanceIdsAsync(propGuidList, cancellationToken))?.FirstOrDefault();
                if (instance != null)
                {
                    var payload = PreparePropertyObjectToUnifiedSetting(instance, companyInstanceId);
                    settingsResponse = await _unifiedSettings.CreateUpdatePropertyInSettingAsync(payload, HttpMethod.Put, cancellationToken);
                }
            }
            response = HandleErrorMessage(booksResponse, settingsResponse, "Error while updating property", response);
        }

        return response;
    }

    /// <inheritdoc/>
    public async Task<PropertyInstanceSearch> SearchPropertyDetailsByCustomerPropertyIdAsync(
        string customerPropertyId, string booksCustomerMasterId, CancellationToken cancellationToken = default)
    {
        var booksPropertyInstance = await _blueBook.GetCustomerPropertyDetailsAsync(customerPropertyId, cancellationToken);
        if (booksPropertyInstance == null)
            return new PropertyInstanceSearch();

        if (booksPropertyInstance.attributes?.customerCompanyId != booksCustomerMasterId)
            return new PropertyInstanceSearch();

        var propertyDetails = await _blueBook.GetCustomerPropertyDetailsAsync(customerPropertyId, cancellationToken);
        var booksPropertyInstances = await _blueBook.GetUPFMPropertyInstancesByCustomerPropertyIdAsync(customerPropertyId, cancellationToken);

        var listPropertySetup = new List<PropertySetup>();
        if (booksPropertyInstances != null)
        {
            foreach (var bp in booksPropertyInstances)
            {
                listPropertySetup.Add(new PropertySetup
                {
                    Name = bp?.attributes.propertyName,
                    Domain = bp?.attributes.domain,
                    IsActive = bp?.attributes.isActive,
                    InstanceId = string.IsNullOrEmpty(bp?.attributes.propertyInstanceSourceId)
                        ? Guid.Empty
                        : Guid.Parse(bp.attributes.propertyInstanceSourceId),
                    ContractedName = bp?.attributes?.customerPropertyMap?.FirstOrDefault()?.customerProperty?.FirstOrDefault()?.propertyName,
                    Address = bp?.attributes?.customerPropertyMap?.FirstOrDefault()?.customerProperty?.FirstOrDefault()?.address?.Address,
                    City = bp?.attributes?.customerPropertyMap?.FirstOrDefault()?.customerProperty?.FirstOrDefault()?.address?.City,
                    State = bp?.attributes?.customerPropertyMap?.FirstOrDefault()?.customerProperty?.FirstOrDefault()?.address?.State,
                    Country = bp?.attributes?.customerPropertyMap?.FirstOrDefault()?.customerProperty?.FirstOrDefault()?.address?.Country,
                    County = bp?.attributes?.customerPropertyMap?.FirstOrDefault()?.customerProperty?.FirstOrDefault()?.address?.County,
                    Latitude = bp?.attributes?.customerPropertyMap?.FirstOrDefault()?.customerProperty?.FirstOrDefault()?.address?.Latitude,
                    Longitude = bp?.attributes?.customerPropertyMap?.FirstOrDefault()?.customerProperty?.FirstOrDefault()?.address?.Longitude
                });
            }
        }

        var allProductInstances = await _blueBook.GetAllProductsPropertyInstanceFromBooksAsync(customerPropertyId, cancellationToken);
        var distinctDomains = allProductInstances?.Where(p => p.attributes.domain != null).Select(p => p.attributes.domain).Distinct().ToList();
        var existingDomains = listPropertySetup.Where(p => p.Domain != null).Select(p => p.Domain).Distinct().ToList();
        var domainList = existingDomains != null ? distinctDomains?.Except(existingDomains).ToList() : new List<string>();

        return new PropertyInstanceSearch
        {
            CustomerProperty = propertyDetails,
            PropertyInstance = listPropertySetup,
            Domain = domainList
        };
    }

    /// <inheritdoc/>
    public async Task<ProductPropertyDetails> GetSourceProductDetailsAsync(
        string propertyInstanceSourceId, string source, CancellationToken cancellationToken = default)
    {
        var booksProductSource = await _blueBook.GetPropertyDetailsByPropertyInstanceIdAndSourceAsync(propertyInstanceSourceId, source, cancellationToken);
        if (booksProductSource == null || booksProductSource.attributes == null)
            return new ProductPropertyDetails();

        var customerProp = booksProductSource.attributes?.customerPropertyMap?
            .Where(p => p.propertyInstanceSourceId == booksProductSource.attributes.propertyInstanceSourceId)
            .FirstOrDefault();

        var productStatus = new ProductStatusDetail
        {
            IsActive = booksProductSource.attributes.isActive,
            ProductInstanceId = booksProductSource.attributes.propertyInstanceSourceId,
            Domain = booksProductSource.attributes.domain,
            CustomerPropertyId = customerProp?.customerPropertyId.ToString(),
            ContractedName = customerProp?.customerProperty?.FirstOrDefault()?.propertyName.ToString()
        };

        var listPropertySetup = new List<PropertySetup>();
        if (customerProp != null)
        {
            var booksPropertyInstances = await _blueBook.GetUPFMPropertyInstancesByCustomerPropertyIdAsync(customerProp.customerPropertyId.ToString(), cancellationToken);
            if (booksPropertyInstances != null)
            {
                foreach (var bp in booksPropertyInstances)
                {
                    listPropertySetup.Add(new PropertySetup
                    {
                        Name = bp?.attributes.propertyName,
                        Domain = bp?.attributes.domain,
                        IsActive = bp?.attributes.isActive,
                        InstanceId = string.IsNullOrEmpty(bp?.attributes.propertyInstanceSourceId)
                            ? Guid.Empty
                            : Guid.Parse(bp.attributes.propertyInstanceSourceId),
                    });
                }
            }
        }

        return new ProductPropertyDetails
        {
            ProductStatusDetail = productStatus,
            PropertyDetails = listPropertySetup
        };
    }

    /// <inheritdoc/>
    public async Task<List<PropertyAudit>> AuditCompanyProductPropertiesToUPFMAsync(
        Guid companyInstanceId, int productId, DefaultUserClaim userClaim, CancellationToken cancellationToken = default)
    {
        var propertyAuditResult = new List<PropertyAudit>();
        var upfmPropertyDetails = new List<PropertySetup>();

        var productInternalSettings = await _manageProduct.GetProductInternalSettingsAsync(productId, cancellationToken);
        var producIntegrationType = productInternalSettings.FirstOrDefault(p => p.Name.Equals("productintegrationtype", StringComparison.OrdinalIgnoreCase))?.Value;

        // IIntegrationTypeFactory and IManageProductPanel have no async equivalents — kept as sync calls.
        ListResponse productResult;
        if (producIntegrationType?.Equals("UPFM", StringComparison.OrdinalIgnoreCase) == true)
        {
            var integration = _integrationTypeFactory.GetIntegration(productId);
            productResult = integration.GetEnterpriseProperties(userClaim.PersonaId, new RequestParameter());
        }
        else
        {
            productResult = _manageProductPanel.GetProductProperties(userClaim.PersonaId, 0, productId, null);
        }

        if (productResult.Records == null)
            return propertyAuditResult;

        var upfmProperties = new UPFMProperty();
        var instanceIds = new List<string>();
        var instanceGuids = new List<Guid>();

        var booksPropertyInstance = await _blueBook.GetPropertyInstanceForCompanyAsync(companyInstanceId, cancellationToken);
        if (booksPropertyInstance != null)
        {
            foreach (var property in booksPropertyInstance)
            {
                instanceIds.Add(property.attributes.propertyInstanceSourceId.ToLower());
                instanceGuids.Add(new Guid(property.attributes.propertyInstanceSourceId));
            }
        }

        if (instanceGuids.Count > 0)
        {
            var upfmList = await _propertyRepo.ListUPFMPropertyInstanceIdByInstanceIdsAsync(instanceGuids, cancellationToken);
            foreach (var pd in upfmList)
            {
                var booksInstance = booksPropertyInstance?.FirstOrDefault(b => b.attributes.propertyInstanceSourceId.Equals(pd.InstanceId.ToString(), StringComparison.OrdinalIgnoreCase));
                upfmPropertyDetails.Add(new PropertySetup
                {
                    Domain = booksInstance?.attributes.domain,
                    ContractedName = booksInstance?.attributes.propertyName,
                    Name = pd.Name,
                    InstanceId = pd.InstanceId
                });
                pd.Domain = booksInstance?.attributes.domain;
            }
        }

        upfmProperties.id = instanceIds;

        var booksProductDetail = await _productRepo.GetBooksMasterProductDetailAsync(productId, cancellationToken);
        TranslatePropertyInstance translatedData;

        if (booksProductDetail.ProductId != (int)ProductEnum.UnifiedPlatform)
        {
            var source = string.IsNullOrEmpty(booksProductDetail.UDMSourceCode)
                ? booksProductDetail.BooksProductCode
                : booksProductDetail.UDMSourceCode;
            translatedData = await _blueBook.GetTranslatePropertiesFromUPFMToProductv3Async(upfmProperties, source, cancellationToken);
        }
        else
        {
            translatedData = new TranslatePropertyInstance
            {
                Data = new TranslatePropertyInstanceData { Attributes = new List<TranslatePropertyInstanceAttribute>() }
            };

            if (booksPropertyInstance != null)
            {
                foreach (var instance in booksPropertyInstance)
                {
                    var tpi = new List<TranslatedPropertyInstanceData>
                    {
                        new TranslatedPropertyInstanceData
                        {
                            PropertyInstanceSourceId = instance.attributes.propertyInstanceSourceId,
                            Source = instance.attributes.source
                        }
                    };
                    translatedData.Data.Attributes.Add(new TranslatePropertyInstanceAttribute
                    {
                        PropertyInstanceSourceId = instance.attributes.propertyInstanceSourceId,
                        Source = booksProductDetail.BooksProductCode,
                        TranslatedPropertyInstances = tpi
                    });
                }
            }
        }

        if (productResult.Records.Count > 0)
        {
            var productPropertyType = productResult.Records[0].GetType();

            if (productPropertyType == typeof(ProductProperty))
            {
                foreach (var property in productResult.Records.Cast<ProductProperty>())
                    AuditPropertyCompare(property.ID, property.Name, property.InstanceId, translatedData, instanceIds, upfmPropertyDetails, propertyAuditResult);
            }
            else if (productPropertyType == typeof(ACProperty))
            {
                foreach (var property in productResult.Records.Cast<ACProperty>())
                    AuditPropertyCompare(property.BookID, property.PropertyName, null, translatedData, instanceIds, upfmPropertyDetails, propertyAuditResult);
            }
            else if (productPropertyType == typeof(AssetGroup))
            {
                foreach (var property in productResult.Records.Cast<AssetGroup>())
                    AuditPropertyCompare(property.AssetID, property.Name, null, translatedData, instanceIds, upfmPropertyDetails, propertyAuditResult);
            }
            else if (productPropertyType == typeof(OnSiteProperty))
            {
                foreach (var property in productResult.Records.Cast<OnSiteProperty>())
                    AuditPropertyCompare(property.GetPropertyId.ToString(), property.GetName, null, translatedData, instanceIds, upfmPropertyDetails, propertyAuditResult);
            }
            else if (productPropertyType == typeof(RumPropertyGroup))
            {
                foreach (var property in productResult.Records.Cast<RumPropertyGroup>())
                    AuditPropertyCompare(property.Id.ToString(), property.Name, null, translatedData, instanceIds, upfmPropertyDetails, propertyAuditResult);
            }
            else if (productPropertyType == typeof(ProductProperties))
            {
                foreach (var property in productResult.Records.Cast<ProductProperties>())
                    AuditPropertyCompare(property.GetPropertyId.ToString(), property.GetName, null, translatedData, instanceIds, upfmPropertyDetails, propertyAuditResult);
            }
            else if (productPropertyType == typeof(Portfolio))
            {
                foreach (var property in productResult.Records.Cast<Portfolio>())
                    AuditPropertyCompare(property.ID, property.Name, null, translatedData, instanceIds, upfmPropertyDetails, propertyAuditResult);
            }
        }

        return propertyAuditResult.OrderBy(p => p.Name).ThenBy(p => p.ContractedName).ToList();
    }

    // ── Private helpers ────────────────────────────────────────────────────

    private async Task<IRepositoryResponse> AddProductsToOrganizationAsync(
        List<int> addProductList, long partyId, int organizationTypeId, string organizationName,
        CancellationToken cancellationToken)
    {
        var orgTypeList = await _orgRepo.ListOrganizationTypeAsync();
        var orgTypeName = orgTypeList.Find(o => o.OrganizationTypeId == organizationTypeId)?.Name;

        var alwaysEnabledSettings = await _productInternalSettingRepo.GetProductSettingByTypeAsync("AlwaysEnableProductForOrgType", cancellationToken);
        foreach (var setting in alwaysEnabledSettings)
        {
            string[] types = setting.Value.Split(',');
            if (types.Contains(orgTypeName) && !addProductList.Contains(setting.ProductId))
                addProductList.Add(setting.ProductId);
        }

        await EnableProductOnOtherProductsActivationAsync(addProductList, cancellationToken);

        RepositoryResponse lastResponse = new RepositoryResponse();
        foreach (int product in addProductList)
        {
            lastResponse = await _manageOrgProduct.InsertUpdateOrganizationProductAsync(partyId, product, null, null, null, organizationName, cancellationToken);
        }

        return lastResponse;
    }

    private async Task<RepositoryResponse> CreateInitialOrgSuperUserAsync(
        long organizationId, string firstName, string middleName, string lastName,
        string title, string suffix, string email, bool defaultIDP, int? idpTypeId,
        Guid organizationRealPageId, CancellationToken cancellationToken)
    {
        IList<int> productIdList = await _productRepo.GetProductIdsByCompanyAsync(organizationRealPageId, cancellationToken);

        var settings = await _productInternalSettingRepo.GetProductInternalSettingsAsync(3, cancellationToken);
        var excludeProductList = settings.FirstOrDefault(a => a.Name.Equals("ExcludeProductFromOrgSupportUser", StringComparison.OrdinalIgnoreCase))?.Value;

        if (excludeProductList != null)
        {
            foreach (var pid in excludeProductList.Split(','))
                productIdList.Remove(productIdList.FirstOrDefault(p => p == Convert.ToInt32(pid)));
        }
        else
        {
            // default exclusion list: Unified Platform, Asset Optimization, RealPage Accounting,
            // Client Portal, Product Updates, EasyLMS, Admin & Support Portal
            int[] removeProductIds = { 3, 4, 8, 14, 28, 36, 89, 98, 104 };
            foreach (var pid in removeProductIds)
                productIdList.Remove(productIdList.FirstOrDefault(p => p == pid));
        }

        return await _orgRepo.CreateInitialOrgSuperUserAsync(organizationId, firstName, middleName, lastName, title, suffix, email, defaultIDP, idpTypeId, productIdList);
    }

    private async Task CreatePrimaryPropertyEnterpriseRoleMasterConfigurationSettingAsync(
        long partyId, string mappingName, int value, long createdBy, CancellationToken cancellationToken)
    {
        var setting = new MasterConfigurationSetting
        {
            PartyId = partyId.ToString(),
            MappingName = mappingName,
            Value = value.ToString(),
            CreatedBy = createdBy
        };
        await _configSettingRepo.CreatePrimaryPropertyEnterpriseRoleMasterConfigurationSettingAsync(setting, cancellationToken);
    }

    private async Task<List<CompanySetup>> EnrichCompaniesWithBooksAddressAsync(
        List<CompanySetup> companyDetails, CancellationToken cancellationToken)
    {
        var compList = new List<UnifiedLoginCompany>();
        var companyInstanceList = new List<string>();

        foreach (var item in companyDetails)
        {
            companyInstanceList.Add(item.RealPageId.ToString().ToLower());
            compList.Add(new UnifiedLoginCompany
            {
                CompanyId = long.Parse(item.BooksMasterId.ToString()),
                BooksCustomerMasterId = long.Parse(item.BooksCustomerMasterId.ToString() == string.Empty ? "0" : item.BooksCustomerMasterId.ToString())
            });
        }

        var booksCompanyDetails = await _blueBook.GetCompanyListByCompIdsAsync(compList, cancellationToken);
        var booksInstanceDetails = await _blueBook.GetUPFMCompanyDetailsByInstanceIdsAsync(companyInstanceList, cancellationToken);

        foreach (var item in companyDetails)
        {
            var address = booksInstanceDetails.Where(add => add.attributes.companyInstanceSourceId == item.RealPageId.ToString())
                .FirstOrDefault()?.attributes.CompanyInstanceLocation;
            item.ContractedName = booksCompanyDetails.Where(add => add.Id == item.BooksCustomerMasterId).FirstOrDefault()?.CompanyName;
            if (address != null && address.Count > 0)
            {
                item.CompanyLocation = address[0];
                item.Address = address[0]?.Address + "," + address[0]?.City + "," + address[0]?.State + "," + address[0]?.PostalCode;
            }

            var idpList = await _orgRepo.GetCompanyIDPListAsync(item.OrganizationPartyId);
            var companyIdps = await _orgRepo.GetOrganizationIdentityProviderTypeAsync(item.RealPageId);

            item.ThirdPartyIdps ??= new List<ThirdPartyIDPs>();

            foreach (var idp in idpList)
            {
                if (idp.IDPName.Equals("IdentityServer", StringComparison.OrdinalIgnoreCase))
                    idp.IDPName = "None";

                item.ThirdPartyIdps.Add(new ThirdPartyIDPs
                {
                    IDPName = idp.IDPName,
                    IsAssigned = companyIdps.Any(p => p.ContactMechanismId == idp.ContactMechanismId)
                });
            }

            if (companyIdps.Count > 1)
            {
                var noneIdp = item.ThirdPartyIdps.FirstOrDefault(i => i.IDPName.Equals("None", StringComparison.OrdinalIgnoreCase));
                if (noneIdp != null)
                    noneIdp.IsAssigned = false;
            }
        }

        return companyDetails;
    }

    private async Task<bool> AddPropertyToBooksAsync(
        UPFMPropertyInstance property, Guid companyInstanceID, CancellationToken cancellationToken)
    {
        var pi = new PropertyInstance
        {
            PropertyName = property.Name,
            CompanyInstanceSourceId = companyInstanceID.ToString().ToLower(),
            PropertyInstanceSourceId = property.InstanceId.ToString(),
            CustomerEnvironment = property.Domain,
            Source = ProductEnumHelper.StringValueOf(ProductEnum.UnifiedPlatform),
            IsActive = true,
            Address = new InstanceAddress
            {
                Address = property.Address,
                City = property.City,
                State = property.State,
                PostalCode = property.PostalCode,
                County = property.County,
                Country = property.Country,
            },
            ModifiedBy = ProductEnumHelper.StringValueOf(ProductEnum.UnifiedPlatform) + " Property Creation"
        };

        if (!string.IsNullOrEmpty(property.PropertyInstancePartner) && !string.IsNullOrEmpty(property.PropertyInstancePartnerSourceId))
        {
            pi.PropertyInstancePartners = new List<PropertyInstancePartner>
            {
                new PropertyInstancePartner { TargetSource = property.PropertyInstancePartner, TargetPropertyInstanceSourceId = property.PropertyInstancePartnerSourceId }
            };
        }

        return await _blueBook.AddBooksGreenBookPropertyInstanceFromProvisioningAsync(pi, cancellationToken);
    }

    private async Task<bool> AddPropertyToUnifiedSettingsAsync(
        UPFMPropertyInstance property, Guid companyInstanceID, CancellationToken cancellationToken)
    {
        var payload = PreparePropertyObjectToUnifiedSetting(property, companyInstanceID);
        return await _unifiedSettings.CreateUpdatePropertyInSettingAsync(payload, HttpMethod.Post, cancellationToken);
    }

    private static UnifiedSettingCompanyPropertyPayload PreparePropertyObjectToUnifiedSetting(
        UPFMPropertyInstance property, Guid companyInstanceID)
    {
        return new UnifiedSettingCompanyPropertyPayload
        {
            Payload = new UnifiedSettingCompanyProperty
            {
                Source = ProductEnumHelper.StringValueOf(ProductEnum.UnifiedPlatform),
                Company = new UnifiedSettingCompanyInstance
                {
                    CompanyInstanceSourceId = companyInstanceID.ToString().ToLower()
                },
                Properties = new List<UnifiedSettingCompanyPropertyInstance>
                {
                    new UnifiedSettingCompanyPropertyInstance
                    {
                        PropertyName = property.Name,
                        PropertyInstanceSourceId = property.InstanceId,
                        CustomerPropertyId = !string.IsNullOrEmpty(property.CustomerPropertyId) ? property.CustomerPropertyId : "0",
                        IsActive = property.IsActive,
                        Address = property.Address,
                        City = property.City,
                        State = property.State,
                        PostalCode = property.PostalCode,
                        County = property.County,
                        Country = property.Country
                    }
                },
                CustomerEnvironment = property.Domain
            }
        };
    }

    private static RepositoryResponse HandleErrorMessage(
        bool booksResponse, bool settingsResponse, string errorMessage, RepositoryResponse response)
    {
        if (booksResponse && settingsResponse)
            return response;

        if (!booksResponse && !settingsResponse)
            response.ErrorMessage = errorMessage + " from Books and Settings";
        else if (!booksResponse)
            response.ErrorMessage = errorMessage + " from Books";
        else
            response.ErrorMessage = errorMessage + " from Settings";

        return response;
    }

    private static List<PropertySetup> AddContractedNameToPropertyList(
        List<BooksPropertyInstance> booksPropertyInstance,
        List<PropertySetup> propertySetup,
        List<int> userProperties)
    {
        foreach (var property in propertySetup)
        {
            var match = booksPropertyInstance?.Find(pi => pi.attributes.propertyInstanceSourceId.ToString() == property.InstanceId.ToString());

            property.ContractedName = match?.attributes.customerPropertyMap?.FirstOrDefault()?.customerProperty.FirstOrDefault()?.propertyName;
            property.Domain = match?.attributes.domain;

            var address = match?.attributes.address;
            property.CustomerStatus = match?.attributes.customerPropertyMap?.FirstOrDefault()?.customerProperty.FirstOrDefault()?.isActive ?? false;
            property.OrderType = match?.attributes.customerPropertyMap?.FirstOrDefault()?.customerProperty.FirstOrDefault()?.customerPropertyOrderType.FirstOrDefault()?.orderType;

            property.Address = address?.Address;
            property.City = address?.City;
            property.State = address?.State;
            property.PostalCode = address?.PostalCode;
            property.Country = address?.Country;
            property.County = address?.County;
            property.PropertyAddress = address?.Address + "," + address?.City + "," + address?.State + "," + address?.PostalCode;

            if (userProperties != null && userProperties.Count > 0 && userProperties.Contains(property.PropertyInstanceId))
                property.IsAssigned = true;
        }

        return propertySetup;
    }

    private static void AuditPropertyCompare(
        string propertyId, string propertyName, string instanceId,
        TranslatePropertyInstance translatedData, List<string> instanceids,
        List<PropertySetup> upfmPropertyDetails, List<PropertyAudit> propertyAuditResult)
    {
        var pa = new PropertyAudit { Name = propertyName, ProductInstanceId = propertyId };

        if (instanceId == null)
        {
            var instanceExists = translatedData.Data?.Attributes.Find(p =>
                p.TranslatedPropertyInstances.Exists(o => o.PropertyInstanceSourceId == propertyId));

            if (instanceExists != null)
            {
                pa.UPFMInstanceId = instanceExists.PropertyInstanceSourceId;
                pa.Status = instanceids.TrueForAll(p => p != instanceExists.PropertyInstanceSourceId) ? "No ID" : "OK";

                var upfmProperty = upfmPropertyDetails.Find(p => p.InstanceId == new Guid(instanceExists.PropertyInstanceSourceId));
                if (upfmProperty != null)
                {
                    pa.UPFMName = upfmProperty.Name;
                    pa.Domain = upfmProperty.Domain;
                    pa.ContractedName = upfmProperty.ContractedName;
                }
            }

            if (translatedData.Data == null)
                pa.Status = "No ID";
        }
        else
        {
            Guid.TryParse(propertyId, out var propertyGuid);
            Guid.TryParse(instanceId, out var instanceIdGuid);
            var upfmProperty = upfmPropertyDetails.Find(p => p.InstanceId == instanceIdGuid || p.InstanceId == propertyGuid);
            if (upfmProperty != null)
            {
                pa.Status = "OK";
                pa.UPFMName = upfmProperty.Name;
                pa.Domain = upfmProperty.Domain;
                pa.ContractedName = upfmProperty.ContractedName;
                pa.UPFMInstanceId = upfmProperty.InstanceId.ToString();
            }
        }

        if (!propertyAuditResult.Exists(p =>
            pa.ProductInstanceId != null && p.ProductInstanceId != null && p.ProductInstanceId.Equals(pa.ProductInstanceId, StringComparison.OrdinalIgnoreCase) &&
            pa.ContractedName != null && p.ContractedName != null && p.ContractedName.Equals(pa.ContractedName, StringComparison.OrdinalIgnoreCase) &&
            pa.Domain != null && p.Domain != null && p.Domain.Equals(pa.Domain, StringComparison.OrdinalIgnoreCase) &&
            pa.Name != null && p.Name != null && p.Name.Equals(pa.Name, StringComparison.OrdinalIgnoreCase) &&
            pa.UPFMInstanceId != null && p.UPFMInstanceId != null && p.UPFMInstanceId.Equals(pa.UPFMInstanceId, StringComparison.OrdinalIgnoreCase) &&
            pa.UPFMName != null && p.UPFMName != null && p.UPFMName.Equals(pa.UPFMName, StringComparison.OrdinalIgnoreCase)))
        {
            propertyAuditResult.Add(pa);
        }
    }
}
