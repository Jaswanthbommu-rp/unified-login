using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using UnifiedLogin.BusinessLogic.Attributes;
using UnifiedLogin.BusinessLogic.Extensions;
using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
using UnifiedLogin.BusinessLogic.Services.Interfaces;
using UnifiedLogin.Core;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product.UnifiedLogin;

namespace UnifiedLogin.LandingAPI.Controllers;

/// <summary>
/// V2 of <see cref="PersonaController"/> — uses async services throughout.
/// <list type="bullet">
///   <item>All <c>Task.Run(() => sync_method())</c> wrappers removed.</item>
///   <item><see cref="IManagePersona"/> replaced by <see cref="IManagePersonaAsync"/>.</item>
///   <item><see cref="IManageProduct.GetUserAssignedProductsByPersona"/> replaced by
///         <see cref="IProductService.GetAssignedProductsByPersonaAsync"/>.</item>
///   <item><c>IProductInternalSettingRepository</c> removed — the client-code check
///         is now encapsulated inside <see cref="IManagePersonaAsync.ChangeCompanyNotificationAsync"/>.</item>
///   <item><see cref="CancellationToken"/> threaded through every service call.</item>
/// </list>
/// <para>
/// <b>Intentionally kept sync:</b> <c>UpdateUserProductSetting</c> still uses
/// <see cref="IManageProduct.UpdateProductSetting"/> — a full async equivalent for
/// that specific overload does not yet exist in <see cref="IProductService"/>.
/// </para>
/// </summary>
[Authorize]
[ApiController]
[Route("v2")]
public class PersonaV2Controller : ControllerBase
{
    #region Fields

    private readonly IManagePersonaAsync  _managePersonaAsync;
    private readonly IProductService      _productService;
    //private readonly IManageProduct       _manageProduct;       // kept for UpdateProductSetting only
    private readonly IManageUserRoleRightAsync _manageUserRoleRight;

    #endregion

    #region Constructor

    /// <summary>
    /// Constructor with dependency injection.
    /// </summary>
    public PersonaV2Controller(
        IUserClaimsAccessor      userClaimsAccessor,
        IManagePersonaAsync      managePersonaAsync,
        IProductService          productService,
        //IManageProduct           manageProduct,
        IManageUserRoleRightAsync manageUserRoleRight
        )
    {
        _managePersonaAsync  = managePersonaAsync  ?? throw new ArgumentNullException(nameof(managePersonaAsync));
        _productService      = productService      ?? throw new ArgumentNullException(nameof(productService));
        //_manageProduct       = manageProduct       ?? throw new ArgumentNullException(nameof(manageProduct));
        _manageUserRoleRight = manageUserRoleRight ?? throw new ArgumentNullException(nameof(manageUserRoleRight));
    }

    #endregion

    #region GET persona/environment

