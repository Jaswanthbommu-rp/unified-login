using Aspose.Cells;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Attributes;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Attribute;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Base;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using Swashbuckle.Swagger.Annotations;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using LE = RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing.Export;

namespace RP.Enterprise.Subsystem.ProductLauncher.Service.LandingAPI.Controllers
{
	/// <summary>
	/// Person Controller to hold all user management related APIs
	/// </summary>
	public class PersonController : BaseApiController
    {
        #region Private variables
        IRepositoryResponse repositoryResponse = new RepositoryResponse();
        #endregion

        #region Constructor
        /// <summary>
        /// Default constructor
        /// </summary>
        public PersonController() : base() { }
        #endregion

        #region Public Methods
        /// <summary>
        /// Create a new person
        /// </summary>
        /// <param name="person">Person object of the parameter values</param>
        /// <returns>Response with Success Message</returns>
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when Person object have invalid entries)")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Newly created Person Id", Type = typeof(Person.PersonOutputResult))]
        [SwaggerResponseExamples(typeof(Person.PersonOutputResult), typeof(NewPersonOutputResultExample))]
        [HttpPost]
        [Route("persons")]
        public HttpResponseMessage CreatePerson([FromBody] Person person)
        {
            //CreatePerson
            if (person == null)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Null parameter: Person.");
            }

            IManagePerson personLogic = new ManagePerson();
            repositoryResponse = personLogic.CreatePerson(person);
            if (repositoryResponse.Id == 0)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, repositoryResponse.ErrorMessage);
            }

            Person.PersonOutputResult result = new Person.PersonOutputResult
            {
                RealPageId = repositoryResponse.RealPageId
            };

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        /// <summary>
        /// Get person detail
        /// </summary>
        /// <param name="realPageId">User unique identifier</param>
        /// <returns>Person object</returns>
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Get information about the person", Type = typeof(IPerson))]
        [SwaggerResponseExamples(typeof(IPerson), typeof(PersonExample))]
        [Route("persons/{realPageId}")]
        [HttpGet]
        public HttpResponseMessage GetPerson(Guid realPageId)
        {
			ObjectOutput<IPerson, IErrorData> output = new ObjectOutput<IPerson, IErrorData>();
			Status<IErrorData> errorStatus = new Status<IErrorData>();

			realPageId = ((realPageId == Guid.Empty) || (realPageId == null)) ? _realpageUserId : realPageId;
            if ((realPageId == Guid.Empty) || (realPageId == null))
            {
				errorStatus.Success = false;
				errorStatus.ErrorCode = "Person.GetPerson.1";
				errorStatus.ErrorMsg = "Get Person: Invalid parameter enterprise User Id";
				output.Status = errorStatus;
				return Request.CreateResponse(HttpStatusCode.OK, output);
            }

			IPerson person = new Person();

            IManagePerson personLogic = new ManagePerson();
            person = personLogic.GetPerson(realPageId);

            if (person != null)
            {
				output.Status = errorStatus;
                output.obj = person;
                return Request.CreateResponse(HttpStatusCode.OK, output);
            }

			//When trying to get a Person that doesn't exists / deleted
			errorStatus.Success = false;
			errorStatus.ErrorCode = "Person.GetPerson.2";
			errorStatus.ErrorMsg = "Get Person: Invalid enterprise User Id";
			output.Status = errorStatus;
			return Request.CreateResponse(HttpStatusCode.OK, output);
        }

        /// <summary>
        /// Update Person
        /// </summary>
        /// <param name="realPageId">User unique identifier</param>
        /// <param name="person">Person object of the parameter values</param>
        /// <returns>Response with Success Message</returns>
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when Person object have invalid entries / when Information is out of sync with the server)")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Person Updated")]
        [Route("persons/{realPageId}")]
        [HttpPut]
        public HttpResponseMessage UpdatePerson(Guid realPageId, [FromBody] Person person)
        {
            realPageId = ((realPageId == Guid.Empty) || (realPageId == null)) ? _realpageUserId : realPageId;
            if ((realPageId == Guid.Empty) || (realPageId == null))
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid parameter: realPageId");
            }

            if (person == null)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Null parameter: Person");
            }

            IManagePerson personLogic = new ManagePerson();
            repositoryResponse = personLogic.UpdatePerson(realPageId, person);

            if (repositoryResponse.Id == 0)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, repositoryResponse.ErrorMessage);
            }

            return Request.CreateResponse(HttpStatusCode.OK, person);
        }
        
        /// <summary>
        /// List person profiles
        /// </summary>
        /// <param name="datafilter"></param>
        /// <returns>List of Person object</returns>
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Get information about a list of person profiles", Type = typeof(IProfileDetail))]
        [SwaggerResponseExamples(typeof(IProfileDetail), typeof(ListPersonsExample))]
        [Route("persons")]
        [AuthorizeRight("viewusers", "viewonlysupporttoolaccess")]
        [HttpGet]
        public HttpResponseMessage ListPersons([FromUri]RequestParameter datafilter)
        {
            IDictionary<object, object> globals = new Dictionary<object, object>();
            ObjectListOutput<ProfileDetail, IErrorData> output = new ObjectListOutput<ProfileDetail, IErrorData>();
            Status<IErrorData> errorStatus = new Status<IErrorData>();

            if (datafilter == null)
            {
                datafilter = new RequestParameter();
            }

            globals.Add(BaseType.RequestParameter, datafilter);

			ManageUser manageUser = new ManageUser(_userClaims);

			IManageProfile manageProfile = new ManageProfile(_userClaims);
            IList<ProfileDetail> profileDetailList = manageProfile.ListProfileDetails(globals: globals, organizationRealPageId: null);

			int totalRecords = profileDetailList.Count > 0 ? profileDetailList[0].TotalRecords : 0;
			decimal resultsPerPage = ((datafilter.Pages.ResultsPerPage == 100) && (totalRecords > 0)) ? totalRecords : datafilter.Pages.ResultsPerPage;
			resultsPerPage = (resultsPerPage == 0) ? totalRecords : resultsPerPage;
			PagingSummary pagingSummary = new PagingSummary()
			{
				TotalRecords = totalRecords,
				TotalPages = (resultsPerPage == 0) ? 0 : (int)Math.Ceiling(totalRecords / resultsPerPage)
			};

			output = new ObjectListOutput<ProfileDetail, IErrorData>() { list = profileDetailList, Status = errorStatus };
			output.pagingSummary = pagingSummary;
			return Request.CreateResponse(HttpStatusCode.OK, output);
        }

	    /// <summary>
	    /// List person profiles by organization id. Internal only
	    /// </summary>
		/// <param name="realPageId">Organization EnterpriseId</param>
	    /// <param name="datafilter"></param>
	    /// <returns>List of Person object</returns>
	    [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
	    [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
	    [SwaggerResponse(HttpStatusCode.OK, Description = "Get information about a list of person profiles", Type = typeof(IProfileDetail))]
	    [SwaggerResponseExamples(typeof(IProfileDetail), typeof(ListPersonsExample))]
	    [Route("persons/company/{realPageId}")]
	    [AuthorizeRight("viewusers")]
	    [HttpGet]
	    public HttpResponseMessage ListPersonsByOrg(Guid realPageId, [FromUri]RequestParameter datafilter)
	    {
		    IDictionary<object, object> globals = new Dictionary<object, object>();
		    ObjectListOutput<ProfileDetail, IErrorData> output = new ObjectListOutput<ProfileDetail, IErrorData>();
		    Status<IErrorData> errorStatus = new Status<IErrorData>();

		    if (datafilter == null)
		    {
			    datafilter = new RequestParameter();
		    }

		    globals.Add(BaseType.RequestParameter, datafilter);

		    IManageProfile manageProfile = new ManageProfile(_userClaims);
		    IList<ProfileDetail> profileDetailList = manageProfile.ListProfileDetails(globals: globals, organizationRealPageId: realPageId);

			int totalRecords = profileDetailList.Count > 0 ? profileDetailList[0].TotalRecords : 0;
			decimal resultsPerPage = ((datafilter.Pages.ResultsPerPage == 100) && (totalRecords > 0)) ? totalRecords : datafilter.Pages.ResultsPerPage;
			PagingSummary pagingSummary = new PagingSummary()
			{
				TotalRecords = totalRecords,
				TotalPages = (int)Math.Ceiling(totalRecords / resultsPerPage)
			};

			output = new ObjectListOutput<ProfileDetail, IErrorData>() { list = profileDetailList, Status = errorStatus };
			output.pagingSummary = pagingSummary;
			return Request.CreateResponse(HttpStatusCode.OK, output);
	    }

		/// <summary>
		/// Export Users
		/// </summary>
		/// <param name="datafilter">Filter, Sort, Paginate</param>
		/// <param name="dataFormat">Retrun data in this format (default = CSV)</param>
		/// <returns>List of Person object</returns>
		[SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
		[SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
		[SwaggerResponse(HttpStatusCode.OK, Description = "Get information about a list of person profiles", Type = typeof(IProfileDetail))]
		[SwaggerResponseExamples(typeof(IProfileDetail), typeof(ListPersonsExample))]
		[Route("persons/export")]
		[AuthorizeRight("viewusers")]
		[HttpGet]
		public HttpResponseMessage ListUsersExport([FromUri]RequestParameter datafilter, SaveFormat dataFormat = SaveFormat.CSV)
		{
			string jsonString = string.Empty;
			byte[] plainBytes;
			IDictionary<object, object> globals = new Dictionary<object, object>();
			ObjectOutput<string, IErrorData> output = new ObjectOutput<string, IErrorData>();
			DateTime parsedMaxValueDate = DateTime.Parse("Dec 31, 9999");

			Status<IErrorData> errorStatus = new Status<IErrorData>();

			if (datafilter == null)
			{
				datafilter = new RequestParameter();
			}

			globals.Add(BaseType.RequestParameter, datafilter);

			IManageProfile manageProfile = new ManageProfile(_userClaims);
			IList<ProfileDetail> profileDetailList = manageProfile.ListProfileDetails(globals);

			IList<LE.User> listUsers = new List<LE.User>();

			ManageUserLogin manageUserLogin = new ManageUserLogin(_userClaims);
            // TODO: FIGURE OUT WHAT TO DO FOR TIMEZONE IN MULTI COMPANY
			var userLogin = manageUserLogin.GetUserLogin(_realpageUserId, _orgPartyId); // keep for now
			TimeZoneInfo timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(userLogin.TimeZoneOffset);

			if (profileDetailList != null)
			{
				profileDetailList.ToList().ForEach(p =>
				{
					//User Type
					string userType = string.Empty;
					if (p.userLogin.UserRoleType != null)
					{
						Enum enumUserRole = p.userLogin.UserRoleType;
						Type enumUserRoleType = enumUserRole.GetType();
						System.Reflection.FieldInfo fieldInfoUserRole = enumUserRoleType.GetField(enumUserRole.ToString());
						object[] userRoleAttributes = fieldInfoUserRole.GetCustomAttributes(typeof(DescriptionAttribute), false);
						userType = userRoleAttributes.Length == 0 ? enumUserRole.ToString() : ((DescriptionAttribute)userRoleAttributes[0]).Description;
					}

					listUsers.Add(
						new LE.User()
						{
							UserType = userType,
							FirstName = p.FirstName,
							MiddleName = p.MiddleName,
							LastName = p.LastName,
							LoginName = p.userLogin.LoginName,
							Products = p.SummaryCount.TotalAssignedProducts,
							LastLogin = p.userLogin.LastLogin != null ? TimeZoneInfo.ConvertTimeFromUtc(p.userLogin.LastLogin.Value, timeZoneInfo).ToString() : string.Empty,
                            Status = p.userLogin.Status.ToString().Equals("disabled", StringComparison.OrdinalIgnoreCase) ? "Deactivated" : p.userLogin.Status.ToString(),
                            IDP = p.userLogin.Is3rdPartyIDP ? "Yes" : "No",
							EffectiveDate = p.userLogin.FromDate != null ? TimeZoneInfo.ConvertTimeFromUtc(p.userLogin.FromDate.Value, timeZoneInfo).ToShortDateString() : string.Empty,
							ExpireDate = ((p.userLogin.ThruDate == null) || (DateTime.Compare(p.userLogin.ThruDate.Value, parsedMaxValueDate) == 0)) ? string.Empty : TimeZoneInfo.ConvertTimeFromUtc(p.userLogin.ThruDate.Value, timeZoneInfo).ToShortDateString(),
							CustomField = p.CustomField
						}
					);
				});

				errorStatus = SetAsposeLicense();
				if (errorStatus.Success)
				{
					plainBytes = ExportUserData(listUsers, dataFormat);
					output = new ObjectOutput<string, IErrorData>()
					{
						obj = Convert.ToBase64String(plainBytes),
						Status = errorStatus
					};

					/*
					DO NOT REMOVE
					SerializerContractResolver jsonResolver = new SerializerContractResolver();
					jsonResolver.RenameProperty(typeof(ObjectOutput<string, IErrorData>), "data", "fileContent");

					JsonSerializerSettings serializerSettings = new JsonSerializerSettings();
					serializerSettings.ContractResolver = jsonResolver;
					jsonString = JsonConvert.SerializeObject(output, serializerSettings);
					object newOutput = JsonConvert.DeserializeObject(jsonString);
					*/
					return Request.CreateResponse(HttpStatusCode.OK, output);
				}
				else
				{
					output.Status = errorStatus;
					return Request.CreateResponse(HttpStatusCode.OK, output);
				}
			}
			else
			{
				errorStatus.Success = false;
				errorStatus.ErrorCode = "Person.ListUsersExport.1";
				errorStatus.ErrorMsg = "List Users Export: No data";
				output.Status = errorStatus;
				return Request.CreateResponse(HttpStatusCode.OK, output);
			}
		}
		#endregion

		#region Persona
		/// <summary>
		/// Used to get a persons active persona by realpageId
		/// </summary>
		/// <param name="realPageId">User personaId</param>
		/// <returns>A list of personas for the give user id</returns>
		[SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when Person object have invalid entries)")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Newly created Person Id", Type = typeof(Persona))]
        //[SwaggerResponseExamples(typeof(Person.PersonOutputResult), typeof(NewPersonOutputResultExample))]
        [HttpGet]
        [Route("person/persona/{realPageId}")]
        public HttpResponseMessage GetActivePersona([FromUri] Guid realPageId)
        {
			ObjectOutput<IPersona, IErrorData> output = new ObjectOutput<IPersona, IErrorData>();
			Status<IErrorData> errorStatus = new Status<IErrorData>();

			if (realPageId == Guid.Empty)
			{
				errorStatus.Success = false;
				errorStatus.ErrorCode = "Person.GetActivePersona.1";
				errorStatus.ErrorMsg = "Get active persona: Invalid parameter enterprise User Id";
				output.Status = errorStatus;
				return Request.CreateResponse(HttpStatusCode.OK, output);
			}

			ManagePersona personaManager = new ManagePersona(_userClaims);
            //Persona persona = personaManager.GetActivePersona(realPageId);
            Persona persona = personaManager.GetFirstAvailablePersonaByCompany(realPageId, _orgPartyId);

			if ((persona != null) && (persona.PersonaId > 0))
			{
				output.obj = persona; ;
				output.Status = errorStatus;
				return Request.CreateResponse(HttpStatusCode.OK, output);
			}

			//When trying to get a Person that doesn't exists / deleted
			errorStatus.Success = false;
			errorStatus.ErrorCode = "Person.GetActivePersona.2";
			errorStatus.ErrorMsg = "Get active persona: Invalid enterprise User Id.";
			output.Status = errorStatus;
			return Request.CreateResponse(HttpStatusCode.OK, output);
		}


        /// <summary>
        /// Used to get a list of personas by realpageId
        /// </summary>
        /// <param name="realPageId">User RealPageId</param>
        /// <returns>A list of personas for the give user id</returns>
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when Person object have invalid entries)")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Person Id", Type = typeof(Persona))]
        [HttpGet]
        [Route("person/personas/{realPageId}")]
        public IList<Persona> GetListOfPersona([FromUri]Guid realPageId)
        {
            if (realPageId == null) { throw new HttpResponseException(HttpStatusCode.BadRequest); }

            ManagePersona personaManager = new ManagePersona();
            IList<Persona> persona = personaManager.ListPersona(realPageId);
            return persona;
        }

        #endregion


        #region Get Examples
        /// <summary>
        /// Used to document examples of the Person Model webapi result
        /// </summary>
        [ExcludeFromCodeCoverage]
        public class PersonExample : IProvideExamples
        {
            /// <summary>
            /// Example object data used by Swagger to document the output of the webapi method
            /// </summary>
            /// <returns>Person example</returns>
            public object GetExamples()
            {
                IPerson example = new Person()
                {
                    PartyId = 1,
                    Title = "Property Manager",
                    FirstName = "John",
                    MiddleName = "X",
                    LastName = "doe",
                    Suffix = "Mr"
                };

                ObjectOutput<IPerson, IErrorData> output = new ObjectOutput<IPerson, IErrorData>() { obj = example };

                return output;
            }
        }
        #endregion
                
        #region Output results for documentation
        /// <summary>
        /// Used to document examples of the New Person webapi result
        /// </summary>
        [ExcludeFromCodeCoverage]
        public class NewPersonOutputResultExample : IProvideExamples
        {
            /// <summary>
            /// Example object data used by Swagger to document the output of the webapi method
            /// </summary>
            /// <returns>Newly created party id</returns>
            public object GetExamples()
            {
                return Person.GetNewPersonExample();
            }
        }

		/// <summary>
		/// Used to document examples of the User Profile
		/// </summary>
		[ExcludeFromCodeCoverage]
		public class ListPersonsExample : IProvideExamples
        {
            /// <summary>
            /// Example object data used by Swagger to document the output of the webapi method
            /// </summary>
            /// <returns>Profile example</returns>
            public object GetExamples()
            {                
                IList<IProfileDetail> listProfile = new List<IProfileDetail>();
                IProfileDetail profile = new ProfileDetail();
                profile.userLogin = new UserLogin()
                {
                    IsActive = true,
                    FromDate = DateTime.Now.AddDays(-7),
                    IsLocked = false,
                    IsSuperUser = false,
                    IsTainted = false,
                    LastLogin = DateTime.Now.AddDays(-30),
                    LoginName = "test@test.com",
                    LoginNameType = "email",
                    PartyId = 100,
                    PasswordModifiedDate = DateTime.Now.AddDays(-30),
                    RealPageId = Guid.NewGuid(),
                    //Status = "active",
                    //StatusSetDate = DateTime.Now.AddDays(-7),
                    ThruDate = DateTime.Now.AddDays(30),
                    UserId = 1
                };

                profile.SummaryCount.TotalAssignedProducts = 10;
                profile.SummaryCount.TotalAssignedProperties = 0;
                profile.SummaryCount.TotalAssignedRoles = 0;
                profile.RealPageId = Guid.NewGuid();
                profile.Suffix = "Mr.";
                profile.Title = "";
                profile.Avatar = "";
                profile.FirstName = "Test";
                profile.LastName = "User";
                profile.MiddleName = "Middle";
                profile.PreferredContactMethodId = 0;
                profile.contactMechanism = null;
                profile.AssignedProducts = null;
                profile.organization = null;
                profile.PartyRole = null;
                profile.TelecommunicationNumber = null;
                listProfile.Add(profile);

                Status<IErrorData> errorStatus = new Status<IErrorData>();
                ObjectListOutput<IProfileDetail, IErrorData> output = new ObjectListOutput<IProfileDetail, IErrorData>() { list = listProfile, Status = errorStatus };

                return output;
            }
        }
		#endregion

		#region Private Methods
		/// <summary>
		/// Set Aspose License
		/// </summary>
		/// <returns>Error Status object</returns>
		private static Status<IErrorData> SetAsposeLicense()
		{
			Status<IErrorData> errorStatus = new Status<IErrorData>();
			try
			{
				Aspose.Cells.License asposeCellsLicense = new Aspose.Cells.License();
				//Gets the base directory that the assembly resolver uses to probe for assemblies + Aspose license file location
				asposeCellsLicense.SetLicense(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"ThirdParty\Aspose.Total.Lic"));
			}
			catch (Exception ex)
			{
				errorStatus.Success = false;
				errorStatus.ErrorCode = "Person.SetAsposeLicense.1";
				errorStatus.ErrorMsg = "Set Aspose License: " + ex.Message;
			}

			return errorStatus;
		}

		/// <summary>
		/// Create Excel WorkSheet
		/// </summary>
		/// <param name="workbook">Aspose Cells Workbook</param>
		/// <param name="worksheet">Aspose Cells WorkSheet</param>
		private static void CreateExcelWorkSheet(out Workbook workbook, out Worksheet worksheet)
		{
			//Instantiate a new Workbook
			workbook = new Workbook();
			//Clear all the worksheets
			workbook.Worksheets.Clear();
			//Add a new Sheet "Data"
			worksheet = workbook.Worksheets.Add("Data");
		}

		/// <summary>
		/// Export data in a list in a the the specified format
		/// </summary>
		/// <param name="listUsers">List of users to export</param>
		/// <param name="dataFormat">Retrun data in this format (default = CSV)</param>
		/// <returns>Array of bytes</returns>
		private byte[] ExportUserData(IList<LE.User> listUsers, SaveFormat dataFormat = SaveFormat.CSV)
		{
            byte[] bytes;
			Workbook workbook;
			Worksheet worksheet;
			MemoryStream memorystream = new MemoryStream();

			IDictionary<object, object> globals = new Dictionary<object, object>();
			//get the enabled custom field with the smallest sequence
			RequestParameter datafilter = new RequestParameter();
			datafilter.Pages.ResultsPerPage = 1;
			datafilter.Pages.StartRow = 1;
			datafilter.SortBy.Add("Sequence", "ASC");
			datafilter.FilterBy.Add("Enabled", "1");
			globals.Add(BaseType.RequestParameter, datafilter);

			ManageCustomFields manageCustomFields = new ManageCustomFields(_userClaims);
			IList<CustomField> customFieldList = manageCustomFields.GetCustomField(globals: globals, bookMasterId: _userClaims.CustomerMasterId, bookMasterTypeId: (int)BookMasterType.CustomerMasterId);
			bool customFieldsEnabled = ((customFieldList != null) && (customFieldList.Count > 0));

			IList<string> propertyNamesList = new List<string>()
			{
				"UserType", "FirstName", "LastName", "LoginName", "Products", "LastLogin", "Status", "IDP", "EffectiveDate", "ExpireDate"
			};

			CreateExcelWorkSheet(out workbook, out worksheet);

			//Manually add the row titles
			int col = 0;
			Cells cells = worksheet.Cells;

			Cell cell = cells[0, col++];
			cell.PutValue("User Type");

			worksheet.Cells[0, col++].PutValue("First Name");
			worksheet.Cells[0, col++].PutValue("Last Name");
			worksheet.Cells[0, col++].PutValue("Username");
			worksheet.Cells[0, col++].PutValue("Products");
			worksheet.Cells[0, col++].PutValue("Last Login");
			worksheet.Cells[0, col++].PutValue("Status");
			worksheet.Cells[0, col++].PutValue("IDP Flag");
			worksheet.Cells[0, col++].PutValue("User Effective");
			worksheet.Cells[0, col++].PutValue("User Expires");

			//Set the columns titles of the Custom Fields
			if (customFieldsEnabled)
			{
				propertyNamesList.Add("CustomField");
				string customFieldName = customFieldList[0].Name;
				worksheet.Cells[0, col++].PutValue(customFieldName);
			}
			int totalColumns = col;

			// Get the pagesetup object
			PageSetup pageSetup = worksheet.PageSetup;

			// Set bottom,left,right and top page margins
			pageSetup.BottomMarginInch = 0.5;
			pageSetup.LeftMarginInch = 0.25;
			pageSetup.RightMarginInch = 0.25;
			pageSetup.TopMarginInch = 0.5;

			string[] propertyNames = propertyNamesList.ToArray();
			worksheet.Cells.ImportCustomObjects(
				(System.Collections.ICollection)listUsers,
				propertyNames,
				false, //Don't show the field names
				1, //Start at second row
				0,
				listUsers.Count,
				true,
				"",
				false
			);

			switch (dataFormat)
			{
				case SaveFormat.CSV:
					//Autofits the columns width
					workbook.Worksheets[0].AutoFitColumns();
					break;
				case SaveFormat.Pdf:
					//Set the width columns
					col = 0;
					worksheet.Cells.SetColumnWidthInch(col++, 1.30);
					worksheet.Cells.SetColumnWidthInch(col++, 0.85);
					worksheet.Cells.SetColumnWidthInch(col++, 0.85);
					worksheet.Cells.SetColumnWidthInch(col++, 2.25);
					worksheet.Cells.SetColumnWidthInch(col++, 0.60);
					worksheet.Cells.SetColumnWidthInch(col++, 1.00);
					worksheet.Cells.SetColumnWidthInch(col++, 0.50);
					worksheet.Cells.SetColumnWidthInch(col++, 0.55);
					worksheet.Cells.SetColumnWidthInch(col++, 0.95);
					worksheet.Cells.SetColumnWidthInch(col++, 0.90);
					worksheet.Cells.SetColumnWidthInch(col++, 0.50);

					//Create a StyleFlag object.
					StyleFlag styleFlag = new StyleFlag
					{
						//Make the corresponding attributes ON.
						Font = true,
						VerticalAlignment = true,
						CellShading = true
					};
					Style style = workbook.CreateStyle();
					Range range = worksheet.Cells.CreateRange(0, 0, 1, totalColumns);
					style.Font.IsBold = true;
					style.VerticalAlignment = TextAlignmentType.Top;
					range.ApplyStyle(style, styleFlag);

					styleFlag = new StyleFlag
					{
						WrapText = true,
						VerticalAlignment = true
					};
                    cell = worksheet.Cells.LastCell;
                    range = worksheet.Cells.CreateRange(1, 0, cell.Row, totalColumns);
					style.IsTextWrapped = true;
					style.VerticalAlignment = TextAlignmentType.Top;
					range.ApplyStyle(style, styleFlag);

					foreach (Worksheet sheet in workbook.Worksheets)
					{
						sheet.PageSetup.Orientation = PageOrientationType.Landscape;
						sheet.PageSetup.FitToPagesWide = 1;
						sheet.PageSetup.FitToPagesTall = 0;
					}
					break;
				default:
					break;
			}

			//Autofits all rows in this worksheet
			workbook.Worksheets[0].AutoFitRows(true);

			//Convert to bytes array
            using (MemoryStream memoryStream = new MemoryStream())
            {
				workbook.Save(memorystream, dataFormat);

				//Get bytes
                bytes = memorystream.ToArray();
            }

			return bytes;
		}
		#endregion
	}
}


