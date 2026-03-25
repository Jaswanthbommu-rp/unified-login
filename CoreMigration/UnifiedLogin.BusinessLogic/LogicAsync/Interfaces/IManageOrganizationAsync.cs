using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.BlackBook;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Maintenance;
using UnifiedLogin.SharedObjects.Product;

namespace UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;

/// <summary>
/// Async-first interface for organization management operations.
/// Replaces: sync <see cref="UnifiedLogin.BusinessLogic.Logic.Interfaces.IManageOrganization"/> + blocking <c>Task.Run</c> calls.
/// UserClaim overloads replace the <c>new ManageOrganization(userClaim)</c> anti-pattern inside controller actions.
/// </summary>
public interface IManageOrganizationAsync
{
    // ── Organization CRUD ──────────────────────────────────────────────────
    Task<Organization> GetOrganizationAsync(Guid realPageId, long? organizationPartyId = null, CancellationToken cancellationToken = default);
    Task<IList<Organization>> GetOrganizationListAsync(CancellationToken cancellationToken = default);
    Task<ObjectOutput<OrganizationCreateResult, IErrorData>> CreateOrganizationAsync(OrganizationCreate organization, List<int> addProductList, bool processBlueBookMessage = false, CancellationToken cancellationToken = default);
    Task<RepositoryResponse> UpdateOrganizationAsync(Organization organization, CancellationToken cancellationToken = default);
    Task UpdateOrganizationUsePrimaryPropertySettingAsync(Organization organization, CancellationToken cancellationToken = default);
    Task UpdateOrganizationThirdPartyIDPAsync(Organization organization, CancellationToken cancellationToken = default);
    Task<Guid> GetOrganizationAdminUserRealPageIdAsync(Guid organizationRealPageId, CancellationToken cancellationToken = default);
    Task<IList<IdentityProviderType>> GetOrganizationIdentityProviderTypeAsync(Guid realPageId, CancellationToken cancellationToken = default);
    Task<bool> AddCompanyToJobAsync(string companyInstanceSourceId, long createdBy, long createUserPersonaId, int organizationIsActive, CancellationToken cancellationToken = default);
    Task<bool> AddUpdateCompanyToUnifiedSettingsAsync(string companyInstanceID, string transactionType, string customerEnvironment = null, CancellationToken cancellationToken = default);
    Task DeleteQueuedOrganizationsAsync(CancellationToken cancellationToken = default);
    Task<OrganizationRemovalQueue> InsertOrganizationRemovalQueueAsync(OrganizationRemovalQueue organizationRemovalQueue, CancellationToken cancellationToken = default);

    // ── Organization types / domains ───────────────────────────────────────
    Task<List<OrganizationType>> ListOrganizationTypeAsync(CancellationToken cancellationToken = default);
    Task<List<OrganizationDomain>> ListOrganizationDomainAsync(CancellationToken cancellationToken = default);
    Task<RepositoryResponse> CreateOrganizationDomainAsync(OrganizationDomain organizationDomain, CancellationToken cancellationToken = default);

    // ── Product parsing ────────────────────────────────────────────────────
    Task<List<string>> ParseProductAsync(List<string> productCode, List<int> addProductList, CancellationToken cancellationToken = default);
    Task EnableProductOnOtherProductsActivationAsync(List<int> addProductList, CancellationToken cancellationToken = default);
    Task<RepositoryResponse> UpdateUsePrimaryPropertyForOrganizationProductAsync(long organizationPartyId, int productId, bool usePrimaryProperty, CancellationToken cancellationToken = default);

    // ── Company setup ──────────────────────────────────────────────────────
    Task<List<CompanySetup>> GetCompanyListAsync(string organizationName, int domain, int? blueId, int organizationId, IDictionary<object, object> globals, CancellationToken cancellationToken = default);
    Task<CompanyMaster> SearchCompanyDetailsByCustomerCompanyIdAsync(long customerCompanyId, CancellationToken cancellationToken = default);

