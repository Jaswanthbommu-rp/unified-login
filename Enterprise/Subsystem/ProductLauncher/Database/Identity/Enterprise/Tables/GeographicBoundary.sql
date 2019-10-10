CREATE TABLE [Enterprise].[GeographicBoundary] (
    [GeographicBoundaryId]     INT           IDENTITY (1, 1) NOT NULL,
    [GeographicBoundaryTypeId] INT           NOT NULL,
    [Name]                     NVARCHAR (50) NOT NULL,
    [GeographicBoundaryCode]   NVARCHAR (50) NULL,
    [Abbreviation]             NVARCHAR (10) NULL,
    CONSTRAINT [PK_GeographicBoundary] PRIMARY KEY CLUSTERED ([GeographicBoundaryId] ASC),
    CONSTRAINT [FK_GeographicBoundary_GeographicBoundaryType] FOREIGN KEY ([GeographicBoundaryTypeId]) REFERENCES [Enterprise].[GeographicBoundaryType] ([GeographicBoundaryTypeId]) ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT [AK_GeographicBoundary_Name_GeographicBoundaryTypeId_GeographicBoundaryCode] UNIQUE NONCLUSTERED ([Name] ASC, [GeographicBoundaryTypeId] ASC, [Abbreviation] ASC)
);
GO

EXECUTE sp_addextendedproperty
	@name = N'MS_Description',
	@value = N'The named value of this Geographic Boundary, such as 75078 in the case of a ZipCode, or Texas if it''s a state.',
	@level0type = N'SCHEMA',
	@level0name = N'Enterprise',
	@level1type = N'TABLE',
	@level1name = N'GeographicBoundary',
	@level2type = N'COLUMN',
	@level2name = N'Name';
GO

EXECUTE sp_addextendedproperty
	@name = N'MS_Description',
	@value = N'The Geographic Boundary Type, which can be any type defined by the GeographicBoundaryType table.',
	@level0type = N'SCHEMA',
	@level0name = N'Enterprise',
	@level1type = N'TABLE',
	@level1name = N'GeographicBoundary',
	@level2type = N'COLUMN',
	@level2name = N'GeographicBoundaryTypeId';
GO

EXECUTE sp_addextendedproperty
	@name = N'MS_Description',
	@value = N'Identity Backing Field',
	@level0type = N'SCHEMA',
	@level0name = N'Enterprise',
	@level1type = N'TABLE',
	@level1name = N'GeographicBoundary',
	@level2type = N'COLUMN',
	@level2name = N'GeographicBoundaryId';
GO

EXECUTE sp_addextendedproperty
	@name = N'MS_Description',
	@value = N'If the geographic boundary has a specific code, this is where it goes.',
	@level0type = N'SCHEMA',
	@level0name = N'Enterprise',
	@level1type = N'TABLE',
	@level1name = N'GeographicBoundary',
	@level2type = N'COLUMN',
	@level2name = N'GeographicBoundaryCode';
GO

EXECUTE sp_addextendedproperty
	@name = N'MS_Description',
	@value = N'If the geographic boundary has a known standard abbreviation, then it would go here, such as TX for Texas.',
	@level0type = N'SCHEMA',
	@level0name = N'Enterprise',
	@level1type = N'TABLE',
	@level1name = N'GeographicBoundary',
	@level2type = N'COLUMN',
	@level2name = N'Abbreviation';
GO