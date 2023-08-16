CREATE TABLE [Security].[DelegatedAdminRoleTemplate](
	[DelegatedAdminRoleTemplateId] [bigint] IDENTITY(1,1) NOT NULL,
	[UserLoginPersonaId] [bigint] NOT NULL,
	[RoleTemplateId] [int] NULL,
 CONSTRAINT [DelegatedAdminRoleTemplate_elegatedAdminRoleTemplateId] PRIMARY KEY CLUSTERED 
(
	[DelegatedAdminRoleTemplateId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [Security].[DelegatedAdminRoleTemplate]  WITH CHECK ADD  CONSTRAINT [FK_DelegatedAdminRoleTemplate_RoleTemplateId] FOREIGN KEY([RoleTemplateId])
REFERENCES [Security].[RoleTemplate] ([RoleTemplateId])
GO

ALTER TABLE [Security].[DelegatedAdminRoleTemplate] CHECK CONSTRAINT [FK_DelegatedAdminRoleTemplate_RoleTemplateId]
GO

ALTER TABLE [Security].[DelegatedAdminRoleTemplate]  WITH CHECK ADD  CONSTRAINT [FK_DelegatedAdminRoleTemplate_UserLoginPersona] FOREIGN KEY([UserLoginPersonaId])
REFERENCES [Ident].[UserLoginPersona] ([UserLoginPersonaId])
ON UPDATE CASCADE
ON DELETE CASCADE
GO

ALTER TABLE [Security].[DelegatedAdminRoleTemplate] CHECK CONSTRAINT [FK_DelegatedAdminRoleTemplate_UserLoginPersona]
GO