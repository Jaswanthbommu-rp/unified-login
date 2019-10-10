CREATE TABLE [Enterprise].[CommunicationEventEmail] (
    [CommunicationEventEmailID]    BIGINT IDENTITY (1, 1) NOT NULL,
    [CommunicationEmailTemplateID]  INT    NOT NULL,
    [CommunicationEventId] BIGINT    NOT NULL,
    CONSTRAINT [PK_CommunicationEventEmail] PRIMARY KEY CLUSTERED ([CommunicationEventEmailID] ASC) ,
    CONSTRAINT [FK_CommunicationEventEmail_CommunicationEmailTemplate] FOREIGN KEY ([CommunicationEmailTemplateID]) REFERENCES [Enterprise].[CommunicationEmailTemplate] ([CommunicationEmailTemplateID]) ,
	CONSTRAINT [FK_CommunicationEventEmail_CommunicationEvent] FOREIGN KEY ([CommunicationEventId]) REFERENCES [Enterprise].[CommunicationEvent] (CommunicationEventID) 
);

