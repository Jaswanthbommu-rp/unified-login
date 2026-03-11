using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using UnifiedLogin.BusinessLogic.Attributes;
using UnifiedLogin.BusinessLogic.Extensions;
using UnifiedLogin.BusinessLogic.Interfaces;
using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
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
    public class PersonaController : ControllerBase
    {
        #region Private fields
        private readonly IManagePersona _managePersona;
        private readonly IManagePersonaAsync _managePersonaAsync;
        private readonly IManageProduct _manageProduct;
        private readonly IProductInternalSettingRepository _productInternalSettingRepository;
        private readonly IManageUserRoleRight _manageUserRoleRight;
        private readonly ICacheService _cacheService;
        #endregion

        #region Constructor
        /// <summary>
        /// Constructor with dependency injection
        /// </summary>
        /// <param name="userClaimsAccessor">Accessor for current authenticated user's claims</param>
        /// <param name="managePersona">Service for managing persona operations</param>
        /// <param name="manageProduct">Service for managing product operations</param>
        /// <param name="productInternalSettingRepository">Repository for product internal settings</param>
        /// <param name="manageUserRoleRight">Service for managing user roles and rights</param>
        public PersonaController(
            IUserClaimsAccessor userClaimsAccessor,
            IManagePersona managePersona,
            IManageProduct manageProduct,
            IProductInternalSettingRepository productInternalSettingRepository,
            IManageUserRoleRight manageUserRoleRight,
            IManagePersonaAsync managePersonaAsync,
            ICacheService cacheService)
        {
            _managePersona = managePersona ?? throw new ArgumentNullException(nameof(managePersona));
            _manageProduct = manageProduct ?? throw new ArgumentNullException(nameof(manageProduct));
            _productInternalSettingRepository = productInternalSettingRepository ?? throw new ArgumentNullException(nameof(productInternalSettingRepository));
            _manageUserRoleRight = manageUserRoleRight ?? throw new ArgumentNullException(nameof(manageUserRoleRight));
            _managePersonaAsync = managePersonaAsync ?? throw new ArgumentNullException(nameof(managePersonaAsync));

            _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
        }
        #endregion

        #region Public Methods

        /// <summary>
        /// Get Persona Environment Type
        /// </summary>
        /// <returns>Response with Success Message</returns>
        [HttpGet]
        [Route("persona/environment")]
        [ProducesResponseType(typeof(ObjectListOutput<PersonaEnvironment, IErrorData>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetPersonaEnvironmentType()
        {
            var personaEnvironmentList = await Task.Run(() => _managePersona.GetPersonaEnvironmentType());

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
        /// <param name="personRealPageId">User unique identifier</param>
        /// <param name="organizationRealPageId">Organization unique identifier</param>
        /// <param name="persona">Person object of the parameter values</param>
        /// <returns>Response with Success Message</returns>
        [HttpPost]
        [Route("persona")]
        [ProducesResponseType(typeof(ObjectOutput<IPersona, IErrorData>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> CreatePersona(Guid personRealPageId, Guid organizationRealPageId, [FromBody] Persona persona)
        {
            var output = new ObjectOutput<IPersona, IErrorData>();
            var errorStatus = new Status<IErrorData>();
            output.obj = persona;

            // Use personRealPageId from parameter or fall back to current user
            var userRealPageId = User.RealPageId();
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

            var repositoryResponse = await Task.Run(() =>
                _managePersona.CreatePersona(personRealPageId, organizationRealPageId, persona));

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
        /// <param name="personaId">Optional persona ID, defaults to current user's persona</param>
        /// <returns>Persona details</returns>
        [HttpGet]
        [Route("persona")]
        [ProducesResponseType(typeof(Persona), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetPersona(long personaId = 0)
        {
            var targetPersonaId = personaId == 0 ? User.PersonaId() : personaId;
            var persona = await Task.Run(() => _managePersona.GetPersona(targetPersonaId));

            if (persona == null)
            {
                return NoContent();
            }

            var personaList = await Task.Run(() => _managePersona.ListActivePersona(persona.RealPageId, false));
            
            persona.hasMultiPersona = personaList.Count(p => p.OrganizationPartyId == persona.OrganizationPartyId) > 1;
            persona.hasMultiCompany = personaList.Count(p => p.OrganizationPartyId != persona.OrganizationPartyId
                && p.Organization.RealPageId != DefaultUserClaim.ExternalCompanyRealPageId) > 0;

            return Ok(persona);
        }

        /// <summary>
        /// Get Persona details
        /// </summary>
        /// <param name="personaId">Optional persona ID, defaults to current user's persona</param>
        /// <param name="token"></param>
        /// <returns>Persona details</returns>
        [HttpGet]
        [Route("persona2")]
        [ProducesResponseType(typeof(Persona), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetPersona2(long personaId = 0, CancellationToken cancellationToken = default)
        {
            var targetPersonaId = personaId == 0 ? User.PersonaId() : personaId;
            var persona = await _managePersonaAsync.GetPersonaAsync(targetPersonaId, User, cancellationToken);

            if (persona == null)
            {
                return NoContent();
            }

            var personaList = await _managePersonaAsync.ListActivePersonaAsync(persona.RealPageId, false, cancellationToken);

            var enumerable = personaList.ToList();
            persona.hasMultiPersona = enumerable.Where(p => p.OrganizationPartyId == persona.OrganizationPartyId).Skip(1).Any();
            persona.hasMultiCompany = enumerable.Any(p => p.OrganizationPartyId != persona.OrganizationPartyId
                                                          && p.Organization.RealPageId != ClaimsPrincipalExtensions.ExternalCompanyRealPageId);

            return Ok(persona);
        }

        /// <summary>
        /// Used to trigger the notification event that the user changed company
        /// </summary>
        /// <param name="personaId">The persona ID to change to</param>
        /// <returns>Accepted if successful, BadRequest or Unauthorized if not</returns>
        [HttpPost]
        [AuthorizeScope("userinfoapi")]
        [Route("persona/{personaId}/company")]
        [ProducesResponseType((int)HttpStatusCode.Accepted)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> ChangeCompany(long personaId = 0)
        {
            // Check for client token from UL
            var productInternalSettingList = await Task.Run(() =>
                _productInternalSettingRepository.GetProductInternalSettings((int)ProductEnum.UnifiedPlatform));

            var unifiedLoginClientId = productInternalSettingList
                .FirstOrDefault(a => a.Name.Equals("UnifiedLoginServerClientName", StringComparison.OrdinalIgnoreCase))?.Value;

            var clientCode = User.ClientCode();
            long targetPersonaId = personaId;

            if (!clientCode.Equals(unifiedLoginClientId, StringComparison.OrdinalIgnoreCase))
            {
                if (User.PersonaId() != 0 && personaId == 0)
                {
                    targetPersonaId = User.PersonaId();
                }
            }
            else
            {
                targetPersonaId = personaId;
            }

            var currentPersona = await Task.Run(() => _managePersona.GetPersona(User.PersonaId()));
            if (currentPersona == null)
            {
                return BadRequest("Current persona not found");
            }

            var personaList = await Task.Run(() => _managePersona.ListActivePersona(currentPersona.RealPageId, false));

            if (personaList.Any(p => p.PersonaId == targetPersonaId))
            {
                var result = await Task.Run(() => _managePersona.ChangeCompanyNotification(targetPersonaId));
                return result == Guid.Empty ? BadRequest() : Accepted();
            }

            return Unauthorized();
        }

        /// <summary>
        /// Get Persona company list
        /// </summary>
        /// <returns>List of persona for the given user</returns>
        [HttpGet]
        [Route("personas")]
        [ProducesResponseType(typeof(ObjectListOutput<PersonaCompany, IErrorData>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetPersonasList()
        {
            var output = new ObjectListOutput<PersonaCompany, IErrorData>();
            var userRealPageId = User.RealPageId();

            var personaList = await Task.Run(() => _managePersona.ListActivePersona(userRealPageId, true));
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
        /// <param name="type">Select type of products to return</param>
        /// <returns>List of persona products</returns>
        [HttpGet]
        [Route("personas/products")]
        [ProducesResponseType(typeof(ObjectListOutput<PersonaProductUserDetails, IErrorData>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetProductsByPersona([FromQuery] ProductSelectType? type = null)
        {
            var output = new ObjectListOutput<PersonaProductUserDetails, IErrorData>();
            var errorStatus = new Status<IErrorData>();

            var personaId = User.PersonaId();
            var persona = await Task.Run(() => _managePersona.GetPersona(personaId));

            if (persona == null)
            {
                errorStatus.Success = false;
                errorStatus.ErrorCode = "400";
                errorStatus.ErrorMsg = "Active persona not found!";
                output.Status = errorStatus;
                return BadRequest(output);
            }

            var productList = await Task.Run(() =>
                _manageProduct.GetUserAssignedProductsByPersona(persona, type));

            output.list = productList;
            output.Status = errorStatus;
            return Ok(output);
        }

        /// <summary>
        /// Expire and create a product setting of a persona
        /// </summary>
        /// <param name="productId">ProductId</param>
        /// <param name="productSetting">Productsetting resource to expire and create</param>
        /// <returns>Repository response with operation result</returns>
        [HttpPut]
        [Route("personas/products/{productId}/productSettings")]
        [ProducesResponseType(typeof(ObjectOutput<RepositoryResponse, IErrorData>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> UpdateUserProductSetting(int? productId, [FromBody] ProductSetting productSetting)
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

            var personaId = User.PersonaId();
            var persona = await Task.Run(() => _managePersona.GetPersonaWithRightsToggle(personaId, withRights: false));

            if (persona == null)
            {
                errorStatus.Success = false;
                errorStatus.ErrorCode = "400";
                errorStatus.ErrorMsg = "Active persona not found!";
                output.Status = errorStatus;
                return BadRequest(output);
            }

            var response = await Task.Run(() =>
                _manageProduct.UpdateProductSetting(productSetting, persona.PersonaId));

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
        /// <param name="personaId">The persona identifier</param>
        /// <param name="productId">The product identifier</param>
        /// <returns>List of roles assigned to the persona for the product</returns>
        [HttpGet]
        [Route("persona/{personaId}/product/{productId}/permissions")]
        [ProducesResponseType(typeof(IList<Role>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetPersonaRolesByProduct([FromRoute] long personaId, [FromRoute] ProductEnum productId)
        {
            if (personaId == 0 || productId == 0)
            {
                return BadRequest("Invalid personaId or productId");
            }

            var roleList = await Task.Run(() =>
                _manageUserRoleRight.GetAssignedRoleForPersona(productId, personaId, null));

            return Ok(roleList);
        }

        #endregion
    }
}
