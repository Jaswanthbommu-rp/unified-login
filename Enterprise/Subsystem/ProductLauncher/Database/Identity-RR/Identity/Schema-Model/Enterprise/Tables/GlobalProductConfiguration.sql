CREATE TABLE [Enterprise].[GlobalProductConfiguration]
(
[GlobalProductConfigurationId] [int] NOT NULL IDENTITY(1, 1),
[ConfigurationId] [int] NOT NULL,
[ProductId] [int] NOT NULL,
[FromDate] [datetime] NOT NULL CONSTRAINT [DF__GlobalPro__FromD__5555A4F4] DEFAULT (getutcdate()),
[ThruDate] [datetime] NULL
)
GO
ALTER TABLE [Enterprise].[GlobalProductConfiguration] ADD CONSTRAINT [PK_GlobalProductConfiguration] PRIMARY KEY CLUSTERED  ([GlobalProductConfigurationId])
GO
ALTER TABLE [Enterprise].[GlobalProductConfiguration] ADD CONSTRAINT [AK_GlobalProductConfiguration_ConfigurationId_ProductId_ThruDate] UNIQUE NONCLUSTERED  ([ConfigurationId], [ProductId], [ThruDate])
GO
ALTER TABLE [Enterprise].[GlobalProductConfiguration] ADD CONSTRAINT [FK_GlobalProductConfiguration_Configuration] FOREIGN KEY ([ConfigurationId]) REFERENCES [Enterprise].[Configuration] ([ConfigurationId]) ON DELETE CASCADE ON UPDATE CASCADE
GO
ALTER TABLE [Enterprise].[GlobalProductConfiguration] ADD CONSTRAINT [FK_GlobalProductConfiguration_Product] FOREIGN KEY ([ProductId]) REFERENCES [Enterprise].[Product] ([ProductId]) ON DELETE CASCADE ON UPDATE CASCADE
GO
