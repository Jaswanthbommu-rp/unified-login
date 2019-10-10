CREATE TABLE [Enterprise].[ProductSettingType]
(
[ProductSettingTypeId] [int] NOT NULL IDENTITY(1, 1),
[Name] [nvarchar] (50) NOT NULL,
[Description] [nvarchar] (100) NULL
)
GO
ALTER TABLE [Enterprise].[ProductSettingType] ADD CONSTRAINT [PK_ProductSettingType] PRIMARY KEY CLUSTERED  ([ProductSettingTypeId])
GO
