CREATE TABLE [Enterprise].[PartyRole]
    (
      [PartyRoleId] INT NOT NULL IDENTITY ,
      [PartyId] BIGINT NOT NULL ,
      [RoleTypeId] INT NOT NULL ,
    CONSTRAINT [PK_PartyRole] PRIMARY KEY (PartyRoleId), 
    CONSTRAINT [FK_PartyRole_Party] FOREIGN KEY (PartyId) REFERENCES [Enterprise].[Party](PartyId) ON DELETE CASCADE ON UPDATE CASCADE, 
    CONSTRAINT [FK_PartyRole_RoleType] FOREIGN KEY (RoleTypeId) REFERENCES [Enterprise].[RoleType]([PartyRoleTypeId]) ON DELETE CASCADE ON UPDATE CASCADE, 
    CONSTRAINT [AK_PartyRole_PartyId_RoleTypeId] UNIQUE ([PartyId], [RoleTypeId])
    );
	GO
	CREATE INDEX [IX_PartyRole_RoleTypeId] ON [Enterprise].[PartyRole] ([RoleTypeId]) INCLUDE ([PartyId])
