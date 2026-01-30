using Aspose.Cells;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using UnifiedLogin.BusinessLogic.Attributes;
using UnifiedLogin.BusinessLogic.Logic;
using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.BusinessLogic.ThirdParty;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Landing.Export;
using LE = UnifiedLogin.SharedObjects.Landing.Export;

namespace UnifiedLogin.LandingAPI.Controllers
{
    /// <summary>
    /// Person Controller to hold all user management related APIs
    /// Migrated to .NET Core 8.0 with dependency injection and async/await patterns
    /// </summary>
    [Authorize]
    [ApiController]
    public class PersonController : ControllerBase
    {
        #region Private Fields
        private readonly IManagePerson _managePerson;
        private readonly IManageProfile _manageProfile;
        private readonly IManagePersona _managePersona;
        private readonly IManageCustomFields _manageCustomFields;
        private readonly IUserClaimsAccessor _userClaimsAccessor;
        private readonly IManageUserLogin _manageUserLogin;
        private readonly IManageUnifiedSettings _manageUnifiedSettings;
        #endregion

        #region Constructor
        /// <summary>
        /// Constructor with dependency injection
        /// </summary>
        /// <param name="managePerson">Person management service</param>
        /// <param name="manageProfile">Profile management service</param>
        /// <param name="managePersona">Persona management service</param>
        /// <param name="manageCustomFields">Custom fields management service</param>
        /// <param name="userClaimsAccessor">User claims accessor</param>
        /// <param name="manageUserLogin">User login management service</param>
        /// <param name="manageUnifiedSettings">Unified settings management service</param>
        public PersonController(
            IManagePerson managePerson,
            IManageProfile manageProfile,
            IManagePersona managePersona,
            IManageCustomFields manageCustomFields,
            IUserClaimsAccessor userClaimsAccessor,
            IManageUserLogin manageUserLogin,
            IManageUnifiedSettings manageUnifiedSettings)
        {
            _managePerson = managePerson ?? throw new ArgumentNullException(nameof(managePerson));
            _manageProfile = manageProfile ?? throw new ArgumentNullException(nameof(manageProfile));
            _managePersona = managePersona ?? throw new ArgumentNullException(nameof(managePersona));
            _manageCustomFields = manageCustomFields ?? throw new ArgumentNullException(nameof(manageCustomFields));
            _userClaimsAccessor = userClaimsAccessor ?? throw new ArgumentNullException(nameof(userClaimsAccessor));
            _manageUserLogin = manageUserLogin ?? throw new ArgumentNullException(nameof(manageUserLogin));
            _manageUnifiedSettings = manageUnifiedSettings ?? throw new ArgumentNullException(nameof(manageUnifiedSettings));
        }
        #endregion

        #region Public Methods

