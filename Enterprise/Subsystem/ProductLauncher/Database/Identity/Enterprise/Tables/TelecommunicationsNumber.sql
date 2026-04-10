CREATE TABLE [Enterprise].[TelecommunicationsNumber] (
    [ContactMechanismID] INT			NOT NULL,
    [CountryCode]        VARCHAR (10)	NOT NULL,
    [AreaCode]           VARCHAR (10)	NOT NULL,
    [PhoneNumber]        VARCHAR (15)	NOT NULL,
	[ISOCode]			 VARCHAR (5)	NULL,
	[Default]            BIT NOT NULL DEFAULT 0
    CONSTRAINT [PK_TelecommunicationsNumber] PRIMARY KEY CLUSTERED ([ContactMechanismID] ASC),
    CONSTRAINT [FK_TelecommunicationsNumber_ContactMechanism] FOREIGN KEY ([ContactMechanismID]) REFERENCES [Enterprise].[ContactMechanism] ([ContactMechanismID]) ON DELETE CASCADE ON UPDATE CASCADE
);
GO

CREATE NONCLUSTERED INDEX [IK_TelecommunicationsNumber_Dflt]
ON [Enterprise].[TelecommunicationsNumber] ([Default])
INCLUDE ([PhoneNumber]);
GO

EXECUTE sp_addextendedproperty
	@name = N'MS_Description',
	@value = N'This table contains telephone numbers.',
	@level0type = N'SCHEMA',
	@level0name = N'Enterprise',
	@level1type = N'TABLE',
	@level1name = N'TelecommunicationsNumber';
GO

EXECUTE sp_addextendedproperty
	@name = N'MS_Description',
	@value = N'Phone number of the Telecommunicatoins number.',
	@level0type = N'SCHEMA',
	@level0name = N'Enterprise',
	@level1type = N'TABLE',
	@level1name = N'TelecommunicationsNumber',
	@level2type = N'COLUMN',
	@level2name = N'PhoneNumber';
GO

EXECUTE sp_addextendedproperty
	@name = N'MS_Description',
	@value = N'Country Code of the Telecommunications number',
	@level0type = N'SCHEMA',
	@level0name = N'Enterprise',
	@level1type = N'TABLE',
	@level1name = N'TelecommunicationsNumber',
	@level2type = N'COLUMN',
	@level2name = N'CountryCode';
GO

EXECUTE sp_addextendedproperty
	@name = N'MS_Description',
	@value = N'Contact Mechanism that has a Telecommunications number tied to it.',
	@level0type = N'SCHEMA',
	@level0name = N'Enterprise',
	@level1type = N'TABLE',
	@level1name = N'TelecommunicationsNumber',
	@level2type = N'COLUMN',
	@level2name = N'ContactMechanismID';
GO

EXECUTE sp_addextendedproperty
	@name = N'MS_Description',
	@value = N'Area code of the Telecommunications number',
	@level0type = N'SCHEMA',
	@level0name = N'Enterprise',
	@level1type = N'TABLE',
	@level1name = N'TelecommunicationsNumber',
	@level2type = N'COLUMN',
	@level2name = N'AreaCode';
GO

EXECUTE sp_addextendedproperty
	@name = N'MS_Description',
	@value = N'ISO code of the Telecommunications number',
	@level0type = N'SCHEMA',
	@level0name = N'Enterprise',
	@level1type = N'TABLE',
	@level1name = N'TelecommunicationsNumber',
	@level2type = N'COLUMN',
	@level2name = N'ISOCode';
GO