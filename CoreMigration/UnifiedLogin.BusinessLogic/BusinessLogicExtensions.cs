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

namespace UnifiedLogin.BusinessLogic
{
    /// <summary>
    /// Extension methods for registering BusinessLogic services in the dependency injection container.
    /// Follows clean architecture principles and modern .NET Core 8.0 DI patterns.
    /// </summary>
    public static class BusinessLogicExtensions
    {
        /// <summary>
        /// Registers all BusinessLogic services with the DI container.
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <returns>The service collection for chaining</returns>
        public static IServiceCollection AddBusinessLogicServices(this IServiceCollection services)
        {
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
            services.AddScoped<IManagePersona, ManagePersona>();
            services.AddScoped<IManagePostalAddress, ManagePostalAddress>();
            services.AddScoped<IManagePreferredContactMethod, ManagePreferredContactMethod>();
            services.AddScoped<IManageProduct, ManageProduct>();
            services.AddScoped<IManageProfile, ManageProfile>();
            services.AddScoped<IManageRedBook, ManageRedBook>();
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
            services.AddScoped<ITwoFactorLogic, TwoFactorLogic>();

            // Batch processing logic services - Scoped for batch operations
            services.AddScoped<ManageBulkUserBatch>();
            services.AddScoped<ManageBulkUsers>();
            services.AddScoped<ManageCloneProductBatch>();
            services.AddScoped<ManageEnterpriseRoleProductBatch>();
            services.AddScoped<ManageEnterpriseRolesPrimaryProperties>();
            services.AddScoped<ManagePrimaryPropertiesBatch>();
            services.AddScoped<ManageProductBatch>();
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
            services.AddScoped<IManageProductUser, ManageProductUser>();
            services.AddScoped<IManageProductVendorServices, ManageProductVendorServices>();
            // TODO: ManageResearchApplication class exists but may have compilation issues - investigate separately
            // services.AddScoped<IManageResearchApplication, Logic.Product.ManageResearchApplication>();
            services.AddScoped<IManageUnifiedAmenities, ManageUnifiedAmenities>();
            services.AddScoped<IManageUPFMProductsIntegration, ManageUPFMProductsIntegration>();

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
            // Product integration factories - Singleton lifetime
            services.AddSingleton<IIntegrationTypeFactory, IntegrationTypeFactory>();

            // Product integration helpers - Transient lifetime for short-lived operations
            services.AddTransient<IDataCollector, DataCollector>();

            // Note: Product integration implementations and batch processors use factory patterns
            // and should be resolved through their respective factories rather than direct DI
        }

        #endregion
    }
}
