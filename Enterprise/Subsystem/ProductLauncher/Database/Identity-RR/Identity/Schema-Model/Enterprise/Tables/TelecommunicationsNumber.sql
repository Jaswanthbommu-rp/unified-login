CREATE TABLE [Enterprise].[TelecommunicationsNumber]
(
[ContactMechanismID] [int] NOT NULL,
[CountryCode] [varchar] (10) NOT NULL,
[AreaCode] [varchar] (10) NOT NULL,
[PhoneNumber] [varchar] (10) NOT NULL
)
GO
ALTER TABLE [Enterprise].[TelecommunicationsNumber] ADD CONSTRAINT [PK_TelecommunicationsNumber] PRIMARY KEY CLUSTERED  ([ContactMechanismID])
GO
ALTER TABLE [Enterprise].[TelecommunicationsNumber] ADD CONSTRAINT [FK_TelecommunicationsNumber_ContactMechanism] FOREIGN KEY ([ContactMechanismID]) REFERENCES [Enterprise].[ContactMechanism] ([ContactMechanismID]) ON DELETE CASCADE ON UPDATE CASCADE
GO
EXEC sp_addextendedproperty N'MS_Description', N'This table contains telephone numbers.', 'SCHEMA', N'Enterprise', 'TABLE', N'TelecommunicationsNumber', NULL, NULL
GO
EXEC sp_addextendedproperty N'MS_Description', N'Area code of the Telecommunications number', 'SCHEMA', N'Enterprise', 'TABLE', N'TelecommunicationsNumber', 'COLUMN', N'AreaCode'
GO
EXEC sp_addextendedproperty N'MS_Description', N'Contact Mechanism that has a Telecommunications number tied to it.', 'SCHEMA', N'Enterprise', 'TABLE', N'TelecommunicationsNumber', 'COLUMN', N'ContactMechanismID'
GO
EXEC sp_addextendedproperty N'MS_Description', N'Country Code of the Telecommunications number', 'SCHEMA', N'Enterprise', 'TABLE', N'TelecommunicationsNumber', 'COLUMN', N'CountryCode'
GO
EXEC sp_addextendedproperty N'MS_Description', N'Phone number of the Telecommunicatoins number.', 'SCHEMA', N'Enterprise', 'TABLE', N'TelecommunicationsNumber', 'COLUMN', N'PhoneNumber'
GO
