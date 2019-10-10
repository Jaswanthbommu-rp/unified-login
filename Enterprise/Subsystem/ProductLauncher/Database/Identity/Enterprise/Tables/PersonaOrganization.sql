CREATE TABLE [Enterprise].[PersonaOrganization]
(
	[PersonaOrganizationId] BIGINT NOT NULL IDENTITY,
	[PersonaConfigurationId] BIGINT NOT NULL,
	[OrganizationId] BIGINT NOT NULL,
	[FromDate] DATETIME NOT NULL DEFAULT GETUTCDATE(),
	[ThruDate] DATETIME NULL, 
    CONSTRAINT [PK_PersonaOrganization] PRIMARY KEY ([PersonaOrganizationId]), 
    CONSTRAINT [FK_PersonaOrganization_PersonaConfiguration] FOREIGN KEY ([PersonaConfigurationId]) REFERENCES [Enterprise].[PersonaConfiguration]([PersonaConfigurationId]) ON DELETE CASCADE, 
    CONSTRAINT [FK_PersonaOrganization_Organization] FOREIGN KEY ([OrganizationId]) REFERENCES [Enterprise].[Organization](PartyId)
)
