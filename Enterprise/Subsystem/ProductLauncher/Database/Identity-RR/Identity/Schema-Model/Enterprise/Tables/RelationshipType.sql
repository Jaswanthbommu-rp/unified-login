CREATE TABLE [Enterprise].[RelationshipType]
(
[RelationshipTypeId] [int] NOT NULL IDENTITY(1, 1),
[RoleTypeIdValidFrom] [int] NOT NULL,
[RoleTypeIdValidTo] [int] NOT NULL,
[Name] [varchar] (50) NOT NULL,
[Description] [varchar] (255) NULL
)
GO
ALTER TABLE [Enterprise].[RelationshipType] ADD CONSTRAINT [PK_PartyRelationshipType] PRIMARY KEY CLUSTERED  ([RelationshipTypeId])
GO
ALTER TABLE [Enterprise].[RelationshipType] ADD CONSTRAINT [FK_PartyRelationshipType_PartyRoleTypeFrom] FOREIGN KEY ([RoleTypeIdValidFrom]) REFERENCES [Enterprise].[RoleType] ([PartyRoleTypeId])
GO
ALTER TABLE [Enterprise].[RelationshipType] ADD CONSTRAINT [FK_PartyRelationshipType_PartyRoleTypeTo] FOREIGN KEY ([RoleTypeIdValidTo]) REFERENCES [Enterprise].[RoleType] ([PartyRoleTypeId])
GO
