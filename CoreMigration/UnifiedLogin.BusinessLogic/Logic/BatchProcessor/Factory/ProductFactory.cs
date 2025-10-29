using UnifiedLogin.BusinessLogic.Logic.ProductIntegration.ProductImplementation;
using UnifiedLogin.SharedObjects.Enum;
using System;
using System.Collections.Generic;

namespace UnifiedLogin.BusinessLogic.Logic.BatchProcessor.Factory
{
	/// <summary>
	/// Product factory returns instance based on product type
	/// </summary>
	public static class ProductFactory
    {
        private static readonly Dictionary<ProductEnum, Type> ProductFactories =
            new Dictionary<ProductEnum, Type>();

        /// <summary>
        /// Processes to add
        /// </summary>
        static ProductFactory()
        {
            ProductFactories.Add(ProductEnum.LeadAnalytics, typeof(LeadManagement)); 
        }
		 
		/// <summary>
		/// Returns instance of class for execution based on process type
		/// </summary> 
		public static IProduct GetProductLogic(ProductEnum productEnum)
        {
            return (IProduct)Activator.CreateInstance(ProductFactories[productEnum]);
        }
    }
}