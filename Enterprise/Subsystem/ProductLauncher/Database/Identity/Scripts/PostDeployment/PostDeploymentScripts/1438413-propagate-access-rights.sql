--Set all PersistRight flag to 0 and set 1 for only below rights
GO
UPDATE [Security].[Right] SET PersistRight = 0 WHERE PersistRight=1

UPDATE [Security].[Right] SET PersistRight = 1 WHERE RightName IN 
('activatedeactivateusers'
,'CloneUser'
,'CreateUser'
,'EditOtherProfile'
,'EditUsers'
,'AbilityToImportUsers'
,'LockUnlockUsers'
,'MigrationTool'
,'ViewUsers'
,'ManageAdminSupportPortalProductAccess'
,'ManageAssetOptimizationProductAccess'
,'ManageClickPayProductAccess'
,'ManageClientPortalProductAccess'
,'ManageCommunityRewardsProductAccess'
,'ManageContactCenterMaintenanceProductAccess'
,'ManageDataHubProductaccess'
,'ManageDepositAlternativeProductAccess'
,'ManageDocumentManagementProductAccess'
,'ManageAccountingProductAccess'
,'G5LLMarketingProductsProductaccess'
,'ManageG5/LLMarketingProductsProductaccess'
,'ManageHandsOnTrainingSystemProductAccess'
,'ManageILMLeadManagemementProductAccess'
,'AccessIntegrationMarketplace'
,'ManageKnockProductaccess'
,'ManageLead2LeaseProductAccess'
,'ManageMarketingCenterProductAccess'
,'ManageHomeSharingProductAccess'
,'ManageOneSiteProductAccess'
,'ManageOnSiteProductAccess'
,'CIMPLManagePII'
,'ManagePortfolioManagementProductAccess'
,'ProspectContactCenterProductAccess'
,'ManageRelate247ProductAccess'
,'ManageRenovationManager'
,'ManageRentersInsuranceProductAccess'
,'AddEditResidentPortalUser'
,'ManageSGTourProductAccess'
,'CIMPLManageSensitiveFinancialData'
,'ManageIntelligentBuildingTrashProductAccess'
,'ManageIntelligentBuildingWaterProductAccess'
,'ManageSpendManagementProductAccess'
,'ManageUnifiedAmenitiesProductAccess'
,'ManageUtilityManagementProductAccess'
,'ManageVendorComplianceProductAccess'
,'ManageVendorMarketplaceProductAccess'
)

GO

Declare @Id int, @rightId bigint;
Select @Id = Id from Enterprise.NavigationMenu where Title = 'Manage Reports' and Origin = 'unified-reporting';
Select @rightId = RightId from Security.[Right] where RightName = 'ManagePropertyLevelReporting';

IF NOT EXISTS (Select Top 1 1 from enterprise.navigationMenuRights where NavigationMenuId =@Id and RightId = @rightId)
BEGIN
  Insert into enterprise.navigationMenuRights (NavigationMenuId,RightId) values (@Id,@rightId);
END

Go