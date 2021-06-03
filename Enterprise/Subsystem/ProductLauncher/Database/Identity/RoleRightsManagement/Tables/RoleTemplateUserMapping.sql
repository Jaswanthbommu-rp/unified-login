CREATE TABLE [Security].[RoleTemplateUserMapping] (
	[RoleTemplateUserMappingId]		INT				NOT NULL	IDENTITY
	,[RoleTemplateId]				INT				NOT NULL
    ,[PersonaId]					BIGINT			NOT NULL
	,[SysStartDateTime]				DATETIME2 GENERATED ALWAYS AS ROW START NOT NULL
	,[SysEndDateTime]				DATETIME2 GENERATED ALWAYS AS ROW END NOT NULL
	,PERIOD FOR SYSTEM_TIME (SysStartDateTime,SysEndDateTime)
    ,CONSTRAINT [PK_RoleTemplateUserMappingId] PRIMARY KEY CLUSTERED ([RoleTemplateUserMappingId] ASC)
    ,CONSTRAINT [FK_RoleTemplateUserMapping_RoleTemplateId] FOREIGN KEY ([RoleTemplateId]) REFERENCES [Security].[RoleTemplate] ([RoleTemplateId])
	,CONSTRAINT [FK_RoleTemplateUserMapping_PersonaId] FOREIGN KEY ([PersonaId]) REFERENCES [Person].[Persona] ([PersonaId])
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE=[Security].[RoleTemplateUserMappingHistory], DATA_CONSISTENCY_CHECK=ON));
GO

EXECUTE sp_addextendedproperty
	@name = N'MS_Description',
	@value = N'This table contains Role Template User Mapping',
	@level0type = N'SCHEMA',
	@level0name = N'Security',
	@level1type = N'TABLE',
	@level1name = N'RoleTemplateUserMapping';
GO

EXECUTE sp_addextendedproperty
	@name = N'MS_Description',
	@value = N'RoleTemplate User Mapping Id.',
	@level0type = N'SCHEMA',
	@level0name = N'Security',
	@level1type = N'TABLE',
	@level1name = N'RoleTemplateUserMapping',
	@level2type = N'COLUMN',
	@level2name = N'RoleTemplateUserMappingId';
GO

EXECUTE sp_addextendedproperty
	@name = N'MS_Description',
	@value = N'Role Template Id',
	@level0type = N'SCHEMA',
	@level0name = N'Security',
	@level1type = N'TABLE',
	@level1name = N'RoleTemplateUserMapping',
	@level2type = N'COLUMN',
	@level2name = N'RoleTemplateId';
GO

EXECUTE sp_addextendedproperty
	@name = N'MS_Description',
	@value = N'Persona Id',
	@level0type = N'SCHEMA',
	@level0name = N'Security',
	@level1type = N'TABLE',
	@level1name = N'RoleTemplateUserMapping',
	@level2type = N'COLUMN',
	@level2name = N'PersonaId';
GO