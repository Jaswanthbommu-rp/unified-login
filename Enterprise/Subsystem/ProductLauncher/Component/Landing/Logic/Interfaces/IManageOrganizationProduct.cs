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
		IRepositoryResponse InsertUpdateOrganizationProduct(Organization org, List<ProductEnum> productList);

		/// <summary>
		/// Used to delete a product from an Organization
		/// </summary>
		/// <param name="partyId"></param>
		/// <param name="product"></param>
		/// <returns></returns>
		IRepositoryResponse DeleteOrganizationProduct(long partyId, ProductEnum product);

		/// <summary>
		/// Used to insert a new product to an Organization
		/// </summary>
		/// <param name="partyId"></param>
		/// <param name="product"></param>
		/// <param name="configurationId"></param>
		/// <param name="fromDate"></param>
		/// <param name="thruDate"></param>
		/// <returns></returns>
		IRepositoryResponse InsertUpdateOrganizationProduct(long partyId, ProductEnum product, int? configurationId, DateTime? fromDate, DateTime? thruDate);

		/// <summary>
		/// Used to delete users for product for an Organization
		/// </summary>
		/// <param name="partyId">The organization id for the product</param>
		/// <param name="product">The product</param>
		/// <returns></returns>
		IRepositoryResponse DisableUsersForProduct(long partyId, ProductEnum product);
	}
}