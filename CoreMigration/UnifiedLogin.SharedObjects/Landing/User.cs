using Newtonsoft.Json;
using System;

namespace UnifiedLogin.SharedObjects.Landing
{
	/// <summary>
	/// User Object
	/// </summary>	
	public class User
	{
		/// <summary>
		/// UserId
		/// </summary>
		[JsonProperty(PropertyName = "userId")]
		public long UserId { get; set; }

		/// <summary>
		/// LoginId
		/// </summary>
		[JsonProperty(PropertyName = "loginId")]
		public string LoginId { get; set; }

		/// <summary>
		/// Firstname
		/// </summary>
		[JsonProperty(PropertyName = "firstname")]
		public string Firstname { get; set; }
		/// <summary>
		/// Lastname
		/// </summary>
		[JsonProperty(PropertyName = "lastname")]
		public string Lastname { get; set; }

		/// <summary>
		/// IsActive
		/// </summary>
		[JsonProperty(PropertyName = "isActive")]
		public bool IsActive { get; set; }

		/// <summary>
		/// IsLocked
		/// </summary>
		[JsonProperty(PropertyName = "isLocked")]
		public bool IsLocked { get; set; }

		/// <summary>
		/// PasswordHash
		/// </summary>				
		public string PasswordHash { get; set; }

		/// <summary>
		/// PasswordHash
		/// </summary>
		public string PasswordSalt { get; set; }

		/// <summary>
		/// IdentityProvider
		/// </summary>
		[JsonProperty(PropertyName = "identityProvider")]
		public string IdentityProvider { get; set; }

		/// <summary>
		/// Last Password Modified Date Time
		/// </summary>
		//[JsonConverter(typeof(DateTimeFormatConverter))]
		[JsonProperty(PropertyName = "lastPasswordModifiedDateTime")]
		public DateTime? LastPasswordModifiedDateTime { get; set; }

		/// <summary>
		/// User account expiration date
		/// </summary>
		//[JsonConverter(typeof(DateTimeFormatConverter))]
		[JsonProperty(PropertyName = "accountExpiration")]
		public DateTime AccountExpiration { get; set; }


		//public IList<PortfolioProductUserDetails> AssignedProducts { get; set; }
		/// <summary>
		/// Example for New User method
		/// </summary>
		/// <returns>Newly Created User Id</returns>
		public static NewUserIDOutputResult GetNewUserExample()
		{
			NewUserIDOutputResult result = new NewUserIDOutputResult() { UserId = 10001 };
			return result;
		}

		/// <summary>
		/// Output result for newly created user
		/// </summary>        
		public class NewUserIDOutputResult
		{
			/// <summary>
			/// Newly created user id
			/// </summary>
			public int UserId { get; set; }
		}

		/// <summary>
		/// Example for Update user output method
		/// </summary>
		/// <returns>Last Modified Date time</returns>
		public static LastModifiedDateOutputResult GetUpdateUserExample()
		{
			LastModifiedDateOutputResult result = new LastModifiedDateOutputResult() { UserId = 10001 };
			return result;
		}

		/// <summary>
		/// Example for Update user output method
		/// </summary>
		public class LastModifiedDateOutputResult
		{
			/// <summary>
			/// User last modified date time
			/// </summary>
			public int UserId { get; set; }
		}
	}
}
