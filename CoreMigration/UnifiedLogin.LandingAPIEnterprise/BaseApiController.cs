using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace UnifiedLogin.LandingAPIEnterprise;

/// <summary>
/// Base controller for all Web API endpoints in the Enterprise API.
/// Provides common functionality for authentication, authorization, and user context.
/// </summary>
[ApiController]
[EnableCors("LandingAPICORSAllowedOrigins")]
[Authorize(Policy = "enterpriseapi")]
public abstract class BaseApiController : ControllerBase
{
    // Dependencies (will be injected via constructor in derived classes)
    private IManagePerson? _managePerson;
    private IManageUserLogin? _manageUserLogin;
    private IManagePersona? _managePersona;

    /// <summary>
    /// Enterprise User ID
    /// </summary>
    protected int EnterpriseUserId { get; set; }

    /// <summary>
    /// RealPage User ID (GUID)
    /// </summary>
    protected Guid RealPageUserId { get; set; }

    /// <summary>
    /// Organization Party ID
    /// </summary>
    protected long OrgPartyId { get; set; }

    /// <summary>
    /// User login name
    /// </summary>
    protected string LoginName { get; set; } = string.Empty;

    /// <summary>
    /// Used to filter, sort and limit the number of records being returned by the request.
    /// </summary>
    protected RequestParameter? GlobalRequestParameter { get; set; }

    /// <summary>
    /// Holds default user claim related information
    /// </summary>
    protected DefaultUserClaim UserClaims { get; set; } = new();

    /// <summary>
    /// Client code identifier
    /// </summary>
    protected string ClientCode { get; set; } = string.Empty;

    /// <summary>
    /// Correlation ID for tracing requests
    /// </summary>
    protected Guid CorrelationId { get; set; }

    /// <summary>
    /// Organization Master ID
    /// </summary>
    protected long OrganizationMasterId { get; set; }

    /// <summary>
    /// Organization name
    /// </summary>
    protected string OrganizationName { get; set; } = string.Empty;

    /// <summary>
    /// Organization RealPage GUID
    /// </summary>
    protected Guid OrganizationRealPageGuid { get; set; }

    /// <summary>
    /// Persona ID
    /// </summary>
    protected long PersonaId { get; set; }

    /// <summary>
    /// GreenBook access token
    /// </summary>
    protected string GreenBookAccessToken { get; set; } = string.Empty;

    /// <summary>
    /// Indicates if the user is a RealPage employee
    /// </summary>
    protected bool IsRealPageEmployee { get; set; }

    /// <summary>
    /// Default constructor
    /// </summary>
    protected BaseApiController()
    {
        CorrelationId = Guid.NewGuid();
    }

    /// <summary>
    /// Constructor with dependency injection for unit testing and derived classes
    /// </summary>
    /// <param name="managePerson">Person management service</param>
    /// <param name="managePersona">Persona management service</param>
    /// <param name="manageUserLogin">User login management service</param>
    /// <param name="httpClientFactory">HTTP client factory for external calls</param>
    protected BaseApiController(
        IManagePerson managePerson,
        IManagePersona managePersona,
        IManageUserLogin manageUserLogin,
        IHttpClientFactory httpClientFactory)
    {
        _managePerson = managePerson ?? throw new ArgumentNullException(nameof(managePerson));
        _managePersona = managePersona ?? throw new ArgumentNullException(nameof(managePersona));
        _manageUserLogin = manageUserLogin ?? throw new ArgumentNullException(nameof(manageUserLogin));
        CorrelationId = Guid.NewGuid();
    }

