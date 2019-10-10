CREATE TABLE [Enterprise].[ContactMechanismUsage]
(
	[ContactMechanismUsageID] INT IDENTITY NOT NULL, 
    [PartyContactMechanismID] BIGINT NOT NULL, 
    [ContactMechanismUsageTypeID] INT NOT NULL, 
    CONSTRAINT [PK_ContactMechanismUsage] PRIMARY KEY (ContactMechanismUsageID), 
    CONSTRAINT [FK_ContactMechanismUsage_PartyContactMechanism] FOREIGN KEY ([PartyContactMechanismID]) REFERENCES [Enterprise].[PartyContactMechanism]([PartyContactMechanismID]) ON DELETE CASCADE ON UPDATE CASCADE, 
    CONSTRAINT [FK_ContactMechanismUsage_ContactMechanismUsageType] FOREIGN KEY ([ContactMechanismUsageTypeID]) REFERENCES [Enterprise].[ContactMechanismUsageType]([ContactMechanismUsageTypeID]) ON DELETE CASCADE ON UPDATE CASCADE, 
    CONSTRAINT [AK_ContactMechanismUsage_PartyContactMechanismId_UsageId] UNIQUE ([PartyContactMechanismID], [ContactMechanismUsageTypeID])
)
GO

EXECUTE sp_addextendedproperty
	@name = N'MS_Description',
	@value = N'A contact mechanism usage.',
	@level0type = N'SCHEMA',
	@level0name = N'Enterprise',
	@level1type = N'TABLE',
	@level1name = N'ContactMechanismUsage';
GO

EXECUTE sp_addextendedproperty
	@name = N'MS_Description',
	@value = N'Identity Backing Field',
	@level0type = N'SCHEMA',
	@level0name = N'Enterprise',
	@level1type = N'TABLE',
	@level1name = N'ContactMechanismUsage',
	@level2type = N'COLUMN',
	@level2name = N'ContactMechanismUsageID';
GO

EXECUTE sp_addextendedproperty
	@name = N'MS_Description',
	@value = N'References PartyContactMechanism.PartyContactMechanismID',
	@level0type = N'SCHEMA',
	@level0name = N'Enterprise',
	@level1type = N'TABLE',
	@level1name = N'ContactMechanismUsage',
	@level2type = N'COLUMN',
	@level2name = N'PartyContactMechanismID';
GO

EXECUTE sp_addextendedproperty
	@name = N'MS_Description',
	@value = N'References ContactMechanismUsageType.ContactMechanismUsageTypeID',
	@level0type = N'SCHEMA',
	@level0name = N'Enterprise',
	@level1type = N'TABLE',
	@level1name = N'ContactMechanismUsage',
	@level2type = N'COLUMN',
	@level2name = N'ContactMechanismUsageTypeID';
GO