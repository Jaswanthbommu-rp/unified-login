using Newtonsoft.Json;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using System.Collections.Generic;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig
{
	/// <summary>
	/// Product Users
	/// </summary>
	public class ProductUsers : Person, IProductUsers
	{
		/// <summary>
		/// UserLogin attributes common to UserLogin and ProductUsers classes
		/// </summary>
		[JsonProperty("UserLogin", NullValueHandling = NullValueHandling.Ignore)]
		public UserLoginCommon userLogin { get; set; } = new UserLoginCommon();

		/// <summary>
		/// List of User Personas
		/// </summary>
		[JsonProperty("Persona", NullValueHandling = NullValueHandling.Ignore)]
		public IList<PersonaCommon> persona { get; set; } = new List<PersonaCommon>();
	}
}
