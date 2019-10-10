using System;
using RP.Enterprise.Foundation.DataAccess.Component;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository
{
	/// <summary>
	/// Used to add/update/delete a product from an organization
	/// </summary>
	public class OrganizationProductRepository : BaseRepository, IOrganizationProductRepository
	{
		#region Constructor
		/// <summary>
		/// Base constructor
		/// </summary>
		public OrganizationProductRepository() : base(DbConnectionEnum.IdpConfigurationDb)
		{
		}

		/// <summary>
		/// Unit test constructor
		/// </summary>
		/// <param name="repository"></param>
		public OrganizationProductRepository(IRepository repository) : base(repository)
		{
		}
		#endregion

		/// <summary>
		/// Used to add/update a product to an organization
		/// </summary>
		/// <param name="partyId">Company party id</param>
		/// <param name="product">The product enum</param>
		/// <param name="configurationId">The configuration id for the product being assigned. NULL will assign global product configuration</param>
		/// <param name="fromDate">When the product will be available from for the Organization</param>
		/// <param name="thruDate">How long the product is available for the Organization</param>
		/// <returns></returns>
		public RepositoryResponse InsertUpdateOrganizationProduct(long partyId, ProductEnum product, int? configurationId, DateTime? fromDate, DateTime? thruDate)
		{
			RepositoryResponse newProductOrganization = new RepositoryResponse();

			using (var repository = GetRepository())
			{
				repository.UnitOfWork.BeginTransaction();
				try
				{
					//Create OrganizationProduct
					dynamic paramNewOrg = new
					{
						PartyId = partyId,
						ProductId = (int)product,
						ConfigurationID = configurationId,
						FromDate = fromDate,
						ThruDate = thruDate
					};

					newProductOrganization = repository.Execute<RepositoryResponse>(StoredProcNameConstants.SP_CreateOrganizationProduct, paramNewOrg);
				}
				catch (Exception exception)
				{
					repository.UnitOfWork.Rollback();
					newProductOrganization.ErrorMessage = "Failed to add/update product to organization";
				}
				repository.UnitOfWork.Commit();
			}
			return newProductOrganization;
		}

		/// <summary>
		/// Used to delete a product from an organization
		/// </summary>
		/// <param name="partyId">Company party id</param>
		/// <param name="product">The product enum</param>
		/// <returns></returns>
		public RepositoryResponse DeleteOrganizationProduct(long partyId, ProductEnum product)
		{
			RepositoryResponse removeProductOrganization = new RepositoryResponse();

			using (var repository = GetRepository())
			{
				repository.UnitOfWork.BeginTransaction();
				try
				{
					//Delete OrganizationProduct
					dynamic paramNewOrg = new
					{
						PartyId = partyId,
						ProductId = (int)product,
					};

					removeProductOrganization = repository.Execute<RepositoryResponse>(StoredProcNameConstants.SP_DeleteOrganizationProduct, paramNewOrg);
				}
				catch (Exception exception)
				{
					repository.UnitOfWork.Rollback();
					removeProductOrganization.ErrorMessage = "Failed to remove product from organization";
				}
				repository.UnitOfWork.Commit();
			}
			// there was nothing to delete so the response was null
			if (removeProductOrganization == null)
			{
				removeProductOrganization = new RepositoryResponse();
			}
			return removeProductOrganization;
		}

	}
}