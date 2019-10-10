using Newtonsoft.Json;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.ResidentPortal
{
	/// <summary>
	/// Access level
	/// </summary>
	public class Level : ILevel
	{
		/// <summary>
		/// level Id
		/// </summary>
		[JsonProperty(PropertyName = "Id")]
		public string Id { get; set; }

		/// <summary>
		/// Level Name
		/// </summary>
		[JsonProperty(PropertyName = "Name")]
		public string Name { get; set; }

		/// <summary>
		/// Is a Level (Role) selected
		/// </summary>
		[JsonProperty(PropertyName = "IsAssigned")]
		public bool IsAssigned { get; set; } = false;

        /// <summary>
		/// Is a Level (Role) Disabled for selection
		/// </summary>
		[JsonProperty(PropertyName = "IsDisabled")]
        public bool IsDisabled { get; set; } = false;
    }
}
