CREATE TABLE [Enterprise].[ElectronicAddress]
(
[ContactMechanismID] [int] NOT NULL,
[ElectronicAddressString] [varchar] (255) NOT NULL,
[ElectronicAddressType] [varchar] (20) NOT NULL CONSTRAINT [DF__Electroni__Elect__04AFB25B] DEFAULT ('E-Mail')
)
GO
ALTER TABLE [Enterprise].[ElectronicAddress] ADD CONSTRAINT [PK_ElectronicAddress] PRIMARY KEY CLUSTERED  ([ContactMechanismID])
GO
ALTER TABLE [Enterprise].[ElectronicAddress] ADD CONSTRAINT [FK_ElectronicAddress_ContactMechanism] FOREIGN KEY ([ContactMechanismID]) REFERENCES [Enterprise].[ContactMechanism] ([ContactMechanismID]) ON DELETE CASCADE ON UPDATE CASCADE
GO
EXEC sp_addextendedproperty N'MS_Description', N'Contact Methods can be electronic addresses such as email or websites. This table is used for that.', 'SCHEMA', N'Enterprise', 'TABLE', N'ElectronicAddress', NULL, NULL
GO
EXEC sp_addextendedproperty N'MS_Description', N'Identity backing field', 'SCHEMA', N'Enterprise', 'TABLE', N'ElectronicAddress', 'COLUMN', N'ContactMechanismID'
GO
EXEC sp_addextendedproperty N'MS_Description', N'The string of the electronic address.', 'SCHEMA', N'Enterprise', 'TABLE', N'ElectronicAddress', 'COLUMN', N'ElectronicAddressString'
GO
