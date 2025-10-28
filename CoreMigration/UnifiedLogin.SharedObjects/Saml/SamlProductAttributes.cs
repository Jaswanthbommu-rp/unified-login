using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnifiedLogin.SharedObjects.Saml
{
	public class SamlProductAttributes
	{

		/// <summary>
		/// ProductID
		/// </summary>
		public int ProductID { get; set; }

		/// <summary>
		/// The name of the SAML product attribute
		/// </summary>
		public string DisplayName { get; set; }		

		/// <summary>
		/// SamlAttributeName
		/// </summary>
		public string SamlAttributeName { get; set; }
	}
}
