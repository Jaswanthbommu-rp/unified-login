using UnifiedLogin.BusinessLogic.Logic.ProductIntegration.Model;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product;
using UnifiedLogin.SharedObjects.Product.Accounting;
using UnifiedLogin.SharedObjects.Product.Onsite;
using UnifiedLogin.SharedObjects.Product.Ops;
using UnifiedLogin.SharedObjects.Product.ResidentPortal;
using UnifiedLogin.SharedObjects.Product.Rum;
using UnifiedLogin.SharedObjects.Product.VendorServices;
using ProductRole = UnifiedLogin.SharedObjects.Product.ProductRole;

namespace UnifiedLogin.BusinessLogic.LogicAsync.Helper;

/// <summary>
/// Async-era static factory for building <see cref="ProductBatch"/> records.
/// <para>
/// Replaces <c>BatchHelper</c> (sync) for all <b>pure-computation</b> methods —
/// no I/O, no <c>DefaultUserClaim</c>, no inline <c>new Xxx()</c> service instantiation.
/// </para>
/// <para>
/// Methods that required I/O (AO clone, DocManagement role fetch) are extracted into
/// <see cref="IAoBatchServiceAsync"/> and <see cref="IDocManagementBatchServiceAsync"/>
/// injectable services.
/// </para>
/// <para>
/// C# 13 improvements applied throughout:
/// <list type="bullet">
///   <item>Collection expressions <c>[]</c> replace <c>new List&lt;T&gt;()</c></item>
///   <item>Property patterns replace <c>.GetType() == typeof(X)</c> + explicit casts</item>
///   <item><c>switch</c> expressions replace multi-branch <c>if/else</c> chains</item>
///   <item>Single <c>GetBoolFlag</c> helper de-duplicates three identical flag-check methods</item>
///   <item>Generic <c>FilterAssigned</c> helper collapses ~180 lines of repetitive property filtering</item>
/// </list>
/// </para>
/// </summary>
public static class BatchHelperAsync
{
    // ── Standard ProductBatch factory ─────────────────────────────────────────

    public static ProductBatch CreateProductBatchRecord(
        ListResponse propertiesResponse,
        ListResponse rolesResponse,
        int productId,
        bool usePrimaryProperties,
        ProductIntegrationTypeEnum integrationType)
    {
        List<string> propertyList = [];
        List<string> roleList     = [];
        string       roleType     = string.Empty;

        bool allProperties            = propertiesResponse.Additional is not null && GetBoolFlag(propertiesResponse.Additional, "allProperties");
        bool isAssignNewPropertyByDefault = propertiesResponse.Additional is not null && GetBoolFlag(propertiesResponse.Additional, "IsAssignedNewPropertyByDefault");

        // ── Roles ────────────────────────────────────────────────────────────
        if (productId != (int)ProductEnum.ProspectContactCenter && rolesResponse.Records is not null)
        {
            foreach (var item in rolesResponse.Records)
            {
                switch (item)
                {
                    case Logic.ProductIntegration.Model.ProductRole { IsAssigned: true } pr:
                        roleList.Add(pr.GetRoleId);
                        break;
                    case ProductRole { IsAssigned: true } pr:
                        roleList.Add(pr.ID);
                        roleType = pr.Roletype;
                        break;
                }
            }
        }

        // ── Properties ───────────────────────────────────────────────────────
        if (allProperties)
        {
            propertyList.Add(productId switch
            {
                (int)ProductEnum.ClientPortal or (int)ProductEnum.AdminSupportPortal => "-1",
                _ when integrationType is ProductIntegrationTypeEnum.UPFM
                                       or ProductIntegrationTypeEnum.StandardV1      => "-1",
                (int)ProductEnum.OneSite
                    or (int)ProductEnum.FinancialSuite
                    or (int)ProductEnum.ProspectContactCenter
                    or (int)ProductEnum.MarketingCenter
                    or (int)ProductEnum.Insurance
                    or (int)ProductEnum.ResidentPortal                               => "ALL",
                _                                                                    => "-1"
            });
        }
        else if (propertiesResponse.Records is not null)
        {
            foreach (var item in propertiesResponse.Records)
            {
                string? id = item switch
                {
                    ProductProperty p                      => integrationType == ProductIntegrationTypeEnum.UPFM ? p.Alias : p.ID,
                    ACProperty      { IsAssigned: true } p => p.Id,
                    AssetGroup      { IsAssigned: true } p => p.AssetID,
                    OnSiteProperty  { IsAssigned: true } p => p.Id.ToString(),
                    RumPropertyGroup{ IsAssigned: true } p => p.Id.ToString(),
                    ProductProperties{ IsAssigned: true }p => p.GetPropertyId,
                    Portfolio       { IsAssigned: true } p => p.ID,
                    _                                      => null
                };
                if (id is not null) propertyList.Add(id);
            }
        }

        return new ProductBatch
        {
            ProductId    = productId,
            StatusTypeId = 5,
            RetryCount   = 0,
            InputJson    = new RolePropertyList
            {
                PropertyList                  = propertyList,
                RoleList                      = roleList,
                IsAssignedNewPropertyByDefault = isAssignNewPropertyByDefault,
                UsePrimaryProperties          = usePrimaryProperties,
                RoleType                      = roleType
            }
        };
    }

