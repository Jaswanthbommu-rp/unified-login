CREATE TABLE [Enterprise].[ContactMechanismPreference ] (
    [ContactMechanismID] INT          NOT NULL
    CONSTRAINT [PK_ContactMechanismPreference ] PRIMARY KEY CLUSTERED ([ContactMechanismID] ASC),
    CONSTRAINT [FK_ContactMechanismPreference _ContactMechanism] FOREIGN KEY ([ContactMechanismID]) 
		REFERENCES [Enterprise].[ContactMechanism] ([ContactMechanismID]) ON DELETE CASCADE ON UPDATE CASCADE
);
GO

EXECUTE sp_addextendedproperty
	@name = N'MS_Description',
	@value = N'This table contains ContactMechanismID for contact preferred.',
	@level0type = N'SCHEMA',
	@level0name = N'Enterprise',
	@level1type = N'TABLE',
	@level1name = N'ContactMechanismPreference ';
GO

EXECUTE sp_addextendedproperty
	@name = N'MS_Description',
	@value = N'Preferred Contact Mechanism Id for primary communication.',
	@level0type = N'SCHEMA',
	@level0name = N'Enterprise',
	@level1type = N'TABLE',
	@level1name = N'ContactMechanismPreference ',
	@level2type = N'COLUMN',
	@level2name = N'ContactMechanismID';
GO