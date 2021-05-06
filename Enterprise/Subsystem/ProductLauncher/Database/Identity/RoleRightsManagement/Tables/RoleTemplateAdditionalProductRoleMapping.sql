CREATE TABLE [Security].[RoleTemplateAdditionalProductRoleMapping] (
	[RoleTemplateAdditionalProductRoleMappingId]	INT				NOT NULL	IDENTITY
	,[RoleTemplateProductId]						INT				NOT NULL
	,[AttributeName]								VARCHAR(255)	NOT NULL
	,[AttributeValue]								VARCHAR(255)	NOT NULL
	,[SysStartDateTime]								DATETIME2 GENERATED ALWAYS AS ROW START NOT NULL
	,[SysEndDateTime]								DATETIME2 GENERATED ALWAYS AS ROW END NOT NULL
	,PERIOD FOR SYSTEM_TIME (SysStartDateTime,SysEndDateTime)
	,CONSTRAINT [PK_RoleTemplateAdditionalProductRoleMapping] PRIMARY KEY CLUSTERED ([RoleTemplateAdditionalProductRoleMappingId] ASC)
	,CONSTRAINT [FK_RoleTemplateAdditionalProductRoleMapping_RoleTemplateProductId] FOREIGN KEY ([RoleTemplateProductId]) REFERENCES [Security].[RoleTemplateProduct] ([RoleTemplateProductId])
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE=[Security].[RoleTemplateAdditionalProductRoleMappingHistory], DATA_CONSISTENCY_CHECK=ON));
GO

EXECUTE sp_addextendedproperty
	@name = N'MS_Description',
	@value = N'This table contains Role Templates Additional Product Mapping',
	@level0type = N'SCHEMA',
	@level0name = N'Security',
	@level1type = N'TABLE',
	@level1name = N'RoleTemplateAdditionalProductRoleMapping';
GO

EXECUTE sp_addextendedproperty
	@name = N'MS_Description',
	@value = N'Role Template Product Id.',
	@level0type = N'SCHEMA',
	@level0name = N'Security',
	@level1type = N'TABLE',
	@level1name = N'RoleTemplateAdditionalProductRoleMapping',
	@level2type = N'COLUMN',
	@level2name = N'RoleTemplateAdditionalProductRoleMappingId';
GO

EXECUTE sp_addextendedproperty
	@name = N'MS_Description',
	@value = N'Attribute Name',
	@level0type = N'SCHEMA',
	@level0name = N'Security',
	@level1type = N'TABLE',
	@level1name = N'RoleTemplateAdditionalProductRoleMapping',
	@level2type = N'COLUMN',
	@level2name = N'RoleTemplateProductId';
GO

EXECUTE sp_addextendedproperty
	@name = N'MS_Description',
	@value = N'Attribute Value',
	@level0type = N'SCHEMA',
	@level0name = N'Security',
	@level1type = N'TABLE',
	@level1name = N'RoleTemplateAdditionalProductRoleMapping',
	@level2type = N'COLUMN',
	@level2name = N'AttributeValue';