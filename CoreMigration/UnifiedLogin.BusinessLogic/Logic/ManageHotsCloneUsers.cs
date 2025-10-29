using Newtonsoft.Json;
using UnifiedLogin.DataAccess;
using UnifiedLogin.BusinessLogic.Logic.Enterprise.Helpers;
using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.BusinessLogic.Logic.Product;
using UnifiedLogin.BusinessLogic.Logic.ProductIntegration.Model;
using UnifiedLogin.BusinessLogic.Logic.ProductIntegration.Model.ClickPay;
using UnifiedLogin.BusinessLogic.Repository;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.BlackBook;
using UnifiedLogin.SharedObjects.Extensions;
using UnifiedLogin.SharedObjects.Hots;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product;
using UnifiedLogin.SharedObjects.Product.Accounting;
using UnifiedLogin.SharedObjects.Product.Ops;
using UnifiedLogin.SharedObjects.Product.ResidentPortal;
using UnifiedLogin.SharedObjects.Product.Rum;
using Serilog;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Web.Security;
using ProductRole = UnifiedLogin.SharedObjects.Product.ProductRole;

namespace UnifiedLogin.BusinessLogic.Logic
{
    public class ManageHotsCloneUsers : IManageHotsCloneUsers
	{
		//step 1 : Get List of users from base line company except employee user
		//step2 : loop through list of users
		// construct user profile object
		// get user assigned products data and replace properties with clone company properties and generate product bach data
		// call sp to create user

		private IProductInternalSettingRepository _productInternalSettingRepository;
		private IHOTSCloneUserRepository _hotsCloneUserRepository;
        private ISamlRepository _samlRepository;
		private DefaultUserClaim _defaultUserClaim;
		private IManageProduct _manageProduct;
		readonly ITokenHelper _tokenHelper;

        private List<ProductInternalSetting> _productInternalSettings;
        #region Ctor

		/// <summary>
		/// Repository test Constructor
		/// </summary>
		public ManageHotsCloneUsers(IRepository repository, DefaultUserClaim userClaim, HttpMessageHandler messageHandler)
		{
			_productInternalSettingRepository = new ProductInternalSettingRepository(repository);
			_hotsCloneUserRepository = new HOTSCloneUserRepository(repository, userClaim);
            _samlRepository = new SamlRepository(repository);
			_manageProduct = new ManageProduct(repository, userClaim, messageHandler);
            _tokenHelper = new TokenHelper(repository);
            _defaultUserClaim = userClaim;
		}

		public ManageHotsCloneUsers(DefaultUserClaim userClaim)
		{
			_productInternalSettingRepository = new ProductInternalSettingRepository();
			_hotsCloneUserRepository = new HOTSCloneUserRepository(userClaim);
            _samlRepository = new SamlRepository();
			_manageProduct = new ManageProduct(userClaim);
            _tokenHelper = new TokenHelper();
			_defaultUserClaim = userClaim;
		}
		#endregion

