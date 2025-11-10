CREATE TABLE [Security].[RoleTemplate] (
	[RoleTemplateId]				INT				NOT NULL	IDENTITY
    ,[RoleTemplateName]				VARCHAR(100)	NOT NULL
    ,[RoleTemplateDescription]		VARCHAR(255)	NOT NULL
    ,[PartyID]						BIGINT			NULL
	,[RoleType]						VARCHAR(50)		DEFAULT  'Custom'
	,[SysStartDateTime]				DATETIME2 GENERATED ALWAYS AS ROW START NOT NULL
	,[SysEndDateTime]				DATETIME2 GENERATED ALWAYS AS ROW END NOT NULL
	,PERIOD FOR SYSTEM_TIME (SysStartDateTime,SysEndDateTime)
    ,CONSTRAINT [PK_RoleTemplateId] PRIMARY KEY CLUSTERED ([RoleTemplateId] ASC)
    ,CONSTRAINT [FK_RoleTemplate_PartyID] FOREIGN KEY ([PartyID]) REFERENCES [Enterprise].[Organization] ([PartyID])
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE=[Security].[RoleTemplateHistory], DATA_CONSISTENCY_CHECK=ON));
GO



EXECUTE sp_addextendedproperty
	@name = N'MS_Description',
	@value = N'This table contains Role Templates',
	@level0type = N'SCHEMA',
	@level0name = N'Security',
	@level1type = N'TABLE',
	@level1name = N'RoleTemplate';
GO

EXECUTE sp_addextendedproperty
	@name = N'MS_Description',
	@value = N'RoleTemplate Id.',
	@level0type = N'SCHEMA',
	@level0name = N'Security',
	@level1type = N'TABLE',
	@level1name = N'RoleTemplate',
	@level2type = N'COLUMN',
	@level2name = N'RoleTemplateId';
GO

EXECUTE sp_addextendedproperty
	@name = N'MS_Description',
	@value = N'Role Template Name',
	@level0type = N'SCHEMA',
	@level0name = N'Security',
	@level1type = N'TABLE',
	@level1name = N'RoleTemplate',
	@level2type = N'COLUMN',
	@level2name = N'RoleTemplateName';
GO

EXECUTE sp_addextendedproperty
	@name = N'MS_Description',
	@value = N'Role Template Description',
	@level0type = N'SCHEMA',
	@level0name = N'Security',
	@level1type = N'TABLE',
	@level1name = N'RoleTemplate',
	@level2type = N'COLUMN',
	@level2name = N'RoleTemplateDescription';
GO

EXECUTE sp_addextendedproperty
	@name = N'MS_Description',
	@value = N'Role Type',
	@level0type = N'SCHEMA',
	@level0name = N'Security',
	@level1type = N'TABLE',
	@level1name = N'RoleTemplate',
	@level2type = N'COLUMN',
	@level2name = N'RoleType';
GO

EXECUTE sp_addextendedproperty
	@name = N'MS_Description',
	@value = N'Party ID',
	@level0type = N'SCHEMA',
	@level0name = N'Security',
	@level1type = N'TABLE',
	@level1name = N'RoleTemplate',
	@level2type = N'COLUMN',
	@level2name = N'PartyID';
GO