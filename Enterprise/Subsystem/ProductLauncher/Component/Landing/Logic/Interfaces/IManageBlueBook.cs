using System.Collections.Generic;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.BlackBook;
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
        /// <param name="booksCompanyMasterId">Master Company Id RPUP</param>
        /// <returns>List of CompanyMap</returns>
        IList<CustomerCompanyMap> GetCompanyMap(long booksCompanyMasterId);

        /// <summary>
        /// Used to get the information about the given company from the BlackBook system
        /// </summary>
        /// <param name="booksCompanyMasterId">Master Company Id</param>
        /// <param name="source">A filter on source if given</param>
        /// <param name="IncludeExtra">Extra Uri Includes (Optional)</param>
        /// <param name="includeGreenBookCares">Filter result using greenbook cares flag</param>
        /// <returns>List of CompanyMap</returns>
        IList<CustomerCompanyMap> GetCompanyMap(long booksCompanyMasterId, string source, string IncludeExtra = "", bool includeGreenBookCares = true);

        /// <summary>
        /// Get a list of property instances under the given company instance in the BlueBook system
        /// </summary>
        /// <param name="companyInstanceId">Ids of the company instances element</param>
        /// <returns>List of CompanyPropertyInstanceMap</returns>
        IList<CompanyPropertyInstanceMap> GetCompanyPropertyInstanceMap(long companyInstanceId);

        /// <summary>
        /// Used to get property information from the books
        /// </summary>
        /// <param name="companyInstanceId">Ids of the company instances element</param>
        /// <returns>List of PropertyInstance</returns>
        //IList<PropertyInstance> GetPropertyInstance(long companyInstanceId);

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
        /// <param name="booksCompanyMasterId">Master Company Id</param>
        /// <returns>Company object</returns>
        CustomerCompany GetCompanyCustomerInfo(long booksCompanyMasterId);

        /// <summary>
        /// Used to get property information from the books
        /// </summary>
        /// <param name="companyInstanceId">Ids of the company instances element</param>
        /// <returns>List of PropertyInstance</returns>
        CompanyPropertyRootObject GetCompanyPropertyInstance(long companyInstanceId);

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
    }
}