    /// <summary>
    /// Returns a <see cref="ListResponse"/> containing only the assigned properties
    /// from <paramref name="propertiesResponse"/>.
    /// </summary>
    public static ListResponse GetUserAssignedPropertiesData(ListResponse propertiesResponse)
    {
        if (propertiesResponse.Records is not { Count: > 0 })
            return new ListResponse();

        var records = propertiesResponse.Records;
        return records[0] switch
        {
            ProductProperty  _ => FilterAssigned<ProductProperty> (records, p => p.IsAssigned == true),
            ACProperty       _ => FilterAssigned<ACProperty>      (records, p => p.IsAssigned),
            AssetGroup       _ => FilterAssigned<AssetGroup>      (records, p => p.IsAssigned),
            OnSiteProperty   _ => FilterAssigned<OnSiteProperty>  (records, p => p.IsAssigned),
            RumPropertyGroup _ => FilterAssigned<RumPropertyGroup>(records, p => p.IsAssigned),
            ProductProperties _ => FilterAssigned<ProductProperties>(records, p => p.IsAssigned),
            Portfolio        _ => FilterAssigned<Portfolio>       (records, p => p.IsAssigned),
            _                  => new ListResponse()
        };
    }

    // ── Product-specific batch factories ─────────────────────────────────────

    public static ProductBatch CreateOnSiteBatchRecord(
        ListResponse propertiesResponse,
        ListResponse rolesResponse,
        ListResponse regionResponse,
        int productId,
        bool usePrimaryProperties)
    {
        List<string> propertyList = [];
        List<string> roleList     = [];
        List<string> regionList   = [];

        bool allProperties = propertiesResponse.Additional is not null && GetBoolFlag(propertiesResponse.Additional, "allProperties");
        bool allRegions    = regionResponse.Additional    is not null && GetBoolFlag(regionResponse.Additional, "allProperties");

        if (allProperties)
        {
            propertyList.Add("-1");
        }
        else if (propertiesResponse.Records is not null)
        {
            propertyList.AddRange(
                propertiesResponse.Records
                    .OfType<OnSiteProperty>()
                    .Where(p => p.IsAssigned)
                    .Select(p => p.Id.ToString()));
        }

        if (allRegions)
        {
            regionList.Add("-1");
        }
        else if (regionResponse.Records is not null)
        {
            regionList.AddRange(
                regionResponse.Records
                    .OfType<OnSiteRegion>()
                    .Where(r => r.IsAssigned)
                    .Select(r => r.Id.ToString()));
        }

        if (rolesResponse.Records is not null)
        {
            roleList.AddRange(
                rolesResponse.Records
                    .OfType<OnSiteRole>()
                    .Where(r => r.IsAssigned == true)
                    .Select(r => r.Level.ToString()));
        }

        return new ProductBatch
        {
            ProductId    = productId,
            StatusTypeId = 5,
            RetryCount   = 0,
            InputJson    = new RolePropertyList
            {
                PropertyList         = propertyList,
                RoleList             = roleList,
                RegionList           = regionList,
                UsePrimaryProperties = usePrimaryProperties
            }
        };
    }

