CREATE TABLE [Enterprise].[ContactMechanismValidRole]
(
[ContactMechanismValidRoleID] [int] NOT NULL IDENTITY(1, 1),
[ContactMechanismTypeID] [int] NOT NULL,
[CommunicationEventRoleTypeID] [int] NOT NULL
)
GO
ALTER TABLE [Enterprise].[ContactMechanismValidRole] ADD CONSTRAINT [PK_ContactMechanismValidRole] PRIMARY KEY CLUSTERED  ([ContactMechanismValidRoleID])
GO
ALTER TABLE [Enterprise].[ContactMechanismValidRole] ADD CONSTRAINT [FK_ContactMechanismValidRole_ContactMechanismType] FOREIGN KEY ([ContactMechanismTypeID]) REFERENCES [Enterprise].[ContactMechanismType] ([ContactMechanismTypeID])
GO
ALTER TABLE [Enterprise].[ContactMechanismValidRole] ADD CONSTRAINT [FK_Enterprise.ContactMechanismValidRole_Enterprise.CommunicationEventRoleType] FOREIGN KEY ([CommunicationEventRoleTypeID]) REFERENCES [Enterprise].[CommunicationEventRoleType] ([CommunicationEventRoleTypeID])
GO
