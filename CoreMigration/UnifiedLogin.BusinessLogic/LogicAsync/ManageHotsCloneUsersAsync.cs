using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;
using UnifiedLogin.BusinessLogic.Logic.Enterprise.Helpers;
using UnifiedLogin.BusinessLogic.Logic.ProductIntegration.Helpers;
using UnifiedLogin.BusinessLogic.Logic.ProductIntegration.Model;
using UnifiedLogin.BusinessLogic.Logic.ProductIntegration.Model.ClickPay;
using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.BlackBook;
using UnifiedLogin.SharedObjects.Extensions;
using UnifiedLogin.SharedObjects.Hots;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product;
using UnifiedLogin.SharedObjects.Product.Accounting;
using UnifiedLogin.SharedObjects.Product.Onsite;
using UnifiedLogin.SharedObjects.Product.Ops;
using UnifiedLogin.SharedObjects.Product.ResidentPortal;
using UnifiedLogin.SharedObjects.Product.Rum;
using ProductRole = UnifiedLogin.SharedObjects.Product.ProductRole;

namespace UnifiedLogin.BusinessLogic.LogicAsync;

public sealed class ManageHotsCloneUsersAsync : IManageHotsCloneUsersAsync
{
    private readonly IHOTSCloneUserRepositoryAsync          _hotsRepo;
    private readonly ISamlRepositoryAsync                   _samlRepo;
    private readonly IProductInternalSettingRepositoryAsync _productInternalSettingRepo;
    private readonly IManageCloneProductBatchAsync          _cloneProductBatch;
    private readonly IManageProductPanelAsync               _productPanel;
    private readonly IPersonaRepositoryAsync                _personaRepo;
    private readonly IManageProfileAsync                    _profileService;
    private readonly ITokenHelperAsync                      _tokenHelper;
    private readonly IHttpClientFactory                     _httpClientFactory;
    private readonly IUserClaimsAccessor                    _userClaims;
    private readonly ILogger<ManageHotsCloneUsersAsync>     _logger;

