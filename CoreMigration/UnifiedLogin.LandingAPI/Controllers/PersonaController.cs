using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using UnifiedLogin.BusinessLogic.Attributes;
using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.BusinessLogic.Services.Interfaces;
using UnifiedLogin.Core;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product.UnifiedLogin;

namespace UnifiedLogin.LandingAPI.Controllers
{
    /// <summary>
    /// Persona Controller to hold all persona management related APIs
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("")]
    public class PersonaController : BaseController
    {
        #region Private fields
        private readonly IManagePersonaAsync _managePersona;
        private readonly IProductService _productService;
        private readonly IProductInternalSettingRepositoryAsync _productInternalSettingRepository;
        private readonly IManageUserRoleRightAsync _manageUserRoleRight;
        #endregion

        #region Constructor
        /// <summary>
        /// Constructor with dependency injection
        /// </summary>
        public PersonaController(
            IUserClaimsAccessor userClaimsAccessor,
            IManagePersonaAsync managePersona,
            IProductService productService,
            IProductInternalSettingRepositoryAsync productInternalSettingRepository,
            IManageUserRoleRightAsync manageUserRoleRight) : base(userClaimsAccessor)
        {
            _managePersona = managePersona ?? throw new ArgumentNullException(nameof(managePersona));
            _productService = productService ?? throw new ArgumentNullException(nameof(productService));
            _productInternalSettingRepository = productInternalSettingRepository ?? throw new ArgumentNullException(nameof(productInternalSettingRepository));
            _manageUserRoleRight = manageUserRoleRight ?? throw new ArgumentNullException(nameof(manageUserRoleRight));
        }
        #endregion

        #region Public Methods

        /// <summary>
        /// Get Persona Environment Type
        /// </summary>
        [HttpGet]
        [Route("persona/environment")]
        [ProducesResponseType(typeof(ObjectListOutput<PersonaEnvironment, IErrorData>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetPersonaEnvironmentType(CancellationToken cancellationToken = default)
        {
            var personaEnvironmentList = await _managePersona.GetPersonaEnvironmentTypeAsync(cancellationToken);

            if (personaEnvironmentList != null && personaEnvironmentList.Count > 0)
            {
                var output = new ObjectListOutput<PersonaEnvironment, IErrorData> { list = personaEnvironmentList };
                return Ok(output);
            }

            return NoContent();
        }

        /// <summary>
        /// Create a new persona
        /// </summary>
        [HttpPost]
        [Route("persona")]
        [ProducesResponseType(typeof(ObjectOutput<IPersona, IErrorData>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> CreatePersona(Guid personRealPageId, Guid organizationRealPageId, [FromBody] Persona persona, CancellationToken cancellationToken = default)
        {
            var output = new ObjectOutput<IPersona, IErrorData>();
            var errorStatus = new Status<IErrorData>();
            output.obj = persona;

            var userRealPageId = _userClaimsAccessor.UserRealPageGuid;
            personRealPageId = (personRealPageId == Guid.Empty) ? userRealPageId : personRealPageId;

            if (personRealPageId == Guid.Empty)
            {
                errorStatus.Success = false;
                errorStatus.ErrorCode = "200.3";
                errorStatus.ErrorMsg = "Invalid parameter: personRealPageId";
                output.Status = errorStatus;
                return BadRequest(output);
            }

            if (organizationRealPageId == Guid.Empty)
            {
                errorStatus.Success = false;
                errorStatus.ErrorCode = "200.3";
                errorStatus.ErrorMsg = "Invalid parameter Organization realPageId";
                output.Status = errorStatus;
                return BadRequest(output);
            }

            if (persona == null)
            {
                errorStatus.Success = false;
                errorStatus.ErrorCode = "200.3";
                errorStatus.ErrorMsg = "Null parameter: Persona.";
                output.Status = errorStatus;
                return BadRequest(output);
            }

            var repositoryResponse = await _managePersona.CreatePersonaAsync(personRealPageId, organizationRealPageId, persona, cancellationToken);

            if (repositoryResponse.Id == 0)
            {
                errorStatus.Success = false;
                errorStatus.ErrorCode = "200.3";
                errorStatus.ErrorMsg = repositoryResponse.ErrorMessage;
                output.Status = errorStatus;
                return BadRequest(output);
            }

            output.obj.PersonaId = repositoryResponse.Id;
            output.Status = errorStatus;
            return Ok(output);
        }

        /// <summary>
        /// Get Persona details
        /// </summary>
        [HttpGet]
        [Route("persona")]
        [ProducesResponseType(typeof(Persona), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetPersona(long personaId = 0, CancellationToken cancellationToken = default)
        {
            var targetPersonaId = personaId == 0 ? _userClaimsAccessor.PersonaId : personaId;
            var persona = await _managePersona.GetPersonaAsync(targetPersonaId, withRights: true, cancellationToken);

            if (persona == null)
            {
                return NoContent();
            }

            var personaList = await _managePersona.ListActivePersonaAsync(persona.RealPageId, false, cancellationToken);

            persona.hasMultiPersona = personaList.Count(p => p.OrganizationPartyId == persona.OrganizationPartyId) > 1;
            persona.hasMultiCompany = personaList.Count(p => p.OrganizationPartyId != persona.OrganizationPartyId
                && p.Organization.RealPageId != DefaultUserClaim.ExternalCompanyRealPageId) > 0;

            return Ok(persona);
        }

        /// <summary>
        /// Used to trigger the notification event that the user changed company
        /// </summary>
        [HttpPost]
        [AuthorizeScope("userinfoapi")]
        [Route("persona/{personaId}/company")]
        [ProducesResponseType((int)HttpStatusCode.Accepted)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> ChangeCompany(long personaId = 0, CancellationToken cancellationToken = default)
        {
            var productInternalSettingList = await _productInternalSettingRepository.GetProductInternalSettingsAsync((int)ProductEnum.UnifiedPlatform, cancellationToken);

            var unifiedLoginClientId = productInternalSettingList
                .FirstOrDefault(a => a.Name.Equals("UnifiedLoginServerClientName", StringComparison.OrdinalIgnoreCase))?.Value;

            var clientCode = _userClaimsAccessor.ClientCode;
            long targetPersonaId = personaId;

            if (!clientCode.Equals(unifiedLoginClientId, StringComparison.OrdinalIgnoreCase))
            {
                if (_userClaimsAccessor.PersonaId != 0 && personaId == 0)
                {
                    targetPersonaId = _userClaimsAccessor.PersonaId;
                }
            }
            else
            {
                targetPersonaId = personaId;
            }

            var currentPersona = await _managePersona.GetPersonaAsync(_userClaimsAccessor.PersonaId, withRights: true, cancellationToken);
            if (currentPersona == null)
            {
                return BadRequest("Current persona not found");
            }

            var personaList = await _managePersona.ListActivePersonaAsync(currentPersona.RealPageId, false, cancellationToken);

            if (personaList.Any(p => p.PersonaId == targetPersonaId))
            {
                var result = await _managePersona.ChangeCompanyNotificationAsync(targetPersonaId, cancellationToken);
                return result == Guid.Empty ? BadRequest() : Accepted();
            }

            return Unauthorized();
        }

        /// <summary>
        /// Get Persona company list
        /// </summary>
        [HttpGet]
        [Route("personas")]
        [ProducesResponseType(typeof(ObjectListOutput<PersonaCompany, IErrorData>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetPersonasList(CancellationToken cancellationToken = default)
        {
            var output = new ObjectListOutput<PersonaCompany, IErrorData>();
            var userRealPageId = _userClaimsAccessor.UserRealPageGuid;

            var personaList = await _managePersona.ListActivePersonaAsync(userRealPageId, true, cancellationToken);
            var pcl = new List<PersonaCompany>();

            foreach (var persona in personaList)
            {
                if (persona.Organization.RealPageId != DefaultUserClaim.ExternalCompanyRealPageId)
                {
                    if (pcl.All(p => p.CompanyRealPageId != persona.Organization.RealPageId))
                    {
                        pcl.Add(new PersonaCompany
                        {
                            CompanyName = persona.Organization.Name,
                            CompanyRealPageId = persona.Organization.RealPageId,
                            Personas = new List<PersonaCompanyDetails>()
                        });
                    }

                    var currentCompanyPersonaList = pcl.Find(p => p.CompanyRealPageId == persona.Organization.RealPageId).Personas;
                    currentCompanyPersonaList.Add(new PersonaCompanyDetails
                    {
                        PersonaId = persona.PersonaId,
                        Name = persona.Name,
                    });
                }
            }

            pcl = pcl.OrderBy(p => p.CompanyName).ToList();
            output.list = pcl;

            return Ok(output);
        }

        /// <summary>
        /// Get persona assigned product(s) with optional selection type
        /// </summary>
        [HttpGet]
        [Route("personas/products")]
        [ProducesResponseType(typeof(ObjectListOutput<PersonaProductUserDetails, IErrorData>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetProductsByPersona([FromQuery] ProductSelectType? type = null, CancellationToken cancellationToken = default)
        {
            var output = new ObjectListOutput<PersonaProductUserDetails, IErrorData>();
            var errorStatus = new Status<IErrorData>();

            var personaId = _userClaimsAccessor.PersonaId;
            var persona = await _managePersona.GetPersonaAsync(personaId, withRights: true, cancellationToken);

            if (persona == null)
            {
                errorStatus.Success = false;
                errorStatus.ErrorCode = "400";
                errorStatus.ErrorMsg = "Active persona not found!";
                output.Status = errorStatus;
                return BadRequest(output);
            }

            var productList = await _productService.GetAssignedProductsByPersonaAsync(persona, type, null, cancellationToken);

            output.list = productList;
            output.Status = errorStatus;
            return Ok(output);
        }

        /// <summary>
        /// Expire and create a product setting of a persona
        /// </summary>
        [HttpPut]
        [Route("personas/products/{productId}/productSettings")]
        [ProducesResponseType(typeof(ObjectOutput<RepositoryResponse, IErrorData>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> UpdateUserProductSetting(int? productId, [FromBody] ProductSetting productSetting, CancellationToken cancellationToken = default)
        {
            var output = new ObjectOutput<RepositoryResponse, IErrorData>();
            var errorStatus = new Status<IErrorData>();

            if (productId == null)
            {
                errorStatus.Success = false;
                errorStatus.ErrorCode = "200.3";
                errorStatus.ErrorMsg = "Null parameter: productId.";
                output.Status = errorStatus;
                return BadRequest(output);
            }

            if (productSetting == null)
            {
                errorStatus.Success = false;
                errorStatus.ErrorCode = "200.3";
                errorStatus.ErrorMsg = "Null parameter: productSetting.";
                output.Status = errorStatus;
                return BadRequest(output);
            }

            var personaId = _userClaimsAccessor.PersonaId;
            var persona = await _managePersona.GetPersonaAsync(personaId, withRights: false, cancellationToken);

            if (persona == null)
            {
                errorStatus.Success = false;
                errorStatus.ErrorCode = "400";
                errorStatus.ErrorMsg = "Active persona not found!";
                output.Status = errorStatus;
                return BadRequest(output);
            }

            var response = await _productService.UpdateProductSettingAsync(productSetting, persona.PersonaId, cancellationToken);

            if (response.Id == 0)
            {
                errorStatus.ErrorCode = "500";
                errorStatus.ErrorMsg = response.ErrorMessage;
                errorStatus.Success = false;
            }

            output.Status = errorStatus;
            output.obj = response;
            return Ok(output);
        }

        /// <summary>
        /// Used to get a list of users roles by persona and productid
        /// </summary>
        [HttpGet]
        [Route("persona/{personaId}/product/{productId}/permissions")]
        [ProducesResponseType(typeof(IList<Role>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetPersonaRolesByProduct([FromRoute] long personaId, [FromRoute] ProductEnum productId, CancellationToken cancellationToken = default)
        {
            if (personaId == 0 || productId == 0)
            {
                return BadRequest("Invalid personaId or productId");
            }

            var roleList = await _manageUserRoleRight.GetAssignedRoleForPersonaAsync(productId, personaId, null, cancellationToken);

            return Ok(roleList);
        }

        #endregion
    }
}
