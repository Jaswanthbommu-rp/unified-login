namespace UnifiedLogin.SharedObjects.IdentityConfig
{
	/// <summary>
	/// Person PostalAddress
	/// </summary>
	public class PostalAddress : CommonAddress, IPostalAddress
	{
		#region Examples
		/// <summary>
		/// Example for linking an Postal Address to a Person method
		/// </summary>
		/// <returns>Newly Created Contact Mechanism Id</returns>
		public static PostalAddressOutputResult LinkPostalAddressOutputResultExample()
		{
			PostalAddressOutputResult result = new PostalAddressOutputResult();
			result.ContactMechanismId = 1;
			return result;
		}

		/// <summary>
		/// Output result for newly linked Electronic Address to a person
		/// </summary>
		public class PostalAddressOutputResult
		{
			/// <summary>
			/// Represents the newly linked Electronic Address to a person
			/// </summary>
			public int ContactMechanismId { get; set; }
		}
		#endregion
	}
}