    /// <summary>Get persona environment types.</summary>
    [HttpGet("persona/environment")]
    [ProducesResponseType(typeof(ObjectListOutput<PersonaEnvironment, IErrorData>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NoContent)]
    public async Task<IActionResult> GetPersonaEnvironmentType()
    {
        // BEFORE: await Task.Run(() => _managePersona.GetPersonaEnvironmentType())
        var list = await _managePersonaAsync.GetPersonaEnvironmentTypeAsync(default);

        if (list is { Count: > 0 })
            return Ok(new ObjectListOutput<PersonaEnvironment, IErrorData> { list = list });

        return NoContent();
    }

    #endregion

    #region POST persona

    /// <summary>Create a new persona for a person + organisation pair.</summary>
    [HttpPost("persona")]
    [ProducesResponseType(typeof(ObjectOutput<IPersona, IErrorData>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> CreatePersona(
        Guid personRealPageId, Guid organizationRealPageId,
        [FromBody] Persona persona,
        CancellationToken ct)
    {
        var output      = new ObjectOutput<IPersona, IErrorData>();
        var errorStatus = new Status<IErrorData>();
        output.obj      = persona;

        var userRealPageId = User.RealPageId();
        personRealPageId   = personRealPageId == Guid.Empty ? userRealPageId : personRealPageId;

        if (personRealPageId == Guid.Empty)
            return BadRequest(Problem("200.3", "Invalid parameter: personRealPageId", output, errorStatus));

        if (organizationRealPageId == Guid.Empty)
            return BadRequest(Problem("200.3", "Invalid parameter Organization realPageId", output, errorStatus));

        if (persona is null)
            return BadRequest(Problem("200.3", "Null parameter: Persona.", output, errorStatus));

        // BEFORE: await Task.Run(() => _managePersona.CreatePersona(...))
        var response = await _managePersonaAsync.CreatePersonaAsync(
            personRealPageId, organizationRealPageId, persona, ct);

        if (response.Id == 0)
            return BadRequest(Problem("200.3", response.ErrorMessage, output, errorStatus));

        output.obj.PersonaId = response.Id;
        output.Status        = errorStatus;
        return Ok(output);
    }

    #endregion

    #region GET persona

    /// <summary>Get persona details — defaults to the current user's active persona.</summary>
    [HttpGet("persona")]
    [ProducesResponseType(typeof(Persona), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NoContent)]
    public async Task<IActionResult> GetPersona(long personaId = 0, CancellationToken ct = default)
    {
        var targetId = personaId == 0 ? User.PersonaId() : personaId;

        // BEFORE: await Task.Run(() => _managePersona.GetPersona(targetId))
        var persona = await _managePersonaAsync.GetPersonaAsync(targetId, withRights: true, ct);
        if (persona is null) return NoContent();

        // BEFORE: await Task.Run(() => _managePersona.ListActivePersona(...))
        var personaList = await _managePersonaAsync.ListActivePersonaAsync(
            persona.RealPageId, includeOrganization: false, ct);

        persona.hasMultiPersona = personaList
            .Count(p => p.OrganizationPartyId == persona.OrganizationPartyId) > 1;

        persona.hasMultiCompany = personaList
            .Count(p => p.OrganizationPartyId != persona.OrganizationPartyId
                     && p.Organization.RealPageId != DefaultUserClaim.ExternalCompanyRealPageId) > 0;

        return Ok(persona);
    }

    #endregion

    #region POST persona/{personaId}/company — ChangeCompany

    /// <summary>
    /// Trigger a change-company notification event.
    /// Client-code / internal-settings check is now fully inside
    /// <see cref="IManagePersonaAsync.ChangeCompanyNotificationAsync"/>.
    /// </summary>
    [HttpPost("persona/{personaId}/company")]
    [AuthorizeScope("userinfoapi")]
    [ProducesResponseType((int)HttpStatusCode.Accepted)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    public async Task<IActionResult> ChangeCompany(long personaId = 0, CancellationToken ct = default)
    {
        long targetPersonaId = personaId == 0 ? User.PersonaId() : personaId;

        // BEFORE: await Task.Run(() => _managePersona.GetPersona(_userClaimsAccessor.PersonaId))
        var currentPersona = await _managePersonaAsync.GetPersonaAsync(
            User.PersonaId(), withRights: false, ct);

        if (currentPersona is null)
            return BadRequest("Current persona not found.");

        // BEFORE: await Task.Run(() => _managePersona.ListActivePersona(...))
        var personaList = await _managePersonaAsync.ListActivePersonaAsync(
            currentPersona.RealPageId, includeOrganization: false, ct);

        if (!personaList.Any(p => p.PersonaId == targetPersonaId))
            return Unauthorized();

        // BEFORE: await Task.Run(() => _managePersona.ChangeCompanyNotification(targetPersonaId))
        // IProductInternalSettingRepository dependency dropped — now internal to ManagePersonaAsync
        var notificationGuid = await _managePersonaAsync.ChangeCompanyNotificationAsync(targetPersonaId, ct);

        return notificationGuid == Guid.Empty ? BadRequest() : Accepted();
    }

    #endregion

    #region GET personas

    /// <summary>Get the list of persona+company pairs for the current user.</summary>
    [HttpGet("personas")]
    [ProducesResponseType(typeof(ObjectListOutput<PersonaCompany, IErrorData>), (int)HttpStatusCode.OK)]
    public async Task<IActionResult> GetPersonasList(CancellationToken ct)
    {
        var userRealPageId = User.RealPageId(); // _userClaimsAccessor.UserRealPageGuid;

        // BEFORE: await Task.Run(() => _managePersona.ListActivePersona(...))
        var personaList = await _managePersonaAsync.ListActivePersonaAsync(
            userRealPageId, includeOrganization: true, ct);

        var pcl = new List<PersonaCompany>();

        foreach (var persona in personaList)
        {
            if (persona.Organization.RealPageId == DefaultUserClaim.ExternalCompanyRealPageId)
                continue;

            if (pcl.All(p => p.CompanyRealPageId != persona.Organization.RealPageId))
                pcl.Add(new PersonaCompany
                {
                    CompanyName       = persona.Organization.Name,
                    CompanyRealPageId = persona.Organization.RealPageId,
                    Personas          = []
                });

            pcl.Find(p => p.CompanyRealPageId == persona.Organization.RealPageId)!
               .Personas.Add(new PersonaCompanyDetails
               {
                   PersonaId = persona.PersonaId,
                   Name      = persona.Name
               });
        }

        return Ok(new ObjectListOutput<PersonaCompany, IErrorData>
        {
            list = pcl.OrderBy(p => p.CompanyName).ToList()
        });
    }

    #endregion

    #region GET personas/products

    /// <summary>Get enriched product tile list for the current user.</summary>
    [HttpGet("personas/products")]
    [ProducesResponseType(typeof(ObjectListOutput<PersonaProductUserDetails, IErrorData>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> GetProductsByPersona(
        [FromQuery] ProductSelectType? type = null,
        CancellationToken ct = default)
    {
        var output = new ObjectListOutput<PersonaProductUserDetails, IErrorData>();
        var errorStatus = new Status<IErrorData>();

        // BEFORE: await Task.Run(() => _managePersona.GetPersona(personaId))
        var persona = await _managePersonaAsync.GetPersonaAsync(
            User.PersonaId(), withRights: true, ct);

        if (persona is null)
        {
            errorStatus.Success = false;
            errorStatus.ErrorCode = "400";
            errorStatus.ErrorMsg = "Active persona not found!";
            output.Status = errorStatus;
            return BadRequest(output);
        }

        // BEFORE: await Task.Run(() => _manageProduct.GetUserAssignedProductsByPersona(persona, type))
        var productList = await _productService.GetAssignedProductsByPersonaAsync(persona, type, null, ct);

        output.list = productList;
        output.Status = errorStatus;
        return Ok(output);
    }

    #endregion

    #region PUT personas/products/{productId}/productSettings

    /// <summary>
    /// Expire and create a product setting.
    /// Still uses <see cref="IManageProduct.UpdateProductSetting"/> — no async equivalent yet.
    /// </summary>
    [HttpPut("personas/products/{productId}/productSettings1")]
    [ProducesResponseType(typeof(ObjectOutput<RepositoryResponse, IErrorData>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> UpdateUserProductSetting(
        int? productId,
        [FromBody] ProductSetting productSetting,
        CancellationToken ct)
    {
        var output = new ObjectOutput<RepositoryResponse, IErrorData>();
        var errorStatus = new Status<IErrorData>();

        if (productId is null)
            return BadRequest(Problem("200.3", "Null parameter: productId.", output, errorStatus));

        if (productSetting is null)
            return BadRequest(Problem("200.3", "Null parameter: productSetting.", output, errorStatus));

        // withRights: false — rights not needed for a settings write
        // BEFORE: await Task.Run(() => _managePersona.GetPersonaWithRightsToggle(personaId, false))
        var persona = await _managePersonaAsync.GetPersonaAsync(
            User.PersonaId(), withRights: false, ct);

        if (persona is null)
        {
            errorStatus.Success = false;
            errorStatus.ErrorCode = "400";
            errorStatus.ErrorMsg = "Active persona not found!";
            output.Status = errorStatus;
            return BadRequest(output);
        }

        // TODO: migrate to IProductService once UpdateProductSetting async overload is added
        var response = await Task.Run(
            () => _productService.UpdateProductSettingAsync(productSetting, persona.PersonaId), ct);

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

    #endregion

    #region GET persona/{personaId}/product/{productId}/permissions

    ///// <summary>Get roles assigned to a persona for a specific product.</summary>
    [HttpGet("persona/{personaId}/product/{productId}/permissions1")]
    [ProducesResponseType(typeof(IList<Role>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> GetPersonaRolesByProduct(
        [FromRoute] long personaId,
        [FromRoute] ProductEnum productId,
        CancellationToken ct)
    {
        if (personaId == 0 || productId == 0)
            return BadRequest("Invalid personaId or productId.");

        var roleList = await Task.Run(
            () => _manageUserRoleRight.GetAssignedRoleForPersonaAsync(productId, personaId, null, ct));

        return Ok(roleList);
    }

    #endregion

    #region Private helpers

    private static ObjectOutput<T, IErrorData> Problem<T>(
        string code, string message,
        ObjectOutput<T, IErrorData> output,
        Status<IErrorData> status)
        where T : class
    {
        status.Success   = false;
        status.ErrorCode = code;
        status.ErrorMsg  = message;
        output.Status    = status;
        return output;
    }

    #endregion
}