    public static ProductBatch CreateProductBatchRecordForClickPay(
        List<OrganizationRole> userOrganizationRole,
        bool usePrimaryProperties)
        => new()
        {
            ProductId    = (int)ProductEnum.ClickPay,
            StatusTypeId = 5,
            RetryCount   = 0,
            InputJson    = new RolePropertyList
            {
                OrganizationRoleList = userOrganizationRole,
                UsePrimaryProperties = usePrimaryProperties
            }
        };

    public static ProductBatch CreateProductBatchRecordForDepositIQ(
        IntegrationProductUser productUser,
        bool usePrimaryProperties)
        => new()
        {
            ProductId    = (int)ProductEnum.DepositAlternative,
            StatusTypeId = 5,
            RetryCount   = 0,
            InputJson    = new RolePropertyList
            {
                RoleList                 = productUser.Roles,
                CanReceiveMonthlyReport  = productUser.CanReceiveMonthlyReport,
                PropertyGroupList        = productUser.PropertyGroups,
                PropertyList             = productUser.Properties,
                UsePrimaryProperties     = usePrimaryProperties
            }
        };

    public static ProductBatch CreateIntegrationMarketplaceBatchRecord(
        int existingRoleId,
        int productProductId,
        bool usePrimaryProperties)
        => new()
        {
            ProductId    = productProductId,
            StatusTypeId = 5,
            RetryCount   = 0,
            InputJson    = new RolePropertyList
            {
                RoleList             = [existingRoleId.ToString()],
                UsePrimaryProperties = usePrimaryProperties
            }
        };

    public static ProductBatch CreateILMProductBatchRecord(
        ProductEnum ilmProduct,
        List<string> productUserProperties,
        List<string> productUserRoles,
        List<string> productUserGroups,
        bool usePrimaryProperties)
        => new()
        {
            ProductId    = (int)ilmProduct,
            StatusTypeId = 5,
            RetryCount   = 0,
            InputJson    = new RolePropertyList
            {
                PropertyList         = productUserProperties,
                RoleList             = productUserRoles,
                PropertyGroupList    = productUserGroups,
                UsePrimaryProperties = usePrimaryProperties
            }
        };

    public static ProductBatch CreateProductBatchRecordForPortfolioManagement(
        List<PAMRolePropertyList> rolePropertyList,
        List<string> roleList,
        bool usePrimaryProperties)
        => new()
        {
            ProductId    = (int)ProductEnum.PortfolioManagement,
            StatusTypeId = 5,
            RetryCount   = 0,
            InputJson    = new RolePropertyList
            {
                RolePropertiesList   = rolePropertyList,
                RoleList             = roleList,
                UsePrimaryProperties = usePrimaryProperties
            }
        };

    public static ProductBatch CreateMarketingCenterProductBatchRecord(
        ListResponse propertiesResponse,
        ListResponse rolesResponse,
        int productId,
        bool usePrimaryProperties)
    {
        List<string> propertyList = [];
        List<string> roleList     = [];

        bool isAssignNewPropertyByDefault = propertiesResponse.Additional is not null
            && GetBoolFlag(propertiesResponse.Additional, "IsAssignedNewPropertyByDefault");

        if (productId != (int)ProductEnum.ProspectContactCenter && rolesResponse.Records is not null)
        {
            roleList.AddRange(
                rolesResponse.Records
                    .OfType<ProductRole>()
                    .Where(r => r.IsAssigned)
                    .Select(r => r.ID));
        }

        if (propertiesResponse.Records is not null)
        {
            foreach (var item in propertiesResponse.Records)
            {
                string? id = item switch
                {
                    AssetGroup   { IsAssigned: true } ag when productId == (int)ProductEnum.OpsBuyer => ag.ID,
                    ProductProperty p when p.IsAssigned == true                                      => p.ID,
                    _                                                                                 => null
                };
                if (id is not null) propertyList.Add(id);
            }
        }

        return new ProductBatch
        {
            ProductId    = productId,
            StatusTypeId = 5,
            RetryCount   = 0,
            InputJson    = new RolePropertyList
            {
                PropertyList                  = propertyList,
                RoleList                      = roleList,
                IsAssignedNewPropertyByDefault = isAssignNewPropertyByDefault,
                UsePrimaryProperties          = usePrimaryProperties
            }
        };
    }

