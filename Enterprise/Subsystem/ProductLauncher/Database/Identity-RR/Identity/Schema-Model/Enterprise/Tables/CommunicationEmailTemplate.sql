CREATE TABLE [Enterprise].[CommunicationEmailTemplate]
(
[CommunicationEmailTemplateID] [int] NOT NULL IDENTITY(1, 1),
[Subject] [nvarchar] (255) NOT NULL,
[Body] [nvarchar] (max) NOT NULL
)
GO
ALTER TABLE [Enterprise].[CommunicationEmailTemplate] ADD CONSTRAINT [PK_Enterprise.CommunicationEmailTemplate] PRIMARY KEY CLUSTERED  ([CommunicationEmailTemplateID])
GO
