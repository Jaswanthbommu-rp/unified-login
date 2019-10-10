CREATE TABLE [Enterprise].[ProductConfiguration]
(
[ProductConfigurationId] [int] NOT NULL IDENTITY(1, 1),
[ConfigurationId] [int] NOT NULL,
[ProductSettingId] [int] NOT NULL,
[FromDate] [datetime] NOT NULL CONSTRAINT [DF__ProductCo__FromD__0C50D423] DEFAULT (getutcdate()),
[ThruDate] [datetime] NULL
)
GO
ALTER TABLE [Enterprise].[ProductConfiguration] ADD CONSTRAINT [PK_ProductConfiguration] PRIMARY KEY CLUSTERED  ([ProductConfigurationId])
GO
ALTER TABLE [Enterprise].[ProductConfiguration] ADD CONSTRAINT [FK_ProductConfiguration_Configuration] FOREIGN KEY ([ConfigurationId]) REFERENCES [Enterprise].[Configuration] ([ConfigurationId]) ON DELETE CASCADE ON UPDATE CASCADE
GO
ALTER TABLE [Enterprise].[ProductConfiguration] ADD CONSTRAINT [FK_ProductConfiguration_ProductSetting] FOREIGN KEY ([ProductSettingId]) REFERENCES [Enterprise].[ProductSetting] ([ProductSettingId]) ON DELETE CASCADE ON UPDATE CASCADE
GO
