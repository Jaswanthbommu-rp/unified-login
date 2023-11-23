using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using RP.Enterprise.Subsystem.ProductLauncher.Service.LandingAPIEnterprise.Swagger;
using Swashbuckle.Swagger;
using Swashbuckle.Swagger.Annotations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Http.Description;
using System.Web.UI.WebControls;

namespace RP.Enterprise.Subsystem.ProductLauncher.Service.LandingAPIEnterprise.Dto
{
	/// <summary>
	/// UserData Data Transform Object
	/// </summary>
	public class UserDataDto : UserDataDtoCommon
	{
		/// <summary>
		/// Suffix
		/// </summary>
		[MaxLength(12, ErrorMessage = "{0} shouldn't be more than {1} characters.")]
		public string Suffix { get; set; }

		/// <summary>
		/// Password
		/// </summary>
		[DataType(DataType.Password)]
		public string Password { get; set; }

		/// <summary>
		/// Phone
		/// </summary>
		public string Phone { get; set; }

		/// <summary>
		/// Title
		/// </summary>
		[MaxLength(50, ErrorMessage = "{0} shouldn't be more than {1} characters.")]
		public string Title { get; set; }

		///// <summary>
		///// CompanyJobTitle
		///// </summary>
		//[MaxLength(255, ErrorMessage = "{0} shouldn't be more than {1} characters.")]
		//public string CompanyJobTitle { get; set; }

		// public string PhoneType { get; set; }
		// public string PreferredContactMethod { get; set; }

		/// <summary>
		/// UserType
		/// </summary>
		[EnumDataType(typeof(UserTypeDto), ErrorMessage= "UserType should be Regular or NoEmail.")]
		public UserTypeDto UserType { get; set; }

		/// <summary>
		/// CustomFields
		/// </summary>
		public Dictionary<string, string> CustomFields = new Dictionary<string, string>();

		/// <summary>
		/// AdditionalFields
		/// </summary>
		public Dictionary<string, string> AdditionalFields = new Dictionary<string, string>();

		/// <summary>
		/// SendInvitationEmail
		/// </summary>
		[JsonProperty("SendInvitationEmail", NullValueHandling = NullValueHandling.Ignore)]
		public bool? SendInvitationEmail { get; set; }

		/// <summary>
		/// doNotForceChangePassword
		/// </summary>
        [JsonProperty("doNotForceChangePassword", NullValueHandling = NullValueHandling.Ignore)]
		[SwaggerIgnore]
         public bool doNotForceChangePassword { get; set; }
    }
}