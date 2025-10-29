using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnifiedLogin.SharedObjects.BlackBook;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product;
using UnifiedLogin.SharedObjects.Product.UnifiedLogin;

namespace UnifiedLogin.BusinessLogic.Logic.Interfaces
{
    /// <summary>
    /// Interface for Manage BlueBook APIs
    /// </summary>
    public interface IManageBlueBook
    {
        /// <summary>
        /// Dispose
        /// </summary>
        void Dispose();

        /// <summary>
        /// Filter Company Map
        /// </summary>
        /// <param name="companyRealPageId">The guid for the company</param>
        /// <param name="booksCompanyMasterId">Master Company Id - RPUP</param>
        /// <param name="domain">The domain to query for the company</param>
        /// <returns>List of CompanyMapResource</returns>
        IList<CustomerCompanyMap> GetCompanyMap(Guid companyRealPageId, long booksCompanyMasterId, string domain);

        /// <summary>
        /// Used to get the information about the given company from the BlackBook system
        /// </summary>
        /// <param name="companyRealPageId">The guid for the company</param>
        /// <param name="booksCompanyMasterId">Master Company Id</param>
        /// <param name="source">A filter on source if given</param>
        /// <param name="domain">The domain to query for the company</param>
        /// <param name="includeExtra">Extra Uri Includes (Optional)</param>
        /// <param name="includeGreenBookCares">Filter result using greenbook cares flag</param>
        /// <param name="useTranslate"></param>
        /// <returns>List of CompanyMapResource</returns>
        IList<CustomerCompanyMap> GetCompanyMap(Guid companyRealPageId, long booksCompanyMasterId, string source, string domain, string includeExtra = "", bool includeGreenBookCares = true, bool useTranslate = true);

        /// <summary>
        /// used to check product is mapped or not
        /// </summary>
        /// <param name="companyRealPageId"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        IList<CustomerCompanyMap> GetProductCompanyMapping(Guid companyRealPageId, string source);
        /// <summary>
        /// Get a list of property instances under the given company instance in the BlueBook system
        /// </summary>
        /// <param name="companyInstanceId">Ids of the company instances element</param>
        /// <returns>List of CompanyPropertyInstanceMap</returns>
        //IList<CompanyPropertyInstanceMap> GetCompanyPropertyInstanceMap(long companyInstanceId);

        /// <summary>
        /// Used to get property information from the books
        /// </summary>
        /// <param name="companyInstanceId">Ids of the company instances element</param>
        /// <returns>List of PropertyInstance</returns>
        IList<PropertyInstance> GetPropertyInstance(long companyInstanceId);

        /// <summary>
        /// Get a list of property master records under the given company id in the BlueBook system
        /// </summary>
        /// <param name="booksCompanyMasterId">Master Company Id RPUP</param>
        /// <param name="filter">filter</param>
        /// <returns>List of VCompanyPropertyMap</returns>
        IList<CustomerCompanyPropertyMap> GetVCompanyPropertyMap(long booksCompanyMasterId, string filter);

        /// <summary>
        /// Used to get the information about the company for RPUP
        /// </summary>
        /// <param name="companyRealPageId"></param>
        /// <param name="domain"></param>
        /// <param name="booksCompanyMasterId"></param>
        /// <returns></returns>
        CustomerCompany GetCompanyCustomerInfo(Guid companyRealPageId, string domain, long booksCompanyMasterId);

        /// <summary>
        /// Used to get property information from the books
        /// </summary>
        /// <param name="companyInstanceId">Ids of the company instances element</param>
        /// <returns>List of PropertyInstance</returns>
        CompanyPropertyRootObject GetCompanyPropertyInstance(long companyInstanceId);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="companyRealPageId"></param>
        /// <returns></returns>
        List<Guid> GetUPFMPropertyInstances(string companyRealPageId);

        /// <summary>
        /// Used to get the information about the list of companies by companyIds from the BlueBook system
        /// </summary>
        /// <param name="booksCompanyMasterList"></param>        
        /// <returns></returns>
        IList<Company> GetCompanyListByCompIds(List<UnifiedLoginCompany> booksCompanyMasterList);