    public static ProductBatch CreateFinancialSuiteProductBatchRecord(
        ListResponse propertiesResponse,
        ListResponse rolesResponse,
        int productId,
        ListResponse companiesResponse,
        ListResponse propertyGroupResponse,
        bool usePrimaryProperties)
    {
        List<string> propertyList      = [];
        List<string> propertyGroupList = [];
        List<string> roleList          = [];
        List<string> companiesList     = [];

        bool hasAccessToSiteSpendManagementOnly     = false;
        bool isAccountingAdmin                      = false;
        bool hasAccessToAllCurrentFutureProperties  = false;

        if (companiesResponse.Additional is AccountingUser au)
        {
            hasAccessToSiteSpendManagementOnly    = au.HasAccessToSiteSpendManagementOnly;
            isAccountingAdmin                     = au.IsAccountingAdmin;
            hasAccessToAllCurrentFutureProperties = au.HasAccessToAllCurrentFutureProperties;
        }

        if (companiesResponse.Records is not null)
        {
            companiesList.AddRange(
                companiesResponse.Records
                    .OfType<ACCompany>()
                    .Where(c => !string.IsNullOrEmpty(c.Id))
                    .Select(c => c.Id));
        }

        if (rolesResponse.Records is not null)
        {
            roleList.AddRange(
                rolesResponse.Records
                    .OfType<ProductRole>()
                    .Where(r => r.IsAssigned)
                    .Select(r => r.ID));
        }

        if (propertiesResponse.Records is not null)
        {
            foreach (var item in propertiesResponse.Records)
            {
                string? id = item switch
                {
                    ProductProperty { IsAssigned : true } p when p.IsAssigned!.Value => p.ID,
                    ACProperty      { IsAssigned: true }          p                  => p.Id,
                    _                                                                => null
                };
                if (id is not null) propertyList.Add(id);
            }
        }

        if (propertyGroupResponse.Records is not null)
        {
            propertyGroupList.AddRange(
                propertyGroupResponse.Records
                    .OfType<ProductPropertyGroup>()
                    .Where(g => g.IsAssigned == true)
                    .Select(g => g.ID));
        }

        return new ProductBatch
        {
            ProductId    = productId,
            StatusTypeId = 5,
            RetryCount   = 0,
            InputJson    = new RolePropertyList
            {
                PropertyList                          = propertyList,
                RoleList                              = roleList,
                HasAccessToSiteSpendManagementOnly    = hasAccessToSiteSpendManagementOnly,
                IsAccountingAdmin                     = isAccountingAdmin,
                HasAccessToAllCurrentFutureProperties = hasAccessToAllCurrentFutureProperties,
                CompaniesList                         = companiesList,
                UsePrimaryProperties                  = usePrimaryProperties
            }
        };
    }

