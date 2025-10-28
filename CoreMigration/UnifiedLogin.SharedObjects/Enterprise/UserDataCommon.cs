using Newtonsoft.Json;
using System;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.SharedObjects.Enterprise
{
	/// <summary>
	/// Shared User attributes
	/// </summary>
	public class UserDataCommon
	{
		#region Private
		private DateTime _userEffectiveDate;
		private bool _userEffectiveDateNull = true;
		private DateTime _userExprirationDate;
		private bool _userExpirationDateNull = true;
		private DateTime _LastLogin;
		private bool _LastLoginNull = true;
		#endregion

		/// <summary>
		/// User Email
		/// </summary>
		[JsonProperty(PropertyName = "Email", NullValueHandling = NullValueHandling.Ignore)]
		public string Email { get; set; }

		/// <summary>
		/// Is External IDP
		/// </summary>
		[JsonProperty(PropertyName = "IsExternalIdp")]
		public bool IsExternalIdp { get; set; }

		/// <summary>
		/// User FirstName
		/// </summary>
		[JsonProperty(PropertyName = "FirstName")]
		public string FirstName { get; set; }

		/// <summary>
		/// User MiddleName
		/// </summary>
		[JsonProperty(PropertyName = "MiddleName")]
		public string MiddleName { get; set; }

		/// <summary>
		/// user LastName
		/// </summary>
		[JsonProperty(PropertyName = "LastName")]
		public string LastName { get; set; }

		/// <summary>
		/// User LoginName
		/// </summary>
		[JsonProperty(PropertyName = "LoginName")]
		public string LoginName { get; set; }

		/// <summary>
		/// User RealPageId
		/// </summary>
		[JsonProperty("UserRealPageId", NullValueHandling = NullValueHandling.Ignore)]
		public Guid UserRealPageId { get; set; }

		/// <summary>
		/// When the account can be used
		/// </summary>
		[JsonProperty(PropertyName = "UserEffectiveDate")]
		public DateTime? UserEffectiveDate
		{
			get
			{
				if (!_userEffectiveDateNull)
				{
					return DateTime.SpecifyKind(_userEffectiveDate, DateTimeKind.Utc);
				}
				else
				{
					return null;
				}
			}

			set
			{
				if (value.HasValue)
				{
					_userEffectiveDate = value.Value;
					_userEffectiveDateNull = false;
				}
				else
				{
					_userEffectiveDateNull = true;
				}
			}
		}

		/// <summary>
		/// When the account can no longer be used
		/// </summary>
		[JsonProperty(PropertyName = "UserExpirationDate")]
		public DateTime? UserExpirationDate
		{
			get
			{
				if (!_userExpirationDateNull)
				{
					return DateTime.SpecifyKind(_userExprirationDate, DateTimeKind.Utc);
				}
				else
				{
					return null;
				}
			}

			set
			{
				if (value.HasValue)
				{
					_userExprirationDate = value.Value;
					_userExpirationDateNull = false;
				}
				else
				{
					_userExpirationDateNull = true;
				}
			}
		}

		/// <summary>
		/// EmployeeId
		/// </summary>
		[JsonProperty("EmployeeId", NullValueHandling = NullValueHandling.Ignore)]
		public string EmployeeId { get; set; }

		/// <summary>
		/// User last login date
		/// </summary>
		[JsonProperty(PropertyName = "LastLogin")]
		//public DateTime? LastLogin { get; set; }
		public DateTime? LastLogin
		{
			get
			{
				if (!_LastLoginNull)
				{
					return DateTime.SpecifyKind(_LastLogin, DateTimeKind.Utc);
				}
				else
				{
					return null;
				}
			}

			set
			{
				if (value.HasValue)
				{
					_LastLogin = value.Value;
					_LastLoginNull = false;
				}
				else
				{
					_LastLoginNull = true;
				}
			}
		}

	}
}
