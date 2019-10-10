CREATE TABLE [Enterprise].[CommunicationEventPurposeUsage]
(
[CommunicationEventPurposeUsageID] [int] NOT NULL IDENTITY(1, 1),
[CommunicationEventID] [bigint] NOT NULL,
[CommunicationEventPurposeTypeID] [int] NOT NULL
)
GO
ALTER TABLE [Enterprise].[CommunicationEventPurposeUsage] ADD CONSTRAINT [PK_CommunicationEventPurposeUsage] PRIMARY KEY CLUSTERED  ([CommunicationEventPurposeUsageID])
GO
ALTER TABLE [Enterprise].[CommunicationEventPurposeUsage] ADD CONSTRAINT [FK_CommunicationEventPurposeUsage_CommunicationEvent] FOREIGN KEY ([CommunicationEventID]) REFERENCES [Enterprise].[CommunicationEvent] ([CommunicationEventID])
GO
ALTER TABLE [Enterprise].[CommunicationEventPurposeUsage] ADD CONSTRAINT [FK_CommunicationEventPurposeUsage_CommunicationEventPurposeType] FOREIGN KEY ([CommunicationEventPurposeTypeID]) REFERENCES [Enterprise].[CommunicationEventPurposeType] ([CommunicationEventPurposeTypeID])
GO
