using Microsoft.Extensions.DependencyInjection;
using UnifiedLogin.BusinessLogic.Logic;
using UnifiedLogin.BusinessLogic.Logic.Enterprise.User;
using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.BusinessLogic.Logic.Product;
using UnifiedLogin.BusinessLogic.Logic.Security;
using UnifiedLogin.BusinessLogic.Repository;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.BusinessLogic.Repository.Security;
using UnifiedLogin.LandingAPIEnterprise.Services;
using UnifiedLogin.ServiceDefaults;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.LandingAPIEnterprise.Configuration
{
    /// <summary>
    /// Extension methods for configuring services in the DI container
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Registers all user-related services for dependency injection
        /// </summary>
        public static IServiceCollection AddUserServices(this IServiceCollection services)
        {
            // Register service layer
            services.AddScoped<IUserManagementService, UserManagementService>();
            services.AddScoped<IUserQueryService, UserQueryService>();
            services.AddScoped<IUserValidationService, UserValidationService>();
            services.AddScoped<ISuperUserValidationService, SuperUserValidationService>();
            services.AddScoped<IUserProfileService, UserProfileService>();
            services.AddScoped<IProductFormattingService, ProductFormattingService>();
            services.AddScoped<IClientAuthenticationService, ClientAuthenticationService>();
            services.AddSingleton<ILoggingService, LoggingService>();

            return services;
        }

        /// <summary>
        /// Registers all business logic services
        /// </summary>
        public static IServiceCollection AddBusinessLogicServices(this IServiceCollection services)
        {
            // Business Logic Services - Use factory pattern for services that need DefaultUserClaim
            services.AddScoped<IManagePersona>(provider =>
            {
                var userClaims = provider.GetRequiredService<DefaultUserClaim>();
                return new ManagePersona(userClaims);
            });

            services.AddScoped<IManagePerson, ManagePerson>();

            services.AddScoped<IManageProduct>(provider =>
            {
                var userClaims = provider.GetRequiredService<DefaultUserClaim>();
                return new ManageProduct(userClaims);
            });

            services.AddScoped<IManageOrganization>(provider =>
            {
                var userClaims = provider.GetRequiredService<DefaultUserClaim>();
                return new ManageOrganization(userClaims);
            });

            services.AddScoped<IManageUnifiedSettings>(provider =>
            {
                var userClaims = provider.GetRequiredService<DefaultUserClaim>();
                return new ManageUnifiedSettings(userClaims);
            });
            services.AddScoped<IManageSecurity, ManageSecurity>();

            services.AddScoped<IManageUnifiedLogin>(provider =>
            {
                var userClaims = provider.GetRequiredService<DefaultUserClaim>();
                return new ManageUnifiedLogin(userClaims);
            });

            services.AddScoped<IManageProductPanel>(provider =>
            {
                var userClaims = provider.GetRequiredService<DefaultUserClaim>();
                return new ManageProductPanel(userClaims);
            });

            services.AddScoped<IManageUserLogin>(provider =>
            {
                var userClaims = provider.GetRequiredService<DefaultUserClaim>();
                return new ManageUserLogin(userClaims);
            });

            services.AddScoped<IManageProductUser>(provider =>
            {
                var userClaims = provider.GetRequiredService<DefaultUserClaim>();
                return new ManageProductUser(userClaims);
            });

            services.AddScoped<IManageCustomFields>(provider =>
            {
                var userClaims = provider.GetRequiredService<DefaultUserClaim>();
                return new ManageCustomFields(userClaims);
            });

            services.AddScoped<UserManagement>(provider =>
            {
                var userClaims = provider.GetRequiredService<DefaultUserClaim>();
                return new UserManagement(userClaims);
            });

            services.AddScoped<ManageUser>(provider =>
            {
                var userClaims = provider.GetRequiredService<DefaultUserClaim>();
                return new ManageUser(userClaims);
            });

            return services;
        }

        /// <summary>
        /// Registers all repository services
        /// </summary>
        public static IServiceCollection AddRepositoryServices(this IServiceCollection services)
        {
            // Repositories
            services.AddScoped<IProductRepository>(provider =>
            {
                var userClaims = provider.GetRequiredService<DefaultUserClaim>();
                return new ProductRepository(userClaims);
            });

            services.AddScoped<IUserRepository>(provider =>
            {
                var userClaims = provider.GetRequiredService<DefaultUserClaim>();
                return new UserRepository(userClaims);
            });

            //services.AddScoped<PersonaRightRepository>(provider =>
            //{
            //    var userClaims = provider.GetRequiredService<DefaultUserClaim>();
            //    return new PersonaRightRepository(userClaims);
            //});
            services.AddScoped<PersonaRightRepository>(provider =>
            {
                var userClaims = provider.GetRequiredService<DefaultUserClaim>();
                return new PersonaRightRepository();
            });
            services.AddScoped<SamlRepository>();
            services.AddScoped<ProductInternalSettingRepository>();

            return services;
        }

        /// <summary>
        /// Registers all services required for the UserController
        /// </summary>
        public static IServiceCollection AddUserControllerServices(this IServiceCollection services)
        {
            // Register HTTP context accessor - required for IUserClaimsAccessor
            services.AddHttpContextAccessor();

            // Register IUserClaimsAccessor - provides access to authenticated user claims
            services.AddScoped<IUserClaimsAccessor, UserClaimsAccessor>();

            // Register DefaultUserClaim as a scoped factory - resolved from IUserClaimsAccessor
            // This allows business logic classes to accept DefaultUserClaim in their constructors
            services.AddScoped(sp =>
            {
                var userClaimsAccessor = sp.GetRequiredService<IUserClaimsAccessor>();
                return userClaimsAccessor.GetUserClaim();
            });
            // Register DefaultUserClaim as scoped
            services.AddScoped<DefaultUserClaim>();

            services.AddUserServices();
            services.AddBusinessLogicServices();
            services.AddRepositoryServices();

            return services;
        }
    }
}
