using Newtonsoft.Json;
using System;

namespace UnifiedLogin.SharedObjects.IdentityConfig
{
	/// <summary>
	/// Person object
	/// </summary>
	public class Person : IPerson
	{
		/// <summary>
		/// PartyId
		/// </summary>
		[JsonProperty(PropertyName = "PartyId")]
		public long PartyId { get; set; }

		/// <summary>
		/// Unique Identifier - EnterpriseUserId
		/// </summary>
		[JsonProperty(PropertyName = "RealPageId")]
		public Guid RealPageId { get; set; }

		/// <summary>
		/// Firstname
		/// </summary>
		[JsonProperty(PropertyName = "FirstName")]
		public string FirstName { get; set; }

		/// <summary>
		/// Middle initial
		/// </summary>
		[JsonProperty(PropertyName = "MiddleName")]
		public string MiddleName { get; set; } = "";

		/// <summary>
		/// Lastname
		/// </summary>
		[JsonProperty(PropertyName = "LastName")]
		public string LastName { get; set; }

		/// <summary>
		/// EmployeeId
		/// </summary>
		[JsonProperty("EmployeeId", NullValueHandling = NullValueHandling.Ignore)]
		public string EmployeeId { get; set; } = "";

		/// <summary>
		/// Suffix
		/// </summary>
		[JsonProperty("Suffix", NullValueHandling = NullValueHandling.Ignore)]
		public string Suffix { get; set; }

		/// <summary>
		/// Title
		/// </summary>
		[JsonProperty("Title", NullValueHandling = NullValueHandling.Ignore)]
		public string Title { get; set; }

		/// <summary>
		/// Preferred Contact Method Id
		/// </summary>
		[JsonProperty("PreferredContactMethodId", NullValueHandling = NullValueHandling.Ignore)]
		public int? PreferredContactMethodId { get; set; }

        /// <summary>
        /// Is FirstName Null or WhiteSpace
        /// </summary>
        [JsonIgnore]
		public bool IsFirstNameNullOrWhiteSpace
		{
			get
			{
				return String.IsNullOrWhiteSpace(FirstName);
			}
		}

		/// <summary>
		/// Is LastName Null or WhiteSpace
		/// </summary>
		[JsonIgnore]
		public bool IsLastNameNullOrWhiteSpace
		{
			get
			{
				return String.IsNullOrWhiteSpace(LastName);
			}
		}

		#region Examples
		/// <summary>
		/// Example for New Person method
		/// </summary>
		/// <returns>Newly Created Party Id</returns>
		public static PersonOutputResult GetNewPersonExample()
		{
			PersonOutputResult result = new PersonOutputResult();
			result.RealPageId = new Guid("13E71DE5-BAFA-469D-9F7A-E12DB3961BA9");
			return result;
		}

		/// <summary>
		/// Output result (Unique Identifier - EnterpriseUserId) for New Person
		/// </summary>
		public class PersonOutputResult
		{
			/// <summary>
			/// Represents the newly created Party Id
			/// </summary>
			public Guid RealPageId { get; set; }
		}
		#endregion
	}
}