		public ClonedUsers CloneUsersFromBaseLineCompany(CloneUsers cloneUsers, long basePartyId, long clonePartyId, DefaultUserClaim baseOrgAdminClaim, long baseOrgAdminPersonaId)
		{
			ClonedUsers clonedUsers = new ClonedUsers
            {
                Status = "Incomplete",
                CloneCustomerCompanyId = cloneUsers.CloneCustomerUPFMId,
                CloneCustomerEnvironment = cloneUsers.CloneCustomerEnvironment,
				Users = new List<HotsUser>()
            };

            _productInternalSettings = _manageProduct.GetProductInternalSettings(3);
			try
			{
				bool isCloneUsersProcessEnabledForHOTS = false;

				if (_productInternalSettings.Any(s => s.Name.Equals("IsCloneUsersProcessEnabledForHOTS", StringComparison.OrdinalIgnoreCase)))
				{
					isCloneUsersProcessEnabledForHOTS = (_productInternalSettings.FirstOrDefault(s => s.Name.Equals("IsCloneUsersProcessEnabledForHOTS", StringComparison.OrdinalIgnoreCase))?.Value == "1");
				}

				if (basePartyId > 0 && clonePartyId > 0 && baseOrgAdminPersonaId  > 0 && isCloneUsersProcessEnabledForHOTS)
				{
					ManageCloneProductBatch manageProductBatch = new ManageCloneProductBatch(baseOrgAdminClaim);
					UPFMProperty upfmProperty = new UPFMProperty();
					var usersToBeCloned = _hotsCloneUserRepository.ListUsers(basePartyId);

					foreach (var user in usersToBeCloned)
					{
						//get user profile
						var profileDetail = getUserProfile(user, basePartyId);
                        if (CheckIfUserAlreadyExists(clonePartyId, profileDetail, clonedUsers)) continue;

                        //get user products
                        var userProducts = _hotsCloneUserRepository.GetUserProducts(user.PersonaId);
						// get product batch data
						IPersonaRepository personaRepository = new PersonaRepository(_defaultUserClaim);
						var personaProductSettings = personaRepository.GetPersonaProductSettings(user.PersonaId);
						List<ProductBatch> pbData = manageProductBatch.GetUserProductBatchData(user.PersonaId, userProducts, baseOrgAdminPersonaId, upfmProperty, personaProductSettings, false).ToList();
                        
						//	get base company product properties
						//	get clone company product properties
						//	Compare base assigned properties with clone properties by name
						//	then add it in array list and use it to replace in batch properties data

						foreach (var productData in pbData)
						{
							var propertyList = productData.InputJson.PropertyList.ToList();
							//find and replace baseline customer property with clone customer property
							if (propertyList?.Count > 0)
							{
								var baseCompanyProperties = GetProductProperties(baseOrgAdminClaim, user.AdminUserPersonaId, user.PersonaId, productData.ProductId);
								var cloneCompanyProperties = GetProductProperties(_defaultUserClaim, _defaultUserClaim.PersonaId, 0, productData.ProductId);

								var matchedProperties = CompareBaseAndCloneProductProperties(baseCompanyProperties, cloneCompanyProperties);

								if (matchedProperties?.Count > 0)
								{
									productData.InputJson.PropertyList = matchedProperties;
								}
							}

                            var roleList = productData.InputJson.RoleList.ToList();
                            if (roleList?.Count > 0)
                            {
                                var baseCompanyRoles = GetProductRoles(baseOrgAdminClaim, user.AdminUserPersonaId, user.PersonaId, basePartyId, productData.ProductId);
                                var cloneCompanyRoles = GetProductRoles(_defaultUserClaim, _defaultUserClaim.PersonaId, 0, clonePartyId, productData.ProductId);

                                var matchedRoles = CompareBaseAndCloneProductRoles(baseCompanyRoles, cloneCompanyRoles);
                                if (matchedRoles?.Count > 0)
                                {
                                    productData.InputJson.RoleList = matchedRoles;
                                }
							}
                        }

                        var userLogin = new UserLogin() { Password = Membership.GeneratePassword(10, 1) };
                        var passwordDetail = userLogin.Password.PasswordHash();
                        userLogin.PasswordHash = passwordDetail.PasswordHash;
                        userLogin.PasswordSalt = passwordDetail.PasswordSalt;

                        var hotsuser = _hotsCloneUserRepository.CreateUser(_defaultUserClaim, clonePartyId, user, profileDetail, pbData, userLogin);
                        if (hotsuser != null)
                        {
                            pbData?.ForEach( pb =>
                            {
                                hotsuser.CloneProducts.Add(pb.ProductId);
                            });
                            clonedUsers.Users.Add(hotsuser);
                        }
                    }

                    clonedUsers.Status = "Complete";
                    CheckUsersProductStatus(clonedUsers);

                    PostToHOTS(clonedUsers);
				}
				return clonedUsers;
			}
			catch (Exception ex)
            {
                WriteToLog(LogEventLevel.Error,
                    "{ActionName} - {state}", exception: ex, messageProperties: new object[] { "CloneUsersFromBaseLineCompany", $"Error while cloning users for Clone Company PartyId {clonePartyId} , BaseLine Company PartyId {basePartyId}" });

                return clonedUsers;
			}
        }

