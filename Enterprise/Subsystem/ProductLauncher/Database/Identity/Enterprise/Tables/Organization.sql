CREATE TABLE [Enterprise].[Organization]
(
	[PartyId] BIGINT NOT NULL, 
    [Name] NVARCHAR(150) NULL, 
    [IdentityProviderTypeId] INT NOT NULL DEFAULT -1, 
	[OrganizationTypeId] [int] NOT NULL DEFAULT -1,
    [OrganizationDomainId] [int] NOT NULL DEFAULT 1,
	[CreateDate] [datetime] NULL,
	[ThruDate] Datetime2 NULL,
	[IsActive] TinyInt NOT NULL DEFAULT 1,
    CONSTRAINT [PK_Organization] PRIMARY KEY (PartyId), 
    CONSTRAINT [FK_Organization_Party] FOREIGN KEY (PartyId) REFERENCES Enterprise.Party(PartyId) ON DELETE CASCADE ON UPDATE CASCADE, 
    CONSTRAINT [FK_Organization_OrganizationDomain] FOREIGN KEY ([OrganizationDomainId]) REFERENCES [Enterprise].[OrganizationDomain] ([OrganizationDomainId])
)
GO
ALTER TABLE [Enterprise].[Organization]  WITH CHECK ADD  CONSTRAINT [FK_Organization_OrganizationType] FOREIGN KEY([OrganizationTypeId])
REFERENCES [Enterprise].[OrganizationType] ([OrganizationTypeId])
GO

CREATE NONCLUSTERED INDEX [IX_Organization_OrganizationDomainId]
ON [Enterprise].[Organization] ([OrganizationDomainId])
INCLUDE (PartyId)
GO

CREATE NONCLUSTERED INDEX [IX_Organization_Name]
ON [Enterprise].[Organization] ([Name])
INCLUDE ([OrganizationTypeId],[OrganizationDomainId],[IsActive])
GO

