using RealPage.DataAccess.Dapper;
using System.Data;
using System.Security.Claims;
using UnifiedLogin.BusinessLogic.Extensions;
using UnifiedLogin.BusinessLogic.Interfaces;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects.Constants;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.Repository;

public class PersonaRepositoryAsync(IDbConnection db, ICacheService cacheService, IOrganizationRepositoryAsync organizationRepositoryAsync, IUserLoginRepositoryAsync userLoginRepositoryAsync)
    : IPersonaRepositoryAsync
{
    private readonly ICacheService _cacheService = cacheService;

    public async Task<Persona> GetPersonaAsync(long personaId, ClaimsPrincipal user, bool withRights = true, CancellationToken token = default)
    {
        var persona = await db.GetOneAsync<Persona>(StoredProcNameConstants.SP_GetPersona, new { personaId }, token: token);
        if (persona == null)
        {
            return null;
        }

        var organization = await organizationRepositoryAsync.GetOrganizationAsync(organizationPartyId: persona.OrganizationPartyId, token: token);
        if (organization != null)
        {
            persona.Organization = organization;
        }

        if (withRights)
        {
            persona = await AddRightsToPersona(persona, user, token);
        }

        return persona;
    }

    public async Task<IEnumerable<Persona>> ListActivePersonaAsync(Guid realPageId, bool includeOrganization, CancellationToken token = default)
    {
        dynamic param = new
        {
            RealPageId = realPageId
        };

        var organizationList = await userLoginRepositoryAsync.ListOrganizationByRealPageIdAsync(realPageId, token);
        var organizationByPartyId = organizationList.ToDictionary(o => o.PartyId);

        var personaList = (await db.GetManyAsync<Persona>(StoredProcNameConstants.SP_ListActivePersona, (object)param, token: token)).ToList();

        foreach (var persona in personaList)
        {
            organizationByPartyId.TryGetValue(persona.OrganizationPartyId, out var organization);
            persona.Organization = organization;
        }
        return personaList;
    }

    private async Task<Persona> AddRightsToPersona(Persona persona, ClaimsPrincipal User, CancellationToken token)
    {
        persona.hasViewOnlySupportToolAccess = false;

        //NOT Super user then check for Right
        if (persona.UserTypeId != UserTypeConstants.SuperUser)
        {
            persona.hasResidentPortalUserAccess = User.Rights().Contains("AddEditResidentPortalUser");

            var editorRights = User.Rights();
            persona.hasManageAccountingProductAccess = editorRights.Contains(nameof(ProductRightEnum.ManageAccountingProductAccess), StringComparer.OrdinalIgnoreCase);

            persona.hasManageAssetOptimizationProductAccess = editorRights.Contains(nameof(ProductRightEnum.ManageAssetOptimizationProductAccess), StringComparer.OrdinalIgnoreCase);

            persona.hasManageClientPortalProductAccess = editorRights.Contains(nameof(ProductRightEnum.ManageClientPortalProductAccess), StringComparer.OrdinalIgnoreCase);

            persona.hasManageDocumentManagementProductAccess = editorRights.Contains(nameof(ProductRightEnum.ManageDocumentManagementProductAccess), StringComparer.OrdinalIgnoreCase);

            persona.hasManageILMLeadManagemementProductAccess = editorRights.Contains(nameof(ProductRightEnum.ManageILMLeadManagemementProductAccess), StringComparer.OrdinalIgnoreCase);

            persona.hasManageILMLeasingAnalyticsProductAccess = editorRights.Contains(nameof(ProductRightEnum.ManageILMLeasingAnalyticsProductAccess), StringComparer.OrdinalIgnoreCase);

            persona.hasManageLead2LeaseProductAccess = editorRights.Contains(nameof(ProductRightEnum.ManageLead2LeaseProductAccess), StringComparer.OrdinalIgnoreCase);

            persona.hasManageMarketingCenterProductAccess = editorRights.Contains(nameof(ProductRightEnum.ManageMarketingCenterProductAccess), StringComparer.OrdinalIgnoreCase);

            persona.hasManageOneSiteProductAccess = editorRights.Contains(nameof(ProductRightEnum.ManageOneSiteProductAccess), StringComparer.OrdinalIgnoreCase);

            persona.hasManageOnSiteProductAccess = editorRights.Contains(nameof(ProductRightEnum.ManageOnSiteProductAccess), StringComparer.OrdinalIgnoreCase);

            persona.hasProspectContactCenterProductAccess = editorRights.Contains(nameof(ProductRightEnum.ProspectContactCenterProductAccess), StringComparer.OrdinalIgnoreCase);

            persona.hasManageRentersInsuranceProductAccess = editorRights.Contains(nameof(ProductRightEnum.ManageRentersInsuranceProductAccess), StringComparer.OrdinalIgnoreCase);

            persona.hasManageSpendManagementProductAccess = editorRights.Contains(nameof(ProductRightEnum.ManageSpendManagementProductAccess), StringComparer.OrdinalIgnoreCase);

            persona.hasManageUnifiedAmenitiesProductAccess = editorRights.Contains(nameof(ProductRightEnum.ManageUnifiedAmenitiesProductAccess), StringComparer.OrdinalIgnoreCase);

            persona.hasManageUtilityManagementProductAccess = editorRights.Contains(nameof(ProductRightEnum.ManageUtilityManagementProductAccess), StringComparer.OrdinalIgnoreCase);

            persona.hasManageVendorComplianceProductAccess = editorRights.Contains(nameof(ProductRightEnum.ManageVendorComplianceProductAccess), StringComparer.OrdinalIgnoreCase);

            persona.hasManageRealConnectProductAccess = editorRights.Contains(nameof(ProductRightEnum.ManageRealConnectProductAccess), StringComparer.OrdinalIgnoreCase);

            persona.hasManagePortfolioManagementProductAccess = editorRights.Contains(nameof(ProductRightEnum.ManagePortfolioManagementProductAccess), StringComparer.OrdinalIgnoreCase);

            persona.hasManageIntegrationMarketplaceProductAccess = editorRights.Contains(nameof(ProductRightEnum.AccessIntegrationMarketplace), StringComparer.OrdinalIgnoreCase);

            persona.hasManagePlatFormSecurity = editorRights.Contains(nameof(ProductRightEnum.ManagePlatFormSecurity), StringComparer.OrdinalIgnoreCase);

            persona.hasManageCustomFields = editorRights.Contains(nameof(ProductRightEnum.ManageCustomFields), StringComparer.OrdinalIgnoreCase);

            persona.hasManageUnifiedSettings = editorRights.Contains(nameof(ProductRightEnum.ManageUnifiedSettings), StringComparer.OrdinalIgnoreCase);

            persona.hasManageClickPayProductAccess = editorRights.Contains(nameof(ProductRightEnum.ManageClickPayProductAccess), StringComparer.OrdinalIgnoreCase);


            persona.hasManageDepositAlternativeProductAccess = editorRights.Contains(nameof(ProductRightEnum.ManageDepositAlternativeProductAccess), StringComparer.OrdinalIgnoreCase);

            persona.hasManageSettingsTemplates = editorRights.Contains(nameof(ProductRightEnum.ManageSettingsTemplates), StringComparer.OrdinalIgnoreCase);

            persona.hasnotificationsAccess = editorRights.Contains("ManageNotifications", StringComparer.OrdinalIgnoreCase);

            persona.hasManageIntelligentBuildingTrashProductAccess = editorRights.Contains(nameof(ProductRightEnum.ManageIntelligentBuildingTrashProductAccess), StringComparer.OrdinalIgnoreCase);

            persona.hasManageIntelligentBuildingEnergyProductAccess = editorRights.Contains(nameof(ProductRightEnum.ManageIntelligentBuildingEnergyProductAccess), StringComparer.OrdinalIgnoreCase);

            persona.hasManageIntelligentBuildingWaterProductAccess = editorRights.Contains(nameof(ProductRightEnum.ManageIntelligentBuildingWaterProductAccess), StringComparer.OrdinalIgnoreCase);

            persona.hasManageHandsOnTrainingSystemAccess = editorRights.Contains(nameof(ProductRightEnum.ManageHandsOnTrainingSystemProductAccess), StringComparer.OrdinalIgnoreCase);

            persona.hasManageLeaseLabsAccess = editorRights.Contains(nameof(ProductRightEnum.ManageLeaseLabsProductAccess), StringComparer.OrdinalIgnoreCase);


            persona.hasManageLeadScoringAccess = editorRights.Contains(nameof(ProductRightEnum.ManageLeadScoringProductAccess), StringComparer.OrdinalIgnoreCase);

            persona.hasPlatformAlertsAccess = (editorRights.Contains("CreatePlatformAlerts", StringComparer.OrdinalIgnoreCase)
                                            || editorRights.Contains("ApprovePlatformAlerts", StringComparer.OrdinalIgnoreCase));

            persona.hasImportUsersAccess = editorRights.Contains("AbilityToImportUsers", StringComparer.OrdinalIgnoreCase);

            persona.hasManageSmartWasteCommercialProductAccess = editorRights.Contains
                (nameof(ProductRightEnum.ManageSmartWasteCommercialProductAccess), StringComparer.OrdinalIgnoreCase);

            persona.hasManageAdminSupportPortalProductAccess = editorRights.Contains
               (nameof(ProductRightEnum.ManageAdminSupportPortalProductAccess), StringComparer.OrdinalIgnoreCase);

        }

        if (User.Identity is { IsAuthenticated: false }) return persona;


        persona.hasViewOnlySupportToolAccess = User.Rights().Contains("ViewOnlySupportToolAccess", StringComparer.OrdinalIgnoreCase);
        persona.hasViewOnlySettingsAccess = User.Rights().Contains("ViewUnifiedSettings", StringComparer.OrdinalIgnoreCase);
        persona.hasManageUnifiedSettings = User.Rights().Contains("ManageUnifiedSetting", StringComparer.OrdinalIgnoreCase);
        persona.hasManageCustomFields = User.Rights().Contains("ManageCustomFields", StringComparer.OrdinalIgnoreCase);
        persona.hasManagePlatFormSecurity = User.Rights().Contains("ManagePlatFormSecurity", StringComparer.OrdinalIgnoreCase);
        persona.hasAccessSettingsAdmin = User.Rights().Contains("AccessSettingsAdmin", StringComparer.OrdinalIgnoreCase);
        persona.hasManageSettingsTemplates = User.Rights().Contains("ManageSettingsTemplates", StringComparer.OrdinalIgnoreCase);
        persona.hasnotificationsAccess = User.Rights().Contains("ManageNotifications", StringComparer.OrdinalIgnoreCase);
        persona.hasPlatformAlertsAccess = (User.Rights().Contains("CreatePlatformAlerts", StringComparer.OrdinalIgnoreCase)
                                        || User.Rights().Contains("ApprovePlatformAlerts", StringComparer.OrdinalIgnoreCase));

        persona.hasImportUsersAccess = User.Rights().Contains("AbilityToImportUsers", StringComparer.OrdinalIgnoreCase);

        if (persona is { hasViewOnlySettingsAccess: true, hasManageUnifiedSettings: true, hasManageCustomFields: true, hasManagePlatFormSecurity: true, hasManageSettingsTemplates: true })
        {
            return persona;
        }

        // check to see impersonating, if they are then check that users rights
        if (User.ImpersonatedBy() == Guid.Empty) return persona;

        long activePersonaId = await GetActivePersonaIdAsync(User.ImpersonatedBy(), token);
        /*
        var userRoles = await _userRoleRightRepository.ListRoleByPersonaAsync((int)ProductEnum.UnifiedPlatform, activePersonaId, User.OrgPartyId(), token);
        var roleList = await _cacheService.GetOrSetAsync($"{nameof(PersonaRepository)}_{User.OrgPartyId()}", async _ =>
        {
            var solutionList = await _companyRepo.GetSolutionsForCompanyAsync(User.OrganizationGuid(), token);
            var productList = solutionList.Select(a => a.ProductId).ToList();
            var roleList = await _userRoleRightRepository.GetAllRoleRightsAsync(User.OrgPartyId(), productList, (int)ProductEnum.UnifiedPlatform, token);
            return roleList;
        }, new CacheEntryOptions() { ExpirationTimeInMinutes = 3 }, cancellationToken: token);


        foreach (SharedObjects.Product.UnifiedLogin.Role userRole in userRoles)
        {
            if (!persona.hasViewOnlySettingsAccess && roleList.Any(r => r.RoleId == userRole.RoleID))
            {
                foreach (Right right in roleList.FirstOrDefault(r => r.RoleId == userRole.RoleID).UserRights)
                {
                    if (!string.IsNullOrEmpty(right.RightNickName))
                    {
                        if (right.RightNickName.Equals("ViewUnifiedSettings", StringComparison.OrdinalIgnoreCase))
                        {
                            persona.hasViewOnlySettingsAccess = true;
                        }

                        if (right.RightNickName.Equals("ManageUnifiedSetting", StringComparison.OrdinalIgnoreCase))
                        {
                            persona.hasManageUnifiedSettings = true;
                        }

                        if (right.RightNickName.Equals("ManageCustomFields", StringComparison.OrdinalIgnoreCase))
                        {
                            persona.hasManageCustomFields = true;
                        }

                        if (right.RightNickName.Equals("ManagePlatFormSecurity", StringComparison.OrdinalIgnoreCase))
                        {
                            persona.hasManagePlatFormSecurity = true;
                        }

                        if (right.RightNickName.Equals("ManageSettingsTemplates", StringComparison.OrdinalIgnoreCase))
                        {
                            persona.hasManageSettingsTemplates = true;
                        }
                    }
                }
            }
        }
        */
        return persona;
    }

    private async Task<long> GetActivePersonaIdAsync(Guid realPageId, CancellationToken token)
    {
        return await db.GetOneAsync<long>(StoredProcNameConstants.SP_GetActivePersona, new { RealPageId = realPageId }, token: token);
    }
}


