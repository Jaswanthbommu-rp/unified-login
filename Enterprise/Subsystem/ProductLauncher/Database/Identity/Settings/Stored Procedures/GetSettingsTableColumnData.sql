CREATE PROCEDURE [Settings].[GetSettingsTableColumnData] (
	@SettingTableRowId bigint)
AS
Begin

	Select [TableColumnName] AS 'Name',
	[TableColumnValue] AS 'Value'
	FROM [Settings].[SettingTableColumn] SC
	Join [Settings].[SettingTableRow] SR ON
		SR.[SettingTableRowId] = SC.[SettingTableRowId]	
	Where SR.[SettingTableRowId] =  @SettingTableRowId

END
