

-- User Story 1666190: Enable All Product Specific rights in the employee company - ignore Target product enablement

GO

update [Security].[Right] set TargetProductId = 3 where TargetProductId <> 3 AND [RightName] in (
'AddEditResidentPortalUser',
'ManageOneSiteProductAccess',
'ManageRentersInsuranceProductAccess',
'ProspectContactCenterProductAccess',
'ManageMarketingCenterProductAccess',
'ManageLead2LeaseProductAccess',
'ManageILMLeadManagemementProductAccess',
'ManageDocumentManagementProductAccess',
'ManageAccountingProductAccess',
'ManageAssetOptimizationProductAccess',
'ManageILMLeasingAnalyticsProductAccess',
'ManageVendorComplianceProductAccess',
'ManageUtilityManagementProductAccess',
'ManageSpendManagementProductAccess',
'ManageUnifiedAmenitiesProductAccess',
'ManageOnSiteProductAccess',
'ManagePortfolioManagementProductAccess',
'AccessIntegrationMarketplace',
'ManageClickPayProductAccess',
'ManageDepositAlternativeProductAccess',
'ManageIntelligentBuildingTrashProductAccess',
'ManageIntelligentBuildingEnergyProductAccess',
'ManageIntelligentBuildingWaterProductAccess',
'ManageHomeSharingProductAccess',
'ManageLeaseLabsProductAccess',
'ManageHandsOnTrainingSystemProductAccess',
'ManageSGTourProductAccess',
'ManageRelate247ProductAccess',
'ManageCommunityRewardsProductAccess',
'ManageVendorMarketplaceProductAccess',
'ManageIndependentFacilitiesProductAccess',
'ManageContactCenterMaintenanceProductAccess',
'G5LLMarketingProductsProductaccess',
'ManageAdminSupportPortalProductAccess',
'ManageDataHubProductaccess',
'ManageKnockProductaccess',
'ManageSustainabilityAnalystProductaccess',
'ManageSmartWasteCommercialProductAccess',
'ManageSustainabilityServicesProductaccess',
'ManageWeb2PrintSocialProductAccess')

GO

update [Security].[Right] set ProductId = 3 where ProductId <> 3 and RightName = 'ManageWeb2PrintSocialProductAccess'

GO



