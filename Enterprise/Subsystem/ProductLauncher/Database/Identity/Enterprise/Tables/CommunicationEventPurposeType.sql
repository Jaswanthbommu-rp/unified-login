CREATE TABLE [Enterprise].[CommunicationEventPurposeType] (
    [CommunicationEventPurposeTypeID] INT           IDENTITY (1, 1) NOT NULL,
    [Description]                     NVARCHAR (50) NOT NULL,
    CONSTRAINT [PK_Enterprise.CommunicationEventPurposeType] PRIMARY KEY CLUSTERED ([CommunicationEventPurposeTypeID] ASC)
);

