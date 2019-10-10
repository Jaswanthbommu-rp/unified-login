CREATE TABLE [Enterprise].[StatusTypeCategoryType]
(
[StatusTypeCategoryTypeId] [int] NOT NULL IDENTITY(1, 1),
[ParentStatusTypeCategoryTypeId] [int] NULL,
[Name] [varchar] (50) NOT NULL
)
GO
ALTER TABLE [Enterprise].[StatusTypeCategoryType] ADD CONSTRAINT [PK_StatusTypeCategoryType] PRIMARY KEY CLUSTERED  ([StatusTypeCategoryTypeId])
GO
ALTER TABLE [Enterprise].[StatusTypeCategoryType] ADD CONSTRAINT [FK_ParentStatusTypeCategoryType_ChildStatusTypeCategoryType] FOREIGN KEY ([ParentStatusTypeCategoryTypeId]) REFERENCES [Enterprise].[StatusTypeCategoryType] ([StatusTypeCategoryTypeId])
GO
