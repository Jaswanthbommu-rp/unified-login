CREATE TABLE [Enterprise].[CommunicationEventEmail]
(
[CommunicationEventEmailID] [bigint] NOT NULL IDENTITY(1, 1),
[CommunicationEmailTemplateID] [int] NOT NULL,
[ContactMechanismValidEmailID] [int] NOT NULL
)
GO
ALTER TABLE [Enterprise].[CommunicationEventEmail] ADD CONSTRAINT [PK_CommunicationEventEmail] PRIMARY KEY CLUSTERED  ([CommunicationEventEmailID])
GO
ALTER TABLE [Enterprise].[CommunicationEventEmail] ADD CONSTRAINT [FK_CommunicationEventEmail_CommunicationEmailTemplate] FOREIGN KEY ([CommunicationEmailTemplateID]) REFERENCES [Enterprise].[CommunicationEmailTemplate] ([CommunicationEmailTemplateID])
GO
ALTER TABLE [Enterprise].[CommunicationEventEmail] ADD CONSTRAINT [FK_CommunicationEventEmail_ContactMechanismValidEmail] FOREIGN KEY ([ContactMechanismValidEmailID]) REFERENCES [Enterprise].[ContactMechanismValidEmail] ([ContactMechanismValidEmailID])
GO
