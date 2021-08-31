CREATE TABLE [Hots].[PropertyRelationship]
(
	[PropertyRelationshipId] BIGINT IDENTITY (1, 1) NOT NULL, 
	[BasePropertyInstanceId] BIGINT NOT NULL,
	[ClonePropertyInstanceId] BIGINT NOT NULL,
	[CloneCompanyPartyId] BIGINT NOT NULL,
	[CreateDate]          DATETIME   DEFAULT (getutcdate()) NOT NULL,
    [CreatedBy] BIGINT NOT NULL,
	CONSTRAINT [PK_PropertyRelationship] PRIMARY KEY CLUSTERED ([PropertyRelationshipId] ASC)
)
GO

CREATE INDEX [IX_PropertyRelationship_CloneCompanyPartyId] ON [Hots].[PropertyRelationship] ([CloneCompanyPartyId]) INCLUDE ([ClonePropertyInstanceId])
GO
