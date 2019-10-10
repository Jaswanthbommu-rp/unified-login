CREATE TABLE [Enterprise].[CommunicationEvent]
(
[CommunicationEventID] [bigint] NOT NULL IDENTITY(1, 1),
[StatusTypeID] [int] NOT NULL,
[ContactMechanismTypeID] [int] NOT NULL,
[CommunicationEventPurposeTypeID] [int] NOT NULL,
[FromPartyId] [bigint] NOT NULL,
[ToPartyId] [bigint] NOT NULL,
[Started] [datetime] NOT NULL,
[Ended] [datetime] NULL,
[Note] [nvarchar] (1000) NULL
)
GO
ALTER TABLE [Enterprise].[CommunicationEvent] ADD CONSTRAINT [PK_CommunicationEvent] PRIMARY KEY CLUSTERED  ([CommunicationEventID])
GO
ALTER TABLE [Enterprise].[CommunicationEvent] ADD CONSTRAINT [FK_CommunicationEvent_ContactMechanismType] FOREIGN KEY ([ContactMechanismTypeID]) REFERENCES [Enterprise].[ContactMechanismType] ([ContactMechanismTypeID])
GO
ALTER TABLE [Enterprise].[CommunicationEvent] ADD CONSTRAINT [FK_CommunicationEvent_Party] FOREIGN KEY ([FromPartyId]) REFERENCES [Enterprise].[Party] ([PartyId])
GO
ALTER TABLE [Enterprise].[CommunicationEvent] ADD CONSTRAINT [FK_CommunicationEvent_Party1] FOREIGN KEY ([ToPartyId]) REFERENCES [Enterprise].[Party] ([PartyId])
GO
ALTER TABLE [Enterprise].[CommunicationEvent] ADD CONSTRAINT [FK_CommunicationEvent_StatusType] FOREIGN KEY ([StatusTypeID]) REFERENCES [Enterprise].[StatusType] ([StatusTypeId])
GO
