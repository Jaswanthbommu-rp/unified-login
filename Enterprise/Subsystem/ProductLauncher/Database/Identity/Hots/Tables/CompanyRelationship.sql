CREATE TABLE [Hots].[CompanyRelationship]
(
	[CompanyRelationshipId] BIGINT IDENTITY (1, 1) NOT NULL, 
    [BaseLineCompanyPartyId] BIGINT NOT NULL, 
    [CloneCompanyPartyId] BIGINT NOT NULL,
    [CreateDate]          DATETIME   DEFAULT (getutcdate()) NOT NULL,
    [CreatedBy] BIGINT NOT NULL,
    CONSTRAINT [PK_CompanyRelationship] PRIMARY KEY CLUSTERED ([CompanyRelationshipId] ASC),
    CONSTRAINT [FK_CompanyRelationship_BaseParty] FOREIGN KEY ([BaseLineCompanyPartyId]) REFERENCES [Enterprise].[Organization] ([PartyId]),
    CONSTRAINT [FK_CompanyRelationship_CloneParty] FOREIGN KEY ([CloneCompanyPartyId]) REFERENCES [Enterprise].[Organization] ([PartyId])
);
GO
CREATE INDEX [IX_CompanyRelationship_Id] ON [Hots].[CompanyRelationship] ([CompanyRelationshipId], [BaseLineCompanyPartyId],[CloneCompanyPartyId])