    public ManageHotsCloneUsersAsync(
        IHOTSCloneUserRepositoryAsync          hotsRepo,
        ISamlRepositoryAsync                   samlRepo,
        IProductInternalSettingRepositoryAsync productInternalSettingRepo,
        IManageCloneProductBatchAsync          cloneProductBatch,
        IManageProductPanelAsync               productPanel,
        IPersonaRepositoryAsync                personaRepo,
        IManageProfileAsync                    profileService,
        ITokenHelperAsync                      tokenHelper,
        IHttpClientFactory                     httpClientFactory,
        IUserClaimsAccessor                    userClaims,
        ILogger<ManageHotsCloneUsersAsync>     logger)
    {
        _hotsRepo                   = hotsRepo                   ?? throw new ArgumentNullException(nameof(hotsRepo));
        _samlRepo                   = samlRepo                   ?? throw new ArgumentNullException(nameof(samlRepo));
        _productInternalSettingRepo = productInternalSettingRepo ?? throw new ArgumentNullException(nameof(productInternalSettingRepo));
        _cloneProductBatch          = cloneProductBatch          ?? throw new ArgumentNullException(nameof(cloneProductBatch));
        _productPanel               = productPanel               ?? throw new ArgumentNullException(nameof(productPanel));
        _personaRepo                = personaRepo                ?? throw new ArgumentNullException(nameof(personaRepo));
        _profileService             = profileService             ?? throw new ArgumentNullException(nameof(profileService));
        _tokenHelper                = tokenHelper                ?? throw new ArgumentNullException(nameof(tokenHelper));
        _httpClientFactory          = httpClientFactory          ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _userClaims                 = userClaims                 ?? throw new ArgumentNullException(nameof(userClaims));
        _logger                     = logger                     ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ClonedUsers> CloneUsersFromBaseLineCompanyAsync(
        CloneUsers cloneUsers,
        long basePartyId,
        long clonePartyId,
        long baseOrgAdminPersonaId,
        CancellationToken cancellationToken = default)
    {
        var clonedUsers = new ClonedUsers
        {
            Status                   = "Incomplete",
            CloneCustomerCompanyId   = cloneUsers.CloneCustomerUPFMId,
            CloneCustomerEnvironment = cloneUsers.CloneCustomerEnvironment,
            Users                    = []
        };

        var productInternalSettings = await _productInternalSettingRepo
            .GetProductInternalSettingsAsync(3, cancellationToken)
            .ConfigureAwait(false);

        try
        {
            bool isEnabled = productInternalSettings
                .FirstOrDefault(s => s.Name.Equals("IsCloneUsersProcessEnabledForHOTS", StringComparison.OrdinalIgnoreCase))
                ?.Value == "1";

            if (basePartyId <= 0 || clonePartyId <= 0 || baseOrgAdminPersonaId <= 0 || !isEnabled)
                return clonedUsers;

            var usersToBeCloned = await _hotsRepo
                .ListUsersAsync(basePartyId, cancellationToken)
                .ConfigureAwait(false);

            var upfmProperty = new UPFMProperty();

            foreach (var user in usersToBeCloned)
            {
                var profileDetail = await GetUserProfileAsync(user, basePartyId, cancellationToken)
                    .ConfigureAwait(false);

                if (await CheckIfUserAlreadyExistsAsync(clonePartyId, profileDetail, clonedUsers, cancellationToken)
                    .ConfigureAwait(false))
                    continue;

                // Fetch user products and persona settings in parallel
                var userProductsTask        = _hotsRepo.GetUserProductsAsync(user.PersonaId, cancellationToken);
                var personaProductSettingsTask = _personaRepo.GetPersonaProductSettingsAsync(user.PersonaId, cancellationToken);
                await Task.WhenAll(userProductsTask, personaProductSettingsTask).ConfigureAwait(false);

                var userProducts           = await userProductsTask;
                var personaProductSettings = await personaProductSettingsTask;

                var pbData = (await _cloneProductBatch.GetUserProductBatchDataAsync(
                    user.PersonaId, userProducts, baseOrgAdminPersonaId,
                    upfmProperty, personaProductSettings, false,
                    cancellationToken).ConfigureAwait(false)).ToList();

                foreach (var productData in pbData)
                {
                    var propertyList = productData.InputJson.PropertyList;
                    var roleList     = productData.InputJson.RoleList;

                    // Fetch properties and roles for base+clone in parallel when needed
                    Task<ListResponse> basePropsTask  = propertyList?.Count > 0
                        ? _productPanel.GetProductPropertiesAsync(user.AdminUserPersonaId, user.PersonaId, productData.ProductId, null!, cancellationToken)
                        : Task.FromResult(new ListResponse());
                    Task<ListResponse> clonePropsTask = propertyList?.Count > 0
                        ? _productPanel.GetProductPropertiesAsync(_userClaims.PersonaId, 0, productData.ProductId, null!, cancellationToken)
                        : Task.FromResult(new ListResponse());
                    Task<ListResponse> baseRolesTask  = roleList?.Count > 0
                        ? _productPanel.GetProductRolesAsync(user.AdminUserPersonaId, user.PersonaId, basePartyId, productData.ProductId, null!, null, cancellationToken)
                        : Task.FromResult(new ListResponse());
                    Task<ListResponse> cloneRolesTask = roleList?.Count > 0
                        ? _productPanel.GetProductRolesAsync(_userClaims.PersonaId, 0, clonePartyId, productData.ProductId, null!, null, cancellationToken)
                        : Task.FromResult(new ListResponse());

                    await Task.WhenAll(basePropsTask, clonePropsTask, baseRolesTask, cloneRolesTask)
                        .ConfigureAwait(false);

                    if (propertyList?.Count > 0)
                    {
                        var matched = CompareBaseAndCloneProductProperties(
                            await basePropsTask, await clonePropsTask);
                        if (matched.Count > 0)
                            productData.InputJson.PropertyList = matched;
                    }

                    if (roleList?.Count > 0)
                    {
                        var matched = CompareBaseAndCloneProductRoles(
                            await baseRolesTask, await cloneRolesTask);
                        if (matched.Count > 0)
                            productData.InputJson.RoleList = matched;
                    }
                }

                var userLogin       = new UserLogin { Password = PasswordGenerator.GeneratePassword(10, 1) };
                var passwordDetail  = userLogin.Password.PasswordHash();
                userLogin.PasswordHash = passwordDetail.PasswordHash;
                userLogin.PasswordSalt = passwordDetail.PasswordSalt;

                var hotsUser = await _hotsRepo
                    .CreateUserAsync(_userClaims.UserClaim, clonePartyId, user, profileDetail, pbData, userLogin, cancellationToken)
                    .ConfigureAwait(false);

                if (hotsUser is not null)
                {
                    pbData.ForEach(pb => hotsUser.CloneProducts.Add(pb.ProductId));
                    clonedUsers.Users.Add(hotsUser);
                }
            }

            clonedUsers.Status = "Complete";
            await CheckUsersProductStatusAsync(clonedUsers, productInternalSettings, cancellationToken)
                .ConfigureAwait(false);
            await PostToHotsAsync(clonedUsers, productInternalSettings, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "{CorrelationId} CloneUsersFromBaseLineCompany failed ClonePartyId={ClonePartyId} BasePartyId={BasePartyId}",
                _userClaims.CorrelationId, clonePartyId, basePartyId);
        }

        return clonedUsers;
    }

    public Task<Guid> GetBaseCompanyUPFMIdAsync(Guid cloneUpfmId, CancellationToken cancellationToken = default)
        => _hotsRepo.GetBaseCompanyUPFMIdAsync(cloneUpfmId, cancellationToken);

    public Task<RepositoryResponse> InsertHotsCompanyRelationshipAsync(
        Guid baselineCompanyRealPageId, Guid cloneCompanyRealPageId, int userId,
        CancellationToken cancellationToken = default)
        => _hotsRepo.InsertHotsCompanyRelationshipAsync(baselineCompanyRealPageId, cloneCompanyRealPageId, userId, cancellationToken);

    public Task<RepositoryResponse> InsertHotsPropertyRelationshipAsync(
        Guid baselinePropertyInstanceId, Guid clonePropertyInstanceId, Guid cloneCompanyRealPageId, int userId,
        CancellationToken cancellationToken = default)
        => _hotsRepo.InsertHotsPropertyRelationshipAsync(baselinePropertyInstanceId, clonePropertyInstanceId, cloneCompanyRealPageId, userId, cancellationToken);

    // ── Private async helpers ──────────────────────────────────────────────

    private async Task<bool> CheckIfUserAlreadyExistsAsync(
        long clonePartyId, IProfileDetail profileDetail, ClonedUsers clonedUsers,
        CancellationToken ct)
    {
        var cloneLoginName = GetLoginName(clonePartyId, profileDetail);
        var userLoginOnly  = await _hotsRepo.GetUserLoginOnlyAsync(cloneLoginName, ct).ConfigureAwait(false);

        if (userLoginOnly is null) return false;

        var existingUser = new HotsUser
        {
            BaselineUserId   = profileDetail.userLogin.UserId,
            BaselineUserName = profileDetail.userLogin.LoginName,
            CloneUserId      = userLoginOnly.UserId,
            CloneUserName    = userLoginOnly.LoginName
        };

        var personaList = await _hotsRepo.ListPersonaAsync(userLoginOnly.RealPageId, ct).ConfigureAwait(false);
        if (personaList is not null)
            existingUser.ClonePersonaId = personaList.First(p => p.OrganizationPartyId == clonePartyId).PersonaId;

        clonedUsers.Users.Add(existingUser);
        return true;
    }

    private async Task CheckUsersProductStatusAsync(
        ClonedUsers clonedUsers,
        IList<ProductInternalSetting> settings,
        CancellationToken ct)
    {
        int retry            = int.TryParse(settings.FirstOrDefault(s => s.Name.Equals("HOTSCheckUserProductStatusRetryCount",   StringComparison.OrdinalIgnoreCase))?.Value, out int r) ? r : 5;
        int statusCheckDelay = int.TryParse(settings.FirstOrDefault(s => s.Name.Equals("HOTSCheckUserProductStatusSleepTimeout", StringComparison.OrdinalIgnoreCase))?.Value, out int d) ? d : 2000;

        var excludedProductIds = new HashSet<int>(
            (settings.FirstOrDefault(s => s.Name.Equals("HOTSCheckUserExcludeProductIds", StringComparison.OrdinalIgnoreCase))
                ?.Value ?? string.Empty)
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(s => int.TryParse(s.Trim(), out int id) ? id : -1)
            .Where(id => id >= 0));

        // Remove excluded products upfront
        foreach (var user in clonedUsers.Users)
            user.CloneProducts.RemoveAll(excludedProductIds.Contains);

        int productsToValidate = clonedUsers.Users.Sum(u => u.CloneProducts.Count);

        while (productsToValidate > 0 && retry > 0)
        {
            await Task.Delay(statusCheckDelay, ct).ConfigureAwait(false);

            foreach (var user in clonedUsers.Users.Where(u => u.CloneProducts.Count > 0))
            {
                var statusList = await _samlRepo
                    .ListAllProductsByPersonaIdAsync(user.ClonePersonaId, 0, "", ct)
                    .ConfigureAwait(false);

                foreach (var productId in user.CloneProducts.ToArray())
                {
                    if (statusList.Any(p => p.ProductId == productId && p.ProductStatus == 8))
                    {
                        user.CloneProducts.Remove(productId);
                        productsToValidate--;
                    }
                }
            }

            retry--;
        }

        if (productsToValidate > 0)
            clonedUsers.Status = "Incomplete";
    }

    private async Task PostToHotsAsync(
        ClonedUsers clonedUsers,
        IList<ProductInternalSetting> settings,
        CancellationToken ct)
    {
        try
        {
            var hotsEndpoint    = settings.FirstOrDefault(s => s.Name.Equals("HOTSCloneUserCallBackEnpoint",  StringComparison.OrdinalIgnoreCase))?.Value;
            var hotsIssuerUri   = settings.FirstOrDefault(s => s.Name.Equals("HOTSCloneIssuerUri",           StringComparison.OrdinalIgnoreCase))?.Value;
            var hotsClientId    = settings.FirstOrDefault(s => s.Name.Equals("HOTSCloneClientId",            StringComparison.OrdinalIgnoreCase))?.Value;
            var hotsClientSecret = settings.FirstOrDefault(s => s.Name.Equals("HOTSCloneClientSecret",       StringComparison.OrdinalIgnoreCase))?.Value;

            if (!string.IsNullOrEmpty(hotsClientSecret))
                hotsClientSecret = Encoding.UTF8.GetString(Convert.FromBase64String(hotsClientSecret));

            if (string.IsNullOrEmpty(hotsClientId) || string.IsNullOrEmpty(hotsClientSecret))
            {
                _logger.LogError("{CorrelationId} PostToHots: missing client credentials — callback skipped",
                    _userClaims.CorrelationId);
                return;
            }

            var token = await _tokenHelper
                .GetExternalClientCredentialServerTokenAsync(
                    $"{hotsIssuerUri}/connect/token", hotsClientId, hotsClientSecret, "hotsapi", ct)
                .ConfigureAwait(false);

            if (token is null)
            {
                _logger.LogError("{CorrelationId} PostToHots: token acquisition failed — callback skipped",
                    _userClaims.CorrelationId);
                return;
            }

            using var httpClient = _httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            httpClient.BaseAddress = new Uri(hotsEndpoint!);

            var payload = JsonConvert.SerializeObject(clonedUsers);
            var request = new HttpRequestMessage(HttpMethod.Post, httpClient.BaseAddress)
            {
                Content = new StringContent(payload, Encoding.UTF8, "application/json")
            };

            _logger.LogInformation("{CorrelationId} PostToHots posting {UserCount} users",
                _userClaims.CorrelationId, clonedUsers.Users?.Count);

            var response = await httpClient.SendAsync(request, ct).ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
                _logger.LogInformation("{CorrelationId} PostToHots succeeded", _userClaims.CorrelationId);
            else
                _logger.LogWarning("{CorrelationId} PostToHots failed StatusCode={StatusCode}",
                    _userClaims.CorrelationId, response.StatusCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{CorrelationId} PostToHots exception", _userClaims.CorrelationId);
        }
    }

    private async Task<IProfileDetail> GetUserProfileAsync(
        BaseLineCustomerCompanyUser user, long partyId, CancellationToken ct)
        => await _profileService
            .GetProfileDetailAsync(user.UserRealPageId, partyId, cancellationToken: ct)
            .ConfigureAwait(false);

    // ── Private static helpers ─────────────────────────────────────────────

    private static string GetLoginName(long partyId, IProfileDetail profile)
        => string.Concat(profile.FirstName, profile.LastName, partyId, "@realpage.com");

    private static List<string> CompareBaseAndCloneProductProperties(
        ListResponse baseProductProperties, ListResponse cloneProductProperties)
    {
        List<string> matched = [];
        if (baseProductProperties.Records?.Count is not > 0 || cloneProductProperties.Records?.Count is not > 0)
            return matched;

        var baseType  = baseProductProperties.Records[0].GetType();
        var cloneType = cloneProductProperties.Records[0].GetType();

        if (baseType == typeof(ProductProperty) && cloneType == typeof(ProductProperty))
        {
            var baseList  = baseProductProperties.Records.Cast<ProductProperty>();
            var cloneList = cloneProductProperties.Records.Cast<ProductProperty>().ToList();
            foreach (var p in baseList.Where(p => p.IsAssigned == true))
            {
                var found = cloneList.FirstOrDefault(c => c.Name.Contains(p.Name, StringComparison.OrdinalIgnoreCase));
                if (found is not null) matched.Add(found.ID);
            }
        }
        else if (baseType == typeof(ACProperty) && cloneType == typeof(ACProperty))
        {
            var baseList  = baseProductProperties.Records.Cast<ACProperty>();
            var cloneList = cloneProductProperties.Records.Cast<ACProperty>().ToList();
            foreach (var p in baseList.Where(p => p.IsAssigned))
            {
                var found = cloneList.FirstOrDefault(c => c.PropertyName.Contains(p.PropertyName, StringComparison.OrdinalIgnoreCase));
                if (found is not null) matched.Add(found.PropertyId);
            }
        }
        else if (baseType == typeof(AssetGroup) && cloneType == typeof(AssetGroup))
        {
            var baseList  = baseProductProperties.Records.Cast<AssetGroup>();
            var cloneList = cloneProductProperties.Records.Cast<AssetGroup>().ToList();
            foreach (var p in baseList.Where(p => p.IsAssigned))
            {
                var found = cloneList.FirstOrDefault(c => c.Name.Contains(p.Name, StringComparison.OrdinalIgnoreCase));
                if (found is not null) matched.Add(found.ID);
            }
        }
        else if (baseType == typeof(OnSiteProperty) && cloneType == typeof(OnSiteProperty))
        {
            var baseList  = baseProductProperties.Records.Cast<AssetGroup>();
            var cloneList = cloneProductProperties.Records.Cast<AssetGroup>().ToList();
            foreach (var p in baseList.Where(p => p.IsAssigned))
            {
                var found = cloneList.FirstOrDefault(c => c.Name.Contains(p.Name, StringComparison.OrdinalIgnoreCase));
                if (found is not null) matched.Add(found.ID);
            }
        }
        else if (baseType == typeof(RumPropertyGroup) && cloneType == typeof(RumPropertyGroup))
        {
            var baseList  = baseProductProperties.Records.Cast<RumPropertyGroup>();
            var cloneList = cloneProductProperties.Records.Cast<RumPropertyGroup>().ToList();
            foreach (var p in baseList.Where(p => p.IsAssigned))
            {
                var found = cloneList.FirstOrDefault(c => c.Name.Contains(p.Name, StringComparison.OrdinalIgnoreCase));
                if (found is not null) matched.Add(found.Id.ToString());
            }
        }
        else if (baseType == typeof(ProductProperties) && cloneType == typeof(ProductProperties))
        {
            var baseList  = baseProductProperties.Records.Cast<ProductProperties>();
            var cloneList = cloneProductProperties.Records.Cast<ProductProperties>().ToList();
            foreach (var p in baseList.Where(p => p.IsAssigned))
            {
                var found = cloneList.FirstOrDefault(c => c.GetName.Contains(p.GetName, StringComparison.OrdinalIgnoreCase));
                if (found is not null) matched.Add(found.GetPropertyId);
            }
        }
        else if (baseType == typeof(Portfolio) && cloneType == typeof(Portfolio))
        {
            var baseList  = baseProductProperties.Records.Cast<Portfolio>();
            var cloneList = cloneProductProperties.Records.Cast<Portfolio>().ToList();
            foreach (var p in baseList.Where(p => p.IsAssigned))
            {
                var found = cloneList.FirstOrDefault(c => c.Name.Contains(p.Name, StringComparison.OrdinalIgnoreCase));
                if (found is not null) matched.Add(found.ID);
            }
        }

        return matched;
    }

    private static List<string> CompareBaseAndCloneProductRoles(
        ListResponse baseCompanyRoles, ListResponse cloneCompanyRoles)
    {
        List<string> matched = [];
        if (baseCompanyRoles.Records?.Count is not > 0 || cloneCompanyRoles.Records?.Count is not > 0)
            return matched;

        var baseType  = baseCompanyRoles.Records[0].GetType();
        var cloneType = cloneCompanyRoles.Records[0].GetType();

        if (baseType == typeof(ProductRole) && cloneType == typeof(ProductRole))
        {
            var baseList  = baseCompanyRoles.Records.Cast<ProductRole>();
            var cloneList = cloneCompanyRoles.Records.Cast<ProductRole>().ToList();
            foreach (var r in baseList.Where(r => r.IsAssigned))
            {
                var found = cloneList.FirstOrDefault(c =>
                    c.Name.Equals(r.Name, StringComparison.OrdinalIgnoreCase) &&
                    c.Roletype.Equals(r.Roletype, StringComparison.OrdinalIgnoreCase));
                if (found is not null) matched.Add(found.ID);
            }
        }
        else if (baseType == typeof(ClickPayRole) && cloneType == typeof(ClickPayRole))
        {
            var baseList  = baseCompanyRoles.Records.Cast<ClickPayRole>();
            var cloneList = cloneCompanyRoles.Records.Cast<ClickPayRole>().ToList();
            foreach (var r in baseList.Where(r => r.IsAssigned))
            {
                var found = cloneList.FirstOrDefault(c => c.Name.Equals(r.Name, StringComparison.OrdinalIgnoreCase));
                if (found is not null) matched.Add(found.Id);
            }
        }
        else if (baseType == typeof(Product.Integrations.Model.ProductRole) && cloneType == typeof(Product.Integrations.Model.ProductRole))
        {
            var baseList  = baseCompanyRoles.Records.Cast<Product.Integrations.Model.ProductRole>();
            var cloneList = cloneCompanyRoles.Records.Cast<Product.Integrations.Model.ProductRole>().ToList();
            foreach (var r in baseList.Where(r => r.IsAssigned))
            {
                var found = cloneList.FirstOrDefault(c => c.GetName.Equals(r.GetName, StringComparison.OrdinalIgnoreCase));
                if (found is not null) matched.Add(found.GetRoleId);
            }
        }
        else if (baseType == typeof(Level) && cloneType == typeof(Level))
        {
            var baseList  = baseCompanyRoles.Records.Cast<ILevel>();
            var cloneList = cloneCompanyRoles.Records.Cast<ILevel>().ToList();
            foreach (var r in baseList.Where(r => r.IsAssigned))
            {
                var found = cloneList.FirstOrDefault(c => c.Name.Equals(r.Name, StringComparison.OrdinalIgnoreCase));
                if (found is not null) matched.Add(found.Id);
            }
        }
        else if (baseType == typeof(SharedObjects.Product.Rum.Role) && cloneType == typeof(SharedObjects.Product.Rum.Role))
        {
            var baseList  = baseCompanyRoles.Records.Cast<SharedObjects.Product.Rum.Role>();
            var cloneList = cloneCompanyRoles.Records.Cast<SharedObjects.Product.Rum.Role>().ToList();
            foreach (var r in baseList.Where(r => r.IsAssigned))
            {
                var found = cloneList.FirstOrDefault(c => c.Name.Equals(r.Name, StringComparison.OrdinalIgnoreCase));
                if (found is not null) matched.Add(found.Id.ToString());
            }
        }

        return matched;
    }
}
