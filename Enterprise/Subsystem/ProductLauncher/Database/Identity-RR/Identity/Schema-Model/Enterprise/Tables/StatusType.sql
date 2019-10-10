CREATE TABLE [Enterprise].[StatusType]
(
[StatusTypeId] [int] NOT NULL,
[Name] [varchar] (50) NOT NULL
)
GO
ALTER TABLE [Enterprise].[StatusType] ADD CONSTRAINT [PK_StatusType] PRIMARY KEY CLUSTERED  ([StatusTypeId])
GO
