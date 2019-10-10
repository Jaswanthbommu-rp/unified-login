CREATE TABLE [Enterprise].[ProductSetting]
(
[ProductSettingId] [int] NOT NULL IDENTITY(1, 1),
[ProductId] [int] NOT NULL,
[ProductSettingTypeId] [int] NOT NULL,
[Value] [nvarchar] (1000) NOT NULL,
[FromDate] [datetime] NOT NULL CONSTRAINT [DF__ProductSe__FromD__078C1F06] DEFAULT (getutcdate()),
[ThruDate] [datetime] NULL
)
GO
ALTER TABLE [Enterprise].[ProductSetting] ADD CONSTRAINT [PK_ProductSetting] PRIMARY KEY CLUSTERED  ([ProductSettingId])
GO
CREATE NONCLUSTERED INDEX [IX_ProductSetting_ProductId_ProductSettingTypeId] ON [Enterprise].[ProductSetting] ([ProductId], [ProductSettingTypeId], [Value])
GO
ALTER TABLE [Enterprise].[ProductSetting] ADD CONSTRAINT [FK_ProductSetting_ProductSettingType] FOREIGN KEY ([ProductSettingTypeId]) REFERENCES [Enterprise].[ProductSettingType] ([ProductSettingTypeId]) ON DELETE CASCADE ON UPDATE CASCADE
GO
ALTER TABLE [Enterprise].[ProductSetting] ADD CONSTRAINT [FK_ProductSetting_ProductType] FOREIGN KEY ([ProductId]) REFERENCES [Enterprise].[Product] ([ProductId]) ON DELETE CASCADE ON UPDATE CASCADE
GO
