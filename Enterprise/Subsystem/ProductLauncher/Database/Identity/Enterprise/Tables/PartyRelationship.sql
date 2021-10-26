CREATE TABLE [Enterprise].[PartyRelationship] (
    [PartyRelationshipId]     BIGINT   IDENTITY (1, 1) NOT NULL,
    [PartyIdFrom]             BIGINT   NOT NULL,
    [PartyIdTo]               BIGINT   NOT NULL,
    [RoleTypeIdFrom]          INT      NOT NULL,
    [RoleTypeIdTo]            INT      NOT NULL,
    [PartyRelationshipTypeId] INT      NOT NULL,
    [FromDate]                DATETIME DEFAULT (getutcdate()) NOT NULL,
    [ThruDate]                DATETIME NULL,
    CONSTRAINT [PK_PartyRelationship] PRIMARY KEY CLUSTERED ([PartyRelationshipId] ASC) WITH (FILLFACTOR = 80),
    CONSTRAINT [FK_PartyRelationship_PartyFrom] FOREIGN KEY ([PartyIdFrom]) REFERENCES [Enterprise].[Party] ([PartyId]) ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT [FK_PartyRelationship_PartyRelationshipType] FOREIGN KEY ([PartyRelationshipTypeId]) REFERENCES [Enterprise].[RelationshipType] ([RelationshipTypeId]) ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT [FK_PartyRelationship_PartyTo] FOREIGN KEY ([PartyIdTo]) REFERENCES [Enterprise].[Party] ([PartyId])
);


GO
CREATE NONCLUSTERED INDEX IDX_PartyRelationShip_COmp01 ON [Enterprise].[PartyRelationship]
(
	[PartyIdFrom] ASC,
	[PartyRelationshipTypeId] ASC,
	[FromDate] ASC,
	[PartyIdTo] ASC,
	[ThruDate] ASC,
	[RoleTypeIdFrom] ASC,
	[PartyRelationshipId] ASC
)WITH (SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF) ON [PRIMARY]
go
CREATE NONCLUSTERED INDEX IDX_PartyRelationship_Comp02 ON [Enterprise].[PartyRelationship]
(
	[FromDate] ASC,
	[PartyIdTo] ASC,
	[ThruDate] ASC,
	[PartyRelationshipTypeId] ASC,
	[PartyIdFrom] ASC,
	[RoleTypeIdFrom] ASC,
	[PartyRelationshipId] ASC
)WITH (SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF) ON [PRIMARY]
go


CREATE NONCLUSTERED INDEX [IX_Enterprise_PartyRelationship_PartyIdTo_PartyRelationshipTypeId]
    ON [Enterprise].[PartyRelationship]([PartyIdTo] ASC, [PartyRelationshipTypeId] ASC, [FromDate] ASC, [ThruDate] ASC)
    INCLUDE([PartyIdFrom], [RoleTypeIdFrom]);

