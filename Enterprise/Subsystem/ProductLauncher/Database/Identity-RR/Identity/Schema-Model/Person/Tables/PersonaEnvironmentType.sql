CREATE TABLE [Person].[PersonaEnvironmentType]
(
[PersonaEnvironmentTypeID] [int] NOT NULL IDENTITY(1, 1),
[Name] [nvarchar] (50) NOT NULL
)
GO
ALTER TABLE [Person].[PersonaEnvironmentType] ADD CONSTRAINT [PK_PersonaEnvironmentType] PRIMARY KEY CLUSTERED  ([PersonaEnvironmentTypeID])
GO
