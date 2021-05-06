CREATE TABLE [Security].[RoleTemplateProductRoleMapping] (
	[RoleTemplateProductRoleMappingId]		INT	NOT NULL  IDENTITY
	,[RoleTemplateProductId]				INT	NOT NULL
	,[ProductRoleId]						INT	NOT NULL
	,[ProductRoleName]						NVARCHAR(510)	NOT NULL
	,[SysStartDateTime]						DATETIME2 GENERATED ALWAYS AS ROW START NOT NULL
	,[SysEndDateTime]						DATETIME2 GENERATED ALWAYS AS ROW END NOT NULL
	,PERIOD FOR SYSTEM_TIME (SysStartDateTime,SysEndDateTime)
	,CONSTRAINT [PK_RoleTemplateProductRoleMapping] PRIMARY KEY CLUSTERED ([RoleTemplateProductRoleMappingId] ASC)
	,CONSTRAINT [FK_RoleTemplateProductRoleMapping_RoleTemplateProductId] FOREIGN KEY ([RoleTemplateProductId]) REFERENCES [Security].[RoleTemplateProduct] ([RoleTemplateProductId])
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE=[Security].[RoleTemplateProductRoleMappingHistory], DATA_CONSISTENCY_CHECK=ON));
GO

EXECUTE sp_addextendedproperty
	@name = N'MS_Description',
	@value = N'This table contains Role Templates Product Mapping',
	@level0type = N'SCHEMA',
	@level0name = N'Security',
	@level1type = N'TABLE',
	@level1name = N'RoleTemplateProductRoleMapping';
GO

EXECUTE sp_addextendedproperty
	@name = N'MS_Description',
	@value = N'Role Template Product Id.',
	@level0type = N'SCHEMA',
	@level0name = N'Security',
	@level1type = N'TABLE',
	@level1name = N'RoleTemplateProductRoleMapping',
	@level2type = N'COLUMN',
	@level2name = N'RoleTemplateProductRoleMappingId';
GO

EXECUTE sp_addextendedproperty
	@name = N'MS_Description',
	@value = N'Role Template ProductId',
	@level0type = N'SCHEMA',
	@level0name = N'Security',
	@level1type = N'TABLE',
	@level1name = N'RoleTemplateProductRoleMapping',
	@level2type = N'COLUMN',
	@level2name = N'RoleTemplateProductId';
GO

EXECUTE sp_addextendedproperty
	@name = N'MS_Description',
	@value = N'Product Role Id',
	@level0type = N'SCHEMA',
	@level0name = N'Security',
	@level1type = N'TABLE',
	@level1name = N'RoleTemplateProductRoleMapping',
	@level2type = N'COLUMN',
	@level2name = N'ProductRoleId';
GO

EXECUTE sp_addextendedproperty
	@name = N'MS_Description',
	@value = N'Product Role Name',
	@level0type = N'SCHEMA',
	@level0name = N'Security',
	@level1type = N'TABLE',
	@level1name = N'RoleTemplateProductRoleMapping',
	@level2type = N'COLUMN',
	@level2name = N'ProductRoleName';
GO