using System;
using System.Collections.Generic;
using UnifiedLogin.DataAccess;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Constants;
using UnifiedLogin.SharedObjects.Enum;
using Serilog.Events;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Landing.Enum;

namespace UnifiedLogin.BusinessLogic.Repository
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
		/// <param name="product">The product id</param>
		/// <param name="configurationId">The configuration id for the product being assigned. NULL will assign global product configuration</param>
		/// <param name="fromDate">When the product will be available from for the Organization</param>
		/// <param name="thruDate">How long the product is available for the Organization</param>
		/// <returns></returns>
		public RepositoryResponse InsertUpdateOrganizationProduct(long partyId, int product, int? configurationId, DateTime? fromDate, DateTime? thruDate)
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
						ProductId = product,
						ConfigurationID = configurationId,
						FromDate = fromDate,
						ThruDate = thruDate
					};

					newProductOrganization = repository.Execute<RepositoryResponse>(StoredProcNameConstants.SP_CreateOrganizationProduct, paramNewOrg);
                    repository.UnitOfWork.Commit();
                }
				catch (Exception exception)
				{
					repository.UnitOfWork.Rollback();
					newProductOrganization.ErrorMessage = "Failed to add/update product to organization";
				}				
			}
			return newProductOrganization;
		}

		/// <summary>
		/// Used to delete a product from an organization
		/// </summary>
		/// <param name="partyId">Company party id</param>
		/// <param name="product">The product enum</param>
		/// <returns></returns>
		public RepositoryResponse DeleteOrganizationProduct(long partyId, int product)
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
                    repository.UnitOfWork.Commit();
                }
				catch (Exception exception)
				{
					repository.UnitOfWork.Rollback();
					removeProductOrganization.ErrorMessage = "Failed to remove product from organization";
				}				
			}
			// there was nothing to delete so the response was null
			if (removeProductOrganization == null)
			{
				removeProductOrganization = new RepositoryResponse();
			}
			return removeProductOrganization;
		}


		/// <summary>
		/// Used to delete users for product for an Organization
		/// </summary>
		/// <param name="partyId">The organization id for the product</param>
		/// <param name="product">The product Id</param>
		/// <returns></returns>
		public RepositoryResponse DisableUsersForProduct(long partyId, ProductEnum product)
		{
			RepositoryResponse removeUsersForProduct = new RepositoryResponse();

			using (var repository = GetRepository())
			{
				repository.UnitOfWork.BeginTransaction();
				try
				{
					dynamic paramNewOrg = new
					{
						PartyId = partyId,
						ProductId = (int)product,
					};

					removeUsersForProduct = repository.Execute<RepositoryResponse>(StoredProcNameConstants.SP_DisableUsersForProduct, paramNewOrg);
                    repository.UnitOfWork.Commit();
                }
				catch (Exception exception)
				{
					repository.UnitOfWork.Rollback();
					removeUsersForProduct.ErrorMessage = "Failed to remove product from organization";
				}

			}
			// there was nothing to delete so the response was null
			if (removeUsersForProduct == null)
			{
				removeUsersForProduct = new RepositoryResponse();
			}
			return removeUsersForProduct;
		}

		/// <summary>
		/// Create organization Product Setting (Expire the setting if exists)
		/// </summary>
		/// <param name="PartyId">User OrgId</param>
		/// <param name="ProductId">ProductId</param>
		/// <param name="ProductSettingTypeId">Product Setting TypeId</param>
		/// <param name="Value">Product Setting Type Value</param>
		/// <returns>Repository response object</returns>
		public RepositoryResponse CreateOrganizationProductSetting(long PartyId, int ProductId, int ProductSettingTypeId, string Value)
		{
			DateTime utcNow = DateTime.UtcNow;
			DateTime utcMaxValue = DateTime.MaxValue.ToUniversalTime();
			RepositoryResponse repositoryResponse = new RepositoryResponse();
			int ProductSettingId = 0;
			int ConfigurationId = 0;
			Dictionary<string, object> dataLog = new Dictionary<string, object>();
			
			using (var repository = GetRepository())
			{
				try
				{
					//Setup the parameter values to CreatePersonaConfiguration
					dynamic param = new
					{
						PartyId = PartyId,
						ProductId = ProductId,
					};
					//Create CreatePersonaConfiguration
					repositoryResponse = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_CreateOrganizationProductConfiguration, param);
					
					if (repositoryResponse.Id == 0)
					{
						repositoryResponse.ErrorMessage = "CreateOrganizationProductSetting Error: CreatePersonaConfiguration failed.";						
					}
					else
					{
						ConfigurationId = Convert.ToInt32(repositoryResponse.Id);
						//Setup the parameter values to CreateProductSetting
						param = new
						{
							ProductId = ProductId,
							ProductSettingTypeId = ProductSettingTypeId,
							Value = Value,
							FromDate = utcNow,
							ProductSettingId = ProductSettingId
						};
						//CreateProductSetting
						repositoryResponse = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_CreateProductSetting, param);
						
						if (repositoryResponse.Id == 0)
						{
							repositoryResponse.ErrorMessage = "CreateOrganizationProductSetting Error: CreateProductSetting failed.";
						}
						else
						{
							ProductSettingId = Convert.ToInt32(repositoryResponse.Id);
							//Setup the parameter values to CreateProductConfigurationbyPersonaId
							param = new
							{
								OrgPartyId = PartyId,
								ConfigurationId = ConfigurationId,
								ProductId = ProductId,
								ProductSettingID = ProductSettingId
							};
							//CreateProductConfigurationbyPartyId
							repositoryResponse = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_CreateOrganizationProductConfigurationbyPartyId, param);
							
							if (repositoryResponse.Id == 0)
							{
								repositoryResponse.ErrorMessage = "CreateOrganizationProductSetting Error: CreateOrganizationProductConfigurationbyPartyId failed.";
								
							}

						}
					}
				}
				catch (Exception exception)
				{
					repositoryResponse = new RepositoryResponse();
					repositoryResponse.ErrorMessage = $"Create/Update Organization Product Setting Error: " + exception.Message;
				}
				
				return repositoryResponse;
			}
		}
		
    }
}