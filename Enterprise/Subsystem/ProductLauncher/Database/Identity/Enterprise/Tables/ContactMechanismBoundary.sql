CREATE TABLE [Enterprise].[ContactMechanismBoundary] (
    [ContactMechanismBoundaryId] INT      IDENTITY (1, 1) NOT NULL,
    [ContactMechanismId]         INT      NOT NULL,
    [GeographicBoundaryId]       INT      NOT NULL,
    [FromDate]                   DATETIME NOT NULL,
    [ThruDate]                   DATETIME NULL,
    CONSTRAINT [PK_PostalAddressBoundary] PRIMARY KEY CLUSTERED ([ContactMechanismBoundaryId] ASC),
    CONSTRAINT [FK_ContactMechanismBoundary_ContactMechanism] FOREIGN KEY ([ContactMechanismId]) REFERENCES [Enterprise].[ContactMechanism] ([ContactMechanismID]) ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT [FK_ContactMechanismBoundary_GeographicBoundary] FOREIGN KEY ([GeographicBoundaryId]) REFERENCES [Enterprise].[GeographicBoundary] ([GeographicBoundaryId]) ON DELETE CASCADE ON UPDATE CASCADE, 
    CONSTRAINT [AK_ContactMechanismBoundary_ContactMechanismId_GeographicBoundaryId] UNIQUE ([ContactMechanismId], [GeographicBoundaryId], ThruDate)
);
GO

EXECUTE sp_addextendedproperty
	@name = N'MS_Description',
	@value = N'This table joins geographic boundaries to a contact mechanism.',
	@level0type = N'SCHEMA',
	@level0name = N'Enterprise',
	@level1type = N'TABLE',
	@level1name = N'ContactMechanismBoundary';
GO

EXECUTE sp_addextendedproperty
	@name = N'MS_Description',
	@value = N'The date this boundary was deactivated.',
	@level0type = N'SCHEMA',
	@level0name = N'Enterprise',
	@level1type = N'TABLE',
	@level1name = N'ContactMechanismBoundary',
	@level2type = N'COLUMN',
	@level2name = N'ThruDate';
GO

EXECUTE sp_addextendedproperty
	@name = N'MS_Description',
	@value = N'The geographic boundary that applies to this boundary. ',
	@level0type = N'SCHEMA',
	@level0name = N'Enterprise',
	@level1type = N'TABLE',
	@level1name = N'ContactMechanismBoundary',
	@level2type = N'COLUMN',
	@level2name = N'GeographicBoundaryId';
GO

EXECUTE sp_addextendedproperty
	@name = N'MS_Description',
	@value = N'The date this boundary became active.',
	@level0type = N'SCHEMA',
	@level0name = N'Enterprise',
	@level1type = N'TABLE',
	@level1name = N'ContactMechanismBoundary',
	@level2type = N'COLUMN',
	@level2name = N'FromDate';
GO

EXECUTE sp_addextendedproperty
	@name = N'MS_Description',
	@value = N'The contact mechanism id that this boundary definition applies to. There may be multiple rows for the same contact mechanism, such as a city,state, zip combination.',
	@level0type = N'SCHEMA',
	@level0name = N'Enterprise',
	@level1type = N'TABLE',
	@level1name = N'ContactMechanismBoundary',
	@level2type = N'COLUMN',
	@level2name = N'ContactMechanismId';
GO

EXECUTE sp_addextendedproperty
	@name = N'MS_Description',
	@value = N'Identity backing field',
	@level0type = N'SCHEMA',
	@level0name = N'Enterprise',
	@level1type = N'TABLE',
	@level1name = N'ContactMechanismBoundary',
	@level2type = N'COLUMN',
	@level2name = N'ContactMechanismBoundaryId';
GO