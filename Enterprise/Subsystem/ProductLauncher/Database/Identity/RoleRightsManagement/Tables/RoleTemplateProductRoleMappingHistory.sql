CREATE TABLE [Security].[RoleTemplateProductRoleMappingHistory] (
	[RoleTemplateProductRoleMappingId]		INT	NOT NULL
	,[RoleTemplateProductId]				INT	NOT NULL
	,[ProductRoleId]						VARCHAR(100)	NOT NULL
	,[ProductRoleName]						VARCHAR(510) NOT NULL
	,[SysStartDateTime]						DATETIME2 NOT NULL
	,[SysEndDateTime]						DATETIME2 NOT NULL
)
GO
CREATE CLUSTERED INDEX [ix_RoleTemplateProductRoleMappingHistory]
    ON [Security].[RoleTemplateProductRoleMappingHistory]([SysEndDateTime] ASC, [SysStartDateTime] ASC) WITH (DATA_COMPRESSION = PAGE);