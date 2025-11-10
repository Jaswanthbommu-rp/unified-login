CREATE TABLE [Security].[RoleTemplateHistory] (
	[RoleTemplateId]				INT				NOT NULL
    ,[RoleTemplateName]				VARCHAR(100)	NOT NULL
    ,[RoleTemplateDescription]		VARCHAR(255)	NOT NULL
    ,[PartyID]						BIGINT			NULL
	,[RoleType]						VARCHAR(50)		DEFAULT  'Custom'
	,[SysStartDateTime]				DATETIME2		NOT NULL
	,[SysEndDateTime]				DATETIME2		NOT NULL
)

GO
CREATE CLUSTERED INDEX [ix_RoleTemplateHistory]
    ON [Security].[RoleTemplateHistory]([SysEndDateTime] ASC, [SysStartDateTime] ASC) WITH (DATA_COMPRESSION = PAGE);