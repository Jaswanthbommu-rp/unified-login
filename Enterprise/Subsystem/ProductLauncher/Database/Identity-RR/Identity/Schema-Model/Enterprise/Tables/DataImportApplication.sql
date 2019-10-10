CREATE TABLE [Enterprise].[DataImportApplication]
(
[DataImportApplicationId] [int] NOT NULL IDENTITY(1, 1),
[Name] [nvarchar] (50) NOT NULL,
[Description] [nvarchar] (100) NULL
)
GO
ALTER TABLE [Enterprise].[DataImportApplication] ADD CONSTRAINT [PK_DataImportApplication] PRIMARY KEY CLUSTERED  ([DataImportApplicationId])
GO
