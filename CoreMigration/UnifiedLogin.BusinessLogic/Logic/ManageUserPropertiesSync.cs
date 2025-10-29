using Newtonsoft.Json;
using UnifiedLogin.DataAccess;
using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.BusinessLogic.Logic.Product;
using UnifiedLogin.BusinessLogic.Logic.ProductIntegration.Model;
using UnifiedLogin.BusinessLogic.Repository;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.BlackBook;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product;
using UnifiedLogin.SharedObjects.Product.Accounting;
using UnifiedLogin.SharedObjects.Product.Ops;
using UnifiedLogin.SharedObjects.Product.Rum;
using Serilog;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.Caching;

namespace UnifiedLogin.BusinessLogic.Logic
{
    public class ManageUserPropertiesSync : IManageUserPropertiesSync
    {       
        private IProductInternalSettingRepository _productInternalSettingRepository;
        private IProductRepository _productRepository;
        private IPropertyRepository _propertyRepository;
        private IManageBlueBook _manageBlueBook;
        private IManageProductPanel _manageProductPanel;       
        private DefaultUserClaim _defaultUserClaim;

        const int CacheTimeSeconds = 300;
        ObjectCache _manageSettingCache = MemoryCache.Default;

        #region Constructors

        /// <summary>
        /// Unit Test Constructor
        /// </summary>
        public ManageUserPropertiesSync(IRepository repository, DefaultUserClaim userClaim, HttpMessageHandler messageHandler)
        {
            _defaultUserClaim = userClaim;            
            _productInternalSettingRepository = new ProductInternalSettingRepository(repository);
            _productRepository = new ProductRepository(repository, userClaim);
            _manageBlueBook = new ManageBlueBook(_defaultUserClaim, repository, _productInternalSettingRepository, messageHandler);
            _manageProductPanel = new ManageProductPanel(_defaultUserClaim, repository, _manageBlueBook, messageHandler, null);
            _propertyRepository = new PropertyRepository(repository);                   
        }

        /// <summary>
        /// Create a basic instance of the ManageUserPropertiesSync Controller class
        /// </summary>
        public ManageUserPropertiesSync(DefaultUserClaim userClaim)
        {           
            _productInternalSettingRepository = new ProductInternalSettingRepository();
            _productRepository = new ProductRepository();
            _propertyRepository = new PropertyRepository();           
            _manageBlueBook = new ManageBlueBook(userClaim);
            _defaultUserClaim = userClaim;           
            _manageProductPanel = new ManageProductPanel(userClaim);                   
        }

        #endregion

