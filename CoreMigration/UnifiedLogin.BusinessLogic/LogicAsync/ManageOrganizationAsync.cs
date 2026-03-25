using UnifiedLogin.BusinessLogic.Logic;
using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.BlackBook;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Maintenance;
using UnifiedLogin.SharedObjects.Product;

namespace UnifiedLogin.BusinessLogic.LogicAsync;

/// <summary>
/// Stepping-stone async wrapper for organization management operations.
/// Delegates to the existing sync <see cref="IManageOrganization"/> via <see cref="Task.FromResult{TResult}"/>.
/// UserClaim overloads replace the <c>new ManageOrganization(userClaim)</c> anti-pattern inside controller actions.
/// </summary>
public sealed class ManageOrganizationAsync : IManageOrganizationAsync
{
    private readonly IManageOrganization _manageOrganization;

    public ManageOrganizationAsync(IManageOrganization manageOrganization)
    {
        _manageOrganization = manageOrganization ?? throw new ArgumentNullException(nameof(manageOrganization));
    }

    // ── Organization CRUD ──────────────────────────────────────────────────

    public Task<Organization> GetOrganizationAsync(Guid realPageId, long? organizationPartyId = null, CancellationToken cancellationToken = default)
        => Task.FromResult(_manageOrganization.GetOrganization(realPageId, organizationPartyId));

    public Task<IList<Organization>> GetOrganizationListAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(_manageOrganization.GetOrganizationList());

    public Task<ObjectOutput<OrganizationCreateResult, IErrorData>> CreateOrganizationAsync(OrganizationCreate organization, List<int> addProductList, bool processBlueBookMessage = false, CancellationToken cancellationToken = default)
        => Task.FromResult(_manageOrganization.CreateOrganization(organization, addProductList, processBlueBookMessage));

    public Task<RepositoryResponse> UpdateOrganizationAsync(Organization organization, CancellationToken cancellationToken = default)
        => Task.FromResult(_manageOrganization.UpdateOrganization(organization));

    public Task UpdateOrganizationUsePrimaryPropertySettingAsync(Organization organization, CancellationToken cancellationToken = default)
    {
        _manageOrganization.UpdateOrganizationUsePrimaryPropertySetting(organization);
        return Task.CompletedTask;
    }

    public Task UpdateOrganizationThirdPartyIDPAsync(Organization organization, CancellationToken cancellationToken = default)
    {
        _manageOrganization.UpdateOrganizationThirdPartyIDP(organization);
        return Task.CompletedTask;
    }

    public Task<Guid> GetOrganizationAdminUserRealPageIdAsync(Guid organizationRealPageId, CancellationToken cancellationToken = default)
        => Task.FromResult(_manageOrganization.GetOrganizationAdminUserRealPageId(organizationRealPageId));

    public Task<IList<IdentityProviderType>> GetOrganizationIdentityProviderTypeAsync(Guid realPageId, CancellationToken cancellationToken = default)
        => Task.FromResult(_manageOrganization.GetOrganizationIdentityProviderType(realPageId));

    public Task<bool> AddCompanyToJobAsync(string companyInstanceSourceId, long createdBy, long createUserPersonaId, int organizationIsActive, CancellationToken cancellationToken = default)
        => Task.FromResult(_manageOrganization.AddCompanyToJob(companyInstanceSourceId, createdBy, createUserPersonaId, organizationIsActive));

    public Task<bool> AddUpdateCompanyToUnifiedSettingsAsync(string companyInstanceID, string transactionType, string customerEnvironment = null, CancellationToken cancellationToken = default)
        => Task.FromResult(_manageOrganization.AddUpdateCompanyToUnifiedSettings(companyInstanceID, transactionType, customerEnvironment));

    public Task DeleteQueuedOrganizationsAsync(CancellationToken cancellationToken = default)
    {
        _manageOrganization.DeleteQueuedOrganizations();
        return Task.CompletedTask;
    }

    public Task<OrganizationRemovalQueue> InsertOrganizationRemovalQueueAsync(OrganizationRemovalQueue organizationRemovalQueue, CancellationToken cancellationToken = default)
        => Task.FromResult(_manageOrganization.InsertOrganizationRemovalQueue(organizationRemovalQueue));

    // ── Organization types / domains ───────────────────────────────────────

    public Task<List<OrganizationType>> ListOrganizationTypeAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(_manageOrganization.ListOrganizationType());

    public Task<List<OrganizationDomain>> ListOrganizationDomainAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(_manageOrganization.ListOrganizationDomain());

    public Task<RepositoryResponse> CreateOrganizationDomainAsync(OrganizationDomain organizationDomain, CancellationToken cancellationToken = default)
        => Task.FromResult(_manageOrganization.CreateOrganizationDomain(organizationDomain));

    // ── Product parsing ────────────────────────────────────────────────────

    public Task<List<string>> ParseProductAsync(List<string> productCode, List<int> addProductList, CancellationToken cancellationToken = default)
        => Task.FromResult(_manageOrganization.ParseProduct(productCode, addProductList));

    public Task EnableProductOnOtherProductsActivationAsync(List<int> addProductList, CancellationToken cancellationToken = default)
    {
        _manageOrganization.EnableProductOnOtherProductsActivation(addProductList);
        return Task.CompletedTask;
    }

    public Task<RepositoryResponse> UpdateUsePrimaryPropertyForOrganizationProductAsync(long organizationPartyId, int productId, bool usePrimaryProperty, CancellationToken cancellationToken = default)
        => Task.FromResult(_manageOrganization.UpdateUsePrimaryPropertyForOrganizationProduct(organizationPartyId, productId, usePrimaryProperty));

    // ── Company setup ──────────────────────────────────────────────────────

