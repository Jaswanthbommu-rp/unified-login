CREATE TABLE [Settings].[SettingsMapping]
(
	[SettingsMappingId] INT IDENTITY(1,1) NOT NULL,
	[SettingCategoryTypeId] SMALLINT NOT NULL,
	[SettingsMappingType] varchar(20) NOT NULL,
	[ModifiedBy] BIGINT NOT NULL,
	[CreatedDate] DATETIME   DEFAULT (getdate()) NOT NULL, 
	[UpdatedDate] DATETIME NULL,
	CONSTRAINT [PK_SettingsMapping] PRIMARY KEY ([SettingsMappingId]), 
    CONSTRAINT [FK_SettingsMapping_Category] FOREIGN KEY ([SettingCategoryTypeId]) REFERENCES [Settings].[SettingCategoryType]([SettingCategoryTypeId]) ON DELETE CASCADE ON UPDATE CASCADE
)
