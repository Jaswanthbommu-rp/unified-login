

CREATE TABLE [Security].[DelegatedRoleTemplate] (
	[UserLoginPersonaId] [bigint] NOT NULL,
	[RoleTemplateId]  [int] NULL,
	[SysStartDateTime] [datetime2](7) GENERATED ALWAYS AS ROW START NOT NULL,
	[SysEndDateTime] [datetime2](7) GENERATED ALWAYS AS ROW END NOT NULL,
 CONSTRAINT [PK_DelegatedRoleTemplate] PRIMARY KEY CLUSTERED 
(
	[UserLoginPersonaId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
	PERIOD FOR SYSTEM_TIME ([SysStartDateTime], [SysEndDateTime])
) ON [PRIMARY]
WITH
(
SYSTEM_VERSIONING = ON (HISTORY_TABLE = [Security].[DelegatedRoleTemplateHistory])
)
GO

ALTER TABLE [Security].[DelegatedRoleTemplate]  WITH CHECK ADD  CONSTRAINT [FK_DelegatedRoleTemplate_UserLoginPersona] FOREIGN KEY([UserLoginPersonaId])
REFERENCES [Ident].[UserLoginPersona] ([UserLoginPersonaId])
ON UPDATE CASCADE
ON DELETE CASCADE
GO

ALTER TABLE [Security].[DelegatedRoleTemplate] CHECK CONSTRAINT [FK_DelegatedRoleTemplate_UserLoginPersona]
GO

ALTER TABLE [Security].[DelegatedRoleTemplate]  WITH CHECK ADD  CONSTRAINT [FK_DelegatedRoleTemplate_RoleTemplateId] FOREIGN KEY([RoleTemplateId])
REFERENCES [Security].[RoleTemplate] ([RoleTemplateId])
GO

ALTER TABLE [Security].[DelegatedRoleTemplate] CHECK CONSTRAINT [FK_DelegatedRoleTemplate_RoleTemplateId]
GO