        public RepositoryResponse TranslateAndSaveUserProductProperties(UserSyncJobTask userData)
        {
            var logData = new Dictionary<string, object> { { "UserSyncData", userData } };
            WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", logData, null, messageProperties: new object[] { "TranslateAndSaveUserProductProperties", "Beginning Translate And Save User Product Properties" });

            RepositoryResponse repositoryResponse = new RepositoryResponse();
            var productList = _productRepository.GetAllProducts();
            int productId = userData.ProductId;                

            var translatedData = TranslateCompareProductPropertiesToUPFM(userData.UserOrgRealpageId,userData.PersonaId,productId);
            var logtranslatedData = new Dictionary<string, object> { { "UserSyncTranslatedData", translatedData } };
            WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", logtranslatedData, null, new object[] { "TranslateAndSaveUserProductProperties", "Translated User Product Properties" });
            if (translatedData?.Count > 0)
            {
                string propertiesForProductJSON = JsonConvert.SerializeObject(translatedData);
                repositoryResponse = _propertyRepository.StageUserProductPrimaryProperties(propertiesForProductJSON, userData.PersonaId, productId, _defaultUserClaim.UserId);
                if (repositoryResponse.Id > 0)
                {
                    WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", null, null, new object[] { "TranslateAndSaveUserProductProperties", "Translated User Product Properties Saved" });
                }
            }
            else
            {
                repositoryResponse = _propertyRepository.DeleteStagedUserProductPrimaryProperties(userData.PersonaId, productId);
            }
            WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", null, null, new object[] { "TranslateAndSaveUserProductProperties", "End Translate And Save User Product Properties" });
            return repositoryResponse;
        }
        private List<SuggestedProperty> TranslateCompareProductPropertiesToUPFM(Guid companyInstanceId,long userPersonaId, int productId)
        {
            var propertiesStagingData = new List<SuggestedProperty>();
            var productResult = _manageProductPanel.GetProductProperties(_defaultUserClaim.PersonaId, userPersonaId, productId, null);
            if (productResult.Records != null && productResult.Records.Count > 0)
            {
                var upfmProperties = new UPFMProperty();
                var instanceIds = new List<string>();
                var instanceGuids = new List<Guid>();
                var booksPropertyInstance = _manageBlueBook.GetPropertyInstanceForCompany(companyInstanceId);
                if (booksPropertyInstance != null)
                {
                    foreach (var property in booksPropertyInstance)
                    {
                        instanceIds.Add(property.attributes.propertyInstanceSourceId.ToLower());
                        instanceGuids.Add(new Guid(property.attributes.propertyInstanceSourceId));
                    }
                }
                upfmProperties.id = instanceIds;

                var booksProductDetail = _productRepository.GetBooksMasterProductDetail(productId);
               
                TranslatePropertyInstance translatedData = new TranslatePropertyInstance();

                if (booksProductDetail.ProductId != (int)ProductEnum.UnifiedPlatform)
                {
                    if (string.IsNullOrEmpty(booksProductDetail.UDMSourceCode))
                    {
                        translatedData = _manageBlueBook.GetTranslatePropertiesFromUPFMToProductv3(upfmProperties, booksProductDetail.BooksProductCode);
                    }
                    else
                    {
                        translatedData = _manageBlueBook.GetTranslatePropertiesFromUPFMToProductv3(upfmProperties, booksProductDetail.UDMSourceCode);
                    }
                }
               
                var productPropertyType = productResult.Records[0].GetType();
                var instanceExists = new TranslatePropertyInstanceAttribute();
                if (productPropertyType == typeof(ProductProperty))
                {
                    var productList = productResult.Records.Cast<ProductProperty>();
                    
                    foreach (var property in productList)
                    {                        
                        if (property.IsAssigned == true)
                        {
                            instanceExists = translatedData.Data?.Attributes.FirstOrDefault(p => p.TranslatedPropertyInstances.Any(o => o.PropertyInstanceSourceId == property.ID));
                            if (instanceExists == null)
                            {
                                instanceExists = translatedData.Data?.Attributes.FirstOrDefault(o => o.PropertyInstanceSourceId == property.InstanceId);
                            }
                            propertiesStagingData.Add(new SuggestedProperty
                            {
                                ProductPropertyId = property.ID,
                                PropertyInstanceId = instanceExists != null ? Guid.Parse(instanceExists.PropertyInstanceSourceId) : Guid.Empty,
                                PropertyName = property.Name
                            });
                            instanceExists = null;
                        }                        
                    }
                    
                }
                else if (productPropertyType == typeof(ACProperty))
                {
                    foreach (var property in productResult.Records.Cast<ACProperty>())
                    {
                        if (property.IsAssigned == true)
                        {
                            instanceExists = translatedData.Data?.Attributes.FirstOrDefault(p => p.TranslatedPropertyInstances.Any(o => o.PropertyInstanceSourceId == property.BookID));
                            if (instanceExists == null)
                            {
                                instanceExists = translatedData.Data?.Attributes.FirstOrDefault(o => o.PropertyInstanceSourceId == property.InstanceId);
                            }
                            propertiesStagingData.Add(new SuggestedProperty
                            {
                                ProductPropertyId = property.Id,
                                PropertyInstanceId = instanceExists != null ? Guid.Parse(instanceExists.PropertyInstanceSourceId) : Guid.Empty,
                                PropertyName = property.PropertyName
                            });
                            instanceExists = null;
                        }
                    }
                }
                else if (productPropertyType == typeof(AssetGroup))
                {
                    foreach (var property in productResult.Records.Cast<AssetGroup>())
                    {                        
                        if (property.IsAssigned == true)
                        {
                            instanceExists = translatedData.Data?.Attributes.FirstOrDefault(p => p.TranslatedPropertyInstances.Any(o => o.PropertyInstanceSourceId == property.AssetID));
                            if (instanceExists == null)
                            {
                                instanceExists = translatedData.Data?.Attributes.FirstOrDefault(o => o.PropertyInstanceSourceId == property.InstanceId);
                            }
                            propertiesStagingData.Add(new SuggestedProperty
                            {
                                ProductPropertyId = property.AssetID,
                                PropertyInstanceId = instanceExists != null ? Guid.Parse(instanceExists.PropertyInstanceSourceId) : Guid.Empty,
                                PropertyName = property.Name
                            });
                            instanceExists = null;
                        }
                    }
                }
                else if (productPropertyType == typeof(OnSiteProperty))
                {
                    foreach (var property in productResult.Records.Cast<OnSiteProperty>())
                    {
                        if (property.IsAssigned == true)
                        {
                            instanceExists = translatedData.Data?.Attributes.FirstOrDefault(p => p.TranslatedPropertyInstances.Any(o => o.PropertyInstanceSourceId == property.GetPropertyId.ToString()));
                            if (instanceExists == null)
                            {
                                instanceExists = translatedData.Data?.Attributes.FirstOrDefault(o => o.PropertyInstanceSourceId == property.InstanceId);
                            }
                            propertiesStagingData.Add(new SuggestedProperty
                            {
                                ProductPropertyId = property.GetPropertyId.ToString(),
                                PropertyInstanceId = instanceExists != null ? Guid.Parse(instanceExists.PropertyInstanceSourceId) : Guid.Empty,
                                PropertyName = property.GetName
                            });
                            instanceExists = null;
                        }
                    }
                }
                else if (productPropertyType == typeof(RumPropertyGroup))
                {
                    foreach (var property in productResult.Records.Cast<RumPropertyGroup>())
                    {
                        if (property.IsAssigned == true)
                        {
                            instanceExists = translatedData.Data?.Attributes.FirstOrDefault(p => p.TranslatedPropertyInstances.Any(o => o.PropertyInstanceSourceId == property.Id.ToString()));
                            if (instanceExists == null)
                            {
                                instanceExists = translatedData.Data?.Attributes.FirstOrDefault(o => o.PropertyInstanceSourceId == property.InstanceId);
                            }
                            propertiesStagingData.Add(new SuggestedProperty
                            {
                                ProductPropertyId = property.Id.ToString(),
                                PropertyInstanceId = instanceExists != null ? Guid.Parse(instanceExists.PropertyInstanceSourceId) : Guid.Empty,
                                PropertyName = property.Name
                            });
                            instanceExists = null;
                        }
                    }
                }
                else if (productPropertyType == typeof(ProductProperties))
                {
                    foreach (var property in productResult.Records.Cast<ProductProperties>())
                    {
                        if (property.IsAssigned == true)
                        {
                            instanceExists = translatedData.Data?.Attributes.FirstOrDefault(p => p.TranslatedPropertyInstances.Any(o => o.PropertyInstanceSourceId == property.GetPropertyId));
                            if (instanceExists == null)
                            {
                                instanceExists = translatedData.Data?.Attributes.FirstOrDefault(o => o.PropertyInstanceSourceId == property.InstanceId);
                            }
                            propertiesStagingData.Add(new SuggestedProperty
                            {
                                ProductPropertyId = property.GetPropertyId,
                                PropertyInstanceId = instanceExists != null ? Guid.Parse(instanceExists.PropertyInstanceSourceId) : Guid.Empty,
                                PropertyName = property.GetName
                            });
                            instanceExists = null;
                        }
                    }
                }
                else if (productPropertyType == typeof(Portfolio))
                {
                    foreach (var property in productResult.Records.Cast<Portfolio>())
                    {
                        if (property.IsAssigned == true)
                        {
                            instanceExists = translatedData.Data?.Attributes.FirstOrDefault(p => p.TranslatedPropertyInstances.Any(o => o.PropertyInstanceSourceId == property.ID));
                            if (instanceExists == null)
                            {
                                instanceExists = translatedData.Data?.Attributes.FirstOrDefault(o => o.PropertyInstanceSourceId == property.InstanceId);
                            }
                            propertiesStagingData.Add(new SuggestedProperty
                            {
                                ProductPropertyId = property.ID,
                                PropertyInstanceId = instanceExists != null ? Guid.Parse(instanceExists.PropertyInstanceSourceId) : Guid.Empty,
                                PropertyName = property.Name
                            });
                            instanceExists = null;
                        }
                    }
                }

            }
            return propertiesStagingData;
        }

        private List<ProductInternalSetting> GetProductInternalSettingList()
        {
            var rpcache = new RPObjectCache();
            var cacheKey = $"productInternalSetting_{(int)ProductEnum.UnifiedPlatform}";
            return rpcache.GetFromCache(cacheKey, 120, () => _productInternalSettingRepository.GetProductInternalSettings((int)ProductEnum.UnifiedPlatform));
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
                var productInternalSettingList = GetProductInternalSettingList();
                string logSettings = null;
                if (productInternalSettingList != null)
                {
                    logSettings = productInternalSettingList.FirstOrDefault(p => p.Name.Equals("Elk_LogManageUserPropertiesSync", StringComparison.OrdinalIgnoreCase))?.Value;
                }

                if (logSettings != "1" && exception == null) return;

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
    }
}
