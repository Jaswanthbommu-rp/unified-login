namespace UnifiedLogin.SharedObjects.Product.ResidentPortal
{
	/// <summary>
	/// Interface for Root level of JSON object
	/// </summary>
	public interface IDataObject<T>
	{
		/// <summary>
		/// Data level
		/// </summary>
		T data { get; set; }
	}
}