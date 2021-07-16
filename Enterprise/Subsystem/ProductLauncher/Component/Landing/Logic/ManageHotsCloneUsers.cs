using Newtonsoft.Json;
using RP.Enterprise.Foundation.DataAccess.Component;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Enterprise.Helpers;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.ProductIntegration.Model;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Audit.Common;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Base;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.BlackBook;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Constants;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enterprise;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Extensions;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Hots;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.Accounting;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.Ops;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.Rum;
using Serilog;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic
{
	public class ManageHotsCloneUsers : IManageHotsCloneUsers
	{
		//step 1 : Get List of users from base line company except employee user
		//step2 : loop through list of users
		// construct user profile object
		// get user assigned products data and replace properties with clone company properties and generate product bach data
		// call sp to create user

		private IProductRepository _productRepository;
		private IProductInternalSettingRepository _productInternalSettingRepository;
		private IManagePersona _managePersona;
		private IHOTSCloneUserRepository _hotsCloneUserRepository;
		private IOrganizationRepository _organizationRepository;
		private IManageProfile _manageProfile;
		private DefaultUserClaim _defaultUserClaim;
		private IManageProduct _manageProduct;
		readonly ITokenHelper _tokenHelper;
		#region Ctor

		/// <summary>
		/// Used for dependency injection
		/// </summary> 
		public ManageHotsCloneUsers(IProductRepository productRepository,
									IProductInternalSettingRepository productInternalSettingRepository,
									IManagePersona managePersona,
									IHOTSCloneUserRepository hotsCloneUserRepository,
									IManageOrganization manageOrganization,
									IManageProfile manageProfile,
									DefaultUserClaim userClaim)
		{
			_productRepository = productRepository;
			_productInternalSettingRepository = productInternalSettingRepository;
			_managePersona = managePersona;
			_hotsCloneUserRepository = hotsCloneUserRepository;
			_organizationRepository = new OrganizationRepository();
			_manageProfile = manageProfile;
			_defaultUserClaim = userClaim;
			_tokenHelper = new TokenHelper();
		}

		/// <summary>
		/// Repository test Constructor
		/// </summary>
		public ManageHotsCloneUsers(IRepository repository, DefaultUserClaim userClaim, HttpMessageHandler messageHandler)
		{
			_productRepository = new ProductRepository(repository, userClaim);
			_productInternalSettingRepository = new ProductInternalSettingRepository(repository);
			_managePersona = new ManagePersona(repository, userClaim, messageHandler);
			_hotsCloneUserRepository = new HOTSCloneUserRepository(repository);
			_organizationRepository = new OrganizationRepository(repository);
			_manageProfile = new ManageProfile(userClaim);
			_manageProduct = new ManageProduct(repository, userClaim, messageHandler);
			_defaultUserClaim = userClaim;
		}

		public ManageHotsCloneUsers(DefaultUserClaim userClaim)
		{
			_productRepository = new ProductRepository();
			_productInternalSettingRepository = new ProductInternalSettingRepository();
			_managePersona = new ManagePersona();
			_hotsCloneUserRepository = new HOTSCloneUserRepository();
			_organizationRepository = new OrganizationRepository();
			_manageProfile = new ManageProfile(userClaim);
			_manageProduct = new ManageProduct(userClaim);
			_defaultUserClaim = userClaim;
		}
		#endregion

		public ClonedUsers CloneUsersFromBaseLineCompany(CloneUsers cloneUsers, long basePartyId, long clonePartyId)
		{
			ClonedUsers clonedUsers = new ClonedUsers();
			clonedUsers.Status = "InComplete";
			clonedUsers.CloneCustomerCompanyId = cloneUsers.CloneCustomerUPFMId;
			clonedUsers.CloneCustomerEnvironment = cloneUsers.CloneCustomerEnvironment;
			var productInternalSettings = _manageProduct.GetProductInternalSettings(3);
			try
			{
				bool isCloneUsersProcessEnabledForHOTS = false;

				if (productInternalSettings.Any(s => s.Name.Equals("IsCloneUsersProcessEnabledForHOTS", StringComparison.OrdinalIgnoreCase)))
				{
					isCloneUsersProcessEnabledForHOTS = (productInternalSettings.FirstOrDefault(s => s.Name.Equals("IsCloneUsersProcessEnabledForHOTS", StringComparison.OrdinalIgnoreCase))?.Value == "1");
				}

				if (basePartyId > 0 && clonePartyId > 0 && isCloneUsersProcessEnabledForHOTS)
				{
					ManageCloneProductBatch manageProductBatch = new ManageCloneProductBatch(_defaultUserClaim);
					UPFMProperty upfmProperty = new UPFMProperty();
					var usersToBeCloned = _hotsCloneUserRepository.ListUsers(basePartyId);

					foreach (var user in usersToBeCloned)
					{
						//get user profile
						var profileDetail = getUserProfile(user, basePartyId);
						//get user products
						var userProducts = _hotsCloneUserRepository.GetUserProducts(user.PersonaId);
						// get product batch data
						IPersonaRepository personaRepository = new PersonaRepository();
						var personaProductSettings = personaRepository.GetPersonaProductSettings(user.PersonaId);
						List<ProductBatch> pbData = manageProductBatch.GetUserProductBatchData(user.PersonaId, userProducts, _defaultUserClaim.PersonaId, upfmProperty, personaProductSettings, false, false).ToList();


						//	get base company product properties
						//	get clone company product properties
						//	Compare base assigned properties with clone properties by name
						//	then add it in array list and use it to replcae in batch properties data

						foreach (var productData in pbData)
						{
							var propertyList = productData.InputJson.PropertyList.ToList();
							//find and reaplace baseline customer property with clone customer property
							if (propertyList?.Count > 0)
							{
								var baseCompanyProperties = GetProductProperties(user.AdminUserPersonaId, user.PersonaId, productData.ProductId);
								var cloneCompanyProperties = GetProductProperties(_defaultUserClaim.PersonaId, 0, productData.ProductId);

								var matchedProperties = CompareBaseAndCloneProductProperties(baseCompanyProperties, cloneCompanyProperties);

								if (matchedProperties?.Count > 0)
								{
									productData.InputJson.PropertyList = matchedProperties;
								}
							}
						}

						var hotsuser = _hotsCloneUserRepository.CreateUser(clonePartyId, user, profileDetail, pbData);
						clonedUsers.Users.Add(hotsuser);
					}
					clonedUsers.Status = "Complete";
				
					PostToHOTS(clonedUsers);
				}
				return clonedUsers;
			}
			catch (Exception ex)
			{
				WriteToLog(LogEventLevel.Error,
					   $"{GetType()} - Error while cloning users for." +
					   $" Clone Customer Company {cloneUsers.CloneCustomerUPFMId} , " +
					   $" BaseLine Customer Company PartyId {basePartyId}", exception: ex);
				return clonedUsers;
			}

		}

		public Guid GetBaseCompanyUPFMId(Guid cloneUpfmId)
		{
			return _hotsCloneUserRepository.GetBaseCompanyUPFMId(cloneUpfmId);
		}

		private IProfileDetail getUserProfile(BaseLineCustomerCompanyUser user, long partyId)
		{
			var profileLogic = new ManageProfile(_defaultUserClaim);
			IProfileDetail profileDetail = new ProfileDetail();
			profileDetail = profileLogic.GetProfileDetail(user.UserRealPageId, partyId);

			return profileDetail;
		}

		/// <summary>
		/// Used to write to the log
		/// </summary>
		private void WriteToLog(LogEventLevel logType, string message, Dictionary<string, object> logData = null, Exception exception = null)
		{
			try
			{
				string correlationId = "";
				if (_defaultUserClaim != null)
				{
					correlationId = (_defaultUserClaim.CorrelationId != Guid.Empty) ? _defaultUserClaim.CorrelationId.ToString() : "";
				}
				var logger = Log.Logger;
				if (logData?.Keys != null)
				{
					logger = logger.ForContext("AdditionalInfo", JsonConvert.SerializeObject(logData, Formatting.Indented), false);
				}
				logger = logger.ForContext("ProductModule", this.GetType());
				logger = logger.ForContext("CorrelationId", correlationId);
				logger.Write(logType, exception, message);
			}
			catch
			{
				/*ignored*/
			}
		}

		private ListResponse GetProductProperties(long editorPersonaId, long userPersonaId, int productId)
		{
			ManageProductPanel manageProductPanel = new ManageProductPanel(_defaultUserClaim);
			var productResult = manageProductPanel.GetProductProperties(editorPersonaId, userPersonaId, productId, null);
			return productResult;
		}

		private List<string> CompareBaseAndCloneProductProperties(ListResponse baseProductProperties, ListResponse cloneProductProperties)
		{
			var matchedProductPropertyIdList = new List<string>();
			if (baseProductProperties.Records.Count > 0 && cloneProductProperties.Records.Count > 0)
			{
				var baseProductPropertyType = baseProductProperties.Records[0].GetType();
				var cloneProductPropertyType = cloneProductProperties.Records[0].GetType();

				if (baseProductPropertyType == typeof(ProductProperty) && cloneProductPropertyType == typeof(ProductProperty))
				{
					var basePropertiesList = baseProductProperties.Records.Cast<ProductProperty>();
					var clonePropertiesList = cloneProductProperties.Records.Cast<ProductProperty>();
					foreach (var property in clonePropertiesList)
					{
						if (basePropertiesList.Any(b => b.Name.Equals(property.Name) && b.IsAssigned == true))
						{
							matchedProductPropertyIdList.Add(property.ID.ToString());
						}
					}
				}
				else if (baseProductPropertyType == typeof(ACProperty) && cloneProductPropertyType == typeof(ACProperty))
				{
					var basePropertiesList = baseProductProperties.Records.Cast<ACProperty>();
					var clonePropertiesList = cloneProductProperties.Records.Cast<ACProperty>();
					foreach (var property in clonePropertiesList)
					{
						if (basePropertiesList.Any(b => b.PropertyName.Equals(property.PropertyName) && b.IsAssigned == true))
						{
							matchedProductPropertyIdList.Add(property.PropertyId.ToString());
						}
					}
				}
				else if (baseProductPropertyType == typeof(AssetGroup) && cloneProductPropertyType == typeof(AssetGroup))
				{
					var basePropertiesList = baseProductProperties.Records.Cast<AssetGroup>();
					var clonePropertiesList = cloneProductProperties.Records.Cast<AssetGroup>();
					foreach (var property in clonePropertiesList)
					{
						if (basePropertiesList.Any(b => b.Name.Equals(property.Name) && b.IsAssigned == true))
						{
							matchedProductPropertyIdList.Add(property.ID.ToString());
						}
					}
				}
				else if (baseProductPropertyType == typeof(OnSiteProperty) && cloneProductPropertyType == typeof(OnSiteProperty))
				{
					var basePropertiesList = baseProductProperties.Records.Cast<AssetGroup>();
					var clonePropertiesList = cloneProductProperties.Records.Cast<AssetGroup>();
					foreach (var property in clonePropertiesList)
					{
						if (basePropertiesList.Any(b => b.Name.Equals(property.Name) && b.IsAssigned == true))
						{
							matchedProductPropertyIdList.Add(property.ID.ToString());
						}
					}
				}
				else if (baseProductPropertyType == typeof(RumPropertyGroup) && cloneProductPropertyType == typeof(RumPropertyGroup))
				{
					var basePropertiesList = baseProductProperties.Records.Cast<RumPropertyGroup>();
					var clonePropertiesList = cloneProductProperties.Records.Cast<RumPropertyGroup>();
					foreach (var property in clonePropertiesList)
					{
						if (basePropertiesList.Any(b => b.Name.Equals(property.Name) && b.IsAssigned == true))
						{
							matchedProductPropertyIdList.Add(property.Id.ToString());
						}
					}
				}
				else if (baseProductPropertyType == typeof(ProductProperties) && baseProductPropertyType == typeof(ProductProperties))
				{
					var basePropertiesList = baseProductProperties.Records.Cast<ProductProperties>();
					var clonePropertiesList = cloneProductProperties.Records.Cast<ProductProperties>();
					foreach (var property in clonePropertiesList)
					{
						if (basePropertiesList.Any(b => b.GetName.Equals(property.GetName) && b.IsAssigned == true))
						{
							matchedProductPropertyIdList.Add(property.GetPropertyId.ToString());
						}
					}
				}
				else if (baseProductPropertyType == typeof(Portfolio) && cloneProductPropertyType == typeof(Portfolio))
				{
					var basePropertiesList = baseProductProperties.Records.Cast<Portfolio>();
					var clonePropertiesList = cloneProductProperties.Records.Cast<Portfolio>();
					foreach (var property in clonePropertiesList)
					{
						if (basePropertiesList.Any(b => b.Name.Equals(property.Name) && b.IsAssigned == true))
						{
							matchedProductPropertyIdList.Add(property.ID.ToString());
						}
					}
				}
			}

			return matchedProductPropertyIdList;
		}

		private void PostToHOTS(ClonedUsers clonedUsers) 
		{
            try
            {
				var ulClientToken = _tokenHelper.GetUnifiedLoginServerToken("hotsapi");
				var productInternalSettingList = GetProductInternalSettings(ProductEnum.UnifiedPlatform);
				var hotsEndpoint = productInternalSettingList.First(a => a.Name.Equals("HOTSCloneUserCallBackEnpoint", StringComparison.OrdinalIgnoreCase)).Value;

				using (var httpClient = new HttpClient())
				{
					httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", ulClientToken);
					httpClient.BaseAddress = new Uri(hotsEndpoint);
					var payloadToPost = JsonConvert.SerializeObject(clonedUsers);
					var request = new HttpRequestMessage
					{
						Method = HttpMethod.Post,
						Content = new StringContent(payloadToPost, Encoding.UTF8, "application/json"),
						RequestUri = new Uri(httpClient.BaseAddress.ToString()),
					};

					Dictionary<string, object> logData = new Dictionary<string, object>() { { "ClonnedUsers", clonedUsers } };
					WriteToLog(LogEventLevel.Information, "Users to be posted to HOTS", logData);

					var response = httpClient.SendAsync(request).Result;
					if (response != null && response.IsSuccessStatusCode)
						WriteToLog(LogEventLevel.Information, "Clonedusers Posted to HOTS succesfully.");
					else
						WriteToLog(LogEventLevel.Information, "Hots callback Failed. Response Message: " + response.Content.ToString());

				}
			}
            catch (Exception ex)
            {
				Dictionary<string, object> logData = new Dictionary<string, object>() { { "Exception", ex.ToString() } };
				WriteToLog(LogEventLevel.Error, "PostToHOTS", logData);
            }
			
		}

		private IList<ProductInternalSetting> GetProductInternalSettings(ProductEnum product)
		{
			var rpcache = new RPObjectCache();
			var cacheKey = $"productInternalSetting_{(int)product}";
			IList<ProductInternalSetting> productInternalSettingList = rpcache.GetFromCache<IList<ProductInternalSetting>>(cacheKey, 600, () =>
			{
				// load from database

				return _productInternalSettingRepository.GetProductInternalSettings((int)product).ToList();
			});

			return productInternalSettingList;
		}
	}
}
