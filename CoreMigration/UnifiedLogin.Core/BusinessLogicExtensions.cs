using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UnifiedLogin.BusinessLogic.CacheHelper;
using UnifiedLogin.BusinessLogic.Logic;
using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.BusinessLogic.Logic.Product;
using UnifiedLogin.BusinessLogic.Logic.Product.Interfaces;
using UnifiedLogin.BusinessLogic.Logic.ProductIntegration.Factory;
using UnifiedLogin.BusinessLogic.Logic.ProductIntegration.Helpers;
using UnifiedLogin.BusinessLogic.Logic.Security;
using UnifiedLogin.BusinessLogic.Repository;
using UnifiedLogin.BusinessLogic.Repository.Enterprise;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.BusinessLogic.Repository.Security;
using UnifiedLogin.ServiceDefaults;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.Core;

public static class BusinessLogicExtensions
{
    /// <summary>
    /// Registers all BusinessLogic services with the DI container.
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddBusinessLogicServices(this IServiceCollection services)
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

        // Register core logic services
        AddLogicServices(services);

        // Register product-specific logic services
        AddProductLogicServices(services);

        // Register repository services
        AddRepositoryServices(services);

        // Register cache services
        AddCacheServices(services);

        // Register security services
        AddSecurityServices(services);

        // Register product integration services
        AddProductIntegrationServices(services);

