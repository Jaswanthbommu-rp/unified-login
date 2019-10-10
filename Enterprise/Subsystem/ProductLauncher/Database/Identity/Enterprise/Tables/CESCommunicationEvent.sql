CREATE TABLE [Enterprise].[CESCommunicationEvent] (
    [CESCommunicationEventId] BIGINT IDENTITY(1,1)        NOT NULL,
    [CESId]                   NVARCHAR (255) NOT NULL,
    [CommunicationEventId]    BIGINT         NOT NULL,
    CONSTRAINT [PK_CESCommunicationEvent] PRIMARY KEY CLUSTERED ([CESCommunicationEventId] ASC),
    CONSTRAINT [FK_CESCommunicationEvent_CommunicationEvent] FOREIGN KEY ([CommunicationEventId]) REFERENCES [Enterprise].[CommunicationEvent] ([CommunicationEventID])
);

