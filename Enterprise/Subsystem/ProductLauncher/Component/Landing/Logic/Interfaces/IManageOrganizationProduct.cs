using System;
using System.Collections.Generic;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces
{
	/// <summary>
	/// Manage Organization Product Interface
	/// </summary>
	public interface IManageOrganizationProduct
	{
		/// <summary>
		/// Used to add a list of products to the given company 
		/// </summary>
		/// <param name="org"></param>
		/// <param name="productList"></param>
		/// <returns></returns>
		IRepositoryResponse InsertUpdateOrganizationProduct(Organization org, List<int> productList);

		/// <summary>
		/// Used to delete a product from an Organization
		/// </summary>
		/// <param name="partyId"></param>
		/// <param name="product"></param>
		/// <param name="org"></param>
		/// <returns></returns>
		RepositoryResponse DeleteOrganizationProduct(long partyId, int product, Organization org, bool logActivity = true);

        /// <summary>
        /// Delete list of products from organization
        /// </summary>
        /// <param name="unassignProductList"></param>
        /// <param name="org"></param>
        /// <returns></returns>
        RepositoryResponse DeleteProductsFromOrganization(List<int> unassignProductList, Organization org);

        /// <summary>
        /// Used to insert a new product to an Organization
        /// </summary>
        /// <param name="partyId"></param>
        /// <param name="product"></param>
        /// <param name="configurationId"></param>
        /// <param name="fromDate"></param>
        /// <param name="thruDate"></param>
        /// <param name="orgName"></param>
        /// <returns></returns>
        RepositoryResponse InsertUpdateOrganizationProduct(long partyId, int product, int? configurationId, DateTime? fromDate, DateTime? thruDate, string orgName);

		/// <summary>
		/// Used to insert a new product to an Organization from provisioning
		/// </summary>
		/// <param name="partyId"></param>
		/// <param name="product"></param>
		/// <param name="configurationId"></param>
		/// <param name="fromDate"></param>
		/// <param name="thruDate"></param>
		/// <param name="org"></param>
		/// <returns></returns>
		IRepositoryResponse InsertUpdateOrganizationProductFromProvisioning(int product, int? configurationId, DateTime? fromDate, DateTime? thruDate, Organization org);

		/// <summary>
		/// Used to delete users for product for an Organization
		/// </summary>
		/// <param name="partyId">The organization id for the product</param>
		/// <param name="product">The product</param>
		/// <returns></returns>
		IRepositoryResponse DisableUsersForProduct(long partyId, ProductEnum product);

        /// <summary>
        /// Used to check whether shared productId enabled or not, if its enabled then not going to enable other product.
        /// </summary>
        /// <param name="orgEnabledproductList"></param>
        /// <param name="productList"></param>
        /// <returns></returns>
        IRepositoryResponse CheckSharedProductsEnabled(IList<ProductUI> orgEnabledproductList, List<int> productList);

    }
}