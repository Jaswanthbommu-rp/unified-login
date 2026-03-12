using System.Security.Claims;

namespace UnifiedLogin.BusinessLogic.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static readonly Guid EmployeeCompanyRealPageId = new Guid("0D018E46-C20E-477D-ADED-4E5A35FB8F99");
    public static readonly Guid ExternalCompanyRealPageId = new Guid("EEFACE50-9F75-4DCE-B133-A97EE0E0D723");
    public static readonly Guid ContractCompanyRealPageId = new Guid("10F5A427-4636-4F47-840E-6212BD842BC0");

    public static Guid OrganizationGuid(this ClaimsPrincipal cp)
    {
        return Guid.Parse(cp.Claims.FirstOrDefault(c => c.Type == "orgId")?.Value
                          ?? throw new System.Exception("No orgId claim was present!"));
    }

    public static Guid RealPageId(this ClaimsPrincipal cp)
    {
        return Guid.Parse(cp.Claims.FirstOrDefault(c => c.Type == "realPageId")?.Value
                          ?? throw new System.Exception("No realPageId claim was present!"));
    }

    public static long PersonaId(this ClaimsPrincipal cp)
    {
        return long.Parse(cp.Claims.FirstOrDefault(c => c.Type == "personaId")?.Value
                          ?? throw new System.Exception("No personaId claim was present!"));
    }

    public static long OrgPartyId(this ClaimsPrincipal cp)
    {
        return long.Parse(cp.Claims.FirstOrDefault(c => c.Type == "orgPartyId")?.Value
                          ?? throw new System.Exception("No organizationPartyId claim was present!"));
    }

    public static bool HasLuminaRight(this ClaimsPrincipal cp)
    {
        return cp.Claims.FirstOrDefault(c => c.Type.ToLower() == "right" && c.Value.ToLower() == "lumina") != null;
    }

    public static List<Claim> Roles(this ClaimsPrincipal cp)
    {
        return cp.Claims.Where(p => p.Type.Equals("role", StringComparison.OrdinalIgnoreCase) ||
                                    p.Type.Equals("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    public static List<int> RoleIds(this ClaimsPrincipal cp)
    {
        var claims = cp.Claims.Where(p => p.Type.Equals("roleid", StringComparison.OrdinalIgnoreCase) ||
                                          p.Type.Equals("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", StringComparison.OrdinalIgnoreCase))
            .ToList();

        List<int> intUserRoles = claims.ConvertAll(x => Convert.ToInt32(x.Value));

        return intUserRoles;
    }

    public static List<string> Rights(this ClaimsPrincipal cp)
    {
        return cp.Claims.Where(p => p.Type.Equals("right", StringComparison.OrdinalIgnoreCase)).Select(c => c.Value).ToList();
    }

    public static long UserId(this ClaimsPrincipal cp)
    {
        return long.Parse(cp.Claims.FirstOrDefault(c => c.Type.Equals("userid", StringComparison.OrdinalIgnoreCase))?.Value
                          ?? throw new System.Exception("No userid sub claim was present!"));
    }

    public static bool IsRealPageEmployee(this ClaimsPrincipal user) => user.OrganizationGuid() == EmployeeCompanyRealPageId;

    public static bool IsRPEmployeeFlag(this ClaimsPrincipal user)
    {
        return Convert.ToBoolean(user.Claims.FirstOrDefault(c => c.Type.Equals("isRPEmployee", StringComparison.OrdinalIgnoreCase))?.Value);
    }

    public static Guid ImpersonatedBy(this ClaimsPrincipal user)
    {
        Guid impersonatedBy = Guid.Empty;
        Guid.TryParse(user.Claims.FirstOrDefault(f => f.Type?.Equals("impersonatedby", StringComparison.OrdinalIgnoreCase) == true)?.Value, out impersonatedBy);
        return impersonatedBy;
    }

    public static string LoginName(this ClaimsPrincipal user)
    {
        return user.Claims.FirstOrDefault(f => f.Type?.Equals("loginName", StringComparison.OrdinalIgnoreCase) == true)?.Value
               ?? throw new System.Exception("No LoginName claim was present!");
    }

    public static string CorrelationId(this ClaimsPrincipal user)
    {
        return user.Claims.FirstOrDefault(f => f.Type?.Equals("correlationId", StringComparison.OrdinalIgnoreCase) == true)?.Value
               ?? throw new System.Exception("No CorrelationId claim was present!");
    }

    public static string FirstName(this ClaimsPrincipal user)
    {
        return user.Claims.FirstOrDefault(f => f.Type?.Equals("firstName", StringComparison.OrdinalIgnoreCase) == true)?.Value
               ?? throw new System.Exception("No FirstName claim was present!");
    }

    public static string LastName(this ClaimsPrincipal user)
    {
        return user.Claims.FirstOrDefault(f => f.Type?.Equals("lastName", StringComparison.OrdinalIgnoreCase) == true)?.Value
               ?? throw new System.Exception("No LastName claim was present!");
    }

    public static long BooksMasterOrganizationId(this ClaimsPrincipal cp)
    {
        return long.Parse(cp.Claims.FirstOrDefault(c => c.Type == "orgCompanyMasterId")?.Value
                          ?? throw new System.Exception("No OrgCompanyMasterId claim was present!"));
    }

    public static string OrgType(this ClaimsPrincipal cp)
    {
        return cp.Claims.FirstOrDefault(c => c.Type == "orgType")?.Value
               ?? throw new System.Exception("No orgType claim was present!");
    }

    public static string ClientCode(this ClaimsPrincipal cp)
    {
        return cp.Claims.FirstOrDefault(c => c.Type.Equals("client_id", StringComparison.OrdinalIgnoreCase))?.Value
               ?? throw new System.Exception("No client_id claim was present!");
    }


}
