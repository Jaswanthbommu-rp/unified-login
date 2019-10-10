using Newtonsoft.Json;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enterprise;
using System.Collections.Generic;

namespace RP.Enterprise.Subsystem.ProductLauncher.Service.LandingAPIEnterprise.Dto
{
	/// <summary>
	/// UserData Data Transform Object for Grt/List
	/// </summary>
	public class UsersDataDto : UserDataDtoCommon
	{
		/// <summary>
		/// Custom Field  
		/// </summary>
		[JsonProperty(PropertyName = "CustomFields")]
		public Dictionary<string, string> CustomFields { get; set; }

		/// <summary>
		/// User Status
		/// </summary>
		[JsonProperty("UserStatus")]
		public string UserStatus { get; set; }

		/// <summary>
		/// UserType Name
		/// </summary>
		[JsonProperty(PropertyName = "UserType")]
		public string UserType { get; set; }

		/// <summary>
		/// List of User Products and SAML attributes
		/// </summary>
		[JsonProperty("Product")]
		public IList<UserProductSAMLDetail> Product { get; set; }
	}
}