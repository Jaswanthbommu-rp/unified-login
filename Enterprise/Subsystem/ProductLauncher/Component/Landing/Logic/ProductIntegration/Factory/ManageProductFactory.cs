using System;
using System.Collections.Generic;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.ProductIntegration.Helpers;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.ProductIntegration.ProductImplementation;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Audit.Common;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using Serilog;
using Serilog.Events;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.ProductIntegration.Factory
{
    /// <summary>
    /// 
    /// </summary>
    public static class ManageProductFactory
    {
        #region Private Members 

        /// <summary>
        /// Factory to return instance based on process type
        /// </summary>
        private static readonly Dictionary<ProductEnum, Type> Factories =
            new Dictionary<ProductEnum, Type>();

        #endregion

        #region Factory Configuration - CONFIGURE HERE NEW PRODUCT 

        /// <summary>
        /// Processes to add
        /// CONFIGURE HERE NEW PRODUCT 
        /// </summary>
        static ManageProductFactory()
        {
            Factories.Add(ProductEnum.LeadManagement, typeof(LeadManagement));
            Factories.Add(ProductEnum.LeadAnalytics, typeof(LeadManagement));
            Factories.Add(ProductEnum.PortfolioManagement, typeof(PortfolioManagement));
            Factories.Add(ProductEnum.DepositAlternative, typeof(DepositAlternativeManagement));
            Factories.Add(ProductEnum.ClickPay, typeof(ClickPayManagement));
            Factories.Add(ProductEnum.RenovationManager, typeof(RenovationManager));
            Factories.Add(ProductEnum.SeniorLeadManagement, typeof(SeniorLeadManagement));
        }


        #endregion

        #region Public Members - NO CHANGES REQUIRED FOR NEW PRODUCT

        /// <summary>
        /// Returns instance of class for execution based on process type
        /// NO CHANGES REQUIRED FOR NEW PRODUCT
        /// </summary> 
        public static IManageProductIntegration GetProductLogic(ProductEnum productType, long editorPersonaId, long subjectPersonaId, DefaultUserClaim userClaims)
        {
            try
            {
                return (IManageProductIntegration)Activator.CreateInstance(Factories[productType], productType, editorPersonaId, subjectPersonaId, userClaims);
            }
            catch (Exception ex)
            {
                try
                {
                    string message = $"ManageProductFactory.GetProductLogic - Exception in Factory, mostly causes if exception in constructor. Message={ex.Message}";

                    LogDetails logDetails = new LogDetails
                    {
                        Message = message,
                        ProductModule = "ManageProductFactory.GetProductLogic",
                        UserId = userClaims?.UserId.ToString(),
                        PmcId = userClaims?.OrganizationPartyId.ToString(),
                        UserName = userClaims?.LoginName,
                        Exception = ex,
                        CorrelationId = userClaims?.CorrelationId.ToString(),
                    };

                    Log.Write(LogEventLevel.Error, ex, message, logDetails);
                }
                catch (Exception)
                { }

                throw ex;
            }
        }

        /// <summary>
        /// Used for Unit Testing
        /// </summary>
        public static IManageProductIntegration GetProductLogic(ProductEnum productType, long editorPersonaId, long subjectPersonaId, DefaultUserClaim userClaims,
            IDataCollector injectedDataCollector, IManagePersona injectedManagePersona, IProductInternalSettingRepository injectedProductInternalSettingRepository)
        {
            try
            {
                return (IManageProductIntegration)Activator.CreateInstance(Factories[productType], productType, editorPersonaId, subjectPersonaId, userClaims,
                    injectedDataCollector, injectedManagePersona, injectedProductInternalSettingRepository);
            }
            catch (Exception ex)
            {
                try
                {
                    string message = $"ManageProductFactory.GetProductLogic - Exception in Factory, mostly causes if exception in constructor. Message={ex.Message}";

                    LogDetails logDetails = new LogDetails
                    {
                        Message = message,
                        ProductModule = "ManageProductFactory.GetProductLogic",
                        UserId = userClaims?.UserId.ToString(),
                        PmcId = userClaims?.OrganizationPartyId.ToString(),
                        UserName = userClaims?.LoginName,
                        Exception = ex,
                        CorrelationId = userClaims?.CorrelationId.ToString(),
                    };

                    Log.Write(LogEventLevel.Error, ex, message, logDetails);
                }
                catch (Exception)
                { }

                throw ex;
            }
        }
        #endregion
    }
}