        /// <summary>
        /// GetUPFMCompanyDetailsByInstanceIds
        /// </summary>
        /// <param name="companyInstanceIds"></param>
        /// <returns></returns>
        IList<CustomerCompanyInstance> GetUPFMCompanyDetailsByInstanceIds(List<string> companyInstanceIds);

        IList<ProductProperty> GetCustomerProperty(long booksCompanyMasterId = 0, string include = null, string filter = null, bool getCached = true);

        /// <summary>
        /// Used to add a new company instance
        /// </summary>
        /// <param name="companyInstance"></param>
        /// <returns></returns>
        bool AddUPFMCompanyFromProvisioningEvent(CompanyInstance companyInstance);

        /// <summary>
        /// Add a UPFM company to UDM from the Add Company page in Unified Login
        /// </summary>
        /// <param name="companyInstance"></param>
        /// <returns></returns>
        bool AddUPFMCompanyFromCompanySetup(CompanyInstanceAdd companyInstance);

        /// <summary>
        /// Used to delete an existing company instance
        /// </summary>
        /// <param name="companyInstance"></param>
        /// <returns></returns>
        bool DeleteBooksGreenBookCompanyInstance(CompanyInstance companyInstance);

        /// <summary>
        /// Used to delete an existing property instance
        /// </summary>
        /// <param name="propertyInstance"></param>
        /// <returns></returns>
        bool DeletePropertyFromBooks(Guid propertyInstance);

        /// <summary>
        /// Used to update an existing company instance
        /// </summary>
        /// <param name="companyInstance"></param>
        /// <param name="oldAddress"></param>
        /// <returns></returns>
        string UpdateBooksGreenBookCompanyInstance(CompanyInstance companyInstance, CompanyLocation oldAddress);
       
        TranslatePropertyInstance GetTranslatePropertiesFromUPFMToProductv3(UPFMProperty upfmProperties, string productSource);

        /// <summary>
        /// Get all UPFM instances related to the given Product instance source. Filters domain automatically
        /// </summary>
        /// <param name="properties">List of product properties</param>
        /// <param name="productSource">productSource</param>
        /// <returns></returns>
        TranslatePropertyInstance GetTranslatePropertiesFromProductToUPFM(UPFMProperty properties, string productSource);

        /// <summary>
        /// Get properties per product center
        /// </summary>
        /// <param name="companyRealPageId"></param>
        /// <param name="productId"></param>
        /// <returns></returns>
        List<Guid> GetPropertiesPerProductCenter(string companyRealPageId, int productId);

        /// <summary>
        /// Get customer property details
        /// </summary>
        /// <param name="propertyInstanceId"></param>
        /// <returns></returns>
        CustomerProperty GetCustomerPropertyDetails(string propertyInstanceId);

        /// <summary>
        /// Add the new UPFM property instance to books
        /// </summary>
        /// <param name="propertyInstance"></param>
        /// <returns></returns>
        bool AddBooksGreenBookPropertyInstanceFromProvisioning(PropertyInstance propertyInstance);

        /// <summary>
        /// Used to acknowledge provisioning events
        /// </summary>
        /// <param name="productCenterEnablement"></param>
        /// <returns></returns>
        bool AcknowledgeProvisioningEvent(ProductCenterEnablement productCenterEnablement);
        
        /// <summary>
        /// Used to get books company details by companyMasterId
        /// </summary>
        /// <param name="companyMasterId"></param>
        /// <returns></returns>
        Company GetBooksCompanyDetailsByCompanyMasterId(long companyMasterId);

        /// <summary>
        /// Used to get company instances by customerCompanyId
        /// </summary>
        /// <param name="customerCompanyId"></param>
        /// <returns></returns>
        List<CustomerCompanyInstance> GetCompanyInstancesByCustomerCompanyId(long customerCompanyId);

        /// <summary>
        /// Used to get a list of domains by companyMasterId
        /// </summary>
        /// <param name="companyMasterId"></param>
        /// <returns></returns>
        List<CustomerCompanyDomain> GetListOfDomainsByCompany(long companyMasterId);

        /// <summary>
        /// Used to acknowledge provisioning Cancel events
        /// </summary>
        /// <param name="productCenterCancel"></param>
        /// <returns></returns>
        bool AcknowledgeProvisioningCancelEvent(ProductCenterCancellation productCenterCancel);

