using UnifiedLogin.SharedObjects.BlackBook;
using UnifiedLogin.SharedObjects.Product;
using System.Collections.Generic;

namespace UnifiedLogin.SharedObjects.Landing.Product.Ops.Extensions
{
	/// <summary>
	/// 
	/// </summary>
	public static class PropertyExtensions
    {
        /// <summary>
        /// Used to convert a product property into a GreenBook property to be used by the UI
        /// </summary>
        /// <param name="properties">The list of roles to convert</param>
        /// <returns></returns>
        public static IList<ProductProperty> ToGBProperties(this IList<PropertyInstance> properties)
        {
            if (properties == null) return null;
            IList<ProductProperty> results = new List<ProductProperty>();
            foreach (PropertyInstance property in properties)
            {
                if (property.IsActive)
                {
                    results.Add(new ProductProperty
                    {
                        ID = property.PropertyInstanceSourceId,
                        Name = property.PropertyName,
                        State = property.Address.State
                    });
                }
            }
            return results;
        }
    }
}
