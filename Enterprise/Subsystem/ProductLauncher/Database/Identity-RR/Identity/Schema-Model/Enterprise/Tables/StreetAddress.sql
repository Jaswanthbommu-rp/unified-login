CREATE TABLE [Enterprise].[StreetAddress]
(
[ContactMechanismID] [int] NOT NULL,
[StreetAddress1] [nvarchar] (50) NOT NULL,
[StreetAddress2] [nvarchar] (50) NULL,
[StreetAddress3] [nvarchar] (50) NULL
)
GO
ALTER TABLE [Enterprise].[StreetAddress] ADD CONSTRAINT [PK_StreetAddress] PRIMARY KEY CLUSTERED  ([ContactMechanismID])
GO
ALTER TABLE [Enterprise].[StreetAddress] ADD CONSTRAINT [FK_StreetAddress_ContactMechanism] FOREIGN KEY ([ContactMechanismID]) REFERENCES [Enterprise].[ContactMechanism] ([ContactMechanismID]) ON DELETE CASCADE ON UPDATE CASCADE
GO
EXEC sp_addextendedproperty N'MS_Description', N'Street Address is part of a contact mechanism based on geographic location. Therefore it has the street address fields, and is related to the contact mechanism geographic boundary to retrieve the other fields related to the location.', 'SCHEMA', N'Enterprise', 'TABLE', N'StreetAddress', NULL, NULL
GO
EXEC sp_addextendedproperty N'MS_Description', N'The Contact Mechanism that has a postal address.', 'SCHEMA', N'Enterprise', 'TABLE', N'StreetAddress', 'COLUMN', N'ContactMechanismID'
GO
EXEC sp_addextendedproperty N'MS_Description', N'Text that goes in the first Address line', 'SCHEMA', N'Enterprise', 'TABLE', N'StreetAddress', 'COLUMN', N'StreetAddress1'
GO
EXEC sp_addextendedproperty N'MS_Description', N'Text that goes in the second Address line', 'SCHEMA', N'Enterprise', 'TABLE', N'StreetAddress', 'COLUMN', N'StreetAddress2'
GO
EXEC sp_addextendedproperty N'MS_Description', N'Text that goes in the third Address line', 'SCHEMA', N'Enterprise', 'TABLE', N'StreetAddress', 'COLUMN', N'StreetAddress3'
GO
