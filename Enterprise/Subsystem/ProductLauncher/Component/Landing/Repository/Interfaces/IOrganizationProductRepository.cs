using System;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces
{
	/// <summary>
	/// Used to add/update/delete a product from an organization
	/// </summary>
	public interface IOrganizationProductRepository
	{
		/// <summary>
		/// Used to delete a product from an organization
		/// </summary>
		/// <param name="partyId">Company party id</param>
		/// <param name="product">The product enum</param>
		/// <returns></returns>
		RepositoryResponse DeleteOrganizationProduct(long partyId, int product);

		/// <summary>
		/// Used to add/update a product to an organization
		/// </summary>
		/// <param name="partyId">Company party id</param>
		/// <param name="product">The product id</param>
		/// <param name="configurationId">The configuration id for the product being assigned. NULL will assign global product configuration</param>
		/// <param name="fromDate">When the product will be available from for the Organization</param>
		/// <param name="thruDate">How long the product is available for the Organization</param>
		RepositoryResponse InsertUpdateOrganizationProduct(long partyId, int product, int? configurationId, DateTime? fromDate, DateTime? thruDate);

		/// <summary>
		/// Used to delete users for product for an Organization
		/// </summary>
		/// <param name="partyId">The organization id for the product</param>
		/// <param name="product">The product Id</param>
		/// <returns></returns>
		RepositoryResponse DisableUsersForProduct(long partyId, ProductEnum product);

		/// <summary>
		/// Create organization Product Setting (Expire the setting if exists)
		/// </summary>
		/// <param name="PartyId">User OrgId</param>
		/// <param name="ProductId">ProductId</param>
		/// <param name="ProductSettingTypeId">Product Setting TypeId</param>
		/// <param name="Value">Product Setting Type Value</param>
		/// <returns>Repository response object</returns>
		RepositoryResponse CreateOrganizationProductSetting(long PartyId, int ProductId, int ProductSettingTypeId, string Value);
	}
}