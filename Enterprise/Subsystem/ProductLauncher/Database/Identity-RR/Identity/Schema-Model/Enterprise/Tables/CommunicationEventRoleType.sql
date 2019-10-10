CREATE TABLE [Enterprise].[CommunicationEventRoleType]
(
[CommunicationEventRoleTypeID] [int] NOT NULL IDENTITY(1, 1),
[Description] [nvarchar] (50) NOT NULL
)
GO
ALTER TABLE [Enterprise].[CommunicationEventRoleType] ADD CONSTRAINT [PK_Enterprise.CommunicationEventRoleType] PRIMARY KEY CLUSTERED  ([CommunicationEventRoleTypeID])
GO
