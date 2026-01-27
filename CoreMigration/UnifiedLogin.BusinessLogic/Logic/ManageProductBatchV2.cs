using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using UnifiedLogin.BusinessLogic.Logic.Helper;
using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.BusinessLogic.Logic.Product;
using UnifiedLogin.BusinessLogic.Logic.Product.Interfaces;
using UnifiedLogin.BusinessLogic.Logic.ProductIntegration.Factory;
using UnifiedLogin.BusinessLogic.Repository;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.DataAccess;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.BlackBook;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product;
using UnifiedLogin.SharedObjects.Product.OneSite;
using UnifiedLogin.SharedObjects.Product.ResidentPortal;

namespace UnifiedLogin.BusinessLogic.Logic
{
    /// <summary>
    /// Manages product batch operations
    /// </summary>
    public class ManageProductBatchV2 : IManageProductBatch
    {
        #region Constants

        private const int CACHE_DURATION_MINUTES = 60;
        private const string CACHE_KEY_PREFIX_ROLE_RIGHTS = "enterpriseRoleProcessgetRolesByParty";
        private const string USE_PRIMARY_PROPERTIES_SETTING = "UsePrimaryProperties";

        #endregion

        #region Private Fields

        private readonly ILogger<ManageProductBatch> _logger;
        private readonly DefaultUserClaim _userClaim;
        private readonly IPropertyRepository _propertyRepository;
        private readonly IIntegrationTypeFactory _integrationTypeFactory;
        private readonly IProductRepository _productRepository;
        private readonly IUserRoleRightRepository _userRoleRightRepository;
        private readonly ISharedDataRepository _sharedDataRepository;
        private readonly IProductInternalSettingRepository _productInternalSettingRepository;
        private readonly IManageProductPanel _manageProductPanel;
        private readonly IRPObjectCache _cache;

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor with full dependency injection (recommended)
        /// </summary>
        public ManageProductBatchV2(
            ILogger<ManageProductBatch> logger,
            DefaultUserClaim userClaim,
            IPropertyRepository propertyRepository,
            IIntegrationTypeFactory integrationTypeFactory,
            IProductRepository productRepository,
            IUserRoleRightRepository userRoleRightRepository,
            ISharedDataRepository sharedDataRepository,
            IProductInternalSettingRepository productInternalSettingRepository,
            IManageProductPanel manageProductPanel,
            IRPObjectCache cache)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _userClaim = userClaim ?? throw new ArgumentNullException(nameof(userClaim));
            _propertyRepository = propertyRepository ?? throw new ArgumentNullException(nameof(propertyRepository));
            _integrationTypeFactory = integrationTypeFactory ?? throw new ArgumentNullException(nameof(integrationTypeFactory));
            _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
            _userRoleRightRepository = userRoleRightRepository ?? throw new ArgumentNullException(nameof(userRoleRightRepository));
            _sharedDataRepository = sharedDataRepository ?? throw new ArgumentNullException(nameof(sharedDataRepository));
            _productInternalSettingRepository = productInternalSettingRepository ?? throw new ArgumentNullException(nameof(productInternalSettingRepository));
            _manageProductPanel = manageProductPanel ?? throw new ArgumentNullException(nameof(manageProductPanel));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        /// <summary>
        /// Test constructor with mocked dependencies
        /// </summary>
        public ManageProductBatchV2(
            IRepository repository,
            HttpMessageHandler messageHandler,
            DefaultUserClaim userClaim,
            IOneSiteProductService oneSiteProductService = null)
        {
            _logger = new NullLogger<ManageProductBatch>();
            _userClaim = userClaim ?? throw new ArgumentNullException(nameof(userClaim));

            _userRoleRightRepository = new UserRoleRightRepository(repository, messageHandler, userClaim);
            _sharedDataRepository = new SharedDataRepository(repository, userClaim);
            _productRepository = new ProductRepository(repository, userClaim);
            _propertyRepository = new PropertyRepository(repository);
            _productInternalSettingRepository = new ProductInternalSettingRepository(repository);

            var manageUnifiedLogin = new ManageUnifiedLogin(repository, userClaim, messageHandler);
            var manageProductOneSite = new ManageProductOneSite(repository, userClaim, messageHandler, oneSiteProductService);
            var manageProduct = new ManageProduct(repository, userClaim, messageHandler);
            var manageBlueBook = new ManageBlueBook(userClaim, repository, _productInternalSettingRepository, messageHandler);

            _integrationTypeFactory = new IntegrationTypeFactory(
                manageProduct,
                manageUnifiedLogin,
                manageProductOneSite,
                _productRepository,
                _productInternalSettingRepository,
                userClaim);

            _manageProductPanel = new ManageProductPanel(userClaim, repository, manageBlueBook, messageHandler, manageProductOneSite);
            _cache = new RPObjectCacheV2();
        }

