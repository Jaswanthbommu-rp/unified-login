using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using UnifiedLogin.Core.DataAccess.Configuration;
using UnifiedLogin.Core.DataAccess.HealthChecks;

namespace UnifiedLogin.Core.DataAccess;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Register modern DataAccess services with comprehensive configuration
    /// </summary>
    public static IServiceCollection AddModernDataAccess(this IServiceCollection services, IConfiguration configuration)
    {
        // Configure options with validation
        services.AddOptions<DataAccessOptions>()
            .Bind(configuration.GetSection(DataAccessOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        // Core services
        services.AddScoped<IConnectionFactory, ConnectionFactory>();
        
        services.AddScoped<IUnitOfWork>(provider =>
        {
            var factory = provider.GetRequiredService<IConnectionFactory>();
            var options = provider.GetRequiredService<IOptions<DataAccessOptions>>();
            var unitOfWork = new DapperUnitOfWork(factory);
            unitOfWork.Initialize(options.Value.ConnectionString);
            return unitOfWork;
        });
        
        // Repository services - both sync and async
        services.AddScoped<IRepository, DapperRepository>();
        services.AddScoped<IRepositoryAsync, DapperRepositoryAsync>();
        
        // Health checks
        services.AddHealthChecks()
            .AddCheck<DatabaseHealthCheck>("database", tags: ["database", "sql"]);
        
        return services;
    }

    /// <summary>
    /// Register DataAccess services with DI container (legacy support)
    /// </summary>
    public static IServiceCollection AddDataAccess(this IServiceCollection services, string connectionString)
    {
        // Configure basic options
        services.Configure<DataAccessOptions>(options =>
        {
            options.ConnectionString = connectionString;
        });

        services.AddScoped<IConnectionFactory, ConnectionFactory>();
        
        services.AddScoped<IUnitOfWork>(provider => 
        {
            var factory = provider.GetRequiredService<IConnectionFactory>();
            var unitOfWork = new DapperUnitOfWork(factory);
            unitOfWork.Initialize(connectionString);
            return unitOfWork;
        });
        
        services.AddScoped<IRepository, DapperRepository>();
        services.AddScoped<IRepositoryAsync, DapperRepositoryAsync>();
        
        return services;
    }

    /// <summary>
    /// Register DataAccess services with configuration-based connection string (legacy support)
    /// </summary>
    public static IServiceCollection AddDataAccess(this IServiceCollection services, 
        IConfiguration configuration, string connectionName = "DefaultConnection")
    {
        var connectionString = configuration.GetConnectionString(connectionName);
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new ArgumentException($"Connection string '{connectionName}' not found in configuration.");
        }
        
        return services.AddDataAccess(connectionString);
    }
}