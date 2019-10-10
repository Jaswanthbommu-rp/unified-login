CREATE TABLE [Enterprise].[StatusTypeCategory]
(
	[StatusTypeCategoryId] INT NOT NULL IDENTITY,
	[ParentStatusTypeCategoryId] INT NULL,
	[StatusTypeCategoryTypeId] INT NOT NULL,
	[Name] VARCHAR(50) NOT NULL, 
    CONSTRAINT [PK_StatusTypeCategory] PRIMARY KEY (StatusTypeCategoryId), 
    CONSTRAINT [FK_ParentStatusTypeCategory_ChildStatusTypeCategory] FOREIGN KEY (ParentStatusTypeCategoryId) REFERENCES [Enterprise].[StatusTypeCategory](StatusTypeCategoryId), 
    CONSTRAINT [FK_StatusTypeCategory_StatusTypeCategoryType] FOREIGN KEY ([StatusTypeCategoryTypeId]) REFERENCES [Enterprise].[StatusTypeCategoryType]([StatusTypeCategoryTypeId]) ON DELETE CASCADE ON UPDATE CASCADE
)