        ///// <summary>
        ///// Legacy constructor for backward compatibility
        ///// </summary>
        //[Obsolete("Use constructor with dependency injection instead")]
        //public ManageProductBatch(DefaultUserClaim userClaim)
        //{
        //    _logger = new NullLogger<ManageProductBatch>();
        //    _userClaim = userClaim ?? throw new ArgumentNullException(nameof(userClaim));

        //    var manageProduct = new ManageProduct(_userClaim);
        //    var manageUnifiedLogin = new ManageUnifiedLogin(_userClaim);
        //    var manageProductOneSite = new ManageProductOneSite(_userClaim);

        //    _productInternalSettingRepository = new ProductInternalSettingRepository();
        //    _productRepository = new ProductRepository();
        //    _propertyRepository = new PropertyRepository();
        //    _userRoleRightRepository = new UserRoleRightRepository();
        //    _sharedDataRepository = new SharedDataRepository();

        //    _integrationTypeFactory = new IntegrationTypeFactory(
        //        manageProduct,
        //        manageUnifiedLogin,
        //        manageProductOneSite,
        //        _productRepository,
        //        _productInternalSettingRepository,
        //        _userClaim);

        //    _manageProductPanel = new ManageProductPanel(_userClaim);
        //    _cache = new RPObjectCache();
        //}

        #endregion

        #region Public Methods

        /// <inheritdoc />
        public async Task<ProductBatch> GetProductBatchRecordAsync(
            long editorUserPersonaId,
            long subjectUserPersonaId,
            IList<ProductRole> productRoles,
            ListResponse propertiesResponse,
            ListResponse rolesResponse,
            int productId,
            bool usePrimaryProperties,
            CancellationToken cancellationToken = default)
        {
            if (productRoles == null)
                throw new ArgumentNullException(nameof(productRoles));

            _logger.LogDebug(
                "Creating product batch record for product {ProductId}, user {SubjectPersonaId}",
                productId,
                subjectUserPersonaId);

            return (ProductEnum)productId switch
            {
                ProductEnum.FinancialSuite => await CreateFinancialSuiteBatchAsync(
                    editorUserPersonaId, subjectUserPersonaId, propertiesResponse, rolesResponse, productId, usePrimaryProperties, cancellationToken),

                ProductEnum.VendorServices => await CreateVendorServicesBatchAsync(
                    editorUserPersonaId, subjectUserPersonaId, propertiesResponse, rolesResponse, productId, usePrimaryProperties, cancellationToken),

                ProductEnum.ResidentPortal => await CreateResidentPortalBatchAsync(
                    editorUserPersonaId, subjectUserPersonaId, productRoles, propertiesResponse, productId, usePrimaryProperties, cancellationToken),

                ProductEnum.OnSite => await CreateOnSiteBatchAsync(
                    editorUserPersonaId, subjectUserPersonaId, propertiesResponse, rolesResponse, productId, usePrimaryProperties, cancellationToken),

                ProductEnum.DepositAlternative => await CreateDepositIQBatchAsync(
                    editorUserPersonaId, subjectUserPersonaId, productRoles, productId, usePrimaryProperties, cancellationToken),

                ProductEnum.IntegrationMarketplace => await CreateIntegrationMarketplaceBatchAsync(
                    productRoles, productId, usePrimaryProperties, cancellationToken),

                ProductEnum.LeadManagement => await CreateLeadManagementBatchAsync(
                    editorUserPersonaId, subjectUserPersonaId, productRoles, productId, usePrimaryProperties, cancellationToken),

                ProductEnum.LeadAnalytics => await CreateLeadAnalyticsBatchAsync(
                    editorUserPersonaId, subjectUserPersonaId, productRoles, productId, usePrimaryProperties, cancellationToken),

                ProductEnum.PortfolioManagement => await CreatePortfolioManagementBatchAsync(
                    editorUserPersonaId, subjectUserPersonaId, productRoles, productId, usePrimaryProperties, cancellationToken),

                ProductEnum.UtilityManagement => await CreateUtilityManagementBatchAsync(
                    editorUserPersonaId, subjectUserPersonaId, propertiesResponse, rolesResponse, productId, usePrimaryProperties, cancellationToken),

                _ => await CreateStandardBatchAsync(
                    propertiesResponse, rolesResponse, productId, usePrimaryProperties, cancellationToken)
            };
        }

