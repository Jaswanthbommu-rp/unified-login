CREATE TABLE [Enterprise].[ProductSettingType]
(
	[ProductSettingTypeId] INT NOT NULL IDENTITY, 
    [Name] NVARCHAR(50) NOT NULL, 
    [Description] NVARCHAR(100) NULL, 
    CONSTRAINT [PK_ProductSettingType] PRIMARY KEY ([ProductSettingTypeId])
)
