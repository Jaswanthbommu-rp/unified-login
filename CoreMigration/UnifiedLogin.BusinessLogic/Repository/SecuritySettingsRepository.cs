using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Constants;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Landing;
using System;
using System.Collections.Generic;
using UnifiedLogin.DataAccess;
using UnifiedLogin.SharedObjects.Landing.Enum;

namespace UnifiedLogin.BusinessLogic.Repository
{
	/// <summary>
	/// Security Settings Repository
	/// </summary>
	public class SecuritySettingsRepository : BaseRepository, ISecuritySettingsRepository
	{
		#region Constructor
		/// <summary>
		/// Security Settings base Constructor
		/// </summary>
		public SecuritySettingsRepository() : base(DbConnectionEnum.IdpConfigurationDb)
		{
		}

		public SecuritySettingsRepository(IRepository repository) : base(repository)
		{
		}
		#endregion

		#region public Security Settings methods
		/// <summary>
		/// Get Security Settings (Password and Activity Configuration Security Settings)
		/// </summary>
		/// <param name="booksCustomerMasterId">Books Customer MasterId</param>
		/// <param name="bookMasterTypeId">Type of Book MasterId (e.g. 1 = Black, 2 = Blue)</param>
		/// <returns>Security Settings List objects (KeyValue pairs)</returns>
		public IList<Setting> GetSecuritySettings(long booksCustomerMasterId, int bookMasterTypeId = (int)BookMasterType.CustomerMasterId)
		{
			dynamic param = new
			{
				SourceId = booksCustomerMasterId,
				DataImportApplicationId = bookMasterTypeId
			};

			using (var repository = GetRepository())
			{
				return repository.GetMany<Setting>(StoredProcNameConstants.SP_GetSecuritySetting, param);
			}
		}

		/// <summary>
		/// Update Security Settings (Password and Activity Configuration Security Settings)
		/// </summary>
		/// <param name="settings">Security Settings (Password and Activity Configuration Security Settings) object of the parameter values</param>
		/// <param name="booksCustomerMasterId">Books Customer MasterId</param>
		/// <param name="bookMasterTypeId">Type of Book MasterId (e.g. 1 = Black, 2 = Blue)</param>
		/// <returns>Repository response object</returns>
		public RepositoryResponse UpdateSecuritySettings(IList<Setting> settings, long booksCustomerMasterId, int bookMasterTypeId = (int)BookMasterType.CustomerMasterId)
		{
			RepositoryResponse repositoryResponse = new RepositoryResponse();

			// ✅ Validate parameters before starting transaction
			if (settings == null || settings.Count == 0)
			{
				repositoryResponse.Id = 0;
				repositoryResponse.ErrorMessage = "Update Security Settings Error: No settings provided.";
				return repositoryResponse;
			}

			if (booksCustomerMasterId <= 0)
			{
				repositoryResponse.Id = 0;
				repositoryResponse.ErrorMessage = "Update Security Settings Error: Invalid booksCustomerMasterId.";
				return repositoryResponse;
			}

			using (var repository = GetRepository())
			{
				repository.UnitOfWork.BeginTransaction();
				try
				{
					string jsonSecuritySettings = Newtonsoft.Json.JsonConvert.SerializeObject(settings);
					
					dynamic param = new
					{
						SourceId = booksCustomerMasterId,
						DataImportApplicationId = bookMasterTypeId,
						JsonSecuritySettings = jsonSecuritySettings
					};

					// ✅ Execute stored procedure
					repositoryResponse = repository.GetOne<RepositoryResponse>(
						StoredProcNameConstants.SP_UpdateSecuritySetting, param);

					// ✅ Check for stored procedure errors
					if (repositoryResponse == null)
					{
						repositoryResponse = new RepositoryResponse
						{
							Id = 0,
							ErrorMessage = "Update Security Settings Error: Stored procedure returned null response."
						};
						repository.UnitOfWork.Rollback();
						return repositoryResponse;
					}

					if (repositoryResponse.Id == 0 || !string.IsNullOrWhiteSpace(repositoryResponse.ErrorMessage))
					{
						// ✅ Rollback on business logic error
						repository.UnitOfWork.Rollback();
						repositoryResponse.ErrorMessage = $"Update security settings Error: {repositoryResponse.ErrorMessage}";
						return repositoryResponse;
					}

					// ✅ SUCCESS - Commit transaction INSIDE try block
					repository.UnitOfWork.Commit();
				}
				catch (InvalidOperationException ex) when (ex.Message.Contains("transaction"))
				{
					// ✅ Handle specific transaction disposal errors
					repository.UnitOfWork.Rollback();
					repositoryResponse.Id = 0;
					repositoryResponse.ErrorMessage = $"Update Security Settings Transaction Error: {ex.Message}. The transaction was rolled back.";
				}
				
				catch (Exception exception)
				{
					// ✅ Handle all other errors
					repository.UnitOfWork.Rollback();
					repositoryResponse.Id = 0;
					repositoryResponse.ErrorMessage = $"Update Security Settings Unexpected Error: {exception.Message}";
				}
				
				return repositoryResponse;
			}
		}
		#endregion
	}
}