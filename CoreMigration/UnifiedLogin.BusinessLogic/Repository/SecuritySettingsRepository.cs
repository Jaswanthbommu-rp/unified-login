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
			repositoryResponse.Id = 0;

			using (var repository = GetRepository())
			{
				repository.UnitOfWork.BeginTransaction();
				try
				{
					dynamic param;
					if (settings != null)
					{
							string jsonSecuritySettings = Newtonsoft.Json.JsonConvert.SerializeObject(settings);
							param = new
							{
								SourceId = booksCustomerMasterId,
								DataImportApplicationId = bookMasterTypeId,
								JsonSecuritySettings = jsonSecuritySettings
							};
							repositoryResponse = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_UpdateSecuritySetting, param);
							if (repositoryResponse.Id == 0)
							{
								repositoryResponse.ErrorMessage = $"Update security settings Error: {repositoryResponse.ErrorMessage}.";
							}
					}
				}
				catch (Exception exception)
				{
					repositoryResponse.Id = 0;
					repositoryResponse.ErrorMessage = "Update Security Settings Error: " + exception.Message;
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