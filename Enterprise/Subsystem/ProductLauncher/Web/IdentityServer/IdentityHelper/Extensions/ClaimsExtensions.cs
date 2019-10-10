using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace RP.Enterprise.Subsystem.ProductLauncher.Web.IdentityHelper.Extensions
{    
    public static class ClaimsExtensions
    {
        public static string RealPageId(this IEnumerable<Claim> claims)
        {
            return claims.FirstOrDefault(a => string.Equals(a.Type, "realPageId", StringComparison.OrdinalIgnoreCase))?.Value;
        }

        public static string UserId(this IEnumerable<Claim> claims)
        {
            return claims.FirstOrDefault(a => string.Equals(a.Type, "sub", StringComparison.OrdinalIgnoreCase))?.Value;
        }

        public static string CorrelationId(this IEnumerable<Claim> claims)
        {
            string correlationId = claims.FirstOrDefault(a => string.Equals(a.Type, "correlationId", StringComparison.OrdinalIgnoreCase))?.Value;
            if (correlationId == null)
            {
                correlationId = Guid.NewGuid().ToString();
            }
            return correlationId;
        }

        public static string OrgMasterId(this IEnumerable<Claim> claims)
        {
            return claims.FirstOrDefault(a => string.Equals(a.Type, "orgMasterId", StringComparison.OrdinalIgnoreCase))?.Value;
        }

        public static string ImpersonatedBy(this IEnumerable<Claim> claims)
        {
            return claims.FirstOrDefault(a => string.Equals(a.Type, "ImpersonatedBy", StringComparison.OrdinalIgnoreCase))?.Value;
        }
    }
}
