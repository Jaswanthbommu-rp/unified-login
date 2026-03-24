using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.BlackBook;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product;
using UnifiedLogin.SharedObjects.Product.UnifiedLogin;
using Company = UnifiedLogin.SharedObjects.BlackBook.Company;

namespace UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;

/// <summary>
/// Async-first interface for all BlueBook / UDM API operations.
/// Replaces: sync <see cref="IManageBlueBook"/> + blocking <c>.Result</c> calls.
/// </summary>
public interface IManageBlueBookAsync
{
    // ── Company map ────────────────────────────────────────────────────────
    Task<IList<CustomerCompanyMap>> GetCompanyMapAsync(Guid companyRealPageId, long booksCompanyMasterId, string domain, CancellationToken cancellationToken = default);

    Task<IList<CustomerCompanyMap>> GetCompanyMapAsync(Guid companyRealPageId, long booksCompanyMasterId, string source, string domain, string includeExtra = "", bool includeGreenBookCares = true, bool useTranslate = true, CancellationToken cancellationToken = default);

    Task<IList<CustomerCompanyMap>> GetProductCompanyMappingAsync(Guid companyRealPageId, string source, CancellationToken cancellationToken = default);

    Task<CustomerCompanyMap> GetCompanyInstanceBySourceAndInstanceIdAsync(string instanceId, string productSource, CancellationToken cancellationToken = default);

    // ── Property translation ───────────────────────────────────────────────
    Task<TranslatePropertyInstance> GetTranslatePropertiesFromUPFMToProductv3Async(UPFMProperty upfmProperties, string productSource, CancellationToken cancellationToken = default);

    Task<TranslatePropertyInstance> GetTranslatePropertiesFromProductToUPFMAsync(UPFMProperty properties, string productSource, CancellationToken cancellationToken = default);

    Task<ListResponse> TranslateProductPrimaryPropertiesDataAsync(UPFMProperty upfmProperty, int productId, ListResponse productResult, CancellationToken cancellationToken = default);

    // ── Property instances ─────────────────────────────────────────────────
    Task<IList<PropertyInstance>> GetPropertyInstanceAsync(long companyInstanceId, CancellationToken cancellationToken = default);

    Task<CompanyPropertyRootObject> GetCompanyPropertyInstanceAsync(long companyInstanceId, CancellationToken cancellationToken = default);

    Task<List<Guid>> GetUPFMPropertyInstancesAsync(string companyRealPageId, CancellationToken cancellationToken = default);

    Task<List<Guid>> GetPropertiesPerProductCenterAsync(string companyRealPageId, int productId, CancellationToken cancellationToken = default);

    Task<List<Guid>> GetProductPropertyInstancesAsync(int companyInstanceSourceId, string source, CancellationToken cancellationToken = default);

    Task<List<BooksPropertyInstance>> GetPropertyInstanceForCompanyAsync(Guid companyRealPageId, CancellationToken cancellationToken = default);

    Task<List<BooksPropertyInstance>> GetPropertyInstanceForCompanyByOperatorIdAsync(Guid companyRealPageId, Guid operatorRealPageId, CancellationToken cancellationToken = default);

    Task<List<BooksPropertyInstance>> GetUPFMPropertyInstancesByCustomerPropertyIdAsync(string customerPropertyId, CancellationToken cancellationToken = default);

    Task<List<BooksPropertyInstance>> GetAllProductsPropertyInstanceFromBooksAsync(string customerPropertyId, CancellationToken cancellationToken = default);

    Task<BooksPropertyInstance> GetPropertyDetailsByPropertyInstanceIdAndSourceAsync(string propertyInstanceSourceId, string source, CancellationToken cancellationToken = default);

    // ── Company operations ─────────────────────────────────────────────────
    Task<bool> AddUPFMCompanyFromProvisioningEventAsync(CompanyInstance companyInstance, CancellationToken cancellationToken = default);

    Task<bool> AddUPFMCompanyFromCompanySetupAsync(CompanyInstanceAdd companyInstance, CancellationToken cancellationToken = default);

