namespace UnifiedLogin.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

public static class BusinessLogicExtensions
{
    public static IServiceCollection AddBusinessLogic(this IServiceCollection services, IConfiguration configuration)
    {
        //Add services required for business logic here

        return services;
    }
}

