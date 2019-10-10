CREATE TABLE [Enterprise].[PersonaConfiguration]
(
[PersonaConfigurationId] [bigint] NOT NULL IDENTITY(1, 1),
[PersonaId] [bigint] NOT NULL,
[ConfigurationId] [int] NOT NULL,
[ProductId] [int] NOT NULL,
[FromDate] [datetime] NOT NULL CONSTRAINT [DF__PersonaCo__FromD__0B5CAFEA] DEFAULT (getutcdate()),
[ThruDate] [datetime] NULL
)
GO
ALTER TABLE [Enterprise].[PersonaConfiguration] ADD CONSTRAINT [PK_PersonaConfiguration] PRIMARY KEY CLUSTERED  ([PersonaConfigurationId])
GO
ALTER TABLE [Enterprise].[PersonaConfiguration] ADD CONSTRAINT [FK_PersonaConfiguration_Configuration] FOREIGN KEY ([ConfigurationId]) REFERENCES [Enterprise].[Configuration] ([ConfigurationId]) ON DELETE CASCADE
GO
ALTER TABLE [Enterprise].[PersonaConfiguration] ADD CONSTRAINT [FK_PersonaConfiguration_Persona] FOREIGN KEY ([PersonaId]) REFERENCES [Person].[Persona] ([PersonaId]) ON DELETE CASCADE
GO
