CREATE TABLE [Enterprise].[StatusTypeCategoryClassification]
(
	[StatusTypeCategoryClassificationId] INT NOT NULL IDENTITY,
	[StatusTypeId] INT NOT NULL,
	[StatusTypeCategoryId] INT NOT NULL,
	[FromDate] DATETIME NOT NULL DEFAULT GETUTCDATE(),
	[ThruDate] DATETIME NULL, 
    CONSTRAINT [PK_StatusTypeCategoryClassification] PRIMARY KEY (StatusTypeCategoryClassificationId), 
    CONSTRAINT [FK_StatusTypeCategoryClassification_StatusType] FOREIGN KEY (StatusTypeId) REFERENCES [Enterprise].[StatusType](StatusTypeId) ON DELETE CASCADE ON UPDATE CASCADE, 
    CONSTRAINT [FK_StatusTypeCategoryClassification_StatusTypeCategory] FOREIGN KEY ([StatusTypeCategoryId]) REFERENCES [Enterprise].[StatusTypeCategory]([StatusTypeCategoryId]) ON DELETE CASCADE ON UPDATE CASCADE
)
