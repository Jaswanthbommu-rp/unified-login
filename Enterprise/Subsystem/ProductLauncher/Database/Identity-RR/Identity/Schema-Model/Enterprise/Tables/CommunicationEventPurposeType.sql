CREATE TABLE [Enterprise].[CommunicationEventPurposeType]
(
[CommunicationEventPurposeTypeID] [int] NOT NULL IDENTITY(1, 1),
[Description] [nvarchar] (50) NOT NULL
)
GO
ALTER TABLE [Enterprise].[CommunicationEventPurposeType] ADD CONSTRAINT [PK_Enterprise.CommunicationEventPurposeType] PRIMARY KEY CLUSTERED  ([CommunicationEventPurposeTypeID])
GO
