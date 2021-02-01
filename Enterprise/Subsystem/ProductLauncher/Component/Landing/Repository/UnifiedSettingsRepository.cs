using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using System;
using System.Collections.Generic;
using RP.Enterprise.Foundation.DataAccess.Component;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository
{
    public class UnifiedSettingsRepository : BaseRepository, IUnifiedSettingsRepository
	{
        #region Constructor
        /// <summary>
        /// Security Settings base Constructor
        /// </summary>
        public UnifiedSettingsRepository() : base(DbConnectionEnum.IdpConfigurationDb)
        {
        }

        public UnifiedSettingsRepository(IRepository repository) : base(repository)
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
		public IList<Setting> GetUnifiedSettings(long PartyId, string Category)
		{
			dynamic param = new
			{
				PartyId = PartyId,
				Category = Category
			};

			using (var repository = GetRepository())
			{
				return repository.GetMany<Setting>(StoredProcNameConstants.SP_GetUnifiedSetting, param);
			}
		}

		/// <summary>
		/// Update Security Settings (Password and Activity Configuration Security Settings)
		/// </summary>
		/// <param name="settings">Security Settings (Password and Activity Configuration Security Settings) object of the parameter values</param>
		/// <param name="booksCustomerMasterId">Books Customer MasterId</param>
		/// <param name="bookMasterTypeId">Type of Book MasterId (e.g. 1 = Black, 2 = Blue)</param>
		/// <returns>Repository response object</returns>
		public RepositoryResponse UpdateUnifiedSettings(IList<Setting> settings, long PartyId, string Category, long userId)
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
							PartyId = PartyId,
							Category = Category,
							JsonUnifiedSettings = jsonSecuritySettings,
							CreatedBy = userId
						};
						repositoryResponse = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_UpdateUnifiedSetting, param);
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
