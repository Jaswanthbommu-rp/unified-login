CREATE TABLE [Enterprise].[RelationshipType]
(
	[RelationshipTypeId] INT NOT NULL IDENTITY,
	[RoleTypeIdValidFrom] INT NOT NULL,
	[RoleTypeIdValidTo] INT NOT NULL,
	[Name] VARCHAR(50) NOT NULL,
	[Description] VARCHAR(255) NULL, 
    CONSTRAINT [PK_PartyRelationshipType] PRIMARY KEY ([RelationshipTypeId]), 
    CONSTRAINT [FK_PartyRelationshipType_PartyRoleTypeFrom] FOREIGN KEY ([RoleTypeIdValidFrom]) REFERENCES [Enterprise].[RoleType](PartyRoleTypeId) , 
    CONSTRAINT [FK_PartyRelationshipType_PartyRoleTypeTo] FOREIGN KEY ([RoleTypeIdValidTo]) REFERENCES [Enterprise].[RoleType](PartyRoleTypeId) )
