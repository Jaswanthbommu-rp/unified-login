using Newtonsoft.Json;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.ResidentPortal
{
	/// <summary>
	/// Staff user notification settings (optional)
	/// </summary>
	public class Notifications : INotifications
	{
		/// <summary>
		/// Notification flag for front desk instructions
		/// </summary>
		[JsonProperty(PropertyName = "managerFdiViaEmail")]
		public bool managerFdiViaEmail { get; set; } = true;

		/// <summary>
		/// Notification flag for new amenity reservations
		/// </summary>
		[JsonProperty(PropertyName = "amenitiesViaEmail")]
		public bool amenitiesViaEmail { get; set; } = true;

		/// <summary>
		/// Notification flag for service requests submissions and updates
		/// </summary>
		[JsonProperty(PropertyName = "managerMrViaEmail")]
		public bool managerMrViaEmail { get; set; } = true;
	}
}
