using Dapper;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.DataAccess;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Batch;
using UnifiedLogin.SharedObjects.Constants;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Helper;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product;

namespace UnifiedLogin.BusinessLogic.Services.Product;

/// <summary>
/// Handles product batch operations with async patterns
/// Extracted from UserRepository for better separation of concerns
/// </summary>
public class ProductBatchService : IProductBatchService
{
    private readonly IRepositoryAsync _repositoryAsync;
    private readonly IUserLoginRepository _userLoginRepository;
    private readonly IManagePersona _managePersona;
    private readonly DefaultUserClaim _userClaim;
    private readonly ILogger<ProductBatchService> _logger;

    public ProductBatchService(
        IRepositoryAsync repositoryAsync,
        IUserLoginRepository userLoginRepository,
        IManagePersona managePersona,
        DefaultUserClaim userClaim,
        ILogger<ProductBatchService> logger)
    {
        _repositoryAsync = repositoryAsync ?? throw new ArgumentNullException(nameof(repositoryAsync));
        _userLoginRepository = userLoginRepository ?? throw new ArgumentNullException(nameof(userLoginRepository));
        _managePersona = managePersona ?? throw new ArgumentNullException(nameof(managePersona));
        _userClaim = userClaim ?? throw new ArgumentNullException(nameof(userClaim));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Create batch processor group (Async)
    /// </summary>
    public async Task<BatchProcessorGroup> CreateBatchProcessGroupAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            await using var repo = _repositoryAsync;

            var param = new DynamicParameters();
            param.Add("@BatchProcessorGroupID", 0, dbType: DbType.Int32, direction: ParameterDirection.Output);

            await repo.ExecuteNonQueryAsync(
                StoredProcNameConstants.SP_CreateBatchProcessorGroup,
                param,
                cancellationToken);

            var groupId = param.Get<int>("@BatchProcessorGroupID");

            _logger.LogDebug("Created batch processor group with ID {GroupId}", groupId);

            return new BatchProcessorGroup
            {
                BatchProcessorGroupId = groupId,
                BatchProcessorGroupActivityLogged = false
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating batch processor group");
            throw;
        }
    }

    /// <summary>
    /// Bundle AO products for batch processing
    /// Extracted from UserRepository.BundleAoProducts
    /// </summary>
    public string BundleAoProducts(IList<ProductBatch> productList, int batchProcessorGroupId = 0)
    {
        var sb = new StringBuilder();

        // Check if any AO products in product batch and group them
        var aoProductList = productList
            .Where(y => ProductEnumHelper.GetAoProductList().Contains((ProductEnum)y.ProductId))
            .ToList();

        if (!aoProductList.Any())
            return string.Empty;

        dynamic expandoList = new ExpandoObject();
        expandoList.IsAssigned = true;
        expandoList.AoUserCompanyPropertyRoleDetailList = new List<ExpandoObject>();

        // Collect ALL JSON(s) for AO products
        foreach (var aoProduct in aoProductList)
        {
            dynamic expandoAo = new ExpandoObject();
            expandoAo.SelectedRoleValues = aoProduct.InputJson.RoleList;
            expandoAo.SelectedPortfolioValues = aoProduct.InputJson.PropertyList;
            expandoAo.CompanyId = aoProduct.InputJson.CompanyId;
            expandoAo.Product = ProductEnumHelper.GetAoProductId((ProductEnum)aoProduct.ProductId);
            expandoAo.DivisionName = ProductEnumHelper.GetAoDivisionName((ProductEnum)aoProduct.ProductId);
            expandoAo.PropertyGroups = aoProduct.InputJson.PropertyGroupList;
            expandoAo.IsAssigned = aoProduct.InputJson.IsAssigned;
            expandoAo.ProductId = aoProduct.ProductId;
            expandoAo.UsePrimaryProperties = aoProduct.InputJson.UsePrimaryProperties;

            expandoList.AoUserCompanyPropertyRoleDetailList.Add(expandoAo);
            productList.Remove(aoProduct);
        }

        sb.Append(JsonConvert.SerializeObject(expandoList));

        // Add bundled AO product back to list
        productList.Add(new ProductBatch
        {
            ProductId = (int)ProductEnum.AssetOptimizer,
            StatusTypeId = 5,
            RetryCount = 0,
            BatchProcessorGroupId = batchProcessorGroupId,
            InputJson = null
        });

        return sb.ToString();
    }