        /// <inheritdoc />
        /// <remarks>Synchronous wrapper for backward compatibility</remarks>
        [Obsolete("Use GetProductBatchRecordAsync instead")]
        public ProductBatch GetProductBatchRecord(
            long editorUserPersonaId,
            long subjectUserPersonaId,
            IList<ProductRole> productRoles,
            ListResponse propertiesResponse,
            ListResponse rolesResponse,
            int product,
            bool usePrimaryProperties)
        {
            return GetProductBatchRecordAsync(
                editorUserPersonaId,
                subjectUserPersonaId,
                productRoles,
                propertiesResponse,
                rolesResponse,
                product,
                usePrimaryProperties).GetAwaiter().GetResult();
        }

        /// <inheritdoc />
        public async Task<ListResponse> GetEnterpriseRoleUserPrimaryPropertiesDataAsync(
            long editorPersonaId,
            long userPersonaId,
            int productId,
            bool usePrimaryProperties = true,
            CancellationToken cancellationToken = default)
        {
            var userProperties = await Task.Run(() =>
                _propertyRepository.ListUPFMPropertyInstanceByPersona(userPersonaId, ProductEnum.UnifiedPlatform),
                cancellationToken);

            ListResponse result;

            if (productId == (int)ProductEnum.KnockCRM)
            {
                result = new ListResponse
                {
                    Records = userProperties.Cast<object>().ToList(),
                    TotalRows = userProperties.Count
                };
            }
            else
            {
                result = await Task.Run(() =>
                    _manageProductPanel.GetProductProperties(editorPersonaId, userPersonaId, productId, null),
                    cancellationToken);
            }

            if (!result.IsError && usePrimaryProperties)
            {
                var upfmProperty = new UPFMProperty
                {
                    id = userProperties?.Select(p => p.InstanceId.ToString()).ToList()
                };

                result = await Task.Run(() =>
                    _manageProductPanel.CompareProductAndPrimaryProperties(upfmProperty, productId, result),
                    cancellationToken);
            }

            return result;
        }

        /// <inheritdoc />
        [Obsolete("Use GetEnterpriseRoleUserPrimaryPropertiesDataAsync instead")]
        public ListResponse GetEnterpriseRoleUserPrimaryPropertiesData(
            long editorPersonaId,
            long userPersonaId,
            int productId,
            bool usePrimaryProperties = true)
        {
            return GetEnterpriseRoleUserPrimaryPropertiesDataAsync(
                editorPersonaId,
                userPersonaId,
                productId,
                usePrimaryProperties).GetAwaiter().GetResult();
        }

        /// <inheritdoc />
        public List<int> GetExistingUserPrimaryPropertiesData(long userPersonaId, int productId)
        {
            return _propertyRepository.ListUPFMPropertyInstanceIdByPersona(userPersonaId, productId);
        }

        /// <inheritdoc />
        public ListResponse GetProductRoles(
            long editorPersonaId,
            long userPersonaId,
            int productId,
            long organizationPartyId,
            DefaultUserClaim userClaim)
        {
            return _manageProductPanel.GetProductRoles(
                editorPersonaId,
                userPersonaId,
                organizationPartyId,
                productId,
                null,
                null);
        }