        private bool CheckIfUserAlreadyExists(long clonePartyId, IProfileDetail profileDetail, ClonedUsers clonedUsers)
        {
            var cloneLoginName = getLoginName(clonePartyId, profileDetail);

            UserLoginOnly userLoginOnly = _hotsCloneUserRepository.GetUserLoginOnly(cloneLoginName);
            if (userLoginOnly != null)
            {
                HotsUser existingUser = new HotsUser()
                {
                    BaselineUserId = profileDetail.userLogin.UserId,
                    BaselineUserName = profileDetail.userLogin.LoginName,
                    CloneUserId = userLoginOnly.UserId,
                    CloneUserName = userLoginOnly.LoginName
                };
                var personaList = _hotsCloneUserRepository.ListPersona(userLoginOnly.RealPageId);
                if (personaList != null)
                {
                    existingUser.ClonePersonaId = personaList.First(p => p.OrganizationPartyId == clonePartyId).PersonaId;
                }

                clonedUsers.Users.Add(existingUser);
                return true;
            }

            return false;
        }

        private void CheckUsersProductStatus(ClonedUsers clonedUsers)
        {
            var retry = 5;
            var statusCheckSleep = 2000;

            var retrySetting = _productInternalSettings.FirstOrDefault(a => a.Name.Equals("HOTSCheckUserProductStatusRetryCount", StringComparison.OrdinalIgnoreCase))?.Value;
            var statusCheckSleepSetting = _productInternalSettings.FirstOrDefault(a => a.Name.Equals("HOTSCheckUserProductStatusSleepTimeout", StringComparison.OrdinalIgnoreCase))?.Value;
            var productsToExclude = _productInternalSettings.FirstOrDefault(a => a.Name.Equals("HOTSCheckUserExcludeProductIds", StringComparison.OrdinalIgnoreCase))?.Value;

            if (retrySetting != null)
            {
                retry = Convert.ToInt16(retrySetting);
            }

            if (statusCheckSleepSetting != null)
            {
                statusCheckSleep = Convert.ToInt32(statusCheckSleepSetting);
            }

            clonedUsers?.Users?.ForEach(user => {
                foreach (var userCloneProductId in user.CloneProducts.ToArray())
                {
                    if (productsToExclude.Split(',').Contains(userCloneProductId.ToString()))
                    {
                        var remove = user.CloneProducts.Find(f => f == userCloneProductId);
                        user.CloneProducts.Remove(remove);
                    }
                }
            });

            var productsToValidate = 0;
            clonedUsers?.Users?.ForEach(user => { productsToValidate += user.CloneProducts.Count; });
            
            while (retry >= 0)
            {
                clonedUsers?.Users?.ForEach(user =>
                {
                    // wait for all the products to be done creating
                    if (user.CloneProducts.Count > 0)
                    {
                        System.Threading.Thread.Sleep(statusCheckSleep);
                        var clonedUserProductStatus = _samlRepository.ListAllProductsByPersonaId(user.ClonePersonaId, 0, "");
                        foreach (var userCloneProductId in user.CloneProducts.ToArray())
                        {
                            if (clonedUserProductStatus.Any(p => p.ProductId == userCloneProductId))
                            {
                                if (clonedUserProductStatus.First(p => p.ProductId == userCloneProductId).ProductStatus == 8)
                                {
                                    var remove = user.CloneProducts.Find(f => f == userCloneProductId);
                                    user.CloneProducts.Remove(remove);
                                    productsToValidate--;
                                }
                            }
                        }
                    }
                });
                if (productsToValidate == 0)
                {
                    break;
                }

                retry--;
                if (retry == 0)
                {
                    clonedUsers.Status = "Incomplete";
                    break;
                }
            }
        }

