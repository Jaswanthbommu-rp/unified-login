using System.Collections.Generic;
using Newtonsoft.Json;
using System;
using UnifiedLogin.SharedObjects.Helper;
using System.Web;

namespace UnifiedLogin.SharedObjects.Landing
{
	/// <summary>
	/// Used to store information about the user
	/// </summary>
	[JsonObject(MemberSerialization.OptIn)]
	public class UserProduct : User, IUserProduct
	{
		/// <summary>
		/// PasswordHash
		/// </summary>		
		//public new string PasswordHash { get; set; }

		/// <summary>
		/// PasswordHash
		/// </summary>		
		//public new string PasswordSalt { get; set; }

		/// <summary>
		/// The user id
		/// </summary>
		//[JsonProperty(PropertyName = "userId")]
		//public Int64 UserId { get; set; }

		/// <summary>
		/// First name
		/// </summary>
		//[JsonProperty(PropertyName = "firstName")]
		//public string FirstName { get; set; }

		/// <summary>
		/// Last name
		/// </summary>
		//[JsonProperty(PropertyName = "lastName")]
		//public string LastName { get; set; }

		/// <summary>
		/// Title
		/// </summary>
		[JsonProperty(PropertyName = "title")]
		public string Title { get; set; }

		/// <summary>
		/// Company name
		/// </summary>
		[JsonProperty(PropertyName = "companyName")]
		public string CompanyName { get; set; }

		/// <summary>
		/// Email address
		/// </summary>
		[JsonProperty(PropertyName = "email")]
		public string Email { get; set; }

		/// <summary>
		/// Phone number
		/// </summary>
		[JsonProperty(PropertyName = "phone")]
		public string Phone { get; set; }

		/// <summary>
		/// User access token
		/// </summary>
		[JsonIgnore]
		public string AccessToken { get; set; }

		/// <summary>
		/// Summary count
		/// </summary>
		[JsonProperty(PropertyName = "summaryCounts")]
		public SummaryCounts SummaryCount { get; set; }

		/// <summary>
		/// Products assigned to user
		/// </summary>
		[JsonProperty(PropertyName = "initialProducts")]
		public IList<ProductUI> AssignedProducts { get; set; }

		/// <summary>
		/// Products assigned to user
		/// </summary>
		[JsonProperty(PropertyName = "assignedProducts")]
		[JsonIgnore]
		public IList<ProductUserDetails> AssignedProducts2 { get; set; }

		/// <summary>
		/// Status
		/// </summary>
		[JsonProperty(PropertyName = "status")]
		public string Status
		{
			get
			{
				if (AccountExpiration.Ticks > 0 && AccountExpiration < DateTime.Now.Date)
				{	
					return "Expired";
				}
				else if (IsActive)
				{
					return "Active";
				}
				else
				{
					return "Disabled";
				}
			}
		}

		/// <summary>
		/// Status
		/// </summary>
		[JsonProperty(PropertyName = "lastLogin")]
		public string LastLogin
		{
			get
			{
				return "March 6 2017 11:36AM";
			}
		}
	}
}