    public static ProductBatch CreateVendorServiceProductBatchRecord(
        ListResponse propertiesResponse,
        ListResponse rolesResponse,
        ListResponse propertyGroup,
        UnifiedLogin.SharedObjects.Product.VendorServices.Notification notification,
        int productId,
        bool usePrimaryProperties)
    {
        List<string> propertyList      = [];
        List<string> roleList          = [];
        var          propertyGroupList = new List<UnifiedLogin.SharedObjects.Product.VendorServices.PropertyGroup>();

        bool allProperties = propertiesResponse.Additional is not null
            && GetBoolFlag(propertiesResponse.Additional, "allProperties");

        // When cloning a user with all-properties, unselected count == total count
        if (!allProperties && propertiesResponse.Records is not null)
        {
            var propColl = propertiesResponse.Records.OfType<ProductProperty>().ToList();
            if (propColl.Count > 0 && propColl.All(p => p.IsAssigned == false))
                allProperties = true;
        }

        if (rolesResponse.Records is not null)
        {
            roleList.AddRange(
                rolesResponse.Records
                    .OfType<ProductRole>()
                    .Where(r => r.IsAssigned)
                    .Select(r => r.ID));
        }

        if (propertyGroup.TotalRows > 0 && propertyGroup.Records is not null)
        {
            foreach (var item in propertyGroup.Records.OfType<VendorServicesPropertyGroup>().Where(g => g.IsAssigned))
            {
                propertyGroupList.Add(new UnifiedLogin.SharedObjects.Product.VendorServices.PropertyGroup
                {
                    Id         = item.PropertyGroupId,
                    IsAssigned = true,
                    Type       = (UnifiedLogin.SharedObjects.Product.VendorServices.AccessTypeEnum)
                                 Enum.Parse(typeof(UnifiedLogin.SharedObjects.Product.VendorServices.AccessTypeEnum), item.AccessLevel)
                });
            }
        }

        if (allProperties)
        {
            propertyList.Add("-1");
        }
        else if (propertiesResponse.Records is not null)
        {
            foreach (var item in propertiesResponse.Records)
            {
                string? id = item switch
                {
                    AssetGroup    { IsAssigned: true } ag when productId == (int)ProductEnum.OpsBuyer => ag.ID,
                    ProductProperty p when p.IsAssigned == true                                       => p.ID,
                    _                                                                                  => null
                };
                if (id is not null) propertyList.Add(id);
            }
        }

        return new ProductBatch
        {
            ProductId    = productId,
            StatusTypeId = 5,
            RetryCount   = 0,
            InputJson    = new RolePropertyList
            {
                PropertyList                     = propertyList,
                RoleList                         = roleList,
                PropertyGroup                    = propertyGroupList.Count > 0 ? propertyGroupList : null,
                IsInsuranceExpired               = notification.IsInsuranceExpired,
                IsVendorRecommendationChanges    = notification.IsVendorRecommendationChanges,
                IsVendorNotLinkedToAnyProperty   = notification.IsVendorNotLinkedToAnyProperty,
                UsePrimaryProperties             = usePrimaryProperties
            }
        };
    }

    public static ProductBatch CreateRumProductBatchRecord(
        ListResponse propertiesResponse,
        ListResponse groupResponse,
        ListResponse regionResponse,
        ListResponse rolesResponse,
        bool usePrimaryProperties)
    {
        List<string> propertyList      = [];
        List<string> propertyGroupList = [];
        List<string> regionsList       = [];
        List<string> roleList          = [];

        if (rolesResponse.Records is not null)
        {
            roleList.AddRange(
                rolesResponse.Records
                    .OfType<UnifiedLogin.SharedObjects.Product.Rum.Role>()
                    .Where(r => r.IsAssigned)
                    .Select(r => r.Name));
        }

        if (regionResponse.Records is not null)
        {
            regionsList.AddRange(
                regionResponse.Records
                    .OfType<RumPropertyGroup>()
                    .Where(r => r.IsAssigned)
                    .Select(r => r.Id.ToString()));
        }

        if (groupResponse.Records is not null)
        {
            propertyGroupList.AddRange(
                groupResponse.Records
                    .OfType<RumPropertyGroup>()
                    .Where(g => g.IsAssigned)
                    .Select(g => g.Id.ToString()));
        }

        if (propertiesResponse.Records is not null)
        {
            var propColl = propertiesResponse.Records.OfType<RumPropertyGroup>().ToList();
            propertyList.AddRange(propColl.Where(p => p.IsAssigned).Select(p => p.Id.ToString()));

            // Clone from all-properties: when none selected treat as "All"
            if (propertyGroupList.Count == 0 && propColl.Count > 0 && propColl.All(p => !p.IsAssigned))
                propertyList.Add("All");
        }

        return new ProductBatch
        {
            ProductId    = (int)ProductEnum.UtilityManagement,
            StatusTypeId = 5,
            RetryCount   = 0,
            InputJson    = new RolePropertyList
            {
                PropertyList         = propertyList,
                PropertyGroupList    = propertyGroupList,
                RegionList           = regionsList,
                RoleList             = roleList,
                UsePrimaryProperties = usePrimaryProperties
            }
        };
    }