    /// <summary>
    /// Save product details for a user (Async)
    /// </summary>
    public async Task<int> SaveProductDetailsAsync(
        IList<ProductBatch> productList,
        long createUserPersonaId,
        long assignUserPersonaId,
        Guid organizationRealPageId,
        int userTypeId,
        bool userIsActive,
        CancellationToken cancellationToken = default,
        IList<string> aoProducts = null)
    {
        if (productList == null || !productList.Any())
        {
            _logger.LogDebug("No products to save for PersonaId {PersonaId}", assignUserPersonaId);
            return 0;
        }

        try
        {
            await using var repo = _repositoryAsync;

            var batchGroup = await CreateBatchProcessGroupAsync(cancellationToken);
            var productCount = 0;

            // Set batch group ID for all products
            foreach (var product in productList)
            {
                product.BatchProcessorGroupId = batchGroup.BatchProcessorGroupId;
            }

            // Remove EasyLMS (handled separately)
            var easyLMSProduct = productList.FirstOrDefault(p => p.ProductId == (int)ProductEnum.EasyLMS);
            if (easyLMSProduct != null)
            {
                productList.Remove(easyLMSProduct);
            }

            // Add SalesForce if needed
            if (userTypeId != (int)UserRoleType.UserNoEmail)
            {
                if (!productList.Any(p => p.ProductId == (int)ProductEnum.SalesForce))
                {
                    productList.Add(new ProductBatch
                    {
                        ProductId = (int)ProductEnum.SalesForce,
                        StatusTypeId = 5,
                        RetryCount = 0,
                        BatchProcessorGroupId = batchGroup.BatchProcessorGroupId,
                        InputJson = new RolePropertyList
                        {
                            PropertyList = new List<string>(),
                            RoleList = new List<string>(),
                            IsAssigned = userIsActive
                        }
                    });
                }
            }

            // Handle AO products bundling
            var aoInputJson = BundleAoProducts(productList, batchGroup.BatchProcessorGroupId);

            // Save each product
            foreach (var product in productList)
            {
                if (product.ProductId == (int)ProductEnum.UnifiedPlatform ||
                    product.ProductId == (int)ProductEnum.UnifiedUI)
                    continue;

                var inputJson = product.ProductId == (int)ProductEnum.AssetOptimizer
                    ? aoInputJson
                    : JsonConvert.SerializeObject(product.InputJson);

                await SaveProductBatchAsync(
                    product,
                    createUserPersonaId,
                    assignUserPersonaId,
                    organizationRealPageId,
                    inputJson,
                    (int)BatchProcessType.CreateUpdateProductUser,
                    cancellationToken);

                productCount++;
            }

            _logger.LogInformation("Saved {ProductCount} products for PersonaId {PersonaId}",
                productCount, assignUserPersonaId);

            return productCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving product details for PersonaId {PersonaId}", assignUserPersonaId);
            throw;
        }
    }