    Task<bool> DeleteBooksGreenBookCompanyInstanceAsync(CompanyInstance companyInstance, CancellationToken cancellationToken = default);

    Task<string> UpdateBooksGreenBookCompanyInstanceAsync(CompanyInstance companyInstance, CompanyLocation oldCompanyLocation, CancellationToken cancellationToken = default);

    Task<IList<Company>> GetCompanyListByCompIdsAsync(List<UnifiedLoginCompany> booksCompanyMasterList, CancellationToken cancellationToken = default);

    Task<IList<CustomerCompanyInstance>> GetUPFMCompanyDetailsByInstanceIdsAsync(List<string> companyInstanceIds, CancellationToken cancellationToken = default);

    Task<Company> GetBooksCompanyDetailsByCompanyMasterIdAsync(long companyMasterId, CancellationToken cancellationToken = default);

    Task<List<CustomerCompanyInstance>> GetCompanyInstancesByCustomerCompanyIdAsync(long customerCompanyId, CancellationToken cancellationToken = default);

    Task<List<CustomerCompanyDomain>> GetListOfDomainsByCompanyAsync(long companyMasterId, CancellationToken cancellationToken = default);

    Task<CustomerCompany> GetCompanyCustomerInfoAsync(Guid companyRealPageId, string domain, long booksCompanyMasterId, CancellationToken cancellationToken = default);

    Task<IList<CustomerCompanyPropertyMap>> GetVCompanyPropertyMapAsync(long booksCompanyMasterId, string filter, CancellationToken cancellationToken = default);

    Task<BooksCompanyInstance> GetCompanyInstanceByUPFMCompanyIdAsync(string upfmCompanyId, CancellationToken cancellationToken = default);

    Task<List<CustomerCompanyMap>> GetCustomerCompanyMapByCustomerCompanyIdAsync(int customerCompanyId, string companyDomain, CancellationToken cancellationToken = default);

    // ── Customer property ──────────────────────────────────────────────────
    Task<IList<ProductProperty>> GetCustomerPropertyAsync(long booksCompanyMasterId = 0, string include = null, string filter = null, bool getCached = true, CancellationToken cancellationToken = default);

    Task<CustomerProperty> GetCustomerPropertyDetailsAsync(string propertyInstanceId, CancellationToken cancellationToken = default);

    // ── Provisioning events ────────────────────────────────────────────────
    Task<bool> AddBooksGreenBookPropertyInstanceFromProvisioningAsync(PropertyInstance propertyInstance, CancellationToken cancellationToken = default);

    Task<bool> DeletePropertyFromBooksAsync(Guid propertyInstance, CancellationToken cancellationToken = default);

    Task<bool> AcknowledgeProvisioningEventAsync(ProductCenterEnablement productCenterEnablement, CancellationToken cancellationToken = default);

    Task<bool> ProductCenterEnableAsync(SystemProductCenter systemProductCenter, CancellationToken cancellationToken = default);

    Task<bool> ProductCenterDisableAsync(SystemProductCenter systemProductCenter, CancellationToken cancellationToken = default);

    Task<bool> AcknowledgeProvisioningCancelEventAsync(ProductCenterCancellation productCenterCancellation, CancellationToken cancellationToken = default);

    Task<bool> AcknowledgePropertyUpdateAsync(PropertyInstanceAck propertyInstanceAck, CancellationToken cancellationToken = default);

    Task<bool> AcknowledgeBulkPropertyListUpdateAsync(BulkPropertyInstanceStatusAck propertyInstanceAck, CancellationToken cancellationToken = default);

    // ── Sources / operators ────────────────────────────────────────────────
    Task<IEnumerable<UDMSource>> GetUDMSourceListAsync(CancellationToken cancellationToken = default);

    Task<IEnumerable<UDMOperators>> GetAllOperatorDetailsForUPFMCompanyAsync(Guid companyRealPageId, string source, CancellationToken cancellationToken = default);

    Task<IEnumerable<UPFMOperators>> GetOperatorListForUPFMCompanyAsync(Guid companyRealPageId, string source, CancellationToken cancellationToken = default);
}