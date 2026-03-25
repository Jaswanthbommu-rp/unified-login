using Aspose.Cells;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using System.Net;
using UnifiedLogin.BusinessLogic.Attributes;
using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
using UnifiedLogin.BusinessLogic.Services.Interfaces;
using UnifiedLogin.BusinessLogic.ThirdParty;
using UnifiedLogin.Core;
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
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("")]
    public class PersonController : BaseController
    {
        #region Private Fields
        private readonly IManagePersonAsync _managePerson;
        private readonly IProfileService _profileService;
        private readonly IManagePersonaAsync _managePersona;
        private readonly IManageCustomFieldsAsync _manageCustomFields;
        private readonly IManageUserLoginAsync _manageUserLogin;
        private readonly IManageUnifiedSettingsAsync _manageUnifiedSettings;
        #endregion

        #region Constructor
        /// <summary>
        /// Constructor with dependency injection
        /// </summary>
        public PersonController(
            IManagePersonAsync managePerson,
            IProfileService profileService,
            IManagePersonaAsync managePersona,
            IManageCustomFieldsAsync manageCustomFields,
            IManageUserLoginAsync manageUserLogin,
            IManageUnifiedSettingsAsync manageUnifiedSettings,
            IUserClaimsAccessor userClaimsAccessor) : base(userClaimsAccessor)
        {
            _managePerson = managePerson ?? throw new ArgumentNullException(nameof(managePerson));
            _profileService = profileService ?? throw new ArgumentNullException(nameof(profileService));
            _managePersona = managePersona ?? throw new ArgumentNullException(nameof(managePersona));
            _manageCustomFields = manageCustomFields ?? throw new ArgumentNullException(nameof(manageCustomFields));
            _manageUserLogin = manageUserLogin ?? throw new ArgumentNullException(nameof(manageUserLogin));
            _manageUnifiedSettings = manageUnifiedSettings ?? throw new ArgumentNullException(nameof(manageUnifiedSettings));
        }
        #endregion

        #region Public Methods

        /// <summary>
        /// Create a new person
        /// </summary>
        [HttpPost]
        [Route("persons")]
        [ProducesResponseType(typeof(Person.PersonOutputResult), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> CreatePerson([FromBody] Person person, CancellationToken cancellationToken = default)
        {
            if (person == null)
            {
                return BadRequest("Null parameter: Person.");
            }

            var repositoryResponse = await _managePerson.CreatePersonAsync(person, cancellationToken);

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
        [Route("persons/{realPageId}")]
        [HttpGet]
        [ProducesResponseType(typeof(ObjectOutput<IPerson, IErrorData>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetPerson(Guid realPageId, CancellationToken cancellationToken = default)
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

            var person = await _managePerson.GetPersonAsync(effectiveRealPageId, cancellationToken);

            if (person != null)
            {
                output.Status = errorStatus;
                output.obj = person;
                return Ok(output);
            }

            errorStatus.Success = false;
            errorStatus.ErrorCode = "Person.GetPerson.2";
            errorStatus.ErrorMsg = "Get Person: Invalid enterprise User Id";
            output.Status = errorStatus;
            return Ok(output);
        }

        /// <summary>
        /// Update Person
        /// </summary>
        [Route("persons/{realPageId}")]
        [HttpPut]
        [ProducesResponseType(typeof(Person), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> UpdatePerson(Guid realPageId, [FromBody] Person person, CancellationToken cancellationToken = default)
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

            var repositoryResponse = await _managePerson.UpdatePersonAsync(effectiveRealPageId, person, cancellationToken);

            if (repositoryResponse.Id == 0)
            {
                return BadRequest(repositoryResponse.ErrorMessage);
            }

            return Ok(person);
        }

        /// <summary>
        /// List person profiles
        /// </summary>
        [Route("persons")]
        [AuthorizeRight("viewusers", "viewonlysupporttoolaccess")]
        [HttpGet]
        [ProducesResponseType(typeof(ObjectUserListOutput<ProfileDetail, IErrorData>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> ListPersons([FromQuery] RequestParameter datafilter, CancellationToken cancellationToken = default)
        {
            var errorStatus = new Status<IErrorData>();

            if (datafilter == null)
            {
                datafilter = new RequestParameter();
            }

            var profileDetailList = await _profileService.ListPersonsAsync(null, null, null, datafilter, false, cancellationToken);

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

            output.OrganizationHasProductAssignmentError = profileDetailList.Count > 0
                 && profileDetailList.Any(x => x.PersonaHasProductError && x.userLogin?.Status != UserUiStatusType.Deactivated);

            return Ok(output);
        }

        /// <summary>
        /// List person profiles by organization id. Internal only
        /// </summary>
        [Route("persons/company/{realPageId}")]
        [AuthorizeRight("viewusers")]
        [HttpGet]
        [ProducesResponseType(typeof(ObjectListOutput<ProfileDetail, IErrorData>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> ListPersonsByOrg(Guid realPageId, [FromQuery] RequestParameter datafilter, CancellationToken cancellationToken = default)
        {
            var errorStatus = new Status<IErrorData>();

            if (datafilter == null)
            {
                datafilter = new RequestParameter();
            }

            var profileDetailList = await _profileService.ListPersonsAsync(null, realPageId, null, datafilter, false, cancellationToken);

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
        [Route("persons/export")]
        [AuthorizeRight("viewusers")]
        [HttpGet]
        [ProducesResponseType(typeof(ObjectOutput<string, IErrorData>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> ListUsersExport([FromQuery] RequestParameter datafilter, SaveFormat dataFormat = SaveFormat.Csv, string internationalDateFormat = "mmddyyyy", CancellationToken cancellationToken = default)
        {
            var output = new ObjectOutput<string, IErrorData>();
            var errorStatus = new Status<IErrorData>();
            DateTime parsedMaxValueDate = DateTime.Parse("Dec 31, 9999");

            if (datafilter == null)
            {
                datafilter = new RequestParameter();
            }

            var profileDetailList = await _profileService.ListPersonsAsync(null, null, null, datafilter, true, cancellationToken);

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

            var userLogin = await _manageUserLogin.GetUserLoginAsync(_userClaimsAccessor.UserRealPageGuid, _userClaimsAccessor.OrganizationPartyId, cancellationToken);

            foreach (var p in profileDetailList)
            {
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

            var customFieldsDataFilter = new RequestParameter();
            customFieldsDataFilter.Pages.ResultsPerPage = 1;
            customFieldsDataFilter.Pages.StartRow = 1;
            customFieldsDataFilter.SortBy.Add("Sequence", "ASC");
            customFieldsDataFilter.FilterBy.Add("Enabled", "1");
            var cfGlobals = new Dictionary<object, object> { [BaseType.RequestParameter] = customFieldsDataFilter };

            var customFieldList = await _manageCustomFields.GetCustomFieldAsync(cfGlobals, _userClaimsAccessor.OrganizationPartyId, cancellationToken);
            bool customFieldsEnabled = customFieldList != null && customFieldList.Count > 0;

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

                var internalSettings = await _manageUnifiedSettings.GetCompanyInternalSettingsAsync(_userClaimsAccessor.OrganizationRealPageGuid, "UPFM", "Company", cancellationToken);
                bool isOperatorEnabled = internalSettings?.Keys?.FirstOrDefault(p => p.Name == "owneroperatorrelationship")?.Value == "1";

                if (isOperatorEnabled)
                {
                    exportConfigurations.Add(new ExportDataFileConfiguration { Header = "Operator", MappedField = "Operator", PDFColumnWidth = "2.30", Preference = 17 });
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
        [HttpGet]
        [Route("person/persona/{realPageId}")]
        [ProducesResponseType(typeof(ObjectOutput<IPersona, IErrorData>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetActivePersona([FromRoute] Guid realPageId, CancellationToken cancellationToken = default)
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

            var persona = await _managePersona.GetFirstAvailablePersonaByCompanyAsync(realPageId, _userClaimsAccessor.OrganizationPartyId, cancellationToken);

            if (persona != null && persona.PersonaId > 0)
            {
                output.obj = persona;
                output.Status = errorStatus;
                return Ok(output);
            }

            errorStatus.Success = false;
            errorStatus.ErrorCode = "Person.GetActivePersona.2";
            errorStatus.ErrorMsg = "Get active persona: Invalid enterprise User Id.";
            output.Status = errorStatus;
            return Ok(output);
        }

        /// <summary>
        /// Used to get a list of personas by realpageId
        /// </summary>
        [HttpGet]
        [Route("person/personas/{realPageId}")]
        [ProducesResponseType(typeof(IList<Persona>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetListOfPersona([FromRoute] Guid realPageId, CancellationToken cancellationToken = default)
        {
            if (realPageId == Guid.Empty)
            {
                return BadRequest("Invalid parameter: realPageId");
            }

            var persona = await _managePersona.ListPersonaAsync(realPageId, cancellationToken);
            return Ok(persona);
        }

        #endregion
    }
}