    /// <summary>
    /// Save individual product batch (Async)
    /// </summary>
    public async Task SaveProductBatchAsync(
        IProductBatch product,
        long createUserPersonaId,
        long assignUserPersonaId,
        Guid realPageId,
        string inputJson,
        int batchProcessTypeId = 1,
        CancellationToken cancellationToken = default)
    {
        try
        {
            product.CreateUserPersonaId = createUserPersonaId;
            product.AssignUserPersonaId = assignUserPersonaId;

            await using var repo = _repositoryAsync;

            var impersonatorUserId = await GetImpersonatorUserIdAsync(cancellationToken);

            var param = new
            {
                PersonRealPageId = realPageId,
                CreateUserPersonaId = product.CreateUserPersonaId,
                AssignUserPersonaId = product.AssignUserPersonaId,
                ProductId = product.ProductId,
                BatchProcessorGroupId = product.BatchProcessorGroupId,
                StatusTypeId = product.StatusTypeId,
                RetryCount = product.RetryCount,
                InputJson = inputJson,
                CorrelationId = _userClaim.CorrelationId.ToString(),
                BatchProcessTypeId = batchProcessTypeId,
                ImpersonatorUserId = impersonatorUserId,
                UseAPIV2 = true
            };

            var response = await repo.GetOneAsync<RepositoryResponse>(
                StoredProcNameConstants.SP_CreateProductBatch,
                param,
                cancellationToken);

            if (response == null || response.Id == 0)
            {
                throw new InvalidOperationException(
                    $"Failed to create product batch for ProductId {product.ProductId}");
            }

            _logger.LogDebug("Saved product batch for ProductId {ProductId}, BatchId {BatchId}",
                product.ProductId, response.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving product batch for ProductId {ProductId}", product.ProductId);
            throw;
        }
    }

    /// <summary>
    /// Disable user products (Async)
    /// </summary>
    public async Task DisableUserProductsAsync(
        Guid createUserRealPageId,
        long createUserPersonaId,
        IList<UserLoginOnly> userLogins,
        CancellationToken cancellationToken = default)
    {
        if (userLogins == null || !userLogins.Any())
            return;

        _logger.LogInformation("Disabling products for {UserCount} users", userLogins.Count);

        // Process in parallel with controlled concurrency
        var semaphore = new SemaphoreSlim(5); // Max 5 concurrent operations
        var tasks = userLogins.Select(async user =>
        {
            await semaphore.WaitAsync(cancellationToken);
            try
            {
                await DisableUserProductDataAsync(
                    createUserRealPageId,
                    createUserPersonaId,
                    user,
                    cancellationToken);
            }
            finally
            {
                semaphore.Release();
            }
        });

        await Task.WhenAll(tasks);
    }

    /// <summary>
    /// Activate user products (Async)
    /// </summary>
    public async Task ActivateUserProductsAsync(
        Guid createUserRealPageId,
        long createUserPersonaId,
        IList<UserLoginOnly> userLogins,
        CancellationToken cancellationToken = default)
    {
        if (userLogins == null || !userLogins.Any())
            return;

        _logger.LogInformation("Activating products for {UserCount} users", userLogins.Count);

        foreach (var userLogin in userLogins)
        {
            var persona = _managePersona.GetFirstAvailablePersonaByCompany(
                userLogin.RealPageId,
                _userClaim.OrganizationPartyId);

            await ProcessActivatedUserProductBatchDataAsync(
                persona.PersonaId,
                createUserRealPageId,
                createUserPersonaId,
                cancellationToken);
        }
    }

    #region Private Helper Methods

    private async Task DisableUserProductDataAsync(
        Guid createUserRealPageId,
        long createUserPersonaId,
        UserLoginOnly user,
        CancellationToken cancellationToken)
    {
        try
        {
            var userLoginOnly = _userLoginRepository.GetUserLoginOnly(user.RealPageId);
            var userPersonaOrganizationList = _userLoginRepository.ListOrganizationByLoginName(userLoginOnly.LoginName);
            var currentPrimaryOrgStatus = _userLoginRepository.GetUserOrganizationWithStatus(
                userLoginOnly.UserId,
                userLoginOnly.LastLogin,
                0,
                true);

            if (userPersonaOrganizationList == null || currentPrimaryOrgStatus == null)
                return;

            var impersonatorUserId = await GetImpersonatorUserIdAsync(cancellationToken);

            if (_userClaim.OrganizationPartyId == currentPrimaryOrgStatus.PartyId &&
                userPersonaOrganizationList.Count > 1)
            {
                // Process multiple organizations
                foreach (var userOrg in userPersonaOrganizationList)
                {
                    var currentOrgStatus = _userLoginRepository.GetUserOrganizationWithStatus(
                        userLoginOnly.UserId,
                        userLoginOnly.LastLogin,
                        userOrg.OrganizationPartyId,
                        false);

                    if (currentOrgStatus.Status == UserUiStatusType.Disabled)
                    {
                        var persona = _managePersona.GetFirstAvailablePersonaByCompany(
                            userLoginOnly.RealPageId,
                            userOrg.OrganizationPartyId);

                        await ProcessDisableUserProductDataAsync(
                            persona.PersonaId,
                            createUserRealPageId,
                            createUserPersonaId,
                            persona.UserTypeId,
                            impersonatorUserId,
                            cancellationToken);
                    }
                }
            }
            else
            {
                var persona = _managePersona.GetFirstAvailablePersonaByCompany(
                    userLoginOnly.RealPageId,
                    _userClaim.OrganizationPartyId);

                await ProcessDisableUserProductDataAsync(
                    persona.PersonaId,
                    createUserRealPageId,
                    createUserPersonaId,
                    persona.UserTypeId,
                    impersonatorUserId,
                    cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disabling products for user {UserRealPageId}", user.RealPageId);
            throw;
        }
    }

    private async Task ProcessDisableUserProductDataAsync(
        long assignUserPersonaId,
        Guid createUserRealPageId,
        long createUserPersonaId,
        int? userTypeId,
        long impersonatorUserId,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug("Processing disable for PersonaId {PersonaId}", assignUserPersonaId);

        await using var repo = _repositoryAsync;

        var batchGroup = await CreateBatchProcessGroupAsync(cancellationToken);

        // Get list of products to remove
        var productsToRemove = await GetListOfProductsToRemoveByPersonaIdAsync(
            repo,
            assignUserPersonaId,
            cancellationToken);

        if (userTypeId != (int)UserRoleType.UserNoEmail)
        {
            // Add SalesForce to disable list
            productsToRemove.Add(new ProductBatch
            {
                ProductId = (int)ProductEnum.SalesForce,
                StatusTypeId = 5,
                RetryCount = 0,
                BatchProcessorGroupId = batchGroup.BatchProcessorGroupId,
                InputJson = new RolePropertyList
                {
                    PropertyList = new List<string>(),
                    RoleList = new List<string>(),
                    IsAssigned = false
                }
            });
        }

        // Bundle AO products if any
        var aoInputJson = BundleAoProducts(productsToRemove, batchGroup.BatchProcessorGroupId);

        // Save each product batch
        foreach (var product in productsToRemove)
        {
            if (product.ProductId == (int)ProductEnum.UnifiedPlatform)
                continue;

            var inputJson = product.ProductId == (int)ProductEnum.AssetOptimizer
                ? aoInputJson
                : JsonConvert.SerializeObject(product.InputJson);

            await SaveProductBatchAsync(
                product,
                createUserPersonaId,
                assignUserPersonaId,
                createUserRealPageId,
                inputJson,
                (int)BatchProcessType.CreateUpdateProductUser,
                cancellationToken);
        }
    }

    private async Task<IList<ProductBatch>> GetListOfProductsToRemoveByPersonaIdAsync(
        IRepositoryAsync repo,
        long assignUserPersonaId,
        CancellationToken cancellationToken)
    {
        var userProducts = await repo.GetManyAsync<PersonaProductUserDetails>(
            StoredProcNameConstants.SP_ListProductsByPersonaId,
            new
            {
                PersonaId = assignUserPersonaId,
                ProductStatusValue = ((int)UserUiStatusType.AccountCreationSuccessful).ToString()
            },
            cancellationToken);

        var productsToRemove = new List<ProductBatch>();

        foreach (var prod in userProducts.Where(p => p.ProductStatus == (int)ProductBatchStatusType.Success))
        {
            productsToRemove.Add(new ProductBatch
            {
                ProductId = prod.ProductId,
                StatusTypeId = 5,
                RetryCount = 0,
                InputJson = new RolePropertyList
                {
                    PropertyList = new List<string>(),
                    RoleList = new List<string>(),
                    IsAssigned = false
                }
            });
        }

        return productsToRemove;
    }

    private async Task ProcessActivatedUserProductBatchDataAsync(
        long personaId,
        Guid createUserRealPageId,
        long createUserPersonaId,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug("Processing activated products for PersonaId {PersonaId}", personaId);

        await using var repo = _repositoryAsync;

        var userProducts = await repo.GetManyAsync<PersonaProductUserDetails>(
            StoredProcNameConstants.SP_ListProductsByPersonaId,
            new
            {
                PersonaId = personaId,
                ProductStatusValue = ((int)UserUiStatusType.Deactivated).ToString()
            },
            cancellationToken);

        if (!userProducts.Any())
            return;

        var batchGroup = await CreateBatchProcessGroupAsync(cancellationToken);
        var productBatchList = await GetActivatedUserProductBatchDataAsync(
            repo,
            personaId,
            cancellationToken);

        var aoInputJson = BundleAoProducts(productBatchList.ToList(), batchGroup.BatchProcessorGroupId);
        var impersonatorUserId = await GetImpersonatorUserIdAsync(cancellationToken);

        foreach (var product in productBatchList)
        {
            if (product.ProductId == (int)ProductEnum.UnifiedPlatform)
                continue;

            var inputJson = product.ProductId == (int)ProductEnum.AssetOptimizer
                ? aoInputJson
                : JsonConvert.SerializeObject(product.InputJson);

            product.BatchProcessorGroupId = batchGroup.BatchProcessorGroupId;

            await SaveProductBatchAsync(
                product,
                createUserPersonaId,
                personaId,
                createUserRealPageId,
                inputJson,
                (int)BatchProcessType.CreateUpdateProductUser,
                cancellationToken);
        }
    }

    private async Task<IList<ProductBatch>> GetActivatedUserProductBatchDataAsync(
        IRepositoryAsync repo,
        long personaId,
        CancellationToken cancellationToken)
    {
        var productBatchList = new List<ProductBatch>();

        var userProducts = await repo.GetManyAsync<PersonaProductUserDetails>(
            StoredProcNameConstants.SP_ListProductsByPersonaId,
            new
            {
                PersonaId = personaId,
                ProductStatusValue = ((int)UserUiStatusType.Deactivated).ToString()
            },
            cancellationToken);

        foreach (var product in userProducts.Where(p => !ProductEnumHelper.GetAoProductList().Contains((ProductEnum)p.ProductId)))
        {
            var productJson = await repo.GetOneAsync<string>(
                StoredProcNameConstants.SP_GetUserProductBatchJsonData,
                new { ProductId = product.ProductId, PersonaId = personaId },
                cancellationToken);

            if (!string.IsNullOrEmpty(productJson))
            {
                productBatchList.Add(new ProductBatch
                {
                    InputJson = JsonConvert.DeserializeObject<RolePropertyList>(productJson.Trim()),
                    ProductId = product.ProductId,
                    StatusTypeId = 5,
                    RetryCount = 0
                });
            }
        }

        return productBatchList;
    }

    private async Task<long> GetImpersonatorUserIdAsync(CancellationToken cancellationToken)
    {
        if (_userClaim.ImpersonatedBy == Guid.Empty)
            return 0;

        var userLogin = _userLoginRepository.GetUserLoginOnly(_userClaim.ImpersonatedBy);
        return userLogin?.UserId ?? 0;
    }

    #endregion
}