CREATE TABLE [Enterprise].[ContactMechanism]
(
[ContactMechanismID] [int] NOT NULL IDENTITY(1, 1)
)
GO
ALTER TABLE [Enterprise].[ContactMechanism] ADD CONSTRAINT [PK_ContactMechanism] PRIMARY KEY CLUSTERED  ([ContactMechanismID])
GO
EXEC sp_addextendedproperty N'MS_Description', N'A contact mechanism is an agency or means by which two or more persons, groups (parties), or other item (facility) are placed in communication with each other.', 'SCHEMA', N'Enterprise', 'TABLE', N'ContactMechanism', NULL, NULL
GO
EXEC sp_addextendedproperty N'MS_Description', N'Identity Column for Contact Mechanisms', 'SCHEMA', N'Enterprise', 'TABLE', N'ContactMechanism', 'COLUMN', N'ContactMechanismID'
GO
