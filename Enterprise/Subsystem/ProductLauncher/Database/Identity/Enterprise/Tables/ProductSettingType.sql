CREATE TABLE [Enterprise].[ProductSettingType]
(
	[ProductSettingTypeId] INT NOT NULL IDENTITY, 
    [Name] NVARCHAR(50) NOT NULL, 
    [Description] NVARCHAR(100) NULL, 
    [SensitiveData] TINYINT NOT NULL DEFAULT 0, 
    CONSTRAINT [PK_ProductSettingType] PRIMARY KEY ([ProductSettingTypeId])
)
