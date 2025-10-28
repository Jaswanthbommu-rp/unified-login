namespace UnifiedLogin.SharedObjects.Product.ResidentPortal
{
	/// <summary>
	/// Interface for Notifications.cs : Staff user notification settings (optional)
	/// </summary>
	public interface INotifications
	{
		/// <summary>
		/// Notification flag for new amenity reservations
		/// </summary>
		bool amenitiesViaEmail { get; set; }

		/// <summary>
		/// Notification flag for front desk instructions
		/// </summary>
		bool managerFdiViaEmail { get; set; }

		/// <summary>
		/// Notification flag for service requests submissions and updates
		/// </summary>
		bool managerMrViaEmail { get; set; }
	}
}