    public static ProductBatch CreateResidentPortalProductBatchRecord(
        ListResponse propertiesResponse,
        List<ILevel> rolesResponse,
        Notifications notifications,
        List<IMessagingGroups> messagingGroups,
        int productId,
        bool usePrimaryProperties)
    {
        List<string> propertyList  = [];
        List<string> roleList      = [];
        List<string> messageGroups = [];

        bool allProperties = propertiesResponse.Additional is not null
            && GetBoolFlag(propertiesResponse.Additional, "allProperties");

        if (allProperties && productId == (int)ProductEnum.ResidentPortal)
        {
            propertyList.Add("ALL");
        }
        else if (propertiesResponse.Records is not null)
        {
            propertyList.AddRange(
                propertiesResponse.Records
                    .OfType<ProductProperty>()
                    .Where(p => p.IsAssigned == true)
                    .Select(p => p.ID));
        }

        roleList.Add(rolesResponse.Find(i => i.IsAssigned == true)!.Id.ToUpper());

        messageGroups.AddRange(
            messagingGroups
                .OfType<MessagingGroups>()
                .Where(m => m.IsAssigned)
                .Select(m => m.Id));

        return new ProductBatch
        {
            ProductId    = productId,
            StatusTypeId = 5,
            RetryCount   = 0,
            InputJson    = new RolePropertyList
            {
                PropertyList         = propertyList,
                RoleList             = roleList,
                Notifications        = notifications,
                MessageGroups        = messageGroups,
                UsePrimaryProperties = usePrimaryProperties
            }
        };
    }

    public static ProductBatch CreateRentersInsuranceProductBatchRecord(
        ListResponse propertiesResponse,
        IList<ProductRole> rolesResponse,
        int productId,
        bool usePrimaryProperties)
    {
        List<string> propertyList = [];

        bool allProperties = propertiesResponse.Additional is not null
            && GetBoolFlag(propertiesResponse.Additional, "allProperties");

        if (allProperties && productId == (int)ProductEnum.Insurance)
        {
            propertyList.Add("ALL");
        }
        else if (propertiesResponse.Records is not null)
        {
            propertyList.AddRange(
                propertiesResponse.Records
                    .OfType<ProductProperty>()
                    .Where(p => p.IsAssigned == true)
                    .Select(p => p.ID));
        }

        return new ProductBatch
        {
            ProductId    = productId,
            StatusTypeId = 5,
            RetryCount   = 0,
            InputJson    = new RolePropertyList
            {
                PropertyList         = propertyList,
                RoleList             = [rolesResponse.First(r => r.IsAssigned).ID],
                UsePrimaryProperties = usePrimaryProperties
            }
        };
    }

    public static ProductBatch CreateSelfProvisioningPortalProductBatchRecord(int productId)
        => new()
        {
            ProductId    = productId,
            StatusTypeId = 5,
            RetryCount   = 0,
            InputJson    = new RolePropertyList()
        };

    // ── Private helpers ───────────────────────────────────────────────────────

    /// <summary>
    /// Single helper for all three flag-check methods that existed in <c>BatchHelper</c>.
    /// Returns <c>true</c> only when <paramref name="additionalInfo"/> is a
    /// <c>Dictionary&lt;string, bool&gt;</c> and the key maps to <c>true</c>.
    /// </summary>
    private static bool GetBoolFlag(object additionalInfo, string key)
        => additionalInfo is Dictionary<string, bool> d
           && d.TryGetValue(key, out bool val)
           && val;

    /// <summary>
    /// Generic filter helper that collapses the ~180-line repetitive per-type
    /// filter block in <c>BatchHelper.GetUserAssignedPropertiesData</c>.
    /// </summary>
    private static ListResponse FilterAssigned<T>(IList<object> source, Func<T, bool> predicate)
        where T : class
    {
        var filtered = source.Cast<T>().Where(predicate).ToList();
        return new ListResponse
        {
            Records      = [.. filtered.Cast<object>()],
            TotalRows    = filtered.Count,
            RowsPerPage  = filtered.Count,
            TotalPages   = 1,
            ErrorReason  = string.Empty
        };
    }
}
