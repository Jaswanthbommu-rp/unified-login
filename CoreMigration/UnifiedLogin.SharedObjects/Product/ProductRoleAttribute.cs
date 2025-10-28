namespace UnifiedLogin.SharedObjects.Product
{
	/// <summary>
	/// Used to store additional information about a role
	/// </summary>
	public class ProductRoleAttribute
	{
		/// <summary>
		/// The id of the attribute
		/// </summary>
		public int Id { get; set; }

		/// <summary>
		/// The name of the attribute
		/// </summary>
		public string AttributeName { get; set; }

		/// <summary>
		/// The value of the attribute
		/// </summary>
		public string AttributeValue { get; set; }
	}
}
