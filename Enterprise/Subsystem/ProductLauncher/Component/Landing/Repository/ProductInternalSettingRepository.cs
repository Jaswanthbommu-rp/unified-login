using Dapper;
using RP.Enterprise.Foundation.DataAccess.Component;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Audit.Common;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Base;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using Serilog;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Data;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository
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
        /// <param name="ProductId">ProductId</param>
        /// <returns>list product settings</returns>
        public IList<ProductInternalSetting> GetProductInternalSettings(int ProductId)
        {
            using (var repo = GetRepository())
            {
                dynamic param = new { ProductId = ProductId };
                return repo.GetMany<ProductInternalSetting>(StoredProcNameConstants.SP_ListGlobalSettingsForProduct, param);
            }
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
                    WriteToLog(LogEventLevel.Debug, $"SP_CreateProductSetting productid:{productId} ProductSettingTypeId:{productSettingTypeId} Value:{productInternalSetting.Value}", dataLog);

                    if (repositoryResponse.Id == 0)
                    {
                        repositoryResponse.ErrorMessage = "CreateProductSettingAndLinkToConfiguration.CreateProductSetting Error: CreateProductSetting failed.";
                        WriteToLog(LogEventLevel.Error, repositoryResponse.ErrorMessage);
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

        private void WriteToLog(LogEventLevel logType, string message, Dictionary<string, object> logData = null, Exception exception = null)
        {
            var logger = Log.Logger;
            foreach (var key in logData?.Keys)
            {
                logger = logger.ForContext($"AdditionalInfo-{key}", logData[key], true);
            }
            logger.Write(logType, exception, message );
        }
    }
}