using System;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.Repository.Interfaces;

/// <summary>
/// Used to add/update/delete a product from an organization
/// </summary>
public interface IOrganizationProductRepositoryAsync
{
	/// <summary>
	/// Used to delete a product from an organization
	/// </summary>
	/// <param name="partyId">Company party id</param>
	/// <param name="product">The product enum</param>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	Task<RepositoryResponse> DeleteOrganizationProductAsync(long partyId, int product, CancellationToken cancellationToken = default);

	/// <summary>
	/// Used to add/update a product to an organization
	/// </summary>
	/// <param name="partyId">Company party id</param>
	/// <param name="product">The product id</param>
	/// <param name="configurationId">The configuration id for the product being assigned. NULL will assign global product configuration</param>
	/// <param name="fromDate">When the product will be available from for the Organization</param>
	/// <param name="thruDate">How long the product is available for the Organization</param>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	Task<RepositoryResponse> InsertUpdateOrganizationProductAsync(long partyId, int product, int? configurationId, DateTime? fromDate, DateTime? thruDate, CancellationToken cancellationToken = default);

	/// <summary>
	/// Used to delete users for product for an Organization
	/// </summary>
	/// <param name="partyId">The organization id for the product</param>
	/// <param name="product">The product Id</param>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	Task<RepositoryResponse> DisableUsersForProductAsync(long partyId, ProductEnum product, CancellationToken cancellationToken = default);

	/// <summary>
	/// Create organization Product Setting (Expire the setting if exists)
	/// </summary>
	/// <param name="PartyId">User OrgId</param>
	/// <param name="ProductId">ProductId</param>
	/// <param name="ProductSettingTypeId">Product Setting TypeId</param>
	/// <param name="Value">Product Setting Type Value</param>
	/// <param name="cancellationToken"></param>
	/// <returns>Repository response object</returns>
	Task<RepositoryResponse> CreateOrganizationProductSettingAsync(long PartyId, int ProductId, int ProductSettingTypeId, string Value, CancellationToken cancellationToken = default);
}