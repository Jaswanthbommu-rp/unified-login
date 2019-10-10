CREATE TABLE [Enterprise].[PersonaOrganization]
(
[PersonaOrganizationId] [bigint] NOT NULL IDENTITY(1, 1),
[PersonaConfigurationId] [bigint] NOT NULL,
[OrganizationId] [bigint] NOT NULL,
[FromDate] [datetime] NOT NULL CONSTRAINT [DF__PersonaOr__FromD__09746778] DEFAULT (getutcdate()),
[ThruDate] [datetime] NULL
)
GO
ALTER TABLE [Enterprise].[PersonaOrganization] ADD CONSTRAINT [PK_PersonaOrganization] PRIMARY KEY CLUSTERED  ([PersonaOrganizationId])
GO
ALTER TABLE [Enterprise].[PersonaOrganization] ADD CONSTRAINT [FK_PersonaOrganization_Organization] FOREIGN KEY ([OrganizationId]) REFERENCES [Enterprise].[Organization] ([PartyId])
GO
ALTER TABLE [Enterprise].[PersonaOrganization] ADD CONSTRAINT [FK_PersonaOrganization_PersonaConfiguration] FOREIGN KEY ([PersonaConfigurationId]) REFERENCES [Enterprise].[PersonaConfiguration] ([PersonaConfigurationId]) ON DELETE CASCADE
GO
