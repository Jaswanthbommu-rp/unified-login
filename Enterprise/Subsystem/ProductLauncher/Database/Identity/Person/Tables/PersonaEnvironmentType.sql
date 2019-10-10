CREATE TABLE [Person].[PersonaEnvironmentType] (
    [PersonaEnvironmentTypeID] INT           IDENTITY (1, 1) NOT NULL,
    [Name]                     NVARCHAR (50) NOT NULL,
    CONSTRAINT [PK_PersonaEnvironmentType] PRIMARY KEY CLUSTERED ([PersonaEnvironmentTypeID] ASC)
);

