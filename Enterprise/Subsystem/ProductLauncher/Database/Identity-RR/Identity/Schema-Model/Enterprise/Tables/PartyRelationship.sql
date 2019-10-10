CREATE TABLE [Enterprise].[PartyRelationship]
(
[PartyRelationshipId] [bigint] NOT NULL IDENTITY(1, 1),
[PartyIdFrom] [bigint] NOT NULL,
[PartyIdTo] [bigint] NOT NULL,
[RoleTypeIdFrom] [int] NOT NULL,
[RoleTypeIdTo] [int] NOT NULL,
[PartyRelationshipTypeId] [int] NOT NULL,
[FromDate] [datetime] NOT NULL CONSTRAINT [DF__PartyRela__FromD__05A3D694] DEFAULT (getutcdate()),
[ThruDate] [datetime] NULL
)
GO
ALTER TABLE [Enterprise].[PartyRelationship] ADD CONSTRAINT [PK_PartyRelationship] PRIMARY KEY CLUSTERED  ([PartyRelationshipId])
GO
ALTER TABLE [Enterprise].[PartyRelationship] ADD CONSTRAINT [FK_PartyRelationship_PartyFrom] FOREIGN KEY ([PartyIdFrom]) REFERENCES [Enterprise].[Party] ([PartyId]) ON DELETE CASCADE ON UPDATE CASCADE
GO
ALTER TABLE [Enterprise].[PartyRelationship] ADD CONSTRAINT [FK_PartyRelationship_PartyRelationshipType] FOREIGN KEY ([PartyRelationshipTypeId]) REFERENCES [Enterprise].[RelationshipType] ([RelationshipTypeId]) ON DELETE CASCADE ON UPDATE CASCADE
GO
ALTER TABLE [Enterprise].[PartyRelationship] ADD CONSTRAINT [FK_PartyRelationship_PartyTo] FOREIGN KEY ([PartyIdTo]) REFERENCES [Enterprise].[Party] ([PartyId])
GO
