CREATE TABLE [Security].[RoleTemplateAdditionalProductRoleMappingHistory] (
	[RoleTemplateAdditionalProductRoleMappingId]	INT				NOT NULL
	,[RoleTemplateProductId]						INT				NOT NULL
	,[AttributeName]								VARCHAR(255)	NOT NULL
	,[AttributeValue]								VARCHAR(255)	NOT NULL
	,[SysStartDateTime]								DATETIME2		NOT NULL
	,[SysEndDateTime]								DATETIME2		NOT NULL
)
GO
CREATE CLUSTERED INDEX [ix_RoleTemplateAdditionalProductRoleMappingHistory]
    ON [Security].[RoleTemplateAdditionalProductRoleMappingHistory]([SysEndDateTime] ASC, [SysStartDateTime] ASC) WITH (DATA_COMPRESSION = PAGE);