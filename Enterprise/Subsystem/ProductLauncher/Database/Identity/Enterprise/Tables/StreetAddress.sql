CREATE TABLE [Enterprise].[StreetAddress] (
    [ContactMechanismID] INT           NOT NULL,
    [StreetAddress1]     NVARCHAR (50) NOT NULL,
    [StreetAddress2]     NVARCHAR (50) NULL,
    [StreetAddress3]     NVARCHAR (50) NULL,
    CONSTRAINT [PK_StreetAddress] PRIMARY KEY CLUSTERED ([ContactMechanismID] ASC),
    CONSTRAINT [FK_StreetAddress_ContactMechanism] FOREIGN KEY ([ContactMechanismID]) REFERENCES [Enterprise].[ContactMechanism] ([ContactMechanismID]) ON DELETE CASCADE ON UPDATE CASCADE
);
GO

EXECUTE sp_addextendedproperty
	@name = N'MS_Description',
	@value = N'Street Address is part of a contact mechanism based on geographic location. Therefore it has the street address fields, and is related to the contact mechanism geographic boundary to retrieve the other fields related to the location.',
	@level0type = N'SCHEMA',
	@level0name = N'Enterprise',
	@level1type = N'TABLE',
	@level1name = N'StreetAddress';
GO

EXECUTE sp_addextendedproperty
	@name = N'MS_Description',
	@value = N'Text that goes in the third Address line',
	@level0type = N'SCHEMA',
	@level0name = N'Enterprise',
	@level1type = N'TABLE',
	@level1name = N'StreetAddress',
	@level2type = N'COLUMN',
	@level2name = N'StreetAddress3';
GO

EXECUTE sp_addextendedproperty
	@name = N'MS_Description',
	@value = N'Text that goes in the second Address line',
	@level0type = N'SCHEMA',
	@level0name = N'Enterprise',
	@level1type = N'TABLE',
	@level1name = N'StreetAddress',
	@level2type = N'COLUMN',
	@level2name = N'StreetAddress2';
GO

EXECUTE sp_addextendedproperty
	@name = N'MS_Description',
	@value = N'Text that goes in the first Address line',
	@level0type = N'SCHEMA',
	@level0name = N'Enterprise',
	@level1type = N'TABLE',
	@level1name = N'StreetAddress',
	@level2type = N'COLUMN',
	@level2name = N'StreetAddress1';
GO

EXECUTE sp_addextendedproperty
	@name = N'MS_Description',
	@value = N'The Contact Mechanism that has a postal address.',
	@level0type = N'SCHEMA',
	@level0name = N'Enterprise',
	@level1type = N'TABLE',
	@level1name = N'StreetAddress',
	@level2type = N'COLUMN',
	@level2name = N'ContactMechanismID';
GO