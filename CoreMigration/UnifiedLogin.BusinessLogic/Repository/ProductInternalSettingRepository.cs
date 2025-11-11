using Dapper;
using UnifiedLogin.DataAccess;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Constants;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.IdentityConfig;
using Serilog;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Data;
using Newtonsoft.Json;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Landing.Enum;

namespace UnifiedLogin.BusinessLogic.Repository
{
    /// <summary>
    /// Used to get the internal settings for a product
    /// </summary>
    public class ProductInternalSettingRepository : BaseRepository, IProductInternalSettingRepository
    {
        #region Ctor
        /// <summary>
        /// SAML base Constructor
        /// </summary>
        public ProductInternalSettingRepository() : base(DbConnectionEnum.IdpConfigurationDb)
        {
        }
        /// <summary>
        /// SAML base Constructor
        /// </summary>
        /// <param name="userClaim"></param>
        public ProductInternalSettingRepository(DefaultUserClaim userClaim) : base(DbConnectionEnum.IdpConfigurationDb)
        {

        }

        /// <summary>
        /// Unit test constructor
        /// </summary>
        /// <param name="repository"></param>
        public ProductInternalSettingRepository(IRepository repository) : base(repository)
        {
        }
        #endregion

        /// <summary>
        /// Get the product settings by product id
        /// </summary>
        /// <param name="productId">ProductId</param>
        /// <returns>list product settings</returns>
        public List<ProductInternalSetting> GetProductInternalSettings(int productId)
        {
            using (var repo = GetRepository())
            {
                dynamic param = new { ProductId = productId };
                return repo.GetMany<ProductInternalSetting>(StoredProcNameConstants.SP_ListGlobalSettingsForProduct, param);
            }
        }

        /// <summary>
        /// Used to get all internal settings by product setting type
        /// </summary>
        /// <param name="productSettingType"></param>
        /// <returns></returns>
        public IList<ProductInternalSettingByType> GetProductSettingByType(string productSettingType)
        {
            RPObjectCache rpcache = new RPObjectCache();
            var cacheKey = "productInternalSettingByType_" + productSettingType;
            var productInternalSettingByTypeList = rpcache.GetFromCache<IList<ProductInternalSettingByType>>(cacheKey, 180, () =>
            {
                using (var repo = GetRepository())
                {
                    dynamic param = new { ProductSettingType = productSettingType };
                    return repo.GetMany<ProductInternalSettingByType>(StoredProcNameConstants.SP_ListProductGlobalSettingsBySettingType, param);
                }
            });
            return productInternalSettingByTypeList;
        }

        /// <summary>
        /// Used to link a product setting to a given configuration
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="productInternalSetting"></param>
        /// <returns></returns>
        public RepositoryResponse CreateProductSettingAndLinkToConfiguration(int productId, ProductInternalSetting productInternalSetting)
        {
            RepositoryResponse response = new RepositoryResponse();
            int productSettingId = 0;
            DateTime utcNow = DateTime.UtcNow;

            using (var repository = GetRepository())
            {
                repository.UnitOfWork.BeginTransaction();
                try
                {
                    // need product setting type
                    int productSettingTypeId = 0;
                    Dapper.DynamicParameters dynparam = new DynamicParameters();
                    dynparam.Add("@Name", productInternalSetting.Name, dbType: DbType.String, direction: ParameterDirection.Input);
                    dynparam.Add("@ProductSettingTypeId", productSettingTypeId, dbType: DbType.Int32, direction: ParameterDirection.Output);

                    try
                    {
                        repository.Execute(StoredProcNameConstants.SP_GetProductSettingType, dynparam);
                        productSettingTypeId = dynparam.Get<int>("@ProductSettingTypeId");
                    }
                    catch
                    {
                    }

                    if (productSettingTypeId == 0)
                    {
                        throw new Exception("Only known product setting types are supported");
                    }

                    dynamic param = new
                    {
                        ProductId = productId,
                        ProductSettingTypeId = productSettingTypeId,
                        Value = productInternalSetting.Value,
                        FromDate = utcNow,
                        ProductSettingId = productSettingId
                    };
                    //CreateProductSetting
                    var repositoryResponse = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_CreateProductSetting, param);
                    Dictionary<string, object> dataLog = new Dictionary<string, object>();
                    dataLog.Add("repositoryResponse", repositoryResponse);
                    WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", dataLog, messageProperties: new object[] { "CreateProductSettingAndLinkToConfiguration", $"Adding setting productid:{productId} ProductSettingTypeId:{productSettingTypeId} Value:{productInternalSetting.Value}" });

                    if (repositoryResponse.Id == 0)
                    {
                        repositoryResponse.ErrorMessage = "CreateProductSettingAndLinkToConfiguration.CreateProductSetting Error: CreateProductSetting failed.";
                        WriteToLog(LogEventLevel.Error, "{ActionName} - {state}", messageProperties: new object[] { "CreateProductSettingAndLinkToConfiguration", "Error: CreateProductSetting failed." });
                    }
                    else
                    {
                        productSettingId = Convert.ToInt32(repositoryResponse.Id);
                        param = new
                        {
                            ConfigurationId = productInternalSetting.ConfigurationId,
                            ProductSettingId = productSettingId,
                        };

                        response = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_LinkProductSettingToConfiguration, param);
                    }
                }
                catch (Exception exception)
                {
                    repository.UnitOfWork.Rollback();
                    response.ErrorMessage = "There was a problem updating the Organization";
                }

                repository.UnitOfWork.Commit();
                return response;
            }
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
            var logger = Log.Logger;
            if (logData?.Keys != null)
            {
                logger = logger.ForContext("AdditionalInfo", JsonConvert.SerializeObject(logData, Formatting.Indented), false);
            }
			logger = logger.ForContext("ProductModule", this.GetType());
            logger = logger.ForContext("CorrelationId", Guid.Empty.ToString());

            logger.Write(level: logType, exception: exception, messageTemplate: message, propertyValue0: messageProperties?[0], propertyValue1: messageProperties?[1]);
        }
    }
}
