using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnifiedLogin.ServiceDefaults;

public class UnifiedLoginUserScopeMiddleware(RequestDelegate next, ILogger<UnifiedLoginUserScopeMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        if (context.User.Identity is { IsAuthenticated: true })
        {
            var user = context.User;
            var realPageId = user.Claims.FirstOrDefault(c => c.Type == "realPageId")?.Value;
            var loginName = user.Claims.FirstOrDefault(c => c.Type == "loginName")?.Value;
            var personaId = user.Claims.FirstOrDefault(c => c.Type == "personaId")?.Value;

            using (logger.BeginScope("User:{user}, RealPageId:{realPageId}, PersonaId:{personaId}", loginName, realPageId, personaId))
            {
                await next(context);
            }
        }
        else
        {
            await next(context);
        }
    }
}
