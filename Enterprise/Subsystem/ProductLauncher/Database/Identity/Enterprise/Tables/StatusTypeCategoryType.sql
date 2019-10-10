CREATE TABLE [Enterprise].[StatusTypeCategoryType]
(
	[StatusTypeCategoryTypeId] INT NOT NULL IDENTITY,
	[ParentStatusTypeCategoryTypeId] INT NULL,
	[Name] VARCHAR(50) NOT NULL, 
    CONSTRAINT [PK_StatusTypeCategoryType] PRIMARY KEY ([StatusTypeCategoryTypeId]), 
    CONSTRAINT [FK_ParentStatusTypeCategoryType_ChildStatusTypeCategoryType] FOREIGN KEY ([ParentStatusTypeCategoryTypeId]) REFERENCES [Enterprise].[StatusTypeCategoryType]([StatusTypeCategoryTypeId])
)
