using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UnifiedLogin.BusinessLogic.Logic.Security;
using UnifiedLogin.ServiceDefaults;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.Core;

public static class BusinessLogicExtensions
{
    public static IServiceCollection AddBusinessLogic(this IServiceCollection services, IConfiguration configuration)
    {
        // Register user claims accessor for dependency injection
        // HttpContextAccessor must be registered first (typically done in Program.cs or ServiceDefaults)
        services.AddScoped<IUserClaimsAccessor, UserClaimsAccessor>();

        // Add services required for business logic here
        services.AddScoped<IManageSecurity, ManageSecurity>();

        return services;
    }
}

