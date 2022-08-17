using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RP.Enterprise.Subsystem.ProductLauncher.Service.LandingAPIEnterprise.Dto
{
	/// <summary>
	/// Shared User Data Transform Object attributes
	/// </summary>
	public class UserDataDtoCommon
	{
		/// <summary>
		/// User RealPageId
		/// </summary>
		public Guid UnityRealPageUserId { get; set; }

		/// <summary>
		/// FirstName
		/// </summary>
		[Required(ErrorMessage = "{0} is a required field.")]
		[MaxLength(50, ErrorMessage = "{0} shouldn't be more than {1} characters.")]
		public string FirstName { get; set; }

		/// <summary>
		/// MiddleName
		/// </summary>
		[MaxLength(50, ErrorMessage = "{0} shouldn't be more than {1} characters.")]
		public string MiddleName { get; set; }

		/// <summary>
		/// LastName
		/// </summary>
		[Required(ErrorMessage = "{0} is a required field.")]
		[MaxLength(50, ErrorMessage = "{0} shouldn't be more than {1} characters.")]
		public string LastName { get; set; }

		/// <summary>
		/// IsExternalIdp
		/// </summary>
		public bool IsExternalIdp { get; set; }

		/// <summary>
		/// LoginName
		/// </summary> 
		[DisplayFormat(ConvertEmptyStringToNull = false)]
		[Required(AllowEmptyStrings = false, ErrorMessage = "{0} is a required field.")]
		[MaxLength(255, ErrorMessage = "{0} shouldn't be more than {1} characters.")]
		public string LoginName { get; set; }

		/// <summary>
		/// Email
		/// </summary>
		[MaxLength(255, ErrorMessage = "{0} shouldn't be more than {1} characters.")]
		[DataType(DataType.EmailAddress)]
		[EmailAddress(ErrorMessage = "The email address is not valid")]
		public string Email { get; set; }

		/// <summary>
		/// UserEffectiveDate
		/// </summary>
		[DataType(DataType.Date)]
		[JsonConverter(typeof(DateFormatConverter), "yyyy-MM-dd")]
		public DateTime? UserEffectiveDate { get; set; }

		/// <summary>
		/// UserExprirationDate
		/// </summary>
		[DataType(DataType.Date)]
		[JsonConverter(typeof(DateFormatConverter), "yyyy-MM-dd")]
		public DateTime? UserExpirationDate { get; set; }

		///// <summary>
		///// UnityRoles
		///// </summary>
		//public List<string> UnityRoles { get; set; }

		/// <summary>
		/// EmployeeId
		/// </summary>
		[JsonProperty("EmployeeId", NullValueHandling = NullValueHandling.Ignore)]
		public string EmployeeId { get; set; }

		/// <summary>
		/// LastLogin date
		/// </summary>
		[DataType(DataType.Date)]
		[JsonConverter(typeof(DateFormatConverter), "yyyy-MM-dd")]
		public DateTime? LastLogin { get; set; }
	}

	/// <summary>
	/// Date Format Converter
	/// </summary>
	public class DateFormatConverter : IsoDateTimeConverter
	{
		/// <summary>
		/// Date Format Converter
		/// </summary>
		/// <param name="format">Date format</param>
		public DateFormatConverter(string format)
		{
			DateTimeFormat = format;
		}
	}
}