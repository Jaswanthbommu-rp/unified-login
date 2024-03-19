using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.ProductIntegration.Helpers;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.ProductIntegration.ProductImplementation;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using Serilog;
using Serilog.Events;
using System;
using System.Collections.Generic;

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

        // Handles the conversion of int to ProductEnum to ease future removal of ProductEnum references
        public static IManageProductIntegration GetProductLogic(int productType, long editorPersonaId, long subjectPersonaId, DefaultUserClaim userClaims) =>
            GetProductLogic((ProductEnum)productType, editorPersonaId, subjectPersonaId, userClaims);

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
                    Log.Write(LogEventLevel.Error, ex, "{ActionName} - {state}", propertyValues: new object[] { "GetProductLogic", $"Exception in Factory, mostly causes if exception in constructor. Message={ex.Message}" });
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

                    Log.Write(LogEventLevel.Error, ex, message);
                }
                catch (Exception)
                { }

                throw ex;
            }
        }
        #endregion
    }
}