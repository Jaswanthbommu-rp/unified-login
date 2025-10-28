namespace UnifiedLogin.SharedObjects.BlackBook
{
	/// <summary>
	/// 
	/// </summary>
	public class CustomerCompanyPropertyMap
	{
		public string Id { get; set; }
		/// <summary>
		/// 
		/// </summary>
		public int CustomerCompanyId { get; set; }
		/// <summary>
		/// 
		/// </summary>
		public int CustomerPropertyId { get; set; }
		/// <summary>
		/// 
		/// </summary>
		public string MapType { get; set; }
		/// <summary>
		/// 
		/// </summary>
		public string PropertyName { get; set; }
		/// <summary>
		/// 
		/// </summary>
		
		public string PropertyAddress { get; set; }
	
		/// <summary>
		/// 
		/// </summary>
		public string PropertyCity { get; set; }
		/// <summary>
		/// 
		/// </summary>
		public string PropertyState { get; set; }
		/// <summary>
		/// 
		/// </summary>
		public string Category { get; set; }
		
		/// <summary>
		/// 
		/// </summary>
		public bool IsActive { get; set; }
	}
}
