CREATE TABLE [Enterprise].[RoleType]
(
	[PartyRoleTypeId] INT NOT NULL , 
    [ParentPartyRoleTypeId] INT NULL, 
    [Name] VARCHAR(50) NOT NULL, 
    CONSTRAINT [PK_RoleType] PRIMARY KEY ([PartyRoleTypeId]), 
    CONSTRAINT [AK_PartyRoleType_Name] UNIQUE ([Name]), 
    CONSTRAINT [FK_RoleType_ParentRoleType] FOREIGN KEY ([ParentPartyRoleTypeId]) REFERENCES [Enterprise].[RoleType] ([PartyRoleTypeId])
)

GO
