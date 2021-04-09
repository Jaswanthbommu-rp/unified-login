CREATE TABLE [Settings].[SettingTable]
(
	[SettingTableId] BIGINT IDENTITY(1,1) NOT NULL,
	[SettingCategoryTypeId] SMALLINT NOT NULL,
	[PartyId] BIGINT NOT NULL,	
	[TableName] nvarchar(100) NOT NULL,
	[ModifiedBy] BIGINT NOT NULL,
	[CreatedDate] DATETIME     DEFAULT (getdate()) NOT NULL, 
	[UpdatedDate] DATETIME NULL,
	CONSTRAINT [PK_SettingTable] PRIMARY KEY ([SettingTableId]), 
    CONSTRAINT [FK_SettingTable_Category] FOREIGN KEY ([SettingCategoryTypeId]) REFERENCES [Settings].[SettingCategoryType]([SettingCategoryTypeId]) ON DELETE CASCADE ON UPDATE CASCADE,
	CONSTRAINT [FK_SettingTable_Party] FOREIGN KEY([PartyId]) REFERENCES [Enterprise].[Organization] ([PartyId]) ON UPDATE CASCADE ON DELETE CASCADE
)
GO
CREATE INDEX [IX_SettingTable_PartyId]
ON [Settings].[SettingTable]
( [SettingTableId],[PartyId], [SettingCategoryTypeId] );