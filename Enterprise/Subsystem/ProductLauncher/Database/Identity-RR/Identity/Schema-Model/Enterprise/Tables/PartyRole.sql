CREATE TABLE [Enterprise].[PartyRole]
(
[PartyRoleId] [int] NOT NULL IDENTITY(1, 1),
[PartyId] [bigint] NOT NULL,
[RoleTypeId] [int] NOT NULL
)
GO
ALTER TABLE [Enterprise].[PartyRole] ADD CONSTRAINT [PK_PartyRole] PRIMARY KEY CLUSTERED  ([PartyRoleId])
GO
ALTER TABLE [Enterprise].[PartyRole] ADD CONSTRAINT [AK_PartyRole_PartyId_RoleTypeId] UNIQUE NONCLUSTERED  ([PartyId], [RoleTypeId])
GO
ALTER TABLE [Enterprise].[PartyRole] ADD CONSTRAINT [FK_PartyRole_Party] FOREIGN KEY ([PartyId]) REFERENCES [Enterprise].[Party] ([PartyId]) ON DELETE CASCADE ON UPDATE CASCADE
GO
ALTER TABLE [Enterprise].[PartyRole] ADD CONSTRAINT [FK_PartyRole_RoleType] FOREIGN KEY ([RoleTypeId]) REFERENCES [Enterprise].[RoleType] ([PartyRoleTypeId]) ON DELETE CASCADE ON UPDATE CASCADE
GO
