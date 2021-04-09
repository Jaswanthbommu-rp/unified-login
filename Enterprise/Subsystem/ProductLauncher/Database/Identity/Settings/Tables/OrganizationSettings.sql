CREATE TABLE [Settings].[OrganizationSettings]
(
	[OrganizationSettingsId] BIGINT NOT NULL IDENTITY, 
    [PartyId] BIGINT NOT NULL, 
    [SettingCategoryTypeId] SMALLINT NOT NULL, 
    [MappingName] NVARCHAR(200) NOT NULL, 
    [MappingValue] NVARCHAR(100) NOT NULL, 
    [Editable] BIT NULL DEFAULT 1, 
    [Hidden] BIT NULL DEFAULT 0,
    [CreatedBy]       BIGINT         NOT NULL,
    [CreatedDate]     DATETIME       CONSTRAINT [DF_Control_CreatedDate] DEFAULT (getdate()) NOT NULL, 
    [UpdatedDate] DATETIME NULL,
    CONSTRAINT [PK_OrganizationSettings] PRIMARY KEY ([OrganizationSettingsId]), 
    CONSTRAINT [FK_OrganizationSettings_Category] FOREIGN KEY ([SettingCategoryTypeId]) REFERENCES [Settings].[SettingCategoryType]([SettingCategoryTypeId]) ON DELETE CASCADE ON UPDATE CASCADE,
	CONSTRAINT [FK_OrganizationSettings_Party] FOREIGN KEY([PartyId]) REFERENCES [Enterprise].[Organization] ([PartyId]) ON UPDATE CASCADE ON DELETE CASCADE
)
GO
CREATE INDEX [IX_OrganizationSettings_PartyId]
ON [Settings].[OrganizationSettings]
( [OrganizationSettingsId],[PartyId], [SettingCategoryTypeId], [MappingName] );