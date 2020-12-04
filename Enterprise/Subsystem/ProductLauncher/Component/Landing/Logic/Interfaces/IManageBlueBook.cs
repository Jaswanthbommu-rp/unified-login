using System;
using System.Collections.Generic;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.BlackBook;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.UnifiedLogin;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces
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

        IList<ProductProperty> GetCustomerProperty(long booksCompanyMasterId = 0, string include = null, string filter = null);

        /// <summary>
        /// Used to add a new company instance
        /// </summary>
        /// <param name="companyInstance"></param>
        /// <returns></returns>
        bool AddBooksGreenBookCompanyInstance(CompanyInstance companyInstance);

        /// <summary>
        /// Used to delete an existing company instance
        /// </summary>
        /// <param name="companyInstance"></param>
        /// <returns></returns>
        bool DeleteBooksGreenBookCompanyInstance(CompanyInstance companyInstance);

        /// <summary>
        /// Used to update an existing company instance
        /// </summary>
        /// <param name="companyInstance"></param>
        /// <returns></returns>
        string UpdateBooksGreenBookCompanyInstance(CompanyInstance companyInstance);
       
        TranslatePropertyInstance GetTranslatePropertiesFromUPFMToProductv3(UPFMProperty upfmProperties, string productSource);
        List<Guid> GetPropertiesPerProductCenter(string companyRealPageId, ProductEnum product);

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
    }
}