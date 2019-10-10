CREATE TABLE [Enterprise].[ContactMechanismUsageType] (
    [ContactMechanismUsageTypeID]		INT NOT NULL,
	[ParentContactMechanismUsageTypeID] INT NULL,
    [Name]								NVARCHAR (50) NOT NULL,
    CONSTRAINT [PK_ContactMechanismUsageType] PRIMARY KEY CLUSTERED ([ContactMechanismUsageTypeID] ASC),
    CONSTRAINT [AK_ContactMechanismUsageType_Name] UNIQUE NONCLUSTERED ([Name] ASC), 
    CONSTRAINT [FK_ContactMechanismUsageType_ParentContactMechanism] FOREIGN KEY ([ParentContactMechanismUsageTypeID]) REFERENCES [Enterprise].[ContactMechanismUsageType]([ContactMechanismUsageTypeID])
);
GO

EXECUTE sp_addextendedproperty
	@name = N'MS_Description',
	@value = N'Defines the seed data for contact usage. Such as Personal, Work, or Account Recovery.',
	@level0type = N'SCHEMA',
	@level0name = N'Enterprise',
	@level1type = N'TABLE',
	@level1name = N'ContactMechanismUsageType';
GO

EXECUTE sp_addextendedproperty
	@name = N'MS_Description',
	@value = N'The usage type name',
	@level0type = N'SCHEMA',
	@level0name = N'Enterprise',
	@level1type = N'TABLE',
	@level1name = N'ContactMechanismUsageType',
	@level2type = N'COLUMN',
	@level2name = N'Name';
GO

EXECUTE sp_addextendedproperty
	@name = N'MS_Description',
	@value = N'Identity Backing Field',
	@level0type = N'SCHEMA',
	@level0name = N'Enterprise',
	@level1type = N'TABLE',
	@level1name = N'ContactMechanismUsageType',
	@level2type = N'COLUMN',
	@level2name = N'ContactMechanismUsageTypeID';
GO