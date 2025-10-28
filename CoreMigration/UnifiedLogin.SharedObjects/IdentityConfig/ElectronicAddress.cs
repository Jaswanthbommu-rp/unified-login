namespace UnifiedLogin.SharedObjects.IdentityConfig
{
	/// <summary>
	/// Electronic Address Contact Mechanism
	/// </summary>
	public class ElectronicAddress : CommonAddress, IElectronicAddress
	{
		#region Examples
		/// <summary>
		/// Example for linking an electronic address to a Person method
		/// </summary>
		/// <returns>Newly Created Contact Mechanism Id</returns>
		public static ElectronicAddressOutputResult LinkElectronicAddressOutputResultExample()
		{
			ElectronicAddressOutputResult result = new ElectronicAddressOutputResult();
			result.ContactMechanismId = 1;
			return result;
		}

		/// <summary>
		/// Output result for newly linked Electronic Address to a person
		/// </summary>
		public class ElectronicAddressOutputResult
		{
			/// <summary>
			/// Represents the newly linked Electronic Address to a person
			/// </summary>
			public int ContactMechanismId { get; set; }
		}
		#endregion
	}
}
