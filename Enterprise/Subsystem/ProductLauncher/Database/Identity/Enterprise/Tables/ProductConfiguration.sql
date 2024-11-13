CREATE TABLE [Enterprise].[ProductConfiguration]
(
	[ProductConfigurationId] INT NOT NULL IDENTITY, 
    [ConfigurationId] INT NOT NULL, 
    [ProductSettingId] INT NOT NULL, 
    [FromDate] DATETIME NOT NULL DEFAULT GETUTCDATE(), 
    [ThruDate] DATETIME NULL, 
    CONSTRAINT [PK_ProductConfiguration] PRIMARY KEY ([ProductConfigurationId]), 
    CONSTRAINT [FK_ProductConfiguration_ProductSetting] FOREIGN KEY ([ProductSettingId]) REFERENCES [Enterprise].[ProductSetting]([ProductSettingId]) ON DELETE CASCADE ON UPDATE CASCADE, 
    CONSTRAINT [FK_ProductConfiguration_Configuration] FOREIGN KEY ([ConfigurationId]) REFERENCES [Enterprise].[Configuration]([ConfigurationId]) ON DELETE CASCADE ON UPDATE CASCADE
)
GO
CREATE NONCLUSTERED INDEX IDX_ProductConfiguration ON [Enterprise].[ProductConfiguration]
(
	[ProductSettingId] ASC,
	[ThruDate] ASC,
	[ConfigurationId] ASC,
	[FromDate] ASC
)WITH (SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF) ON [PRIMARY]
go
CREATE NONCLUSTERED INDEX IDX_ProductConfiguration_Comp02 ON [Enterprise].[ProductConfiguration]
(
	[ConfigurationId] ASC,
	[ThruDate] ASC,
	[ProductSettingId] ASC,
	[FromDate] ASC
)WITH (SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IX_ProductConfiguration_Comp3] ON [Enterprise].[ProductConfiguration]
(
	[ThruDate] ASC,
	[ProductSettingId] ASC,
	[ConfigurationId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO