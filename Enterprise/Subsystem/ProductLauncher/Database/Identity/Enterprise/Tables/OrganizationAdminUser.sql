CREATE TABLE [Enterprise].[OrganizationAdminUser]
(
	[OrganizationAdminUserId] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
	[OrganizationPartyId] BIGINT NOT NULL,
	[UserLoginPersonaId] BIGINT NOT NULL
)
GO

ALTER TABLE [Enterprise].[OrganizationAdminUser]  WITH CHECK ADD CONSTRAINT [FK_OrganizationAdminUser_OrganizationPartyId] FOREIGN KEY([OrganizationPartyId])
REFERENCES [Enterprise].[Organization] ([PartyId])
GO

ALTER TABLE [Enterprise].[OrganizationAdminUser]  WITH CHECK ADD CONSTRAINT [FK_OrganizationAdminUser_UserLoginPersonaId] FOREIGN KEY([UserLoginPersonaId])
REFERENCES [Ident].[UserLoginPersona] ([UserLoginPersonaId])
GO

CREATE NONCLUSTERED INDEX [IDX_OrganizationAdminUser_OrganizationPartyId]
ON [Enterprise].[OrganizationAdminUser] ([OrganizationPartyId])
