CREATE PROCEDURE [Settings].[GetSettingsTableRowsData] (
	@SettingTableId bigint)
AS
Begin

	Select [SettingTableRowId] AS 'Id',
			[Editable],
			[Deletable]	
	FROM [Settings].[SettingTableRow] SR 
	JOIN [Settings].[SettingTable] ST ON
		SR.[SettingTableId] = ST.[SettingTableId]	
	Where SR.[SettingTableId] =  @SettingTableId
	AND [IsActive] = 1

END