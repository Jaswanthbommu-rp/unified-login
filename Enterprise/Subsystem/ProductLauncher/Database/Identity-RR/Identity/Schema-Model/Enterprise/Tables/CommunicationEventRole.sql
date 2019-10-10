CREATE TABLE [Enterprise].[CommunicationEventRole]
(
[CommunicationEventRoleID] [int] NOT NULL IDENTITY(1, 1),
[CommunicationEventID] [bigint] NOT NULL,
[PartyID] [bigint] NOT NULL,
[CommunicationEventRoleTypeID] [int] NOT NULL
)
GO
ALTER TABLE [Enterprise].[CommunicationEventRole] ADD CONSTRAINT [PK_Enterprise.CommunicationEventRole] PRIMARY KEY CLUSTERED  ([CommunicationEventRoleID])
GO
ALTER TABLE [Enterprise].[CommunicationEventRole] ADD CONSTRAINT [FK_Enterprise.CommunicationEventRole_CommunicationEvent] FOREIGN KEY ([CommunicationEventID]) REFERENCES [Enterprise].[CommunicationEvent] ([CommunicationEventID])
GO
ALTER TABLE [Enterprise].[CommunicationEventRole] ADD CONSTRAINT [FK_Enterprise.CommunicationEventRole_Enterprise.CommunicationEventRoleType] FOREIGN KEY ([CommunicationEventRoleTypeID]) REFERENCES [Enterprise].[CommunicationEventRoleType] ([CommunicationEventRoleTypeID])
GO
ALTER TABLE [Enterprise].[CommunicationEventRole] ADD CONSTRAINT [FK_Enterprise.CommunicationEventRole_Party] FOREIGN KEY ([PartyID]) REFERENCES [Enterprise].[Party] ([PartyId])
GO
