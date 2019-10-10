CREATE TABLE [Person].[PersonaType]
(
[PersonaTypeId] [int] NOT NULL IDENTITY(1, 1),
[Name] [varchar] (50) NULL
)
GO
ALTER TABLE [Person].[PersonaType] ADD CONSTRAINT [PK_PersonaType] PRIMARY KEY CLUSTERED  ([PersonaTypeId])
GO
