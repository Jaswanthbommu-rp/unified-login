CREATE TABLE [Enterprise].[StatusTypeCategory]
(
[StatusTypeCategoryId] [int] NOT NULL IDENTITY(1, 1),
[ParentStatusTypeCategoryId] [int] NULL,
[StatusTypeCategoryTypeId] [int] NOT NULL,
[Name] [varchar] (50) NOT NULL
)
GO
ALTER TABLE [Enterprise].[StatusTypeCategory] ADD CONSTRAINT [PK_StatusTypeCategory] PRIMARY KEY CLUSTERED  ([StatusTypeCategoryId])
GO
ALTER TABLE [Enterprise].[StatusTypeCategory] ADD CONSTRAINT [FK_ParentStatusTypeCategory_ChildStatusTypeCategory] FOREIGN KEY ([ParentStatusTypeCategoryId]) REFERENCES [Enterprise].[StatusTypeCategory] ([StatusTypeCategoryId])
GO
ALTER TABLE [Enterprise].[StatusTypeCategory] ADD CONSTRAINT [FK_StatusTypeCategory_StatusTypeCategoryType] FOREIGN KEY ([StatusTypeCategoryTypeId]) REFERENCES [Enterprise].[StatusTypeCategoryType] ([StatusTypeCategoryTypeId]) ON DELETE CASCADE ON UPDATE CASCADE
GO
