CREATE TABLE [Settings].[SettingTableRow]
(
	[SettingTableRowId] BIGINT IDENTITY(1,1) NOT NULL,
	[SettingTableId] BIGINT  NOT NULL,	
	[Editable] BIT NOT NULL DEFAULT 0,
	[Deletable] BIT NOT NULL DEFAULT 0,
	[IsActive] BIT NOT NULL DEFAULT 0,
	[ModifiedBy] BIGINT NOT NULL,
	[CreatedDate] DATETIME    DEFAULT (getutcdate()) NOT NULL, 
	[UpdatedDate] DATETIME NULL,
	CONSTRAINT [PK_SettingTableRow] PRIMARY KEY ([SettingTableRowId]), 
	CONSTRAINT [FK_SettingTableRow_SettingTableId] FOREIGN KEY([SettingTableId]) REFERENCES [Settings].[SettingTable] ([SettingTableId]) ON UPDATE CASCADE ON DELETE CASCADE
)
GO
CREATE INDEX [IX_SettingTableRow_SettingTableId]
ON [Settings].[SettingTableRow]
( [SettingTableId],[SettingTableRowId] );