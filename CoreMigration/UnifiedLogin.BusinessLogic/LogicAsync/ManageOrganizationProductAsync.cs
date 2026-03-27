using Microsoft.Extensions.Logging;
using UnifiedLogin.BusinessLogic.Logic.Helper;
using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Audit.Common;
using UnifiedLogin.SharedObjects.BlackBook;
using UnifiedLogin.SharedObjects.Constants;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Extensions;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product;

namespace UnifiedLogin.BusinessLogic.LogicAsync;

/// <summary>
/// Async-first implementation of <see cref="IManageOrganizationProductAsync"/>.
/// Replaces the <c>Task.FromResult</c> stepping-stone. All DB and HTTP calls are
/// awaited; claims are accessed via <see cref="IUserClaimsAccessor"/>; Serilog replaced
/// by <see cref="ILogger{TCategoryName}"/>.
/// </summary>
public sealed class ManageOrganizationProductAsync : IManageOrganizationProductAsync
{
    #region Fields

    private readonly IOrganizationProductRepositoryAsync     _orgProductRepository;
    private readonly IManageBlueBookAsync                    _manageBlueBook;
    private readonly IManageProductAsync                     _manageProduct;
    private readonly IProductInternalSettingRepositoryAsync  _productInternalSettingRepository;
    private readonly IUserClaimsAccessor                     _userClaimsAccessor;
    private readonly ILogger<ManageOrganizationProductAsync> _logger;

    #endregion

    #region Constructor

    public ManageOrganizationProductAsync(
        IOrganizationProductRepositoryAsync     orgProductRepository,
        IManageBlueBookAsync                    manageBlueBook,
        IManageProductAsync                     manageProduct,
        IProductInternalSettingRepositoryAsync  productInternalSettingRepository,
        IUserClaimsAccessor                     userClaimsAccessor,
        ILogger<ManageOrganizationProductAsync> logger)
    {
        _orgProductRepository           = orgProductRepository           ?? throw new ArgumentNullException(nameof(orgProductRepository));
        _manageBlueBook                 = manageBlueBook                 ?? throw new ArgumentNullException(nameof(manageBlueBook));
        _manageProduct                  = manageProduct                  ?? throw new ArgumentNullException(nameof(manageProduct));
        _productInternalSettingRepository = productInternalSettingRepository ?? throw new ArgumentNullException(nameof(productInternalSettingRepository));
        _userClaimsAccessor             = userClaimsAccessor             ?? throw new ArgumentNullException(nameof(userClaimsAccessor));
        _logger                         = logger                         ?? throw new ArgumentNullException(nameof(logger));
    }

    #endregion

    #region Validation

    /// <inheritdoc/>
    public async Task<RepositoryResponse> CheckSharedProductsEnabledAsync(
        IList<ProductUI> orgEnabledProductList,
        List<int> addProductList,
        List<int> removeProductList,
        CancellationToken cancellationToken = default)
    {
        var response      = new RepositoryResponse();
        var errorProducts = new List<string>();

        var sharedRules = await _productInternalSettingRepository
            .GetProductSettingByTypeAsync("PreventEnablingThisProductID", cancellationToken);

        foreach (int productId in addProductList)
        {
            var rule = sharedRules.FirstOrDefault(m => m.ProductId == productId);

            if (rule is not null
                && orgEnabledProductList.Any(m => m.ProductId == Convert.ToInt32(rule.Value))
                && !removeProductList.Any(m => m == Convert.ToInt32(rule.Value)))
            {
                errorProducts.Add(rule.ProductName);
            }
        }

        if (errorProducts.Count > 0)
            response.ErrorMessage = "Unable to enable products : " + string.Join(",", errorProducts);

        return response;
    }

    #endregion

    #region Insert / Update

