using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnifiedLogin.SharedObjects.Product
{
	public class ProductPropertyMap
	{
		/// <summary>
		/// The id of the property in the product
		/// </summary>
		public string PropertyId { get; set; }
		/// <summary>
		/// The name of the property in the product
		/// </summary>
		public string PropertyName { get; set; }
		/// <summary>
		/// The state where the property is located
		/// </summary>
		public string State { get; set; }
        /// <summary>
		/// The Status of the property
		/// </summary>
		public string Active { get; set; }
    }
}