    /// <summary>
    /// Initializes the controller with user claims from the HTTP context.
    /// This method is called automatically by ASP.NET Core before action execution.
    /// </summary>
    protected void InitializeUserContext()
    {
        var currentUser = HttpContext.User;

        if (!currentUser.Identity?.IsAuthenticated ?? true)
        {
            // Anonymous request - set default correlation ID
            UserClaims = new DefaultUserClaim { CorrelationId = Guid.NewGuid() };
            CorrelationId = UserClaims.CorrelationId;
            return;
        }

        // Initialize services if not already set (for non-test scenarios)
        _managePerson ??= HttpContext.RequestServices.GetRequiredService<IManagePerson>();
        _managePersona ??= HttpContext.RequestServices.GetRequiredService<IManagePersona>();
        _manageUserLogin ??= HttpContext.RequestServices.GetRequiredService<IManageUserLogin>();

        // Check for client_info claim (service account authentication)
        var clientInfoClaim = currentUser.FindFirst(c => c.Type.Equals("client_info", StringComparison.OrdinalIgnoreCase));
        
        if (clientInfoClaim != null && Guid.TryParse(clientInfoClaim.Value, out var realPageGuid))
        {
            ProcessClientInfoClaims(currentUser, realPageGuid);
        }

        // Extract user claims
        UserClaims = new DefaultUserClaim(currentUser);

        // Set protected properties from claims
        EnterpriseUserId = UserClaims.UserId;
        OrgPartyId = UserClaims.OrganizationPartyId;
        LoginName = UserClaims.LoginName;
        OrganizationMasterId = UserClaims.OrganizationMasterId;
        OrganizationName = UserClaims.OrganizationName;
        RealPageUserId = UserClaims.UserRealPageGuid;
        CorrelationId = UserClaims.CorrelationId;
        OrganizationRealPageGuid = UserClaims.OrganizationRealPageGuid;
        ClientCode = UserClaims.ClientCode;
        PersonaId = UserClaims.PersonaId;
        IsRealPageEmployee = UserClaims.RealPageEmployee;

        // Extract authorization token
        if (HttpContext.Request.Headers.TryGetValue("Authorization", out var authHeader))
        {
            var authValue = authHeader.ToString();
            if (authValue.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                GreenBookAccessToken = authValue["Bearer ".Length..];
            }
        }

        // Get user rights
        var userRights = BaseUserRights.GetUserRightsBy(currentUser, UserClaims);
        UserClaims.Rights = userRights;
    }

    /// <summary>
    /// Processes claims for client_info based authentication (service accounts)
    /// </summary>
    private void ProcessClientInfoClaims(ClaimsPrincipal currentUser, Guid realPageGuid)
    {
        RealPageUserId = realPageGuid;

        var person = _managePerson!.GetPerson(realPageGuid);
        if (person == null)
        {
            var clientId = currentUser.FindFirst("client_id")?.Value ?? "unknown";
            throw new InvalidOperationException(
                $"Missing persona information for client_info user. Client: {clientId}, RealPageId: {realPageGuid}");
        }

        var userLogin = _manageUserLogin!.GetUserLoginOnly(realPageGuid);
        var persona = _managePersona!.GetActivePersonaWithoutRights(realPageGuid);

        if (persona == null)
        {
            throw new InvalidOperationException(
                $"No active persona found for user. RealPageId: {realPageGuid}");
        }

        // Add additional claims to the current user identity
        var identity = (ClaimsIdentity)currentUser.Identity!;
        var claimsToAdd = new List<Claim>
        {
            new("realPageId", realPageGuid.ToString()),
            new("sub", userLogin.UserId.ToString()),
            new("orgPartyId", persona.Organization.PartyId.ToString()),
            new("ORGID", persona.Organization.RealPageId.ToString()),
            new("LOGINNAME", userLogin.LoginName),
            new("ORGMASTERID", persona.Organization.BooksMasterId.ToString()),
            new("ORGNAME", persona.Organization.Name),
            new("FIRSTNAME", person.FirstName),
            new("LASTNAME", person.LastName),
            new("PERSONAID", persona.PersonaId.ToString())
        };

        identity.AddClaims(claimsToAdd);

        // Add role claims
        var userRoleRight = HttpContext.RequestServices.GetRequiredService<IManageUserRoleRight>();
        var userRoles = userRoleRight.GetAssignedRoleForPersona(
            ProductEnum.UnifiedPlatform,
            persona.PersonaId,
            persona.Organization.PartyId);

        if (userRoles?.Count > 0)
        {
            identity.AddClaims(userRoles.Select(role => new Claim("roleid", role.Name)));
        }
    }

    /// <summary>
    /// Recreates claims for a client by RealPage user ID.
    /// Used for impersonation or context switching scenarios.
    /// </summary>
    /// <param name="realPageUserId">The RealPage user ID</param>
    protected void RecreateClaimsForClient(Guid realPageUserId)
    {
        if (realPageUserId == Guid.Empty)
        {
            throw new ArgumentException("RealPage User ID cannot be empty.", nameof(realPageUserId));
        }

        _managePerson ??= HttpContext.RequestServices.GetRequiredService<IManagePerson>();
        _managePersona ??= HttpContext.RequestServices.GetRequiredService<IManagePersona>();
        _manageUserLogin ??= HttpContext.RequestServices.GetRequiredService<IManageUserLogin>();

        var person = _managePerson.GetPerson(realPageUserId);
        if (person == null)
        {
            throw new InvalidOperationException(
                $"Missing persona information for client during claims recreation. RealPageId: {realPageUserId}");
        }

        var userLogin = _manageUserLogin.GetUserLoginOnly(realPageUserId);
        var persona = _managePersona.GetActivePersonaWithoutRights(realPageUserId);

        if (persona == null)
        {
            throw new InvalidOperationException(
                $"No active persona found during claims recreation. RealPageId: {realPageUserId}");
        }

        UserClaims = new DefaultUserClaim
        {
            UserId = (int)userLogin.UserId,
            OrganizationPartyId = persona.Organization.PartyId,
            LoginName = userLogin.LoginName,
            OrganizationMasterId = (long)persona.Organization.BooksMasterId,
            CustomerMasterId = (long)persona.Organization.BooksMasterId,
            OrganizationName = persona.Organization.Name,
            FirstName = person.FirstName,
            LastName = person.LastName,
            PersonaId = persona.PersonaId,
            OrganizationRealPageGuid = persona.Organization.RealPageId,
            UserRealPageGuid = realPageUserId,
            CorrelationId = Guid.NewGuid(),
            RealPageEmployee = string.Equals(persona.Organization.Name, "REALPAGE EMPLOYEE", StringComparison.OrdinalIgnoreCase)
        };

        // Update protected properties
        SyncPropertiesFromUserClaims();
    }

