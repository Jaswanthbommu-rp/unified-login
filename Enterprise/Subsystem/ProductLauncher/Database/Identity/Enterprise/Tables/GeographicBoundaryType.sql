CREATE TABLE [Enterprise].[GeographicBoundaryType] (
    [GeographicBoundaryTypeId] INT           IDENTITY (1, 1) NOT NULL,
    [Name]                     NVARCHAR (50) NOT NULL,
    CONSTRAINT [PK_GeographicBoundaryType] PRIMARY KEY CLUSTERED ([GeographicBoundaryTypeId] ASC),
    CONSTRAINT [AK_GeographicBoundaryType_Name] UNIQUE NONCLUSTERED ([Name] ASC)
);
GO

EXECUTE sp_addextendedproperty
	@name = N'MS_Description',
	@value = N'This table defines the different kinds of Geographic Boundaries. Please note: Geographic "locations" are NOT the same as boundaries. Do not store Geographic "location" types here.',
	@level0type = N'SCHEMA',
	@level0name = N'Enterprise',
	@level1type = N'TABLE',
	@level1name = N'GeographicBoundaryType';
GO

EXECUTE sp_addextendedproperty
	@name = N'MS_Description',
	@value = N'The name of the type that will be used to group things under. For example, State, Region, Country as it relates to a geographic boundary.',
	@level0type = N'SCHEMA',
	@level0name = N'Enterprise',
	@level1type = N'TABLE',
	@level1name = N'GeographicBoundaryType',
	@level2type = N'COLUMN',
	@level2name = N'Name';
GO

EXECUTE sp_addextendedproperty
	@name = N'MS_Description',
	@value = N'Identity backing field',
	@level0type = N'SCHEMA',
	@level0name = N'Enterprise',
	@level1type = N'TABLE',
	@level1name = N'GeographicBoundaryType',
	@level2type = N'COLUMN',
	@level2name = N'GeographicBoundaryTypeId';
GO