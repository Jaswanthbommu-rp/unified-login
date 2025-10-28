using Newtonsoft.Json;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.SharedObjects.Enterprise
{
	/// <summary>
	/// User Product and SAML attributes
	/// </summary>
	public class UserProductSAMLDetail
	{
		/// <summary>
		/// Product Code
		/// </summary>
		[JsonProperty(PropertyName = "productCode")]
		public string ProductCode { get; set; }

		/// <summary>
		/// Product User Id
		/// </summary>
		[JsonProperty(PropertyName = "id")]
		public string Id { get; set; }

		/// <summary>
		/// Product UserName
		/// </summary>
		[JsonProperty(PropertyName = "userName")]
		public string UserName { get; set; }
	}
}
