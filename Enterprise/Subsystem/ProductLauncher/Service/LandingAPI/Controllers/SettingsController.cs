using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Attribute;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Base;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Helper;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using Swashbuckle.Swagger.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.UnifiedLogin;

namespace RP.Enterprise.Subsystem.ProductLauncher.Service.LandingAPI.Controllers
{
	/// <summary>
	/// Unified Settings Controller
	/// </summary>
	public class SettingsController : BaseApiController
	{
		#region Private variables
		IRepositoryResponse _repositoryResponse = new RepositoryResponse();
		#endregion

		#region Constructor
		/// <summary>
		/// Default constructor
		/// </summary>
		public SettingsController() : base() { }
		#endregion

		#region Public Methods
		/// <summary>
		/// Get Settings Details
		/// </summary>
		/// <param name="category">Setting category (e.g. Security, CustomFields)</param>
		/// <param name="booksCustomerMasterId">Books Customer MasterId</param>
		/// <param name="bookMasterTypeId">Type of Book MasterId (e.g. 1 = Black, 2 = Blue)</param>
		/// <returns>A Settings Details based on category</returns>
		[SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when  object have invalid entries / when Information is out of sync with the server)")]
		[SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
		[SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
		[SwaggerResponse(HttpStatusCode.OK, Description = "Get information about the Settings based on category", Type = typeof(IList<Setting>))]
		[SwaggerResponseExamples(typeof(IList<Setting>), typeof(SettingsExample))]
		[Route("settings/company/{booksCustomerMasterId}")]
		[HttpGet]
		public HttpResponseMessage GetSettings(string category, long booksCustomerMasterId, int bookMasterTypeId = (int)BookMasterType.CustomerMasterId)
		{
			IApiError apiError;
			Organization organization = new Organization();
			IManageOrganization manageOrganization = new ManageOrganization(_userClaims);
            var orgList = manageOrganization.GetUnifiedLoginCompanyList();
            
			switch (bookMasterTypeId)
			{
				case 1:
				case 2:
					//BlueBookId
					//organization = manageOrganization.GetOrganization(Guid.Empty, null, bookMasterId, null);
                    UnifiedLoginCompany ufl = orgList.FirstOrDefault(c => c.Domain.Equals("Primary") && (c.BooksCustomerMasterId == booksCustomerMasterId));
                    if (ufl != null)
                    {
                        organization = manageOrganization.GetOrganization(new Guid(ufl.CompanyRealPageId), null);
                    }
					break;
				default:
					apiError = new ApiError()
					{
						Id = Guid.NewGuid().ToString(),
						Status = (short)HttpStatusCode.BadRequest,
						Title = "Invalid Book Master Type.",
						Detail = "Invalid Book Master Type.",
						Links = string.Empty,
						Code = "Settings.GetSettings.1",
						Source = new ApiErrorSource()
						{
							JsonPointer = string.Empty,
							Parameter = string.Empty
						}
					};
					return Request.CreateResponse(HttpStatusCode.BadRequest, apiError);
			}

			if (organization == null)
			{				
				apiError = new ApiError()
				{
					Id = Guid.NewGuid().ToString(),
					Status = (short)HttpStatusCode.BadRequest,
					Title = "Company not found.",
					Detail = $"Company not found for Blue Books Customer MasterId: {booksCustomerMasterId}",
					Links = string.Empty,
					Code = "Settings.GetSettings.2",
					Source = new ApiErrorSource()
					{
						JsonPointer = string.Empty,
						Parameter = string.Empty
					}
				};
				return Request.CreateResponse(HttpStatusCode.BadRequest, apiError);
			}

			bool IsValid = manageOrganization.ValidateOrganization(_userClaims.OrganizationMasterId, _userClaims.UserRealPageGuid, organization.RealPageId);
			if (!IsValid)
			{
				apiError = new ApiError()
				{
					Id = Guid.NewGuid().ToString(),
					Status = (short)HttpStatusCode.BadRequest,
					Title = "User is not authorized.",
					Detail = $"Logged in user is not authorized to view security settings for {organization.Name}.",
					Links = string.Empty,
					Code = "Settings.GetSettings.3",
					Source = new ApiErrorSource()
					{
						JsonPointer = string.Empty,
						Parameter = string.Empty
					}
				};
				return Request.CreateResponse(HttpStatusCode.BadRequest, apiError);
			}

			IList<Setting> settingList = new List<Setting>();
			switch (category.ToUpper())
			{
				case "SECURITY":
					IManageSecuritySettings manageSecuritySettings = new ManageSecuritySettings(_userClaims);
					settingList = manageSecuritySettings.GetSecuritySettings(booksCustomerMasterId, bookMasterTypeId);

					if (settingList == null)
					{
						//When trying to get a Security Settings that don't exists
						apiError = new ApiError()
						{
							Id = Guid.NewGuid().ToString(),
							Status = (short)HttpStatusCode.NotFound,
							Title = "Security settings not found.",
							Detail = $"Security settings not found for {organization.Name}",
							Links = string.Empty,
							Code = "Settings.GetSettings.4",
							Source = new ApiErrorSource()
							{
								JsonPointer = string.Empty,
								Parameter = string.Empty
							}
						};
						return Request.CreateResponse(HttpStatusCode.BadRequest, apiError);
					}
					break;
				case "CUSTOMFIELDS":
					IManageCustomFields manageCustomFields = new ManageCustomFields(_userClaims);
					RequestParameter datafilter = new RequestParameter();
					datafilter.Pages.ResultsPerPage = 0;
					IDictionary<object, object> globals = new Dictionary<object, object>();
					globals.Add(BaseType.RequestParameter, datafilter);
					settingList = manageCustomFields.GetCustomFields(globals, booksCustomerMasterId);

					if (settingList == null)
					{
						//When trying to get a Security Settings that don't exists
						apiError = new ApiError()
						{
							Id = Guid.NewGuid().ToString(),
							Status = (short)HttpStatusCode.NotFound,
							Title = "Custom Fields not found.",
							Detail = $"Custom Fields not found for {organization.Name}",
							Links = string.Empty,
							Code = "Settings.GetSettings.5",
							Source = new ApiErrorSource()
							{
								JsonPointer = string.Empty,
								Parameter = string.Empty
							}
						};
						return Request.CreateResponse(HttpStatusCode.BadRequest, apiError);
					}
					break;
				default:
					apiError = new ApiError()
					{
						Id = Guid.NewGuid().ToString(),
						Status = (short)HttpStatusCode.NotFound,
						Title = $"{category} settings is not implemented.",
						Detail = $"{category} settings is not implemented.",
						Links = string.Empty,
						Code = "Settings.GetSettings.6",
						Source = new ApiErrorSource()
						{
							JsonPointer = string.Empty,
							Parameter = string.Empty
						}
					};
					return Request.CreateResponse(HttpStatusCode.BadRequest, apiError);
			};
			return Request.CreateResponse(HttpStatusCode.OK, settingList);
		}

		/// <summary>
		/// Update Settings by category
		/// </summary>
		/// <param name="settings">Settings list of object (Key value pairs) of the parameter values</param>
		/// <param name="category">Setting category (e.g. Security, CustomFields)</param>
		/// <param name="booksCustomerMasterId">Books Customer MasterId</param>
		/// <param name="bookMasterTypeId">Type of Book MasterId (e.g. 1 = Black, 2 = Blue)</param>
		/// <returns></returns>
		[SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when SecuritySettings object have invalid entries / when Information is out of sync with the server)")]
		[SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
		[SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
		[SwaggerResponse(HttpStatusCode.OK, Description = "Settings updated")]
		[Route("settings/company/{booksCustomerMasterId}")]
		[HttpPatch]
		public HttpResponseMessage UpdateSettings([FromBody] IList<Setting> settings, string category, long booksCustomerMasterId, int bookMasterTypeId = (int)BookMasterType.CustomerMasterId)
		{
			IApiError apiError;
			ISetting setting;

			Organization organization = new Organization();
			IManageOrganization manageOrganization = new ManageOrganization(_userClaims);
            var orgList = manageOrganization.GetUnifiedLoginCompanyList();

			switch (bookMasterTypeId)
			{
                case 1:
                case 2:
                    //BlueBookId
                    //organization = manageOrganization.GetOrganization(Guid.Empty, null, bookMasterId, null);
                    UnifiedLoginCompany ufl = orgList.FirstOrDefault(c => c.Domain.Equals("Primary") && (c.BooksCustomerMasterId == booksCustomerMasterId));
                    if (ufl != null)
                    {
                        organization = manageOrganization.GetOrganization(new Guid(ufl.CompanyRealPageId), null);
                    }
                    break;
				default:
					apiError = new ApiError()
					{
						Id = Guid.NewGuid().ToString(),
						Status = (short)HttpStatusCode.BadRequest,
						Title = "Invalid Book Master Type.",
						Detail = "Invalid Book Master Type.",
						Links = string.Empty,
						Code = "Settings.UpdateSettings.1",
						Source = new ApiErrorSource()
						{
							JsonPointer = string.Empty,
							Parameter = string.Empty
						}
					};
					return Request.CreateResponse(HttpStatusCode.BadRequest, apiError);
			}

			if (organization == null)
			{				
				apiError = new ApiError()
				{
					Id = Guid.NewGuid().ToString(),
					Status = (short)HttpStatusCode.BadRequest,
					Title = "Company not found.",
					Detail = $"Company not found for Blue Book MasterId: {booksCustomerMasterId}",
					Links = string.Empty,
					Code = "Settings.UpdateSettings.2",
					Source = new ApiErrorSource()
					{
						JsonPointer = string.Empty,
						Parameter = string.Empty
					}
				};
				return Request.CreateResponse(HttpStatusCode.BadRequest, apiError);
			}

			bool IsValid = manageOrganization.ValidateOrganization(_userClaims.OrganizationMasterId, _userClaims.UserRealPageGuid, organization.RealPageId);
			if (!IsValid)
			{
				apiError = new ApiError()
				{
					Id = Guid.NewGuid().ToString(),
					Status = (short)HttpStatusCode.BadRequest,
					Title = "User is not authorized.",
					Detail = $"Logged in user is not authorized to edit security settings for {organization.Name}.",
					Links = string.Empty,
					Code = "Settings.UpdateSettings.3",
					Source = new ApiErrorSource()
					{
						JsonPointer = string.Empty,
						Parameter = string.Empty
					}
				};
				return Request.CreateResponse(HttpStatusCode.BadRequest, apiError);
			}

			if (settings == null)
			{
				apiError = new ApiError()
				{
					Id = Guid.NewGuid().ToString(),
					Status = (short)HttpStatusCode.BadRequest,
					Title = "Invalid parameter.",
					Detail = $"Null parameter: {category} Settings",
					Links = string.Empty,
					Code = "Settings.UpdateSettings.4",
					Source = new ApiErrorSource()
					{
						JsonPointer = string.Empty,
						Parameter = string.Empty
					}
				};
				return Request.CreateResponse(HttpStatusCode.BadRequest, apiError);
			}

			switch (category.ToUpper())
			{
				case "SECURITY":
					setting = new Setting();
					setting = settings.ToList().FirstOrDefault(p => p.Name.Equals("NumberOfPasswordsToRemember", StringComparison.OrdinalIgnoreCase));
					if ((setting != null) && ((Convert.ToInt32(setting.Value) < 1) || (Convert.ToInt32(setting.Value) > 5)))
					{
						apiError = new ApiError()
						{
							Id = Guid.NewGuid().ToString(),
							Status = (short)HttpStatusCode.BadRequest,
							Title = "Previous passwords to track.",
							Detail = "Previous passwords to track can be between 1 and 5.",
							Links = string.Empty,
							Code = "Settings.UpdateSettings.5",
							Source = new ApiErrorSource()
							{
								JsonPointer = string.Empty,
								Parameter = string.Empty
							}
						};
						return Request.CreateResponse(HttpStatusCode.BadRequest, apiError);
					}

					setting = new Setting();
					setting = settings.ToList().FirstOrDefault(p => p.Name.Equals("PasswordExpirationPeriodInDays", StringComparison.OrdinalIgnoreCase));
					if ((setting != null) && ((Convert.ToInt32(setting.Value) < 30) || (Convert.ToInt32(setting.Value) > 120)))
					{
						apiError = new ApiError()
						{
							Id = Guid.NewGuid().ToString(),
							Status = (short)HttpStatusCode.BadRequest,
							Title = "Days before password expires.",
							Detail = "Time frame can be between 30 and 120 days.",
							Links = string.Empty,
							Code = "Settings.UpdateSettings.6",
							Source = new ApiErrorSource()
							{
								JsonPointer = string.Empty,
								Parameter = string.Empty
							}
						};
						return Request.CreateResponse(HttpStatusCode.BadRequest, apiError);
					}

					setting = new Setting();
					setting = settings.ToList().FirstOrDefault(p => p.Name.Equals("MinimumLength", StringComparison.OrdinalIgnoreCase));
					if ((setting != null) && ((Convert.ToInt32(setting.Value) < 8) || (Convert.ToInt32(setting.Value) > 20)))
					{
						apiError = new ApiError()
						{
							Id = Guid.NewGuid().ToString(),
							Status = (short)HttpStatusCode.BadRequest,
							Title = "Minimum character length for password.",
							Detail = "Minimum length can be between 8 and 20 characters.",
							Links = string.Empty,
							Code = "Settings.UpdateSettings.7",
							Source = new ApiErrorSource()
							{
								JsonPointer = string.Empty,
								Parameter = string.Empty
							}
						};
						return Request.CreateResponse(HttpStatusCode.BadRequest, apiError);
					}

					setting = new Setting();
					setting = settings.ToList().FirstOrDefault(p => p.Name.Equals("ForcedLock", StringComparison.OrdinalIgnoreCase));
					//ActivityTokenExpirationMinutes
					if ((setting != null) && ((Convert.ToInt32(setting.Value) < 0) || (Convert.ToInt32(setting.Value) > 999)))
					{
						apiError = new ApiError()
						{
							Id = Guid.NewGuid().ToString(),
							Status = (short)HttpStatusCode.BadRequest,
							Title = "Minutes a user is locked out.",
							Detail = "Locked out duration can be between 0 and 999 minutes.",
							Links = string.Empty,
							Code = "Settings.UpdateSettings.8",
							Source = new ApiErrorSource()
							{
								JsonPointer = string.Empty,
								Parameter = string.Empty
							}
						};
						return Request.CreateResponse(HttpStatusCode.BadRequest, apiError);
					}

					setting = new Setting();
					setting = settings.ToList().FirstOrDefault(p => p.Name.Equals("Login", StringComparison.OrdinalIgnoreCase));
					//MaxActivityAttemptCount
					if ((setting != null) && ((Convert.ToInt32(setting.Value) < 1) || (Convert.ToInt32(setting.Value) > 10)))
					{
						apiError = new ApiError()
						{
							Id = Guid.NewGuid().ToString(),
							Status = (short)HttpStatusCode.BadRequest,
							Title = "Failed login attempts allowed.",
							Detail = "Allowed failed login attempts can be between 1 and 10.",
							Links = string.Empty,
							Code = "Settings.UpdateSettings.9",
							Source = new ApiErrorSource()
							{
								JsonPointer = string.Empty,
								Parameter = string.Empty
							}
						};
						return Request.CreateResponse(HttpStatusCode.BadRequest, apiError);
					}

					setting = new Setting();
					setting = settings.ToList().FirstOrDefault(p => p.Name.Equals("NewUserRegistration", StringComparison.OrdinalIgnoreCase));
					//ActivityTokenExpirationDays
					if ((setting != null) && ((Convert.ToInt32(setting.Value) < 1) || (Convert.ToInt32(setting.Value) > 14)))
					{
						apiError = new ApiError()
						{
							Id = Guid.NewGuid().ToString(),
							Status = (short)HttpStatusCode.BadRequest,
							Title = "Days temporary passwords and new user links are active.",
							Detail = "Temporary passwords and new user links can be active 1 to 14 days.",
							Links = string.Empty,
							Code = "Settings.UpdateSettings.10",
							Source = new ApiErrorSource()
							{
								JsonPointer = string.Empty,
								Parameter = string.Empty
							}
						};
						return Request.CreateResponse(HttpStatusCode.BadRequest, apiError);
					}

					IManageSecuritySettings manageSecuritySettings = new ManageSecuritySettings(_userClaims);
					_repositoryResponse = manageSecuritySettings.UpdateSecuritySettings(settings, booksCustomerMasterId, bookMasterTypeId);

					if (_repositoryResponse.Id == 0)
					{
						apiError = new ApiError()
						{
							Id = Guid.NewGuid().ToString(),
							Status = (short)HttpStatusCode.BadRequest,
							Title = "Update security settings Failed.",
							Detail = _repositoryResponse.ErrorMessage,
							Links = string.Empty,
							Code = "Settings.UpdateSettings.11",
							Source = new ApiErrorSource()
							{
								JsonPointer = string.Empty,
								Parameter = string.Empty
							}
						};
						return Request.CreateResponse(HttpStatusCode.BadRequest, apiError);
					}
					break;
				case "CUSTOMFIELDS":
					IManageCustomFields manageCustomFields = new ManageCustomFields(_userClaims);

					if ((settings != null) && (settings.Count > 0))
					{
						IDictionary<object, object> globals = new Dictionary<object, object>();
						RequestParameter datafilter = new RequestParameter();
						globals.Add(BaseType.RequestParameter, datafilter);
						IList<CustomField> customFieldList = manageCustomFields.GetCustomField(globals: globals, booksCustomerMasterId: _userClaims.CustomerMasterId);

						string jsonCustomFields = settings[0].Value;
						jsonCustomFields = JObject.Parse(jsonCustomFields).SelectToken("customField").ToString();
						bool IsValidJson = ValidateJson.IsValidJson<IList<CustomField>>(jsonCustomFields);
						if (IsValidJson)
						{
							IList<CustomField> newCustomFieldList = JsonConvert.DeserializeObject<IList<CustomField>>(jsonCustomFields);
							newCustomFieldList.ToList().RemoveAll(c => c.FieldId > 0);

							if (newCustomFieldList.ToList().Any(n => customFieldList.Select(e => e.Sequence).Contains(n.Sequence)))
							{
								apiError = new ApiError()
								{
									Id = Guid.NewGuid().ToString(),
									Status = (short)HttpStatusCode.BadRequest,
									Title = "Add/Update custom fields Failed.",
									Detail = "A custom field(s) with the same sequence already exists.",
									Links = string.Empty,
									Code = "Settings.UpdateSettings.14",
									Source = new ApiErrorSource()
									{
										JsonPointer = string.Empty,
										Parameter = string.Empty
									}
								};
								return Request.CreateResponse(HttpStatusCode.BadRequest, apiError);
							}
						}
					}

					_repositoryResponse = manageCustomFields.AddUpdateCustomFields(settings, booksCustomerMasterId, bookMasterTypeId);

					if (_repositoryResponse.Id == 0)
					{
						apiError = new ApiError()
						{
							Id = Guid.NewGuid().ToString(),
							Status = (short)HttpStatusCode.BadRequest,
							Title = "Update custom fields Failed.",
							Detail = _repositoryResponse.ErrorMessage,
							Links = string.Empty,
							Code = "Settings.UpdateSettings.12",
							Source = new ApiErrorSource()
							{
								JsonPointer = string.Empty,
								Parameter = string.Empty
							}
						};
						return Request.CreateResponse(HttpStatusCode.BadRequest, apiError);
					}
					break;
				default:
					apiError = new ApiError()
					{
						Id = Guid.NewGuid().ToString(),
						Status = (short)HttpStatusCode.NotFound,
						Title = $"{category} settings is not implemented.",
						Detail = $"{category} settings is not implemented.",
						Links = string.Empty,
						Code = "Settings.UpdateSettings.13",
						Source = new ApiErrorSource()
						{
							JsonPointer = string.Empty,
							Parameter = string.Empty
						}
					};
					return Request.CreateResponse(HttpStatusCode.BadRequest, apiError);
			};

			return Request.CreateResponse(HttpStatusCode.OK, settings);
		}
		#endregion

		#region Get Examples
		/// <summary>
		/// Used to document examples of the User List Model webapi result
		/// </summary>
		public class SettingsExample : IProvideExamples
		{
			/// <summary>
			/// Example object data used by Swagger to document the output of the webapi method
			/// </summary>
			/// <returns>SecuritySettings example</returns>
			public object GetExamples()
			{
				IList<Setting> securitySettingList = new List<Setting>()
				{
					new Setting()
					{
						Name = "Settings OR CustomFields",
						Value = "Value",
						Right = 0
					}
				};
				return securitySettingList;
			}
		}
		#endregion
	}
}