    /// <summary>
    /// Recreates claims for a specific organization context.
    /// Used for multi-company user scenarios.
    /// </summary>
    /// <param name="realPageUserId">The RealPage user ID</param>
    /// <param name="upfmId">The organization (UPFM) ID</param>
    protected void RecreateClaimsForClient(Guid realPageUserId, Guid upfmId)
    {
        if (realPageUserId == Guid.Empty)
        {
            throw new ArgumentException("RealPage User ID cannot be empty.", nameof(realPageUserId));
        }

        if (upfmId == Guid.Empty)
        {
            throw new ArgumentException("Organization ID (UPFM) cannot be empty.", nameof(upfmId));
        }

        _managePerson ??= HttpContext.RequestServices.GetRequiredService<IManagePerson>();
        _managePersona ??= HttpContext.RequestServices.GetRequiredService<IManagePersona>();
        _manageUserLogin ??= HttpContext.RequestServices.GetRequiredService<IManageUserLogin>();

        var person = _managePerson.GetPerson(realPageUserId);
        if (person == null)
        {
            throw new InvalidOperationException(
                $"Missing persona information during claims recreation. RealPageId: {realPageUserId}");
        }

        var personas = _managePersona.ListPersona(realPageUserId);
        var userPersona = personas?.FirstOrDefault(p => p.Organization.RealPageId == upfmId);

        if (userPersona == null)
        {
            throw new InvalidOperationException(
                $"No persona found for organization. RealPageId: {realPageUserId}, OrganizationId: {upfmId}");
        }

        var userLogin = _manageUserLogin.GetUserLoginOnly(realPageUserId);

        UserClaims = new DefaultUserClaim
        {
            UserId = (int)userPersona.UserId,
            OrganizationPartyId = userPersona.Organization.PartyId,
            LoginName = userLogin.LoginName,
            OrganizationMasterId = (long)userPersona.Organization.BooksMasterId,
            CustomerMasterId = (long)userPersona.Organization.BooksMasterId,
            OrganizationName = userPersona.Organization.Name,
            FirstName = person.FirstName,
            LastName = person.LastName,
            PersonaId = userPersona.PersonaId,
            OrganizationRealPageGuid = userPersona.Organization.RealPageId,
            UserRealPageGuid = realPageUserId,
            CorrelationId = Guid.NewGuid(),
            RealPageEmployee = personas?.Any(p => string.Equals(p.Organization.Name, "REALPAGE EMPLOYEE", StringComparison.OrdinalIgnoreCase)) ?? false
        };

        // Update protected properties
        SyncPropertiesFromUserClaims();
    }

    /// <summary>
    /// Synchronizes protected properties from UserClaims after recreation
    /// </summary>
    private void SyncPropertiesFromUserClaims()
    {
        EnterpriseUserId = UserClaims.UserId;
        OrgPartyId = UserClaims.OrganizationPartyId;
        LoginName = UserClaims.LoginName;
        OrganizationMasterId = UserClaims.OrganizationMasterId;
        OrganizationName = UserClaims.OrganizationName;
        RealPageUserId = UserClaims.UserRealPageGuid;
        CorrelationId = UserClaims.CorrelationId;
        OrganizationRealPageGuid = UserClaims.OrganizationRealPageGuid;
        ClientCode = UserClaims.ClientCode;
        PersonaId = UserClaims.PersonaId;
        IsRealPageEmployee = UserClaims.RealPageEmployee;
    }
}

#region Supporting Types and Interfaces

// Note: These interfaces should be defined in separate files in a production scenario
// They are included here as placeholders to make the migration clear