        /// <inheritdoc />
        public bool IsProductEnabledForUsePrimaryProperty(int productId)
        {
            var productInternalSettingList = _productInternalSettingRepository.GetProductInternalSettings(productId);
            var productInternalSetting = productInternalSettingList.FirstOrDefault(item =>
                item.Name.Equals(USE_PRIMARY_PROPERTIES_SETTING, StringComparison.OrdinalIgnoreCase));

            return productInternalSetting?.Value.Trim() == "1";
        }

        /// <inheritdoc />
        public List<string> GetPersonaRoleRights(long personaId, long orgPartyId)
        {
            var userRights = new List<string>();

            var userRoles = _userRoleRightRepository.ListRoleByPersona(
                (int)ProductEnum.UnifiedPlatform,
                personaId,
                orgPartyId);

            if (userRoles == null || !userRoles.Any())
            {
                return userRights;
            }

            var cacheKey = $"{CACHE_KEY_PREFIX_ROLE_RIGHTS}_{orgPartyId}_{(int)ProductEnum.UnifiedPlatform}";
            var roleList = _cache.GetFromCache<IList<UserRoleRights>>(cacheKey, CACHE_DURATION_MINUTES, () =>
            {
                var productList = _sharedDataRepository.GetProductIdsByCompany(orgPartyId);
                return _userRoleRightRepository.GetAllRoleRights(orgPartyId, productList, (int)ProductEnum.UnifiedPlatform);
            });

            if (roleList == null)
            {
                return userRights;
            }

            foreach (var userRole in userRoles)
            {
                var roleRights = roleList.FirstOrDefault(r => r.RoleId == userRole.RoleID);
                if (roleRights?.UserRights == null)
                {
                    continue;
                }

                var rightsToAdd = roleRights.UserRights
                    .Where(right => !string.IsNullOrWhiteSpace(right.RightNickName))
                    .Select(right => right.RightNickName.Trim())
                    .Where(rightName => !userRights.Contains(rightName));

                userRights.AddRange(rightsToAdd);
            }

            return userRights.Distinct().ToList();
        }

        #endregion

        #region Product-Specific Batch Creation Methods

        private async Task<ProductBatch> CreateFinancialSuiteBatchAsync(
            long editorUserPersonaId,
            long subjectUserPersonaId,
            ListResponse propertiesResponse,
            ListResponse rolesResponse,
            int productId,
            bool usePrimaryProperties,
            CancellationToken cancellationToken)
        {
            var accounting = new ManageProductOneSiteAccounting(_userClaim);

            var propertyGroupsTask = Task.Run(() =>
                accounting.GetUserPropertyGroups(editorUserPersonaId, subjectUserPersonaId, null),
                cancellationToken);

            var companiesTask = Task.Run(() =>
                accounting.GetUserCompanies(editorUserPersonaId, subjectUserPersonaId, null),
                cancellationToken);

            await Task.WhenAll(propertyGroupsTask, companiesTask);

            var propertyGroupResponse = await propertyGroupsTask;
            var companiesResponse = await companiesTask;

            return BatchHelper.CreateFinancialSuiteProductBatchRecord(
                propertiesResponse,
                rolesResponse,
                productId,
                companiesResponse,
                propertyGroupResponse,
                usePrimaryProperties);
        }

        private async Task<ProductBatch> CreateVendorServicesBatchAsync(
            long editorUserPersonaId,
            long subjectUserPersonaId,
            ListResponse propertiesResponse,
            ListResponse rolesResponse,
            int productId,
            bool usePrimaryProperties,
            CancellationToken cancellationToken)
        {
            var vs = new ManageProductVendorServices(_userClaim);

            var notificationsTask = Task.Run(() =>
                vs.GetNotificationSettings(editorUserPersonaId, subjectUserPersonaId),
                cancellationToken);

            var propertyGroupsTask = Task.Run(() =>
                vs.GetPropertyGroups(editorUserPersonaId, subjectUserPersonaId, null),
                cancellationToken);

            await Task.WhenAll(notificationsTask, propertyGroupsTask);

            var notifications = await notificationsTask;
            var propertyGroupResponse = await propertyGroupsTask;

            return BatchHelper.CreateVendorServiceProductBatchRecord(
                propertiesResponse,
                rolesResponse,
                propertyGroupResponse,
                notifications,
                productId,
                usePrimaryProperties);
        }

