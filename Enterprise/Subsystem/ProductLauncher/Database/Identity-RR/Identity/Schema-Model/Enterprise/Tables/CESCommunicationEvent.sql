CREATE TABLE [Enterprise].[CESCommunicationEvent]
(
[CESCommunicationEventId] [bigint] NOT NULL,
[CESId] [nvarchar] (255) NOT NULL,
[CommunicationEventId] [bigint] NOT NULL
)
GO
ALTER TABLE [Enterprise].[CESCommunicationEvent] ADD CONSTRAINT [PK_CESCommunicationEvent] PRIMARY KEY CLUSTERED  ([CESCommunicationEventId])
GO
ALTER TABLE [Enterprise].[CESCommunicationEvent] ADD CONSTRAINT [FK_CESCommunicationEvent_CommunicationEvent] FOREIGN KEY ([CommunicationEventId]) REFERENCES [Enterprise].[CommunicationEvent] ([CommunicationEventID])
GO