        /// <summary>
        /// Used to link a cloned company to a baseline company when using HOTS
        /// </summary>
        /// <param name="baselineCompanyRealPageId"></param>
        /// <param name="cloneCompanyRealPageId"></param>
        /// <param name="userId"></param>
		public RepositoryResponse InsertHotsCompanyRelationship(Guid baselineCompanyRealPageId, Guid cloneCompanyRealPageId, int userId)
        {
            return _hotsCloneUserRepository.InsertHotsCompanyRelationship(baselineCompanyRealPageId, cloneCompanyRealPageId, userId);
        }

        /// <summary>
        /// Used to link a cloned property to a baseline property when using HOTS
        /// </summary>
        /// <param name="baselinePropertyInstanceId"></param>
        /// <param name="clonePropertyInstanceId"></param>
        /// <param name="cloneCompanyRealPageId"></param>
        /// <param name="userId"></param>
        public RepositoryResponse InsertHotsPropertyRelationship(Guid baselinePropertyInstanceId, Guid clonePropertyInstanceId, Guid cloneCompanyRealPageId, int userId)
        {
            return _hotsCloneUserRepository.InsertHotsPropertyRelationship(baselinePropertyInstanceId, clonePropertyInstanceId, cloneCompanyRealPageId, userId);
        }


		public Guid GetBaseCompanyUPFMId(Guid cloneUpfmId)
		{
			return _hotsCloneUserRepository.GetBaseCompanyUPFMId(cloneUpfmId);
		}

        private string getLoginName(long partyId, IProfileDetail baseUserProfile)
        {
            return string.Concat(baseUserProfile.FirstName.Substring(0), baseUserProfile.LastName, partyId.ToString(), "@realpage.com");
        }

        private IProfileDetail getUserProfile(BaseLineCustomerCompanyUser user, long partyId)
		{
			var profileLogic = new ManageProfile(_defaultUserClaim);
			IProfileDetail profileDetail = new ProfileDetail();
			profileDetail = profileLogic.GetProfileDetail(user.UserRealPageId, partyId);

			return profileDetail;
		}

        /// <summary>
        /// Used to write to the central log
        /// </summary>
        /// <param name="logType">Log Type</param>
        /// <param name="message">Message template</param>
        /// <param name="logData">Dictionary of additional properties to log</param>
        /// <param name="exception">Exception details</param>
        /// <param name="messageProperties">Message properties</param>
        private void WriteToLog(LogEventLevel logType, string message, Dictionary<string, object> logData = null, Exception exception = null, object[] messageProperties = null)
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