        private async Task<ProductBatch> CreateResidentPortalBatchAsync(
            long editorUserPersonaId,
            long subjectUserPersonaId,
            IList<ProductRole> productRoles,
            ListResponse propertiesResponse,
            int productId,
            bool usePrimaryProperties,
            CancellationToken cancellationToken)
        {
            var manageProductResidentPortal = new ManageProductResidentPortal(_userClaim);

            var levelList = productRoles.Select(rRole => new Level
            {
                Id = rRole.ID,
                Name = rRole.Name,
                IsAssigned = rRole.IsAssigned
            }).Cast<ILevel>().ToList();

            var notificationsTask = Task.Run(() =>
                manageProductResidentPortal.GetNotificationSettings(editorUserPersonaId, subjectUserPersonaId),
                cancellationToken);

            var messagingGroupsTask = Task.Run(() =>
                manageProductResidentPortal.ListMessageGroups(editorUserPersonaId, subjectUserPersonaId),
                cancellationToken);

            await Task.WhenAll(notificationsTask, messagingGroupsTask);

            var notifications = await notificationsTask;
            var messagingGroups = await messagingGroupsTask;

            return BatchHelper.CreateResidentPortalProductBatchRecord(
                propertiesResponse,
                levelList,
                notifications,
                messagingGroups,
                productId,
                usePrimaryProperties);
        }

        private async Task<ProductBatch> CreateOnSiteBatchAsync(
            long editorUserPersonaId,
            long subjectUserPersonaId,
            ListResponse propertiesResponse,
            ListResponse rolesResponse,
            int productId,
            bool usePrimaryProperties,
            CancellationToken cancellationToken)
        {
            var manageProductOnSite = new ManageProductOnSite(_userClaim);

            var regionResponse = await Task.Run(() =>
                manageProductOnSite.GetRegions(editorUserPersonaId, subjectUserPersonaId, null),
                cancellationToken);

            return BatchHelper.CreateOnSiteBatchRecord(
                propertiesResponse,
                rolesResponse,
                regionResponse,
                productId,
                usePrimaryProperties);
        }

        private async Task<ProductBatch> CreateDepositIQBatchAsync(
            long editorUserPersonaId,
            long subjectUserPersonaId,
            IList<ProductRole> productRoles,
            int productId,
            bool usePrimaryProperties,
            CancellationToken cancellationToken)
        {
            var productLogic = ManageProductFactory.GetProductLogic(productId, editorUserPersonaId, subjectUserPersonaId, _userClaim);
            var productUser = await Task.Run(() => productLogic.GetProductUser(), cancellationToken);
            productUser.RoleList = productRoles.Select(p => p.ID).ToList();

            return BatchHelper.CreateProductBatchRecordForDepositIQ(productUser, usePrimaryProperties);
        }

        private Task<ProductBatch> CreateIntegrationMarketplaceBatchAsync(
            IList<ProductRole> productRoles,
            int productId,
            bool usePrimaryProperties,
            CancellationToken cancellationToken)
        {
            var existingRoleId = productRoles.Any()
                ? int.Parse(productRoles.First().ID)
                : 0;

            var batch = BatchHelper.CreateIntegrationMarketplaceBatchRecord(
                existingRoleId,
                productId,
                usePrimaryProperties);

            return Task.FromResult(batch);
        }

