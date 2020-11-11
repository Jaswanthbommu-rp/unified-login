CREATE TABLE [Enterprise].[CompanyContractedName] (
    [CompanyContractedNameId] [bigint] IDENTITY(1,1) NOT NULL,
	[PartyId] [bigint] NOT NULL, 
	[ContractedName] [nvarchar](200) NOT NULL
    CONSTRAINT [PK_CompanyContractedName] PRIMARY KEY CLUSTERED ([CompanyContractedNameId] ASC),
    CONSTRAINT [FK_CompanyContractedName_Organization] FOREIGN KEY ([PartyId]) REFERENCES [Enterprise].[Organization] ([PartyId]) ON DELETE CASCADE ON UPDATE CASCADE,	
	CONSTRAINT [UC_CompanyContractedName_PartyId] UNIQUE (PartyId),
);
GO

EXECUTE sp_addextendedproperty
	@name = N'MS_Description',
	@value = N'This table contains Company Contracted Name.',
	@level0type = N'SCHEMA',
	@level0name = N'Enterprise',
	@level1type = N'TABLE',
	@level1name = N'CompanyContractedName';
GO

EXECUTE sp_addextendedproperty
	@name = N'MS_Description',
	@value = N'CompanyAddressId of the Company Contracted Name.',
	@level0type = N'SCHEMA',
	@level0name = N'Enterprise',
	@level1type = N'TABLE',
	@level1name = N'CompanyContractedName',
	@level2type = N'COLUMN',
	@level2name = N'CompanyContractedNameId';
GO

EXECUTE sp_addextendedproperty
	@name = N'MS_Description',
	@value = N'PartyId of the Company Contracted Name',
	@level0type = N'SCHEMA',
	@level0name = N'Enterprise',
	@level1type = N'TABLE',
	@level1name = N'CompanyContractedName',
	@level2type = N'COLUMN',
	@level2name = N'PartyId';
GO

EXECUTE sp_addextendedproperty
	@name = N'MS_Description',
	@value = N'Address that has a Company Contracted Name.',
	@level0type = N'SCHEMA',
	@level0name = N'Enterprise',
	@level1type = N'TABLE',
	@level1name = N'CompanyContractedName',
	@level2type = N'COLUMN',
	@level2name = N'ContractedName';
GO