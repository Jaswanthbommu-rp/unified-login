using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using UnifiedLogin.BusinessLogic.Logic.EnterpriseRolesProperties;
using UnifiedLogin.BusinessLogic.Logic.Helper;
using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.BusinessLogic.Logic.Product;
using UnifiedLogin.BusinessLogic.Logic.ProductIntegration.Factory;
using UnifiedLogin.BusinessLogic.Repository;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.DataAccess;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.EnterpriseRole;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product;
using UnifiedLogin.SharedObjects.Product.OneSite;
using UnifiedLogin.SharedObjects.Product.ResidentPortal;
using UnifiedLogin.SharedObjects.Product.Rum;
using ProductRole = UnifiedLogin.SharedObjects.Product.ProductRole;
using UL = UnifiedLogin.SharedObjects.Product.UnifiedLogin;

namespace UnifiedLogin.BusinessLogic.Logic
{
    /// <summary>
    /// Manages enterprise roles and primary properties operations
    /// </summary>
    public class ManageEnterpriseRolesPrimaryPropertiesV2 : IManageEnterpriseRolesPrimaryProperties
    {
        private readonly ILogger<ManageEnterpriseRolesPrimaryProperties> _logger;
        private readonly DefaultUserClaim _userClaim;
        private readonly IProductRepository _productRepository;
        private readonly IPropertyRepository _propertyRepository;
        private readonly IProductInternalSettingRepository _productInternalSettingRepository;
        private readonly IUnifiedSettingsRepository _unifiedSettingsRepository;
        private readonly IManagePersona _managePersona;
        private readonly IManageProductBatch _manageProductBatch;
        private readonly IPersonaRepository _personaRepository;
        private readonly IUserLoginRepository _userLoginRepository;
        private readonly IBatchProductBulkUpdateRepository _enterpriseRoleProductRepository;
        private readonly IUserRoleRightRepository _userRoleRightRepository;
        private readonly IIntegrationTypeFactory _integrationTypeFactory;
        private readonly Dictionary<int, IProductBatchStrategy> _productStrategies;

        private const string SUCCESS_RESULT = "";
        private const string ERROR_RESULT = "Error";
        private const int DEFAULT_STATUS_TYPE = 5;
        private const int DEFAULT_RETRY_COUNT = 0;

        #region Constructors

        /// <summary>
        /// Constructor with full dependency injection (recommended)
        /// </summary>
        public ManageEnterpriseRolesPrimaryPropertiesV2(
            ILogger<ManageEnterpriseRolesPrimaryProperties> logger,
            DefaultUserClaim userClaim,
            IProductRepository productRepository,
            IPropertyRepository propertyRepository,
            IProductInternalSettingRepository productInternalSettingRepository,
            IUnifiedSettingsRepository unifiedSettingsRepository,
            IManagePersona managePersona,
            IManageProductBatch manageProductBatch,
            IPersonaRepository personaRepository,
            IUserLoginRepository userLoginRepository,
            IBatchProductBulkUpdateRepository enterpriseRoleProductRepository,
            IUserRoleRightRepository userRoleRightRepository,
            IEnumerable<IProductBatchStrategy> productStrategies,
            IIntegrationTypeFactory integrationTypeFactory)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _userClaim = userClaim ?? throw new ArgumentNullException(nameof(userClaim));
            _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
            _propertyRepository = propertyRepository ?? throw new ArgumentNullException(nameof(propertyRepository));
            _productInternalSettingRepository = productInternalSettingRepository ?? throw new ArgumentNullException(nameof(productInternalSettingRepository));
            _unifiedSettingsRepository = unifiedSettingsRepository ?? throw new ArgumentNullException(nameof(unifiedSettingsRepository));
            _managePersona = managePersona ?? throw new ArgumentNullException(nameof(managePersona));
            _manageProductBatch = manageProductBatch ?? throw new ArgumentNullException(nameof(manageProductBatch));
            _personaRepository = personaRepository ?? throw new ArgumentNullException(nameof(personaRepository));
            _userLoginRepository = userLoginRepository ?? throw new ArgumentNullException(nameof(userLoginRepository));
            _enterpriseRoleProductRepository = enterpriseRoleProductRepository ?? throw new ArgumentNullException(nameof(enterpriseRoleProductRepository));
            _userRoleRightRepository = userRoleRightRepository ?? throw new ArgumentNullException(nameof(userRoleRightRepository));
            _integrationTypeFactory = integrationTypeFactory ?? throw new ArgumentNullException(nameof(integrationTypeFactory));
            _productStrategies = productStrategies?.ToDictionary(s => s.ProductId) ?? new Dictionary<int, IProductBatchStrategy>();
        }

        /// <summary>
        /// Test constructor with mocked dependencies
        /// </summary>
        public ManageEnterpriseRolesPrimaryPropertiesV2(
            IRepository repository,
            HttpMessageHandler messageHandler,
            DefaultUserClaim userClaims,
            IOneSiteProductService oneSiteProductService = null,
            IManageBlueBook manageBlueBook = null)
        {
            // Use NullLogger for testing if logger not provided
            _logger = new NullLogger<ManageEnterpriseRolesPrimaryProperties>();
            _userClaim = userClaims ?? throw new ArgumentNullException(nameof(userClaims));

            // Initialize repositories with injected dependencies
            _productRepository = new ProductRepository(repository);
            _propertyRepository = new PropertyRepository(repository);
            _productInternalSettingRepository = new ProductInternalSettingRepository(repository);
            _unifiedSettingsRepository = new UnifiedSettingsRepository(repository);
            _personaRepository = new PersonaRepository(repository);
            _userLoginRepository = new UserLoginRepository(repository);
            _enterpriseRoleProductRepository = new BatchProductBulkUpdateRepositoryV2(repository, _userClaim);
            _userRoleRightRepository = new UserRoleRightRepository(repository);

            // Initialize services
            _managePersona = new ManagePersona(repository, userClaims, messageHandler);
            _manageProductBatch = new ManageProductBatchV2(repository, messageHandler, userClaims, oneSiteProductService);

            var manageUnifiedLogin = new ManageUnifiedLogin(repository, userClaims, messageHandler);
            var manageProductOneSite = new ManageProductOneSite(repository, userClaims, messageHandler, oneSiteProductService);
            var manageProduct = new ManageProduct(repository, userClaims, messageHandler);
           
            _integrationTypeFactory = new IntegrationTypeFactory(
                manageProduct,
                manageUnifiedLogin,
                manageProductOneSite,
                _productRepository,
                _productInternalSettingRepository,
                userClaims);
            
            // Initialize strategies (empty for test constructor)
            _productStrategies = new Dictionary<int, IProductBatchStrategy>();
        }

      
        #endregion

