CREATE TABLE Enterprise.DataImportApplication
(
	[DataImportApplicationId] INT IDENTITY NOT NULL,
	[Name] NVARCHAR(50) NOT NULL,
	[Description] NVARCHAR(100) NULL, 
    CONSTRAINT [PK_DataImportApplication] PRIMARY KEY ([DataImportApplicationId])
)
GO
CREATE INDEX IDX_DataImportApplication ON Enterprise.DataImportApplication(DataImportApplicationId) INCLUDE (Name) 
