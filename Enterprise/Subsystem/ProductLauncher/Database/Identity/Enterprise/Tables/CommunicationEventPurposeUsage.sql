CREATE TABLE [Enterprise].[CommunicationEventPurposeUsage] (
    [CommunicationEventPurposeUsageID] BIGINT    IDENTITY (1, 1) NOT NULL,
    [CommunicationEventID]             BIGINT NOT NULL,
    [CommunicationEventPurposeTypeID]  INT    NOT NULL,
    CONSTRAINT [PK_CommunicationEventPurposeUsage] PRIMARY KEY CLUSTERED ([CommunicationEventPurposeUsageID] ASC),
    CONSTRAINT [FK_CommunicationEventPurposeUsage_CommunicationEvent] FOREIGN KEY ([CommunicationEventID]) REFERENCES [Enterprise].[CommunicationEvent] ([CommunicationEventID]),
    CONSTRAINT [FK_CommunicationEventPurposeUsage_CommunicationEventPurposeType] FOREIGN KEY ([CommunicationEventPurposeTypeID]) REFERENCES [Enterprise].[CommunicationEventPurposeType] ([CommunicationEventPurposeTypeID])
);

