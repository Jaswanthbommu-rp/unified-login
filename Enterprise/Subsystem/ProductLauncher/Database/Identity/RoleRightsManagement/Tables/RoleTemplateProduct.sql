CREATE TABLE [Security].[RoleTemplateProduct] (
	[RoleTemplateProductId]					INT		NOT NULL	IDENTITY
	,[RoleTemplateId]						INT		NOT NULL
	,[ProductId]							INT		NOT NULL
	,[SysStartTime]							DATETIME2 GENERATED ALWAYS AS ROW START NOT NULL
	,[SysEndTime]							DATETIME2 GENERATED ALWAYS AS ROW END NOT NULL
	,PERIOD FOR SYSTEM_TIME (SysStartTime,SysEndTime)
	,CONSTRAINT [PK_RoleTemplateProduct] PRIMARY KEY CLUSTERED ([RoleTemplateProductId] ASC)
	,CONSTRAINT [FK_RoleTemplateProduct_RoleTemplateId] FOREIGN KEY ([RoleTemplateId]) REFERENCES [Security].[RoleTemplate] ([RoleTemplateId])
	,CONSTRAINT [FK_RoleTemplateProduct_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Enterprise].[Product] ([ProductId])
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE=[Security].[RoleTemplateProductHistory], DATA_CONSISTENCY_CHECK=ON));
GO
EXECUTE sp_addextendedproperty
	@name = N'MS_Description',
	@value = N'This table contains Role Templates Product',
	@level0type = N'SCHEMA',
	@level0name = N'Security',
	@level1type = N'TABLE',
	@level1name = N'RoleTemplateProduct';
GO

EXECUTE sp_addextendedproperty
	@name = N'MS_Description',
	@value = N'Role Template Product Id.',
	@level0type = N'SCHEMA',
	@level0name = N'Security',
	@level1type = N'TABLE',
	@level1name = N'RoleTemplateProduct',
	@level2type = N'COLUMN',
	@level2name = N'RoleTemplateProductId';
GO

EXECUTE sp_addextendedproperty
	@name = N'MS_Description',
	@value = N'Role TemplateId',
	@level0type = N'SCHEMA',
	@level0name = N'Security',
	@level1type = N'TABLE',
	@level1name = N'RoleTemplateProduct',
	@level2type = N'COLUMN',
	@level2name = N'RoleTemplateId';
GO

EXECUTE sp_addextendedproperty
	@name = N'MS_Description',
	@value = N'Product Id',
	@level0type = N'SCHEMA',
	@level0name = N'Security',
	@level1type = N'TABLE',
	@level1name = N'RoleTemplateProduct',
	@level2type = N'COLUMN',
	@level2name = N'ProductId';