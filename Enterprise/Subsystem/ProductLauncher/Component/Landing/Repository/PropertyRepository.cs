using RP.Enterprise.Foundation.DataAccess.Component;
using RP.Enterprise.Foundation.DataAccess.Component.Helper;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Base;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.BlackBook;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enterprise;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository
{
	/// <summary>
	/// Used to get property information from UnifiedLogin for the given persona and product. 
	/// </summary>
	public class PropertyRepository : BaseRepository, IPropertyRepository
	{
		#region Constructor
		/// <summary>
		/// PropertyRepository base Constructor
		/// </summary>
		public PropertyRepository() : base(DbConnectionEnum.IdpConfigurationDb)
		{
		}

        /// <summary>
        /// Unit test constructor
        /// </summary>
        /// <param name="repository"></param>
        public PropertyRepository(IRepository repository) : base(repository)
        {
        }

        #endregion

		/// <summary>
		/// List of Roles by User Persona ID
		/// </summary>
		/// <param name="userPersonaId">Persona ID</param>   
		/// <param name="productId">Product ID</param>   
		/// <returns>List of Properties/Role assigned to Persona</returns>
		public List<ProductProperty> ListPropertiesByPersona(long userPersonaId, int productId)
		{
			using (var repository = GetRepository())
			{
				dynamic param = new
				{
					PersonaID = userPersonaId,
					ProductID = productId
				};

				List<ProductProperty> propList = new List<ProductProperty>();
				var result = repository.GetMany<dynamic>(StoredProcNameConstants.SP_ListPropertyMapping, param);
				if (result != null)
				{
					foreach (var item in result)
					{
						propList.Add(new ProductProperty { ID = item.PropertyID.ToString() });
					}
				}
				return propList;
			}
		}

        /// <summary>
        /// Used to get the list of UPFM property instances for the given persona and product
        /// </summary>
        /// <param name="userPersonaId"></param>
        /// <param name="productId"></param>
        /// <returns></returns>
        public List<UPFMPropertyInstance> ListUPFMPropertyInstanceByPersona(long userPersonaId, ProductEnum productId)
        {
            using (var repository = GetRepository())
            {
                dynamic param = new
                {
                    PersonaID = userPersonaId,
                    ProductID = (int)productId
                };

                List<UPFMPropertyInstance> propList = repository.GetMany<UPFMPropertyInstance>(StoredProcNameConstants.SP_GetPropertyInstanceByPersonaId, param);
                return propList;
            }
        }

        /// <summary>
        /// Used to get the list of the internal UPFM property instance ids for the given persona and product
        /// </summary>
        /// <param name="userPersonaId"></param>
        /// <param name="productId"></param>
        /// <returns></returns>
        public List<int> ListUPFMPropertyInstanceIdByPersona(long userPersonaId, ProductEnum productId)
        {
            using (var repository = GetRepository())
            {
                dynamic param = new
                {
                    PersonaID = userPersonaId,
                    ProductID = (int)productId
                };

                List<int> propList = repository.GetMany<int>(StoredProcNameConstants.SP_GetPropertyInstanceIdsByPersonaId, param);
                return propList;
            }
        }

        /// <summary>
        /// Used to get the list of the internal UPFM property instance ids for the given persona and product
        /// </summary>
        /// <param name="userPersonaId"></param>
        /// <param name="productId"></param>
        /// <returns></returns>
        public List<int> ListUPFMPropertyInstanceIdByPersona(long userPersonaId, int productId)
        {
            using (var repository = GetRepository())
            {
                dynamic param = new
                {
                    PersonaID = userPersonaId,
                    ProductID = productId
                };

                List<int> propList = repository.GetMany<int>(StoredProcNameConstants.SP_GetPropertyInstanceIdsByPersonaId, param);
                return propList;
            }
        }

        /// <summary>
        /// Used to get the UPFM property details for the given instance ids
        /// </summary>
        /// <param name="propertyInstanceIds"></param>
        /// <returns></returns>

        public List<UPFMPropertyInstance> ListUPFMPropertyInstanceIdByInstanceIds(List<Guid> propertyInstanceIds)
        {
            using (var repository = GetRepository())
            {
                dynamic param = new
                {
                    @InstanceList = TableValueParamHelper.ConvertToTableValuedParameter(propertyInstanceIds, "Enterprise.PropertyInstanceType")
                };

                return repository.GetMany<UPFMPropertyInstance>(StoredProcNameConstants.SP_GetPropertyInstanceListById, param);
            }
        }

        /// <summary>
		/// Insert or Remove a Property for the given User
		/// </summary>
		/// <param name="userPersonaId">User Persona ID</param>      
		/// <param name="productId">Product ID</param>      
		/// <param name="propertyId">Property ID</param>      
		/// <param name="remove">isDeleted</param>   
		/// <returns>List of Roles assigned to Persona</returns>
		public RepositoryResponse InsertRemoveAssignedPropertyToUser(long userPersonaId, ProductEnum productId, long propertyId, int remove = 0)
		{
			using (var repository = GetRepository())
			{
				RepositoryResponse repositoryResponse = new RepositoryResponse();
				dynamic param = new
				{
					PersonaID = userPersonaId,
					ProductID = (int)productId,
					PropertyID = propertyId,
					Deleted = remove
				};

				int i = repository.ExecuteNonQuery(StoredProcNameConstants.SP_CreatePropertyMapping, param);
				repositoryResponse.Id = i;

				return repositoryResponse;
			}
		}

        /// <summary>
        /// Insert or Remove a Property instance for the given User
        /// </summary>
        /// <param name="userPersonaId">User Persona ID</param>      
        /// <param name="productId">Product ID</param>      
        /// <param name="propertyInstanceId">Property Instance ID</param>      
        /// <param name="remove">isDeleted</param>   
        /// <returns>List of Roles assigned to Persona</returns>
        public RepositoryResponse InsertRemoveAssignedPropertyInstanceToUser(long userPersonaId, int productId, long propertyInstanceId, int remove = 0)
        {
           
            RepositoryResponse result = new RepositoryResponse() { Id = 0, ErrorMessage = "" };

            dynamic param = new
            {
                PersonaID = userPersonaId,
                ProductID = productId,
                PropertyInstanceID = propertyInstanceId,
                Deleted = remove
            };

            using (var repository = GetRepository())
            {
                repository.UnitOfWork.BeginTransaction();
                try
                {
                    result = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_CreatePropertyInstanceMapping, param);
                }
                catch (Exception exception)
                {
                    result.ErrorMessage = exception.Message;
                }
                finally
                {
                    if (result.ErrorMessage.Length == 0)
                    {
                        repository.UnitOfWork.Commit();
                    }
                    else
                    {
                        repository.UnitOfWork.Rollback();
                    }
                }
                return result;
            }
        }
        
		/// <summary>
		/// Used to update any property mapping records that match the old id to a new id
		/// </summary>
		/// <param name="originalPropertyId"></param>
		/// <param name="newPropertyId"></param>
		/// <returns></returns>
        public RepositoryResponse UpdatePropertyMappingReMap(long originalPropertyId, long newPropertyId)
        {
            RepositoryResponse result = new RepositoryResponse() {Id = 0, ErrorMessage = ""};
            
            dynamic param = new
            {
                @OriginalPropertyID = originalPropertyId,
                @NewPropertyID = newPropertyId
            };

            using (var repository = GetRepository())
            {
                repository.UnitOfWork.BeginTransaction();
                try
                {
                    result = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_UpdatePropertyMappingReMap, param);
                }
                catch (Exception exception)
                {
                    result.ErrorMessage = exception.Message;
                }
                finally
                {
                    if (result.ErrorMessage.Length == 0)
                    {
                        repository.UnitOfWork.Commit();
                    }
                    else
                    {
                        repository.UnitOfWork.Rollback();
                    }
                }
                return result;
            }
        }

        /// <summary>
        /// Used to insert new UPFM property instances into the database
        /// </summary>
        /// <param name="propertyInstance"></param>
        /// <returns></returns>
        public RepositoryResponse InsertUPFMPropertyInstance(UPFMPropertyInstance propertyInstance)
        {
            RepositoryResponse result = new RepositoryResponse() {Id = 0, ErrorMessage = ""};
            
            dynamic param = new
            {
                @Name			= propertyInstance.Name
                ,@Address		= propertyInstance.Address
                ,@City			= propertyInstance.City
                ,@State			= propertyInstance.State
                ,@PostalCode	= propertyInstance.PostalCode
                ,@Country		= propertyInstance.Country
                ,@County		= propertyInstance.County
                ,@Latitude		= propertyInstance.Latitude
                ,@Longitude		= propertyInstance.Longitude
                ,@CustomerPropertyId = propertyInstance.CustomerPropertyId
                ,@Domain = propertyInstance.Domain
            };

            using (var repository = GetRepository())
            {
                repository.UnitOfWork.BeginTransaction();
                try
                {
                    result = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_CreatePropertyInstance, param);
                }
                catch (Exception exception)
                {
                    result.ErrorMessage = exception.Message;
                }
                finally
                {
                    if (result.ErrorMessage.Length == 0)
                    {
                        repository.UnitOfWork.Commit();
                    }
                    else
                    {
                        repository.UnitOfWork.Rollback();
                    }
                }
                return result;
            }
        }
        /// <summary>
        /// Bulk insert/delete property instance mappings for a user using TVP
        /// </summary>
        /// <param name="userPersonaId">User Persona ID</param>
        /// <param name="productId">Product ID</param>
        /// <param name="propertyMappings">List of property mappings to insert/delete</param>
        /// <returns>Repository response with success/error counts</returns>
        public RepositoryResponse BulkInsertRemovePropertyInstanceMappings(
            long userPersonaId,
            int productId,
            List<UPFMPropertyInstanceMapping> propertyMappings)
        {
            using (var repository = GetRepository())
            {
                var tableInfo = new RP.Enterprise.Foundation.DataAccess.Component.Model.TableValueParmInfo
                {
                    StoredProcedureName = StoredProcNameConstants.SP_BulkCreateDeleteUPFMPropertyInstanceMapping,
                    TableParamTypeName = "Enterprise.UPFMPropertyInstanceMapping",
                    TableVariableName = "@PropertyMappings",
                    OrderedColumnName = new List<string> { "PropertyInstanceID", "IsDeleted" }
                };

                var param = new Dapper.DynamicParameters();
                param.Add("@PersonaID", userPersonaId);
                param.Add("@ProductID", productId);

                var result = repository.GetManyWithTvp<UPFMPropertyInstanceMapping, dynamic>(
                    tableInfo,
                    propertyMappings,
                    param).FirstOrDefault();

                return new RepositoryResponse
                {
                    Id = result?.SuccessCount ?? 0,
                    ErrorMessage = result?.ErrorMessage ?? string.Empty
                };
            }
        }

        #region Get PropertyList for Company
        /// <summary>
        /// Get Properties for a Organization
        /// </summary>
        /// <param name="propertyInstanceIds">propertyInstanceIds</param>
        /// <param name="propertyName">PropertyName</param>
        /// <param name="propertyMasterid ">propertyMasterid </param>
        /// <param name="status"></param>
        /// <param name="dataFilterSort">datafilter</param>
        /// <returns>List of Properties for a company </returns>
        public List<PropertySetup> GetPropertiesForCompany(List<Guid> propertyInstanceIds, string propertyName = null, int? propertyMasterid = null, int? status = null, RequestParameter dataFilterSort = null)
        {
            string sortBy = "Name";
            string sortDirection = "Asc";
           
            if (dataFilterSort != null)
            {
                if (dataFilterSort.SortBy != null)
                {
                    foreach (string SortKey in dataFilterSort.SortBy.Keys)
                    {
                        sortBy = SortKey;
                        sortDirection = dataFilterSort.SortBy[SortKey];
                    }
                }
            }
            dynamic param = new
            {
                InstanceList = TableValueParamHelper.ConvertToTableValuedParameter(propertyInstanceIds, "Enterprise.PropertyInstanceType"),
                Name = propertyName,
                PropertyMasterid = propertyMasterid,
                Status = status,
                SortColumn = sortBy,
                SortDirection = sortDirection,
                RowsPerPage = dataFilterSort.Pages.ResultsPerPage == 100 ? 0 : dataFilterSort.Pages.ResultsPerPage,
                PageNumber = ((dataFilterSort.Pages.ResultsPerPage == 100) || (dataFilterSort.Pages.StartRow <= 0)) ? 1 : dataFilterSort.Pages.StartRow
            };
            using (var repository = GetRepository())
            {
                return repository.GetMany<PropertySetup>(StoredProcNameConstants.SP_GetPropertyInstanceListByIdWithPaging, param);
            }
        }
        #endregion

        #region Update Property
        /// <summary>
        /// Update property
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        public RepositoryResponse UpdateProperty(UPFMPropertyInstance property)
        {
            dynamic param = new
            {
                InstanceId = property.InstanceId,
                Name = property.Name,
                Active = property.IsActive,
                Address = property.Address,
                City = property.City,
                State = property.State,
                PostalCode = property.PostalCode,
                Country = property.Country,
                County = property.County
            };

            using (var repository = GetRepository())
            {
                return repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_UpdatePropertyInstance, param);                
            }
        }

        public RepositoryResponse UpdateUPFMPropertyList(List<UPFMPropertyInstance> propertyInstanceIds)
        {
            using (var repository = GetRepository())
            {
                dynamic param = new
                {
                    @InstanceList = TableValueParamHelper.ConvertToTableValuedParameter(propertyInstanceIds.Select(m => m.InstanceId).ToList(), "Enterprise.PropertyInstanceType"),
                    @Active = propertyInstanceIds.FirstOrDefault().IsActive
                };

                return repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_UpdatePropertyInstances, param);
            }
        }
        #endregion

        #region Delete Property
        /// <summary>
        /// Delete Property
        /// </summary>
        /// <param name="instanceId">propertyInstanceId</param>
        /// <returns>Repository response object</returns>
        public RepositoryResponse DeleteUPFMPropertyInstance(Guid instanceId)
        {
            dynamic param = new
            {
                instanceId
            };
            using (var repository = GetRepository())
            {
                return repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_DeletePropertyInstance, param);
            }
        }
        #endregion

        #region Stage User Product Primary Properties
        public RepositoryResponse StageUserProductPrimaryProperties(string stagingData, long userPersonaId, int productId, long createdBy)
        {
            RepositoryResponse result = new RepositoryResponse() { Id = 0, ErrorMessage = "" };

            using (var repository = GetRepository())
            {
                dynamic param = new
                {
                    @ProductId = productId,
                    @PersonaId = userPersonaId,
                    @ModifiedBy = createdBy,
                    @PropertyInstanceJSON = stagingData                    
                };

                result = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_AddPersonaProductMatchedPrimaryProperties, param);
            }
            return result;

        }

        public RepositoryResponse DeleteStagedUserProductPrimaryProperties(long userPersonaId, int productId)
        {
            RepositoryResponse result = new RepositoryResponse() { Id = 0, ErrorMessage = "" };

            using (var repository = GetRepository())
            {
                dynamic param = new
                {
                    @ProductId = productId,
                    @PersonaId = userPersonaId
                };

                result = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_DeletePersonaProductMatchedPrimaryProperties, param);
            }
            return result;
        }
        #endregion

        #region Private methods

        #endregion
    }
}