    /// <inheritdoc/>
    public async Task<IRepositoryResponse> InsertUpdateOrganizationProductAsync(
        Organization org,
        List<int> productList,
        CancellationToken cancellationToken = default)
    {
        var response     = new RepositoryResponse();
        var responseList = new List<KeyValuePair<int, RepositoryResponse>>();

        foreach (int product in productList.Distinct())
        {
            var settings    = await _manageProduct.GetProductInternalSettingsAsync(product, cancellationToken);
            var updateInUdm = settings.FirstOrDefault(
                x => x.Name.Equals("UPDATEPRODUCTINUDM", StringComparison.OrdinalIgnoreCase));

            if (updateInUdm?.Value == "1")
            {
                bool ok = await _manageBlueBook.ProductCenterEnableAsync(
                    BuildSpc(id: 0, org.RealPageId, product), cancellationToken);

                if (!ok)
                {
                    response.ErrorMessage = "Unable to update product in UDM";
                    return response;
                }
            }

            response = await InsertUpdateOrganizationProductAsync(
                org.PartyId, product, null, null, null, org.Name, cancellationToken);
            responseList.Add(new(product, response));
        }

        if (responseList.Count > 0)
            await LogBulkProductActivityAsync(responseList, org, isEnable: true, cancellationToken);

        return response;
    }

    /// <inheritdoc/>
    public async Task<RepositoryResponse> InsertUpdateOrganizationProductAsync(
        long partyId,
        int product,
        int? configurationId,
        DateTime? fromDate,
        DateTime? thruDate,
        string orgName,
        CancellationToken cancellationToken = default)
        => await _orgProductRepository.InsertUpdateOrganizationProductAsync(
            partyId, product, configurationId, fromDate, thruDate, cancellationToken);

    /// <inheritdoc/>
    public async Task<IRepositoryResponse> InsertUpdateOrganizationProductFromProvisioningAsync(
        int product,
        int? configurationId,
        DateTime? fromDate,
        DateTime? thruDate,
        Organization org,
        CancellationToken cancellationToken = default)
    {
        var response = await _orgProductRepository.InsertUpdateOrganizationProductAsync(
            org.PartyId, product, configurationId, fromDate, thruDate, cancellationToken);

        if (response.ErrorMessage.Length == 0)
        {
            var allProducts = await _manageProduct.ListProductsAsync(cancellationToken);
            var name        = allProducts.FirstOrDefault(p => p.ProductId == product)?.Name;
            LogAuditActivity(
                LogActivityTypeConstants.PRODUCT_ENABLED_FOR_COMPANY,
                LogActivityCategoryType.CompanySetup,
                $"{_userClaimsAccessor.FirstName} {_userClaimsAccessor.LastName} enabled {name} for {org.Name}");
        }

        return response;
    }

    #endregion

    #region Delete

    /// <inheritdoc/>
    public async Task<RepositoryResponse> DeleteOrganizationProductAsync(
        long partyId,
        int product,
        Organization org,
        bool logActivity = true,
        CancellationToken cancellationToken = default)
    {
        var response = await _orgProductRepository.DeleteOrganizationProductAsync(partyId, product, cancellationToken);

        if (response.ErrorMessage.Length == 0 && logActivity)
        {
            var allProducts = await _manageProduct.ListProductsAsync(cancellationToken);
            var name        = allProducts.FirstOrDefault(p => p.ProductId == product)?.Name;
            LogAuditActivity(
                LogActivityTypeConstants.PRODUCT_DISABLED_FOR_COMPANY,
                LogActivityCategoryType.CompanySetup,
                $"{_userClaimsAccessor.FirstName} {_userClaimsAccessor.LastName} disabled {name} for {org.Name}");
        }

        return response;
    }

    /// <inheritdoc/>
    public async Task<RepositoryResponse> DeleteProductsFromOrganizationAsync(
        List<int> unassignProductList,
        Organization org,
        CancellationToken cancellationToken = default)
    {
        var response     = new RepositoryResponse();
        var responseList = new List<KeyValuePair<int, RepositoryResponse>>();

        foreach (int product in unassignProductList)
        {
            var settings    = await _manageProduct.GetProductInternalSettingsAsync(product, cancellationToken);
            var updateInUdm = settings.FirstOrDefault(
                x => x.Name.Equals("UPDATEPRODUCTINUDM", StringComparison.OrdinalIgnoreCase));

            if (updateInUdm?.Value == "1")
            {
                bool ok = await _manageBlueBook.ProductCenterDisableAsync(
                    BuildSpc(id: org.PartyId, org.RealPageId, product), cancellationToken);

                if (!ok)
                {
                    response.ErrorMessage = "Unable to delete product in UDM";
                    return response;
                }
            }

            response = await DeleteOrganizationProductAsync(
                org.PartyId, product, org, logActivity: false, cancellationToken);
            responseList.Add(new(product, response));
        }

        if (responseList.Count > 0)
            await LogBulkProductActivityAsync(responseList, org, isEnable: false, cancellationToken);

        return response;
    }

