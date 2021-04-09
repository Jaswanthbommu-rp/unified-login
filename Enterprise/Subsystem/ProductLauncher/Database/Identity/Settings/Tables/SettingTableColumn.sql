CREATE TABLE [Settings].[SettingTableColumn]
(
	[SettingTableColumnId] BIGINT IDENTITY(1,1) NOT NULL,
	[SettingTableRowId] BIGINT NOT NULL,
	[TableColumnName] nvarchar(200) NOT NULL,	
	[TableColumnValue] nvarchar(200) NOT NULL,		
	[ModifiedBy] BIGINT NOT NULL,
	[CreatedDate] DATETIME    DEFAULT (getdate()) NOT NULL, 
	[UpdatedDate] DATETIME NULL,
	CONSTRAINT [PK_SettingTableColumn] PRIMARY KEY ([SettingTableColumnId]), 
	CONSTRAINT [FK_SettingTableColumn_SSettingTableRowId] FOREIGN KEY([SettingTableRowId]) REFERENCES [Settings].[SettingTableRow] ([SettingTableRowId]) ON UPDATE CASCADE ON DELETE CASCADE
)
GO
CREATE INDEX [IX_SettingTableColumn_SettingTableRowId]
ON [Settings].[SettingTableColumn] ( [SettingTableColumnId],[SettingTableRowId]);
