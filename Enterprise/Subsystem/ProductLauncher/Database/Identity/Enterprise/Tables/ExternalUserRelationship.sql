CREATE TABLE [Enterprise].[ExternalUserRelationship] (
    [UserLoginPersonaId]        BIGINT   NOT NULL PRIMARY KEY,
    [ThirdPartyRelationshipId]  TINYINT      NOT NULL,
    [CompanyName]               VARCHAR(200) NULL,
    [ThirdPartyCompanyPartyId]  BIGINT   NULL
    CONSTRAINT [FK_ExternalUserRelationship_UserLoginPersona] FOREIGN KEY ([UserLoginPersonaId]) REFERENCES [Ident].[UserLoginPersona] ([UserLoginPersonaId]) ON DELETE CASCADE ON UPDATE CASCADE,
	CONSTRAINT [FK_ExternalUserRelationship_ThirdPartyRelationship] FOREIGN KEY ([ThirdPartyRelationshipId]) REFERENCES [Enterprise].[ThirdPartyRelationship] ([ThirdPartyRelationshipId]),
    CONSTRAINT [FK_ExternalUserRelationship_Organization] FOREIGN KEY ([ThirdPartyCompanyPartyId]) REFERENCES [Enterprise].[Organization] ([PartyId]) 
)
