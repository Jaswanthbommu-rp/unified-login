using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Helper;
using System;
using System.Web;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects
{
	/// <summary>
	/// User Object
	/// </summary>
	public interface User
	{
		/// <summary>
		/// UserId
		/// </summary>
		public long UserId { get; set; }

		/// <summary>
		/// LoginId
		/// </summary>
		public string LoginId { get; set; }

		/// <summary>
		/// Firstname
		/// </summary>
		public string Firstname { get; set; }
		/// <summary>
		/// Lastname
		/// </summary>
		public string Lastname { get; set; }

		/// <summary>
		/// IsActive
		/// </summary>
		public bool IsActive { get; set; }

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
		public string IdentityProvider { get; set; }

		/// <summary>
		/// Last Password Modified Date Time
		/// </summary>
		public DateTime? LastPasswordModifiedDateTime { get; set; }
		
		/// <summary>
		/// User account expiration date
		/// </summary>
		public DateTime AccountExpiration { get; set; }
				
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