    public Task<List<CompanySetup>> GetCompanyListAsync(string organizationName, int domain, int? blueId, int organizationId, IDictionary<object, object> globals, CancellationToken cancellationToken = default)
        => Task.FromResult(_manageOrganization.GetCompanyList(organizationName, domain, blueId, organizationId, globals));

    public Task<CompanyMaster> SearchCompanyDetailsByCustomerCompanyIdAsync(long customerCompanyId, CancellationToken cancellationToken = default)
        => Task.FromResult(_manageOrganization.SearchCompanyDetailsByCustomerCompanyId(customerCompanyId));

    // ── Property management ────────────────────────────────────────────────

    public Task<List<CompanyPropertySetup>> GetPropertiesForCompanyAsync(Guid companyInstanceId, string propertyName = null, string domain = null, int? blueId = null, int? status = null, IDictionary<object, object> globals = null, long editorPersonaId = 0, long userPersonaId = 0, bool? isSelectedProperties = null, List<Guid> selectedProperties = null, string operatorCode = null, string operatorValue = null, CancellationToken cancellationToken = default)
        => Task.FromResult(_manageOrganization.GetPropertiesForCompany(companyInstanceId, propertyName, domain, blueId, status, globals, editorPersonaId, userPersonaId, isSelectedProperties, selectedProperties, operatorCode, operatorValue));

    public Task<List<UPFMPropertyInstance>> GetPropertiesByInstanceIdAsync(List<Guid> propertyInstanceIds, CancellationToken cancellationToken = default)
        => Task.FromResult(_manageOrganization.GetPropertiesByInstanceId(propertyInstanceIds));

    public Task<List<UPFMPropertyInstance>> GetPropertiesByInstanceIdAsync(List<Guid> propertyInstanceIds, DefaultUserClaim userClaim, CancellationToken cancellationToken = default)
        => Task.FromResult(new ManageOrganization(userClaim).GetPropertiesByInstanceId(propertyInstanceIds));

    public async Task<RepositoryResponse> UpdatePropertyListAsync(List<UPFMPropertyInstance> propertyList, Guid companyInstanceId, CancellationToken cancellationToken = default)
        => await _manageOrganization.UpdatePropertyList(propertyList, companyInstanceId).ConfigureAwait(false);

    public async Task<RepositoryResponse> UpdatePropertyListAsync(List<UPFMPropertyInstance> propertyList, Guid companyInstanceId, DefaultUserClaim userClaim, CancellationToken cancellationToken = default)
        => await new ManageOrganization(userClaim).UpdatePropertyList(propertyList, companyInstanceId).ConfigureAwait(false);

    public async Task<RepositoryResponse> UpdateCompanyInstanceAsync(long companyBatchJobId, int statusTypeId, string errorMessage, CancellationToken cancellationToken = default)
        => await _manageOrganization.UpdateCompanyInstance(companyBatchJobId, statusTypeId, errorMessage).ConfigureAwait(false);

    public async Task<IRepositoryResponse> ProcessPropertyListAsync(UPFMPropertyInstance property, Guid companyInstanceId, DefaultUserClaim userClaim, CancellationToken cancellationToken = default)
        => await new ManageOrganization(userClaim).ProcessPropertyList(property, companyInstanceId).ConfigureAwait(false);

    public Task<bool> UpdatePropertyInSettingsAndActivityLogsAsync(UPFMPropertyInstance property, Guid companyInstanceId, List<UPFMPropertyInstance> oldPropertyList, DefaultUserClaim userClaim, CancellationToken cancellationToken = default)
        => Task.FromResult(new ManageOrganization(userClaim).UpdatePropertyInSettingsAndActivityLogs(property, companyInstanceId, oldPropertyList));

    public Task<RepositoryResponse> AddPropertyForOrganizationAsync(UPFMPropertyInstance property, Guid companyInstanceID, CancellationToken cancellationToken = default)
        => Task.FromResult(_manageOrganization.AddPropertyForOrganization(property, companyInstanceID));

    public Task<RepositoryResponse> DeletePropertyForOrganizationAsync(Guid propertyInstanceID, Guid companyInstanceID, CancellationToken cancellationToken = default)
        => Task.FromResult(_manageOrganization.DeletePropertyForOrganization(propertyInstanceID, companyInstanceID));

    public Task<List<UPFMPropertyInstance>> GetPropertyByInstanceIdAsync(Guid propertyInstanceId, CancellationToken cancellationToken = default)
        => Task.FromResult(_manageOrganization.GetPropertyByInstanceId(propertyInstanceId));

    public Task<RepositoryResponse> UpdatePropertyAsync(UPFMPropertyInstance property, Guid companyInstanceId, CancellationToken cancellationToken = default)
        => Task.FromResult(_manageOrganization.UpdateProperty(property, companyInstanceId));

    public Task<PropertyInstanceSearch> SearchPropertyDetailsByCustomerPropertyIdAsync(string customerPropertyId, string booksCustomerMasterId, CancellationToken cancellationToken = default)
        => Task.FromResult(_manageOrganization.SearchPropertyDetailsByCustomerPropertyId(customerPropertyId, booksCustomerMasterId));

    public Task<ProductPropertyDetails> GetSourceProductDetailsAsync(string propertyInstanceSourceId, string source, CancellationToken cancellationToken = default)
        => Task.FromResult(_manageOrganization.GetSourceProductDetails(propertyInstanceSourceId, source));

    public Task<List<PropertyAudit>> AuditCompanyProductPropertiesToUPFMAsync(Guid companyInstanceId, int productId, DefaultUserClaim userClaim, CancellationToken cancellationToken = default)
        => Task.FromResult(new ManageOrganization(userClaim).AuditCompanyProductPropertiesToUPFM(companyInstanceId, productId));
}
