CREATE TABLE [Enterprise].[ContactMechanismType]
(
[ContactMechanismTypeID] [int] NOT NULL IDENTITY(1, 1),
[Description] [nvarchar] (50) NOT NULL
)
GO
ALTER TABLE [Enterprise].[ContactMechanismType] ADD CONSTRAINT [PK_ContactMechanismType] PRIMARY KEY CLUSTERED  ([ContactMechanismTypeID])
GO
EXEC sp_addextendedproperty N'MS_Description', N'A contact mechanism is an agency or means by which two or more persons, groups (parties), or other item (facility) are placed in communication with each other.', 'SCHEMA', N'Enterprise', 'TABLE', N'ContactMechanismType', NULL, NULL
GO
EXEC sp_addextendedproperty N'MS_Description', N'Identity Column for Contact Mechanisms', 'SCHEMA', N'Enterprise', 'TABLE', N'ContactMechanismType', 'COLUMN', N'ContactMechanismTypeID'
GO
