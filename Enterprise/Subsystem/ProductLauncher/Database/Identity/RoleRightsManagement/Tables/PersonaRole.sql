CREATE TABLE [Security].[PersonaRole](
	[PersonaRoleId] [bigint] IDENTITY(1,1) NOT NULL,
	[PersonaId] [bigint] NOT NULL,
	[RoleId] INT NOT NULL,
	[FromDate] [datetime],
	[ThruDate] [datetime],
	[CreatedBy] [nvarchar](255) NOT NULL,
	[CreatedDate] [datetime] NOT NULL,
 CONSTRAINT [PK_PersonaSecurityRole] PRIMARY KEY CLUSTERED 
(
	[PersonaRoleId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [Security].[PersonaRole] ADD  CONSTRAINT [DF_PersonaSecurityRole_CreatedBy]  DEFAULT ('00000000-0000-0000-0000-000000000000') FOR [CreatedBy]
GO

ALTER TABLE [Security].[PersonaRole] ADD  CONSTRAINT [DF_PersonaSecurityRole_CreatedDate]  DEFAULT (GETUTCDATE()) FOR [CreatedDate]
GO

ALTER TABLE [Security].[PersonaRole]  WITH CHECK ADD  CONSTRAINT [FK_PersonaRole_Persona] FOREIGN KEY([PersonaId])
REFERENCES [Person].[Persona] ([PersonaId])
GO

ALTER TABLE [Security].[PersonaRole] CHECK CONSTRAINT [FK_PersonaRole_Persona]
GO

ALTER TABLE [Security].[PersonaRole]  WITH CHECK ADD  CONSTRAINT [FK_PersonaSecurityRole_SecurityRole] FOREIGN KEY([RoleId])
REFERENCES [Security].[Role] ([RoleId])
GO

ALTER TABLE [Security].[PersonaRole] CHECK CONSTRAINT [FK_PersonaSecurityRole_SecurityRole]
GO
CREATE NONCLUSTERED INDEX [IX_SecurityPersonaRole_PersonaId]
ON [Security].[PersonaRole] ([PersonaId])
INCLUDE ([RoleId])
GO

CREATE NONCLUSTERED INDEX [IX_SecurityPersonaRole_RoleId]
ON [Security].[PersonaRole] ([RoleId])
INCLUDE ([PersonaId])
GO