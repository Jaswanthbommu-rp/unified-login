namespace UnifiedLogin.SharedObjects.Product.ResidentPortal
{
	/// <summary>
	/// Interface for Access level
	/// </summary>
	public interface ILevel
	{
		/// <summary>
		/// level Id
		/// </summary>
		string Id { get; set; }

		/// <summary>
		/// level Name
		/// </summary>		
		string Name { get; set; }

		/// <summary>
		/// Is a Level (Role) selected
		/// </summary>
		bool IsAssigned { get; set; }

        /// <summary>
		/// Is a Level (Role) disabled for selection
		/// </summary>
		bool IsDisabled { get; set; } 
    }
}