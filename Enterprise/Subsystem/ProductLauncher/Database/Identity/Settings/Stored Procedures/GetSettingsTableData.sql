CREATE PROCEDURE [Settings].[GetSettingsTableData] (
	@PartyId bigint,
	@Category nvarchar(100))
AS
Begin
	Select [TableName] AS 'Name',
		   [SettingTableId] AS 'Id'
	From settings.SettingTable ST
	Join settings.[SettingCategoryType] SCT ON
		SCT.[SettingCategoryTypeId] = ST.[SettingCategoryTypeId]
	Where [Name] = @Category
	AND [PartyId] = @PartyId
End