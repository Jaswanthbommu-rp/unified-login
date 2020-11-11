CREATE TABLE [Enterprise].[CompanyAddress] (
    [CompanyAddressId] [bigint] IDENTITY(1,1) NOT NULL,
	[PartyId] [bigint] NOT NULL,		
	[Address] [nvarchar](200) NOT NULL,
	[City] [nvarchar](60) NOT NULL,
	[State] [nvarchar](20) NULL,
	[PostalCode] [nvarchar](25) NULL,
	[County] [nvarchar](60) NULL,
	[Country] [nvarchar](25) NULL,
	[Latitude] [decimal](9, 6) NULL,
	[Longitude] [decimal](9, 6) NULL
    CONSTRAINT [PK_CompanyAddress] PRIMARY KEY CLUSTERED ([CompanyAddressId] ASC),
    CONSTRAINT [FK_CompanyAddress_Organization] FOREIGN KEY ([PartyId]) REFERENCES [Enterprise].[Organization] ([PartyId]) ON DELETE CASCADE ON UPDATE CASCADE,	
	CONSTRAINT [UC_CompanyAddress_PartyId] UNIQUE (PartyId),
);
GO

EXECUTE sp_addextendedproperty
	@name = N'MS_Description',
	@value = N'This table contains Company Address.',
	@level0type = N'SCHEMA',
	@level0name = N'Enterprise',
	@level1type = N'TABLE',
	@level1name = N'CompanyAddress';
GO

EXECUTE sp_addextendedproperty
	@name = N'MS_Description',
	@value = N'CompanyAddressId of the Company Address.',
	@level0type = N'SCHEMA',
	@level0name = N'Enterprise',
	@level1type = N'TABLE',
	@level1name = N'CompanyAddress',
	@level2type = N'COLUMN',
	@level2name = N'CompanyAddressId';
GO

EXECUTE sp_addextendedproperty
	@name = N'MS_Description',
	@value = N'PartyId of the Company Address',
	@level0type = N'SCHEMA',
	@level0name = N'Enterprise',
	@level1type = N'TABLE',
	@level1name = N'CompanyAddress',
	@level2type = N'COLUMN',
	@level2name = N'PartyId';
GO

EXECUTE sp_addextendedproperty
	@name = N'MS_Description',
	@value = N'Address that has a Company Address.',
	@level0type = N'SCHEMA',
	@level0name = N'Enterprise',
	@level1type = N'TABLE',
	@level1name = N'CompanyAddress',
	@level2type = N'COLUMN',
	@level2name = N'Address';
GO

EXECUTE sp_addextendedproperty
	@name = N'MS_Description',
	@value = N'City of the Company Address',
	@level0type = N'SCHEMA',
	@level0name = N'Enterprise',
	@level1type = N'TABLE',
	@level1name = N'CompanyAddress',
	@level2type = N'COLUMN',
	@level2name = N'City';
GO

EXECUTE sp_addextendedproperty
	@name = N'MS_Description',
	@value = N'State of the Company Address',
	@level0type = N'SCHEMA',
	@level0name = N'Enterprise',
	@level1type = N'TABLE',
	@level1name = N'CompanyAddress',
	@level2type = N'COLUMN',
	@level2name = N'State';
GO

EXECUTE sp_addextendedproperty
	@name = N'MS_Description',
	@value = N'PostalCode of the Company Address',
	@level0type = N'SCHEMA',
	@level0name = N'Enterprise',
	@level1type = N'TABLE',
	@level1name = N'CompanyAddress',
	@level2type = N'COLUMN',
	@level2name = N'PostalCode';
GO

EXECUTE sp_addextendedproperty
	@name = N'MS_Description',
	@value = N'County of the Company Address',
	@level0type = N'SCHEMA',
	@level0name = N'Enterprise',
	@level1type = N'TABLE',
	@level1name = N'CompanyAddress',
	@level2type = N'COLUMN',
	@level2name = N'County';
GO

EXECUTE sp_addextendedproperty
	@name = N'MS_Description',
	@value = N'Country of the Company Address',
	@level0type = N'SCHEMA',
	@level0name = N'Enterprise',
	@level1type = N'TABLE',
	@level1name = N'CompanyAddress',
	@level2type = N'COLUMN',
	@level2name = N'Country';
GO

EXECUTE sp_addextendedproperty
	@name = N'MS_Description',
	@value = N'Latitude of the Company Address',
	@level0type = N'SCHEMA',
	@level0name = N'Enterprise',
	@level1type = N'TABLE',
	@level1name = N'CompanyAddress',
	@level2type = N'COLUMN',
	@level2name = N'Latitude';
GO

EXECUTE sp_addextendedproperty
	@name = N'MS_Description',
	@value = N'Longitude of the Company Address',
	@level0type = N'SCHEMA',
	@level0name = N'Enterprise',
	@level1type = N'TABLE',
	@level1name = N'CompanyAddress',
	@level2type = N'COLUMN',
	@level2name = N'Longitude';
GO