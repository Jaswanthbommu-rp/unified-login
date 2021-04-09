CREATE PROCEDURE [Settings].[GetSettingsMappingData](
	@Category varchar(50))
AS
Begin
	Select SCT.[Name] AS 'Category',
		   SM.SettingsMappingType AS 'MappingType'
	From settings.SettingsMapping SM
	Join settings.[SettingCategoryType] SCT ON
		SCT.[SettingCategoryTypeId] = SM.[SettingCategoryTypeId]
	Where [Name] = @Category
End