        /// <summary>
        /// Used to acknowledge on property name update
        /// </summary>
        /// <param name="propertyInstanceAck"></param>
        /// <returns></returns>
        bool AcknowledgePropertyUpdate(PropertyInstanceAck propertyInstanceAck);


        /// <summary>
        /// Used to acknowledge on property status update.
        /// </summary>
        /// <param name="propertyInstanceAck"></param>
        /// <returns></returns>
        Task<bool> AcknowledgeBulkPropertyListUpdate(BulkPropertyInstanceStatusAck propertyInstanceAck);

        /// <summary>
        /// GetPropertyInstanceForCompany
        /// </summary>
        /// <param name="companyRealPageId"></param>
        /// <returns></returns>
        List<BooksPropertyInstance> GetPropertyInstanceForCompany(Guid companyRealPageId);

        /// <summary>
        /// Used to get the Properties of the company, using the books customer master id or the UPFM id
        /// </summary>
        /// <param name="companyRealPageId"></param>
        /// <param name="operatorRealPageId"></param>
        /// <returns></returns>
        List<BooksPropertyInstance> GetPropertyInstanceForCompanyByOperatorId(Guid companyRealPageId, Guid operatorRealPageId);

        ///// <summary>
        ///// Get Property Details By CustomerPropertyId(blue book Id)
        ///// </summary>
        ///// <param name="customerPropertyId"></param>
        ///// <returns></returns>
        List<BooksPropertyInstance> GetUPFMPropertyInstancesByCustomerPropertyId(string CustomerPropertyId);

        /// <summary>
        /// GetAllProductsPropertyInstanceFromBooks
        /// </summary>
        /// <param name="customerPropertyId"></param>
        /// <returns></returns>
        List<BooksPropertyInstance> GetAllProductsPropertyInstanceFromBooks(string customerPropertyId);

        /// <summary>
        /// Get source product details from books
        /// </summary>
        /// <param name="propertyInstanceSourceId">propertyInstanceSourceId</param>
        /// <param name="source">source</param>
        /// <returns></returns>
        BooksPropertyInstance GetPropertyDetailsByPropertyInstanceIdAndSource(string propertyInstanceSourceId, string source);

        /// <summary>
        /// Get CustomerCompanyMap By CustomerCompanyId
        /// </summary>
        /// <param name="customerCompanyId">customerCompanyId</param>
        /// <param name="companyDomain">companyDomain</param>
        /// <returns></returns>
        List<CustomerCompanyMap> GetCustomerCompanyMapByCustomerCompanyId(int customerCompanyId, string companyDomain);

        /// <summary>
        /// Get CompanyInstance By UPFMCompanyId
        /// </summary>
        /// <param name="upfmCompanyId">upfmCompanyId</param>
        /// <returns></returns>
        BooksCompanyInstance GetCompanyInstanceByUPFMCompanyId(string upfmCompanyId);

        ///// <summary>
        ///// Get translated product primary properties data
        ///// </summary>
        ///// <param name="upfmProperty"></param>
        ///// <returns></returns>
        ListResponse TranslateProductPrimaryPropertiesData(UPFMProperty upfmProperty, int productId, ListResponse productResult);

        /// <summary>
        /// Used to enable product for an organization
        /// </summary>
        /// <param name="systemProductCenter"></param>
        /// <returns></returns>
        bool ProductCenterEnable(SystemProductCenter systemProductCenter);

        /// <summary>
        /// Used to delete product for an organization
        /// </summary>
        /// <param name="systemProductCenter"></param>
        /// <returns></returns>
        bool ProductCenterDisable(SystemProductCenter systemProductCenter);

        /// <summary>
        /// Used to get a specific product instance by source and source instance id
        /// </summary>
        /// <param name="instanceId"></param>
        /// <param name="productSource"></param>
        /// <returns></returns>
        CustomerCompanyMap GetCompanyInstanceBySourceAndInstanceId(string instanceId, string productSource);

        /// <summary>
        /// Used to get a list of UDM Sources
        /// </summary>
        /// <returns></returns>
        IEnumerable<UDMSource> GetUDMSourceList();

        IEnumerable<UDMOperators> GetAllOperatorDetailsForUPFMCompany(Guid companyRealPageId, string source);
        IEnumerable<UPFMOperators> GetOperatorListForUPFMCompany(Guid companyRealPageId, string source);
    }
}