        return services;
    }

    #region Core Logic Services (IManage* → Manage*)

    private static void AddLogicServices(IServiceCollection services)
    {
        // Core business logic services - Scoped lifetime for request-bound operations
        services.AddScoped<IManageBlueBook, ManageBlueBook>();
        services.AddScoped<IManageCommunicationEvents, ManageCommunicationEvents>();
        services.AddScoped<IManageConfigurationSetting, ManageConfigurationSetting>();
        services.AddScoped<IManageContactMechanism, ManageContactMechanism>();
        services.AddScoped<IManageContactMechanismUsageType, ManageContactMechanismUsageType>();
        services.AddScoped<IManageCredential, ManageCredential>();
        services.AddScoped<IManageCustomFields, ManageCustomFields>();
        services.AddScoped<IManageDashboardContent, ManageDashboardContent>();
        services.AddScoped<IManageElectronicAddress, ManageElectronicAddress>();
        services.AddScoped<IManageEmail, ManageEmail>();
        services.AddScoped<IManageEmployeeAccess, ManageEmployeeAccess>();
        services.AddScoped<IManageGeographicBoundary, ManageGeographicBoundary>();
        services.AddScoped<IManageHotsCloneUsers, ManageHotsCloneUsers>();
        services.AddScoped<IManageMicrosoftAzure, ManageMicrosoftAzure>();
        services.AddScoped<IManageOrganization, ManageOrganization>();
        services.AddScoped<IManageOrganizationProduct, ManageOrganizationProduct>();
        services.AddScoped<IManagePartyRelationship, ManagePartyRelationship>();
        services.AddScoped<IManagePartyRole, ManagePartyRole>();
        services.AddScoped<IManagePasswordPolicy, ManagePasswordPolicy>();
        services.AddScoped<IManagePerson, ManagePerson>();

        // ManagePersona has ambiguous constructors - use specific constructor with IPersonaRepository
        services.AddScoped<IManagePersona>(sp =>
        {
            var personaRepository = sp.GetRequiredService<IPersonaRepository>();
            return new ManagePersona(personaRepository);
        });

        services.AddScoped<IManagePostalAddress, ManagePostalAddress>();
        services.AddScoped<IManagePreferredContactMethod, ManagePreferredContactMethod>();
        services.AddScoped<IManageProduct, ManageProduct>();
        services.AddScoped<IManageProfile, ManageProfile>();

        // ManageRedBook requires a string parameter (gbtoken) - cannot be registered in DI without request context
        // This service should be instantiated manually when needed: new ManageRedBook(gbtoken)
        // services.AddScoped<IManageRedBook, ManageRedBook>(); // COMMENTED OUT - requires gbtoken parameter

        services.AddScoped<IManageRelationshipType, ManageRelationshipType>();
        services.AddScoped<IManageRoleType, ManageRoleType>();
        services.AddScoped<IManageSaml, ManageSaml>();
        services.AddScoped<IManageSecuritySettings, ManageSecuritySettings>();
        services.AddScoped<IManageStatusType, ManageStatusType>();
        services.AddScoped<IManageStreetAddress, ManageStreetAddress>();
        services.AddScoped<IManageTelecommunicationNumber, ManageTelecommunicationNumber>();
        services.AddScoped<IManageUnifiedLogin, ManageUnifiedLogin>();
        services.AddScoped<IManageUnifiedSettings, ManageUnifiedSettings>();
        services.AddScoped<IManageUser, ManageUser>();
        services.AddScoped<IManageUserLogin, ManageUserLogin>();
        services.AddScoped<IManageUserLoginPersona, ManageUserLoginPersona>();
        services.AddScoped<IManageUserPropertiesSync, ManageUserPropertiesSync>();
        services.AddScoped<IManageUserProperty, ManageUserProperty>();
        services.AddScoped<IManageUserRegistrationEmail, ManageUserRegistrationEmail>();
        services.AddScoped<IManageUserRoleRight, ManageUserRoleRight>();

        // ITwoFactorLogic requires IRepository which is not registered - use factory
        // IRepository is a legacy data access interface that should not be used with DI
        services.AddScoped<ITwoFactorLogic>(sp =>
        {
            var userClaim = sp.GetRequiredService<DefaultUserClaim>();
            return new TwoFactorLogic(userClaim, repository: null);
        });

        // Batch processing logic services - Scoped for batch operations
        // ManageBulkUserBatch has ambiguous constructors - use constructor with DefaultUserClaim
        services.AddScoped<ManageBulkUserBatch>(sp =>
        {
            var userClaim = sp.GetRequiredService<DefaultUserClaim>();
            return new ManageBulkUserBatch(userClaim);
        });

        services.AddScoped<ManageBulkUsers>();
        services.AddScoped<ManageCloneProductBatch>();
        services.AddScoped<ManageEnterpriseRoleProductBatch>();
        services.AddScoped<ManageEnterpriseRolesPrimaryProperties>();

        // ManagePrimaryPropertiesBatch has ambiguous constructors - use constructor with DefaultUserClaim
        services.AddScoped<ManagePrimaryPropertiesBatch>(sp =>
        {
            var userClaim = sp.GetRequiredService<DefaultUserClaim>();
            return new ManagePrimaryPropertiesBatch(userClaim);
        });

        services.AddScoped<ManageProductBatch>();

        services.AddScoped<IManagePersonaAsync, ManagePersonaAsync>();

    }

    #endregion

    #region Product-Specific Logic Services

    private static void AddProductLogicServices(IServiceCollection services)
    {
        // Product-specific business logic services - Scoped lifetime
        services.AddScoped<IManageIntelligentBuilding, ManageIntelligentBuilding>();
        services.AddScoped<IManageProductAdminSupportPortal, ManageProductAdminSupportPortal>();
        services.AddScoped<IManageProductAssetOptimization, ManageProductAssetOptimization>();
        services.AddScoped<IManageProductClientPortal, ManageProductClientPortal>();
        services.AddScoped<IManageProductEasyLMS, ManageProductEasyLMS>();
        services.AddScoped<IManageProductIntegrationMarketplace, ManageProductIntegrationMarketplace>();
        services.AddScoped<IManageProductLead2Lease, ManageProductLead2Lease>();
        services.AddScoped<IManageProductMarketingCenter, ManageProductMarketingCenter>();
        services.AddScoped<IManageProductOmniChannel, ManageProductOmniChannel>();
        services.AddScoped<IManageProductOneSite, ManageProductOneSite>();
        services.AddScoped<IManageProductOneSiteAccounting, ManageProductOneSiteAccounting>();
        services.AddScoped<IManageProductOnSite, ManageProductOnSite>();
        services.AddScoped<IManageProductOps, ManageProductOps>();
        services.AddScoped<IManageProductPanel, ManageProductPanel>();
        services.AddScoped<IManageProductProspectContact, ManageProductProspectContact>();
        services.AddScoped<IManageProductRentersInsurance, ManageProductRentersInsurance>();
        services.AddScoped<IManageProductResidentPortal, ManageProductResidentPortal>();
        services.AddScoped<IManageProductRPDocumentManagement, ManageProductRPDocumentManagement>();
        services.AddScoped<IManageProductRum, ManageProductRum>();
        services.AddScoped<IManageProductSelfProvisioningPortal, ManageProductSelfProvisioningPortal>();

        // ManageProductUser has ambiguous constructors - use constructor with all dependencies
        services.AddScoped<IManageProductUser>(sp =>
        {
            var productRepository = sp.GetRequiredService<IProductRepository>();
            var productInternalSettingRepository = sp.GetRequiredService<IProductInternalSettingRepository>();
            var samlRepository = sp.GetRequiredService<ISamlRepository>();
            var manageProduct = sp.GetRequiredService<IManageProduct>();
            var organizationRepository = sp.GetRequiredService<IOrganizationRepository>();
            var userRepository = sp.GetRequiredService<IUserRepository>();
            var userLoginRepository = sp.GetRequiredService<IUserLoginRepository>();
            var personaRepository = sp.GetRequiredService<IPersonaRepository>();
            return new ManageProductUser(productRepository, productInternalSettingRepository, samlRepository,
                manageProduct, organizationRepository, userRepository, userLoginRepository, personaRepository);
        });

        services.AddScoped<IManageProductVendorServices, ManageProductVendorServices>();
        // TODO: ManageResearchApplication class exists but may have compilation issues - investigate separately
        // services.AddScoped<IManageResearchApplication, Logic.Product.ManageResearchApplication>();
        services.AddScoped<IManageUnifiedAmenities, ManageUnifiedAmenities>();

        // ManageUPFMProductsIntegration requires productId parameter - cannot be registered without it
        // This service should be resolved through factory or instantiated manually: new ManageUPFMProductsIntegration(productId, userClaims)
        // services.AddScoped<IManageUPFMProductsIntegration, ManageUPFMProductsIntegration>(); // COMMENTED OUT - requires productId parameter

        // Product-specific services without interfaces
        services.AddScoped<ManageProductRealConnect>();
    }

    #endregion

    #region Repository Services

    private static void AddRepositoryServices(IServiceCollection services)
    {
        // Repository services - Scoped lifetime for database operations
        services.AddScoped<ICommunicationEventRepository, CommunicationEventRepository>();
        services.AddScoped<IConfigurationSettingRepository, ConfigurationSettingRepository>();
        services.AddScoped<IContactMechanismRepository, ContactMechanismRepository>();
        services.AddScoped<IContactMechanismUsageTypeRepository, ContactMechanismUsageTypeRepository>();
        services.AddScoped<ICredentialRepository, CredentialRepository>();
        services.AddScoped<ICustomFieldsRepository, CustomFieldsRepository>();
        services.AddScoped<IElectronicAddressRepository, ElectronicAddressRepository>();
        services.AddScoped<IEmailRepository, EmailRepository>();
        services.AddScoped<IEntUserRepository, EntUserRepository>();
        services.AddScoped<IGeographicBoundaryRepository, GeographicBoundaryRepository>();
        services.AddScoped<IGlobalSettingRepository, GlobalSettingRepository>();
        services.AddScoped<IHOTSCloneUserRepository, HOTSCloneUserRepository>();
        services.AddScoped<IOrganizationProductRepository, OrganizationProductRepository>();
        services.AddScoped<IOrganizationRepository, OrganizationRepository>();
        services.AddScoped<IPartyRelationshipRepository, PartyRelationshipRepository>();
        services.AddScoped<IPartyRoleRepository, PartyRoleRepository>();
        services.AddScoped<IPasswordPolicyRepository, PasswordPolicyRepository>();
        services.AddScoped<IPersonaRepository, PersonaRepository>();
        services.AddScoped<IPersonaRightRepository, PersonaRightRepository>();
        services.AddScoped<IPersonRepository, PersonRepository>();
        services.AddScoped<IPostalAddressRepository, PostalAddressRepository>();
        services.AddScoped<IPreferredContactMethodRepository, PreferredContactMethodRepository>();
        services.AddScoped<IProductInternalSettingRepository, ProductInternalSettingRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IProfileRepository, ProfileRepository>();
        services.AddScoped<IPropertyRepository, PropertyRepository>();
        services.AddScoped<IRelationshipTypeRepository, RelationshipTypeRepository>();
        services.AddScoped<IRoleTypeRepository, RoleTypeRepository>();
        services.AddScoped<ISamlRepository, SamlRepository>();
        services.AddScoped<ISecuritySettingsRepository, SecuritySettingsRepository>();
        services.AddScoped<ISharedDataRepository, SharedDataRepository>();
        services.AddScoped<IStatusTypeRepository, StatusTypeRepository>();
        services.AddScoped<IStreetAddressRepository, StreetAddressRepository>();
        services.AddScoped<ITelecommunicationNumberRepository, TelecommunicationNumberRepository>();
        services.AddScoped<ITwoFactorRepository, TwoFactorRepository>();
        services.AddScoped<IUnifiedLoginRepository, UnifiedLoginRepository>();
        services.AddScoped<IUnifiedSettingsRepository, UnifiedSettingsRepository>();
        services.AddScoped<IUserLoginPersonaRepository, UserLoginPersonaRepository>();
        services.AddScoped<IUserLoginRepository, UserLoginRepository>();
        services.AddScoped<IUserPropertiesSyncRepository, UserPropertiesSyncRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserRoleRightRepository, UserRoleRightRepository>();
        services.AddScoped<IUserTokenRepository, UserTokenRepository>();

        // NEW - Asynchronous repository services - Scoped lifetime for async database operations
        services.AddScoped<IPersonaRepositoryAsync, PersonaRepositoryAsync>();
        services.AddScoped<IOrganizationRepositoryAsync, OrganizationRepositoryAsync>();
        services.AddScoped<IUserLoginRepositoryAsync, UserLoginRepositoryAsync>();

        // Specialized repository services without interfaces
        services.AddScoped<BatchProductBulkUpdateRepository>();
        services.AddScoped<OmniChannelRepository>();
        services.AddScoped<ResearchApplicationRepository>();
    }

    #endregion

    #region Cache Services

    private static void AddCacheServices(IServiceCollection services)
    {
        // Cache services - Singleton lifetime for application-wide caching
        services.AddSingleton<IRedisCacheService, RedisCacheService>();
    }

    #endregion

    #region Security Services

    private static void AddSecurityServices(IServiceCollection services)
    {
        // Security services - Scoped lifetime for request-bound security operations
        services.AddScoped<IManageSecurity, ManageSecurity>();
    }

    #endregion

    #region Product Integration Services

    private static void AddProductIntegrationServices(IServiceCollection services)
    {
        // Product integration factories - Changed to Scoped to avoid singleton consuming scoped services
        // IntegrationTypeFactory depends on IManageProduct (Scoped), so it must also be Scoped
        services.AddScoped<IIntegrationTypeFactory, IntegrationTypeFactory>();

        // Product integration helpers - Transient lifetime for short-lived operations
        services.AddTransient<IDataCollector, DataCollector>();

        // Note: Product integration implementations and batch processors use factory patterns
        // and should be resolved through their respective factories rather than direct DI
    }

    #endregion

    //public static IServiceCollection AddBusinessLogic(this IServiceCollection services, IConfiguration configuration)
    //{
    //    // Register user claims accessor for dependency injection
    //    // HttpContextAccessor must be registered first (typically done in Program.cs or ServiceDefaults)
    //    services.AddScoped<IUserClaimsAccessor, UserClaimsAccessor>();

    //    // Add services required for business logic here
    //    services.AddScoped<IManageSecurity, ManageSecurity>();

    //    return services;
    //}
}

