using System;
using System.Collections.Generic;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic
{
	/// <summary>
	/// ManageOrganizationProduct class
	/// </summary>
	public class ManageOrganizationProduct : IManageOrganizationProduct
	{
		#region Private Variables
		IOrganizationProductRepository _organizationProductRepository;
		#endregion

		#region Constructors
		/// <summary>
		/// Manage Organization Product Constructor
		/// </summary>
		/// <param name="organizationProductRepository">Organization Product Repository</param>

		public ManageOrganizationProduct(IOrganizationProductRepository organizationProductRepository)
		{
			_organizationProductRepository = organizationProductRepository;
		}

		/// <summary>
		/// Create a basic instance of the ManageOrganizationProduct class
		/// </summary>
		/// 
		public ManageOrganizationProduct()
		{
			_organizationProductRepository = new OrganizationProductRepository();
		}
		#endregion

		#region Public methods

		/// <summary>
		/// Used to add a list of products to the given company
		/// </summary>
		/// <param name="partyId"></param>
		/// <param name="productList"></param>
		/// <returns></returns>
		public IRepositoryResponse InsertUpdateOrganizationProduct(long partyId, List<ProductEnum> productList)
		{
            IRepositoryResponse response = new RepositoryResponse();
			foreach (ProductEnum product in productList)
			{
				response = InsertUpdateOrganizationProduct(partyId: partyId, product: product, configurationId: null, fromDate: null, thruDate: null);
				if (!string.IsNullOrEmpty(response.ErrorMessage))
				{
					return response;
				}
			}
			return response;
		}

		/// <summary>
		/// Used to insert a new product to an Organization
		/// </summary>
		/// <param name="partyId"></param>
		/// <param name="product"></param>
		/// <param name="configurationId"></param>
		/// <param name="fromDate"></param>
		/// <param name="thruDate"></param>
		/// <returns></returns>
		public IRepositoryResponse InsertUpdateOrganizationProduct(long partyId, ProductEnum product, int? configurationId, DateTime? fromDate, DateTime? thruDate )
		{
			return _organizationProductRepository.InsertUpdateOrganizationProduct(partyId, product, configurationId, fromDate, thruDate);
		}

		/// <summary>
		/// Used to delete a product from an Organization
		/// </summary>
		/// <param name="partyId">The organization id for the product to delete</param>
		/// <param name="product">The product to delete</param>
		/// <returns></returns>
		public IRepositoryResponse DeleteOrganizationProduct(long partyId, ProductEnum product)
		{
			return _organizationProductRepository.DeleteOrganizationProduct(partyId, product);
		}

		/// <summary>
		/// Used to delete users for product for an Organization
		/// </summary>
		/// <param name="partyId">The organization id for the product</param>
		/// <param name="product">The product Id</param>
		/// <returns></returns>
		public IRepositoryResponse DisableUsersForProduct(long partyId, ProductEnum product)
		{
			return _organizationProductRepository.DisableUsersForProduct(partyId, product);
		}

		#endregion
	}
}