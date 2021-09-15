CREATE PROCEDURE [Enterprise].[GetNavigationMenuSettingUnaccessable]
(
    @PartyId int
)
AS
BEGIN
	select A.NavigationMenuSettingAccessId AS NavigationMenuSettingAccessId, 
		A.NavigationMenuId AS NavigationMenuId,
		ISNULL(OS.MappingValue, 0) AS MappingValue
	FROM Enterprise.NavigationMenuSettingAccess A
	LEFT JOIN Settings.OrganizationSettings OS ON OS.SettingCategoryTypeId = A.SettingCategoryTypeId AND OS.MappingName = A.MappingName
	 AND OS.PartyId = @PartyId
	 where  (OS.[MappingValue] = '0' OR OS.[MappingValue] IS NULL)
END