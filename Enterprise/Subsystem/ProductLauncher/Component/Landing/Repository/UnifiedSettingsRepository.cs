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
        /// Unified Settings base Constructor
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
		/// Get Unified Settings (Password and Activity Configuration Security Settings)
		/// </summary>
		/// <param name="PartyId">partyid</param>
		/// <param name="Category">setting category</param>
		/// <returns> Settings List objects (KeyValue pairs)</returns>
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

		public IList<Picklist> GetSettingsPickList(string category)
		{
			List<Picklist> picklists = new List<Picklist>();
			using (var repo = GetRepository())
			{
				dynamic param = new{
					categoryName = category
				};

				Picklist picklist = new Picklist();
				picklist.name = category;
				picklist.options = new List<Option>();
				var settingPickList = repo.GetMany<SettingPickList>(StoredProcNameConstants.SP_GetUnifiedSettingPicklist, param);

				foreach (SettingPickList item in settingPickList)
				{
					picklist.options.Add(new Option()
					{
						label = item.MappingName,
						value = item.MappingValue
					});
				}

				picklists.Add(picklist);
			};

			return picklists;
		}

		/// <summary>
		/// Update  Settings 
		/// </summary>
		/// <param name="settings"> Settings object of the parameter values</param>
		/// <param name="PartyId">partyid</param>
		/// <param name="Category">setting category</param>
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
							CreatedBy = userId,
							JsonUnifiedSettings = jsonSecuritySettings							
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
