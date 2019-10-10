CREATE TABLE [Enterprise].[StatusTypeCategoryClassification]
(
[StatusTypeCategoryClassificationId] [int] NOT NULL IDENTITY(1, 1),
[StatusTypeId] [int] NOT NULL,
[StatusTypeCategoryId] [int] NOT NULL,
[FromDate] [datetime] NOT NULL CONSTRAINT [DF__StatusTyp__FromD__03BB8E22] DEFAULT (getutcdate()),
[ThruDate] [datetime] NULL
)
GO
ALTER TABLE [Enterprise].[StatusTypeCategoryClassification] ADD CONSTRAINT [PK_StatusTypeCategoryClassification] PRIMARY KEY CLUSTERED  ([StatusTypeCategoryClassificationId])
GO
ALTER TABLE [Enterprise].[StatusTypeCategoryClassification] ADD CONSTRAINT [FK_StatusTypeCategoryClassification_StatusType] FOREIGN KEY ([StatusTypeId]) REFERENCES [Enterprise].[StatusType] ([StatusTypeId]) ON DELETE CASCADE ON UPDATE CASCADE
GO
ALTER TABLE [Enterprise].[StatusTypeCategoryClassification] ADD CONSTRAINT [FK_StatusTypeCategoryClassification_StatusTypeCategory] FOREIGN KEY ([StatusTypeCategoryId]) REFERENCES [Enterprise].[StatusTypeCategory] ([StatusTypeCategoryId]) ON DELETE CASCADE ON UPDATE CASCADE
GO