/// <summary>
/// Interface for person management operations
/// </summary>
public interface IManagePerson
{
    Person? GetPerson(Guid realPageUserId);
}

/// <summary>
/// Interface for persona management operations
/// </summary>
public interface IManagePersona
{
    Persona? GetActivePersonaWithoutRights(Guid realPageUserId);
    IList<Persona> ListPersona(Guid realPageUserId);
}

/// <summary>
/// Interface for user login management operations
/// </summary>
public interface IManageUserLogin
{
    UserLogin GetUserLoginOnly(Guid realPageUserId);
}

/// <summary>
/// Interface for user role and rights management
/// </summary>
public interface IManageUserRoleRight
{
    IList<Role> GetAssignedRoleForPersona(ProductEnum product, long personaId, long orgPartyId);
}

/// <summary>
/// Represents a person entity
/// </summary>
public class Person
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
}

/// <summary>
/// Represents a persona (user-organization relationship)
/// </summary>
public class Persona
{
    public long PersonaId { get; set; }
    public long UserId { get; set; }
    public Organization Organization { get; set; } = new();
}

/// <summary>
/// Represents an organization
/// </summary>
public class Organization
{
    public long PartyId { get; set; }
    public long BooksMasterId { get; set; }
    public Guid RealPageId { get; set; }
    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// Represents a user login
/// </summary>
public class UserLogin
{
    public long UserId { get; set; }
    public string LoginName { get; set; } = string.Empty;
}

/// <summary>
/// Represents a user role
/// </summary>
public class Role
{
    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// Request parameter for filtering and pagination
/// </summary>
public class RequestParameter
{
    public int PageSize { get; set; } = 50;
    public int PageNumber { get; set; } = 1;
    public string? SortBy { get; set; }
    public bool SortDescending { get; set; }
}

/// <summary>
/// Default user claims container
/// </summary>
public class DefaultUserClaim
{
    public int UserId { get; set; }
    public long OrganizationPartyId { get; set; }
    public string LoginName { get; set; } = string.Empty;
    public long OrganizationMasterId { get; set; }
    public long CustomerMasterId { get; set; }
    public string OrganizationName { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public long PersonaId { get; set; }
    public Guid OrganizationRealPageGuid { get; set; }
    public Guid UserRealPageGuid { get; set; }
    public Guid CorrelationId { get; set; }
    public bool RealPageEmployee { get; set; }
    public string ClientCode { get; set; } = string.Empty;
    public List<string> Rights { get; set; } = new();

    public DefaultUserClaim() { }

    public DefaultUserClaim(ClaimsPrincipal principal)
    {
        UserId = int.TryParse(principal.FindFirst("sub")?.Value, out var uid) ? uid : 0;
        OrganizationPartyId = long.TryParse(principal.FindFirst("orgPartyId")?.Value, out var opid) ? opid : 0;
        LoginName = principal.FindFirst("LOGINNAME")?.Value ?? string.Empty;
        OrganizationMasterId = long.TryParse(principal.FindFirst("ORGMASTERID")?.Value, out var omid) ? omid : 0;
        CustomerMasterId = OrganizationMasterId;
        OrganizationName = principal.FindFirst("ORGNAME")?.Value ?? string.Empty;
        FirstName = principal.FindFirst("FIRSTNAME")?.Value ?? string.Empty;
        LastName = principal.FindFirst("LASTNAME")?.Value ?? string.Empty;
        PersonaId = long.TryParse(principal.FindFirst("PERSONAID")?.Value, out var pid) ? pid : 0;
        OrganizationRealPageGuid = Guid.TryParse(principal.FindFirst("ORGID")?.Value, out var orgGuid) ? orgGuid : Guid.Empty;
        UserRealPageGuid = Guid.TryParse(principal.FindFirst("realPageId")?.Value, out var userGuid) ? userGuid : Guid.Empty;
        CorrelationId = Guid.TryParse(principal.FindFirst("correlationId")?.Value, out var corrId) ? corrId : Guid.NewGuid();
        ClientCode = principal.FindFirst("client_id")?.Value ?? string.Empty;
        RealPageEmployee = principal.HasClaim(c => c.Type == "realpage_employee" && c.Value == "true");
    }
}

/// <summary>
/// Product enumeration
/// </summary>
public enum ProductEnum
{
    UnifiedPlatform = 1
}

/// <summary>
/// Static helper class for retrieving user rights
/// </summary>
public static class BaseUserRights
{
    public static List<string> GetUserRightsBy(ClaimsPrincipal principal, DefaultUserClaim userClaims)
    {
        // Extract rights from claims
        var rights = principal.FindAll("right")
            .Select(c => c.Value)
            .Distinct()
            .ToList();

        return rights;
    }
}

#endregion
