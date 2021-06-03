CREATE TABLE [Security].[RoleTemplateUserMappingHistory] (
	 [RoleTemplateUserMappingId]		INT				NOT NULL
	,[RoleTemplateId]					INT				NOT NULL
    ,[PersonaId]						BIGINT			NOT NULL
	,[SysStartDateTime]					DATETIME2		NOT NULL
	,[SysEndDateTime]					DATETIME2		NOT NULL
)

GO
CREATE CLUSTERED INDEX [ix_RoleTemplateUserMappingHistory]
    ON [Security].[RoleTemplateUserMappingHistory]([SysEndDateTime] ASC, [SysStartDateTime] ASC) WITH (DATA_COMPRESSION = PAGE);