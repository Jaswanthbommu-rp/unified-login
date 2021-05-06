CREATE TABLE [Security].[RoleTemplateProductHistory] (
	[RoleTemplateProductId]					INT		NOT NULL
	,[RoleTemplateId]						INT		NOT NULL
	,[ProductId]							INT		NOT NULL
	,[SysStartDateTime]						DATETIME2 NOT NULL
	,[SysEndDateTime]						DATETIME2 NOT NULL
)
GO
CREATE CLUSTERED INDEX [ix_RoleTemplateProductHistory]
    ON [Security].[RoleTemplateProductHistory]([SysEndDateTime] ASC, [SysStartDateTime] ASC) WITH (DATA_COMPRESSION = PAGE);