    // ── Property management ────────────────────────────────────────────────
    Task<List<CompanyPropertySetup>> GetPropertiesForCompanyAsync(Guid companyInstanceId, string propertyName = null, string domain = null, int? blueId = null, int? status = null, IDictionary<object, object> globals = null, long editorPersonaId = 0, long userPersonaId = 0, bool? isSelectedProperties = null, List<Guid> selectedProperties = null, string operatorCode = null, string operatorValue = null, CancellationToken cancellationToken = default);
    Task<List<UPFMPropertyInstance>> GetPropertiesByInstanceIdAsync(List<Guid> propertyInstanceIds, CancellationToken cancellationToken = default);

    /// <summary>Overload using a per-request <see cref="DefaultUserClaim"/> context — replaces <c>new ManageOrganization(userClaim).GetPropertiesByInstanceId(...)</c>.</summary>
    Task<List<UPFMPropertyInstance>> GetPropertiesByInstanceIdAsync(List<Guid> propertyInstanceIds, DefaultUserClaim userClaim, CancellationToken cancellationToken = default);

    Task<RepositoryResponse> UpdatePropertyListAsync(List<UPFMPropertyInstance> propertyList, Guid companyInstanceId, CancellationToken cancellationToken = default);

    /// <summary>Overload using a per-request <see cref="DefaultUserClaim"/> context — replaces <c>new ManageOrganization(userClaim).UpdatePropertyList(...)</c>.</summary>
    Task<RepositoryResponse> UpdatePropertyListAsync(List<UPFMPropertyInstance> propertyList, Guid companyInstanceId, DefaultUserClaim userClaim, CancellationToken cancellationToken = default);

    Task<RepositoryResponse> UpdateCompanyInstanceAsync(long companyBatchJobId, int statusTypeId, string errorMessage, CancellationToken cancellationToken = default);

    /// <summary>Replaces <c>new ManageOrganization(userClaim).ProcessPropertyList(...)</c>.</summary>
    Task<IRepositoryResponse> ProcessPropertyListAsync(UPFMPropertyInstance property, Guid companyInstanceId, DefaultUserClaim userClaim, CancellationToken cancellationToken = default);

    /// <summary>Replaces <c>new ManageOrganization(userClaim).UpdatePropertyInSettingsAndActivityLogs(...)</c>.</summary>
    Task<bool> UpdatePropertyInSettingsAndActivityLogsAsync(UPFMPropertyInstance property, Guid companyInstanceId, List<UPFMPropertyInstance> oldPropertyList, DefaultUserClaim userClaim, CancellationToken cancellationToken = default);

    Task<RepositoryResponse> AddPropertyForOrganizationAsync(UPFMPropertyInstance property, Guid companyInstanceID, CancellationToken cancellationToken = default);
    Task<RepositoryResponse> DeletePropertyForOrganizationAsync(Guid propertyInstanceID, Guid companyInstanceID, CancellationToken cancellationToken = default);
    Task<List<UPFMPropertyInstance>> GetPropertyByInstanceIdAsync(Guid propertyInstanceId, CancellationToken cancellationToken = default);
    Task<RepositoryResponse> UpdatePropertyAsync(UPFMPropertyInstance property, Guid companyInstanceId, CancellationToken cancellationToken = default);
    Task<PropertyInstanceSearch> SearchPropertyDetailsByCustomerPropertyIdAsync(string customerPropertyId, string booksCustomerMasterId, CancellationToken cancellationToken = default);
    Task<ProductPropertyDetails> GetSourceProductDetailsAsync(string propertyInstanceSourceId, string source, CancellationToken cancellationToken = default);

    /// <summary>Replaces <c>new ManageOrganization(userClaim).AuditCompanyProductPropertiesToUPFM(...)</c>.</summary>
    Task<List<PropertyAudit>> AuditCompanyProductPropertiesToUPFMAsync(Guid companyInstanceId, int productId, DefaultUserClaim userClaim, CancellationToken cancellationToken = default);
}