        /// <summary>
        /// Create a new person
        /// </summary>
        /// <param name="person">Person object of the parameter values</param>
        /// <returns>Response with Success Message</returns>
        [HttpPost]
        [Route("persons")]
        [ProducesResponseType(typeof(Person.PersonOutputResult), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> CreatePerson([FromBody] Person person)
        {
            if (person == null)
            {
                return BadRequest("Null parameter: Person.");
            }

            var repositoryResponse = await Task.Run(() => _managePerson.CreatePerson(person));

            if (repositoryResponse.Id == 0)
            {
                return BadRequest(repositoryResponse.ErrorMessage);
            }

            var result = new Person.PersonOutputResult
            {
                RealPageId = repositoryResponse.RealPageId
            };

            return Ok(result);
        }

        /// <summary>
        /// Get person detail
        /// </summary>
        /// <param name="realPageId">User unique identifier</param>
        /// <returns>Person object</returns>
        [Route("persons/{realPageId}")]
        [HttpGet]
        [ProducesResponseType(typeof(ObjectOutput<IPerson, IErrorData>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetPerson(Guid realPageId)
        {
            var output = new ObjectOutput<IPerson, IErrorData>();
            var errorStatus = new Status<IErrorData>();

            var effectiveRealPageId = (realPageId == Guid.Empty) ? _userClaimsAccessor.UserRealPageGuid : realPageId;

            if (effectiveRealPageId == Guid.Empty)
            {
                errorStatus.Success = false;
                errorStatus.ErrorCode = "Person.GetPerson.1";
                errorStatus.ErrorMsg = "Get Person: Invalid parameter enterprise User Id";
                output.Status = errorStatus;
                return Ok(output);
            }

            var person = await Task.Run(() => _managePerson.GetPerson(effectiveRealPageId));

            if (person != null)
            {
                output.Status = errorStatus;
                output.obj = person;
                return Ok(output);
            }

            // When trying to get a Person that doesn't exist / deleted
            errorStatus.Success = false;
            errorStatus.ErrorCode = "Person.GetPerson.2";
            errorStatus.ErrorMsg = "Get Person: Invalid enterprise User Id";
            output.Status = errorStatus;
            return Ok(output);
        }

        /// <summary>
        /// Update Person
        /// </summary>
        /// <param name="realPageId">User unique identifier</param>
        /// <param name="person">Person object of the parameter values</param>
        /// <returns>Response with Success Message</returns>
        [Route("persons/{realPageId}")]
        [HttpPut]
        [ProducesResponseType(typeof(Person), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> UpdatePerson(Guid realPageId, [FromBody] Person person)
        {
            var effectiveRealPageId = (realPageId == Guid.Empty) ? _userClaimsAccessor.UserRealPageGuid : realPageId;

            if (effectiveRealPageId == Guid.Empty)
            {
                return BadRequest("Invalid parameter: realPageId");
            }

            if (person == null)
            {
                return BadRequest("Null parameter: Person");
            }

            var repositoryResponse = await Task.Run(() => _managePerson.UpdatePerson(effectiveRealPageId, person));

            if (repositoryResponse.Id == 0)
            {
                return BadRequest(repositoryResponse.ErrorMessage);
            }

            return Ok(person);
        }

        /// <summary>
        /// List person profiles
        /// </summary>
        /// <param name="datafilter">Request parameter for filtering and pagination</param>
        /// <returns>List of Person object</returns>
        [Route("persons")]
        [AuthorizeRight("viewusers", "viewonlysupporttoolaccess")]
        [HttpGet]
        [ProducesResponseType(typeof(ObjectUserListOutput<ProfileDetail, IErrorData>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> ListPersons([FromQuery] RequestParameter datafilter)
        {
            var globals = new Dictionary<object, object>();
            var errorStatus = new Status<IErrorData>();

            if (datafilter == null)
            {
                datafilter = new RequestParameter();
            }

            globals.Add(BaseType.RequestParameter, datafilter);

            var userClaim = _userClaimsAccessor.GetUserClaim();
            var profileDetailList = await Task.Run(() => _manageProfile.ListProfileDetails(globals: globals, organizationRealPageId: null));

            int totalRecords = profileDetailList.Count > 0 ? profileDetailList[0].TotalRecords : 0;
            decimal resultsPerPage = ((datafilter.Pages.ResultsPerPage == 100) && (totalRecords > 0)) ? totalRecords : datafilter.Pages.ResultsPerPage;
            resultsPerPage = (resultsPerPage == 0) ? totalRecords : resultsPerPage;

            var pagingSummary = new PagingSummary()
            {
                TotalRecords = totalRecords,
                TotalPages = (resultsPerPage == 0) ? 0 : (int)Math.Ceiling(totalRecords / resultsPerPage)
            };

            var output = new ObjectUserListOutput<ProfileDetail, IErrorData>()
            {
                list = profileDetailList,
                Status = errorStatus,
                pagingSummary = pagingSummary
            };

            output.OrganizationHasProductAssignmentError = profileDetailList != null && profileDetailList.Count > 0
                ? profileDetailList.Any(x => x.PersonaHasProductError && x.userLogin.Status != UserUiStatusType.Deactivated)
                : false;

            return Ok(output);
        }

        /// <summary>
        /// List person profiles by organization id. Internal only
        /// </summary>
        /// <param name="realPageId">Organization EnterpriseId</param>
        /// <param name="datafilter">Request parameter for filtering and pagination</param>
        /// <returns>List of Person object</returns>
        [Route("persons/company/{realPageId}")]
        [AuthorizeRight("viewusers")]
        [HttpGet]
        [ProducesResponseType(typeof(ObjectListOutput<ProfileDetail, IErrorData>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> ListPersonsByOrg(Guid realPageId, [FromQuery] RequestParameter datafilter)
        {
            var globals = new Dictionary<object, object>();
            var errorStatus = new Status<IErrorData>();

            if (datafilter == null)
            {
                datafilter = new RequestParameter();
            }

            globals.Add(BaseType.RequestParameter, datafilter);

            var profileDetailList = await Task.Run(() => _manageProfile.ListProfileDetails(globals: globals, organizationRealPageId: realPageId));

            int totalRecords = profileDetailList.Count > 0 ? profileDetailList[0].TotalRecords : 0;
            decimal resultsPerPage = ((datafilter.Pages.ResultsPerPage == 100) && (totalRecords > 0)) ? totalRecords : datafilter.Pages.ResultsPerPage;

            var pagingSummary = new PagingSummary()
            {
                TotalRecords = totalRecords,
                TotalPages = (int)Math.Ceiling(totalRecords / resultsPerPage)
            };

            var output = new ObjectListOutput<ProfileDetail, IErrorData>()
            {
                list = profileDetailList,
                Status = errorStatus,
                pagingSummary = pagingSummary
            };

            return Ok(output);
        }

        /// <summary>
        /// Export Users
        /// </summary>
        /// <param name="datafilter">Filter, Sort, Paginate</param>
        /// <param name="dataFormat">Return data in this format (default = CSV)</param>
        /// <param name="internationalDateFormat">InternationalDateFormat</param>
        /// <returns>List of Person object</returns>
        [Route("persons/export")]
        [AuthorizeRight("viewusers")]
        [HttpGet]
        [ProducesResponseType(typeof(ObjectOutput<string, IErrorData>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> ListUsersExport([FromQuery] RequestParameter datafilter, SaveFormat dataFormat = SaveFormat.CSV, string internationalDateFormat = "mmddyyyy")
        {
            var globals = new Dictionary<object, object>();
            var output = new ObjectOutput<string, IErrorData>();
            var errorStatus = new Status<IErrorData>();
            DateTime parsedMaxValueDate = DateTime.Parse("Dec 31, 9999");

            if (datafilter == null)
            {
                datafilter = new RequestParameter();
            }

            globals.Add(BaseType.RequestParameter, datafilter);
            globals.Add("isExport", true);

            var userClaim = _userClaimsAccessor.GetUserClaim();
            var profileDetailList = await Task.Run(() => _manageProfile.ListProfileDetails(globals));

            // Determine date format
            string dateFormat = internationalDateFormat.Trim().ToLower() switch
            {
                "mmddyyyy" => "MM/dd/yyyy",
                "ddmmyyyy" => "dd/MM/yyyy",
                "yyyymmdd" => "yyyy/MM/dd",
                _ => "MM/dd/yyyy"
            };

            if (profileDetailList == null || profileDetailList.Count == 0)
            {
                errorStatus.Success = false;
                errorStatus.ErrorCode = "Person.ListUsersExport.1";
                errorStatus.ErrorMsg = "List Users Export: No data";
                output.Status = errorStatus;
                return Ok(output);
            }

            var listUsers = new List<LE.User>();

            // Get user login for current user
            var userLogin = await Task.Run(() => _manageUserLogin.GetUserLogin(_userClaimsAccessor.UserRealPageGuid, _userClaimsAccessor.OrganizationPartyId));

            foreach (var p in profileDetailList)
            {
                // User Type
                string userType = string.Empty;
                if (p.userLogin.UserRoleType != null)
                {
                    Enum enumUserRole = p.userLogin.UserRoleType;
                    Type enumUserRoleType = enumUserRole.GetType();
                    System.Reflection.FieldInfo fieldInfoUserRole = enumUserRoleType.GetField(enumUserRole.ToString());
                    object[] userRoleAttributes = fieldInfoUserRole.GetCustomAttributes(typeof(DescriptionAttribute), false);
                    userType = userRoleAttributes.Length == 0 ? enumUserRole.ToString() : ((DescriptionAttribute)userRoleAttributes[0]).Description;
                }

                listUsers.Add(new LE.User()
                {
                    UserType = userType,
                    FirstName = p.FirstName,
                    MiddleName = p.MiddleName,
                    LastName = p.LastName,
                    LoginName = p.userLogin.LoginName,
                    Products = p.SummaryCount.TotalAssignedProducts,
                    LastLogin = p.userLogin.LastLogin != null ? p.userLogin.LastLogin?.ToString(dateFormat) : string.Empty,
                    Status = p.userLogin.Status.ToString().Equals("disabled", StringComparison.OrdinalIgnoreCase) ? "Deactivated" : p.userLogin.Status.ToString(),
                    IDP = p.userLogin.Is3rdPartyIDP ? "Yes" : "No",
                    EffectiveDate = p.userLogin.FromDate != null ? p.userLogin.FromDate?.ToString(dateFormat) : string.Empty,
                    ExpireDate = ((p.userLogin.ThruDate == null) || (DateTime.Compare(p.userLogin.ThruDate.Value, parsedMaxValueDate) == 0)) ? string.Empty : p.userLogin.ThruDate?.ToString(dateFormat),
                    CustomField = p.CustomField,
                    EmployeeId = p.EmployeeId,
                    Operator = p.Operator,
                    UserRelationshipType = p.UserRelationshipType,
                    CompanyName = p.CompanyName,
                    Supervisor = p.SuperVisorUser.LoginName != null ? p.SuperVisorUser.LoginName + " (" + p.SuperVisorUser.FirstName + " " + p.SuperVisorUser.LastName + ")" : "",
                    PhoneNumber = p.PhoneNumber,
                    PhoneNumberType = p.PhoneNumberType
                });
            }

            errorStatus = DataExport.SetAsposeLicense();

            if (!errorStatus.Success)
            {
                output.Status = errorStatus;
                return Ok(output);
            }

            var exportConfigurations = new List<ExportDataFileConfiguration>
            {
                new ExportDataFileConfiguration { Header = "User Type", MappedField = "UserType", PDFColumnWidth = "1.30", Preference = 2 },
                new ExportDataFileConfiguration { Header = "First Name", MappedField = "FirstName", PDFColumnWidth = "0.85", Preference = 3 },
                new ExportDataFileConfiguration { Header = "Last Name", MappedField = "LastName", PDFColumnWidth = "0.85", Preference = 4 },
                new ExportDataFileConfiguration { Header = "Employee ID", MappedField = "EmployeeId", PDFColumnWidth = "0.85", Preference = 5 },
                new ExportDataFileConfiguration { Header = "Username", MappedField = "LoginName", PDFColumnWidth = "2.25", Preference = 6 },
                new ExportDataFileConfiguration { Header = "Products", MappedField = "Products", PDFColumnWidth = "0.60", Preference = 7 },
                new ExportDataFileConfiguration { Header = "Last Login", MappedField = "LastLogin", PDFColumnWidth = "1.00", Preference = 8 },
                new ExportDataFileConfiguration { Header = "Phone Number", MappedField = "PhoneNumber", PDFColumnWidth = "1.00", Preference = 9 },
                new ExportDataFileConfiguration { Header = "Phone Number Type", MappedField = "PhoneNumberType", PDFColumnWidth = "1.30", Preference = 10 },
                new ExportDataFileConfiguration { Header = "Status", MappedField = "Status", PDFColumnWidth = "0.60", Preference = 11 },
                new ExportDataFileConfiguration { Header = "IDP Flag", MappedField = "IDP", PDFColumnWidth = "0.55", Preference = 12 },
                new ExportDataFileConfiguration { Header = "User Effective", MappedField = "EffectiveDate", PDFColumnWidth = "0.95", Preference = 13 },
                new ExportDataFileConfiguration { Header = "User Expires", MappedField = "ExpireDate", PDFColumnWidth = "0.90", Preference = 14 }
            };

            // Get the enabled custom field with the smallest sequence
            var CFglobals = new Dictionary<object, object>();
            var customFieldsDataFilter = new RequestParameter();
            customFieldsDataFilter.Pages.ResultsPerPage = 1;
            customFieldsDataFilter.Pages.StartRow = 1;
            customFieldsDataFilter.SortBy.Add("Sequence", "ASC");
            customFieldsDataFilter.FilterBy.Add("Enabled", "1");
            CFglobals.Add(BaseType.RequestParameter, customFieldsDataFilter);

            var customFieldList = await Task.Run(() => _manageCustomFields.GetCustomField(globals: CFglobals, partyId: _userClaimsAccessor.OrganizationPartyId));
            bool customFieldsEnabled = ((customFieldList != null) && (customFieldList.Count > 0));

            if (customFieldsEnabled)
            {
                string customFieldName = customFieldList[0].Name;
                exportConfigurations.Add(new ExportDataFileConfiguration { Header = customFieldName, MappedField = "CustomField", PDFColumnWidth = "", Preference = 15 });
            }

            if (FeatureFlag.GetUserCompanyAssociationFeatureFlag())
            {
                var exportConfigurationsAdditionalFields = new List<ExportDataFileConfiguration>
                {
                    new ExportDataFileConfiguration { Header = "User Relationship", MappedField = "UserRelationshipType", PDFColumnWidth = "2.20", Preference = 1 },
                    new ExportDataFileConfiguration { Header = "Company Name", MappedField = "CompanyName", PDFColumnWidth = "1.30", Preference = 16 }
                };
                exportConfigurations.AddRange(exportConfigurationsAdditionalFields);

                if (GetUnifiedSettingsForOperator(_userClaimsAccessor.OrganizationRealPageGuid, "Company"))
                {
                    var exportConfigurations_operator = new List<ExportDataFileConfiguration>
                    {
                        new ExportDataFileConfiguration { Header = "Operator", MappedField = "Operator", PDFColumnWidth = "2.30", Preference = 17 }
                    };
                    exportConfigurations.AddRange(exportConfigurations_operator);
                }
            }

            exportConfigurations.Add(new ExportDataFileConfiguration { Header = "Supervisor", MappedField = "Supervisor", PDFColumnWidth = "2.25", Preference = 18 });

            byte[] plainBytes = DataExport.ExportDataToFile<LE.User>(exportConfigurations.OrderBy(p => p.Preference).ToList(), listUsers, dataFormat);

            output = new ObjectOutput<string, IErrorData>()
            {
                obj = Convert.ToBase64String(plainBytes),
                Status = errorStatus
            };

            return Ok(output);
        }

        #endregion

        #region Persona

        /// <summary>
        /// Used to get a persons active persona by realpageId
        /// </summary>
        /// <param name="realPageId">User personaId</param>
        /// <returns>A list of personas for the give user id</returns>
        [HttpGet]
        [Route("person/persona/{realPageId}")]
        [ProducesResponseType(typeof(ObjectOutput<IPersona, IErrorData>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetActivePersona([FromRoute] Guid realPageId)
        {
            var output = new ObjectOutput<IPersona, IErrorData>();
            var errorStatus = new Status<IErrorData>();

            if (realPageId == Guid.Empty)
            {
                errorStatus.Success = false;
                errorStatus.ErrorCode = "Person.GetActivePersona.1";
                errorStatus.ErrorMsg = "Get active persona: Invalid parameter enterprise User Id";
                output.Status = errorStatus;
                return Ok(output);
            }

            var userClaim = _userClaimsAccessor.GetUserClaim();
            var persona = await Task.Run(() => _managePersona.GetFirstAvailablePersonaByCompany(realPageId, _userClaimsAccessor.OrganizationPartyId));

            if ((persona != null) && (persona.PersonaId > 0))
            {
                output.obj = persona;
                output.Status = errorStatus;
                return Ok(output);
            }

            // When trying to get a Person that doesn't exist / deleted
            errorStatus.Success = false;
            errorStatus.ErrorCode = "Person.GetActivePersona.2";
            errorStatus.ErrorMsg = "Get active persona: Invalid enterprise User Id.";
            output.Status = errorStatus;
            return Ok(output);
        }

        /// <summary>
        /// Used to get a list of personas by realpageId
        /// </summary>
        /// <param name="realPageId">User RealPageId</param>
        /// <returns>A list of personas for the give user id</returns>
        [HttpGet]
        [Route("person/personas/{realPageId}")]
        [ProducesResponseType(typeof(IList<Persona>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetListOfPersona([FromRoute] Guid realPageId)
        {
            if (realPageId == Guid.Empty)
            {
                return BadRequest("Invalid parameter: realPageId");
            }

            var persona = await Task.Run(() => _managePersona.ListPersona(realPageId));
            return Ok(persona);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Get unified settings for operator
        /// </summary>
        /// <param name="realPageId">Organization RealPageId</param>
        /// <param name="settingType">Setting type</param>
        /// <returns>Boolean indicating if operator setting is enabled</returns>
        private bool GetUnifiedSettingsForOperator(Guid realPageId, string settingType)
        {
            var data = _manageUnifiedSettings.GetCompanyInternalSettings(realPageId, "UPFM", settingType);
            return data?.Keys?.Where(p => p.Name == "owneroperatorrelationship")?.FirstOrDefault()?.Value == "1";
        }

        #endregion
    }
}
