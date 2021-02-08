Create PROCEDURE [Ident].[GetUnifiedSetting] (
	@PartyId bigint,
	@Category Varchar(50))
AS
BEGIN
	SET NOCOUNT ON;

	Select OS.MappingName AS 'Name',
		   OS.[MappingValue] AS 'Value',
		   OS.[Editable] AS 'Editable',
           OS.[Hidden] AS 'Hidden'
	From [Ident].[OrganizationSettings] OS
	JOIN [Ident].[SettingCategoryType] SCT ON
		OS.SettingCategoryTypeId = SCT.SettingCategoryTypeId
	WHERE OS.PartyId = @PartyId
	AND SCT.Name = @Category
END
