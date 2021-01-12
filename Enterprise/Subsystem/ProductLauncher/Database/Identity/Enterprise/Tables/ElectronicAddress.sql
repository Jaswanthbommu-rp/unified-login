CREATE TABLE [Enterprise].[ElectronicAddress] (
    [ContactMechanismID]      INT           NOT NULL,
    [ElectronicAddressString] VARCHAR (255) NOT NULL,
    [ElectronicAddressType] VARCHAR(20) NOT NULL DEFAULT 'E-Mail', 
    CONSTRAINT [PK_ElectronicAddress] PRIMARY KEY CLUSTERED ([ContactMechanismID] ASC),
    CONSTRAINT [FK_ElectronicAddress_ContactMechanism] FOREIGN KEY ([ContactMechanismID]) REFERENCES [Enterprise].[ContactMechanism] ([ContactMechanismID]) ON DELETE CASCADE ON UPDATE CASCADE
)
GO

EXECUTE sp_addextendedproperty
	@name = N'MS_Description',
	@value = N'Contact Methods can be electronic addresses such as email or websites. This table is used for that.',
	@level0type = N'SCHEMA',
	@level0name = N'Enterprise',
	@level1type = N'TABLE',
	@level1name = N'ElectronicAddress';
GO

EXECUTE sp_addextendedproperty
	@name = N'MS_Description',
	@value = N'The string of the electronic address.',
	@level0type = N'SCHEMA',
	@level0name = N'Enterprise',
	@level1type = N'TABLE',
	@level1name = N'ElectronicAddress',
	@level2type = N'COLUMN',
	@level2name = N'ElectronicAddressString';
GO

EXECUTE sp_addextendedproperty
	@name = N'MS_Description',
	@value = N'Identity backing field',
	@level0type = N'SCHEMA',
	@level0name = N'Enterprise',
	@level1type = N'TABLE',
	@level1name = N'ElectronicAddress',
	@level2type = N'COLUMN',
	@level2name = N'ContactMechanismID';
GO

CREATE NONCLUSTERED INDEX [IX_ElectronicAddress_ElectronicAddressString]
ON [Enterprise].[ElectronicAddress] ([ElectronicAddressString])
INCLUDE ([ContactMechanismID]);
GO