    #endregion

    #region Users

    /// <inheritdoc/>
    public async Task<RepositoryResponse> DisableUsersForProductAsync(
        long partyId,
        ProductEnum product,
        CancellationToken cancellationToken = default)
        => await _orgProductRepository.DisableUsersForProductAsync(partyId, product, cancellationToken);

    #endregion

    #region Private Helpers

    private async Task LogBulkProductActivityAsync(
        List<KeyValuePair<int, RepositoryResponse>> responseList,
        Organization org,
        bool isEnable,
        CancellationToken cancellationToken)
    {
        var allProducts   = await _manageProduct.ListProductsAsync(cancellationToken);
        var succeeded     = new List<string>();
        var failed        = new List<string>();
        var extraParams   = new List<AdditionalParameters>();

        foreach (var (productId, result) in responseList)
        {
            var name = allProducts.FirstOrDefault(p => p.ProductId == productId)?.Name ?? productId.ToString();
            if (string.IsNullOrEmpty(result.ErrorMessage)) succeeded.Add(name);
            else                                           failed.Add(name);
        }

        string verb    = isEnable ? "changed" : "Deleted";
        string message = $"{_userClaimsAccessor.FirstName} {_userClaimsAccessor.LastName} {verb} products for {org.Name}";

        if (succeeded.Count > 0)
            extraParams.Add(new AdditionalParameters
            {
                Key   = isEnable ? "EnabledProducts" : "DeletedProducts",
                Value = $"{{ \"old\": \"\", \"new\": \"{string.Join(", ", succeeded)}\" }}"
            });

        if (failed.Count > 0)
            extraParams.Add(new AdditionalParameters
            {
                Key   = "FailedProducts",
                Value = $"{{ \"old\": \"\", \"new\": \"{string.Join(", ", failed)}\" }}"
            });

        LogAuditActivity(
            LogActivityTypeConstants.PRODUCT_ENABLED_FOR_COMPANY,
            LogActivityCategoryType.CompanySetup,
            message,
            extraParams);
    }

    private static SystemProductCenter BuildSpc(long id, Guid orgRealPageId, int productId)
        => new()
        {
            Id                       = id,
            CompanyInstanceSourceId  = orgRealPageId.ToString().ToLower(),
            CreatedBy                = ProductEnumHelper.StringValueOf(ProductEnum.UnifiedPlatform) + " Automation",
            ProductCenterSourceId    = productId.ToString(),
            PropertyInstanceSourceId = null,
            Source                   = ProductEnumHelper.StringValueOf(ProductEnum.UnifiedPlatform)
        };

    private void LogAuditActivity(
        string activityType,
        LogActivityCategoryType categoryType,
        string message,
        List<AdditionalParameters>? additionalParameters = null)
    {
        try
        {
            LogActivity.WriteActivity(new ActivityDetails
            {
                LogActivityTypeName       = activityType,
                LogCategoryName           = categoryType.ToString(),
                CorrelationId             = _userClaimsAccessor.CorrelationId.ToString(),
                BooksMasterOrganizationId = _userClaimsAccessor.OrganizationMasterId,
                OrganizationPartyId       = _userClaimsAccessor.OrganizationPartyId,
                Message                   = message,
                FromUserLoginName         = _userClaimsAccessor.LoginName,
                FromUserLoginId           = _userClaimsAccessor.UserId,
                FromUserRealpageId        = _userClaimsAccessor.UserRealPageGuid.ToString(),
                FromUserFirstName         = _userClaimsAccessor.FirstName,
                FromUserLastName          = _userClaimsAccessor.LastName,
                AdditionalInformation     = additionalParameters
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Audit log failed for org {OrgName} user {LoginName}",
                _userClaimsAccessor.OrganizationName,
                _userClaimsAccessor.LoginName);
        }
    }

    #endregion
}