                logger.Write(level: logType, exception: exception, messageTemplate: message, propertyValue0: messageProperties?[0], propertyValue1: messageProperties?[1]);
            }
			catch
			{
				/*ignored*/
			}
		}

		private ListResponse GetProductProperties(DefaultUserClaim userClaim, long editorPersonaId, long userPersonaId, int productId)
		{
			ManageProductPanel manageProductPanel = new ManageProductPanel(userClaim);
			var productResult = manageProductPanel.GetProductProperties(editorPersonaId, userPersonaId, productId, null);
			return productResult;
		}

        private ListResponse GetProductRoles(DefaultUserClaim userClaim, long editorPersonaId, long userPersonaId, long partyId, int productId)
        {
            ManageProductPanel manageProductPanel = new ManageProductPanel(userClaim);
            var productResult = manageProductPanel.GetProductRoles(editorPersonaId, userPersonaId, partyId, productId, null, null);
            return productResult;
        }

        private List<string> CompareBaseAndCloneProductRoles(ListResponse baseCompanyRoles, ListResponse cloneCompanyRoles)
        {
            var matchedProductRoleIdList = new List<string>();
            if (!(baseCompanyRoles.Records?.Count > 0) || !(cloneCompanyRoles.Records?.Count > 0)) return matchedProductRoleIdList;

            var baseProductRoleType = baseCompanyRoles.Records[0].GetType();
            var cloneProductRoleType = cloneCompanyRoles.Records[0].GetType();

            if (baseProductRoleType == typeof(ProductRole) && cloneProductRoleType == typeof(ProductRole))
            {
                var baseList = baseCompanyRoles.Records.Cast<ProductRole>();
                var cloneList = cloneCompanyRoles.Records.Cast<ProductRole>();
                foreach (var role in baseList)
                {
                    if (!role.IsAssigned) continue;

                    var foundRole = cloneList.FirstOrDefault(b => b.Name.Equals(role.Name, StringComparison.OrdinalIgnoreCase) && b.Roletype.Equals(role.Roletype, StringComparison.OrdinalIgnoreCase));
                    if (foundRole != null)
                    {
                        matchedProductRoleIdList.Add(foundRole.ID);
                    }
                }
            }
            else if(baseProductRoleType == typeof(ClickPayRole) && cloneProductRoleType == typeof(ClickPayRole))
            {
                var baseList = baseCompanyRoles.Records.Cast<ClickPayRole>();
                var cloneList = cloneCompanyRoles.Records.Cast<ClickPayRole>();
                foreach (var role in baseList)
                {
                    if (!role.IsAssigned) continue;

                    var foundRole = cloneList.FirstOrDefault(b => b.Name.Equals(role.Name, StringComparison.OrdinalIgnoreCase));
                    if (foundRole != null)
                    {
                        matchedProductRoleIdList.Add(foundRole.Id);
                    }
                }
            }
            else if (baseProductRoleType == typeof(ProductIntegration.Model.ProductRole) && cloneProductRoleType == typeof(ProductIntegration.Model.ProductRole))
            {
                var baseList = baseCompanyRoles.Records.Cast<ProductIntegration.Model.ProductRole>();
                var cloneList = cloneCompanyRoles.Records.Cast<ProductIntegration.Model.ProductRole>();
                foreach (var role in baseList)
                {
                    if (!role.IsAssigned) continue;

                    var foundRole = cloneList.FirstOrDefault(b => b.GetName.Equals(role.GetName, StringComparison.OrdinalIgnoreCase));
                    if (foundRole != null)
                    {
                        matchedProductRoleIdList.Add(foundRole.GetRoleId);
                    }
                }
            }
            else if (baseProductRoleType == typeof(Level) && cloneProductRoleType == typeof(Level))
            {
                var baseList = baseCompanyRoles.Records.Cast<ILevel>();
                var cloneList = cloneCompanyRoles.Records.Cast<ILevel>();
                foreach (var role in baseList)
                {
                    if (!role.IsAssigned) continue;

                    var foundRole = cloneList.FirstOrDefault(b => b.Name.Equals(role.Name, StringComparison.OrdinalIgnoreCase));
                    if (foundRole != null)
                    {
                        matchedProductRoleIdList.Add(foundRole.Id);
                    }
                }
            }
            else if (baseProductRoleType == typeof(SharedObjects.Product.Rum.Role) && cloneProductRoleType == typeof(SharedObjects.Product.Rum.Role))
            {
                var baseList = baseCompanyRoles.Records.Cast<SharedObjects.Product.Rum.Role>();
                var cloneList = cloneCompanyRoles.Records.Cast<SharedObjects.Product.Rum.Role>();
                foreach (var role in baseList)
                {
                    if (!role.IsAssigned) continue;

                    var foundRole = cloneList.FirstOrDefault(b => b.Name.Equals(role.Name, StringComparison.OrdinalIgnoreCase));
                    if (foundRole != null)
                    {
                        matchedProductRoleIdList.Add(foundRole.Id.ToString());
                    }
                }
            }

            return matchedProductRoleIdList;
        }
		private List<string> CompareBaseAndCloneProductProperties(ListResponse baseProductProperties, ListResponse cloneProductProperties)
		{
			var matchedProductPropertyIdList = new List<string>();
            if (!(baseProductProperties.Records?.Count > 0) || !(cloneProductProperties.Records?.Count > 0)) return matchedProductPropertyIdList;

            var baseProductPropertyType = baseProductProperties.Records[0].GetType();
            var cloneProductPropertyType = cloneProductProperties.Records[0].GetType();

            if (baseProductPropertyType == typeof(ProductProperty) && cloneProductPropertyType == typeof(ProductProperty))
            {
                var basePropertiesList = baseProductProperties.Records.Cast<ProductProperty>();
                var clonePropertiesList = cloneProductProperties.Records.Cast<ProductProperty>();
                foreach (var property in basePropertiesList)
                {
                    if (property.IsAssigned == true)
                    {
                        var foundProperty = clonePropertiesList.FirstOrDefault(b => b.Name.IndexOf(property.Name, StringComparison.OrdinalIgnoreCase) >= 0);
                        if (foundProperty != null)
                        {
                            matchedProductPropertyIdList.Add(foundProperty.ID);
                        }
                    }
                }
            }
            else if (baseProductPropertyType == typeof(ACProperty) && cloneProductPropertyType == typeof(ACProperty))
            {
                var basePropertiesList = baseProductProperties.Records.Cast<ACProperty>();
                var clonePropertiesList = cloneProductProperties.Records.Cast<ACProperty>();
                foreach (var property in basePropertiesList)
                {
                    if (property.IsAssigned)
                    {
                        var foundProperty = clonePropertiesList.FirstOrDefault(b => b.PropertyName.IndexOf(property.PropertyName, StringComparison.OrdinalIgnoreCase) >= 0);
                        if (foundProperty != null)
                        {
                            matchedProductPropertyIdList.Add(foundProperty.PropertyId);
                        }
                    }
                }
            }
            else if (baseProductPropertyType == typeof(AssetGroup) && cloneProductPropertyType == typeof(AssetGroup))
            {
                var basePropertiesList = baseProductProperties.Records.Cast<AssetGroup>();
                var clonePropertiesList = cloneProductProperties.Records.Cast<AssetGroup>();
                foreach (var property in basePropertiesList)
                {
                    if (!property.IsAssigned) continue;

                    var foundProperty = clonePropertiesList.FirstOrDefault(b => b.Name.IndexOf(property.Name, StringComparison.OrdinalIgnoreCase) >= 0);
                    if (foundProperty != null)
                    {
                        matchedProductPropertyIdList.Add(foundProperty.ID);
                    }
                }
            }
            else if (baseProductPropertyType == typeof(OnSiteProperty) && cloneProductPropertyType == typeof(OnSiteProperty))
            {
                var basePropertiesList = baseProductProperties.Records.Cast<AssetGroup>();
                var clonePropertiesList = cloneProductProperties.Records.Cast<AssetGroup>();
                foreach (var property in basePropertiesList)
                {
                    if (!property.IsAssigned) continue;

                    var foundProperty = clonePropertiesList.FirstOrDefault(b => b.Name.IndexOf(property.Name, StringComparison.OrdinalIgnoreCase) >= 0);
                    if (foundProperty != null)
                    {
                        matchedProductPropertyIdList.Add(foundProperty.ID);
                    }
                }
            }
            else if (baseProductPropertyType == typeof(RumPropertyGroup) && cloneProductPropertyType == typeof(RumPropertyGroup))
            {
                var basePropertiesList = baseProductProperties.Records.Cast<RumPropertyGroup>();
                var clonePropertiesList = cloneProductProperties.Records.Cast<RumPropertyGroup>();
                foreach (var property in basePropertiesList)
                {
                    if (!property.IsAssigned) continue;

                    var foundProperty = clonePropertiesList.FirstOrDefault(b => b.Name.IndexOf(property.Name, StringComparison.OrdinalIgnoreCase) >= 0);
                    if (foundProperty != null)
                    {
                        matchedProductPropertyIdList.Add(foundProperty.Id.ToString());
                    }
                }
            }
            else if (baseProductPropertyType == typeof(ProductProperties) && baseProductPropertyType == typeof(ProductProperties))
            {
                var basePropertiesList = baseProductProperties.Records.Cast<ProductProperties>();
                var clonePropertiesList = cloneProductProperties.Records.Cast<ProductProperties>();
                foreach (var property in basePropertiesList)
                {
                    if (!property.IsAssigned) continue;

                    var foundProperty = clonePropertiesList.FirstOrDefault(b => b.GetName.IndexOf(property.GetName, StringComparison.OrdinalIgnoreCase) >= 0);
                    if (foundProperty != null)
                    {
                        matchedProductPropertyIdList.Add(foundProperty.GetPropertyId);
                    }
                }
            }
            else if (baseProductPropertyType == typeof(Portfolio) && cloneProductPropertyType == typeof(Portfolio))
            {
                var basePropertiesList = baseProductProperties.Records.Cast<Portfolio>();
                var clonePropertiesList = cloneProductProperties.Records.Cast<Portfolio>();
                foreach (var property in basePropertiesList)
                {
                    if (!property.IsAssigned) continue;

                    var foundProperty = clonePropertiesList.FirstOrDefault(b => b.Name.IndexOf(property.Name, StringComparison.OrdinalIgnoreCase) >= 0);
                    if (foundProperty != null)
                    {
                        matchedProductPropertyIdList.Add(foundProperty.ID);
                    }
                }
            }

            return matchedProductPropertyIdList;
		}

 

        private void PostToHOTS(ClonedUsers clonedUsers) 
		{
            try
            {
                var hotsEndpoint = _productInternalSettings.FirstOrDefault(a => a.Name.Equals("HOTSCloneUserCallBackEnpoint", StringComparison.OrdinalIgnoreCase))?.Value;
				var hotsIssuerUri = _productInternalSettings.FirstOrDefault(a => a.Name.Equals("HOTSCloneIssuerUri", StringComparison.OrdinalIgnoreCase))?.Value;
				var hotsClientId = _productInternalSettings.FirstOrDefault(a => a.Name.Equals("HOTSCloneClientId", StringComparison.OrdinalIgnoreCase))?.Value;
				var hotsClientSecret = _productInternalSettings.FirstOrDefault(a => a.Name.Equals("HOTSCloneClientSecret", StringComparison.OrdinalIgnoreCase))?.Value;
                if (!string.IsNullOrEmpty(hotsClientSecret))
                {
                    hotsClientSecret = Encoding.UTF8.GetString(Convert.FromBase64String(hotsClientSecret));
                }
                string ulClientToken = null;

                if (!string.IsNullOrEmpty(hotsClientId) && !string.IsNullOrEmpty(hotsClientSecret))
                {
                    ulClientToken = _tokenHelper.GetExternalClientCredentialServerToken(hotsIssuerUri+ "/connect/token", hotsClientId, hotsClientSecret, "hotsapi");
                }

                if (ulClientToken != null)
                {
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

                        var logData = new Dictionary<string, object>() { { "clonedUsers", clonedUsers } };
                        WriteToLog(LogEventLevel.Information, "{ActionName} - {state}", logData, messageProperties: new object[] { "PostToHOTS", "Users to be posted to HOTS" });

                        var response = httpClient.SendAsync(request).Result;
                        if (response != null && response.IsSuccessStatusCode)
                            WriteToLog(LogEventLevel.Information, "{ActionName} - {state}", messageProperties: new object[] { "PostToHOTS", "Posted to HOTS successfully" });
                        else
                            WriteToLog(LogEventLevel.Information, "{ActionName} - {state}", messageProperties: new object[] { "PostToHOTS", "Hots callback Failed. Response Message: " + response.Content });

                    }
                }
                else
                {
                    WriteToLog(LogEventLevel.Error, "{ActionName} - {state}", messageProperties: new object[] { "PostToHOTS", "Unable to post update to HOTS" });
                }
            }
            catch (Exception ex)
            {
				var logData = new Dictionary<string, object>() { { "Exception", ex.ToString() } };
                WriteToLog(LogEventLevel.Error, exception: ex, message: "{ActionName} - {state}", logData: logData, messageProperties: new object[] { "PostToHOTS", "Error" });
            }
        }
	}
}
