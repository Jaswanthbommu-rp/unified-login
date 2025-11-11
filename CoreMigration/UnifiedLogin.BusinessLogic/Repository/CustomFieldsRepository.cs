using Newtonsoft.Json;
using UnifiedLogin.DataAccess;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Landing;
using System;
using System.Collections.Generic;
using System.Linq;
using UnifiedLogin.SharedObjects.Constants;
using UnifiedLogin.SharedObjects.Landing.Enum;

namespace UnifiedLogin.BusinessLogic.Repository
{
	/// <summary>
	/// Custom fields Repository
	/// </summary>
	public class CustomFieldsRepository : BaseRepository, ICustomFieldsRepository
	{
		#region Constructor
		/// <summary>
		/// Security Settings base Constructor
		/// </summary>
		public CustomFieldsRepository() : base(DbConnectionEnum.IdpConfigurationDb)
		{
		}

		/// <summary>
		/// Unit test constructor
		/// </summary>
		/// <param name="repository"></param>
		public CustomFieldsRepository(IRepository repository) : base(repository)
		{
		}
		#endregion

		#region public Custom Field methods
		
		/// <summary>
		/// Get Custom Fields by partyid
		/// </summary>
		/// <param name="partyId">partyId</param>	
		/// <param name="dataFilterSort">Data Filtering and Sorting</param>
		/// <returns>Custom Fields (KeyValue pairs)</returns>
		public IList<Setting> GetCustomFields(long partyId, RequestParameter dataFilterSort = null)
		{
			string customFieldsJson = string.Empty;

			IList<CustomField> customField = GetCustomField(partyId, dataFilterSort);

			if (customField.Count > 0)
			{
				customFieldsJson = JsonConvert.SerializeObject(
					new
					{
						customField
					}
				);
			}

			IList<Setting> settingList = new List<Setting>()
			{
				new Setting()
				{
					Name = "customfields",
					Value = customFieldsJson,
					Right = 0
				}
			};

			return settingList;
		}

		/// <summary>
		/// Get Custom Fields
		/// </summary>
		/// <param name="partyId">party id</param>	
		/// <param name="dataFilterSort">Data Filtering and Sorting</param>
		/// <returns>List of Custom Fields objects</returns>
		public IList<CustomField> GetCustomField(long partyId,  RequestParameter dataFilterSort = null)
		{
			IList<CustomField> customFields = new List<CustomField>();

			IList<FilterTableType> filterBy = new List<FilterTableType>();
			dataFilterSort.FilterBy.ToList().ForEach(f =>
			{
				if (f.Key.Equals("Enabled", StringComparison.OrdinalIgnoreCase))
				{
					filterBy.Add(
						new FilterTableType()
						{
							ColumnName = f.Key,
							SearchValue = f.Value.Substring(0, Math.Min(128, f.Value.Length))
						}
					);
				}
			});
			string filterByJson = string.Empty;
			if (filterBy.Count > 0)
			{
				filterByJson = JsonConvert.SerializeObject(
					new
					{
						filterBy
					}
				);
			}

			IList<SortTableType> sortBy = new List<SortTableType>();
			dataFilterSort.SortBy.ToList().ForEach(s =>
			{
				sortBy.Add(
					new SortTableType()
					{
						ColumnName = s.Key,
						SortDirection = s.Value.Substring(0, Math.Min(128, s.Value.Length))
					}
				);
			});
			string sortByJson = string.Empty;
			if (sortBy.Count > 0)
			{
				sortByJson = JsonConvert.SerializeObject(
					new
					{
						sortBy
					}
				);
			}

			dynamic param = new
			{
				PartyId = partyId,
				FilterBy = filterByJson,
				SortBy = sortByJson,
				RowsPerPage = dataFilterSort.Pages.ResultsPerPage,
				PageNumber = (dataFilterSort.Pages.StartRow <= 0) ? 1 : dataFilterSort.Pages.StartRow
			};

			using (var repository = GetRepository())
			{
				customFields = repository.GetMany<CustomField>(StoredProcNameConstants.SP_GetFieldsByPartyId, param);
			}
			return customFields;
		}
		
		/// <summary>
		/// Get Custom Fields Values for a User
		/// </summary>
		/// <param name="organizationPartyId">Unique Organization PartyId</param>
		/// <param name="userLoginPersonaId">@UserLoginPersonaId</param>
		/// <param name="enabled">Enabled</param>
		/// <returns>Custom Fields Values for a User</returns>
		public IList<CustomFieldValue> GetCustomFieldsValues(long organizationPartyId, long? userLoginPersonaId = null, bool? enabled = null)
		{
			IList<CustomFieldValue> customFieldValueList = new List<CustomFieldValue>();
			dynamic param = new
			{
				OrganizationPartyId = organizationPartyId,
				UserLoginPersonaId = userLoginPersonaId,
				Enabled = enabled
			};

			using (var repository = GetRepository())
			{
				customFieldValueList = repository.GetMany<CustomFieldValue>(StoredProcNameConstants.SP_GetFieldsValuesByUserLoginPersonaId, param);
			}

			return customFieldValueList;
		}

		/// <summary>
		/// Add/Update Custom Fields values for a user
		/// </summary>
		/// <param name="customFieldsValuesJson">Custom Fields values</param>
		/// <param name="createdBy">Created/Modified by UserId</param>
		/// <returns>Repository response object</returns>
		public RepositoryResponse AddUpdateFieldValue(string customFieldsValuesJson, long createdBy)
		{
			RepositoryResponse repositoryResponse = new RepositoryResponse();
			repositoryResponse.Id = 0;

			using (var repository = GetRepository())
			{
				repository.UnitOfWork.BeginTransaction();
				try
				{
					dynamic paramUserCustomFieldValues = new
					{
						JSON = customFieldsValuesJson,
						CreatedBy = createdBy
					};
					repositoryResponse = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_AddUpdateFieldValue, paramUserCustomFieldValues);
					if ((repositoryResponse.Id == 0) && (!string.IsNullOrWhiteSpace(repositoryResponse.ErrorMessage)))
					{
						repositoryResponse.ErrorMessage = $"Add/Update custom fields values {customFieldsValuesJson} Error: {repositoryResponse.ErrorMessage}.";
					}
				}
				catch (Exception exception)
				{
					repositoryResponse.Id = 0;
					repositoryResponse.ErrorMessage = $"Update Custom Fields values {customFieldsValuesJson} Exception: " + exception.Message;
				}
				finally
				{
					if (repositoryResponse.ErrorMessage.Length == 0)
					{
						//Commit and end transaction.
						repository.UnitOfWork.Commit();
					}
					else
					{
						//Rollback transaction and dispose it.
						repository.UnitOfWork.Rollback();
					}
				}
				return repositoryResponse;
			}
		}
		#endregion
	}
}
