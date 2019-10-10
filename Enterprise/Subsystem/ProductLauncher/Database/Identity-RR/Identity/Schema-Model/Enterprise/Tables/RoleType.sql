CREATE TABLE [Enterprise].[RoleType]
(
[PartyRoleTypeId] [int] NOT NULL,
[ParentPartyRoleTypeId] [int] NULL,
[Name] [varchar] (50) NOT NULL
)
GO
ALTER TABLE [Enterprise].[RoleType] ADD CONSTRAINT [PK_RoleType] PRIMARY KEY CLUSTERED  ([PartyRoleTypeId])
GO
ALTER TABLE [Enterprise].[RoleType] ADD CONSTRAINT [AK_PartyRoleType_Name] UNIQUE NONCLUSTERED  ([Name])
GO
ALTER TABLE [Enterprise].[RoleType] ADD CONSTRAINT [FK_RoleType_ParentRoleType] FOREIGN KEY ([ParentPartyRoleTypeId]) REFERENCES [Enterprise].[RoleType] ([PartyRoleTypeId])
GO