        private async Task<ProductBatch> CreateLeadManagementBatchAsync(
            long editorUserPersonaId,
            long subjectUserPersonaId,
            IList<ProductRole> productRoles,
            int productId,
            bool usePrimaryProperties,
            CancellationToken cancellationToken)
        {
            var productLogic = ManageProductFactory.GetProductLogic(productId, editorUserPersonaId, subjectUserPersonaId, _userClaim);
            var productUser = await Task.Run(() => productLogic.GetProductUser(), cancellationToken);

            return BatchHelper.CreateILMProductBatchRecord(
                ProductEnum.LeadManagement,
                productUser.Properties,
                productRoles.Select(p => p.ID).ToList(),
                null, // No groups for Lead Management
                usePrimaryProperties);
        }

        private async Task<ProductBatch> CreateLeadAnalyticsBatchAsync(
            long editorUserPersonaId,
            long subjectUserPersonaId,
            IList<ProductRole> productRoles,
            int productId,
            bool usePrimaryProperties,
            CancellationToken cancellationToken)
        {
            var productLogic = ManageProductFactory.GetProductLogic(productId, editorUserPersonaId, subjectUserPersonaId, _userClaim);
            var productUser = await Task.Run(() => productLogic.GetProductUser(), cancellationToken);

            return BatchHelper.CreateILMProductBatchRecord(
                ProductEnum.LeadAnalytics,
                productUser.Properties,
                productRoles.Select(p => p.ID).ToList(),
                productUser.PropertyGroups,
                usePrimaryProperties);
        }

        private async Task<ProductBatch> CreatePortfolioManagementBatchAsync(
            long editorUserPersonaId,
            long subjectUserPersonaId,
            IList<ProductRole> productRoles,
            int productId,
            bool usePrimaryProperties,
            CancellationToken cancellationToken)
        {
            var productLogic = ManageProductFactory.GetProductLogic(productId, editorUserPersonaId, subjectUserPersonaId, _userClaim);
            var productUser = await Task.Run(() => productLogic.GetProductUser(), cancellationToken);

            var propertyRoles = productUser.PropertyRoleList;
            var roles = productRoles.Select(p => p.ID).ToList();

            return BatchHelper.CreateProductBatchRecordForPortfolioManagement(propertyRoles, roles, usePrimaryProperties);
        }

        private async Task<ProductBatch> CreateUtilityManagementBatchAsync(
            long editorUserPersonaId,
            long subjectUserPersonaId,
            ListResponse propertiesResponse,
            ListResponse rolesResponse,
            int productId,
            bool usePrimaryProperties,
            CancellationToken cancellationToken)
        {
            var manageProductRum = new ManageProductRum(_userClaim);

            var propertyGroupsTask = Task.Run(() =>
                manageProductRum.GetPropertyGroups(editorUserPersonaId, subjectUserPersonaId, null),
                cancellationToken);

            var regionsTask = Task.Run(() =>
                manageProductRum.GetRegions(editorUserPersonaId, subjectUserPersonaId, null),
                cancellationToken);

            await Task.WhenAll(propertyGroupsTask, regionsTask);

            var propertyGroupResponse = await propertyGroupsTask;
            var regionResponse = await regionsTask;

            return BatchHelper.CreateRumProductBatchRecord(
                propertiesResponse,
                propertyGroupResponse,
                regionResponse,
                rolesResponse,
                usePrimaryProperties);
        }

        private Task<ProductBatch> CreateStandardBatchAsync(
            ListResponse propertiesResponse,
            ListResponse rolesResponse,
            int productId,
            bool usePrimaryProperties,
            CancellationToken cancellationToken)
        {
            var type = _integrationTypeFactory.GetIntegrationTypeForProductId(productId);

            var productBatchRecord = BatchHelper.CreateProductBatchRecord(
                propertiesResponse,
                rolesResponse,
                productId,
                usePrimaryProperties,
                type);

            return Task.FromResult(productBatchRecord);
        }

        #endregion
    }

    #region Supporting Types

    ///// <summary>
    ///// Null logger implementation for legacy constructors
    ///// </summary>
    //internal class NullLogger<T> : ILogger<T>
    //{
    //    public IDisposable BeginScope<TState>(TState state) where TState : notnull => null;
    //    public bool IsEnabled(LogLevel logLevel) => false;
    //    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter) { }
    //}

    #endregion
}