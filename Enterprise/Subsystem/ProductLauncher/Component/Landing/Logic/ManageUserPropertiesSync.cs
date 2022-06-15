using Newtonsoft.Json;
using RP.Enterprise.Foundation.DataAccess.Component;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.ProductIntegration.Model;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.BlackBook;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
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
using System.Threading.Tasks;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic
{
    public class ManageUserPropertiesSync : IManageUserPropertiesSync
    {       
        private IProductInternalSettingRepository _productInternalSettingRepository;
        private IProductRepository _productRepository;
        private IPropertyRepository _propertyRepository;
        private IManageBlueBook _manageBlueBook;
        private IManageProductPanel _manageProductPanel;       
        private DefaultUserClaim _defaultUserClaim;

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
            WriteToLog(LogEventLevel.Debug, $"Beginning Translate And Save User Product Properties", logData);

            RepositoryResponse repositoryResponse = new RepositoryResponse();
            var productList = _productRepository.GetAllProducts();
            int productId = userData.ProductId;                

            var translatedData = TranslateCompareProductPropertiesToUPFM(userData.UserOrgRealpageId,userData.PersonaId,productId);
            var logtranslatedData = new Dictionary<string, object> { { "UserSyncTranslatedData", translatedData } };
            WriteToLog(LogEventLevel.Debug, $"Translated User Product Properties", logtranslatedData);
            if (translatedData?.Count > 0)
            {
                string propertiesForProductJSON = JsonConvert.SerializeObject(translatedData);
                repositoryResponse = _propertyRepository.StageUserProductPrimaryProperties(propertiesForProductJSON, userData.PersonaId, productId, _defaultUserClaim.UserId);
                if (repositoryResponse.Id > 0)
                {
                    WriteToLog(LogEventLevel.Debug, $"Translated User Product Properties Saved");
                }
            }
            WriteToLog(LogEventLevel.Debug, $"End Translate And Save User Product Properties");
            return repositoryResponse;
        }
        private List<SuggestedProperty> TranslateCompareProductPropertiesToUPFM(Guid companyInstanceId,long userPersonaId, int productId)
        {
            var propertiesStagingData = new List<SuggestedProperty>();
            var productResult = _manageProductPanel.GetProductProperties(_defaultUserClaim.PersonaId, userPersonaId, productId, null);
            if (productResult.Records != null)
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
                if (productPropertyType == typeof(ProductProperty))
                {
                    var productList = productResult.Records.Cast<ProductProperty>();
                    foreach (var property in productList)
                    {                        
                        if (property.IsAssigned == true)
                        {
                            var instanceExists = translatedData.Data?.Attributes.FirstOrDefault(p => p.TranslatedPropertyInstances.Any(o => o.PropertyInstanceSourceId == property.ID));
                            propertiesStagingData.Add(new SuggestedProperty
                            {
                                ProductPropertyId = long.Parse(property.ID),
                                PropertyInstanceId = instanceExists != null ? Guid.Parse(instanceExists.PropertyInstanceSourceId) : Guid.Empty,
                                PropertyName = property.Name
                            });
                        }                        
                    }
                }
                else if (productPropertyType == typeof(ACProperty))
                {
                    foreach (var property in productResult.Records.Cast<ACProperty>())
                    {
                        if (property.IsAssigned == true)
                        {
                            var instanceExists = translatedData.Data?.Attributes.FirstOrDefault(p => p.TranslatedPropertyInstances.Any(o => o.PropertyInstanceSourceId == property.Id));
                            propertiesStagingData.Add(new SuggestedProperty
                            {
                                ProductPropertyId = long.Parse(property.Id),
                                PropertyInstanceId = instanceExists != null ? Guid.Parse(instanceExists.PropertyInstanceSourceId) : Guid.Empty,
                                PropertyName = property.PropertyName
                            });
                        }
                    }
                }
                else if (productPropertyType == typeof(AssetGroup))
                {
                    foreach (var property in productResult.Records.Cast<AssetGroup>())
                    {                        
                        if (property.IsAssigned == true)
                        {
                            var instanceExists = translatedData.Data?.Attributes.FirstOrDefault(p => p.TranslatedPropertyInstances.Any(o => o.PropertyInstanceSourceId == property.AssetID));
                            propertiesStagingData.Add(new SuggestedProperty
                            {
                                ProductPropertyId = long.Parse(property.AssetID),
                                PropertyInstanceId = instanceExists != null ? Guid.Parse(instanceExists.PropertyInstanceSourceId) : Guid.Empty,
                                PropertyName = property.Name
                            });
                        }
                    }
                }
                else if (productPropertyType == typeof(OnSiteProperty))
                {
                    foreach (var property in productResult.Records.Cast<OnSiteProperty>())
                    {
                        if (property.IsAssigned == true)
                        {
                            var instanceExists = translatedData.Data?.Attributes.FirstOrDefault(p => p.TranslatedPropertyInstances.Any(o => o.PropertyInstanceSourceId == property.GetPropertyId.ToString()));
                            propertiesStagingData.Add(new SuggestedProperty
                            {
                                ProductPropertyId = long.Parse(property.GetPropertyId.ToString()),
                                PropertyInstanceId = instanceExists != null ? Guid.Parse(instanceExists.PropertyInstanceSourceId) : Guid.Empty,
                                PropertyName = property.GetName
                            });
                        }
                    }
                }
                else if (productPropertyType == typeof(RumPropertyGroup))
                {
                    foreach (var property in productResult.Records.Cast<RumPropertyGroup>())
                    {
                        if (property.IsAssigned == true)
                        {
                            var instanceExists = translatedData.Data?.Attributes.FirstOrDefault(p => p.TranslatedPropertyInstances.Any(o => o.PropertyInstanceSourceId == property.Id.ToString()));
                            propertiesStagingData.Add(new SuggestedProperty
                            {
                                ProductPropertyId = long.Parse(property.Id.ToString()),
                                PropertyInstanceId = instanceExists != null ? Guid.Parse(instanceExists.PropertyInstanceSourceId) : Guid.Empty,
                                PropertyName = property.Name
                            });
                        }
                    }
                }
                else if (productPropertyType == typeof(ProductProperties))
                {
                    foreach (var property in productResult.Records.Cast<ProductProperties>())
                    {
                        if (property.IsAssigned == true)
                        {
                            var instanceExists = translatedData.Data?.Attributes.FirstOrDefault(p => p.TranslatedPropertyInstances.Any(o => o.PropertyInstanceSourceId == property.GetPropertyId));
                            propertiesStagingData.Add(new SuggestedProperty
                            {
                                ProductPropertyId = long.Parse(property.GetPropertyId),
                                PropertyInstanceId = instanceExists != null ? Guid.Parse(instanceExists.PropertyInstanceSourceId) : Guid.Empty,
                                PropertyName = property.GetName
                            });
                        }
                    }
                }
                else if (productPropertyType == typeof(Portfolio))
                {
                    foreach (var property in productResult.Records.Cast<Portfolio>())
                    {
                        if (property.IsAssigned == true)
                        {
                            var instanceExists = translatedData.Data?.Attributes.FirstOrDefault(p => p.TranslatedPropertyInstances.Any(o => o.PropertyInstanceSourceId == property.ID));
                            propertiesStagingData.Add(new SuggestedProperty
                            {
                                ProductPropertyId = long.Parse(property.ID),
                                PropertyInstanceId = instanceExists != null ? Guid.Parse(instanceExists.PropertyInstanceSourceId) : Guid.Empty,
                                PropertyName = property.Name
                            });
                        }
                    }
                }

            }
            return propertiesStagingData;
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
    }
}