        #region Public Methods

        /// <summary>
        /// Processes enterprise roles and primary properties data asynchronously
        /// </summary>
        public async Task<string> ProcessEnterpriseRolesAndPrimaryPropertiesDataAsync(
            long editorUserPersonaId,
            long subjectUserPersonaId,
            int? enterpriseRoleTemplateId = null,
            DateTime? createdDateTime = null,
            int batchProcessTypeId = 0,
            bool isUnassignAllProducts = false,
            CancellationToken cancellationToken = default)
        {
            var batchProcessorType = enterpriseRoleTemplateId != null ? "Enterprise Role" : "Primary Properties";

            _logger.LogInformation(
                "Starting {BatchType} processing for user {SubjectPersonaId}, template: {TemplateId}",
                batchProcessorType,
                subjectUserPersonaId,
                enterpriseRoleTemplateId);

            try
            {
                // Step 1: Get personas and validate
                var (editorPersona, userPersona) = await GetPersonasAsync(
                    editorUserPersonaId,
                    subjectUserPersonaId,
                    cancellationToken);

                if (editorPersona == null || userPersona == null)
                {
                    _logger.LogError("Failed to retrieve personas");
                    return ERROR_RESULT;
                }

                // Step 2: Determine product lists
                var productLists = await DetermineProductListsAsync(
                    editorPersona,
                    userPersona,
                    enterpriseRoleTemplateId,
                    createdDateTime,
                    batchProcessTypeId,
                    isUnassignAllProducts,
                    cancellationToken);

                if (productLists.ShouldReturnEarly)
                {
                    return SUCCESS_RESULT;
                }

                // Step 3: Get organization and user context
                var context = await CreateProcessingContextAsync(
                    userPersona,
                    subjectUserPersonaId,
                    cancellationToken);

                // Step 4: Create product batches
                var productBatchesResult = await CreateProductBatchesAsync(
                    editorPersona,
                    userPersona,
                    productLists,
                    context,
                    enterpriseRoleTemplateId,
                    cancellationToken);

                if (!string.IsNullOrEmpty(productBatchesResult.ErrorMessage))
                {
                    return productBatchesResult.ErrorMessage;
                }

                // Step 5: Handle OneSite mixed products
                var (oneSiteMixData, isOnesiteMix) = PrepareOneSiteMixData(productBatchesResult.ProductBatches);

                // Step 6: Save product batches
                if (productBatchesResult.ProductBatches.Any())
                {
                    var success = await SaveProductBatchesAsync(
                        editorUserPersonaId,
                        subjectUserPersonaId,
                        editorPersona.RealPageId,
                        productBatchesResult.ProductBatches,
                        oneSiteMixData,
                        isOnesiteMix,
                        context.ImpersonatorUserId,
                        productBatchesResult.AoJson,
                        cancellationToken);

                    if (!success)
                    {
                        _logger.LogError("{BatchType} processing failed for user {SubjectPersonaId}",
                            batchProcessorType,
                            subjectUserPersonaId);
                        return ERROR_RESULT;
                    }
                }

                _logger.LogInformation(
                    "Successfully completed {BatchType} processing for user {SubjectPersonaId}",
                    batchProcessorType,
                    subjectUserPersonaId);

                return SUCCESS_RESULT;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Exception during {BatchType} processing for user {SubjectPersonaId}",
                    batchProcessorType,
                    subjectUserPersonaId);
                return ERROR_RESULT;
            }
        }

        /// <summary>
        /// Synchronous version for backward compatibility
        /// </summary>
        [Obsolete("Use ProcessEnterpriseRolesAndPrimaryPropertiesDataAsync instead")]
        public string ProcessEnterpriseRolesAndPrimaryPropertiesData(
            long editorUserPersonaId,
            long subjectUserPersonaId,
            int? enterpriseRoleTemplateId = null,
            DateTime? createdDateTime = null,
            int batchProcessTypeId = 0,
            bool isUnassignAllProducts = false)
        {
            return ProcessEnterpriseRolesAndPrimaryPropertiesDataAsync(
                editorUserPersonaId,
                subjectUserPersonaId,
                enterpriseRoleTemplateId,
                createdDateTime,
                batchProcessTypeId,
                isUnassignAllProducts).GetAwaiter().GetResult();
        }

        #endregion

        #region Private Helper Methods

        private async Task<(Persona EditorPersona, Persona UserPersona)> GetPersonasAsync(
            long editorPersonaId,
            long userPersonaId,
            CancellationToken cancellationToken)
        {
            var editorTask = Task.Run(() => _managePersona.GetPersona(editorPersonaId), cancellationToken);
            var userTask = Task.Run(() => _managePersona.GetPersona(userPersonaId), cancellationToken);

            await Task.WhenAll(editorTask, userTask);

            return (await editorTask, await userTask);
        }

        private async Task<ProductListsResult> DetermineProductListsAsync(
            Persona editorPersona,
            Persona userPersona,
            int? enterpriseRoleTemplateId,
            DateTime? createdDateTime,
            int batchProcessTypeId,
            bool isUnassignAllProducts,
            CancellationToken cancellationToken)
        {
            var result = new ProductListsResult();

            if (batchProcessTypeId == (int)BatchProcessType.BulkAddUpdateEnterpriseRole)
            {
                if (isUnassignAllProducts)
                {
                    result.DeletedProducts = await GetProductsToUnassignAsync(
                        userPersona.PersonaId,
                        userPersona.PersonaId,
                        cancellationToken);

                    if (!result.DeletedProducts.Any())
                    {
                        result.ShouldReturnEarly = true;
                    }
                }
                else
                {
                    result.RoleTemplateProductRoles = await Task.Run(() =>
                        _productRepository.GetRoleTemplateProductRoleMapping(
                            enterpriseRoleTemplateId.Value,
                            editorPersona.OrganizationPartyId),
                        cancellationToken);

                    result.NewProducts = result.RoleTemplateProductRoles
                        .Select(p => p.ProductId)
                        .Distinct()
                        .ToList();

                    // Add UPFM to updated products for role deletion/update
                    var upfmProduct = result.NewProducts.FirstOrDefault(m => m == (int)ProductEnum.UnifiedPlatform);
                    if (upfmProduct > 0)
                    {
                        result.UpdatedProducts.Add(upfmProduct);
                    }
                }
            }
            else if (enterpriseRoleTemplateId != null && createdDateTime != null)
            {
                var tasksToRun = new Task<object>[]
                 {
                    Task.Run(async () => (object)await Task.FromResult(_productRepository.GetEnterpriseRoleNewProductsByRoleTemplateId(enterpriseRoleTemplateId.Value, createdDateTime.Value)), cancellationToken),
                    Task.Run(async () => (object)await Task.FromResult(_productRepository.GetEnterpriseRoleUpdatedProductsByRoleTemplateId(enterpriseRoleTemplateId.Value, createdDateTime.Value)), cancellationToken),
                    Task.Run(async () => (object)await Task.FromResult(_productRepository.GetEnterpriseRoleDeletedProductsByRoleTemplateId(enterpriseRoleTemplateId.Value, createdDateTime.Value)), cancellationToken),
                    Task.Run(async () => (object)await Task.FromResult(_productRepository.GetRoleTemplateProductRoleMapping(enterpriseRoleTemplateId.Value, editorPersona.OrganizationPartyId)), cancellationToken)
                 };

                await Task.WhenAll(tasksToRun);

                result.NewProducts = (List<int>)await tasksToRun[0];
                result.UpdatedProducts = (List<int>)await tasksToRun[1];
                result.DeletedProducts = (List<int>)await tasksToRun[2];
                result.RoleTemplateProductRoles = (List<RoleTemplateProductRole>)await tasksToRun[3];
                result.NewProducts.AddRange(result.UpdatedProducts);

                _logger.LogDebug(
                    "Product lists - New: {NewCount}, Updated: {UpdatedCount}, Deleted: {DeletedCount}",
                    result.NewProducts.Count,
                    result.UpdatedProducts.Count,
                    result.DeletedProducts.Count);
            }
            else
            {
                // Primary properties mode
                result.NewProducts = await GetProductsForPrimaryPropertiesAsync(
                    userPersona,
                    userPersona.PersonaId,
                    cancellationToken);
            }

            return result;
        }

        private async Task<List<int>> GetProductsToUnassignAsync(
            long personaId,
            long subjectUserPersonaId,
            CancellationToken cancellationToken)
        {
            var personaProducts = await Task.Run(() =>
                _productRepository.ListProductsByPersonaId(
                    personaId,
                    (int)UserUiStatusType.AccountCreationSuccessful).ToList(),
                cancellationToken);

            // Remove products that shouldn't be unassigned
            personaProducts.RemoveAll(m =>
                m.ProductId == (int)ProductEnum.UnifiedPlatform ||
                m.ProductId == (int)ProductEnum.AssetOptimizer);

            // Handle Admin Support Portal special case
            var adminSupportProductId = (int)ProductEnum.AdminSupportPortal;
            if (personaProducts.Any(m => m.ProductId == adminSupportProductId))
            {
                var productAttributes = await Task.Run(() =>
                    _productRepository.GetProductSamlDetails(subjectUserPersonaId, adminSupportProductId),
                    cancellationToken);

                if (productAttributes?.Count == 0)
                {
                    personaProducts.RemoveAll(a => a.ProductId == adminSupportProductId);
                }
            }

            return personaProducts.Select(p => p.ProductId).ToList();
        }

        private async Task<List<int>> GetProductsForPrimaryPropertiesAsync(
            Persona userPersona,
            long subjectUserPersonaId,
            CancellationToken cancellationToken)
        {
            var personaProducts = await Task.Run(() =>
                _productRepository.ListProductsByPersonaId(
                    userPersona.PersonaId,
                    (int)UserUiStatusType.AccountCreationSuccessful).ToList(),
                cancellationToken);

            // Get user's enterprise role and merge products
            var userEnterpriseRole = await Task.Run(() =>
                _productRepository.GetEnterpriseRoleForPersona(subjectUserPersonaId),
                cancellationToken);

            if (userEnterpriseRole != null)
            {
                var roleTemplateProductRoles = await Task.Run(() =>
                    _productRepository.GetRoleTemplateProductRoleMapping(
                        userEnterpriseRole.RoleTemplateId,
                        userPersona.OrganizationPartyId),
                    cancellationToken);

                foreach (var product in roleTemplateProductRoles)
                {
                    if (!personaProducts.Any(p => p.ProductId == product.ProductId) &&
                        product.ProductId != (int)ProductEnum.UnifiedPlatform)
                    {
                        personaProducts.Add(new PersonaProductUserDetails
                        {
                            ProductId = product.ProductId,
                            ProductName = product.ProductName
                        });
                    }
                }
            }

            _logger.LogDebug("Primary properties mode - processing {ProductCount} products", personaProducts.Count);

            return personaProducts.Select(p => p.ProductId).ToList();
        }

        private async Task<ProcessingContext> CreateProcessingContextAsync(
            Persona userPersona,
            long subjectUserPersonaId,
            CancellationToken cancellationToken)
        {
            var context = new ProcessingContext();

            var organizationListTask = Task.Run(() => _userLoginRepository.ListOrganizationByEnterpriseUserId(userPersona.RealPageId, string.Empty), cancellationToken);
            var personaProductSettingsTask = Task.Run(() => _personaRepository.GetPersonaProductSettings(subjectUserPersonaId), cancellationToken);
            var productsWithNoPropertiesTask = GetProductsWithNoPropertiesAsync(cancellationToken);

            await Task.WhenAll(organizationListTask, personaProductSettingsTask, productsWithNoPropertiesTask);

            var organizationList = await organizationListTask;
            context.PersonaOrganization = organizationList.FirstOrDefault(i => i.PartyId == userPersona.OrganizationPartyId);
            context.PersonaProductSettings = await personaProductSettingsTask;
            context.ProductsWithNoProperties = await productsWithNoPropertiesTask;

            // Determine if external user
            context.IsExternalUser = context.PersonaOrganization?.RelationshipType != null &&
                context.PersonaOrganization.RelationshipType.Equals("User Type", StringComparison.OrdinalIgnoreCase) &&
                context.PersonaOrganization.RoleNameFrom.Equals("External User", StringComparison.OrdinalIgnoreCase);

            // Get impersonator if applicable
            if (_userClaim.ImpersonatedBy != Guid.Empty)
            {
                var impersonator = await Task.Run(() =>
                    _userLoginRepository.GetUserLoginOnly(_userClaim.ImpersonatedBy),
                    cancellationToken);
                context.ImpersonatorUserId = impersonator?.UserId ?? 0;
            }

            return context;
        }

        private async Task<ProductBatchesResult> CreateProductBatchesAsync(
            Persona editorPersona,
            Persona userPersona,
            ProductListsResult productLists,
            ProcessingContext context,
            int? enterpriseRoleTemplateId,
            CancellationToken cancellationToken)
        {
            var result = new ProductBatchesResult();
            var distinctProducts = productLists.NewProducts.Distinct().ToList();

            _logger.LogDebug("Creating batches for {ProductCount} products", distinctProducts.Count);

            foreach (var productId in distinctProducts)
            {
                if (productId == (int)ProductEnum.AssetOptimizer)
                {
                    continue; // Skip AssetOptimizer - handled via bundling
                }

                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    var batch = await CreateSingleProductBatchAsync(
                        editorPersona,
                        userPersona,
                        productId,
                        productLists,
                        context,
                        enterpriseRoleTemplateId,
                        cancellationToken);

                    if (batch != null)
                    {
                        result.ProductBatches.Add(batch);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating batch for product {ProductId}", productId);
                    result.ErrorMessage = $"Error processing product {productId}: {ex.Message}";
                    return result;
                }
            }

            // Handle deleted products
            if (productLists.DeletedProducts?.Any() == true)
            {
                await CreateDeletedProductBatchesAsync(
                    productLists.DeletedProducts,
                    context,
                    result.ProductBatches,
                    cancellationToken);
            }

            // Bundle AO products
            result.AoJson = BundleAoProducts(result.ProductBatches);

            return result;
        }

        private async Task<ProductBatch> CreateSingleProductBatchAsync(
            Persona editorPersona,
            Persona userPersona,
            int productId,
            ProductListsResult productLists,
            ProcessingContext context,
            int? enterpriseRoleTemplateId,
            CancellationToken cancellationToken)
        {
            // Determine if product uses primary properties
            var usePrimaryProperties = await DetermineUsePrimaryPropertiesAsync(
                productId,
                userPersona.PersonaId,
                context.PersonaProductSettings,
                cancellationToken);

            // Get roles and properties for the product
            var (productRoles, propertiesResponse, rolesResponse) = await GetProductRolesAndPropertiesAsync(
                editorPersona,
                userPersona,
                productId,
                productLists.RoleTemplateProductRoles,
                enterpriseRoleTemplateId,
                usePrimaryProperties,
                cancellationToken);

            // Create batch context
            var batchContext = new ProductBatchContext
            {
                EditorPersonaId = editorPersona.PersonaId,
                SubjectPersonaId = userPersona.PersonaId,
                ProductId = productId,
                EditorPersona = editorPersona,
                UserPersona = userPersona,
                PropertiesResponse = propertiesResponse,
                RolesResponse = rolesResponse,
                ProductRoles = productRoles,
                UsePrimaryProperties = usePrimaryProperties,
                IsExternalUser = context.IsExternalUser,
                IntegrationType = _integrationTypeFactory.GetIntegrationTypeForProductId(productId)
            };

            // Use strategy if available
            if (_productStrategies.TryGetValue(productId, out var strategy))
            {
                return await strategy.CreateBatchAsync(batchContext, cancellationToken);
            }

            // Handle product-specific logic for products without strategies
            return await CreateProductSpecificBatchAsync(
                batchContext,
                productLists,
                context,
                cancellationToken);
        }

        private async Task<bool> DetermineUsePrimaryPropertiesAsync(
            int productId,
            long subjectUserPersonaId,
            List<ProductSettingList> personaProductSettings,
            CancellationToken cancellationToken)
        {
            // UnifiedPlatform always uses primary properties
            if (productId == (int)ProductEnum.UnifiedPlatform)
            {
                return true;
            }

            var productIdForSetting = ProductEnumHelper.GetAoProductList().Contains((ProductEnum)productId)
                ? (int)ProductEnum.AssetOptimizer
                : productId;

            var ppEnabledForCompanyAndProduct = await Task.Run(() =>
                GetPrimaryPropertySettingsForCompanyAndProduct(productIdForSetting),
                cancellationToken);

            var productEnabledForPrimaryProperty = await Task.Run(() =>
                _manageProductBatch.IsProductEnabledForUsePrimaryProperty(productId),
                cancellationToken);

            var productSetting = personaProductSettings.FirstOrDefault(item =>
                item.Name.Equals("UsePrimaryProperties", StringComparison.OrdinalIgnoreCase) &&
                item.ProductId == productId);

            bool personaProductUsePrimaryProperty;

            if (productSetting != null)
            {
                personaProductUsePrimaryProperty = productSetting.Value.Trim() == "1";
            }
            else
            {
                var userProperties = await Task.Run(() =>
                    _propertyRepository.ListUPFMPropertyInstanceByPersona(subjectUserPersonaId, ProductEnum.UnifiedPlatform),
                    cancellationToken);
                personaProductUsePrimaryProperty = userProperties.Count > 0;
            }

            return productEnabledForPrimaryProperty &&
                   personaProductUsePrimaryProperty &&
                   ppEnabledForCompanyAndProduct;
        }

        private async Task<(IList<ProductRole> ProductRoles, ListResponse PropertiesResponse, ListResponse RolesResponse)>
            GetProductRolesAndPropertiesAsync(
                Persona editorPersona,
                Persona userPersona,
                int productId,
                List<RoleTemplateProductRole> roleTemplateProductRoles,
                int? enterpriseRoleTemplateId,
                bool usePrimaryProperties,
                CancellationToken cancellationToken)
        {
            IList<ProductRole> productRoles = null;
            ListResponse rolesResponse;
            ListResponse propertiesResponse;

            // Get properties
            propertiesResponse = await _manageProductBatch.GetEnterpriseRoleUserPrimaryPropertiesDataAsync(
                editorPersona.PersonaId,
                userPersona.PersonaId,
                productId,
                usePrimaryProperties);

            // Get roles based on enterprise role template
            if (enterpriseRoleTemplateId != null ||
                (roleTemplateProductRoles != null && roleTemplateProductRoles.Any(m => m.ProductId == productId)))
            {
                rolesResponse = await Task.Run(() =>
                    _manageProductBatch.GetProductRoles(
                        editorPersona.PersonaId,
                        0,
                        productId,
                        userPersona.OrganizationPartyId,
                        _userClaim),
                    cancellationToken);

                productRoles = GetProductRoleList(roleTemplateProductRoles, productId);

                // Validate roles exist in product
                if (productRoles?.Any() == true && rolesResponse.Records?.Any() == true)
                {
                    productRoles = ValidateAndFilterRoles(productRoles, rolesResponse);
                }

                rolesResponse = CreateRolesListResponse(productRoles);
            }
            else
            {
                rolesResponse = await Task.Run(() =>
                    _manageProductBatch.GetProductRoles(
                        editorPersona.PersonaId,
                        userPersona.PersonaId,
                        productId,
                        userPersona.OrganizationPartyId,
                        _userClaim),
                    cancellationToken);

                productRoles = ExtractAssignedRoles(rolesResponse, productId);
            }

            return (productRoles, propertiesResponse, rolesResponse);
        }

        private IList<ProductRole> ValidateAndFilterRoles(IList<ProductRole> productRoles, ListResponse rolesResponse)
        {
            if (rolesResponse.Records[0].GetType() != typeof(ProductRole))
            {
                return productRoles;
            }

            var allProductRolesFromProducts = rolesResponse.Records.Cast<ProductRole>().ToList();
            return productRoles
                .Where(m => allProductRolesFromProducts.Any(l => l.ID.ToString() == m.ID))
                .ToList();
        }

        private IList<ProductRole> ExtractAssignedRoles(ListResponse rolesResponse, int productId)
        {
            if (rolesResponse?.Records?.Count == 0)
            {
                return new List<ProductRole>();
            }

            var roleType = rolesResponse.Records[0].GetType();

            return roleType.Name switch
            {
                nameof(ProductRole) when roleType == typeof(ProductRole) =>
                    rolesResponse.Records.Cast<ProductRole>().Where(p => p.IsAssigned).ToList(),

                nameof(ProductIntegration.Model.ProductRole) =>
                    rolesResponse.Records
                        .Cast<ProductIntegration.Model.ProductRole>()
                        .Where(p => p.IsAssigned)
                        .Select(p => new ProductRole
                        {
                            ID = p.GetRoleId,
                            Name = p.GetName,
                            IsAssigned = p.IsAssigned
                        })
                        .ToList(),

                nameof(Level) when productId == (int)ProductEnum.ResidentPortal =>
                    rolesResponse.Records
                        .Cast<Level>()
                        .Where(p => p.IsAssigned)
                        .Select(p => new ProductRole
                        {
                            ID = p.Id,
                            Name = p.Name,
                            IsAssigned = p.IsAssigned
                        })
                        .ToList(),

                _ => new List<ProductRole>()
            };
        }

        private async Task<ProductBatch> CreateProductSpecificBatchAsync(
            ProductBatchContext batchContext,
            ProductListsResult productLists,
            ProcessingContext context,
            CancellationToken cancellationToken)
        {
            // Handle Asset Optimization products
            if (ProductEnumHelper.GetAoProductList().Contains((ProductEnum)batchContext.ProductId))
            {
                return await CreateAoProductBatchAsync(batchContext, context, cancellationToken);
            }

            // Handle standard products
            if (batchContext.PropertiesResponse?.Records?.Count > 0)
            {
                var propertiesResponse = BatchHelper.GetUserAssignedPropertiesData(batchContext.PropertiesResponse);

                var batch = await _manageProductBatch.GetProductBatchRecordAsync(
                    batchContext.EditorPersonaId,
                    batchContext.SubjectPersonaId,
                    batchContext.ProductRoles,
                    propertiesResponse,
                    batchContext.RolesResponse,
                    batchContext.ProductId,
                    batchContext.UsePrimaryProperties,
                    cancellationToken);

                // Apply product-specific customizations
                await ApplyProductSpecificCustomizationsAsync(batch, batchContext, productLists, cancellationToken);

                return batch;
            }

            // Handle products with no properties
            return BatchHelper.CreateProductBatchRecord(
                batchContext.PropertiesResponse,
                batchContext.RolesResponse,
                batchContext.ProductId,
                batchContext.UsePrimaryProperties,
                batchContext.IntegrationType);
        }

        private async Task ApplyProductSpecificCustomizationsAsync(
            ProductBatch batch,
            ProductBatchContext context,
            ProductListsResult productLists,
            CancellationToken cancellationToken)
        {
            // UPFM products - handle removed properties
            if (context.IntegrationType == ProductIntegrationTypeEnum.UPFM)
            {
                await AddRemovedPropertiesForUpfmAsync(batch, context.SubjectPersonaId, context.ProductId, cancellationToken);
            }

            // Product ID 8 (specific product customization)
            if (context.ProductId == 8)
            {
                ApplyProduct8Attributes(batch, productLists.RoleTemplateProductRoles, context.ProductId);
            }

            // Mark as unassigned if no properties and not exempt
            if (context.PropertiesResponse?.Records?.Count == 0 &&
                context.ProductId != (int)ProductEnum.DataHub)
            {
                batch.InputJson.IsAssigned = false;
            }
        }

        private async Task AddRemovedPropertiesForUpfmAsync(
            ProductBatch batch,
            long subjectUserPersonaId,
            int productId,
            CancellationToken cancellationToken)
        {
            var currentProductProperties = await Task.Run(() =>
                _manageProductBatch.GetExistingUserPrimaryPropertiesData(subjectUserPersonaId, productId),
                cancellationToken);

            var currentUnifiedUIProperties = await Task.Run(() =>
                _manageProductBatch.GetExistingUserPrimaryPropertiesData(subjectUserPersonaId, (int)ProductEnum.UnifiedUI),
                cancellationToken);

            var newPropertyIds = batch.InputJson.PropertyList?.Select(int.Parse).ToList() ?? new List<int>();

            var propertiesToRemove = currentProductProperties
                .Except(currentUnifiedUIProperties)
                .Except(newPropertyIds)
                .ToList();

            if (propertiesToRemove.Any())
            {
                batch.InputJson.RemovedPropertyList = propertiesToRemove.Select(i => i.ToString()).ToList();
            }
        }

        private void ApplyProduct8Attributes(
            ProductBatch batch,
            List<RoleTemplateProductRole> roleTemplateProductRoles,
            int productId)
        {
            var productRoleData = roleTemplateProductRoles?.Where(p => p.ProductId == productId);
            var roleTemplateAdditionalRoles = productRoleData?
                .Select(p => new
                {
                    p.RoleTemplateProductRoleMappingId,
                    p.AttributeName,
                    p.AttributeValue
                })
                .Distinct()
                .ToList();

            if (roleTemplateAdditionalRoles == null)
            {
                return;
            }

            var attributes = new Dictionary<string, string>
            {
                ["hasAccessToSiteSpendManagementOnly"] = null,
                ["isAccountingAdmin"] = null,
                ["hasAccessToAllCurrentFutureProperties"] = null
            };

            foreach (var role in roleTemplateAdditionalRoles)
            {
                if (attributes.ContainsKey(role.AttributeName))
                {
                    attributes[role.AttributeName] = role.AttributeValue;
                }
            }

            if (bool.TryParse(attributes["hasAccessToSiteSpendManagementOnly"], out var siteSpend))
                batch.InputJson.HasAccessToSiteSpendManagementOnly = siteSpend;

            if (bool.TryParse(attributes["isAccountingAdmin"], out var accountingAdmin))
                batch.InputJson.IsAccountingAdmin = accountingAdmin;

            if (bool.TryParse(attributes["hasAccessToAllCurrentFutureProperties"], out var allProperties))
                batch.InputJson.HasAccessToAllCurrentFutureProperties = allProperties;
        }

        private async Task<ProductBatch> CreateAoProductBatchAsync(
            ProductBatchContext context,
            ProcessingContext processingContext,
            CancellationToken cancellationToken)
        {
            if (processingContext.ProductsWithNoProperties.Contains(context.ProductId))
            {
                return CreateAoProductWithoutProperties(
                    context.ProductRoles,
                    context.UsePrimaryProperties,
                    context.ProductId,
                    isAssigned: true);
            }

            var propertiesResponse = await _manageProductBatch.GetEnterpriseRoleUserPrimaryPropertiesDataAsync(
                context.EditorPersonaId,
                context.SubjectPersonaId,
                context.ProductId,
                context.UsePrimaryProperties,
                cancellationToken);

            propertiesResponse = BatchHelper.GetUserAssignedPropertiesData(propertiesResponse);

            // Use BatchHelper to create AO batch records
            var batches = new List<ProductBatch>();
            BatchHelper.CreateAoBatchRecords(
                _userClaim,
                context.EditorPersonaId,
                context.SubjectPersonaId,
                context.IsExternalUser,
                context.UsePrimaryProperties,
                propertiesResponse,
                context.ProductId,
                context.ProductRoles,
                batches);

            return batches.FirstOrDefault();
        }

        private async Task CreateDeletedProductBatchesAsync(
            List<int> deletedProducts,
            ProcessingContext context,
            List<ProductBatch> productBatches,
            CancellationToken cancellationToken)
        {
            foreach (var productId in deletedProducts)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (ProductEnumHelper.GetAoProductList().Contains((ProductEnum)productId))
                {
                    if (context.ProductsWithNoProperties.Contains(productId))
                    {
                        var batch = CreateAoProductWithoutProperties(null, true, productId, isAssigned: false);
                        productBatches.Add(batch);
                    }
                    else
                    {
                        var batches = new List<ProductBatch>();
                        BatchHelper.CreateAoBatchRecords(
                            _userClaim,
                            0,
                            0,
                            context.IsExternalUser,
                            true,
                            null,
                            productId,
                            null,
                            batches,
                            isDeleted: true);
                        productBatches.AddRange(batches);
                    }
                }
                else
                {
                    productBatches.Add(new ProductBatch
                    {
                        ProductId = productId,
                        StatusTypeId = DEFAULT_STATUS_TYPE,
                        RetryCount = DEFAULT_RETRY_COUNT,
                        InputJson = new RolePropertyList
                        {
                            PropertyList = new List<string>(),
                            RoleList = new List<string>(),
                            IsAssigned = false
                        }
                    });
                }
            }
        }

        private async Task<bool> SaveProductBatchesAsync(
            long editorUserPersonaId,
            long subjectUserPersonaId,
            Guid editorRealPageId,
            List<ProductBatch> productBatches,
            string oneSiteMixData,
            bool isOnesiteMix,
            long impersonatorUserId,
            string aoJson,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "Saving {BatchCount} product batches for user {SubjectPersonaId}",
                productBatches.Count,
                subjectUserPersonaId);

            return await Task.Run(() =>
                _enterpriseRoleProductRepository.SaveProductBatch(
                    editorUserPersonaId,
                    subjectUserPersonaId,
                    editorRealPageId,
                    productBatches,
                    oneSiteMixData,
                    isOnesiteMix,
                    (int)BatchProcessType.EnterpriseRoleCreateUpdateProductUser,
                    impersonatorUserId,
                    aoJson),
                cancellationToken);
        }

        private (string OneSiteMixData, bool IsOnesiteMix) PrepareOneSiteMixData(List<ProductBatch> productBatches)
        {
            if (!productBatches.Any(a => a.ProductId == (int)ProductEnum.OneSite) ||
                (!productBatches.Any(a => a.ProductId == (int)ProductEnum.Lead2Lease) &&
                 !productBatches.Any(a => a.ProductId == (int)ProductEnum.SeniorLeadManagement)))
            {
                return (string.Empty, false);
            }

            _logger.LogDebug("Preparing OneSite mixed product data");

            var oneSiteAndOtherProducts = new Dictionary<string, RolePropertyList>();

            var pbOneSite = productBatches.FirstOrDefault(a => a.ProductId == (int)ProductEnum.OneSite);
            if (pbOneSite != null)
            {
                oneSiteAndOtherProducts.Add(ProductEnum.OneSite.ToString(), pbOneSite.InputJson);
            }

            var pbLead2Lease = productBatches.FirstOrDefault(a => a.ProductId == (int)ProductEnum.Lead2Lease);
            if (pbLead2Lease != null)
            {
                oneSiteAndOtherProducts.Add(ProductEnum.Lead2Lease.ToString(), pbLead2Lease.InputJson);
                productBatches.Remove(pbLead2Lease);
            }

            var pbSeniorLead = productBatches.FirstOrDefault(a => a.ProductId == (int)ProductEnum.SeniorLeadManagement);
            if (pbSeniorLead != null)
            {
                oneSiteAndOtherProducts.Add(ProductEnum.SeniorLeadManagement.ToString(), pbSeniorLead.InputJson);
                productBatches.Remove(pbSeniorLead);
            }

            return (JsonConvert.SerializeObject(oneSiteAndOtherProducts), true);
        }

        #endregion

        #region Utility Methods

        private ProductBatch CreateAoProductWithoutProperties(
            IList<ProductRole> productRoles,
            bool usePrimaryProperties,
            int productId,
            bool isAssigned)
        {
            return new ProductBatch
            {
                ProductId = productId,
                StatusTypeId = DEFAULT_STATUS_TYPE,
                RetryCount = DEFAULT_RETRY_COUNT,
                InputJson = new RolePropertyList
                {
                    PropertyList = new List<string>(),
                    RoleList = productRoles?.Select(i => i.Name).ToList() ?? new List<string>(),
                    CompanyId = 0,
                    PropertyGroupList = new List<string>(),
                    UsePrimaryProperties = usePrimaryProperties,
                    IsAssigned = isAssigned
                }
            };
        }

        private async Task<List<int>> GetProductsWithNoPropertiesAsync(CancellationToken cancellationToken)
        {
            var upSettingList = await GetProductInternalSettingsAsync((int)ProductEnum.UnifiedPlatform, cancellationToken);
            var productsWithNoProperties = upSettingList?
                .FirstOrDefault(ps => ps.Name.Equals("UserAccessDetails_ProductsWithNoProperties", StringComparison.InvariantCultureIgnoreCase))?
                .Value;

            if (string.IsNullOrEmpty(productsWithNoProperties))
            {
                return new List<int>();
            }

            return productsWithNoProperties
                .Split(',')
                .Select(pId => int.TryParse(pId.Trim(), out var id) ? id : 0)
                .Where(id => id > 0)
                .Distinct()
                .ToList();
        }

        private async Task<List<ProductInternalSetting>> GetProductInternalSettingsAsync(
            int productId,
            CancellationToken cancellationToken)
        {
            var rpcache = new RPObjectCache();
            var cacheKey = $"productInternalSetting_{productId}";

            return await Task.Run(() =>
                rpcache.GetFromCache(cacheKey, 120, () =>
                    _productInternalSettingRepository.GetProductInternalSettings(productId).ToList()),
                cancellationToken);
        }

        private bool GetPrimaryPropertySettingsForCompanyAndProduct(int productId)
        {
            var productGlobalSettingType = _productInternalSettingRepository.GetProductSettingByType("UsePrimaryProperties");
            var companyProductSettings = _productRepository.GetProductSettings(_userClaim.OrganizationRealPageGuid);

            // Get organization setting
            var organizationUsePrimaryProperties = GetOrganizationPrimaryPropertySetting();
            if (organizationUsePrimaryProperties < 0)
            {
                return false;
            }

            // Get product and company settings
            var productGlobalSetting = productGlobalSettingType?
                .FirstOrDefault(p => p.Name.Equals("useprimaryproperties", StringComparison.OrdinalIgnoreCase) && p.ProductId == productId)?
                .Value?.Trim();

            var companyProductSetting = companyProductSettings?
                .FirstOrDefault(p => p.Name.Equals("useprimaryproperties", StringComparison.OrdinalIgnoreCase) && p.ProductId == productId)?
                .Value?.Trim();

            if (int.TryParse(productGlobalSetting, out var productGlobal) &&
                int.TryParse(companyProductSetting, out var companySetting) &&
                productGlobal >= 0)
            {
                return productGlobal == 1 && organizationUsePrimaryProperties == 1 && companySetting == 1;
            }

            return false;
        }

        private int GetOrganizationPrimaryPropertySetting()
        {
            var settings = _unifiedSettingsRepository.GetUnifiedSettings(_userClaim.OrganizationPartyId, "Company");
            var primaryPropertySetting = settings.FirstOrDefault(a =>
                a.Name.Equals("PrimaryProperty", StringComparison.OrdinalIgnoreCase));

            return int.TryParse(primaryPropertySetting?.Value, out var value) ? value : -1;
        }

        private IList<ProductRole> GetProductRoleList(List<RoleTemplateProductRole> roleTemplateProductRoles, int productId)
        {
            if (roleTemplateProductRoles == null)
            {
                return new List<ProductRole>();
            }

            return roleTemplateProductRoles
                .Where(p => p.ProductId == productId && p.RoleTemplateProductRoleMappingId != 0)
                .GroupBy(p => new
                {
                    p.RoleTemplateProductRoleMappingId,
                    p.ProductRoleId,
                    p.ProductRoleName
                })
                .Select(g => g.First())
                .Select(role => new ProductRole
                {
                    ID = role.ProductRoleId.ToString(),
                    Name = role.ProductRoleName,
                    IsAssigned = true
                })
                .ToList();
        }

        private ListResponse CreateRolesListResponse(IList<ProductRole> productRoles)
        {
            return new ListResponse
            {
                Records = productRoles?.Cast<object>().ToList() ?? new List<object>(),
                TotalRows = productRoles?.Count ?? 0,
                RowsPerPage = productRoles?.Count ?? 0,
                TotalPages = 1,
                ErrorReason = string.Empty
            };
        }

        /// <summary>
        /// Bundles Asset Optimization products into a single batch
        /// </summary>
        public static string BundleAoProducts(IList<ProductBatch> productList, int batchProcessorGroupId = 0)
        {
            var aoProductList = productList
                .Where(y => ProductEnumHelper.GetAoProductList().Contains((ProductEnum)y.ProductId))
                .ToList();

            if (!aoProductList.Any())
            {
                return string.Empty;
            }

            var aoData = new
            {
                IsAssigned = true,
                AoUserCompanyPropertyRoleDetailList = aoProductList.Select(aoProduct => new
                {
                    SelectedRoleValues = aoProduct.InputJson.RoleList,
                    SelectedPortfolioValues = aoProduct.InputJson.PropertyList,
                    CompanyId = aoProduct.InputJson.CompanyId,
                    Product = ProductEnumHelper.GetAoProductId((ProductEnum)aoProduct.ProductId),
                    DivisionName = ProductEnumHelper.GetAoDivisionName((ProductEnum)aoProduct.ProductId),
                    PropertyGroups = aoProduct.InputJson.PropertyGroupList,
                    IsAssigned = aoProduct.InputJson.IsAssigned,
                    ProductId = aoProduct.ProductId,
                    UsePrimaryProperties = aoProduct.InputJson.UsePrimaryProperties
                }).ToList()
            };

            // Remove AO products from main list
            foreach (var aoProduct in aoProductList)
            {
                productList.Remove(aoProduct);
            }

            // Add bundled AO batch
            productList.Add(new ProductBatch
            {
                ProductId = (int)ProductEnum.AssetOptimizer,
                StatusTypeId = DEFAULT_STATUS_TYPE,
                RetryCount = DEFAULT_RETRY_COUNT,
                BatchProcessorGroupId = batchProcessorGroupId,
                InputJson = null
            });

            return JsonConvert.SerializeObject(aoData);
        }

        #endregion

        #region Supporting Classes

        private class ProductListsResult
        {
            public List<int> NewProducts { get; set; } = new List<int>();
            public List<int> UpdatedProducts { get; set; } = new List<int>();
            public List<int> DeletedProducts { get; set; } = new List<int>();
            public List<RoleTemplateProductRole> RoleTemplateProductRoles { get; set; } = new List<RoleTemplateProductRole>();
            public bool ShouldReturnEarly { get; set; }
        }

        private class ProcessingContext
        {
            public Organization PersonaOrganization { get; set; }
            public List<ProductSettingList> PersonaProductSettings { get; set; }
            public List<int> ProductsWithNoProperties { get; set; } = new List<int>();
            public bool IsExternalUser { get; set; }
            public long ImpersonatorUserId { get; set; }
        }

        private class ProductBatchesResult
        {
            public List<ProductBatch> ProductBatches { get; set; } = new List<ProductBatch>();
            public string AoJson { get; set; } = string.Empty;
            public string ErrorMessage { get; set; }
        }

        #endregion
    }

    /// <summary>
    /// Null logger for legacy constructors
    /// </summary>
    internal class NullLogger<T> : ILogger<T>
    {
        public IDisposable BeginScope<TState>(TState state) => null;
        public bool IsEnabled(LogLevel logLevel) => false;
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter) { }
    }
}