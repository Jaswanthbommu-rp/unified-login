using System.Collections.Generic;
using Newtonsoft.Json;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.SharedObjects.Product.RPDocumentManagement
{
	/// <summary>
	/// A DocManagement user
	/// </summary>
	public class RPDMUser
	{
		/// <summary>
		/// The Id of the user
		/// </summary>
		[JsonProperty(PropertyName = "id", NullValueHandling = NullValueHandling.Ignore)]
		public string Id { get; set; }

		/// <summary>
		/// The domain the user belongs under
		/// </summary>
		[JsonProperty(PropertyName = "domain")]
		public string Domain { get; set; }

		/// <summary>
		/// The email address of the user
		/// </summary>
		[JsonProperty(PropertyName = "email")]
		public string Email { get; set; }

		/// <summary>
		/// The user name of the user
		/// </summary>
		[JsonProperty(PropertyName = "name")]
		public string Name { get; set; }

		/// <summary>
		/// The first name of the user
		/// </summary>
		[JsonProperty(PropertyName = "firstName")]
		public string FirstName { get; set; }

		/// <summary>
		/// The last name of the user
		/// </summary>
		[JsonProperty(PropertyName = "lastName")]
		public string LastName { get; set; }

		/// <summary>
		/// Is the user enabled/disabled
		/// </summary>
		[JsonProperty(PropertyName = "enabled")]
		public bool Enabled { get; set; }

		/// <summary>
		/// The locale of the user
		/// </summary>
		[JsonProperty(PropertyName = "locale")]
		public string Locale { get; set; }

		/// <summary>
		/// The time zone of the user
		/// </summary>
		[JsonProperty(PropertyName = "timeZone")]
		public string TimeZone { get; set; }

		/// <summary>
		/// The groups the user belongs to
		/// </summary>
		[JsonProperty(PropertyName = "groups", NullValueHandling = NullValueHandling.Ignore)]
		public List<RPDMScope> Groups { get; set; }

		/// <summary>
		/// The roles the user belongs to
		/// </summary>
		[JsonProperty(PropertyName = "roles")]
		public List<RPDMUserRoles> Roles { get; set; }

		/// <summary>
		/// Photo information about the user
		/// </summary>
		[JsonProperty(PropertyName = "photo", NullValueHandling = NullValueHandling.Ignore)]
		public RPDMScope Photo { get; set; }
	}
}
