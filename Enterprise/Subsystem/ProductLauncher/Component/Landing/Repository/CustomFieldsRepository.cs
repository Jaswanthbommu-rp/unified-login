using Newtonsoft.Json;
using RP.Enterprise.Foundation.DataAccess.Component;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Base;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository
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
		/// Get Custom Fields
		/// </summary>
		/// <param name="partyId">PartyId/param>
		/// <param name="dataFilterSort">Data Filtering and Sorting</param>
		/// <returns>Custom Fields (KeyValue pairs)</returns>
		public IList<Setting> GetCustomFields(long partyId, RequestParameter dataFilterSort = null)
		{
			string customFieldsJson = string.Empty;

			IList<CustomField> customField = GetCustomField(partyId, dataFilterSort);

			if (customField.Count > 0)
			{
				customFieldsJson = JsonConvert.SerializeObject(
					new {
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
		/// <param name="booksCustomerMasterId">Books Customer MasterId</param>
		/// <param name="dataFilterSort">Data Filtering and Sorting</param>
		/// <returns>List of Custom Fields objects</returns>
		public IList<CustomField> GetCustomField(long partyId, RequestParameter dataFilterSort = null)
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
				customFields = repository.GetMany<CustomField>(StoredProcNameConstants.SP_GetFieldsByMasterId, param);
			}
			return customFields;
		}

		/// <summary>
		/// Get CustomField Type
		/// </summary>
		/// <param name="fieldTypeId">Optional FieldTypeId</param>
		/// <returns>List of CustomField types</returns>
		public IList<CustomFieldType> GetCustomFieldType(byte? fieldTypeId = null)
		{
			IList<CustomFieldType> customFieldTypeList = new List<CustomFieldType>();
			dynamic param = new
			{
				FieldTypeId = fieldTypeId
			};

			using (var repository = GetRepository())
			{
				customFieldTypeList = repository.GetMany<CustomFieldType>(StoredProcNameConstants.SP_GetFieldType, param);
			}
			return customFieldTypeList;
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
		/// Add/Update Custom Fields
		/// </summary>
		/// <param name="settings">A list of one Setting object where the Value is a JSON of the Custom Fields to Add/Update</param>
		/// <param name="userId">Logged in UserId</param>
		/// <param name="booksCustomerMasterId">Books Customer MasterId</param>
		/// <param name="bookMasterTypeId">Type of Book MasterId (e.g. 1 = Black, 2 = Blue)</param>
		/// <returns>Repository response object</returns>
		public RepositoryResponse AddUpdateDeleteCustomFields(IList<Setting> settings, long userId, long partyId, string operation = "update")
		{
			RepositoryResponse repositoryResponse = new RepositoryResponse();
			repositoryResponse.Id = 0;

			using (var repository = GetRepository())
			{
				repository.UnitOfWork.BeginTransaction();
				try
				{
					dynamic param;
					if ((settings != null) && (settings.Count > 0))
					{
						string jsonCustomFields = settings[0].Value;
						param = new
						{
							PartyId = partyId,
							Operation = operation,
							Json = jsonCustomFields,
							CreatedBy = userId
						};
						repositoryResponse = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_AddUpdateFieldFromJSON, param);
						if ((repositoryResponse.Id == 0) && (!string.IsNullOrWhiteSpace(repositoryResponse.ErrorMessage)))
						{
							repositoryResponse.ErrorMessage = $"Update custom fields Error: {repositoryResponse.ErrorMessage}.";
						}
					}
				}
				catch (Exception exception)
				{
					repositoryResponse.Id = 0;
					repositoryResponse.ErrorMessage = "Update Custom Fields Exception: " + exception.Message;
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
